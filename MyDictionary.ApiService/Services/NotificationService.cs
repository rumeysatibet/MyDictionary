using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Text.Json;

namespace MyDictionary.ApiService.Services;

public interface INotificationService
{
    Task CreateFriendRequestNotificationAsync(int receiverId, int senderId);
    Task CreateFriendRequestAcceptedNotificationAsync(int senderId, int accepterId);
    Task CreateMessageNotificationAsync(int receiverId, int senderId, string messagePreview);
    Task CreateSystemNotificationAsync(int userId, string content);
}

public class NotificationService : INotificationService
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(DictionaryDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateFriendRequestNotificationAsync(int receiverId, int senderId)
    {
        try
        {
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null) return;

            var notification = new Notification
            {
                UserId = receiverId,
                FromUserId = senderId,
                Type = NotificationType.FriendRequest,
                Content = $"{sender.Username} size arkadaşlık isteği gönderdi.",
                Data = JsonSerializer.Serialize(new { senderId = senderId, senderUsername = sender.Username }),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"🔔 Arkadaşlık isteği bildirimi oluşturuldu - Receiver: {receiverId}, Sender: {senderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık isteği bildirimi oluşturma hatası: {ex.Message}");
        }
    }

    public async Task CreateFriendRequestAcceptedNotificationAsync(int senderId, int accepterId)
    {
        try
        {
            var accepter = await _context.Users.FindAsync(accepterId);
            if (accepter == null) return;

            var notification = new Notification
            {
                UserId = senderId,
                FromUserId = accepterId,
                Type = NotificationType.FriendRequestAccepted,
                Content = $"{accepter.Username} arkadaşlık isteğinizi kabul etti.",
                Data = JsonSerializer.Serialize(new { accepterId = accepterId, accepterUsername = accepter.Username }),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"🔔 Arkadaşlık kabul bildirimi oluşturuldu - Sender: {senderId}, Accepter: {accepterId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Arkadaşlık kabul bildirimi oluşturma hatası: {ex.Message}");
        }
    }

    public async Task CreateMessageNotificationAsync(int receiverId, int senderId, string messagePreview)
    {
        try
        {
            var sender = await _context.Users.FindAsync(senderId);
            if (sender == null) return;

            var notification = new Notification
            {
                UserId = receiverId,
                FromUserId = senderId,
                Type = NotificationType.NewMessage,
                Content = $"{sender.Username} size yeni bir mesaj gönderdi: {messagePreview}",
                Data = JsonSerializer.Serialize(new { senderId = senderId, senderUsername = sender.Username, messagePreview = messagePreview }),
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"🔔 Mesaj bildirimi oluşturuldu - Receiver: {receiverId}, Sender: {senderId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Mesaj bildirimi oluşturma hatası: {ex.Message}");
        }
    }

    public async Task CreateSystemNotificationAsync(int userId, string content)
    {
        try
        {
            var notification = new Notification
            {
                UserId = userId,
                FromUserId = null,
                Type = NotificationType.System,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"🔔 Sistem bildirimi oluşturuldu - User: {userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"❌ Sistem bildirimi oluşturma hatası: {ex.Message}");
        }
    }
}