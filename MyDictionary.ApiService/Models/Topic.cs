namespace MyDictionary.ApiService.Models;

public class Topic
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
