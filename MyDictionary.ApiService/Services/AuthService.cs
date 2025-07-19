using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.DTOs;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserAgreementDto?> GetActiveUserAgreementAsync();
    Task<List<object>> GetAllUsersAsync();
}

public class AuthService : IAuthService
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<AuthService> _logger;

    public AuthService(DictionaryDbContext context, ILogger<AuthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto, string ipAddress)
    {
        try
        {
            // Kullanıcı adı kontrolü
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Bu kullanıcı adı veya e-posta adresi zaten kullanılıyor."
                };
            }

            // Sözleşme kontrolü
            var activeAgreement = await _context.UserAgreements
                .FirstOrDefaultAsync(ua => ua.IsActive);

            if (activeAgreement == null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Aktif kullanıcı sözleşmesi bulunamadı."
                };
            }

            // Şifre hashleme
            var passwordHash = HashPassword(registerDto.Password);

            // Kullanıcı oluşturma
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                BirthDate = registerDto.BirthDate,
                Gender = registerDto.Gender,
                Role = "User",
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("User oluşturuluyor - Username: {Username}, Email: {Email}, Role: {Role}, Gender: {Gender}, BirthDate: {BirthDate}", 
                user.Username, user.Email, user.Role, user.Gender, user.BirthDate);
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Sözleşme kabulü kaydetme
            var acceptance = new UserAgreementAcceptance
            {
                UserId = user.Id,
                AgreementId = activeAgreement.Id,
                AcceptedAt = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.UserAgreementAcceptances.Add(acceptance);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kullanıcı başarıyla oluşturuldu ve kayıt tamamlandı. UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Kayıt başarıyla tamamlandı! Artık giriş yapabilirsiniz.",
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt hatası - Detay: {ErrorMessage}", ex.Message);
            return new AuthResponseDto
            {
                Success = false,
                Message = $"Kayıt hatası: {ex.Message}"
            };
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => 
                    u.Username == loginDto.UsernameOrEmail || 
                    u.Email == loginDto.UsernameOrEmail);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Geçersiz kullanıcı adı/e-posta veya şifre."
                };
            }


            // Son giriş zamanını güncelle
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Token oluştur (basit implementasyon - JWT olmadan)
            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Giriş başarılı!",
                Token = token,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Giriş hatası");
            return new AuthResponseDto
            {
                Success = false,
                Message = "Giriş sırasında bir hata oluştu."
            };
        }
    }

    public async Task<UserAgreementDto?> GetActiveUserAgreementAsync()
    {
        var agreement = await _context.UserAgreements
            .FirstOrDefaultAsync(ua => ua.IsActive);

        if (agreement == null)
            return null;

        return new UserAgreementDto
        {
            Id = agreement.Id,
            Title = agreement.Title,
            Content = agreement.Content,
            Version = agreement.Version,
            CreatedAt = agreement.CreatedAt
        };
    }

    public async Task<List<object>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.CreatedAt,
                u.LastLoginAt,
                u.Role,
                EntryCount = _context.Entries.Count(e => e.CreatedByUserId == u.Id)
            })
            .ToListAsync();

        return users.Cast<object>().ToList();
    }

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

    private static string GenerateToken(User user)
    {
        // Basit token - gerçek uygulamada JWT kullanılmalı
        var tokenData = $"{user.Id}:{user.Username}:{DateTime.UtcNow.Ticks}";
        var tokenBytes = Encoding.UTF8.GetBytes(tokenData);
        return Convert.ToBase64String(tokenBytes);
    }

    private static UserDto MapToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            BirthDate = user.BirthDate,
            Gender = user.Gender,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}