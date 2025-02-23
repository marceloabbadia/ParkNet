namespace ProjParkNet.Models.User;

public class UserTransaction
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateTime TransactionDate { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string Type { get; set; }

    public IdentityUser User { get; set; }
}

