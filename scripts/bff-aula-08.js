// scripts/bff-aula-08.js
// A tela inicial do celular montada de dois jeitos, com CLIENTES clientes de cada lado por SEGUNDOS segundos.
//   Sem BFF: o app chama a API (GraphQL) e o serviço de notificações (REST), em paralelo,
//            soma o patrimônio e corta os três últimos avisos na tela.
//   Com BFF: o app faz uma chamada só ao BFF, que devolve a tela pronta.
// Para cada um: telas montadas, chamadas do app, bytes que o app baixou e o tempo médio até a tela ficar pronta.
// Os dois cenários rodam ao mesmo tempo. Precisa de make servicos e make run-bff. Quem chama é o make bff.
import http from 'k6/http';
import { sleep } from 'k6';
import { Counter, Trend } from 'k6/metrics';

const api = __ENV.API_URL || 'http://localhost:5000';
const notificacoes = __ENV.NOTIFICACOES_URL || 'http://localhost:5200';
const bff = __ENV.BFF_URL || 'http://localhost:5100';
const clientes = Number(__ENV.CLIENTES || 10);
const duracao = Number(__ENV.SEGUNDOS || 30);
const json = { headers: { 'Content-Type': 'application/json' } };

const m = {
  sem: { telas: new Counter('sem_telas'), chamadas: new Counter('sem_chamadas'), bytes: new Counter('sem_bytes'), tempo: new Trend('sem_tempo', true) },
  com: { telas: new Counter('com_telas'), chamadas: new Counter('com_chamadas'), bytes: new Counter('com_bytes'), tempo: new Trend('com_tempo', true) },
};

export const options = {
  scenarios: {
    semBff: { executor: 'constant-vus', vus: clientes, duration: `${duracao}s`, exec: 'semBff' },
    comBff: { executor: 'constant-vus', vus: clientes, duration: `${duracao}s`, exec: 'comBff' },
  },
};

const usuario = () => 1 + Math.floor(Math.random() * 5);

export function semBff() {
  const id = usuario();
  const inicio = Date.now();
  const [daApi, dasNotificacoes] = http.batch([
    ['POST', `${api}/graphql`, JSON.stringify({ query: `{ usuario(id: ${id}) { nome saldoDisponivel carteira { ativo saldo } } }` }), json],
    ['GET', `${notificacoes}/notificacoes?usuarioIds=${id}`],
  ]);
  // O trabalho que fica no app: somar o patrimônio e cortar os avisos.
  const u = daApi.json('data.usuario');
  const patrimonio = u.saldoDisponivel + u.carteira.reduce((soma, p) => soma + p.saldo, 0);
  const avisos = dasNotificacoes.json().slice(0, 3);
  m.sem.tempo.add(Date.now() - inicio);
  if (patrimonio > 0 && avisos) m.sem.telas.add(1);
  m.sem.chamadas.add(2);
  m.sem.bytes.add(daApi.body.length + dasNotificacoes.body.length);
  sleep(0.5);
}

export function comBff() {
  const id = usuario();
  const inicio = Date.now();
  const r = http.post(`${bff}/graphql`,
    JSON.stringify({ query: `{ inicio(usuarioId: ${id}) { nome patrimonio avisos(ultimos: 3) { texto } } }` }), json);
  m.com.tempo.add(Date.now() - inicio);
  if (r.json('data.inicio.patrimonio') > 0) m.com.telas.add(1);
  m.com.chamadas.add(1);
  m.com.bytes.add(r.body.length);
  sleep(0.5);
}

function valor(data, nome, campo) {
  return data.metrics[nome] ? data.metrics[nome].values[campo] : 0;
}

export function handleSummary(data) {
  const linha = (rotulo, a, b) => `${rotulo.padEnd(34)}${String(a).padEnd(22)}${b}`;
  const ms = (v) => `${Math.round(v)} ms`;
  const porTela = (nome, telas) => (valor(data, telas, 'count') ? Math.round(valor(data, nome, 'count') / valor(data, telas, 'count')) : 0);
  const texto = [
    '',
    `A tela inicial do celular, ${clientes} clientes de cada lado por ${duracao} s`,
    linha('', 'Sem BFF', 'Com BFF'),
    '-'.repeat(72),
    linha('Telas montadas', valor(data, 'sem_telas', 'count'), valor(data, 'com_telas', 'count')),
    linha('Chamadas do app por tela', porTela('sem_chamadas', 'sem_telas'), porTela('com_chamadas', 'com_telas')),
    linha('Bytes baixados por tela', porTela('sem_bytes', 'sem_telas'), porTela('com_bytes', 'com_telas')),
    linha('Tempo médio até a tela pronta', ms(valor(data, 'sem_tempo', 'avg')), ms(valor(data, 'com_tempo', 'avg'))),
    linha('Tempo no pior 5% (p95)', ms(valor(data, 'sem_tempo', 'p(95)')), ms(valor(data, 'com_tempo', 'p(95)'))),
    linha('Contas feitas no app', 'patrimônio e avisos', 'nenhuma'),
    '',
  ].join('\n');
  return { stdout: texto, 'relatorio-aula-08.json': JSON.stringify(data.metrics) };
}
