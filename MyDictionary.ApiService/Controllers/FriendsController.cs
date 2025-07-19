using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using MyDictionary.ApiService.Services;
using System.Security.Claims;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<FriendsController> _logger;
    private readonly INotificationService _notificationService;

    public FriendsController(DictionaryDbContext context, ILogger<FriendsController> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpPost("send-request")]
    public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto request)
    {
        try
        {
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"👋 Arkadaşlık isteği gönderiliyor - From: {senderId}, To: {request.ReceiverId}");

            if (senderId == request.ReceiverId)
            {
                return BadRequest(new { success = false, message = "Kendinize arkadaşlık isteği gönderemezsiniz." });
            }

            // Alıcının var olup olmadığını kontrol et
            var receiver = await _context.Users.FindAsync(request.ReceiverId);
            if (receiver == null)
            {
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Zaten arkadaş mı?
            var existingFriendship = await _context.Friendships
                .AnyAsync(f => (f.UserId == senderId && f.FriendId == request.ReceiverId) ||
                              (f.UserId == request.ReceiverId && f.FriendId == senderId));

            if (existingFriendship)
            {
                return BadRequest(new { success = false, message = "Bu kullanıcı zaten arkadaşınız." });
            }

            // Bekleyen istek var mı?
            var existingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => 
                    ((fr.SenderId == senderId && fr.ReceiverId == request.ReceiverId) ||
                     (fr.SenderId == request.ReceiverId && fr.ReceiverId == senderId)) &&
                    fr.Status == FriendRequestStatus.Pending);

            if (existingRequest != null)
            {
                if (existingRequest.SenderId == senderId)
                {
                    return BadRequest(new { success = false, message = "Bu kullanıcıya zaten arkadaşlık isteği gönderdiniz." });
                }
                else
                {
                    // Karşı taraftan gelen isteği kabul et
                    return await AcceptFriendRequest(existingRequest.Id);
                }
            }

            // Yeni arkadaşlık isteği oluştur
            var friendRequest = new FriendRequest
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Status = FriendRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.FriendRequests.Add(friendRequest);
            await _context.SaveChangesAsync();

            // Bildirim gönder
            await _notificationService.CreateFriendRequestNotificationAsync(request.ReceiverId, senderId);

            _logger.LogInformation($"✅ Arkadaşlık isteği gönderildi - ID: {friendRequest.Id}");

            return Ok(new
            {
                success = true,
                message = "Arkadaşlık isteği gönderildi.",
                requestId = friendRequest.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık isteği gönderme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("requests")]
    public async Task<IActionResult> GetFriendRequests()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"📋 Arkadaşlık istekleri getiriliyor - UserId: {userId}");

            var incomingRequests = await _context.FriendRequests
                .Where(fr => fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Sender)
                .Select(fr => new
                {
                    fr.Id,
                    fr.CreatedAt,
                    Sender = new
                    {
                        fr.Sender.Id,
                        fr.Sender.Username,
                        fr.Sender.ProfilePhotoUrl,
                        fr.Sender.EntryCount
                    }
                })
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            var outgoingRequests = await _context.FriendRequests
                .Where(fr => fr.SenderId == userId && fr.Status == FriendRequestStatus.Pending)
                .Include(fr => fr.Receiver)
                .Select(fr => new
                {
                    fr.Id,
                    fr.CreatedAt,
                    Receiver = new
                    {
                        fr.Receiver.Id,
                        fr.Receiver.Username,
                        fr.Receiver.ProfilePhotoUrl,
                        fr.Receiver.EntryCount
                    }
                })
                .OrderByDescending(fr => fr.CreatedAt)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                incomingRequests = incomingRequests,
                outgoingRequests = outgoingRequests
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık istekleri getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("{requestId}/accept")]
    public async Task<IActionResult> AcceptFriendRequest(int requestId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"✅ Arkadaşlık isteği kabul ediliyor - RequestId: {requestId}, UserId: {userId}");

            var friendRequest = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending);

            if (friendRequest == null)
            {
                return NotFound(new { success = false, message = "Arkadaşlık isteği bulunamadı." });
            }

            // İsteği kabul et
            friendRequest.Status = FriendRequestStatus.Accepted;

            // İki yönlü arkadaşlık oluştur
            var friendship1 = new Friendship
            {
                UserId = friendRequest.SenderId,
                FriendId = friendRequest.ReceiverId,
                CreatedAt = DateTime.UtcNow
            };

            var friendship2 = new Friendship
            {
                UserId = friendRequest.ReceiverId,
                FriendId = friendRequest.SenderId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.AddRange(friendship1, friendship2);

            // Kullanıcıların takipçi sayısını güncelle
            var sender = await _context.Users.FindAsync(friendRequest.SenderId);
            var receiver = await _context.Users.FindAsync(friendRequest.ReceiverId);

            if (sender != null) sender.FollowerCount++;
            if (receiver != null) receiver.FollowerCount++;

            await _context.SaveChangesAsync();

            // Arkadaşlık kabul bildirimi gönder
            await _notificationService.CreateFriendRequestAcceptedNotificationAsync(friendRequest.SenderId, friendRequest.ReceiverId);

            _logger.LogInformation($"✅ Arkadaşlık kabul edildi - {friendRequest.Sender.Username} ve {friendRequest.Receiver.Username}");

            return Ok(new
            {
                success = true,
                message = "Arkadaşlık isteği kabul edildi!",
                friendship = new
                {
                    friendRequest.Sender.Id,
                    friendRequest.Sender.Username,
                    friendRequest.Sender.ProfilePhotoUrl
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık isteği kabul etme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("{requestId}/reject")]
    public async Task<IActionResult> RejectFriendRequest(int requestId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"❌ Arkadaşlık isteği reddediliyor - RequestId: {requestId}, UserId: {userId}");

            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == requestId && fr.ReceiverId == userId && fr.Status == FriendRequestStatus.Pending);

            if (friendRequest == null)
            {
                return NotFound(new { success = false, message = "Arkadaşlık isteği bulunamadı." });
            }

            friendRequest.Status = FriendRequestStatus.Rejected;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"❌ Arkadaşlık isteği reddedildi - RequestId: {requestId}");

            return Ok(new
            {
                success = true,
                message = "Arkadaşlık isteği reddedildi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık isteği reddetme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"👥 Arkadaş listesi getiriliyor - UserId: {userId}");

            var friends = await _context.Friendships
                .Where(f => f.UserId == userId)
                .Include(f => f.Friend)
                .Select(f => new
                {
                    f.Friend.Id,
                    f.Friend.Username,
                    f.Friend.ProfilePhotoUrl,
                    f.Friend.EntryCount,
                    f.Friend.LastLoginAt,
                    f.CreatedAt
                })
                .OrderBy(f => f.Username)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                friends = friends,
                totalCount = friends.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaş listesi getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }
}

public class SendFriendRequestDto
{
    public int ReceiverId { get; set; }
}