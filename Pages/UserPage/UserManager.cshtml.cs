
namespace ProjParkNet.Pages.UserPage;

public class UserManagerModel : PageModel
{
    public IActionResult OnGet()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToPage("/Login");
        }

        return Page();
    }

}
