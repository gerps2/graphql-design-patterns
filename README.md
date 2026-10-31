# Corretora

Aplicação da disciplina GraphQL e Design Patterns (FIAP PosTech).

```
make run      sobe a API em http://localhost:5000
make build    compila a solução
make medir    mede a aula atual com o k6 e abre o relatório
make test     roda os testes
make test-handlers  roda só os testes de handler, sem subir a API
make clean    limpa os artefatos
```

| Aula | Tema | Branch |
|---|---|---|
| 1 | Fundamentos de GraphQL em .NET | `feat/aula-01-fundamentos` |
| 2 | Schemas e o padrão Resolver | `feat/aula-02-schemas-resolvers` |
| 3 | Queries e CQRS | `feat/aula-03-queries-cqrs` |

## Abrir na IDE

Abra `Corretora.sln` no Visual Studio 2026 ou no Rider e rode o perfil `http`. No VS Code, use `make run` e a extensão REST Client. A API sobe sempre em `http://localhost:5000`, e os arquivos de `flows/` funcionam nas três ferramentas.

Para repetir a medição de outra aula, use `make medir AULA=02`.
