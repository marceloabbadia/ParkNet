
namespace ProjParkNet.Pages.ParkingPage;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ParkingRepository _parkingRepository;

    public CreateModel(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
    }

    public List<Parking> Parkings { get; set; } = new List<Parking>();

    public async Task OnGetAsync()
    {
        Parkings = await _parkingRepository.GetAllParkingsAsync();
    }

    [BindProperty]
    public IFormFile File { get; set; }

    [BindProperty]
    public string NamePark { get; set; }

    [BindProperty]
    public string Address { get; set; }

    [BindProperty]
    public string District { get; set; }

    [BindProperty]
    public string ZipCode { get; set; }

    [BindProperty]
    public string TelephoneNumber { get; set; }
    public string Message { get; set; }

    [BindProperty]
    public decimal MonthlyAgreement { get; set; }

    [BindProperty]
    public decimal PricePerMinute { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (File == null || File.Length == 0)
        {
            Message = "Tipo de arquivo inválido!";
            return Page();
        }

        string fileValidationMessage;
        if (!_parkingRepository.IsFileValid(File, out fileValidationMessage))
        {
            Message = fileValidationMessage;
            return Page();
        }

        try
        {
            var filePath = await _parkingRepository.SaveFileAsync(File);

            if (!string.IsNullOrEmpty(filePath))
            {
                var parking = _parkingRepository.ProcessParkingFile(filePath);
                parking.NamePark = NamePark;
                parking.Address = Address;
                parking.District = District;
                parking.ZipCode = ZipCode;
                parking.TelephoneNumber = TelephoneNumber;
                parking.PricePerMinute = PricePerMinute;
                parking.MonthlyAgreement = MonthlyAgreement;


                await _parkingRepository.SaveParkingAsync(parking);

                Message = "Parque cadastrado com sucesso!";

                return RedirectToPage("/ParkingPage/Index");

            }
        }
        catch (Exception ex)
        {
            Message = $"Erro ao cadastrar o novo Parque: {ex.Message}";
        }

        return RedirectToPage();
    }
}

