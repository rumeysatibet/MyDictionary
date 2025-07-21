namespace MyDictionary.ApiService.Models;

public class Entry
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty; // Formatted HTML content
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int FavoriteCount { get; set; } = 0; // Favori sayısı
    public bool IsEdited { get; set; } = false;
    
    // Navigation Properties
    public ICollection<EntryFavorite> Favorites { get; set; } = new List<EntryFavorite>();
    public ICollection<EntryLink> Links { get; set; } = new List<EntryLink>();
}
