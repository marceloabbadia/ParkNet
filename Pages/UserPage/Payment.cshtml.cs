
namespace ProjParkNet.Pages.UserPage;

[Authorize]
public class PaymentModel : PageModel
{

    private readonly ParkingUsageRepository _parkingUsageRepository;
    private readonly TransactionsRepository _transactionsRepository;

    public PaymentModel(ParkingUsageRepository parkingUsageRepository, TransactionsRepository transactionsRepository)
    {
        _parkingUsageRepository = parkingUsageRepository;
        _transactionsRepository = transactionsRepository;
    }

    [BindProperty]
    public string Matricula { get; set; }
    [BindProperty]
    public string TypeVehicle { get; set; }
    [BindProperty]
    public int SelectedParkId { get; set; }
    [BindProperty]
    public string SelectedSpotIdent { get; set; }
    [BindProperty]
    public DateTime EntryTime { get; set; }
    [BindProperty]
    public DateTime? ExitTime { get; set; }
    [BindProperty]
    public decimal AmountToPay { get; set; } // Valor a ser pago
    [BindProperty]
    public string PaymentMethod { get; set; } // Método de pagamento (ex: "CreditCard", "Balance")
    public bool IsCreditCardActive { get; set; } // Indica se o cartão de crédito está ativo

    public async Task<IActionResult> OnGetAsync(int parkingUsageId)
    {
        try
        {
            if (parkingUsageId <= 0)
            {
                ModelState.AddModelError(string.Empty, "ID do registro de estacionamento inválido.");
                return Page();
            }

            var parkingUsage = await _parkingUsageRepository.GetActiveParkingUsageForIdAsync(parkingUsageId);
            if (parkingUsage == null)
            {
                ModelState.AddModelError(string.Empty, "Registro de estacionamento não encontrado.");
                return Page();
            }

            Matricula = parkingUsage.Matricula;
            TypeVehicle = parkingUsage.TypeVehicle;
            SelectedParkId = parkingUsage.ParkingSpot?.ParkingFloor?.ParkingId ?? 0;
            SelectedSpotIdent = parkingUsage.ParkingSpot?.SpotIdent ?? string.Empty;
            EntryTime = parkingUsage.EntryTime;
            ExitTime = parkingUsage.ExitTime;
            AmountToPay = parkingUsage.Price ?? 0;
            IsCreditCardActive = true; // Supondo que o cartão de crédito esteja sempre ativo
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro ao carregar os dados de pagamento: {ex.Message}");
        }
        return Page();

    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtém o ID do usuário logado
        if (string.IsNullOrEmpty(userId))
        {
            ModelState.AddModelError(string.Empty, "Usuário não autenticado.");
            return Page();
        }

        try
        {
            // Recupera o registro de ParkingUsage correspondente
            var parkingUsageId = int.Parse(Request.Query["parkingUsageId"]);
            var parkingUsage = await _parkingUsageRepository.GetActiveParkingUsageForIdAsync(parkingUsageId);
            if (parkingUsage == null)
            {
                ModelState.AddModelError(string.Empty, "Registro de estacionamento não encontrado.");
                return Page();
            }

            // Valida o método de pagamento
            if (string.IsNullOrEmpty(PaymentMethod) || !new[] { "CreditCard", "Balance" }.Contains(PaymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Método de pagamento inválido.");
                return Page();
            }

            // Processa o pagamento com base no método selecionado
            switch (PaymentMethod)
            {
                case "CreditCard":
                    var currentBalance = await _transactionsRepository.GetCurrentBalanceAsync(userId);
                    if (currentBalance < AmountToPay)
                    {
                        ModelState.AddModelError(string.Empty, "Saldo insuficiente para realizar o pagamento.");
                        return Page();
                    }

                    await _transactionsRepository.AddTransactionAsync(
                        userId,
                        AmountToPay, // Valor negativo para débito
                        "Debito",
                        "Pagamento de estacionamento com cartão de crédito"
                    );
                    break;

                //case "Balance":
                //    var currentBalance = await _transactionsRepository.GetCurrentBalanceAsync(userId);
                //    if (currentBalance < AmountToPay)
                //    {
                //        ModelState.AddModelError(string.Empty, "Saldo insuficiente para realizar o pagamento.");
                //        return Page();
                //    }

                //    await _transactionsRepository.AddTransactionAsync(
                //        userId,
                //        AmountToPay, // Valor negativo para débito
                //        "Debito",
                //        "Pagamento de estacionamento com saldo"
                //    );
                //    break;

                default:
                    ModelState.AddModelError(string.Empty, "Método de pagamento inválido.");
                    return Page();
            }

            // Atualiza o status de pagamento na tabela ParkingUsages
            await _parkingUsageRepository.UpdatePaymentStatusAsync(parkingUsage.Id, true);


            // Redireciona para a página de confirmação com os dados necessários
            return RedirectToPage("/UserPage/PaymentConfirmation", new
            {
                Matricula = parkingUsage.Matricula,
                TypeVehicle = parkingUsage.TypeVehicle,
                EntryTime = parkingUsage.EntryTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                ExitTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss"),
                AmountPaid = parkingUsage.Price,
                PaymentMethod = PaymentMethod
            });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Erro ao processar o pagamento: {ex.Message}");
            return Page();
        }
    }
}