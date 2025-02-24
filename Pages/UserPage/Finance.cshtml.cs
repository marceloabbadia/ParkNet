
using ProjParkNet.Data.Repositories;

namespace ProjParkNet.Pages.UserPage;

public class FinanceModel : PageModel
{
    private readonly TransactionsRepository _transactionsRepository;
    private readonly ParkingRepository _parkingRepository;

    public FinanceModel(TransactionsRepository transactionsRepository, ParkingRepository parkingRepository)
    {
        _transactionsRepository = transactionsRepository;
        _parkingRepository = parkingRepository;
    }

    [BindProperty(SupportsGet = true)]
    public int SelectedParkingId { get; set; } // ID do estacionamento selecionado

    public decimal CurrentBalance { get; set; } // Saldo atual do usuário

    public List<UserTransaction> Transactions { get; set; } = new List<UserTransaction>(); // Histórico de transações

    public List<Parking> Parkings { get; set; } = new List<Parking>(); // Lista de estacionamentos disponíveis

    public decimal MonthlySubscriptionPrice { get; set; } // Preço da assinatura mensal
    public string PaymentMethod { get; set; } // Método de pagamento (ex: "CreditCard", "Balance")


    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Obtém o ID do usuário logado
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        // Carrega o saldo atual
        CurrentBalance = await _transactionsRepository.GetCurrentBalanceAsync(userId);

        // Carrega o histórico de transações
        Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId) ?? new List<UserTransaction>();

        // Carrega os estacionamentos disponíveis para o usuário
        Parkings = await _parkingRepository.GetAllParkingsAsync();

        // Se um estacionamento foi selecionado, carrega o preço da assinatura mensal
        if (SelectedParkingId > 0)
        {
            var selectedParking = Parkings.FirstOrDefault(p => p.Id == SelectedParkingId);
            if (selectedParking != null)
            {
                MonthlySubscriptionPrice = selectedParking.MonthlyAgreement;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBuyMonthlySubscriptionAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        // Verifica se um estacionamento foi selecionado
        if (SelectedParkingId <= 0)
        {
            ModelState.AddModelError(string.Empty, "Selecione um estacionamento.");
            return Page();
        }

        // Busca o preço da assinatura mensal do estacionamento selecionado
        var selectedParking = await _parkingRepository.GetParkingByIdAsync(SelectedParkingId);
        if (selectedParking == null || selectedParking.MonthlyAgreement <= 0)
        {
            ModelState.AddModelError(string.Empty, "Preço da assinatura mensal não disponível.");
            return Page();
        }

        var subscriptionPrice = selectedParking.MonthlyAgreement;
        var currentBalance = await _transactionsRepository.GetCurrentBalanceAsync(userId);

        if (currentBalance < subscriptionPrice)
        {
            ModelState.AddModelError(string.Empty, "Saldo insuficiente para comprar esta assinatura.");
            return Page();
        }

        // Deduz o valor da assinatura do saldo
        await _transactionsRepository.AddTransactionAsync(
            userId,
            subscriptionPrice,
            "Debito",
             $"Compra de assinatura mensal para o estacionamento: {PaymentMethod}",
             PaymentMethod
        );

        // Atualiza o saldo e o histórico
        CurrentBalance = await _transactionsRepository.GetCurrentBalanceAsync(userId);
        Transactions = await _transactionsRepository.GetUserTransactionsAsync(userId);

        return RedirectToPage();
    }
}
