using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Security.Claims;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(DictionaryDbContext context, ILogger<NotificationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetNotifications(int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"🔔 Bildirimler getiriliyor - UserId: {userId}");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.FromUser)
                .Select(n => new
                {
                    n.Id,
                    n.Type,
                    n.Content,
                    n.Data,
                    n.IsRead,
                    n.CreatedAt,
                    FromUser = n.FromUser != null ? new
                    {
                        n.FromUser.Id,
                        n.FromUser.Username,
                        n.FromUser.ProfilePhotoUrl
                    } : null
                })
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId);

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new
            {
                success = true,
                notifications = notifications,
                totalCount = totalCount,
                unreadCount = unreadCount,
                currentPage = page,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Bildirimler getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new
            {
                success = true,
                unreadCount = unreadCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Okunmamış bildirim sayısı getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("{notificationId}/mark-read")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📖 Bildirim okundu olarak işaretleniyor - NotificationId: {notificationId}");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Bildirim okundu olarak işaretlendi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Bildirim okuma işaretleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📖 Tüm bildirimler okundu olarak işaretleniyor - UserId: {userId}");

            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(p => p.IsRead, true));

            return Ok(new
            {
                success = true,
                message = "Tüm bildirimler okundu olarak işaretlendi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Tüm bildirimleri okuma işaretleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(int notificationId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"🗑️ Bildirim siliniyor - NotificationId: {notificationId}");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
            {
                return NotFound(new { success = false, message = "Bildirim bulunamadı." });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Bildirim silindi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Bildirim silme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }
}