

namespace ProjParkNet.Pages.ParkingPage.Admin
{
    [Authorize(Roles = "Admin")]
    public class AdminFinanceModel : PageModel
    {
        private readonly TransactionsRepository _transactionsRepository;
        private readonly ParkingDbContext _context;

        public AdminFinanceModel(TransactionsRepository transactionsRepository, ParkingDbContext context)
        {
            _transactionsRepository = transactionsRepository;
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Nif { get; set; }

        public decimal CurrentBalance { get; set; }
        public List<UserTransaction> Transactions { get; set; } = new List<UserTransaction>();

        [BindProperty]
        public decimal AddBalanceAmount { get; set; }

        [BindProperty]
        public decimal DebitBalanceAmount { get; set; } // Agora separado de AddBalanceAmount
        public string PaymentMethod { get; set; } // Método de pagamento (ex: "CreditCard", "Balance")


        public async Task<IActionResult> OnGetAsync()
        {
            // Carrega os dados apenas se o NIF estiver presente
            if (!string.IsNullOrEmpty(Nif))
            {
                if (Nif.Length != 9)
                {
                    ModelState.AddModelError(nameof(Nif), "O NIF deve conter exatamente 9 caracteres.");
                    return Page();
                }

                var userId = await _transactionsRepository.GetUserIdByNifAsync(Nif);
                if (string.IsNullOrEmpty(userId))
                {
                    ModelState.AddModelError(nameof(Nif), "NIF não encontrado ou inválido.");
                    return Page();
                }

                CurrentBalance = await _transactionsRepository.GetCurrentBalanceByNifAsync(Nif);
                Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId);
            }
            else
            {
                // Limpa os dados se o NIF estiver vazio
                CurrentBalance = 0;
                Transactions = new List<UserTransaction>();
            }

            return Page();
        }



        public async Task<IActionResult> OnPostAddBalanceAdminAsync()
        {
            if (string.IsNullOrEmpty(Nif) || Nif.Length != 9)
            {
                ModelState.AddModelError(nameof(Nif), "O NIF deve conter exatamente 9 caracteres.");
                return Page();
            }

            var userId = await _transactionsRepository.GetUserIdByNifAsync(Nif);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(nameof(Nif), "NIF não encontrado ou inválido.");
                return Page();
            }

            if (AddBalanceAmount <= 0)
            {
                ModelState.AddModelError(nameof(AddBalanceAmount), "O valor deve ser maior que zero.");
                return Page();
            }

            await _transactionsRepository.AddTransactionAsync(
               userId,
               AddBalanceAmount,
               "Credito",
               "Crédito adicionado pelo administrador",
                PaymentMethod ="Realizado no Caixa"
        );

            // Atualiza o saldo e as transações após a operação
            CurrentBalance = await _transactionsRepository.GetCurrentBalanceByNifAsync(Nif);
            Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId);

            return RedirectToPage(new { nif = Nif });
        }

        public async Task<IActionResult> OnPostDebitBalanceAdminAsync()
        {
            // Validação do NIF
            if (string.IsNullOrEmpty(Nif) || Nif.Length != 9)
            {
                ModelState.AddModelError(nameof(Nif), "O NIF deve conter exatamente 9 caracteres.");
                return Page();
            }

            var userId = await _transactionsRepository.GetUserIdByNifAsync(Nif);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError(nameof(Nif), "NIF não encontrado ou inválido.");
                return Page();
            }

            // Validação do valor a ser debitado
            if (DebitBalanceAmount <= 0)
            {
                ModelState.AddModelError(nameof(DebitBalanceAmount), "O valor deve ser maior que zero.");
                return Page();
            }

            // Obtém o saldo atual antes de realizar a operação
            CurrentBalance = await _transactionsRepository.GetCurrentBalanceByNifAsync(Nif);

            // Verifica se o saldo é suficiente
            if (DebitBalanceAmount > CurrentBalance)
            {
                ModelState.AddModelError(nameof(DebitBalanceAmount), "Saldo insuficiente.");

                // Atualiza o saldo e o histórico para garantir que a página seja recarregada com os dados corretos
                CurrentBalance = await _transactionsRepository.GetCurrentBalanceByNifAsync(Nif);
                Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId);

                return Page(); // Retorna à página com os dados atuais (saldo e histórico) intactos
            }

            // Cria a transação de débito
            await _transactionsRepository.AddTransactionAsync(
                userId,
                DebitBalanceAmount, // Valor negativo para débito
                "Debito",
                "Débito realizado pelo administrador",
                PaymentMethod = "Realizado no Caixa"
            );

            // Atualiza o saldo e as transações após a operação
            CurrentBalance = await _transactionsRepository.GetCurrentBalanceByNifAsync(Nif);
            Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId);

            return RedirectToPage(new { nif = Nif });
        }

        public IActionResult OnPostBackAsync()
        {
            return Redirect("/ParkingPage");
        }
    }
}
