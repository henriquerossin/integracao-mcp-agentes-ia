using MediatR;

namespace Tarefas.Domain.Commands;

public sealed record ExcluirTarefaCommand(string TarefaId) : IRequest;