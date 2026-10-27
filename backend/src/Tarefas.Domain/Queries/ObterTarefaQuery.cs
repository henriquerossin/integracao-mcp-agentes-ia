using MediatR;
using Tarefas.Domain.DTOs;

namespace Tarefas.Domain.Queries;

public sealed record ObterTarefaQuery(string TarefaId) : IRequest<TarefaDto>;