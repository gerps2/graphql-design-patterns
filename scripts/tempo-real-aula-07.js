// scripts/tempo-real-aula-07.js
// Polling contra subscription, os dois com CLIENTES clientes olhando a PETR4 por SEGUNDOS segundos.
//   Polling: cada cliente pergunta a cotação de 1 em 1 segundo.
//   Subscription: cada cliente assina cotacaoAtualizada pelo WebSocket e espera.
// Para cada um: quantas mensagens chegaram, quantos bytes, e o atraso entre o preço mudar na bolsa
// (o horario da cotação) e o cliente ver o preço novo. Os dois cenários rodam ao mesmo tempo e veem as mesmas mudanças.
// Precisa da API de pé (make run). Quem chama é o make tempo-real.
import http from 'k6/http';
import { sleep } from 'k6';
import { WebSocket } from 'k6/websockets';
import { Counter, Trend } from 'k6/metrics';

const base = __ENV.BASE_URL || 'http://localhost:5000';
const clientes = Number(__ENV.CLIENTES || 10);
const duracao = Number(__ENV.SEGUNDOS || 60);

const m = {
  polling: { mensagens: new Counter('polling_mensagens'), novas: new Counter('polling_novas'), bytes: new Counter('polling_bytes'), atraso: new Trend('polling_atraso', true) },
  sub: { mensagens: new Counter('sub_mensagens'), novas: new Counter('sub_novas'), bytes: new Counter('sub_bytes'), atraso: new Trend('sub_atraso', true) },
};

export const options = {
  scenarios: {
    polling: { executor: 'constant-vus', vus: clientes, duration: `${duracao}s`, exec: 'polling' },
    subscription: { executor: 'per-vu-iterations', vus: clientes, iterations: 1, maxDuration: `${duracao + 10}s`, exec: 'subscription' },
  },
};

let anterior = null;

export function polling() {
  const r = http.post(`${base}/graphql`, JSON.stringify({ query: '{ cotacao(ativo: "PETR4") { preco horario } }' }),
    { headers: { 'Content-Type': 'application/json' } });
  m.polling.mensagens.add(1);
  m.polling.bytes.add(r.body ? r.body.length : 0);
  const horario = r.json('data.cotacao.horario');
  if (anterior !== null && horario !== anterior) {
    m.polling.novas.add(1);
    m.polling.atraso.add(Date.now() - Date.parse(horario));
  }
  anterior = horario;
  sleep(1);
}

export function subscription() {
  const ws = new WebSocket(base.replace('http', 'ws') + '/graphql', 'graphql-transport-ws');
  ws.onopen = () => ws.send(JSON.stringify({ type: 'connection_init' }));
  ws.onmessage = (e) => {
    const msg = JSON.parse(e.data);
    if (msg.type === 'connection_ack') {
      ws.send(JSON.stringify({ id: '1', type: 'subscribe', payload: { query: 'subscription { cotacaoAtualizada(ativo: "PETR4") { preco horario } }' } }));
    } else if (msg.type === 'next') {
      m.sub.mensagens.add(1);
      m.sub.novas.add(1);
      m.sub.bytes.add(e.data.length);
      m.sub.atraso.add(Date.now() - Date.parse(msg.payload.data.cotacaoAtualizada.horario));
    }
  };
  setTimeout(() => ws.close(), duracao * 1000);
}

function valor(data, nome, campo) {
  return data.metrics[nome] ? data.metrics[nome].values[campo] : 0;
}

export function handleSummary(data) {
  const linha = (rotulo, a, b) => `${rotulo.padEnd(34)}${String(a).padEnd(16)}${b}`;
  const ms = (v) => `${Math.round(v)} ms`;
  const texto = [
    '',
    `${clientes} clientes olhando a PETR4 por ${duracao} s`,
    linha('', 'Polling (1 s)', 'Subscription'),
    '-'.repeat(66),
    linha('Mensagens recebidas', valor(data, 'polling_mensagens', 'count'), valor(data, 'sub_mensagens', 'count')),
    linha('Com preço novo', valor(data, 'polling_novas', 'count'), valor(data, 'sub_novas', 'count')),
    linha('Bytes recebidos', valor(data, 'polling_bytes', 'count'), valor(data, 'sub_bytes', 'count')),
    linha('Atraso médio até ver o preço novo', ms(valor(data, 'polling_atraso', 'avg')), ms(valor(data, 'sub_atraso', 'avg'))),
    '',
  ].join('\n');
  return { stdout: texto, 'relatorio-aula-07.json': JSON.stringify(data.metrics) };
}
