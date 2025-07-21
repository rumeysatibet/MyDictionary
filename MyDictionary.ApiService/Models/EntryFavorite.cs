namespace MyDictionary.ApiService.Models;

public class EntryFavorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int EntryId { get; set; }
    public Entry? Entry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}