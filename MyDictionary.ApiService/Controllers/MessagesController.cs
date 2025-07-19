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
public class MessagesController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<MessagesController> _logger;
    private readonly INotificationService _notificationService;

    public MessagesController(DictionaryDbContext context, ILogger<MessagesController> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"💬 Konuşmalar getiriliyor - UserId: {userId}");

            var conversations = await _context.Conversations
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.LastMessage)
                .OrderByDescending(c => c.LastMessageAt)
                .Select(c => new
                {
                    c.Id,
                    c.LastMessageAt,
                    OtherUser = c.User1Id == userId ? new
                    {
                        c.User2.Id,
                        c.User2.Username,
                        c.User2.ProfilePhotoUrl
                    } : new
                    {
                        c.User1.Id,
                        c.User1.Username,
                        c.User1.ProfilePhotoUrl
                    },
                    LastMessage = c.LastMessage != null ? new
                    {
                        c.LastMessage.Content,
                        c.LastMessage.CreatedAt,
                        c.LastMessage.IsRead,
                        c.LastMessage.SenderId
                    } : null,
                    UnreadCount = _context.Messages.Count(m => 
                        m.ReceiverId == userId && 
                        !m.IsRead && 
                        ((c.User1Id == userId && m.SenderId == c.User2Id) || 
                         (c.User2Id == userId && m.SenderId == c.User1Id)))
                })
                .ToListAsync();

            return Ok(new
            {
                success = true,
                conversations = conversations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Konuşmalar getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("conversation/{otherUserId}")]
    public async Task<IActionResult> GetConversation(int otherUserId, int page = 1, int pageSize = 50)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"💬 Konuşma getiriliyor - UserId: {userId}, OtherUserId: {otherUserId}");

            // Arkadaş kontrolü
            var areFriends = await _context.Friendships
                .AnyAsync(f => (f.UserId == userId && f.FriendId == otherUserId) ||
                              (f.UserId == otherUserId && f.FriendId == userId));

            if (!areFriends)
            {
                return BadRequest(new { success = false, message = "Sadece arkadaşlarınızla mesajlaşabilirsiniz." });
            }

            var messages = await _context.Messages
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId))
                .Include(m => m.Sender)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    m.Id,
                    m.Content,
                    m.CreatedAt,
                    m.IsRead,
                    m.ReadAt,
                    Sender = new
                    {
                        m.Sender.Id,
                        m.Sender.Username,
                        m.Sender.ProfilePhotoUrl
                    }
                })
                .ToListAsync();

            // Okunmamış mesajları okundu olarak işaretle
            var unreadMessages = await _context.Messages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();
            
            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                msg.ReadAt = DateTime.UtcNow;
            }
            
            if (unreadMessages.Any())
            {
                await _context.SaveChangesAsync();
            }

            // Diğer kullanıcı bilgilerini getir
            var otherUser = await _context.Users
                .Where(u => u.Id == otherUserId)
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.ProfilePhotoUrl
                })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                success = true,
                messages = messages.AsEnumerable().Reverse(), // En eski mesajdan en yeniye
                otherUser = otherUser,
                currentPage = page,
                hasMore = messages.Count == pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Konuşma getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
    {
        try
        {
            var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            _logger.LogInformation($"💬 Mesaj gönderiliyor - From: {senderId}, To: {request.ReceiverId}");

            if (senderId == request.ReceiverId)
            {
                return BadRequest(new { success = false, message = "Kendinize mesaj gönderemezsiniz." });
            }

            // Arkadaş kontrolü
            var areFriends = await _context.Friendships
                .AnyAsync(f => (f.UserId == senderId && f.FriendId == request.ReceiverId) ||
                              (f.UserId == request.ReceiverId && f.FriendId == senderId));

            if (!areFriends)
            {
                return BadRequest(new { success = false, message = "Sadece arkadaşlarınızla mesajlaşabilirsiniz." });
            }

            // Mesajı oluştur
            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = request.ReceiverId,
                Content = request.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Konuşmayı bul veya oluştur
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => 
                    (c.User1Id == senderId && c.User2Id == request.ReceiverId) ||
                    (c.User1Id == request.ReceiverId && c.User2Id == senderId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = Math.Min(senderId, request.ReceiverId),
                    User2Id = Math.Max(senderId, request.ReceiverId),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Conversations.Add(conversation);
            }

            conversation.LastMessageId = message.Id;
            conversation.LastMessageAt = message.CreatedAt;

            await _context.SaveChangesAsync();

            // Bildirim gönder
            var messagePreview = message.Content.Length > 50 ? message.Content.Substring(0, 50) + "..." : message.Content;
            await _notificationService.CreateMessageNotificationAsync(request.ReceiverId, senderId, messagePreview);

            _logger.LogInformation($"✅ Mesaj gönderildi - ID: {message.Id}");

            return Ok(new
            {
                success = true,
                message = "Mesaj gönderildi.",
                messageId = message.Id,
                createdAt = message.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Mesaj gönderme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadMessageCount()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var unreadCount = await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);

            return Ok(new
            {
                success = true,
                unreadCount = unreadCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Okunmamış mesaj sayısı getirme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }

    [HttpPost("{messageId}/mark-read")]
    public async Task<IActionResult> MarkMessageAsRead(int messageId)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var message = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == messageId && m.ReceiverId == userId);

            if (message == null)
            {
                return NotFound(new { success = false, message = "Mesaj bulunamadı." });
            }

            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Mesaj okundu olarak işaretlendi."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Mesaj okuma işaretleme hatası: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Sunucu hatası oluştu." });
        }
    }
}

public class SendMessageDto
{
    public int ReceiverId { get; set; }
    public string Content { get; set; } = "";
}