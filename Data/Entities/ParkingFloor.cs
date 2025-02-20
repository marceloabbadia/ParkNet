using System.ComponentModel.DataAnnotations.Schema;
using ProjParkNet.Models;

namespace ProjParkNet.Data.Entities
{
    public class ParkingFloor
    {
        public int Id { get; set; }

        [Column("floor_number")]
        public int FloorNumber { get; set; }
        public List<ParkingSpot> Spots { get; set; } = new List<ParkingSpot>();

        public int ParkingId { get; set; }
        public Parking Parking { get; set; }
    }
}

