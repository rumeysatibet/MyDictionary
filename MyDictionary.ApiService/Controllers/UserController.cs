using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Security.Claims;

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
                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
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
                totalCount = await _context.Users.CountAsync(u => u.Username.Contains(query) || u.Email.Contains(query))
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
}