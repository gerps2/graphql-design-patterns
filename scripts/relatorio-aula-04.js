// scripts/relatorio-aula-04.js
// Monta o HTML do relatório a partir da consistência medida pelo k6.

const cartao = (valor, texto) => `<section class="cartao resumo"><strong>${valor}</strong> ${texto}</section>`;

export function gerarRelatorio(falha) {
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
  .sub { color:var(--suave); font-size:18px; margin-bottom:32px; }
  .cartao { background:var(--cartao); border:1px solid var(--linha); border-radius:14px; padding:26px; margin-bottom:18px; }
  .resumo { font-size:20px; }
  .resumo strong { color:var(--destaque); font-size:34px; margin-right:10px; }
  .nota { color:var(--suave); font-size:14px; margin-top:20px; }
</style>
</head>
<body>
<main>
  <p class="marca">CORRETORA · AULA 4</p>
  <h1>Consistência da escrita</h1>
  <p class="sub">Quando a ordem falha no meio do caminho, o dinheiro some?</p>
  ${cartao(falha.orfaos, `débito sem ordem depois de uma falha ao gravar a ordem (${falha.debitos} débito, ${falha.ordens} ordem)`)}
  <p class="nota">Medido com k6 contra o Postgres local. Conta registros, sem medir tempo. Não é teste de carga.</p>
</main>
</body>
</html>`;
}
