.PHONY: up down migrar banco-zerar run build test test-unit test-arch clean

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

run:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000

build:
	dotnet build Corretora.sln

test:
	dotnet test Corretora.sln

test-unit:
	dotnet test tests/Corretora.UnitTests --logger "console;verbosity=normal"

test-arch:
	dotnet test tests/Corretora.ArchitectureTests --logger "console;verbosity=normal"

clean:
	dotnet clean Corretora.sln
