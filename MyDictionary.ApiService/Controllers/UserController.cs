using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using MyDictionary.ApiService.Services;
using MyDictionary.ApiService.DTOs;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(DictionaryDbContext context, ILogger<UserController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUsers(string query, int page = 1, int pageSize = 10)
    {
        try
        {
            _logger.LogInformation($"🔍 Kullanıcı arama - Query: {query}");

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                return BadRequest(new { success = false, message = "En az 2 karakter girin." });
            }

            var users = await _context.Users
                .Where(u => u.Username.ToLower().Contains(query.ToLower()) || u.Email.ToLower().Contains(query.ToLower()))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.ProfilePhotoUrl,
                    u.CreatedAt,
                    u.EntryCount,
                    u.FollowerCount
                })
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation($"✅ {users.Count} kullanıcı bulundu");

            return Ok(new
            {
                success = true,
                users = users,
                totalCount = await _context.Users.CountAsync(u => u.Username.ToLower().Contains(query.ToLower()) || u.Email.ToLower().Contains(query.ToLower()))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Kullanıcı arama hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("{userId}/relationship")]
    [Authorize]
    public async Task<IActionResult> GetUserRelationship(int userId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"👥 Kullanıcı ilişkisi kontrol ediliyor - Current: {currentUserId}, Target: {userId}");

            if (currentUserId == userId)
            {
                return Ok(new { success = true, relationship = "self" });
            }

            // Arkadaş mı?
            var friendship = await _context.Friendships
                .AnyAsync(f => (f.UserId == currentUserId && f.FriendId == userId) ||
                              (f.UserId == userId && f.FriendId == currentUserId));

            if (friendship)
            {
                return Ok(new { success = true, relationship = "friend" });
            }

            // Bekleyen istek var mı?
            var pendingRequest = await _context.FriendRequests
                .Where(fr => fr.Status == FriendRequestStatus.Pending)
                .FirstOrDefaultAsync(fr => (fr.SenderId == currentUserId && fr.ReceiverId == userId) ||
                                          (fr.SenderId == userId && fr.ReceiverId == currentUserId));

            if (pendingRequest != null)
            {
                if (pendingRequest.SenderId == currentUserId)
                {
                    return Ok(new { success = true, relationship = "request_sent" });
                }
                else
                {
                    return Ok(new { success = true, relationship = "request_received", requestId = pendingRequest.Id });
                }
            }

            return Ok(new { success = true, relationship = "none" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Kullanıcı ilişkisi kontrol hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"🔐 Şifre değiştirme isteği - UserId: {currentUserId}");

            if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                _logger.LogWarning($"❌ Şifre değiştirme - Boş şifre alanları - UserId: {currentUserId}");
                return BadRequest(new { success = false, message = "Mevcut şifre ve yeni şifre gerekli." });
            }

            if (request.NewPassword.Length < 6)
            {
                _logger.LogWarning($"❌ Şifre değiştirme - Çok kısa şifre - UserId: {currentUserId}");
                return BadRequest(new { success = false, message = "Yeni şifre en az 6 karakter olmalı." });
            }

            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
            {
                _logger.LogWarning($"❌ Şifre değiştirme - Kullanıcı bulunamadı - UserId: {currentUserId}");
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Mevcut şifre doğrulama
            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                _logger.LogWarning($"❌ Şifre değiştirme - Yanlış mevcut şifre - UserId: {currentUserId}, Username: {user.Username}");
                return BadRequest(new { success = false, message = "Mevcut şifre yanlış." });
            }

            // Yeni şifre hashleme ve güncelleme
            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Şifre başarıyla değiştirildi - UserId: {currentUserId}, Username: {user.Username}");
            return Ok(new { success = true, message = "Şifreniz başarıyla değiştirildi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Şifre değiştirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("change-email")]
    [Authorize]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailRequest request)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📧 E-posta değiştirme isteği - UserId: {currentUserId}, NewEmail: {request.NewEmail}");

            if (string.IsNullOrWhiteSpace(request.NewEmail) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning($"❌ E-posta değiştirme - Boş alanlar - UserId: {currentUserId}");
                return BadRequest(new { success = false, message = "Yeni e-posta ve şifre gerekli." });
            }

            if (!IsValidEmail(request.NewEmail))
            {
                _logger.LogWarning($"❌ E-posta değiştirme - Geçersiz e-posta formatı - UserId: {currentUserId}, Email: {request.NewEmail}");
                return BadRequest(new { success = false, message = "Geçerli bir e-posta adresi girin." });
            }

            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
            {
                _logger.LogWarning($"❌ E-posta değiştirme - Kullanıcı bulunamadı - UserId: {currentUserId}");
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Şifre doğrulama
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"❌ E-posta değiştirme - Yanlış şifre - UserId: {currentUserId}, Username: {user.Username}");
                return BadRequest(new { success = false, message = "Şifre yanlış." });
            }

            // E-posta zaten kullanılıyor mu kontrol et
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == request.NewEmail.ToLower() && u.Id != currentUserId);
            
            if (existingUser != null)
            {
                _logger.LogWarning($"❌ E-posta değiştirme - E-posta zaten kullanılıyor - UserId: {currentUserId}, Email: {request.NewEmail}");
                return BadRequest(new { success = false, message = "Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor." });
            }

            var oldEmail = user.Email;
            user.Email = request.NewEmail;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ E-posta başarıyla değiştirildi - UserId: {currentUserId}, Username: {user.Username}, OldEmail: {oldEmail}, NewEmail: {request.NewEmail}");
            return Ok(new { success = true, message = "E-posta adresiniz başarıyla değiştirildi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ E-posta değiştirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"👤 Profil güncelleme isteği - UserId: {currentUserId}");

            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
            {
                _logger.LogWarning($"❌ Profil güncelleme - Kullanıcı bulunamadı - UserId: {currentUserId}");
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // E-posta formatı kontrolü
            if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            {
                _logger.LogWarning($"❌ Profil güncelleme - Geçersiz e-posta formatı - UserId: {currentUserId}, Email: {request.Email}");
                return BadRequest(new { success = false, message = "Geçerli bir e-posta adresi girin." });
            }

            // E-posta zaten kullanılıyor mu kontrol et
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.Id != currentUserId);
                
                if (existingUser != null)
                {
                    _logger.LogWarning($"❌ Profil güncelleme - E-posta zaten kullanılıyor - UserId: {currentUserId}, Email: {request.Email}");
                    return BadRequest(new { success = false, message = "Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor." });
                }
            }

            // Profil bilgilerini güncelle
            if (!string.IsNullOrWhiteSpace(request.Email))
                user.Email = request.Email;
            
            if (request.Bio != null) // null check - boş string kabul edilebilir
                user.About = request.Bio.Length > 500 ? request.Bio.Substring(0, 500) : request.Bio;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Profil başarıyla güncellendi - UserId: {currentUserId}, Username: {user.Username}");
            return Ok(new { success = true, message = "Profil bilgileriniz başarıyla güncellendi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Profil güncelleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpDelete("delete-account")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"🗑️ Hesap silme isteği - UserId: {currentUserId}");

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning($"❌ Hesap silme - Şifre gerekli - UserId: {currentUserId}");
                return BadRequest(new { success = false, message = "Hesabınızı silmek için şifrenizi girin." });
            }

            var user = await _context.Users
                .Include(u => u.Topics)
                .Include(u => u.Entries)
                //.Include(u => u.EntryFavorites)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
            {
                _logger.LogWarning($"❌ Hesap silme - Kullanıcı bulunamadı - UserId: {currentUserId}");
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Şifre doğrulama
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning($"❌ Hesap silme - Yanlış şifre - UserId: {currentUserId}, Username: {user.Username}");
                return BadRequest(new { success = false, message = "Şifre yanlış." });
            }

            // Transaction başlat - tüm veriler silinmeli
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Favorileri sil
                //if (user.EntryFavorites.Any())
                //{
                //    _context.EntryFavorites.RemoveRange(user.EntryFavorites);
                //}

                // Entry'leri sil
                if (user.Entries.Any())
                {
                    _context.Entries.RemoveRange(user.Entries);
                }

                // Topic'leri sil
                if (user.Topics.Any())
                {
                    _context.Topics.RemoveRange(user.Topics);
                }

                // Arkadaşlık isteklerini sil
                var friendRequests = await _context.FriendRequests
                    .Where(fr => fr.SenderId == currentUserId || fr.ReceiverId == currentUserId)
                    .ToListAsync();
                if (friendRequests.Any())
                {
                    _context.FriendRequests.RemoveRange(friendRequests);
                }

                // Arkadaşlıkları sil
                var friendships = await _context.Friendships
                    .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
                    .ToListAsync();
                if (friendships.Any())
                {
                    _context.Friendships.RemoveRange(friendships);
                }

                // Mesajları sil
                var messages = await _context.Messages
                    .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                    .ToListAsync();
                if (messages.Any())
                {
                    _context.Messages.RemoveRange(messages);
                }

                // Son olarak kullanıcıyı sil
                _context.Users.Remove(user);
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"✅ Hesap başarıyla silindi - UserId: {currentUserId}, Username: {user.Username}");
                return Ok(new { success = true, message = "Hesabınız ve tüm verileriniz başarıyla silindi." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"❌ Hesap silme transaction hatası - UserId: {currentUserId}: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Hesap silme işlemi sırasında bir hata oluştu." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Hesap silme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("privacy-settings")]
    [Authorize]
    public async Task<IActionResult> GetPrivacySettings()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(currentUserId);
            
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            var settings = new
            {
                isProfilePrivate = user.IsProfilePrivate,
                allowMessagesFromFriendsOnly = user.AllowMessagesFromFriendsOnly,
                hideFollowersList = user.HideFollowersList,
                hideOnlineStatus = user.HideOnlineStatus,
                makeEntriesPrivate = user.MakeEntriesPrivate
            };

            return Ok(new { success = true, settings });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Gizlilik ayarları yükleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPut("privacy-settings")]
    [Authorize]
    public async Task<IActionResult> UpdatePrivacySettings([FromBody] PrivacySettingsModel settings)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(currentUserId);
            
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            user.IsProfilePrivate = settings.IsProfilePrivate;
            user.AllowMessagesFromFriendsOnly = settings.AllowMessagesFromFriendsOnly;
            user.HideFollowersList = settings.HideFollowersList;
            user.HideOnlineStatus = settings.HideOnlineStatus;
            user.MakeEntriesPrivate = settings.MakeEntriesPrivate;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Gizlilik ayarları güncellendi - UserId: {currentUserId}");
            return Ok(new { success = true, message = "Gizlilik ayarları başarıyla güncellendi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Gizlilik ayarları güncelleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("notification-settings")]
    [Authorize]
    public async Task<IActionResult> GetNotificationSettings()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(currentUserId);
            
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            var settings = new
            {
                friendRequests = user.NotifyOnFriendRequests,
                newFollowers = user.NotifyOnNewFollowers,
                newMessages = user.NotifyOnNewMessages,
                entryLikes = user.NotifyOnEntryLikes,
                entryComments = user.NotifyOnEntryComments
            };

            return Ok(new { success = true, settings });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Bildirim ayarları yükleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPut("notification-settings")]
    [Authorize]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsModel settings)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(currentUserId);
            
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            user.NotifyOnFriendRequests = settings.FriendRequests;
            user.NotifyOnNewFollowers = settings.NewFollowers;
            user.NotifyOnNewMessages = settings.NewMessages;
            user.NotifyOnEntryLikes = settings.EntryLikes;
            user.NotifyOnEntryComments = settings.EntryComments;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Bildirim ayarları güncellendi - UserId: {currentUserId}");
            return Ok(new { success = true, message = "Bildirim ayarları başarıyla güncellendi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Bildirim ayarları güncelleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("blocked-users")]
    [Authorize]
    public async Task<IActionResult> GetBlockedUsers()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var blockedUsers = await _context.UserBlocks
                .Include(ub => ub.BlockedUser)
                .Where(ub => ub.BlockingUserId == currentUserId)
                .Select(ub => new BlockedUserResponse
                {
                    Id = ub.BlockedUser.Id,
                    Username = ub.BlockedUser.Username,
                    ProfilePhotoUrl = ub.BlockedUser.ProfilePhotoUrl,
                    BlockedAt = ub.BlockedAt
                })
                .OrderByDescending(bu => bu.BlockedAt)
                .ToListAsync();

            return Ok(new { success = true, users = blockedUsers });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Engellenen kullanıcılar yükleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("block/{userId}")]
    [Authorize]
    public async Task<IActionResult> BlockUser(int userId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (currentUserId == userId)
                return BadRequest(new { success = false, message = "Kendinizi engelleyemezsiniz." });

            var targetUser = await _context.Users.FindAsync(userId);
            if (targetUser == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            // Zaten engellenmiş mi kontrol et
            var existingBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(ub => ub.BlockingUserId == currentUserId && ub.BlockedUserId == userId);
            
            if (existingBlock != null)
                return BadRequest(new { success = false, message = "Bu kullanıcı zaten engellenmiş." });

            var userBlock = new UserBlock
            {
                BlockingUserId = currentUserId,
                BlockedUserId = userId,
                BlockedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(userBlock);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Kullanıcı engellendi - BlockingUserId: {currentUserId}, BlockedUserId: {userId}");
            return Ok(new { success = true, message = "Kullanıcı başarıyla engellendi!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Kullanıcı engelleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpDelete("block/{userId}")]
    [Authorize]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var userBlock = await _context.UserBlocks
                .FirstOrDefaultAsync(ub => ub.BlockingUserId == currentUserId && ub.BlockedUserId == userId);
            
            if (userBlock == null)
                return NotFound(new { success = false, message = "Engel bulunamadı." });

            _context.UserBlocks.Remove(userBlock);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Kullanıcı engeli kaldırıldı - BlockingUserId: {currentUserId}, BlockedUserId: {userId}");
            return Ok(new { success = true, message = "Kullanıcı engeli başarıyla kaldırıldı!" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Kullanıcı engeli kaldırma hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    // Helper metodlar
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}