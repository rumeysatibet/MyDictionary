using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Security.Claims;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<ProfileController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(DictionaryDbContext context, ILogger<ProfileController> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        try
        {
            _logger.LogInformation($"📄 Profil getiriliyor - Username: {username}");

            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.BirthDate,
                    u.Gender,
                    u.Role,
                    u.CreatedAt,
                    u.LastLoginAt,
                    u.About,
                    u.ProfilePhotoUrl,
                    u.FollowerCount,
                    u.FollowingCount,
                    u.EntryCount
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning($"❌ Kullanıcı bulunamadı - Username: {username}");
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            _logger.LogInformation($"✅ Profil bulundu - Username: {username}");

            return Ok(new
            {
                success = true,
                user = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Profil getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPut("update")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📝 Profil güncelleniyor - UserId: {userId}");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // About bilgisini güncelle
            if (request.About != null)
            {
                user.About = request.About.Trim();
                _logger.LogInformation($"📝 About güncellendi - UserId: {userId}");
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Profil başarıyla güncellendi.",
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.BirthDate,
                    user.Gender,
                    user.Role,
                    user.CreatedAt,
                    user.LastLoginAt,
                    user.About,
                    user.ProfilePhotoUrl,
                    user.FollowerCount,
                    user.FollowingCount,
                    user.EntryCount
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Profil güncelleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("upload-photo")]
    [Authorize]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📷 Profil fotoğrafı yükleniyor - UserId: {userId}");
            _logger.LogInformation($"📷 WebRootPath: {_environment.WebRootPath}");
            _logger.LogInformation($"📷 Content Root: {_environment.ContentRootPath}");

            if (photo == null || photo.Length == 0)
            {
                _logger.LogWarning("❌ Fotoğraf dosyası boş veya null");
                return BadRequest(new { success = false, message = "Geçerli bir fotoğraf seçin." });
            }

            _logger.LogInformation($"📷 Dosya bilgileri - Name: {photo.FileName}, Size: {photo.Length}, ContentType: {photo.ContentType}");

            // Dosya boyutu kontrolü (max 5MB)
            if (photo.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { success = false, message = "Fotoğraf boyutu 5MB'dan büyük olamaz." });
            }

            // Dosya türü kontrolü
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png" };
            if (!allowedTypes.Contains(photo.ContentType.ToLower()))
            {
                return BadRequest(new { success = false, message = "Sadece JPG ve PNG formatları desteklenir." });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Yeni dosya adı oluştur
            var fileExtension = Path.GetExtension(photo.FileName);
            var fileName = $"{userId}_{DateTime.Now.Ticks}{fileExtension}";
            
            // Upload klasörünü oluştur 
            var uploadsPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsPath);
            _logger.LogInformation($"📷 Upload klasörü: {uploadsPath}");
            
            var filePath = Path.Combine(uploadsPath, fileName);
            _logger.LogInformation($"📷 Dosya yolu: {filePath}");

            // Eski fotoğrafı sil
            if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
            {
                var oldPhotoPath = Path.Combine(uploadsPath, user.ProfilePhotoUrl);
                if (System.IO.File.Exists(oldPhotoPath))
                {
                    System.IO.File.Delete(oldPhotoPath);
                    _logger.LogInformation($"🗑️ Eski profil fotoğrafı silindi - UserId: {userId}");
                }
            }

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Veritabanını güncelle
            user.ProfilePhotoUrl = fileName;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"✅ Profil fotoğrafı yüklendi - UserId: {userId}, FileName: {fileName}");

            return Ok(new
            {
                success = true,
                message = "Profil fotoğrafı başarıyla yüklendi.",
                photoUrl = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Profil fotoğrafı yükleme hatası: {ex.Message}");
            _logger.LogError($"❌ Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu.", error = ex.Message });
        }
    }
}

public class UpdateProfileRequest
{
    public string? About { get; set; }
}