
namespace ProjParkNet.Pages.ParkingPage;

[Authorize(Roles = "Admin")]
public class DetailsModel : PageModel
{
    private readonly ParkingRepository _parkingRepository;

    public DetailsModel(ParkingRepository parkingRepository)
    {
        _parkingRepository = parkingRepository;
    }

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
}
