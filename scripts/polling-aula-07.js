// scripts/polling-aula-07.js
// O jeito ingênuo de saber se o preço mudou: perguntar a cada segundo.
// CLIENTES usuários virtuais perguntam a cotação da PETR4 de 1 em 1 segundo por DURACAO.
// No fim, um quadro: quantas perguntas, quantas voltaram iguais à anterior e quantos bytes trafegaram.
// Precisa da API de pé (make run). Quem chama é o make polling.
import http from 'k6/http';
import { sleep } from 'k6';
import { Counter } from 'k6/metrics';

const base = __ENV.BASE_URL || 'http://localhost:5000';
const query = JSON.stringify({ query: '{ cotacao(ativo: "PETR4") { preco horario } }' });

const perguntas = new Counter('perguntas');
const iguais = new Counter('iguais');
const mudou = new Counter('mudou');
const bytes = new Counter('bytes_resposta');

export const options = {
  vus: Number(__ENV.CLIENTES || 10),
  duration: __ENV.DURACAO || '60s',
};

let anterior = null;

export default function () {
  const r = http.post(`${base}/graphql`, query, { headers: { 'Content-Type': 'application/json' } });
  const preco = r.json('data.cotacao.preco');
  perguntas.add(1);
  bytes.add(r.body ? r.body.length : 0);
  if (anterior !== null && preco === anterior) iguais.add(1); else if (anterior !== null) mudou.add(1);
  anterior = preco;
  sleep(1);
}

export function handleSummary(data) {
  const v = (m) => (data.metrics[m] ? data.metrics[m].values.count : 0);
  const total = v('perguntas');
  const linhas = [
    '',
    `Polling: ${options.vus} clientes, 1 pergunta por segundo, por ${options.duration}`,
    '-'.repeat(60),
    `Perguntas feitas:            ${total}`,
    `Voltaram iguais à anterior:  ${v('iguais')} (${total ? Math.round((v('iguais') / total) * 100) : 0}%)`,
    `Trouxeram preço novo:        ${v('mudou')}`,
    `Bytes recebidos:             ${v('bytes_resposta')}`,
    '',
  ];
  return {
    stdout: linhas.join('\n'),
    'relatorio-aula-07-polling.json': JSON.stringify({ perguntas: total, iguais: v('iguais'), mudou: v('mudou'), bytes: v('bytes_resposta') }),
  };
}
