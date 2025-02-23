
namespace ProjParkNet.Pages.UserPage;

[Authorize]
public class UserManagerModel : PageModel
{
    
    public IActionResult OnGet()
    {
        return Page();

    }

}
