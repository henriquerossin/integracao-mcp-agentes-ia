using Tarefas.Domain.Enums;
using Tarefas.Domain.Exceptions;
using Tarefas.Domain.Helpers;

namespace Tarefas.Domain.Entities;

/// <summary>
/// Entidade principal. Todas as regras de negócio sobre tarefas ficam aqui,
/// de modo que nenhuma camada externa (nem o agente de IA) consiga
/// colocar uma tarefa em um estado inválido.
/// </summary>
public sealed class Tarefa
{
    public const int TamanhoMaximoTitulo = 120;

    public string Id { get; private set; }
    public string Titulo { get; private set; }
    public string? Descricao { get; private set; }
    public StatusTarefa Status { get; private set; }
    public Prioridade Prioridade { get; private set; }
    public string? ResponsavelId { get; private set; }
    public DateOnly? Prazo { get; private set; }
    public DateTime CriadaEm { get; private set; }
    public DateTime AtualizadaEm { get; private set; }

    private Tarefa(
        string id,
        string titulo,
        string? descricao,
        StatusTarefa status,
        Prioridade prioridade,
        string? responsavelId,
        DateOnly? prazo,
        DateTime criadaEm,
        DateTime atualizadaEm)
    {
        Id = id;
        Titulo = titulo;
        Descricao = descricao;
        Status = status;
        Prioridade = prioridade;
        ResponsavelId = responsavelId;
        Prazo = prazo;
        CriadaEm = criadaEm;
        AtualizadaEm = atualizadaEm;
    }

    public static Tarefa Criar(
        string titulo,
        string? descricao,
        Prioridade prioridade,
        DateOnly? prazo,
        string? responsavelId,
        DateTime agora)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new DomainException("O título da tarefa é obrigatório.");

        titulo = titulo.Trim();
        if (titulo.Length > TamanhoMaximoTitulo)
            throw new DomainException($"O título deve ter no máximo {TamanhoMaximoTitulo} caracteres.");

        // "Hoje" é o dia no calendário de Brasília, e não em UTC (que já virou o dia a partir das 21h).
        if (prazo is not null && prazo.Value < HorarioBrasilia.Hoje(agora))
            throw new DomainException("O prazo da tarefa não pode estar no passado.");

        return new Tarefa(
            Guid.NewGuid().ToString("N"),
            titulo,
            string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
            StatusTarefa.AFazer,
            prioridade,
            string.IsNullOrWhiteSpace(responsavelId) ? null : responsavelId,
            prazo,
            agora,
            agora
        );
    }

    public void AlterarStatus(StatusTarefa novoStatus, DateTime agora)
    {
        if (Status == StatusTarefa.Concluida)
            throw new DomainException("Tarefas concluídas não podem ser reabertas nem alteradas.");
        if (novoStatus == Status)
            throw new DomainException($"A tarefa já está com o status {novoStatus}.");
        if (novoStatus == StatusTarefa.Concluida && ResponsavelId is null)
            throw new DomainException("Uma tarefa só pode ser concluída se tiver um responsável.");

        Status = novoStatus;
        AtualizadaEm = agora;
    }

    public void AtribuirResponsavel(string membroId, DateTime agora)
    {
        if (Status == StatusTarefa.Concluida)
            throw new DomainException("Não é possível alterar o responsável de uma tarefa concluída.");
        if (string.IsNullOrWhiteSpace(membroId))
            throw new DomainException("Informe o membro responsável.");

        ResponsavelId = membroId;
        AtualizadaEm = agora;
    }

    /// <summary>Reconstrói uma tarefa que já existe no banco.</summary>
    public static Tarefa Restaurar(
        string id,
        string titulo,
        string? descricao,
        StatusTarefa status,
        Prioridade prioridade,
        string? responsavelId,
        DateOnly? prazo,
        DateTime criadaEm,
        DateTime atualizadaEm
    ) => new(
        id,
        titulo,
        descricao,
        status,
        prioridade,
        responsavelId,
        prazo,
        criadaEm,
        atualizadaEm
    );
}