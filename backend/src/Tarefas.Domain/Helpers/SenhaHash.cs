using System.Security.Cryptography;

namespace Tarefas.Domain.Helpers;

/// <summary>
/// A senha nunca é gravada: guarda-se apenas um hash PBKDF2 (HMAC-SHA256) com salt aleatório.
/// Formato: "pbkdf2-sha256$iterações$salt$hash" (salt e hash em Base64).
/// </summary>
public static class SenhaHash
{
    private const int Iteracoes = 600_000;
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;

    public static string Gerar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return $"pbkdf2-sha256${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }
}
