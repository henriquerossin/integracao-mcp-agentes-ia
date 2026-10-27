using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Enums;

namespace Tarefas.Infrastructure.Data.Documents;

/// <summary>Formato em que a tarefa é gravada na coleção "tarefas" do MongoDB.</summary>
public sealed class TarefaDocument
{
    [BsonId] public string Id { get; set; } = default!;
    public string Titulo { get; set; } = default!;
    public string? Descricao { get; set; }
    [BsonRepresentation(BsonType.String)] public StatusTarefa Status { get; set; }
    [BsonRepresentation(BsonType.String)] public Prioridade Prioridade { get; set; }
    public string? ResponsavelId { get; set; }
    public DateOnly? Prazo { get; set; }
    public DateTime CriadaEm { get; set; }
    public DateTime AtualizadaEm { get; set; }

    public static TarefaDocument FromEntity(Tarefa tarefa) => new()
    {
        Id = tarefa.Id,
        Titulo = tarefa.Titulo,
        Descricao = tarefa.Descricao,
        Status = tarefa.Status,
        Prioridade = tarefa.Prioridade,
        ResponsavelId = tarefa.ResponsavelId,
        Prazo = tarefa.Prazo,
        CriadaEm = tarefa.CriadaEm,
        AtualizadaEm = tarefa.AtualizadaEm
    };

    public Tarefa ToEntity() => Tarefa.Restaurar(
        Id,
        Titulo,
        Descricao,
        Status,
        Prioridade,
        ResponsavelId,
        Prazo,
        CriadaEm,
        AtualizadaEm
    );
}