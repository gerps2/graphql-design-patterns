// scripts/relatorio-aula-05.js
// Monta o HTML do relatório a partir do que o k6 mediu em cada passo.

const nome = (passo) => (passo === 'atual' ? 'Código atual' : `Passo ${passo}`);

const cartao = (valor, texto) => `<section class="cartao resumo"><strong>${valor}</strong> ${texto}</section>`;

function linhaDaTabela(r) {
  return `<tr><td>${nome(r.passo)}</td><td><strong>${r.consultas}</strong></td><td>${r.mediaMs} ms</td>`
    + `<td>${r.p95Ms} ms</td><td>${r.porSegundo}</td><td>${r.erros}</td></tr>`;
}

export function gerarRelatorio(resultados) {
  const antes = resultados[0];
  const depois = resultados[resultados.length - 1];
  const resumo = resultados.length > 1
    ? cartao(`${antes.consultas} → ${depois.consultas}`, `consultas ao banco por tela (${nome(antes.passo)} → ${nome(depois.passo)})`)
      + cartao(`${antes.mediaMs} → ${depois.mediaMs} ms`, 'de tempo médio por tela')
    : cartao(antes.consultas, 'consultas ao banco para montar uma tela')
      + cartao(`${antes.mediaMs} ms`, `de tempo médio por tela (p95 ${antes.p95Ms} ms)`);

  return `<!doctype html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Performance da tela inicial</title>
<style>
  :root { --fundo:#0B1230; --cartao:#141D45; --linha:#2B3669; --texto:#F4F6FC; --suave:#A9B3D1; --destaque:#E63B6F; }
  * { box-sizing:border-box; margin:0; }
  body { min-height:100vh; padding:48px 24px; background:var(--fundo); color:var(--texto); font-family:"Segoe UI",system-ui,sans-serif; }
  main { max-width:980px; margin:0 auto; zoom:1.2; }
  .marca { color:var(--destaque); font-size:14px; font-weight:700; letter-spacing:2px; }
  h1 { margin:8px 0 6px; font-size:40px; font-weight:600; }
  h2 { margin:28px 0 12px; font-size:22px; font-weight:600; }
  .sub { color:var(--suave); font-size:18px; margin-bottom:32px; }
  .cartao { background:var(--cartao); border:1px solid var(--linha); border-radius:14px; padding:26px; margin-bottom:18px; }
  .resumo { font-size:20px; }
  .resumo strong { color:var(--destaque); font-size:34px; margin-right:10px; }
  table { width:100%; border-collapse:collapse; font-size:17px; }
  th, td { text-align:left; padding:10px 8px; border-bottom:1px solid var(--linha); }
  th { color:var(--suave); font-weight:600; }
  td strong { color:var(--destaque); }
  code { color:var(--suave); }
  .nota { color:var(--suave); font-size:14px; margin-top:20px; }
</style>
</head>
<body>
<main>
  <p class="marca">CORRETORA · AULA 5</p>
  <h1>Performance da tela inicial</h1>
  <p class="sub">Uma tela só: quantas vezes o banco trabalha, e quanto tempo isso leva?</p>
  ${resumo}
  <h2>Por passo</h2>
  <section class="cartao">
    <table>
      <tr><th>Código</th><th>Consultas por tela</th><th>Tempo médio</th><th>p95</th><th>Telas por segundo</th><th>Erros</th></tr>
      ${resultados.map(linhaDaTabela).join('\n      ')}
    </table>
  </section>
  <p class="nota">Query <code>TelaInicial { usuarios { nome carteira { ativo saldo } transacoes { ativo valor } } }</code>.
  Medido com k6 (5 usuários virtuais por 15 s) contra o Postgres local, com o log de SQL desligado.
  O tempo depende da máquina: compare os passos entre si, na mesma máquina.</p>
</main>
</body>
</html>`;
}
