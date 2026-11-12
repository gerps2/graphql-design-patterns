// scripts/medir-aula-04.js
// Um cenário por execução (CENARIO): falha, SemTrava, Pessimista, Otimista ou relatorio.
// Quem chama é o scripts/medir-aula-04.sh (make medir), que sobe a API certa para cada um.
import http from 'k6/http';
import { gerarRelatorio } from './relatorio-aula-04.js';

const base = __ENV.BASE_URL || 'http://localhost:5000';
const cenario = __ENV.CENARIO || 'falha';
const cenarios = ['falha', 'SemTrava', 'Pessimista', 'Otimista'];

// O relatório lê os resultados dos cenários. open() só funciona aqui, na carga do script.
const resultados = cenario === 'relatorio'
  ? Object.fromEntries(cenarios.map((c) => [c, JSON.parse(open(`../relatorio-aula-04-${c}.json`))]))
  : null;

function graphql(query) {
  return http.post(`${base}/graphql`, JSON.stringify({ query }), {
    headers: { 'Content-Type': 'application/json' },
  });
}

function estado(usuario) {
  const dados = graphql(`{ usuario(id: ${usuario}) { saldoDisponivel ordens { id } } }`).json('data.usuario');
  return { saldo: dados.saldoDisponivel, ordens: dados.ordens.length };
}

// Uma ordem de 385 com a falha ligada: quantos débitos ficaram sem ordem?
function ordemComFalha() {
  const usuario = 1;
  const valorDaOrdem = 385;
  const antes = estado(usuario);
  graphql(`mutation { enviarOrdem(input: { usuarioId: ${usuario}, ativo: "PETR4", quantidade: 10, preco: 38.5 }) { ordem { id } } }`);
  const depois = estado(usuario);

  const debitos = Math.round((antes.saldo - depois.saldo) / valorDaOrdem);
  const ordens = depois.ordens - antes.ordens;
  return { debitos, ordens, orfaos: debitos - ordens };
}

// 40 ordens de 100 ao mesmo tempo contra um saldo de 1000: só 10 cabem.
function ordensConcorrentes() {
  const usuario = 5;
  const enviadas = 40;
  const saldoInicial = 1000;
  const antes = estado(usuario);

  const corpo = JSON.stringify({
    query: `mutation { enviarOrdem(input: { usuarioId: ${usuario}, ativo: "HGLG11", quantidade: 1, preco: 100 }) { ordem { id } } }`,
  });
  const pedidos = Array.from({ length: enviadas }, () => [
    'POST', `${base}/graphql`, corpo, { headers: { 'Content-Type': 'application/json' } },
  ]);
  const respostas = http.batch(pedidos);

  // O cliente só vê um erro genérico: saldo insuficiente e conflito chegam iguais (a Aula 6 resolve isso).
  const aceitas = respostas.filter((r) => r.json('data.enviarOrdem.ordem.id')).length;
  const recusadas = respostas.filter((r) => r.json('errors')).length;
  const depois = estado(usuario);
  const ordens = depois.ordens - antes.ordens;
  // Dinheiro que deveria ter saído e não saiu: o débito de uma ordem sobrescreveu o de outra.
  const debitosPerdidos = Math.round((depois.saldo - (saldoInicial - 100 * ordens)) / 100);

  return { enviadas, possiveis: saldoInicial / 100, aceitas, recusadas, ordens, saldoFinal: depois.saldo, debitosPerdidos };
}

export const options = { vus: 1, iterations: 1, batch: 40, batchPerHost: 40 };

export function setup() {
  if (cenario === 'relatorio') return resultados;
  return cenario === 'falha' ? ordemComFalha() : ordensConcorrentes();
}

export default function () {}

function linha(c, r) {
  if (c === 'falha') return `Ordem com falha: ${r.debitos} débito, ${r.ordens} ordem, ${r.orfaos} órfão`;
  return `${c}: ${r.aceitas} de ${r.enviadas} aceitas (cabiam ${r.possiveis}), ${r.recusadas} recusadas, `
    + `saldo final ${r.saldoFinal}, ${r.debitosPerdidos} débitos perdidos`;
}

export function handleSummary(data) {
  const r = data.setup_data;
  if (cenario === 'relatorio') {
    return {
      'relatorio-aula-04.html': gerarRelatorio(r),
      stdout: `\n${cenarios.map((c) => linha(c, r[c])).join('\n')}\n`,
    };
  }
  return {
    [`relatorio-aula-04-${cenario}.json`]: JSON.stringify(r),
    stdout: `\n${linha(cenario, r)}\n`,
  };
}
