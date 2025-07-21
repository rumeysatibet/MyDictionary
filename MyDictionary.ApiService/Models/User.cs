namespace MyDictionary.ApiService.Models;

public enum Gender
{
    NotSpecified = 0,
    Male = 1,
    Female = 2,
    Other = 3
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; } = Gender.NotSpecified;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string Role { get; set; } = "User"; // "User", "Admin"
    
    // Profile bilgileri
    public string? About { get; set; }
    public string? ProfilePhotoUrl { get; set; }
    public int FollowerCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;
    public int EntryCount { get; set; } = 0;
    public int TopicCount { get; set; } = 0; // Açtığı başlık sayısı
    
    // Privacy Settings
    public bool IsProfilePrivate { get; set; } = false;
    public bool AllowMessagesFromFriendsOnly { get; set; } = false;
    public bool HideFollowersList { get; set; } = false;
    public bool HideOnlineStatus { get; set; } = false;
    public bool MakeEntriesPrivate { get; set; } = false;
    
    // Notification Settings
    public bool NotifyOnFriendRequests { get; set; } = true;
    public bool NotifyOnNewFollowers { get; set; } = true;
    public bool NotifyOnNewMessages { get; set; } = true;
    public bool NotifyOnEntryLikes { get; set; } = true;
    public bool NotifyOnEntryComments { get; set; } = true;
    
    // Navigation Properties
    public ICollection<Topic> Topics { get; set; } = new List<Topic>(); // Açtığı başlıklar
    public ICollection<Entry> Entries { get; set; } = new List<Entry>(); // Yazdığı entry'ler
    public ICollection<EntryFavorite> FavoriteEntries { get; set; } = new List<EntryFavorite>(); // Favori entry'leri
}
