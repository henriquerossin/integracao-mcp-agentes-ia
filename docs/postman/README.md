# Testes da API no Postman (validação sem MCP)

Objetivo: provar, de forma técnica, que a API REST funciona **antes** de existir o servidor MCP. É o "jeito antigo" de operar o sistema (Postman, chave de acesso, JSON), que depois será comparado com o uso em linguagem natural pelo agente.

## 1. Preparar o ambiente

Na raiz do repositório:

```bash
docker compose up -d                                        # MongoDB
dotnet run --project backend/src/Tarefas.Api -- --reset     # API com o banco em estado conhecido (4 membros, 12 tarefas)
```

A API sobe em `http://localhost:5080`.

## 2. Importar a coleção

No Postman: **Import** → arquivo `TCC-Tarefas-API.postman_collection.json`.

- A URL e a chave ficam em **variáveis da coleção** (`baseUrl`, `apiKey`).
- A chave vai no cabeçalho `X-Api-Key` pela aba **Authorization** da coleção; as requisições herdam essa configuração.
- Os IDs são gerados de novo a cada seed. Por isso a pasta **0** lê os dados e guarda os IDs em variáveis (`membroJoaoId`, `tarefaConcluidaId` etc.). **Rode a pasta 0 primeiro.**

## 3. Estrutura da coleção

| Pasta | Conteúdo | Resultado esperado |
|---|---|---|
| 0 – Preparação | Captura dos IDs do seed | 200 |
| 1 – Consultas | Listar, filtrar (status, responsável, prioridade), obter por ID, listar membros (sem CPF nem senha) | 200 |
| 2 – Comandos | Criar → iniciar → reatribuir → concluir; cadastrar membro (com CPF e senha, sem devolvê-los); excluir tarefa | 201 / 200 / 204 |
| 3 – Regras de negócio | Reabrir concluída, concluir sem responsável, prazo no passado, título longo ou vazio, e-mail repetido/inválido, IDs inexistentes, prioridade inválida, CPF inválido/repetido, senha curta | 422 / 404 / 400 |
| 4 – Controle de acesso | Sem chave, chave errada, exclusão sem chave | 401 |

Cada requisição tem **testes automáticos** (aba *Tests*/*Scripts*), que conferem o código HTTP, a mensagem de erro e os campos do retorno.

### Sobre os códigos de erro

- **422**: regra de negócio violada no domínio (`DomainException`), com corpo `{ "erro": "mensagem em português" }`.
- **404**: recurso inexistente (`NotFoundException`), com o mesmo formato de corpo.
- **400**: o corpo nem chega ao domínio. Com um valor fora do enum (`"prioridade": "Urgentissima"`), o ASP.NET Core não consegue converter o JSON para o comando e recusa a requisição (`[ApiController]`). O corpo, nesse caso, é o *ProblemDetails* padrão do ASP.NET Core, em inglês, e não o formato `{ "erro": ... }`.
- **401**: chave ausente ou inválida (`ApiKeyMiddleware`).

## 4. Rodar tudo de uma vez

No Postman: botão direito na coleção → **Run collection** → **Run**. O Runner executa as pastas na ordem e mostra quantos testes passaram.

Pela linha de comando, o mesmo resultado sai com o Newman, o executor de coleções do Postman:

```bash
npx newman run docs/postman/TCC-Tarefas-API.postman_collection.json
```

Para repetir do zero, pare a API e rode-a de novo com `-- --reset`: os IDs e os dados mudam a cada carga inicial, e a pasta 0 os captura de novo.

## 5. Execuções registradas

| Data | Ferramenta | Requisições | Testes | Falhas | Evidência |
|---|---|---|---|---|---|
| 04/10/2026 | Postman Runner | 32 | 64 | 0 | `docs/evidencias/postman/11-runner-resumo-*.png` e prints das requisições |
| 19/10/2026 | Newman 6.2.3 | 35 | 72 | 0 | `docs/evidencias/postman/12-newman-2026-10-19.txt` (saída completa) e `.json` (relatório) |

Entre as duas execuções, o membro passou a ter CPF e senha: a coleção ganhou testes que conferem que esses dados não saem da API (1.7 e 2.5) e as requisições 3.13 (CPF inválido), 3.14 (CPF repetido) e 3.15 (senha curta).
