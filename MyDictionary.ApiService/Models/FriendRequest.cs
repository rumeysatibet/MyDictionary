namespace MyDictionary.ApiService.Models;

public enum FriendRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class FriendRequest
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    
    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}