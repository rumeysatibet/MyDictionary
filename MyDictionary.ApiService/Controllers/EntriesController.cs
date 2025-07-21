using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Text.RegularExpressions;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntriesController : ControllerBase
{
    private readonly DictionaryDbContext _context;

    public EntriesController(DictionaryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetEntries(int? topicId = null, int? userId = null, int page = 1, int pageSize = 20)
    {
        var query = _context.Entries
            .Include(e => e.CreatedByUser)
            .Include(e => e.Topic)
                .ThenInclude(t => t.Category)
            .AsQueryable();

        if (topicId.HasValue)
            query = query.Where(e => e.TopicId == topicId.Value);

        if (userId.HasValue)
            query = query.Where(e => e.CreatedByUserId == userId.Value);

        var entries = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new
            {
                e.Id,
                e.Content,
                e.ContentHtml,
                Topic = new { e.Topic!.Title, e.Topic.Slug },
                Category = new { e.Topic.Category!.Name, e.Topic.Category.Color },
                CreatedBy = e.CreatedByUser!.Username,
                e.CreatedAt,
                e.UpdatedAt,
                e.FavoriteCount,
                e.IsEdited
            })
            .ToListAsync();

        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetEntry(int id)
    {
        var entry = await _context.Entries
            .Include(e => e.CreatedByUser)
            .Include(e => e.Topic)
                .ThenInclude(t => t.Category)
            .Include(e => e.Links)
            .Where(e => e.Id == id)
            .Select(e => new
            {
                e.Id,
                e.Content,
                e.ContentHtml,
                Topic = new { e.Topic!.Title, e.Topic.Slug },
                Category = new { e.Topic.Category!.Name, e.Topic.Category.Color },
                CreatedBy = e.CreatedByUser!.Username,
                e.CreatedAt,
                e.UpdatedAt,
                e.FavoriteCount,
                e.IsEdited,
                Links = e.Links.Select(l => new
                {
                    l.Id,
                    l.Url,
                    l.DisplayText,
                    l.Position
                })
            })
            .FirstOrDefaultAsync();

        if (entry == null)
            return NotFound();

        return Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateEntry(CreateEntryRequest request)
    {
        Console.WriteLine($"[ENTRY DEBUG] Creating entry - TopicId: {request.TopicId}, UserId: {request.CreatedByUserId}, Content: {request.Content[..Math.Min(50, request.Content.Length)]}...");
        
        if (string.IsNullOrWhiteSpace(request.Content) || request.TopicId <= 0 || request.CreatedByUserId <= 0)
            return BadRequest("Content, TopicId ve CreatedByUserId gerekli.");

        // HTML içeriği oluştur ve linkleri işle
        var (contentHtml, links) = ProcessEntryContent(request.Content);
        Console.WriteLine($"[ENTRY DEBUG] Processed HTML: {contentHtml}");

        var entry = new Entry
        {
            Content = request.Content.Trim(),
            ContentHtml = contentHtml,
            TopicId = request.TopicId,
            CreatedByUserId = request.CreatedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Entries.Add(entry);
        await _context.SaveChangesAsync();

        Console.WriteLine($"[ENTRY DEBUG] Entry saved with ID: {entry.Id}");

        // Linkleri kaydet
        foreach (var link in links)
        {
            link.EntryId = entry.Id;
            _context.EntryLinks.Add(link);
        }

        // Topic'in entry count ve last entry date güncelle
        var topic = await _context.Topics.FindAsync(request.TopicId);
        if (topic != null)
        {
            topic.EntryCount++;
            topic.LastEntryAt = DateTime.UtcNow;
            Console.WriteLine($"[ENTRY DEBUG] Updated topic '{topic.Title}' - new entry count: {topic.EntryCount}");
        }

        // User'ın entry count'ını artır
        var user = await _context.Users.FindAsync(request.CreatedByUserId);
        if (user != null)
        {
            user.EntryCount++;
            Console.WriteLine($"[ENTRY DEBUG] Updated user '{user.Username}' - new entry count: {user.EntryCount}");
        }

        await _context.SaveChangesAsync();

        var result = new
        {
            entry.Id,
            entry.Content,
            entry.ContentHtml,
            entry.TopicId,
            entry.CreatedByUserId,
            entry.CreatedAt
        };

        return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateEntry(int id, UpdateEntryRequest request)
    {
        var entry = await _context.Entries
            .Include(e => e.Links)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry == null)
            return NotFound();

        if (entry.CreatedByUserId != request.UserId)
            return Forbid("Bu entry'yi sadece yazarı düzenleyebilir.");

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content gerekli.");

        // Eski linkleri sil
        _context.EntryLinks.RemoveRange(entry.Links);

        // HTML içeriği oluştur ve linkleri işle
        var (contentHtml, links) = ProcessEntryContent(request.Content);

        entry.Content = request.Content.Trim();
        entry.ContentHtml = contentHtml;
        entry.UpdatedAt = DateTime.UtcNow;
        entry.IsEdited = true;

        // Yeni linkleri kaydet
        foreach (var link in links)
        {
            link.EntryId = entry.Id;
            _context.EntryLinks.Add(link);
        }

        await _context.SaveChangesAsync();

        var result = new
        {
            entry.Id,
            entry.Content,
            entry.ContentHtml,
            entry.UpdatedAt,
            entry.IsEdited
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEntry(int id, [FromQuery] int userId)
    {
        var entry = await _context.Entries.FindAsync(id);
        if (entry == null)
            return NotFound();

        if (entry.CreatedByUserId != userId)
            return Forbid("Bu entry'yi sadece yazarı silebilir.");

        _context.Entries.Remove(entry);

        // Topic'in entry count'ını azalt
        var topic = await _context.Topics.FindAsync(entry.TopicId);
        if (topic != null)
        {
            topic.EntryCount = Math.Max(0, topic.EntryCount - 1);
        }

        // User'ın entry count'ını azalt
        var user = await _context.Users.FindAsync(entry.CreatedByUserId);
        if (user != null)
        {
            user.EntryCount = Math.Max(0, user.EntryCount - 1);
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static (string contentHtml, List<EntryLink> links) ProcessEntryContent(string content)
    {
        var links = new List<EntryLink>();
        var contentHtml = content;

        // URL pattern'ı bul
        var urlPattern = @"https?://[^\s]+";
        var matches = Regex.Matches(content, urlPattern);

        int position = 0;
        foreach (Match match in matches)
        {
            var url = match.Value;
            var link = new EntryLink
            {
                Url = url,
                DisplayText = "bakınız",
                Position = position++
            };
            links.Add(link);

            // HTML'de linki "(bakınız)" ile değiştir
            contentHtml = contentHtml.Replace(url, $"<span class=\"entry-link\" data-url=\"{url}\">(bakınız)</span>");
        }

        // Basit HTML formatlaması
        contentHtml = contentHtml.Replace("\n", "<br>");

        return (contentHtml, links);
    }
}

public class CreateEntryRequest
{
    public string Content { get; set; } = string.Empty;
    public int TopicId { get; set; }
    public int CreatedByUserId { get; set; }
}

public class UpdateEntryRequest
{
    public string Content { get; set; } = string.Empty;
    public int UserId { get; set; }
}