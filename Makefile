.PHONY: up down migrar banco-zerar run falha build medir test test-unit test-arch test-ordem clean

ifeq ($(OS),Windows_NT)
ABRIR = powershell -NoProfile -Command Start-Process
else ifeq ($(shell uname),Darwin)
ABRIR = open
else
ABRIR = xdg-open
endif

COMPOSE = docker compose -f deployments/docker-compose.yml

up:
	$(COMPOSE) up -d --wait

down:
	$(COMPOSE) down

migrar:
	dotnet tool restore
	dotnet ef database update --project src/Corretora.Api

banco-zerar:
	$(COMPOSE) down -v
	$(COMPOSE) up -d --wait
	$(MAKE) migrar

# make run ESTRATEGIA=Otimista troca a estratégia de concorrência (padrão: Pessimista).
ESTRATEGIA ?= Pessimista

run:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Concorrencia:Estrategia=$(ESTRATEGIA)

# Sobe a API com a falha ao gravar a ordem ligada (só para a aula).
falha:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Aula:FalharAoGravarOrdem=true

build:
	dotnet build Corretora.sln

# Sobe a API sozinha em cada cenário (falha, sem trava, pessimista, otimista). Precisa de make up e make migrar.
medir:
	bash scripts/medir-aula-04.sh
	$(ABRIR) relatorio-aula-04.html

test:
	dotnet test Corretora.sln

test-unit:
	dotnet test tests/Corretora.UnitTests --logger "console;verbosity=normal"

test-arch:
	dotnet test tests/Corretora.ArchitectureTests --logger "console;verbosity=normal"

test-ordem:
	dotnet test tests/Corretora.IntegrationTests --filter EnviarOrdem --logger "console;verbosity=normal"

clean:
	dotnet clean Corretora.sln
