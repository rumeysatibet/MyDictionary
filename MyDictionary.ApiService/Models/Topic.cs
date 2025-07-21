namespace MyDictionary.ApiService.Models;

public class Topic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL için: asp-net-core-nedir
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastEntryAt { get; set; } // Son entry tarihi
    public int EntryCount { get; set; } = 0; // Entry sayısı
    public int ViewCount { get; set; } = 0; // Görüntülenme sayısı
    
    // Navigation Properties
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
