namespace MyDictionary.ApiService.DTOs;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangeEmailRequest
{
    public string NewEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    public string? Email { get; set; }
    public string? Bio { get; set; }
}

public class DeleteAccountRequest
{
    public string Password { get; set; } = string.Empty;
}

public class PrivacySettingsModel
{
    public bool IsProfilePrivate { get; set; }
    public bool AllowMessagesFromFriendsOnly { get; set; }
    public bool HideFollowersList { get; set; }
    public bool HideOnlineStatus { get; set; }
    public bool MakeEntriesPrivate { get; set; }
}

public class NotificationSettingsModel
{
    public bool FriendRequests { get; set; } = true;
    public bool NewFollowers { get; set; } = true;
    public bool NewMessages { get; set; } = true;
    public bool EntryLikes { get; set; } = true;
    public bool EntryComments { get; set; } = true;
}

public class BlockedUserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
    public DateTime BlockedAt { get; set; }
}