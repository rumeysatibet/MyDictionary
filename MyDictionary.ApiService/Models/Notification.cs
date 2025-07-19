using System.ComponentModel.DataAnnotations;

namespace MyDictionary.ApiService.Models;

public class Notification
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public int? FromUserId { get; set; }
    
    [Required]
    public NotificationType Type { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = "";
    
    public string? Data { get; set; } // JSON data for additional information
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User? FromUser { get; set; }
}

public enum NotificationType
{
    FriendRequest = 1,
    FriendRequestAccepted = 2,
    NewMessage = 3,
    EntryLiked = 4,
    EntryCommented = 5,
    System = 6
}