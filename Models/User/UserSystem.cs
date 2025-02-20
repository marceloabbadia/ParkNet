
namespace ProjParkNet.Models.User;
public class UserSystem
{
    [Key]
    public string Id { get; set; }

    [ForeignKey("Id")]
    public IdentityUser IdentityUser { get; set; }


    [Required]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "O NIF deve conter 9 caracteres.")]
    [Column("nif")]
    public string Nif { get; set; }

    [Required]
    [StringLength(15, ErrorMessage = "O número da carta de condução não pode ser maior que 15 caracteres.")]
    [Column("driving_license")]
    public string DrivingLicense { get; set; }

   
}


