using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Exceptions;
using Tarefas.Domain.Interfaces.Repositories;

namespace Tarefas.Domain.Commands.CommandHandlers;

public sealed class MembroCommandHandler(IMembroRepository membroRepository) : IRequestHandler<CadastrarMembroCommand, MembroDto>
{
    public async Task<MembroDto> Handle(CadastrarMembroCommand command, CancellationToken ct)
    {
        var membro = Membro.Criar(command.Nome, command.Email, command.Cpf, command.Senha);

        if (await membroRepository.ExisteEmailAsync(membro.Email, ct))
            throw new DomainException("Já existe um membro cadastrado com este e-mail.");

        if (await membroRepository.ExisteCpfAsync(membro.Cpf, ct))
            throw new DomainException("Já existe um membro cadastrado com este CPF.");

        await membroRepository.SalvarAsync(membro, ct);

        return MembroDto.From(membro);
    }
}