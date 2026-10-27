using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Enums;

namespace Tarefas.Domain.Queries;

public sealed record ListarTarefasQuery(
    StatusTarefa? Status,
    string? ResponsavelId,
    Prioridade? Prioridade) : IRequest<IReadOnlyList<TarefaDto>>;