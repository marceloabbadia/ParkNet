
namespace ProjParkNet.Data.Repositories;

public class ParkingSpotsRepository
{

    private readonly ParkingDbContext _context;

    public ParkingSpotsRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetAvailableSpotIdentsAsync(int parkingId)
    {
        var spotIdents = await _context.ParkingSpots
        .Include(spot => spot.ParkingFloor) 
        .ThenInclude(floor => floor.Parking) 
        .Where(spot => spot.ParkingFloor.ParkingId == parkingId && !spot.IsOccupied) 
        .Select(spot => spot.SpotIdent) 
        .ToListAsync(); 
        return spotIdents;
    }

    public async Task<List<ParkingFloor>> GetFloorsWithSpotsAsync(int parkingId)
    {
        var floors = await _context.Floors
            .Include(floor => floor.Spots) // Carrega as vagas (Spots) de cada andar
            .Where(floor => floor.ParkingId == parkingId) // Filtra pelo ID do estacionamento
            .ToListAsync();

        return floors;
    }


    public async Task<int?> GetParkingIdFromSpotAsync(int spotId)
    {
        return await _context.ParkingSpots
            .Where(spot => spot.Id == spotId)
            .Select(spot => spot.ParkingFloor.ParkingId)
            .FirstOrDefaultAsync();
    }


    public async Task<ParkingSpot?> GetSpotDetailsByIdAsync(int spotId)
    {
        return await _context.ParkingSpots
            .Include(spot => spot.ParkingFloor) // Inclui a informação do andar
            .ThenInclude(floor => floor.Parking) // Inclui a informação do estacionamento
            .FirstOrDefaultAsync(spot => spot.Id == spotId);
    }
}


