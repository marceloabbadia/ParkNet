
namespace ProjParkNet.Data.Repositories;

public class TransactionsRepository
{
    private readonly ParkingDbContext _context;

    public TransactionsRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetCurrentBalanceAsync(string userId)
    {
        var userBalance = await _context.UserBalances
            .FirstOrDefaultAsync(b => b.UserId == userId);
        return userBalance?.CurrentBalance ?? 0;
    }


    public async Task<string> GetUserIdByNifAsync(string nif)
    {
        if (string.IsNullOrEmpty(nif) || nif.Length != 9)
        {
            throw new ArgumentException("O NIF deve conter exatamente 9 caracteres.", nameof(nif));
        }

        // Consulta o UserSystem pelo NIF
        var userSystem = await _context.UserSystems
            .FirstOrDefaultAsync(us => us.Nif == nif);

        // Retorna o Id (userId) associado ao NIF
        return userSystem?.Id; // Se o NIF não for encontrado, retorna null
    }


    public async Task<decimal> GetCurrentBalanceByNifAsync(string Nif)
    {
        if (string.IsNullOrEmpty(Nif) || Nif.Length != 9)
        {
            throw new ArgumentException("O NIF deve conter exatamente 9 caracteres.", nameof(Nif));
        }

        // Consulta o UserSystem pelo NIF
        var userSystem = await _context.UserSystems
            .FirstOrDefaultAsync(us => us.Nif == Nif);

        if (userSystem == null)
        {
            throw new InvalidOperationException("NIF não encontrado ou inválido.");
        }

        // Consulta o saldo do usuário usando o Id do UserSystem (que é igual ao userId)
        var userBalance = await _context.UserBalances
            .FirstOrDefaultAsync(b => b.UserId == userSystem.Id); // Aqui usa userSystem.Id

        return userBalance?.CurrentBalance ?? 0; // Retorna o saldo ou 0 se não houver registro
    }



    // Obtém o histórico de transações do usuário
    public async Task<List<UserTransaction>> GetUserTransactionsAsync(string userId)
    {
        return await _context.UserTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();
    }

    // Adiciona uma nova transação e atualiza o saldo
    public async Task AddTransactionAsync(string userId, decimal amount, string type, string description, string paymentMethod)
    {
        try
        {
            // Validação dos parâmetros
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("O UserId não pode ser nulo ou vazio.", nameof(userId));
            }
            if (amount <= 0)
            {
                throw new ArgumentException("O valor absoluto da transação deve ser maior que zero.", nameof(amount));
            }
            if (type != "Credito" && type != "Debito")
            {
                throw new ArgumentException("O tipo de transação deve ser 'Credito' ou 'Debito'.", nameof(type));
            }

            // Garante que o valor da transação seja positivo
            decimal transactionAmount = Math.Abs(amount);

            // Inicia uma transação bancária para garantir consistência
            using (var dbTransaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Cria a nova transação
                    var transaction = new UserTransaction
                    {
                        UserId = userId,
                        Amount = transactionAmount,
                        TransactionDate = DateTime.UtcNow,
                        Description = description,
                        Type = type
                    };
                    _context.UserTransactions.Add(transaction);

                    // Atualiza o saldo apenas se o método de pagamento for "Balance"
                    if (paymentMethod == "Balance")
                    {
                        var userBalance = await GetUserBalanceOrCreateAsync(userId);

                        if (type == "Credito")
                        {
                            userBalance.CurrentBalance += transactionAmount;
                        }
                        else if (type == "Debito")
                        {
                            if (userBalance.CurrentBalance < transactionAmount)
                            {
                                throw new InvalidOperationException("Saldo insuficiente para realizar a transação.");
                            }
                            userBalance.CurrentBalance -= transactionAmount;
                        }
                    }

                    // Salva as alterações no banco de dados
                    await _context.SaveChangesAsync();

                    // Confirma a transação bancária
                    await dbTransaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Reverte a transação bancária em caso de erro
                    await dbTransaction.RollbackAsync();

                    // Registra o erro usando um sistema de log
                    LogError($"Erro ao processar transação para o usuário {userId}: {ex.Message}", ex);

                    // Relança a exceção para o chamador
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            // Registra o erro geral
            LogError($"Erro inesperado ao adicionar transação: {ex.Message}", ex);

            // Relança a exceção para o chamador
            throw;
        }
    }

    // Método auxiliar para obter ou criar o registro de saldo do usuário
    private async Task<UserBalance> GetUserBalanceOrCreateAsync(string userId)
    {
        var userBalance = await _context.UserBalances
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (userBalance == null)
        {
            userBalance = new UserBalance
            {
                UserId = userId,
                CurrentBalance = 0
            };
            _context.UserBalances.Add(userBalance);
        }

        return userBalance;
    }

    // Método auxiliar para registrar erros (pode ser substituído por um sistema de log real)
    private void LogError(string message, Exception exception)
    {
        // Use um sistema de log como Serilog, NLog ou outro
        Console.WriteLine($"{DateTime.UtcNow} - ERRO: {message}");
        Console.WriteLine(exception.ToString());
    }
}