using MediatR;
using Tarefas.Domain.DTOs;

namespace Tarefas.Domain.Commands;

public sealed record AtribuirResponsavelCommand(string TarefaId, string MembroId) : IRequest<TarefaDto>;