// scripts/medir-aula-05.js
// Um passo por execução (PASSO): a tela inicial sob carga leve, contando consultas e tempo.
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

const telaInicial = 'query TelaInicial { usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }';
const consultas = new Trend('consultas_ao_banco');

function graphql(query) {
  return http.post(`${base}/graphql`, JSON.stringify({ query }), {
    headers: { 'Content-Type': 'application/json' },
  });
}

// Cinco usuários virtuais por 15 segundos. Cada um pede a tela inicial sem parar.
export const options = passo === 'relatorio'
  ? { vus: 1, iterations: 1 }
  : { vus: 5, duration: '15s' };

export function setup() {
  if (passo === 'relatorio') return resultados;
  // Aquecimento: as primeiras chamadas compilam o schema e abrem conexões. Elas não entram na medição.
  for (let i = 0; i < 5; i += 1) graphql(telaInicial);
  return null;
}

export default function () {
  if (passo === 'relatorio') return;
  const resposta = graphql(telaInicial);
  check(resposta, { 'sem erro': (r) => r.status === 200 && !r.json('errors') });
  consultas.add(Number(resposta.headers['X-Consultas-Banco']));
}

const arredondar = (valor) => Math.round(valor * 100) / 100;

function linha(r) {
  return `Passo ${r.passo}: ${r.consultas} consultas por tela, média ${r.mediaMs} ms, p95 ${r.p95Ms} ms, `
    + `${r.porSegundo} telas/s, ${r.erros} com erro`;
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
    consultas: m.consultas_ao_banco.values.max,
    mediaMs: arredondar(m.http_req_duration.values.avg),
    p95Ms: arredondar(m.http_req_duration.values['p(95)']),
    porSegundo: arredondar(m.http_reqs.values.rate),
    erros: m.checks.values.fails,
  };
  return {
    [`relatorio-aula-05-${passo}.json`]: JSON.stringify(r),
    stdout: `\n${linha({ passo, ...r })}\n`,
  };
}
