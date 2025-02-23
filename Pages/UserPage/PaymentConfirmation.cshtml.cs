using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProjParkNet.Pages.UserPage
{
    public class PaymentConfirmationModel : PageModel
    {
            [BindProperty(SupportsGet = true)]
            public string Matricula { get; set; } 

            [BindProperty(SupportsGet = true)]
            public string TypeVehicle { get; set; } 

            [BindProperty(SupportsGet = true)]
            public DateTime EntryTime { get; set; }

            [BindProperty(SupportsGet = true)]
            public DateTime ExitTime { get; set; }

            [BindProperty(SupportsGet = true)]
            public decimal AmountPaid { get; set; }

            [BindProperty(SupportsGet = true)]
            public string PaymentMethod { get; set; }

            public string Barcode { get; set; } = "|| ||||||| ||| |||||||";

            public IActionResult OnGet()
            {
                // Verifica se os dados obrigatórios estão presentes
                if (string.IsNullOrEmpty(Matricula) || string.IsNullOrEmpty(TypeVehicle) || AmountPaid <= 0)
                {
                    ModelState.AddModelError(string.Empty, "Dados do ticket inválidos.");
                    return Page();
                }

                return Page();
            }
        }
    }
