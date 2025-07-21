using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly DictionaryDbContext _context;

    public CategoriesController(DictionaryDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetCategories()
    {
        var categories = await _context.Categories
            .Include(c => c.Topics)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Icon,
                c.Color,
                TopicCount = c.Topics.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("with-stats")]
    public async Task<ActionResult<IEnumerable<object>>> GetCategoriesWithStats()
    {
        var categories = await _context.Categories
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Icon,
                c.Color,
                TopicCount = c.Topics.Count,
                TotalEntries = c.Topics.Sum(t => t.EntryCount)
            })
            .Where(c => c.TopicCount > 0) // Sadece topic'i olan kategoriler
            .OrderByDescending(c => c.TotalEntries)
            .ToListAsync();

        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<object>> GetCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.Topics)
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Slug,
                c.Description,
                c.Icon,
                c.Color,
                Topics = c.Topics.Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Slug,
                    t.EntryCount,
                    t.ViewCount,
                    t.CreatedAt,
                    t.LastEntryAt
                })
            })
            .FirstOrDefaultAsync();

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<object>> GetCategoryBySlug(string slug)
    {
        Console.WriteLine($"[CATEGORY DEBUG] Getting category by slug: {slug}");
        
        var category = await _context.Categories
            .Include(c => c.Topics)
            .Where(c => c.Slug == slug)
            .FirstOrDefaultAsync();

        if (category == null)
        {
            Console.WriteLine($"[CATEGORY DEBUG] Category not found: {slug}");
            return NotFound(new { success = false, message = "Kategori bulunamadı" });
        }

        Console.WriteLine($"[CATEGORY DEBUG] Found category: {category.Name}, Topic count: {category.Topics.Count}");

        var result = new
        {
            success = true,
            category = new
            {
                category.Id,
                category.Name,
                category.Slug,
                category.Description,
                category.Icon,
                category.Color,
                TopicCount = category.Topics.Count
            }
        };

        return Ok(result);
    }

    [HttpGet("{slug}/topics")]
    public async Task<ActionResult<object>> GetCategoryTopics(string slug, int page = 1, int pageSize = 20, string sortBy = "latest")
    {
        Console.WriteLine($"[CATEGORY DEBUG] Getting topics for category: {slug}, page: {page}, sortBy: {sortBy}");
        
        var category = await _context.Categories
            .Where(c => c.Slug == slug)
            .FirstOrDefaultAsync();

        if (category == null)
        {
            Console.WriteLine($"[CATEGORY DEBUG] Category not found for topics: {slug}");
            return NotFound(new { success = false, message = "Kategori bulunamadı" });
        }

        var query = _context.Topics
            .Include(t => t.CreatedByUser)
            .Where(t => t.CategoryId == category.Id);

        Console.WriteLine($"[CATEGORY DEBUG] Found category for topics: {category.Name} (ID: {category.Id})");

        // Sıralama
        query = sortBy switch
        {
            "popular" => query.OrderByDescending(t => t.ViewCount),
            "entries" => query.OrderByDescending(t => t.EntryCount),
            _ => query.OrderByDescending(t => t.CreatedAt) // latest
        };

        var totalCount = await query.CountAsync();
        var topics = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.Id,
                t.Title,
                t.Slug,
                CreatedByUsername = t.CreatedByUser.Username,
                t.CreatedAt,
                t.LastEntryAt,
                t.EntryCount,
                t.ViewCount
            })
            .ToListAsync();

        // Toplam entry sayısını hesapla
        var totalEntries = await _context.Entries
            .Join(_context.Topics, e => e.TopicId, t => t.Id, (e, t) => new { e, t })
            .Where(x => x.t.CategoryId == category.Id)
            .CountAsync();

        Console.WriteLine($"[CATEGORY DEBUG] Returning {topics.Count} topics, total entries: {totalEntries}");

        var result = new
        {
            success = true,
            topics = topics,
            totalEntries = totalEntries,
            hasMore = (page * pageSize) < totalCount,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            currentPage = page
        };

        return Ok(result);
    }
}