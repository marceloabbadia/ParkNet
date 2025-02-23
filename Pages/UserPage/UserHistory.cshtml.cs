using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjParkNet.Pages.UserPage
{
    [Authorize]
    public class UserHistoryModel : PageModel
    {
        private readonly ParkingUsageRepository _parkingUsageRepository;

        public UserHistoryModel(ParkingUsageRepository parkingUsageRepository)
        {
            _parkingUsageRepository = parkingUsageRepository;
        }


        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public List<ParkingUsage> ParkingHistory { get; set; } = new List<ParkingUsage>();

        public async Task<IActionResult> OnGetAsync()
        {
            // Verifica se o usuário está logado
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                // Redireciona para a página de login se o usuário não estiver logado
                return RedirectToPage("/Login");
            }

            ParkingHistory = await _parkingUsageRepository.GetUserParkingHistoryAsync(userId, StartDate, EndDate);

            return Page();
        }
    }
}