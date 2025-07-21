namespace MyDictionary.ApiService.Models;

public class EntryLink
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public Entry? Entry { get; set; }
    public string Url { get; set; } = string.Empty;
    public string DisplayText { get; set; } = "bakınız"; // Gösterilecek metin
    public int Position { get; set; } = 0; // Entry içindeki pozisyon
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}