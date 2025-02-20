namespace ProjParkNet.Data.Entities
{
    public class ParkingUsage
    {
        public int Id { get; set; }

        public int ParkingSpotId { get; set; }
        public ParkingSpot ParkingSpot { get; set; }

        public string UserId { get; set; }
        public UserSystem User { get; set; }

        public string Matricula { get; set; }

        [Column("type_vehicle")]
        public string TypeVehicle { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }

        public decimal? Price { get; set; }

        public bool IsPaid { get; set; }
    }

}

