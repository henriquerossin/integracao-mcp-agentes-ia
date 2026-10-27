using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Enums;

namespace Tarefas.Domain.Commands;

public sealed record CriarTarefaCommand(
    string Titulo,
    string? Descricao,
    Prioridade Prioridade,
    DateOnly? Prazo,
    string? ResponsavelId) : IRequest<TarefaDto>;