
namespace ProjParkNet.Models.Parking;

public class ParkingModel
{
    public int Id { get; set; }

    [Column("name_park")]
    [Required(ErrorMessage = "O nome do parque é obrigatório.")]
    public string NamePark { get; set; }

    [Column("file_name")]
    [Required(ErrorMessage = "O arquivo é obrigatório.")]
    public string FileName { get; set; }

    [Column("address_park")]
    [Required(ErrorMessage = "O endereço é obrigatório.")]
    public string Address { get; set; }

    [Column("district_park")]
    [Required(ErrorMessage = "O distrito é obrigatório.")]
    public string District { get; set; }

    [Column("zip_code")]
    [Required(ErrorMessage = "O código postal é obrigatório -Apenas números.")]
    public string ZipCode { get; set; }

    [Column("telephone_number")]
    [Required(ErrorMessage = "O telefone é obrigatório.")]
    [Phone(ErrorMessage = "Número de telefone inválido.")]
    public string TelephoneNumber { get; set; }

    [Column("price_hour")]
    [Required(ErrorMessage = "O Preço por hora é obrigatório.")]
    public decimal PricePerHour { get; set; }

    [Column("price_minute")]
    [Required(ErrorMessage = "O Preço por minuto é obrigatório.")]
    public decimal PricePerMinute { get; set; }

    public List<ParkingFloorModel> Floors { get; set; } = new List<ParkingFloorModel>();
}

