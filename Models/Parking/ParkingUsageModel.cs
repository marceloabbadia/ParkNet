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

        public TimeSpan TotalTime => ExitTime.HasValue ? ExitTime.Value - EntryTime : TimeSpan.Zero;

        public decimal TotalAmount => CalculateAmount();

        private decimal CalculateAmount()
        {
            var parking = GetParkingBySpotId(ParkingSpotId);

            if (parking != null)
            {
                decimal ratePerHour = parking.PricePerHour;
                return (decimal)TotalTime.TotalHours * ratePerHour;
            }
            return 0.0m;
        }

        private ParkingModel GetParkingBySpotId(int parkingSpotId)
        {
            // Aqui você recuperaria o parque associado ao spot de estacionamento
            // No código real, você deve consultar o banco de dados para buscar o parque com base no ParkingSpotId
            // Para fins de exemplo, estamos retornando um parque fictício
            return new ParkingModel
            {
                PricePerHour = 10.0m // Exemplo de preço por hora (deve ser substituído por consulta real ao banco)
            };
        }
    }
}