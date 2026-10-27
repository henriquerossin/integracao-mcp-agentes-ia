using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Enums;

namespace Tarefas.Domain.Commands;

public sealed record AlterarStatusTarefaCommand(string TarefaId, StatusTarefa NovoStatus) : IRequest<TarefaDto>;