
namespace ProjParkNet.Data.Repositories;

public class ParkingUsageRepository
{
    private readonly ParkingDbContext _context;

    public ParkingUsageRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<ParkingUsage?> GetActiveParkingForUserAsync(string userId)
    {
        return await _context.ParkingUsages
            .Where(p => p.UserId == userId && p.ExitTime == null)
            .OrderByDescending(p => p.EntryTime)
            .FirstOrDefaultAsync();
    }

    //public async Task StartParkingAsync(string userId, int parkingId, string spotIdent, string vehicleType, string plateNumber, DateTime entryTime)
    //{
    //    var newParkingUsage = new ParkingUsage
    //    {
    //        UserId = userId,
    //        ParkingId = parkingId,
    //        SpotIdent = spotIdent,
    //        VehicleType = vehicleType,
    //        PlateNumber = plateNumber,
    //        EntryTime = entryTime
    //    };

    //    _context.ParkingUsages.Add(newParkingUsage);
    //    await _context.SaveChangesAsync();
    //}

    public async Task EndParkingAsync(string userId, DateTime exitTime)
    {
        

        var activeParking = await GetActiveParkingForUserAsync(userId);
        if (activeParking != null)
        {
            activeParking.ExitTime = exitTime;
            activeParking.ParkingSpot.IsOccupied = false;
            await _context.SaveChangesAsync();
        }

       
    }


    public async Task SaveParkingUsageDataAsync(string userId, string matricula, string typeVehicle, int selectedParkId, string selectedSpotIdent, string entryTime, string exitTime)
    {
        
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new Exception("Usuário não encontrado na tabela AspNetUsers.");
        }

        // Verificar se a vaga já está ocupada
        var spot = await _context.ParkingSpots
            .Include(s => s.ParkingFloor) // Inclui o andar relacionado
            .FirstOrDefaultAsync(s => s.SpotIdent == selectedSpotIdent && s.ParkingFloor.ParkingId == selectedParkId);

        if (spot == null)
        {
            throw new Exception("Vaga não encontrada.");
        }

        if (spot.IsOccupied)
        {
            throw new Exception("Vaga já está ocupada.");
        }

        // Criar um novo registro de uso do estacionamento
        var parkingUsage = new ParkingUsage
        {
            UserId = userId,
            Matricula = matricula,
            TypeVehicle = typeVehicle,
            ParkingSpotId = spot.Id,
            EntryTime = DateTime.Parse(entryTime),
            ExitTime = string.IsNullOrEmpty(exitTime) ? (DateTime?)null : DateTime.Parse(exitTime)
        };

        // Marcar a vaga como ocupada
        spot.IsOccupied = true;

        // Adicionar o registro ao contexto
        _context.ParkingUsages.Add(parkingUsage);
        _context.ParkingSpots.Update(spot);

        // Salvar as alterações no banco de dados
        await _context.SaveChangesAsync();
    }
}