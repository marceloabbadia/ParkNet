using System.ComponentModel.DataAnnotations.Schema;
using ProjParkNet.Models;

namespace ProjParkNet.Data.Entities
{
    public class ParkingSpot
    {
        public int Id { get; set; }

        [Column("spot_ident")]
        public string SpotIdent { get; set; }

        [Column("type_vehicle")]
        public string TypeVehicle { get; set; }
        public bool IsOccupied { get; set; } = false;

        public int ParkingFloorId { get; set; }
        public ParkingFloor ParkingFloor { get; set; }
    }
}

