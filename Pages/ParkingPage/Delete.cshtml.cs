
namespace ProjParkNet.Pages.ParkingPage;

public class DeleteModel : PageModel
{
    private readonly ParkingRepository _parkingRepository;

    public DeleteModel(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
    }

    [BindProperty]
    public Parking Parking { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var parking = await _parkingRepository.GetParkingByIdAsync(id.Value);

        if (parking is not null)
        {
            Parking = parking;

            return Page();
        }

        return NotFound();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        await _parkingRepository.DeleteParkingAsync(id.Value);

        return RedirectToPage("/ParkingPage/Index");
    }
}
