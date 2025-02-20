
using System.Security.Claims;

namespace ProjParkNet.Pages
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
        public string EntryTime { get; set; } = string.Empty;

        [BindProperty]
        public string Matricula { get; set; } = string.Empty;

        [BindProperty]
        public string ExitTime { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Login"); // Redireciona se não estiver autenticado
            }

            // Carregar todos os estacionamentos
            Parks = await _parkingRepository.GetAllParkingsAsync();

            // Buscar estacionamento ativo do usuário
            var parkingUsage = await _parkingUsageRepository.GetActiveParkingForUserAsync(userId);

            if (parkingUsage != null && parkingUsage.EntryTime != null && parkingUsage.ExitTime == null)
            {
                // Buscar o ID do estacionamento diretamente
                SelectedParkId = await _parkingSpotsRepository.GetParkingIdFromSpotAsync(parkingUsage.ParkingSpotId) ?? 0;

                // Buscar os detalhes da vaga
                var parkingSpot = await _parkingSpotsRepository.GetSpotDetailsByIdAsync(parkingUsage.ParkingSpotId);
                if (parkingSpot != null)
                {
                    SelectedSpotIdent = parkingSpot.SpotIdent;
                    TypeVehicle = parkingSpot.TypeVehicle;
                }

                Matricula = parkingUsage.Matricula;
                EntryTime = parkingUsage.EntryTime.ToString("yyyy-MM-dd HH:mm:ss");
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

            // Atualiza a vaga selecionada, se fornecida
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
                    EntryTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    break;

                case "saida":
                    ExitTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    break;

                case "confirmar":
                    try
                    {
                        await _parkingUsageRepository.SaveParkingUsageDataAsync(userId, Matricula, TypeVehicle, SelectedParkId, SelectedSpotIdent, EntryTime, ExitTime);
                        return RedirectToPage("/Confirmation");
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