# Backend – API de gestão de tarefas

ASP.NET Core (.NET 10) com Controllers, camadas separadas, Commands/Queries (CQRS) e MongoDB.

## Estrutura da solução

| Pasta da solução | Projeto | Responsabilidade |
|---|---|---|
| 1 - Presentation | `Tarefas.Api` | Controllers, middlewares (erros e chave de acesso), configuração e registro centralizado da injeção de dependência (`Infrastructure/IoC/Bootstrapper`) |
| 2 - Domain | `Tarefas.Domain` | Entidades e regras de negócio, Commands, Queries e Handlers (MediatR), DTOs e interfaces |
| 3 - Infrastructure | `Tarefas.Infrastructure.Data` | Contexto e repositórios do MongoDB, dados de exemplo |

## Como rodar

1. Na raiz do repositório, suba o MongoDB: `docker compose up -d`.
2. Rode a API: `dotnet run --project backend/src/Tarefas.Api` (ou abra `Tarefas.slnx` no Visual Studio e execute o projeto `Tarefas.Api`, perfil `http`).
3. A API sobe em `http://localhost:5080` e, em ambiente de desenvolvimento, popula o banco com dados de exemplo na primeira execução.
4. Para voltar o banco ao estado inicial: `dotnet run --project backend/src/Tarefas.Api -- --reset`.

Chave de acesso em desenvolvimento: `dev-chave-local-tcc` (cabeçalho `X-Api-Key`).
Documento OpenAPI: `http://localhost:5080/openapi/v1.json`.
O arquivo `src/Tarefas.Api/Tarefas.Api.http` traz exemplos de requisições.

## Membros: CPF e senha

O cadastro de membro (`POST /api/membros`) exige nome, e-mail, CPF e senha:

- o CPF precisa ser válido pelos dígitos verificadores e não pode se repetir;
- a senha precisa ter pelo menos 8 caracteres e **nunca é gravada**: o banco guarda só o hash PBKDF2-SHA256 com *salt* aleatório;
- nenhuma resposta da API devolve CPF ou senha (`MembroDto` = id, nome e e-mail), portanto o servidor MCP e o agente não têm acesso a esses dados.

## Datas

O banco guarda instantes em UTC; a API trabalha no horário de Brasília. O `prazo` é só a data (`"2026-10-01"`) e `criadaEm`/`atualizadaEm` saem com fuso (`"2026-09-24T23:41:06-03:00"`).
