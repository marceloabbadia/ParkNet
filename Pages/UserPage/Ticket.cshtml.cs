namespace ProjParkNet.Pages.UserPage;

public class TicketModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string Matricula { get; set; }

    [BindProperty(SupportsGet = true)]
    public string TypeVehicle { get; set; }

    [BindProperty(SupportsGet = true)]
    public int SelectedParkId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SelectedSpotIdent { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime EntryTime { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime ExitTime { get; set; }

    public string Barcode { get; set; } = "|| ||||||| ||| |||||||";

    public IActionResult OnGet()
    {
        // Validações básicas para garantir que os dados foram fornecidos
        if (string.IsNullOrEmpty(Matricula) || string.IsNullOrEmpty(TypeVehicle) || SelectedParkId <= 0 || string.IsNullOrEmpty(SelectedSpotIdent) || EntryTime == default || ExitTime == default)
        {
            ModelState.AddModelError(string.Empty, "Dados do ticket inválidos.");
            return Page();
        }

        return Page();
    }
}
