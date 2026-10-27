using MediatR;
using Tarefas.Domain.DTOs;

namespace Tarefas.Domain.Queries;

public sealed record ListarMembrosQuery() : IRequest<IReadOnlyList<MembroDto>>;