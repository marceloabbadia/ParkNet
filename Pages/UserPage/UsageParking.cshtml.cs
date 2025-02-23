
using System.Security.Claims;
using ProjParkNet.Data.Entities;

namespace ProjParkNet.Pages.UserPage
{
    [Authorize]
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
        public DateTime? ExitTime { get; set; }

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

                        return RedirectToPage("/UserPage/Ticket", new
                        
                        {
                            userId,
                            Matricula,
                            TypeVehicle,
                            EntryTime,
                            SelectedParkId,
                            SelectedSpotIdent,
                        });
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = ex.Message;
                    }
                    break;

                case "ticket":
                    return RedirectToPage("/UserPage/Ticket");

                default:
                    ErrorMessage = "Ação inválida.";
                    break;
            }

            return Page();
        }


        public async Task<IActionResult> OnPostSaidaAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login");
            }

            try
            {
                // Finaliza o estacionamento chamando o repositório
                var parkingUsageId = await _parkingUsageRepository.EndParkingAsync(userId);
                if (parkingUsageId == null)
                {
                    ErrorMessage = "Não foi possível encontrar um registro ativo de estacionamento.";
                    return Page();
                }

                // Redireciona para a página de pagamento com o ID do estacionamento
                return RedirectToPage("/UserPage/Payment", new { parkingUsageId });
               
            }
            catch (Exception ex)
            {
                // Captura erros e exibe uma mensagem de erro na página
                ErrorMessage = $"Erro ao registrar saída: {ex.Message}";
                return Page();
            }
        }

    }
}