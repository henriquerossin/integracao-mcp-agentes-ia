"""Teste do servidor MCP sem o agente de IA.

Este script faz o papel de um cliente MCP: inicia o servidor.py pelo
transporte stdio (como o Claude Desktop fará), lista as ferramentas e faz
chamadas que NÃO alteram dados. Serve para verificar o servidor isoladamente,
antes de conectar o agente.

Uso (com a API rodando):  uv run testar_servidor.py
"""

import asyncio
import json
import sys
from pathlib import Path

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

SERVIDOR = StdioServerParameters(
    command=sys.executable,
    args=[str(Path(__file__).resolve().parent / "servidor.py")],
)


def mostrar(titulo: str, resultado) -> None:
    texto = "\n".join(bloco.text for bloco in resultado.content if hasattr(bloco, "text"))
    situacao = "ERRO" if resultado.is_error else "OK"
    print(f"\n--- {titulo} [{situacao}]\n{texto[:600]}")


async def main() -> None:
    async with stdio_client(SERVIDOR) as (leitura, escrita):
        async with ClientSession(leitura, escrita) as sessao:
            await sessao.initialize()

            ferramentas = await sessao.list_tools()
            print(f"Ferramentas expostas ({len(ferramentas.tools)}):")
            for ferramenta in ferramentas.tools:
                print(f"  - {ferramenta.name}: {ferramenta.description}")

            mostrar("listar_membros", await sessao.call_tool("listar_membros", {}))
            mostrar("listar_tarefas (Concluida)", await sessao.call_tool("listar_tarefas", {"status": "Concluida"}))
            mostrar("obter_tarefa (ID inexistente -> 404)", await sessao.call_tool("obter_tarefa", {"tarefa_id": "0" * 32}))
            mostrar("obter_tarefa (ID manipulado -> recusado)", await sessao.call_tool("obter_tarefa", {"tarefa_id": "../membros"}))
            mostrar("criar_tarefa (prioridade inválida -> recusada)", await sessao.call_tool(
                "criar_tarefa", {"titulo": "Teste", "prioridade": "Urgentissima"}))
            mostrar("excluir_tarefa (ferramenta inexistente)", await sessao.call_tool("excluir_tarefa", {"tarefa_id": "x"}))

            print("\nEsquema de entrada de criar_tarefa (o que o agente recebe):")
            criar = next(f for f in ferramentas.tools if f.name == "criar_tarefa")
            print(json.dumps(criar.input_schema, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    asyncio.run(main())
