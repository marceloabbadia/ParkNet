namespace ProjParkNet.Models.User;

public class UserBalance
{
    [Key]
    public string UserId { get; set; }

    [Required]
    public decimal CurrentBalance { get; set; }

    public IdentityUser User { get; set; }
}

