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
}
