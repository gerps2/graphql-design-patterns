#!/usr/bin/env bash
# scripts/medir-aula-05.sh
# Mede a tela inicial e a lista de últimas ordens: quantas consultas cada request manda ao banco
# (header X-Consultas-Banco) e quanto tempo leva.
# PASSOS escolhe o que medir, separado por espaço: "atual" é o código de agora; um número N é o commit
# "aula-05 passo N" desta branch, compilado numa cópia temporária (git worktree). O make medir usa "0 atual".
# PORTA troca a porta da API medida (padrão: 5000).
# Precisa do banco e do Redis de pé, com o banco migrado (make up, make migrar). Roda no Git Bash, no macOS e no Linux.
set -euo pipefail

PASSOS=${PASSOS:-atual}
PORTA=${PORTA:-5000}
BASE_URL="http://localhost:$PORTA"

if curl -s -o /dev/null "$BASE_URL"; then
  echo "A porta $PORTA está ocupada. Pare a API (make run) antes de medir, ou use PORTA=<outra>." >&2
  exit 1
fi

# No Git Bash o kill não derruba um processo nativo do Windows; lá quem resolve é o taskkill.
parar() {
  if [ -r "/proc/$1/winpid" ] && command -v taskkill > /dev/null; then
    taskkill //F //PID "$(cat "/proc/$1/winpid")" > /dev/null
  else
    kill "$1" 2> /dev/null || true
  fi
  wait "$1" 2> /dev/null || true
}

# Se algo der errado no meio, a API não fica presa na porta e as cópias temporárias somem.
api=
copias=()
limpar() {
  if [ -n "$api" ]; then parar "$api"; fi
  for copia in "${copias[@]+"${copias[@]}"}"; do git worktree remove --force "$copia" > /dev/null 2>&1 || true; done
}
trap limpar EXIT

for passo in $PASSOS; do
  # O código atual é esta pasta. Um passo antigo vira uma cópia do commit dele.
  pasta=$PWD
  if [ "$passo" != atual ]; then
    commit=$(git log --format=%h --grep="^aula-05 passo $passo:" -1)
    if [ -z "$commit" ]; then echo "Não achei o commit do passo $passo." >&2; exit 1; fi
    pasta="$(mktemp -d)/passo-$passo"
    git worktree add --detach "$pasta" "$commit" > /dev/null 2>&1
    copias+=("$pasta")
  fi

  echo "Passo $passo: compilando"
  dotnet build "$pasta/src/Corretora.Api" -v q -nologo > /dev/null

  # O SQL no terminal fica desligado: escrever no console também custa tempo.
  dotnet "$pasta/src/Corretora.Api/bin/Debug/net10.0/Corretora.Api.dll" --contentRoot "$pasta/src/Corretora.Api" \
    --urls "$BASE_URL" --environment Development --Aula:MostrarSql=false \
    > "relatorio-aula-05-$passo.log" 2>&1 &
  api=$!
  until curl -s -o /dev/null "$BASE_URL/graphql?sdl"; do
    if ! kill -0 "$api" 2> /dev/null; then echo "A API do passo $passo não subiu. Veja relatorio-aula-05-$passo.log." >&2; exit 1; fi
    sleep 1
  done

  k6 run --quiet -e PASSO="$passo" -e BASE_URL="$BASE_URL" scripts/medir-aula-05.js
  parar "$api"
  api=
done

k6 run --quiet -e PASSO=relatorio -e PASSOS="$PASSOS" scripts/medir-aula-05.js
