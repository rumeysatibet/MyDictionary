using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly DictionaryDbContext _context;

    public TopicsController(DictionaryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTopics(int? categoryId = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Topics
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        var topics = await query
            .OrderByDescending(t => t.LastEntryAt ?? t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Slug,
                Category = new { t.Category!.Name, t.Category.Color },
                CreatedBy = t.CreatedByUser!.Username,
                t.EntryCount,
                t.ViewCount,
                t.CreatedAt,
                t.LastEntryAt
            })
            .ToListAsync();

        return Ok(topics);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTopic(int id)
    {
        var topic = await _context.Topics
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Entries)
                .ThenInclude(e => e.CreatedByUser)
            .Where(t => t.Id == id)
            .FirstOrDefaultAsync();

        if (topic == null)
            return NotFound();

        // Görüntülenme sayısını artır
        topic.ViewCount++;
        await _context.SaveChangesAsync();

        var result = new
        {
            topic.Id,
            topic.Title,
            topic.Slug,
            Category = new { topic.Category!.Name, topic.Category.Color },
            CreatedBy = topic.CreatedByUser!.Username,
            topic.EntryCount,
            topic.ViewCount,
            topic.CreatedAt,
            topic.LastEntryAt,
            Entries = topic.Entries.Select(e => new
            {
                e.Id,
                e.Content,
                e.ContentHtml,
                CreatedBy = e.CreatedByUser!.Username,
                e.CreatedAt,
                e.UpdatedAt,
                e.FavoriteCount,
                e.IsEdited
            })
        };

        return Ok(result);
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<object>> GetTopicBySlug(string slug)
    {
        var topic = await _context.Topics
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Entries)
                .ThenInclude(e => e.CreatedByUser)
            .Where(t => t.Slug == slug)
            .FirstOrDefaultAsync();

        if (topic == null)
            return NotFound();

        Console.WriteLine($"[TOPIC DEBUG] Topic: {topic.Title}, Entry Count: {topic.Entries.Count}");

        // Görüntülenme sayısını artır
        topic.ViewCount++;
        await _context.SaveChangesAsync();

        var result = new
        {
            topic.Id,
            topic.Title,
            topic.Slug,
            Category = new { topic.Category!.Name, topic.Category.Color },
            CreatedBy = topic.CreatedByUser!.Username,
            topic.EntryCount,
            topic.ViewCount,
            topic.CreatedAt,
            topic.LastEntryAt,
            Entries = topic.Entries.OrderByDescending(e => e.CreatedAt).Select(e => new
            {
                e.Id,
                e.Content,
                e.ContentHtml,
                CreatedBy = e.CreatedByUser!.Username,
                e.CreatedAt,
                e.UpdatedAt,
                e.FavoriteCount,
                e.IsEdited
            })
        };

        Console.WriteLine($"[TOPIC DEBUG] Returning {result.Entries.Count()} entries");

        return Ok(result);
    }

    [HttpGet("popular")]
    public async Task<ActionResult<IEnumerable<object>>> GetPopularTopics(int count = 10)
    {
        var popularTopics = await _context.Topics
            .Include(t => t.Category)
            .Include(t => t.CreatedByUser)
            .Where(t => t.EntryCount > 0) // Sadece entry'si olan topic'ler
            .OrderByDescending(t => t.EntryCount)
            .ThenByDescending(t => t.ViewCount)
            .Take(count)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Slug,
                Category = new { t.Category!.Name, t.Category.Color },
                CreatedBy = t.CreatedByUser!.Username,
                t.EntryCount,
                t.ViewCount,
                t.CreatedAt,
                t.LastEntryAt
            })
            .ToListAsync();

        return Ok(popularTopics);
    }

    [HttpGet("categories/{categoryId}/stats")]
    public async Task<ActionResult<object>> GetCategoryStats(int categoryId)
    {
        var stats = await _context.Topics
            .Where(t => t.CategoryId == categoryId)
            .GroupBy(t => t.CategoryId)
            .Select(g => new
            {
                CategoryId = g.Key,
                TopicCount = g.Count(),
                TotalEntries = g.Sum(t => t.EntryCount),
                TotalViews = g.Sum(t => t.ViewCount)
            })
            .FirstOrDefaultAsync();

        if (stats == null)
        {
            return Ok(new { CategoryId = categoryId, TopicCount = 0, TotalEntries = 0, TotalViews = 0 });
        }

        return Ok(stats);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTopic(CreateTopicRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.CategoryId <= 0 || request.CreatedByUserId <= 0)
            return BadRequest("Title, CategoryId ve CreatedByUserId gerekli.");

        // Slug oluştur
        var slug = GenerateSlug(request.Title);
        
        // Slug'ın benzersiz olduğunu kontrol et
        var existingTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Slug == slug);
        if (existingTopic != null)
        {
            slug = $"{slug}-{DateTime.UtcNow.Ticks}";
        }

        var topic = new Topic
        {
            Title = request.Title.Trim(),
            Slug = slug,
            CategoryId = request.CategoryId,
            CreatedByUserId = request.CreatedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Topics.Add(topic);
        
        // User'ın topic count'ını artır
        var user = await _context.Users.FindAsync(request.CreatedByUserId);
        if (user != null)
        {
            user.TopicCount++;
        }

        await _context.SaveChangesAsync();

        var result = new
        {
            topic.Id,
            topic.Title,
            topic.Slug,
            topic.CategoryId,
            topic.CreatedByUserId,
            topic.CreatedAt
        };

        return CreatedAtAction(nameof(GetTopic), new { id = topic.Id }, result);
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace("ç", "c")
            .Replace("ğ", "g")
            .Replace("ı", "i")
            .Replace("ö", "o")
            .Replace("ş", "s")
            .Replace("ü", "u")
            .Replace(" ", "-")
            .Replace("?", "")
            .Replace("!", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace(":", "")
            .Replace(";", "");

        return slug;
    }
}

public class CreateTopicRequest
{
    public string Title { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int CreatedByUserId { get; set; }
}