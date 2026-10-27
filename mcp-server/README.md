# Servidor MCP (Python)

Intermediário entre o agente de IA e a API REST:

**Agente de IA (Claude Desktop) → Servidor MCP (este projeto) → API REST → Backend → MongoDB**

O servidor não acessa o banco de dados. Ele só faz requisições HTTP à API, com a chave de acesso, e devolve ao agente o que a API retornou.

## Arquivos

| Arquivo | Responsabilidade |
|---|---|
| `servidor.py` | Define as 6 ferramentas (nome, descrição, parâmetros tipados) e a auditoria de cada chamada |
| `cliente_api.py` | Único ponto que chama a API (HTTP + `X-Api-Key`), com os erros traduzidos para o agente |
| `auditoria.py` | Grava cada chamada em `logs/auditoria.jsonl` (um JSON por linha) |
| `testar_servidor.py` | Cliente MCP de teste, sem o agente: lista as ferramentas e faz chamadas que não alteram dados |
| `pyproject.toml` / `uv.lock` | Dependências e versões exatas (reprodutibilidade) |

SDK: pacote oficial `mcp` (versão 2.x, classe `MCPServer`), transporte **stdio**.

## Ferramentas expostas

| Ferramenta | Endpoint |
|---|---|
| `listar_tarefas` | `GET /api/tarefas` (filtros opcionais: status, responsável, prioridade) |
| `obter_tarefa` | `GET /api/tarefas/{id}` |
| `listar_membros` | `GET /api/membros` |
| `criar_tarefa` | `POST /api/tarefas` |
| `atualizar_status_tarefa` | `PATCH /api/tarefas/{id}/status` |
| `atribuir_responsavel` | `PATCH /api/tarefas/{id}/responsavel` |

**Não expostas de propósito** (menor privilégio): `DELETE /api/tarefas/{id}` e `POST /api/membros`. Não há ferramenta genérica de consulta.

## Como os critérios de segurança aparecem no código

- **(i) Descrições fixas:** o texto de cada ferramenta está no código (`servidor.py`) e versionado no Git.
- **(ii) Menor privilégio:** só as 6 ferramentas acima existem. Uma chamada a qualquer outra é recusada pelo SDK (`Unknown tool`).
- **Validação:** `status` e `prioridade` só aceitam os valores do enum, `prazo` precisa ser uma data válida e os IDs precisam ter o formato gerado pelo backend (32 caracteres hexadecimais). Um valor inválido é recusado antes de chegar à API. A validação dos IDs impede que um valor como `../membros` mude o endereço chamado na API.
- **Regras de negócio:** continuam no domínio (backend). Quando a API recusa (422, 404), a mensagem em português chega ao agente.
- **(iii) Auditoria:** a classe `ServidorAuditado` sobrescreve `call_tool`, por onde passa toda chamada do agente. Por isso até as chamadas recusadas ficam no log.

Exemplo de linha do log:

```json
{"data_hora": "2026-09-30T13:32:03-03:00", "ferramenta": "excluir_tarefa", "parametros": {"tarefa_id": "x"}, "status": "erro", "detalhe": "Unknown tool: excluir_tarefa"}
```

## Configuração

O servidor lê a URL e a chave da API do arquivo `.env` na **raiz do repositório** (não versionado). Para criá-lo, copie o modelo:

```bash
cp .env.example .env
```

```
TAREFAS_API_URL=http://localhost:5080
TAREFAS_API_KEY=dev-chave-local-tcc
```

## Testar sem o agente

Com o MongoDB e a API rodando (veja o README da raiz):

```bash
cd mcp-server
uv run testar_servidor.py
```

## Conectar ao Claude Desktop

No Claude Desktop: **Configurações → Desenvolvedor → Editar configuração**. Acrescente o servidor ao arquivo `claude_desktop_config.json`, trocando o caminho pelo local onde o repositório foi clonado:

```json
{
  "mcpServers": {
    "tarefas": {
      "command": "uv",
      "args": ["--directory", "C:\\caminho\\para\\integracao-mcp-agentes-ia\\mcp-server", "run", "servidor.py"]
    }
  }
}
```

No macOS ou no Linux, o caminho fica como `/caminho/para/integracao-mcp-agentes-ia/mcp-server`. Se o Claude Desktop não encontrar o `uv`, use o caminho completo do executável (no Windows, normalmente `C:\Users\<usuário>\.local\bin\uv.exe`; dentro do JSON, cada `\` é escrita como `\\`).

Depois, feche o Claude Desktop por completo (inclusive o ícone da bandeja) e abra de novo. O próprio Claude Desktop inicia o servidor; não é preciso rodar o `servidor.py` à mão. As chamadas passam a ser registradas em `mcp-server/logs/auditoria.jsonl`.
