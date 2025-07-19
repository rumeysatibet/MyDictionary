namespace MyDictionary.ApiService.Models;

public class Entry
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }
    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UpVotes { get; set; } = 0;
    public int DownVotes { get; set; } = 0;
}
