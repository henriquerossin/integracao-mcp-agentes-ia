namespace Tarefas.Domain.Exceptions;

/// <summary>Violação de uma regra de negócio. A API responde com 422.</summary>
public sealed class DomainException(string message) : Exception(message);