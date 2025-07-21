namespace MyDictionary.ApiService.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty; // URL için: web-gelistirme
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty; // Emoji veya icon class
    public string Color { get; set; } = "#53925F"; // Kategori rengi
    public int SortOrder { get; set; } = 0; // Sıralama için
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}