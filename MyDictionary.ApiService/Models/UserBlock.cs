namespace MyDictionary.ApiService.Models;

public class UserBlock
{
    public int Id { get; set; }
    public int BlockingUserId { get; set; } // Engelleyen kullanıcı
    public int BlockedUserId { get; set; } // Engellenen kullanıcı
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User BlockingUser { get; set; } = null!;
    public User BlockedUser { get; set; } = null!;
}