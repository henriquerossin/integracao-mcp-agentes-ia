using MongoDB.Bson.Serialization.Attributes;
using Tarefas.Domain.Entities;

namespace Tarefas.Infrastructure.Data.Documents;

/// <summary>Formato em que o membro é gravado na coleção "membros" do MongoDB.</summary>
public sealed class MembroDocument
{
    [BsonId] public string Id { get; set; } = default!;
    public string Nome { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Cpf { get; set; } = default!;
    public string SenhaHash { get; set; } = default!;

    public static MembroDocument FromEntity(Membro membro)
        => new()
        {
            Id = membro.Id,
            Nome = membro.Nome,
            Email = membro.Email,
            Cpf = membro.Cpf,
            SenhaHash = membro.SenhaHash
        };

    public Membro ToEntity() => Membro.Restaurar(
        Id,
        Nome,
        Email,
        Cpf,
        SenhaHash
    );
}