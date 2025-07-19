namespace MyDictionary.ApiService.Models;

public class UserAgreement
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty; // "1.0", "1.1", etc.
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class UserAgreementAcceptance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int AgreementId { get; set; }
    public UserAgreement? Agreement { get; set; }
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
}