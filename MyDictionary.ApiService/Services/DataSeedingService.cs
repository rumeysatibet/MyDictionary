using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;

namespace MyDictionary.ApiService.Services;

public class DataSeedingService
{
    private readonly DictionaryDbContext _context;
    private readonly ILogger<DataSeedingService> _logger;

    public DataSeedingService(DictionaryDbContext context, ILogger<DataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDataAsync()
    {
        try
        {
            // Veritabanının hazır olup olmadığını kontrol et
            await _context.Database.EnsureCreatedAsync();

            // UserAgreement seed
            await SeedUserAgreementsAsync();

            await _context.SaveChangesAsync();
            _logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
        }
    }

    private async Task SeedUserAgreementsAsync()
    {
        // Eğer aktif sözleşme yoksa, varsayılan sözleşme ekle
        var existingAgreement = await _context.UserAgreements
            .FirstOrDefaultAsync(ua => ua.IsActive);

        if (existingAgreement == null)
        {
            var userAgreement = new UserAgreement
            {
                Title = "MyDictionary Kullanıcı Sözleşmesi",
                Version = "1.0",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Content = @"# MyDictionary Kullanıcı Sözleşmesi

## 1. Genel Koşullar
MyDictionary platformunu kullanarak aşağıdaki şartları kabul etmiş olursunuz:

### 1.1 Kullanım Koşulları
- Platform sadece 18 yaş ve üzeri kullanıcılar tarafından kullanılabilir
- Hesabınızın güvenliğinden siz sorumlusunuz
- Şifrenizi kimseyle paylaşmamalısınız

### 1.2 İçerik Kuralları
- Uygunsuz, hakaret içeren veya yanıltıcı içerik paylaşmak yasaktır
- Telif hakkı ihlali yapan içerikler kaldırılacaktır
- Spam ve reklam içerikleri yasaktır

### 1.3 Gizlilik
- Kişisel bilgileriniz gizlilik politikamız çerçevesinde korunmaktadır
- E-posta adresiniz sadece platform bildirimleri için kullanılır
- Üçüncü taraflarla kişisel bilgilerinizi paylaşmayız

### 1.4 Sorumluluk
- Platform kullanımından doğan sorumluluk kullanıcıya aittir
- Teknik arızalar ve veri kayıplarından platform sorumlu değildir
- Hizmet kesintileri önceden bildirilmeye çalışılır

### 1.5 Değişiklikler
- Bu sözleşme herhangi bir zamanda güncellenebilir
- Önemli değişiklikler e-posta ile bildirilir
- Güncellemeler yayınlandığı tarihten itibaren geçerlidir

## 2. Kabul
Bu sözleşmeyi kabul ederek yukarıdaki tüm koşulları anladığınızı ve kabul ettiğinizi beyan edersiniz.

**Son Güncelleme:** {DateTime.UtcNow:dd.MM.yyyy}
**Sürüm:** 1.0"
            };

            _context.UserAgreements.Add(userAgreement);
            _logger.LogInformation("Default user agreement added");
        }
    }
}