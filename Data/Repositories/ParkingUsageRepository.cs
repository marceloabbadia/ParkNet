
namespace ProjParkNet.Data.Repositories;

public class ParkingUsageRepository
{
    private readonly ParkingDbContext _context;

    public ParkingUsageRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task<int?> EndParkingAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentNullException(nameof(userId), "User ID cannot be empty or null.");
        }

        // Busca o estacionamento ativo do usuário
        var activeParking = await GetActiveParkingForUserAsync(userId);
        if (activeParking == null)
        {
            throw new InvalidOperationException("No active parking found for the given user.");
        }

        // Verifica se a vaga de estacionamento existe
        if (activeParking.ParkingSpot == null)
        {
            throw new InvalidOperationException("The associated parking spot is not available.");
        }

        // Calcula o tempo total e o preço, definindo o horário de saída dentro do método
        CalculateUsageData(activeParking);

        // Libera a vaga de estacionamento
        activeParking.ParkingSpot.IsOccupied = false;

        // Salva as alterações no banco de dados
        await _context.SaveChangesAsync();

        return activeParking.Id;
    }

    private void CalculateUsageData(ParkingUsage parkingUsage)
    {
        if (parkingUsage == null)
        {
            throw new ArgumentNullException(nameof(parkingUsage), "O registro de uso de estacionamento não pode ser nulo.");
        }

        // Define o horário de saída
        parkingUsage.ExitTime = DateTime.UtcNow;

        // Verifica se o horário de entrada é válido
        if (parkingUsage.EntryTime == default(DateTime))
        {
            throw new InvalidOperationException("O horário de entrada não foi definido.");
        }

        // Verifica se a vaga de estacionamento e o estacionamento associado estão disponíveis
        if (parkingUsage.ParkingSpot?.ParkingFloor?.Parking == null)
        {
            throw new InvalidOperationException("A vaga de estacionamento ou o estacionamento associado não foram encontrados.");
        }

        // Recupera o preço por minuto do estacionamento
        decimal pricePerMinute = parkingUsage.ParkingSpot.ParkingFloor.Parking.PricePerMinute;

        // Calcula o tempo total em minutos
        TimeSpan totalTime = parkingUsage.ExitTime.Value - parkingUsage.EntryTime;
        parkingUsage.TotalTimeMinutes = (int)Math.Ceiling(totalTime.TotalMinutes);

        // Calcula o preço com base no tempo total
        parkingUsage.Price = parkingUsage.TotalTimeMinutes * pricePerMinute;
    }


    public async Task<ParkingUsage> GetActiveParkingForUserAsync(string userId)
    {
        return await _context.ParkingUsages
            .Include(pu => pu.ParkingSpot)
            .ThenInclude(ps => ps.ParkingFloor)
            .ThenInclude(pf => pf.Parking)
            .FirstOrDefaultAsync(pu => pu.UserId == userId && pu.ExitTime == null);
    }

    public async Task<ParkingUsage> GetActiveParkingUsageForIdAsync(int parkingUsageId)
    {
        return await _context.ParkingUsages
            .Include(pu => pu.ParkingSpot)
            .ThenInclude(ps => ps.ParkingFloor)
            .ThenInclude(pf => pf.Parking)
            .FirstOrDefaultAsync(pu => pu.Id == parkingUsageId);
    }


    public async Task SaveParkingUsageDataAsync(string userId, string matricula, string typeVehicle, int selectedParkId, string selectedSpotIdent, DateTime entryTime)
    {

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            throw new Exception("User not found.");
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
            EntryTime = RoundToMinute(DateTime.UtcNow),
            Price = 0.0m,
            IsPaid = false
        };

        // Marcar a vaga como ocupada
        spot.IsOccupied = true;

        // Adicionar o registro ao contexto
        _context.ParkingUsages.Add(parkingUsage);
        _context.ParkingSpots.Update(spot);

        // Salvar as alterações no banco de dados
        await _context.SaveChangesAsync();
    }

    public async Task<List<ParkingUsage>> GetUserParkingHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.ParkingUsages
            .Include(pu => pu.ParkingSpot) // Inclui informações da vaga
            .ThenInclude(ps => ps.ParkingFloor) // Inclui informações do andar
            .ThenInclude(pf => pf.Parking) // Inclui informações do estacionamento
            .Where(pu => pu.UserId == userId); // Filtra pelo usuário logado

        // Aplica o filtro de data de início (se fornecido)
        if (startDate.HasValue)
        {
            query = query.Where(pu => pu.EntryTime >= startDate.Value);
        }

        // Aplica o filtro de data de término (se fornecido)
        if (endDate.HasValue)
        {
            query = query.Where(pu => pu.EntryTime <= endDate.Value);
        }

        // Ordena por data de entrada (mais recente primeiro)
        return await query.OrderByDescending(pu => pu.EntryTime).ToListAsync();
    }

    private DateTime RoundToMinute(DateTime dateTime)
    {
        return new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute,
            0, // Zera os segundos
            DateTimeKind.Utc
        );
    }

    public async Task UpdatePaymentStatusAsync(int parkingUsageId, bool isPaid)
    {
        // Busca o registro de ParkingUsage pelo ID
        var parkingUsage = await _context.ParkingUsages
                .FirstOrDefaultAsync(p => p.Id == parkingUsageId);

        if (parkingUsage == null)
        {
            throw new InvalidOperationException("Registro de estacionamento não encontrado.");
        }

        // Atualiza o status de pagamento
        parkingUsage.IsPaid = isPaid;

        // Salva as alterações no banco de dados
        await _context.SaveChangesAsync();
    }
}
