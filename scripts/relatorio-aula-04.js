// scripts/relatorio-aula-04.js
// Monta o HTML do relatório a partir da consistência medida pelo k6.

const cartao = (valor, texto) => `<section class="cartao resumo"><strong>${valor}</strong> ${texto}</section>`;

const nomes = {
  SemTrava: 'Sem trava',
  Pessimista: 'Pessimista (FOR UPDATE)',
  Otimista: 'Otimista (xmin)',
};

function linhaDaTabela(chave, r) {
  return `<tr><td>${nomes[chave]}</td><td>${r.aceitas} de ${r.enviadas}</td><td>${r.recusadas}</td>`
    + `<td>${r.saldoFinal}</td><td>${r.debitosPerdidos}</td></tr>`;
}

export function gerarRelatorio(resultados) {
  const { falha } = resultados;
  const linhas = ['SemTrava', 'Pessimista', 'Otimista'].map((c) => linhaDaTabela(c, resultados[c])).join('\n');

  return `<!doctype html>
<html lang="pt-BR">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>Consistência da escrita</title>
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
  .nota { color:var(--suave); font-size:14px; margin-top:20px; }
</style>
</head>
<body>
<main>
  <p class="marca">CORRETORA · AULA 4</p>
  <h1>Consistência da escrita</h1>
  <p class="sub">Quando a ordem falha no meio do caminho, o dinheiro some? E quando 40 chegam juntas?</p>
  ${cartao(falha.orfaos, `débito sem ordem depois de uma falha ao gravar a ordem (${falha.debitos} débito, ${falha.ordens} ordem)`)}
  <h2>40 ordens de 100 ao mesmo tempo, com saldo para 10</h2>
  <section class="cartao">
    <table>
      <tr><th>Estratégia</th><th>Aceitas</th><th>Recusadas</th><th>Saldo final</th><th>Débitos perdidos</th></tr>
      ${linhas}
    </table>
  </section>
  <p class="nota">Medido com k6 contra o Postgres local. Conta registros, sem medir tempo. Não é teste de carga.</p>
</main>
</body>
</html>`;
}
