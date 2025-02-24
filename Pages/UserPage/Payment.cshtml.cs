
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
    public string ErrorMessage { get; set; } = string.Empty;

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

            // Preenche os campos da página com os dados do estacionamento
            Matricula = parkingUsage.Matricula;
            TypeVehicle = parkingUsage.TypeVehicle;
            SelectedParkId = parkingUsage.ParkingSpot?.ParkingFloor?.ParkingId ?? 0;
            SelectedSpotIdent = parkingUsage.ParkingSpot?.SpotIdent ?? string.Empty;
            EntryTime = parkingUsage.EntryTime;
            ExitTime = parkingUsage.ExitTime;
            AmountToPay = parkingUsage.Price ?? 0;

            // Suponha que o cartão de crédito esteja sempre ativo
            IsCreditCardActive = true;
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
            // Finaliza o estacionamento e obtém o ID do registro de ParkingUsage
            var parkingUsageId = await _parkingUsageRepository.EndParkingAsync(userId);
            if (parkingUsageId == null)
            {
                ErrorMessage = "Não foi possível encontrar um registro ativo de estacionamento.";
                return Page();
            }

            // Recupera os detalhes do registro de estacionamento finalizado
            var parkingUsage = await _parkingUsageRepository.GetActiveParkingUsageForIdAsync((int)parkingUsageId);
            if (parkingUsage == null)
            {
                ModelState.AddModelError(string.Empty, "Registro de estacionamento não encontrado após finalização.");
                return Page();
            }

            // Define o valor a ser pago
            AmountToPay = parkingUsage.Price ?? 0;

            // Valida o método de pagamento
            if (string.IsNullOrEmpty(PaymentMethod) || !new[] { "CreditCard", "Balance" }.Contains(PaymentMethod))
            {
                ModelState.AddModelError(string.Empty, "Método de pagamento inválido.");
                return Page();
            }

            // Processa o pagamento com base no método selecionado
            await _transactionsRepository.AddTransactionAsync(
                userId,
                AmountToPay,
                "Debito",
                $"Pagamento de estacionamento com {PaymentMethod}",
                PaymentMethod // Passa o método de pagamento para o repositório
            );

            // Atualiza o status de pagamento na tabela ParkingUsages
            await _parkingUsageRepository.UpdatePaymentStatusAsync(parkingUsage.Id, true);

            // Redireciona para a página de confirmação com os dados necessários
            return RedirectToPage("/UserPage/PaymentConfirmation", new
            {
                Matricula = parkingUsage.Matricula,
                TypeVehicle = parkingUsage.TypeVehicle,
                EntryTime = parkingUsage.EntryTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                ExitTime = parkingUsage.ExitTime?.ToString("yyyy-MM-ddTHH:mm:ss"),
                AmountPaid = AmountToPay,
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