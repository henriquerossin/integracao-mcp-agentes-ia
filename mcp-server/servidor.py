"""Servidor MCP do sistema de tarefas (transporte stdio).

Fluxo: Agente de IA -> ESTE SERVIDOR -> API REST (ASP.NET Core) -> MongoDB.

Decisões de segurança visíveis neste arquivo:
- Menor privilégio: só 6 ferramentas são expostas. Excluir tarefa e cadastrar
  membro existem na API, mas NÃO têm ferramenta aqui, e não há nenhuma
  ferramenta genérica do tipo "executar consulta".
- Parâmetros tipados: status e prioridade só aceitam os valores listados, o
  prazo precisa ser uma data válida e os IDs precisam ter o formato gerado pelo
  backend (32 caracteres hexadecimais). O SDK recusa qualquer outro valor antes
  de chamar a API. A validação dos IDs impede que um valor como "../membros"
  mude o endereço chamado na API (mediação completa).
- Descrições fixas: o texto que o agente lê sobre cada ferramenta está escrito
  aqui no código e versionado no Git (mitigação de tool poisoning).
- Auditoria: toda chamada passa por call_tool (abaixo) e é registrada.

Importante: no transporte stdio a saída padrão é o canal do protocolo. Por isso
este código não usa print(); os registros vão para o arquivo de auditoria.
"""

import json
from datetime import date
from typing import Annotated, Any, Literal

from mcp.server.mcpserver import MCPServer
from pydantic import Field, StringConstraints

import auditoria
from cliente_api import chamar_api

Status = Literal["AFazer", "EmAndamento", "Concluida"]
Prioridade = Literal["Baixa", "Media", "Alta"]

# IDs gerados pelo backend: Guid sem hífens (32 caracteres hexadecimais).
Id = Annotated[str, StringConstraints(pattern=r"^[0-9a-f]{32}$")]


class ServidorAuditado(MCPServer):
    """MCPServer com registro de auditoria em todas as chamadas de ferramentas.

    O SDK encaminha toda chamada vinda do agente para o método call_tool. Ao
    sobrescrevê-lo, a auditoria cobre também as chamadas recusadas pela
    validação de parâmetros e as de ferramentas inexistentes.
    """

    async def call_tool(self, name: str, arguments: dict[str, Any], context=None):
        try:
            resultado = await super().call_tool(name, arguments, context)
        except Exception as erro:
            auditoria.registrar(name, arguments, "erro", str(erro))
            raise
        auditoria.registrar(name, arguments, "sucesso", _resumir(resultado))
        return resultado


def _resumir(resultado) -> str:
    """Resumo curto do retorno para o log (a resposta completa vai para o agente).

    Listas chegam em structured_content; já as ferramentas que devolvem uma única
    tarefa (dict) chegam só como texto JSON, por isso o texto também é lido.
    """
    estruturado = getattr(resultado, "structured_content", None)
    if estruturado:
        dados = estruturado.get("result", estruturado)
    else:
        try:
            dados = json.loads(resultado.content[0].text)
        except (AttributeError, IndexError, TypeError, ValueError):
            dados = {}
    if isinstance(dados, list):
        return f"{len(dados)} registro(s) retornado(s)"
    if isinstance(dados, dict) and "id" in dados:
        return f"tarefa {dados['id']} com status {dados.get('status')}"
    return "concluído"


mcp = ServidorAuditado("tarefas")


# ===================== CONSULTAS (somente leitura) =====================

@mcp.tool()
async def listar_tarefas(
    status: Annotated[Status | None, Field(description="Filtra pelo status: AFazer, EmAndamento ou Concluida.")] = None,
    responsavel_id: Annotated[Id | None, Field(description="Filtra pelo ID do membro responsável (obtido em listar_membros).")] = None,
    prioridade: Annotated[Prioridade | None, Field(description="Filtra pela prioridade: Baixa, Media ou Alta.")] = None,
) -> list[dict]:
    """Lista as tarefas da equipe. Todos os filtros são opcionais e podem ser combinados."""
    return await chamar_api(
        "GET", "/api/tarefas",
        parametros={"status": status, "responsavelId": responsavel_id, "prioridade": prioridade},
    )


@mcp.tool()
async def obter_tarefa(
    tarefa_id: Annotated[Id, Field(description="ID da tarefa.")],
) -> dict:
    """Obtém os detalhes de uma tarefa pelo seu ID."""
    return await chamar_api("GET", f"/api/tarefas/{tarefa_id}")


@mcp.tool()
async def listar_membros() -> list[dict]:
    """Lista os membros da equipe, com ID, nome e e-mail. Use para descobrir o ID de uma pessoa pelo nome."""
    return await chamar_api("GET", "/api/membros")


# ===================== COMANDOS (alteram dados) =====================

@mcp.tool()
async def criar_tarefa(
    titulo: Annotated[str, Field(description="Título da tarefa (obrigatório, até 120 caracteres).")],
    prioridade: Annotated[Prioridade, Field(description="Prioridade: Baixa, Media ou Alta.")],
    descricao: Annotated[str | None, Field(description="Descrição da tarefa.")] = None,
    prazo: Annotated[date | None, Field(description="Prazo no formato AAAA-MM-DD. Não pode estar no passado.")] = None,
    responsavel_id: Annotated[Id | None, Field(description="ID do membro responsável (obtido em listar_membros).")] = None,
) -> dict:
    """Cria uma nova tarefa com status AFazer."""
    return await chamar_api(
        "POST", "/api/tarefas",
        corpo={
            "titulo": titulo,
            "descricao": descricao,
            "prioridade": prioridade,
            "prazo": prazo.isoformat() if prazo else None,
            "responsavelId": responsavel_id,
        },
    )


@mcp.tool()
async def atualizar_status_tarefa(
    tarefa_id: Annotated[Id, Field(description="ID da tarefa.")],
    status: Annotated[Status, Field(description="Novo status: AFazer, EmAndamento ou Concluida.")],
) -> dict:
    """Altera o status de uma tarefa. Uma tarefa concluída não pode ser reaberta nem alterada."""
    return await chamar_api("PATCH", f"/api/tarefas/{tarefa_id}/status", corpo={"status": status})


@mcp.tool()
async def atribuir_responsavel(
    tarefa_id: Annotated[Id, Field(description="ID da tarefa.")],
    membro_id: Annotated[Id, Field(description="ID do membro que será o responsável (obtido em listar_membros).")],
) -> dict:
    """Define o membro responsável por uma tarefa."""
    return await chamar_api("PATCH", f"/api/tarefas/{tarefa_id}/responsavel", corpo={"membroId": membro_id})


if __name__ == "__main__":
    mcp.run()  # transporte padrão: stdio
