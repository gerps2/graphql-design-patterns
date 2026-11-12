// scripts/relatorio-aula-05.js
// Monta o HTML do relatório a partir do que o k6 mediu em cada passo.

const nome = (passo) => (passo === 'atual' ? 'Código atual' : `Passo ${passo}`);

const cartao = (valor, texto) => `<section class="cartao resumo"><strong>${valor}</strong> ${texto}</section>`;

const celulas = (t) => (t
  ? `<td><strong>${t.consultas}</strong></td><td>${t.mediaMs} ms</td><td>${t.p95Ms} ms</td>`
  : '<td colspan="3" class="ausente">ainda não existe</td>');

function linhaDaTabela(r) {
  return `<tr><td>${nome(r.passo)}</td>${celulas(r.telaInicial)}${celulas(r.ultimasOrdens)}<td>${r.erros}</td></tr>`;
}

// Compara o primeiro e o último passo que mediram a tela.
function comparar(resultados, tela, titulo) {
  const medidos = resultados.filter((r) => r[tela]);
  if (medidos.length === 0) return '';
  const antes = medidos[0];
  const depois = medidos[medidos.length - 1];
  if (medidos.length === 1) {
    return cartao(antes[tela].consultas, `consultas ao banco por ${titulo} (${nome(antes.passo)}, média ${antes[tela].mediaMs} ms)`);
  }
  return cartao(`${antes[tela].consultas} → ${depois[tela].consultas}`,
    `consultas por ${titulo} e ${antes[tela].mediaMs} → ${depois[tela].mediaMs} ms de tempo médio (${nome(antes.passo)} → ${nome(depois.passo)})`);
}

export function gerarRelatorio(resultados) {
  return `<!doctype html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Performance das telas</title>
<style>
  :root { --fundo:#0B1230; --cartao:#141D45; --linha:#2B3669; --texto:#F4F6FC; --suave:#A9B3D1; --destaque:#E63B6F; }
  * { box-sizing:border-box; margin:0; }
  body { min-height:100vh; padding:48px 24px; background:var(--fundo); color:var(--texto); font-family:"Segoe UI",system-ui,sans-serif; }
  main { max-width:1080px; margin:0 auto; zoom:1.2; }
  .marca { color:var(--destaque); font-size:14px; font-weight:700; letter-spacing:2px; }
  h1 { margin:8px 0 6px; font-size:40px; font-weight:600; }
  h2 { margin:28px 0 12px; font-size:22px; font-weight:600; }
  .sub { color:var(--suave); font-size:18px; margin-bottom:32px; }
  .cartao { background:var(--cartao); border:1px solid var(--linha); border-radius:14px; padding:26px; margin-bottom:18px; }
  .resumo { font-size:20px; }
  .resumo strong { color:var(--destaque); font-size:34px; margin-right:10px; }
  table { width:100%; border-collapse:collapse; font-size:16px; }
  th, td { text-align:left; padding:10px 8px; border-bottom:1px solid var(--linha); }
  th { color:var(--suave); font-weight:600; }
  td strong { color:var(--destaque); }
  .ausente { color:var(--suave); }
  code { color:var(--suave); }
  .nota { color:var(--suave); font-size:14px; margin-top:20px; line-height:1.5; }
</style>
</head>
<body>
<main>
  <p class="marca">CORRETORA · AULA 5</p>
  <h1>Performance das telas</h1>
  <p class="sub">Uma tela só: quantas vezes o banco trabalha, e quanto tempo isso leva?</p>
  ${comparar(resultados, 'telaInicial', 'tela inicial')}
  ${comparar(resultados, 'ultimasOrdens', 'lista de últimas ordens')}
  <h2>Por passo</h2>
  <section class="cartao">
    <table>
      <tr><th rowspan="2">Código</th><th colspan="3">Tela inicial</th><th colspan="3">Últimas ordens</th><th rowspan="2">Erros</th></tr>
      <tr><th>Consultas</th><th>Média</th><th>p95</th><th>Consultas</th><th>Média</th><th>p95</th></tr>
      ${resultados.map(linhaDaTabela).join('\n      ')}
    </table>
  </section>
  <p class="nota">Tela inicial: <code>usuarios { nome carteira { ativo saldo } transacoes { ativo valor } }</code>.
  Últimas ordens: <code>ultimasOrdens(quantidade: 20) { ativo valor dono { nome } }</code>.<br>
  Medido com k6 (5 usuários virtuais por 15 s, as duas telas em sequência) contra o Postgres local, com o log de SQL desligado.
  O tempo depende da máquina: compare os passos entre si, na mesma máquina.</p>
</main>
</body>
</html>`;
}
