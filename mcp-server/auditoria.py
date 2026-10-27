"""Registro de auditoria das chamadas de ferramentas.

Cada chamada feita pelo agente vira uma linha no arquivo logs/auditoria.jsonl
(formato JSON Lines: um objeto JSON por linha). Assim é possível reconstruir
depois qual ferramenta foi chamada, quando, com quais parâmetros e qual foi o
resultado, inclusive as chamadas recusadas.
"""

import json
from datetime import datetime
from pathlib import Path
from zoneinfo import ZoneInfo

ARQUIVO_AUDITORIA = Path(__file__).resolve().parent / "logs" / "auditoria.jsonl"
FUSO_BRASILIA = ZoneInfo("America/Sao_Paulo")


def registrar(ferramenta: str, parametros: dict, status: str, detalhe: str) -> None:
    """Acrescenta uma linha ao arquivo de auditoria (nunca apaga o que já existe)."""
    registro = {
        "data_hora": datetime.now(FUSO_BRASILIA).isoformat(timespec="seconds"),
        "ferramenta": ferramenta,
        "parametros": parametros,
        "status": status,  # "sucesso" ou "erro"
        "detalhe": detalhe,
    }

    ARQUIVO_AUDITORIA.parent.mkdir(exist_ok=True)
    with ARQUIVO_AUDITORIA.open("a", encoding="utf-8") as arquivo:
        arquivo.write(json.dumps(registro, ensure_ascii=False) + "\n")
