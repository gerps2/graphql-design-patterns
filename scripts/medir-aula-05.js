// scripts/medir-aula-05.js
// Um passo por execução (PASSO): duas telas sob carga leve, contando consultas ao banco e tempo.
//   Tela inicial: usuários com carteira e transações (1 para N, DataLoader de grupo).
//   Últimas ordens: cada ordem com o dono da conta (N para 1, DataLoader de lote). Só existe a partir do passo 1.
// Com PASSO=relatorio, junta os passos medidos (PASSOS) no relatorio-aula-05.html.
// Quem chama é o scripts/medir-aula-05.sh (make medir AULA=05), que sobe a API de cada passo.
import http from 'k6/http';
import { check } from 'k6';
import { Trend } from 'k6/metrics';
import { gerarRelatorio } from './relatorio-aula-05.js';

const base = __ENV.BASE_URL || 'http://localhost:5000';
const passo = __ENV.PASSO || 'atual';
const passos = (__ENV.PASSOS || 'atual').split(' ').filter(Boolean);

// O relatório lê os resultados de cada passo. open() só funciona aqui, na carga do script.
const resultados = passo === 'relatorio'
  ? passos.map((p) => ({ passo: p, ...JSON.parse(open(`../relatorio-aula-05-${p}.json`)) }))
  : null;

const telas = {
  telaInicial: 'query TelaInicial { usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }',
  ultimasOrdens: 'query UltimasOrdens { ultimasOrdens(quantidade: 20) { ativo valor dono { nome } } }',
};

const metricas = Object.fromEntries(Object.keys(telas).map((tela) => [tela, {
  consultas: new Trend(`consultas_${tela}`),
  tempo: new Trend(`tempo_${tela}`, true),
}]));

function graphql(query) {
  return http.post(`${base}/graphql`, JSON.stringify({ query }), {
    headers: { 'Content-Type': 'application/json' },
  });
}

// Cinco usuários virtuais por 15 segundos. Cada um pede as telas sem parar.
export const options = passo === 'relatorio'
  ? { vus: 1, iterations: 1 }
  : { vus: 5, duration: '15s' };

export function setup() {
  if (passo === 'relatorio') return resultados;

  // As últimas ordens precisam de ordens de donos diferentes. Se o banco tiver poucas, cria 20, uma por usuário.
  const existentes = graphql('{ usuario(id: 1) { ordens { id } } }').json('data.usuario.ordens.length') || 0;
  if (existentes < 4) {
    for (let i = 0; i < 20; i += 1) {
      graphql(`mutation { enviarOrdem(input: { usuarioId: ${(i % 5) + 1}, ativo: "PETR4", quantidade: 1, preco: 1 }) { ordem { id } } }`);
    }
  }

  // Aquecimento: as primeiras chamadas compilam o schema e abrem conexões. Elas não entram na medição.
  const ativas = Object.keys(telas).filter((tela) => !graphql(telas[tela]).json('errors'));
  for (let i = 0; i < 5; i += 1) ativas.forEach((tela) => graphql(telas[tela]));
  return { ativas };
}

export default function (dados) {
  if (passo === 'relatorio') return;
  for (const tela of dados.ativas) {
    const resposta = graphql(telas[tela]);
    check(resposta, { 'sem erro': (r) => r.status === 200 && !r.json('errors') });
    metricas[tela].consultas.add(Number(resposta.headers['X-Consultas-Banco']));
    metricas[tela].tempo.add(resposta.timings.duration);
  }
}

const arredondar = (valor) => Math.round(valor * 100) / 100;

function resumo(m, tela) {
  const consultas = m[`consultas_${tela}`];
  if (!consultas) return null;
  const tempo = m[`tempo_${tela}`].values;
  return { consultas: consultas.values.max, mediaMs: arredondar(tempo.avg), p95Ms: arredondar(tempo['p(95)']) };
}

function texto(r, tela) {
  const t = r[tela];
  return t ? `${t.consultas} consultas, média ${t.mediaMs} ms, p95 ${t.p95Ms} ms` : 'não existe';
}

function linha(r) {
  return `Passo ${r.passo}: tela inicial ${texto(r, 'telaInicial')}; últimas ordens ${texto(r, 'ultimasOrdens')}; `
    + `${r.erros} com erro`;
}

export function handleSummary(data) {
  if (passo === 'relatorio') {
    const r = data.setup_data;
    return {
      'relatorio-aula-05.html': gerarRelatorio(r),
      stdout: `\n${r.map(linha).join('\n')}\n`,
    };
  }

  const m = data.metrics;
  const r = {
    telaInicial: resumo(m, 'telaInicial'),
    ultimasOrdens: resumo(m, 'ultimasOrdens'),
    erros: m.checks ? m.checks.values.fails : 0,
  };
  return {
    [`relatorio-aula-05-${passo}.json`]: JSON.stringify(r),
    stdout: `\n${linha({ passo, ...r })}\n`,
  };
}
