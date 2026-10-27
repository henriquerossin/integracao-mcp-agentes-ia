using Tarefas.Domain.Entities;

namespace Tarefas.Domain.DTOs;

/// <summary>
/// Dados do membro devolvidos pela API. CPF e hash da senha ficam de fora de propósito:
/// o que não está no DTO não chega ao servidor MCP nem ao agente.
/// </summary>
public sealed record MembroDto(
    string Id, 
    string Nome, 
    string Email)
{
    public static MembroDto From(Membro m) => new(m.Id, m.Nome, m.Email);
}