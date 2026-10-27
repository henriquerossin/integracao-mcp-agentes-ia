using System.Security.Cryptography;
using MongoDB.Driver;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Enums;
using Tarefas.Domain.Helpers;
using Tarefas.Infrastructure.Data.Context;
using Tarefas.Infrastructure.Data.Documents;

namespace Tarefas.Infrastructure.Data.Seed;

/// <summary>
/// Preenche o banco com dados de exemplo para desenvolvimento, testes e demonstração.
/// Com recriar = true, apaga tudo antes, voltando o sistema a um estado conhecido.
/// </summary>
public sealed class DataSeeder(MongoDbContext context)
{
    public async Task ExecutarAsync(bool recriar, CancellationToken cancellationToken = default)
    {
        if (recriar)
        {
            await context.Database.DropCollectionAsync("tarefas", cancellationToken);

            await context.Database.DropCollectionAsync("membros", cancellationToken);
        }

        if (await context.Membros.CountDocumentsAsync(FilterDefinition<MembroDocument>.Empty, cancellationToken: cancellationToken) > 0)
            return;

        // CPFs fictícios (válidos só pelos dígitos verificadores). As senhas são aleatórias e
        // descartadas logo após gerar o hash: nem o código-fonte nem o banco as conhecem.
        var ana = Membro.Criar("Ana Souza", "ana.souza@equipe.com", "123.456.789-09", SenhaAleatoria());
        var bruno = Membro.Criar("Bruno Lima", "bruno.lima@equipe.com", "987.654.321-00", SenhaAleatoria());
        var carla = Membro.Criar("Carla Mendes", "carla.mendes@equipe.com", "111.444.777-35", SenhaAleatoria());
        var joao = Membro.Criar("João Pereira", "joao.pereira@equipe.com", "246.813.579-28", SenhaAleatoria());

        await context.Membros.InsertManyAsync(
            new[] { ana, bruno, carla, joao }.Select(MembroDocument.FromEntity), cancellationToken: cancellationToken);

        var agora = DateTime.UtcNow;
        var hoje = HorarioBrasilia.Hoje(agora);

        Tarefa T(
            string titulo,
            string descricao,
            StatusTarefa status,
            Prioridade prioridade,
            Membro? responsavel,
            int prazoEmDias
        ) => Tarefa.Restaurar(
            Guid.NewGuid().ToString("N"),
            titulo,
            descricao,
            status,
            prioridade,
            responsavel?.Id,
            hoje.AddDays(prazoEmDias),
            agora.AddDays(-10),
            agora.AddDays(-1)
        );

        var tarefas = new[]
        {
            T("Revisar relatório trimestral de desempenho", "Conferir os indicadores antes do envio à diretoria.", StatusTarefa.AFazer, Prioridade.Alta, joao, 3),
            T("Atualizar documentação da API", "Incluir os novos endpoints e exemplos de uso.", StatusTarefa.EmAndamento, Prioridade.Media, ana, 5),
            T("Corrigir erro no login do portal", "Usuários relatam falha intermitente ao entrar.", StatusTarefa.EmAndamento, Prioridade.Alta, bruno, -1),
            T("Preparar apresentação para o cliente", "Slides com o andamento do projeto e próximos passos.", StatusTarefa.AFazer, Prioridade.Alta, carla, 2),
            T("Organizar reunião de planejamento da sprint", "Definir pauta e convidar a equipe.", StatusTarefa.Concluida, Prioridade.Media, joao, -3),
            T("Configurar backup automático do banco", "Agendar rotina diária e testar a restauração.", StatusTarefa.AFazer, Prioridade.Media, null, 7),
            T("Revisar contratos de fornecedores", "Verificar prazos de renovação e reajustes.", StatusTarefa.AFazer, Prioridade.Baixa, joao, 10),
            T("Testar nova versão do aplicativo", "Executar o roteiro de testes de regressão.", StatusTarefa.EmAndamento, Prioridade.Media, carla, 4),
            T("Enviar relatório de horas do mês", "Consolidar as horas lançadas pela equipe.", StatusTarefa.Concluida, Prioridade.Baixa, ana, -5),
            T("Atualizar planilha de custos do projeto", "Incluir as despesas do último mês.", StatusTarefa.AFazer, Prioridade.Baixa, bruno, -2),
            T("Responder chamados pendentes de suporte", "Priorizar os chamados abertos há mais de dois dias.", StatusTarefa.EmAndamento, Prioridade.Alta, joao, 1),
            T("Definir metas do próximo trimestre", "Levantar propostas com cada área.", StatusTarefa.AFazer, Prioridade.Media, null, 14),
        };

        await context.Tarefas.InsertManyAsync(tarefas.Select(TarefaDocument.FromEntity), cancellationToken: cancellationToken);
    }

    private static string SenhaAleatoria()
        => RandomNumberGenerator.GetString("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%", 16);
}