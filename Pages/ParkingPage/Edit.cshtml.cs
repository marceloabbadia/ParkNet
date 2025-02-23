
namespace ProjParkNet.Pages.ParkingPage;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ParkingRepository _parkingRepository;

    public EditModel(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
    }

    [BindProperty]
    public Parking Parking { get; set; }

    public string Message { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parking = await _parkingRepository.GetParkingByIdAsync(id.Value);

        if (parking == null)
        {
            return NotFound();
        }

        Parking = parking;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _parkingRepository.UpdateParkingAsync(Parking);

            Message = "Park updated successfully";

        }
        catch (Exception ex)
        {
            Message = $"Erro: {ex.Message}";
            return Page();
        }

        return RedirectToPage("/ParkingPage/Index");
    }
}


