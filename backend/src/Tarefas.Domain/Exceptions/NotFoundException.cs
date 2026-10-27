namespace Tarefas.Domain.Exceptions;

/// <summary>Recurso não encontrado. A API responde com 404.</summary>
public sealed class NotFoundException(string message) : Exception(message);