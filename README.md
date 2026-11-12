Aplicação da disciplina GraphQL e Design Patterns (FIAP PosTech).

```
make up         sobe o Postgres (porta 5433) e o Redis (porta 6379) no Docker
make migrar     cria as tabelas e a carga inicial
make run        sobe a API em http://localhost:5000 (ESTRATEGIA=SemTrava|Pessimista|Otimista)
make run-dapper sobe a API gravando com Dapper no lugar do EF Core
make falha      sobe a API com uma falha ao gravar a ordem (só para a aula)
make medir      mede as telas com o k6 (consultas ao banco e tempo; sobe a API sozinho) e abre o relatório
                compara o passo 0 da Aula 5 com o código atual; PASSOS="0 1 2 3 4 5 6 atual" mede passo a passo
                AULA=04 mede a escrita da Aula 4; PORTA=<outra> mede numa porta que não seja a 5000
make build      compila a solução
make test       roda todos os testes (o Postgres e o Redis dos testes sobem sozinhos, com Testcontainers)
make test-unit  roda só os testes de unidade, sem subir a API
make test-arch  roda só os testes de arquitetura (as regras do slice)
make test-ordem roda só os testes da mutation enviarOrdem
make banco-zerar  apaga o banco e recria do zero
make down       para o Postgres
make clean      limpa os artefatos
```

| Aula | Tema | Branch |
|---|---|---|
| 1 | Fundamentos de GraphQL em .NET | `feat/aula-01-fundamentos` |
| 2 | Schemas e o padrão Resolver | `feat/aula-02-schemas-resolvers` |
| 3 | Queries e CQRS | `feat/aula-03-queries-cqrs` |
| 4 | Mutations, Repository e Unit of Work | `feat/aula-04-mutations-repository` |
| 5 | Performance com DataLoader | `feat/aula-05-dataloader` |

Antes da primeira execução: Docker rodando, `make up` e `make migrar`.

Abra `Corretora.sln` no Visual Studio 2026 ou no Rider e rode o perfil `http`. No VS Code, use `make run` e a extensão REST Client. A API sobe sempre em `http://localhost:5000`, e os arquivos de `flows/` funcionam nas três ferramentas.

A partir da Aula 4 a API é só GraphQL. As medições das Aulas 1 a 3 continuam nas branches delas.

## Quantas consultas cada request faz

Em desenvolvimento, o `make run` escreve no terminal cada SQL enviado ao banco (`Aula:MostrarSql`, com o `LogTo` do EF Core) e, no fim do request, a linha `Banco: N consultas neste request`. O mesmo número volta no header `X-Consultas-Banco` da resposta: é o que o k6 e os testes leem. Quem conta é o `ContadorDeConsultasInterceptor`, em `Database/Interceptors/`.

## Como o código se organiza

Cada caso de uso é uma pasta em `Features/<Feature>/<CasoDeUso>/`, com um arquivo por papel:

| Arquivo | Papel |
|---|---|
| `<CasoDeUso>Resolver` | porta de entrada GraphQL: monta a Query ou o Command e chama o `IMediator` |
| `<CasoDeUso>Query` ou `Command` | a mensagem, só com os dados da entrada |
| `<CasoDeUso>Handler` | o caso de uso |

A entidade e o que é da feature inteira ficam na raiz da feature. `Common/` guarda o que vale para todas: `Messaging` (mediator), `Results` (Result e Error) e `Decorators`.

## Docker ou Podman

Os testes de integração sobem o Postgres com Testcontainers e precisam de um Docker rodando. Com o Podman no Windows (máquina WSL), dois ajustes ficam só no seu terminal, sem ir para o repositório:

```
export TESTCONTAINERS_RYUK_DISABLED=true
export TESTCONTAINERS_HOST_OVERRIDE=<ip da máquina WSL do Podman>
```

Se a porta 5433 não responder em `localhost`, aponte a API para o mesmo IP com `export ConnectionStrings__Corretora="Host=<ip>;Port=5433;Database=corretora;Username=corretora;Password=corretora_dev"`. O Redis segue a mesma regra: `export ConnectionStrings__Redis="<ip>:6379"`.

## Cache do saldo

A partir da Aula 5, o saldo fica no HybridCache: L1 na memória da API e L2 no Redis do `make up`. Uma ordem apaga do cache tudo o que tem a etiqueta do usuário (`usuario:<id>`). Sem `ConnectionStrings:Redis`, o cache continua só na memória.
