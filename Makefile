.PHONY: up obs servicos run-notificacoes run-bff down migrar banco-zerar run run-dapper falha falha-rentabilidade build medir erros polling duas-instancias tempo-real test test-unit test-arch test-ordem clean

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

# A stack de observabilidade: Collector, Prometheus, Loki, Jaeger e Grafana (http://localhost:3000).
obs:
	$(COMPOSE) --profile observabilidade up -d --wait

# A API e o serviço de notificações em contêineres (Aula 8): API em 5000, notificações em 5200.
# Pare antes o make run, porque a porta 5000 passa a ser do contêiner.
servicos:
	$(COMPOSE) --profile servicos up -d --build --wait

down:
	$(COMPOSE) --profile observabilidade --profile servicos down

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
# DETALHES=true põe a mensagem interna e a stack da exceção na resposta (só para ver o que vaza).
DETALHES ?= false
# INTROSPECCAO=false fecha o schema como em produção (o Nitro deixa de ver os tipos).
INTROSPECCAO ?= true
# PORTA=5001 sobe uma segunda instância da API ao lado da primeira (Aula 7).
PORTA ?= 5000

run:
	dotnet run --project src/Corretora.Api --urls http://localhost:$(PORTA) -- --Concorrencia:Estrategia=$(ESTRATEGIA) --Persistencia:Provedor=$(PROVEDOR) --GraphQL:DetalhesDaExcecao=$(DETALHES) --GraphQL:PermitirIntrospeccao=$(INTROSPECCAO)

# O BFF da corretora na porta 5100, na frente da API (5000) e das notificações (5200) (Aula 8).
run-bff:
	dotnet run --project src/Corretora.Bff

# O serviço de notificações fora do contêiner, na porta 5200 (Aula 8).
run-notificacoes:
	dotnet run --project src/Corretora.Notificacoes

# A mesma API gravando com Dapper: SQL escrito à mão, o handler não muda.
run-dapper:
	$(MAKE) run PROVEDOR=Dapper

# Sobe a API com a falha ao gravar a ordem ligada (só para a aula).
falha:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Aula:FalharAoGravarOrdem=true

# Sobe a API com a leitura da rentabilidade falhando (só para a aula): a resposta chega pela metade.
falha-rentabilidade:
	dotnet run --project src/Corretora.Api --urls http://localhost:5000 -- --Aula:FalharRentabilidade=true

build:
	dotnet build Corretora.sln

# Sobe a API sozinha e mede com o k6. Precisa de make up e make migrar.
# AULA=05 (padrão): consultas e tempo das telas, do passo 0 (antes) e do código atual (depois).
#   PASSOS="0 1 2 3 4 5 6 atual" mede passo a passo; PASSOS=atual mede só o código de agora.
# AULA=04: consistência da escrita em cada cenário (falha, sem trava, pessimista, otimista).
AULA ?= 05
PASSOS ?= 0 atual

medir:
	PROVEDOR=$(PROVEDOR) PASSOS="$(PASSOS)" PORTA="$(PORTA)" bash scripts/medir-aula-$(AULA).sh
	$(ABRIR) relatorio-aula-$(AULA).html

# Faz cada pedido que dá errado uma vez e mostra o que o cliente recebe. Precisa da API de pé (make run).
erros:
	k6 run --quiet scripts/erros-aula-06.js

# Dez clientes perguntam a cotação da PETR4 de 1 em 1 segundo e o k6 conta quantas respostas voltaram iguais.
# Precisa da API de pé (make run). DURACAO=30s encurta.
DURACAO ?= 60s
polling:
	k6 run --quiet -e DURACAO=$(DURACAO) scripts/polling-aula-07.js

# Polling contra subscription: dez clientes de cada jeito olhando a PETR4 ao mesmo tempo, por SEGUNDOS segundos.
# Compara mensagens, bytes e o atraso até ver o preço novo. Precisa da API de pé (make run).
SEGUNDOS ?= 60
tempo-real:
	k6 run --quiet -e SEGUNDOS=$(SEGUNDOS) scripts/tempo-real-aula-07.js

# Assina as ordens da Ana na instância da porta 5001 e envia uma ordem pela instância da 5000.
# Precisa das duas de pé: make run e, em outro terminal, make run PORTA=5001.
duas-instancias:
	k6 run --quiet scripts/duas-instancias-aula-07.js

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
