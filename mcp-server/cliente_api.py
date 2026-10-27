"""Cliente HTTP da API REST de tarefas.

Este é o ÚNICO ponto do servidor MCP que conversa com o sistema. O servidor
nunca acessa o MongoDB: ele só faz requisições HTTP à API, sempre enviando a
chave de acesso no cabeçalho X-Api-Key. O que volta para o agente é exatamente
o que a API devolveu (DTOs em JSON).
"""

import os
from pathlib import Path

import httpx
from dotenv import load_dotenv
from mcp.server.mcpserver.exceptions import ToolError

# O .env fica na raiz do repositório (veja .env.example) e não é versionado.
load_dotenv(Path(__file__).resolve().parent.parent / ".env")

URL_API = os.getenv("TAREFAS_API_URL", "http://localhost:5080")
CHAVE_API = os.getenv("TAREFAS_API_KEY")

if not CHAVE_API:
    raise RuntimeError("Configure TAREFAS_API_KEY no arquivo .env da raiz do repositório.")


async def chamar_api(metodo: str, caminho: str, parametros: dict | None = None, corpo: dict | None = None):
    """Faz uma requisição à API e devolve o JSON da resposta.

    Se a API recusar a operação (401, 404, 422...), lança ToolError com a
    mensagem em português enviada pela API. O SDK do MCP transforma esse erro
    em uma resposta de erro legível para o agente, que pode explicar o motivo
    ao usuário (ex.: "Tarefa concluída não pode ser reaberta").
    """
    # Filtros não informados (None) não são enviados na URL.
    if parametros:
        parametros = {chave: valor for chave, valor in parametros.items() if valor is not None}

    try:
        async with httpx.AsyncClient(base_url=URL_API, headers={"X-Api-Key": CHAVE_API}, timeout=10) as cliente:
            resposta = await cliente.request(metodo, caminho, params=parametros, json=corpo)
    except httpx.HTTPError:
        raise ToolError("A API de tarefas não está respondendo. Verifique se ela está em execução.")

    if resposta.is_success:
        return resposta.json()

    # A API devolve os erros no formato { "erro": "mensagem em português" }.
    try:
        mensagem = resposta.json().get("erro") or resposta.text
    except ValueError:
        mensagem = resposta.text
    raise ToolError(f"A API recusou a operação (HTTP {resposta.status_code}): {mensagem}")
