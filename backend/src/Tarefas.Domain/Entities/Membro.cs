using Tarefas.Domain.Exceptions;

namespace Tarefas.Domain.Entities;

/// <summary>
/// Membro da equipe. CPF e senha são dados sensíveis: ficam só no banco
/// (a senha apenas como hash) e nunca saem da API (ver MembroDto).
/// </summary>
public sealed class Membro
{
    public const int TamanhoMinimoSenha = 8;

    public string Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Cpf { get; private set; }
    public string SenhaHash { get; private set; }

    private Membro(
        string id, 
        string nome, 
        string email,
        string cpf,
        string senhaHash)
    {
        Id = id;
        Nome = nome;
        Email = email;
        Cpf = cpf;
        SenhaHash = senhaHash;
    }

    public static Membro Criar(string nome, string email, string cpf, string senha)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do membro é obrigatório.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new DomainException("Informe um e-mail válido.");

        var cpfNormalizado = Helpers.Cpf.Normalizar(cpf)
            ?? throw new DomainException("Informe um CPF válido.");

        if (string.IsNullOrEmpty(senha) || senha.Length < TamanhoMinimoSenha)
            throw new DomainException($"A senha deve ter no mínimo {TamanhoMinimoSenha} caracteres.");

        return new Membro(
            Guid.NewGuid().ToString("N"),
            nome.Trim(),
            email.Trim().ToLowerInvariant(),
            cpfNormalizado,
            Helpers.SenhaHash.Gerar(senha)
        );
    }

    /// <summary>Reconstrói um membro que já existe no banco.</summary>
    public static Membro Restaurar(
        string id, 
        string nome, 
        string email,
        string cpf,
        string senhaHash
    ) => new(id, nome, email, cpf, senhaHash);
}
