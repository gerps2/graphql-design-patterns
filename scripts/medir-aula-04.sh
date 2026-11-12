#!/usr/bin/env bash
# scripts/medir-aula-04.sh
# Mede a consistência da escrita em quatro cenários. Cada cenário sobe a API com uma configuração,
# zera o saldo por SQL, roda o k6 e derruba a API. No fim, o k6 junta tudo em relatorio-aula-04.html.
# Precisa do Postgres de pé e migrado (make up, make migrar). Roda no Git Bash, no macOS e no Linux.
set -euo pipefail

COMPOSE="docker compose -f deployments/docker-compose.yml"
sql() { $COMPOSE exec -T postgres psql -q -U corretora -d corretora -c "$1"; }
no_ar() { curl -s -o /dev/null http://localhost:5000/graphql?sdl; }

if no_ar; then
  echo "A porta 5000 está ocupada. Pare a API (make run) antes de medir." >&2
  exit 1
fi

# No Git Bash o kill não derruba um processo nativo do Windows; lá quem resolve é o taskkill.
# Depois, espera a porta liberar para o próximo cenário não encontrar a API antiga.
parar() {
  if [ -r "/proc/$1/winpid" ] && command -v taskkill > /dev/null; then
    taskkill //F //PID "$(cat "/proc/$1/winpid")" > /dev/null 2>&1 || true
  else
    kill "$1" 2> /dev/null || true
  fi
  wait "$1" 2> /dev/null || true
  while no_ar; do sleep 1; done
}

# Se algo der errado no meio, a API não fica presa na porta 5000.
api=
trap '[ -n "$api" ] && parar "$api"' EXIT

dotnet build src/Corretora.Api -v q -nologo > /dev/null

medir() {
  local cenario=$1; shift
  sql "UPDATE contas SET saldo = 10000 WHERE usuario_id = 1; UPDATE contas SET saldo = 1000 WHERE usuario_id = 5;"

  dotnet src/Corretora.Api/bin/Debug/net10.0/Corretora.Api.dll --contentRoot "$PWD/src/Corretora.Api" \
    --urls http://localhost:5000 --environment Development "$@" \
    > "relatorio-aula-04-$cenario.log" 2>&1 &
  api=$!
  until no_ar; do
    if ! kill -0 "$api" 2> /dev/null; then
      echo "A API do cenário $cenario não subiu. Veja relatorio-aula-04-$cenario.log." >&2
      api=
      exit 1
    fi
    sleep 1
  done

  k6 run --quiet -e CENARIO="$cenario" scripts/medir-aula-04.js
  parar "$api"
  api=
}

medir falha --Aula:FalharAoGravarOrdem=true
medir SemTrava --Concorrencia:Estrategia=SemTrava
medir Pessimista --Concorrencia:Estrategia=Pessimista
medir Otimista --Concorrencia:Estrategia=Otimista

k6 run --quiet -e CENARIO=relatorio scripts/medir-aula-04.js
