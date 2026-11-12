#!/usr/bin/env bash
# scripts/medir-aula-05.sh
# Mede a tela inicial: quantas consultas cada request manda ao banco (header X-Consultas-Banco) e quanto tempo leva.
# PASSOS escolhe o que medir, separado por espaço: "atual" é o código de agora; um número N é o commit
# "aula-05 passo N" desta branch, compilado numa cópia temporária (git worktree). Padrão: só o atual.
# Precisa do banco de pé e migrado (make up, make migrar). Roda no Git Bash, no macOS e no Linux.
set -euo pipefail

PASSOS=${PASSOS:-atual}

if curl -s -o /dev/null http://localhost:5000; then
  echo "A porta 5000 está ocupada. Pare a API (make run) antes de medir." >&2
  exit 1
fi

# No Git Bash o kill não derruba um processo nativo do Windows; lá quem resolve é o taskkill.
parar() {
  if [ -r "/proc/$1/winpid" ] && command -v taskkill > /dev/null; then
    taskkill //F //PID "$(cat "/proc/$1/winpid")" > /dev/null
  else
    kill "$1"
  fi
  wait "$1" 2> /dev/null || true
}

# Se algo der errado no meio, a API não fica presa na porta 5000 e as cópias temporárias somem.
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
    --urls http://localhost:5000 --environment Development --Aula:MostrarSql=false \
    > "relatorio-aula-05-$passo.log" 2>&1 &
  api=$!
  until curl -s -o /dev/null http://localhost:5000/graphql?sdl; do sleep 1; done

  k6 run --quiet -e PASSO="$passo" scripts/medir-aula-05.js
  parar "$api"
  api=
done

k6 run --quiet -e PASSO=relatorio -e PASSOS="$PASSOS" scripts/medir-aula-05.js
