using Tarefas.Domain.Enums;

namespace Tarefas.Api.Models;

public sealed record AlterarStatusRequest(StatusTarefa Status);