
namespace ProjParkNet.Pages
{
    [Authorize ]

    public class ViewModel : PageModel
    {
        
        private readonly ParkingRepository _parkingRepository;

        public ViewModel (ParkingRepository parkingRepository)
        {
            _parkingRepository = parkingRepository;

        }

        public Parking parking { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            parking = await _parkingRepository.GetParkingByIdAsync(id);

            if (parking == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}

