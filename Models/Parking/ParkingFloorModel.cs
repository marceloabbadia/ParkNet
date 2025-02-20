
namespace ProjParkNet.Models.Parking;

    public class ParkingFloorModel
    {
        public int Id { get; set; }

        [Column("floor_number")]
        public int FloorNumber { get; set; }

        public int ParkingId { get; set; }
        public ParkingModel Parking { get; set; }

        public List<ParkingSpotModel> Spots { get; set; } = new List<ParkingSpotModel>();
    }
