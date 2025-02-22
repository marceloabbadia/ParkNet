
using System.Security.Claims;

namespace ProjParkNet.Pages.UserPage
{
    public class UserModel : PageModel
    {
        private readonly ParkingUsageRepository _parkingUsageRepository;
        private readonly ParkingRepository _parkingRepository;
        private readonly ParkingSpotsRepository _parkingSpotsRepository;

        public UserModel(ParkingUsageRepository parkingUsageRepository, ParkingRepository parkingRepository, ParkingSpotsRepository parkingSpotsRepository)
        {
            _parkingUsageRepository = parkingUsageRepository;
            _parkingRepository = parkingRepository;
            _parkingSpotsRepository = parkingSpotsRepository;
        }

        public List<Parking> Parks { get; private set; } = new();
        public List<ParkingFloor> Floors { get; private set; } = new();

        [BindProperty]
        public int SelectedParkId { get; set; }

        [BindProperty]
        public string SelectedSpotIdent { get; set; } = string.Empty;

        [BindProperty]
        public string TypeVehicle { get; set; } = "Carro";

        [BindProperty]
        public DateTime EntryTime { get; set; }

        [BindProperty]
        public string Matricula { get; set; } = string.Empty;

        [BindProperty]
        public DateTime ExitTime { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            Parks = await _parkingRepository.GetAllParkingsAsync();

            var parkingUsage = await _parkingUsageRepository.GetActiveParkingForUserAsync(userId);

            if (parkingUsage != null && parkingUsage.EntryTime != null && parkingUsage.ExitTime == null)
            {
                SelectedParkId = await _parkingSpotsRepository.GetParkingIdFromSpotAsync(parkingUsage.ParkingSpotId) ?? 0;

                var parkingSpot = await _parkingSpotsRepository.GetSpotDetailsByIdAsync(parkingUsage.ParkingSpotId);
                if (parkingSpot != null)
                {
                    SelectedSpotIdent = parkingSpot.SpotIdent;
                    TypeVehicle = parkingSpot.TypeVehicle;
                }

                Matricula = parkingUsage.Matricula;
                EntryTime = parkingUsage.EntryTime;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string handler, string selectedSpotIdent)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            Parks = await _parkingRepository.GetAllParkingsAsync();

            if (!string.IsNullOrEmpty(selectedSpotIdent))
            {
                SelectedSpotIdent = selectedSpotIdent;
            }

            switch (handler.ToLowerInvariant())
            {
                case "loadspots":
                    if (SelectedParkId > 0)
                    {
                        Floors = await _parkingSpotsRepository.GetFloorsWithSpotsAsync(SelectedParkId);
                    }
                    break;

                case "entrada":

                    try
                    {
                        EntryTime = DateTime.UtcNow;


                        await _parkingUsageRepository.SaveParkingUsageDataAsync(
                            userId,
                            Matricula,
                            TypeVehicle,
                            SelectedParkId,
                            SelectedSpotIdent,
                            EntryTime);

                        return RedirectToPage("/UserPage/Ticket");
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    break;

                case "saida":
                    try
                    {
                        ExitTime = DateTime.UtcNow;

                        await _parkingUsageRepository.EndParkingAsync(userId, ExitTime);

                        return RedirectToPage("/UserPage/Payment");
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    break;

            }
            return Page();
        }
    }
}