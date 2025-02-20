
namespace ProjParkNet.Data.Repositories;

public class ParkingRepository
{
    private readonly ParkingDbContext _context;

    public ParkingRepository(ParkingDbContext context)
    {
        _context = context;
    }

    public async Task SaveParkingAsync(Parking parking)
    {
        await _context.Parkings.AddAsync(parking);
        await _context.SaveChangesAsync();
    }

    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(uploadsFolder);

        var filePath = Path.Combine(uploadsFolder, file.FileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return filePath;
    }


    public async Task<List<Parking>> GetAllParkingsAsync()
    {
        return await _context.Parkings.ToListAsync();
    }

    public async Task<Parking> GetParkingByIdAsync(int id)
    {
        var parking = await _context.Parkings
            .Include(p => p.Floors)
            .ThenInclude(f => f.Spots)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parking == null)
        {
            throw new KeyNotFoundException($"Parking lot with ID {id} was not found.");
        }

        return parking;
    }

    public Parking ProcessParkingFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file '{filePath}' was not found.");
            }

            var lines = File.ReadAllLines(filePath);
            var parking = new Parking
            {
                FileName = Path.GetFileNameWithoutExtension(filePath),
                Floors = new List<ParkingFloor>()
            };

            int floorIndex = 0;
            ParkingFloor currentFloor = new ParkingFloor
            {
                FloorNumber = floorIndex + 1,
                Spots = new List<ParkingSpot>()
            };

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    if (currentFloor.Spots.Count > 0)
                    {
                        parking.Floors.Add(currentFloor);
                        floorIndex++;
                        currentFloor = new ParkingFloor { FloorNumber = floorIndex + 1 };
                    }
                    continue;
                }

                for (int j = 0; j < lines[i].Length; j++)
                {
                    if (lines[i][j] == 'M' || lines[i][j] == 'C')
                    {
                        var spot = new ParkingSpot
                        {
                            SpotIdent = $"{(char)('A' + i)}{j + 1}",
                            TypeVehicle = lines[i][j] == 'M' ? "Moto" : "Car"
                        };
                        currentFloor.Spots.Add(spot);
                    }
                }
            }

            if (currentFloor.Spots.Count > 0)
            {
                parking.Floors.Add(currentFloor);
            }

            return parking;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
            throw;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"IO error while accessing the file: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }


    public async Task DeleteParkingAsync(int parkingId)
    {
        try
        {
            var parking = await _context.Parkings.FindAsync(parkingId);

            if (parking != null)
            {
                _context.Parkings.Remove(parking);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new Exception("Not Found Park.");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Unexpected error: ", ex);
        }
    }


    public async Task UpdateParkingAsync(Parking parking)
    {
        var existingParking = await _context.Parkings
            .FirstOrDefaultAsync(p => p.Id == parking.Id);

        if (existingParking != null)
        {
            existingParking.NamePark = parking.NamePark;
            existingParking.Address = parking.Address;
            existingParking.District = parking.District;
            existingParking.ZipCode = parking.ZipCode;
            existingParking.TelephoneNumber = parking.TelephoneNumber;
            try
            {
                _context.Parkings.Update(existingParking);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error updating Parking.", ex);
            }
        }
        else
        {
            throw new InvalidOperationException("Parking not found.");
        }
    }



    public async Task<bool> ParkingExistsAsync(int id)
    {
        return await _context.Parkings.AnyAsync(e => e.Id == id);
    }


    public bool IsFileValid(IFormFile file, out string message)
    {
        message = string.Empty;

        var allowedExtensions = new[] { ".csv", ".txt" };
        var fileExtension = Path.GetExtension(file.FileName);

        if (!allowedExtensions.Contains(fileExtension.ToLower()))
        {
            message = "Invalid file type!";
            return false;
        }

        return true;
    }
}



