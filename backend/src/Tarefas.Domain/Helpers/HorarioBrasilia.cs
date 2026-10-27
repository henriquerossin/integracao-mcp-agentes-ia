namespace Tarefas.Domain.Helpers;

/// <summary>
/// Datas no contexto brasileiro. O banco guarda os instantes em UTC; as regras de negócio
/// e as respostas da API usam o horário de Brasília, que é o do usuário do sistema.
/// </summary>
public static class HorarioBrasilia
{
    public static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>Converte um instante em UTC para o horário de Brasília (ex.: 2026-09-24T23:41:06-03:00).</summary>
    public static DateTimeOffset Converter(DateTime utc)
        => TimeZoneInfo.ConvertTime(new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)), Fuso);

    /// <summary>Data do calendário em Brasília no instante informado (em UTC).</summary>
    public static DateOnly Hoje(DateTime utc) => DateOnly.FromDateTime(Converter(utc).DateTime);
}
