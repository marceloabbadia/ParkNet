namespace ProjParkNet.Models.Parking
{
    public class ParkingUsageModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public UserSystem User { get; set; }

        [ForeignKey("ParkingSpotId")]
        public int ParkingSpotId { get; set; }
        public ParkingSpotModel ParkingSpot { get; set; }

        public string Matricula { get; set; }

        public DateTime EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }


        [Column(TypeName = "total_time")]
        public TimeSpan TotalTime { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public bool IsPaid { get; set; }

    }
}