Aplicação da disciplina GraphQL e Design Patterns (FIAP PosTech).

```
make up         sobe o Postgres no Docker (porta 5433)
make migrar     cria as tabelas e a carga inicial
make run        sobe a API em http://localhost:5000
make build      compila a solução
make test       roda todos os testes (o Postgres dos testes sobe sozinho, com Testcontainers)
make test-unit  roda só os testes de unidade, sem subir a API
make test-arch  roda só os testes de arquitetura (as regras do slice)
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

Antes da primeira execução: Docker rodando, `make up` e `make migrar`.

Abra `Corretora.sln` no Visual Studio 2026 ou no Rider e rode o perfil `http`. No VS Code, use `make run` e a extensão REST Client. A API sobe sempre em `http://localhost:5000`, e os arquivos de `flows/` funcionam nas três ferramentas.

A partir da Aula 4 a API é só GraphQL. As medições das Aulas 1 a 3 continuam nas branches delas.

## Como o código se organiza

Cada caso de uso é uma pasta em `Features/<Feature>/<CasoDeUso>/`, com um arquivo por papel:

| Arquivo | Papel |
|---|---|
| `<CasoDeUso>Resolver` | porta de entrada GraphQL: monta a Query ou o Command e chama o `IMediator` |
| `<CasoDeUso>Query` ou `Command` | a mensagem, só com os dados da entrada |
| `<CasoDeUso>Handler` | o caso de uso |

A entidade e o que é da feature inteira ficam na raiz da feature. `Common/` guarda o que vale para todas: `Messaging` (mediator), `Results` (Result e Error) e `Decorators`.
