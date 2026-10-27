using MediatR;
using Tarefas.Domain.DTOs;

namespace Tarefas.Domain.Commands;

public sealed record CadastrarMembroCommand(string Nome, string Email, string Cpf, string Senha) : IRequest<MembroDto>;