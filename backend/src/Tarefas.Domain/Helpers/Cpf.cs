namespace Tarefas.Domain.Helpers;

/// <summary>Validação do CPF pelos dígitos verificadores (módulo 11).</summary>
public static class Cpf
{
    /// <summary>
    /// Devolve o CPF só com os 11 dígitos (aceita "123.456.789-09" ou "12345678909"),
    /// ou null se o número for inválido.
    /// </summary>
    public static string? Normalizar(string? cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            return null;

        var digitos = new string(cpf.Where(char.IsDigit).ToArray());

        if (digitos.Length != 11 || digitos.Distinct().Count() == 1)
            return null;

        return DigitoVerificador(digitos, 9) == digitos[9] - '0' && DigitoVerificador(digitos, 10) == digitos[10] - '0'
            ? digitos
            : null;
    }

    private static int DigitoVerificador(string digitos, int quantidade)
    {
        var soma = 0;
        for (var i = 0; i < quantidade; i++)
            soma += (digitos[i] - '0') * (quantidade + 1 - i);

        var resto = soma * 10 % 11;
        return resto == 10 ? 0 : resto;
    }
}
