// scripts/medir-aula-04.js
// Envia uma ordem com a falha ligada e conta os débitos que ficaram sem ordem (órfãos).
// Antes: make falha em outro terminal. Rodar: make medir (gera relatorio-aula-04.html e abre no navegador).
import http from 'k6/http';
import { gerarRelatorio } from './relatorio-aula-04.js';

const base = __ENV.BASE_URL || 'http://localhost:5000';
const usuario = 1;
const valorDaOrdem = 385;
const ordem = `mutation { enviarOrdem(input: { usuarioId: ${usuario}, ativo: "PETR4", quantidade: 10, preco: 38.5 }) { ordem { id } } }`;

function graphql(query) {
  return http.post(`${base}/graphql`, JSON.stringify({ query }), {
    headers: { 'Content-Type': 'application/json' },
  });
}

function estado() {
  const dados = graphql(`{ usuario(id: ${usuario}) { saldoDisponivel ordens { id } } }`).json('data.usuario');
  return { saldo: dados.saldoDisponivel, ordens: dados.ordens.length };
}

function ordemComFalha() {
  const antes = estado();
  graphql(ordem);
  const depois = estado();

  const debitos = Math.round((antes.saldo - depois.saldo) / valorDaOrdem);
  const ordens = depois.ordens - antes.ordens;
  return { debitos, ordens, orfaos: debitos - ordens };
}

export const options = { vus: 1, iterations: 1 };

export function setup() {
  return { falha: ordemComFalha() };
}

export default function () {}

export function handleSummary(data) {
  const { falha } = data.setup_data;
  return {
    'relatorio-aula-04.html': gerarRelatorio(falha),
    stdout: `\nOrdem com falha: ${falha.debitos} débito, ${falha.ordens} ordem, ${falha.orfaos} órfão\n`,
  };
}
