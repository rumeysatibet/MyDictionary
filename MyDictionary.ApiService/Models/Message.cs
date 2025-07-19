using System.ComponentModel.DataAnnotations;

namespace MyDictionary.ApiService.Models;

public class Message
{
    public int Id { get; set; }
    
    [Required]
    public int SenderId { get; set; }
    
    [Required]
    public int ReceiverId { get; set; }
    
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = "";
    
    public bool IsRead { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ReadAt { get; set; }
    
    // Navigation properties
    public User Sender { get; set; } = null!;
    public User Receiver { get; set; } = null!;
}

public class Conversation
{
    public int Id { get; set; }
    
    [Required]
    public int User1Id { get; set; }
    
    [Required]
    public int User2Id { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;
    
    public int? LastMessageId { get; set; }
    
    // Navigation properties
    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public Message? LastMessage { get; set; }
}