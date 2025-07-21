using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FavoritesController : ControllerBase
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<FavoritesController> _logger;

    public FavoritesController(DictionaryDbContext context, ILogger<FavoritesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetUserFavorites(int userId, int page = 1, int pageSize = 20)
    {
        var favorites = await _context.EntryFavorites
            .Include(ef => ef.Entry)
                .ThenInclude(e => e.Topic)
                    .ThenInclude(t => t.Category)
            .Include(ef => ef.Entry)
                .ThenInclude(e => e.CreatedByUser)
            .Where(ef => ef.UserId == userId)
            .OrderByDescending(ef => ef.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ef => new
            {
                ef.Id,
                ef.CreatedAt,
                Entry = new
                {
                    ef.Entry!.Id,
                    ef.Entry.Content,
                    ef.Entry.ContentHtml,
                    Topic = new { ef.Entry.Topic!.Title, ef.Entry.Topic.Slug },
                    Category = new { ef.Entry.Topic.Category!.Name, ef.Entry.Topic.Category.Color },
                    CreatedBy = ef.Entry.CreatedByUser!.Username,
                    ef.Entry.CreatedAt,
                    ef.Entry.FavoriteCount
                }
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpPost]
    public async Task<ActionResult<object>> AddFavorite(AddFavoriteRequest request)
    {
        if (request.UserId <= 0 || request.EntryId <= 0)
            return BadRequest("UserId ve EntryId gerekli.");

        // Zaten favorilere eklenmiş mi kontrol et
        var existingFavorite = await _context.EntryFavorites
            .FirstOrDefaultAsync(ef => ef.UserId == request.UserId && ef.EntryId == request.EntryId);

        if (existingFavorite != null)
            return BadRequest("Bu entry zaten favorilerde.");

        // Entry'nin var olduğunu kontrol et
        var entry = await _context.Entries.FindAsync(request.EntryId);
        if (entry == null)
            return NotFound("Entry bulunamadı.");

        var favorite = new EntryFavorite
        {
            UserId = request.UserId,
            EntryId = request.EntryId,
            CreatedAt = DateTime.UtcNow
        };

        _context.EntryFavorites.Add(favorite);

        // Entry'nin favorite count'ını artır
        entry.FavoriteCount++;

        await _context.SaveChangesAsync();

        var result = new
        {
            favorite.Id,
            favorite.UserId,
            favorite.EntryId,
            favorite.CreatedAt
        };

        return CreatedAtAction(nameof(GetUserFavorites), new { userId = request.UserId }, result);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveFavorite(RemoveFavoriteRequest request)
    {
        _logger.LogInformation($"🗑️ RemoveFavorite API çağrıldı - UserId: {request?.UserId}, EntryId: {request?.EntryId}");
        
        if (request == null)
        {
            _logger.LogError("❌ Request null geldi");
            return BadRequest("Request boş olamaz.");
        }
        
        if (request.UserId <= 0 || request.EntryId <= 0)
        {
            _logger.LogError($"❌ Geçersiz parametreler - UserId: {request.UserId}, EntryId: {request.EntryId}");
            return BadRequest("UserId ve EntryId gerekli.");
        }

        _logger.LogInformation($"🔍 Favori aranıyor - UserId: {request.UserId}, EntryId: {request.EntryId}");
        var favorite = await _context.EntryFavorites
            .FirstOrDefaultAsync(ef => ef.UserId == request.UserId && ef.EntryId == request.EntryId);

        if (favorite == null)
        {
            _logger.LogWarning($"❌ Favori bulunamadı - UserId: {request.UserId}, EntryId: {request.EntryId}");
            return NotFound("Favori bulunamadı.");
        }

        _logger.LogInformation($"✅ Favori bulundu, siliniyor - FavoriteId: {favorite.Id}");
        _context.EntryFavorites.Remove(favorite);

        // Entry'nin favorite count'ını azalt
        var entry = await _context.Entries.FindAsync(request.EntryId);
        if (entry != null)
        {
            var oldCount = entry.FavoriteCount;
            entry.FavoriteCount = Math.Max(0, entry.FavoriteCount - 1);
            _logger.LogInformation($"📊 Entry favorite count güncellendi - EntryId: {request.EntryId}, {oldCount} -> {entry.FavoriteCount}");
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"💾 Veritabanı kaydedildi");
        return NoContent();
    }

    [HttpGet("check")]
    public async Task<ActionResult<object>> CheckFavorite(int userId, int entryId)
    {
        if (userId <= 0 || entryId <= 0)
            return BadRequest("UserId ve EntryId gerekli.");

        var favorite = await _context.EntryFavorites
            .FirstOrDefaultAsync(ef => ef.UserId == userId && ef.EntryId == entryId);

        return Ok(new { IsFavorited = favorite != null });
    }

    [HttpGet("entry/{entryId}/count")]
    public async Task<ActionResult<object>> GetFavoriteCount(int entryId)
    {
        var count = await _context.EntryFavorites
            .CountAsync(ef => ef.EntryId == entryId);

        return Ok(new { Count = count });
    }
}

public class AddFavoriteRequest
{
    public int UserId { get; set; }
    public int EntryId { get; set; }
}

public class RemoveFavoriteRequest
{
    public int UserId { get; set; }
    public int EntryId { get; set; }
}