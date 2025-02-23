using ProjParkNet.Models.User;

namespace ProjParkNet.Data.Repositories
{
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
        public async Task AddTransactionAsync(string userId, decimal amount, string type, string description)
        {

            try
            {

                if (type != "Credito" && type != "Debito")
                {
                    throw new ArgumentException("O tipo de transação deve ser 'Credito' ou 'Debito'.");
                }

                // Cria a nova transação
                var transaction = new UserTransaction
                {
                    UserId = userId,
                    Amount = amount,
                    TransactionDate = DateTime.UtcNow,
                    Description = description,
                    Type = type
                };

                // Adiciona a transação ao banco de dados
                _context.UserTransactions.Add(transaction);

                // Atualiza o saldo do usuário
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

                // Atualiza o saldo com base no tipo de transação
                if (type == "Credito")
                {
                    userBalance.CurrentBalance += amount;
                }
                else if (type == "Debito")
                {
                    userBalance.CurrentBalance -= amount;

                    // Verifica se o saldo ficará negativo
                    if (userBalance.CurrentBalance < 0)
                    {
                        throw new InvalidOperationException("Saldo insuficiente para realizar a transação.");
                    }
                }

                // Salva as alterações no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Registra o erro (você pode usar logs aqui)
                Console.WriteLine($"Erro ao adicionar transação: {ex.Message}");
                throw; // Relança a exceção para que o chamador possa tratá-la
            }
        }
    }
}