using System.ComponentModel.DataAnnotations.Schema;

namespace ProjParkNet.Data.Entities
{
    public class Parking
    {
        public int Id { get; set; }

        [Column("name_park")]
        public string NamePark { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("address_park")]
        public string Address { get; set; }

        [Column("district_park")]
        public string District { get; set; }

        [Column("zip_code")]
        public string ZipCode { get; set; }

        [Column("telephone_number")]
        public string TelephoneNumber { get; set; }

        [Column("price_monthly_agreement")]
        public decimal MonthlyAgreement { get; set; }

        [Column("price_minute")]
        public decimal PricePerMinute { get; set; }

        public List<ParkingFloor> Floors { get; set; } = new List<ParkingFloor>();
    }
}


