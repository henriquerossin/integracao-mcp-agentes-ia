using Tarefas.Domain.Entities;
using Tarefas.Domain.Enums;
using Tarefas.Domain.Helpers;

namespace Tarefas.Domain.DTOs;

/// <summary>
/// Dados da tarefa devolvidos pela API. Só contém o que interessa ao consumidor,
/// incluindo o nome do responsável para respostas legíveis. O prazo é só a data
/// e os instantes saem no horário de Brasília (ex.: 2026-09-24T23:41:06-03:00).
/// </summary>
public sealed record TarefaDto(
    string Id,
    string Titulo,
    string? Descricao,
    StatusTarefa Status,
    Prioridade Prioridade,
    string? ResponsavelId,
    string? ResponsavelNome,
    DateOnly? Prazo,
    DateTimeOffset CriadaEm,
    DateTimeOffset AtualizadaEm)
{
    public static TarefaDto From(Tarefa t, string? responsavelNome) => new(
        t.Id, t.Titulo, t.Descricao, t.Status, t.Prioridade,
        t.ResponsavelId, responsavelNome, t.Prazo,
        HorarioBrasilia.Converter(t.CriadaEm), HorarioBrasilia.Converter(t.AtualizadaEm));
}
