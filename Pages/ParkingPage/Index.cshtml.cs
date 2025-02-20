
namespace ProjParkNet.Pages.ParkingPage;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{

    private readonly ParkingDbContext _context;

    public IndexModel(ParkingDbContext context)
    {
        _context = context;
    }

    public IList<Parking> Parkings { get; set; }  

    public async Task OnGetAsync()
    {
        Parkings = await _context.Parkings.ToListAsync();  
    }
}

