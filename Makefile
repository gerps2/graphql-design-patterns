.PHONY: up down migrar banco-zerar run run-dapper falha build medir test test-unit test-arch test-ordem clean

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
# PROVEDOR=Dapper troca quem grava (padrão: EfCore).
ESTRATEGIA ?= Pessimista
PROVEDOR ?= EfCore

run:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Concorrencia:Estrategia=$(ESTRATEGIA) --Persistencia:Provedor=$(PROVEDOR)

# A mesma API gravando com Dapper: SQL escrito à mão, o handler não muda.
run-dapper:
	$(MAKE) run PROVEDOR=Dapper

# Sobe a API com a falha ao gravar a ordem ligada (só para a aula).
falha:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Aula:FalharAoGravarOrdem=true

build:
	dotnet build Corretora.sln

# Sobe a API sozinha e mede com o k6. Precisa de make up e make migrar.
# AULA=05 (padrão): consultas e tempo da tela inicial. PASSOS="0 atual" compara o passo 0 com o código atual.
# AULA=04: consistência da escrita em cada cenário (falha, sem trava, pessimista, otimista).
AULA ?= 05
PASSOS ?= atual

medir:
	PROVEDOR=$(PROVEDOR) PASSOS="$(PASSOS)" bash scripts/medir-aula-$(AULA).sh
	$(ABRIR) relatorio-aula-$(AULA).html

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
