using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjParkNet.Pages.UserPage
{
    public class TicketModel : PageModel
    {
        public string Matricula { get; set; }
        public string TypeVehicle { get; set; }
        public int SelectedParkId { get; set; }
        public string SelectedSpotIdent { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime ExitTime { get; set; }

        public void OnGet(
            string matricula,
            string typeVehicle,
            int selectedParkId,
            string selectedSpotIdent,
            DateTime entryTime,
            DateTime exitTime)
        {
            Matricula = matricula;
            TypeVehicle = typeVehicle;
            SelectedParkId = selectedParkId;
            SelectedSpotIdent = selectedSpotIdent;
            EntryTime = entryTime;
            ExitTime = exitTime;
        }
    }
}
