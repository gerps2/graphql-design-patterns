// scripts/erros-aula-06.js
// Faz cada pedido que dá errado uma vez e mostra, num quadro, o que o cliente recebe:
// a mensagem, o código, onde o erro veio (no payload da mutation ou em errors, com o path) e se vazou detalhe interno.
// Rode com a API de pé (make run) e chame pelo make erros.
// Para ver o caso "banco fora do ar", pare o Postgres antes (docker compose ... stop postgres).
import http from 'k6/http';

const base = __ENV.BASE_URL || 'http://localhost:5000';

const casos = [
  ['Saldo insuficiente', 'mutation { enviarOrdem(input: { usuarioId: 1, ativo: "PETR4", quantidade: 100000, preco: 38.5 }) { ordem { id } errors { __typename ... on Error { code message } } } }'],
  ['Entrada inválida', 'mutation { enviarOrdem(input: { usuarioId: 1, ativo: "petr", quantidade: 0, preco: 38.5 }) { ordem { id } errors { __typename ... on Error { code message } } } }'],
  ['Banco fora do ar', '{ usuario(id: 1) { nome saldoDisponivel } }'],
  ['Query em ciclo', '{ ultimasOrdens(quantidade: 10) { dono { ordens { dono { ordens { dono { ordens { dono { ordens { id } } } } } } } } } }'],
  ['30 apelidos (custo)', `{ ${Array.from({ length: 30 }, (_, i) => `a${i}: usuarios { ordens { dono { nome } } transacoes { valor } carteira { saldo } rentabilidade { ativo } }`).join(' ')} }`],
  ['Introspecção', '{ __schema { types { name } } }'],
];

export const options = { vus: 1, iterations: 1 };

// Os pedidos vão no setup: o que ele devolve chega inteiro ao handleSummary, que desenha o quadro.
export function setup() {
  return casos.map(([caso, query]) => {
    const resposta = http.post(`${base}/graphql`, JSON.stringify({ query }), {
      headers: { 'Content-Type': 'application/json' },
    });
    let corpo = {};
    try { corpo = resposta.json(); } catch (e) { corpo = {}; }
    const vazou = /Exception|Npgsql|at Corretora|stackTrace/i.test(resposta.body || '');
    // O erro de negócio vem dentro do payload da mutation (data.enviarOrdem.errors); o resto, em errors.
    const payload = Object.values(corpo.data || {}).find((v) => v && Array.isArray(v.errors));
    const doPayload = payload ? payload.errors[0] : null;
    const erro = (corpo.errors || [])[0];
    return {
      caso,
      status: resposta.status,
      resultado: doPayload ? `${doPayload.__typename}: ${doPayload.message}` : (erro ? erro.message : 'passou, sem erro'),
      codigo: doPayload ? doPayload.code : (erro && erro.extensions && erro.extensions.code ? erro.extensions.code : '-'),
      caminho: doPayload ? 'payload' : (erro && erro.path ? erro.path.join('.') : '-'),
      vazou: vazou ? 'sim' : 'não',
    };
  });
}

export default function () {}

function coluna(texto, largura) {
  const t = String(texto);
  return t.length > largura ? `${t.slice(0, largura - 1)}…` : t.padEnd(largura);
}

export function handleSummary(data) {
  const linhas = data.setup_data || [];
  const titulo = `${coluna('Caso', 21)} ${coluna('HTTP', 5)} ${coluna('O cliente recebe', 46)} ${coluna('Código', 26)} ${coluna('Caminho', 12)} Vazou detalhe?`;
  const corpo = linhas.map((l) => `${coluna(l.caso, 21)} ${coluna(l.status, 5)} ${coluna(l.resultado, 46)} ${coluna(l.codigo, 26)} ${coluna(l.caminho, 12)} ${l.vazou}`);
  return { stdout: `\n${titulo}\n${'-'.repeat(titulo.length)}\n${corpo.join('\n')}\n\n` };
}
