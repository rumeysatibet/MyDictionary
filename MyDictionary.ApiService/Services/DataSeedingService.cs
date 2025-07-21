using Microsoft.EntityFrameworkCore;
using MyDictionary.ApiService.Data;
using MyDictionary.ApiService.Models;
using System.Security.Cryptography;
using System.Text;

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

            // Categories seed
            await SeedCategoriesAsync();

            // Test users seed
            await SeedTestUsersAsync();

            // Topics and Entries seed
            await SeedTopicsAndEntriesAsync();

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

    private async Task SeedTestUsersAsync()
    {
        // Check if we already have users in the database
        var existingUserCount = await _context.Users.CountAsync();
        if (existingUserCount > 0)
        {
            _logger.LogInformation("Users already exist in database. Skipping test user seeding.");
            return;
        }

        _logger.LogInformation("No users found. Creating test users...");

        // Get active user agreement
        var activeAgreement = await _context.UserAgreements.FirstOrDefaultAsync(ua => ua.IsActive);
        if (activeAgreement == null)
        {
            _logger.LogWarning("No active user agreement found. Cannot create test users.");
            return;
        }

        // Test users to create
        var testUsers = new[]
        {
            new { Username = "esma", Email = "esma@example.com", Password = "Test123456!", Gender = Gender.Female, BirthDate = new DateTime(1995, 6, 15), About = "yazılım geliştirici ve teknoloji tutkunu", EntryCount = 25 },
            new { Username = "ahmet", Email = "ahmet@example.com", Password = "Test123456!", Gender = Gender.Male, BirthDate = new DateTime(1990, 3, 10), About = "full-stack developer", EntryCount = 42 },
            new { Username = "mehmet", Email = "mehmet@example.com", Password = "Test123456!", Gender = Gender.Male, BirthDate = new DateTime(1988, 11, 22), About = "backend developer ve devops uzmanı", EntryCount = 38 },
            new { Username = "zeynep", Email = "zeynep@example.com", Password = "Test123456!", Gender = Gender.Female, BirthDate = new DateTime(1993, 7, 8), About = "frontend developer ve ui/ux tasarımcı", EntryCount = 31 },
            new { Username = "socialtest", Email = "socialtest@example.com", Password = "Test123456!", Gender = Gender.NotSpecified, BirthDate = new DateTime(1992, 8, 22), About = "test kullanıcısı", EntryCount = 5 }
        };

        foreach (var testUser in testUsers)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == testUser.Username || u.Email == testUser.Email);

            if (existingUser != null)
            {
                _logger.LogInformation("Test user {Username} already exists, skipping.", testUser.Username);
                continue;
            }

            // Create new user
            var user = new User
            {
                Username = testUser.Username,
                Email = testUser.Email,
                PasswordHash = HashPassword(testUser.Password),
                BirthDate = testUser.BirthDate,
                Gender = testUser.Gender,
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                About = testUser.About,
                EntryCount = testUser.EntryCount,
                FollowerCount = Random.Shared.Next(5, 25),
                FollowingCount = Random.Shared.Next(3, 20)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // Save to get the user ID

            // Create user agreement acceptance
            var acceptance = new UserAgreementAcceptance
            {
                UserId = user.Id,
                AgreementId = activeAgreement.Id,
                AcceptedAt = DateTime.UtcNow,
                IpAddress = "127.0.0.1" // Default for seeded users
            };

            _context.UserAgreementAcceptances.Add(acceptance);
            
            _logger.LogInformation("Created test user: {Username} (ID: {UserId})", user.Username, user.Id);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Test user seeding completed.");
    }

    private async Task SeedCategoriesAsync()
    {
        var existingCategoryCount = await _context.Categories.CountAsync();
        if (existingCategoryCount > 0)
        {
            _logger.LogInformation("Categories already exist in database. Skipping category seeding.");
            return;
        }

        _logger.LogInformation("No categories found. Creating categories...");

        var categories = new[]
        {
            new Category { Name = "yazılım geliştirme", Slug = "yazilim-gelistirme", Description = "genel yazılım geliştirme konuları", Icon = "💻", Color = "#007acc" },
            new Category { Name = "web geliştirme", Slug = "web-gelistirme", Description = "frontend ve backend web teknolojileri", Icon = "🌐", Color = "#61dafb" },
            new Category { Name = "mobil geliştirme", Slug = "mobil-gelistirme", Description = "ios, android ve cross-platform mobil uygulamalar", Icon = "📱", Color = "#32de84" },
            new Category { Name = ".net platformu", Slug = "dotnet-platformu", Description = "c#, asp.net, .net core ve .net framework", Icon = "🟣", Color = "#512bd4" },
            new Category { Name = "blazor", Slug = "blazor", Description = "blazor server, blazor webassembly ve hybrid uygulamalar", Icon = "🔥", Color = "#ff6b35" },
            new Category { Name = "yapay zeka", Slug = "yapay-zeka", Description = "machine learning, deep learning ve ai uygulamaları", Icon = "🤖", Color = "#ff6b9d" },
            new Category { Name = "devops", Slug = "devops", Description = "ci/cd, containerization, cloud ve automation", Icon = "⚙️", Color = "#326ce5" },
            new Category { Name = "siber güvenlik", Slug = "siber-guvenlik", Description = "güvenlik açıkları, penetration testing ve güvenli kodlama", Icon = "🔐", Color = "#dc3545" },
            new Category { Name = "clean code", Slug = "clean-code", Description = "temiz kod yazma teknikleri ve best practices", Icon = "✨", Color = "#28a745" },
            new Category { Name = "design patterns", Slug = "design-patterns", Description = "yazılım tasarım kalıpları ve mimari desenler", Icon = "🎨", Color = "#fd7e14" },
            new Category { Name = "algoritmalar", Slug = "algoritmalar", Description = "veri yapıları, algoritma analizi ve problem çözme", Icon = "🧮", Color = "#6610f2" },
            new Category { Name = "yazılımda kariyer", Slug = "yazilimda-kariyer", Description = "kariyer gelişimi, iş arama ve profesyonel büyüme", Icon = "🚀", Color = "#e83e8c" }
        };

        foreach (var category in categories)
        {
            _context.Categories.Add(category);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Categories seeding completed.");
    }

    private async Task SeedTopicsAndEntriesAsync()
    {
        _logger.LogInformation("Checking existing topics and adding missing ones...");

        var categories = await _context.Categories.ToListAsync();
        var users = await _context.Users.ToListAsync();

        if (!categories.Any() || !users.Any())
        {
            _logger.LogWarning("Cannot create topics: Categories or Users not found.");
            return;
        }

        var sampleTopics = new[]
        {
            // Yazılım Geliştirme
            new { Title = "python vs c# performans", CategorySlug = "yazilim-gelistirme", CreatedBy = "ahmet", FirstEntry = "c# compiled language olduğu için genel olarak python'dan hızlı. ancak python development hızı çok yüksek. data science ve ai için python, enterprise uygulamalar için c# daha uygun. her ikisinin de kendine has avantajları var." },
            new { Title = "git kullanımı", CategorySlug = "yazilim-gelistirme", CreatedBy = "zeynep", FirstEntry = "version control için git vazgeçilmez. branch'ler ile paralel çalışabilir, merge conflict'leri çözebilirsiniz. commit message'larınızı anlamlı yazın. pull request süreçleri takım çalışması için çok önemli." },
            new { Title = "agile metodoloji", CategorySlug = "yazilim-gelistirme", CreatedBy = "esma", FirstEntry = "scrum, kanban gibi agile framework'ler ile iterative development yapabilirsiniz. sprint'ler ile kısa dönemli hedefler koyarsınız. daily standup'lar ile takım koordinasyonu sağlanır." },

            // Web Geliştirme
            new { Title = "react hooks kullanımı", CategorySlug = "web-gelistirme", CreatedBy = "zeynep", FirstEntry = "react hooks, fonksiyonel componentlerde state ve lifecycle yönetimi için kullanılır. usestate, useeffect gibi built-in hook'lar var. custom hook'lar da yazabilirsiniz. class componentlere göre çok daha temiz kod yazabiliyorsunuz." },
            new { Title = "nodejs express framework", CategorySlug = "web-gelistirme", CreatedBy = "mehmet", FirstEntry = "express.js ile hızlı web uygulamaları geliştirebilirsiniz. middleware'ler ile request/response pipeline'ını yönetebilirsiniz. routing, templating ve static file serving built-in geliyor." },
            new { Title = "css grid vs flexbox", CategorySlug = "web-gelistirme", CreatedBy = "ahmet", FirstEntry = "css grid 2d layout için, flexbox 1d layout için idealdir. grid ile kompleks layout'lar kolayca yapılır. flexbox alignment ve distribution için mükemmel. modern browser'larda her ikisi de destekleniyor." },

            // Mobil Geliştirme
            new { Title = "flutter vs react native", CategorySlug = "mobil-gelistirme", CreatedBy = "zeynep", FirstEntry = "flutter dart dilini kullanır, react native javascript. flutter performance olarak biraz daha iyi. react native daha büyük community'e sahip. her ikisi de cross-platform development için harika." },
            new { Title = "android jetpack compose", CategorySlug = "mobil-gelistirme", CreatedBy = "esma", FirstEntry = "declarative ui toolkit ile android uygulamaları geliştirebilirsiniz. xml layout'lara gerek kalmıyor. kotlin ile type-safe ui kodları yazabiliyorsunuz. preview'lar ile hızlı development mümkün." },
            new { Title = "ios swiftui temelleri", CategorySlug = "mobil-gelistirme", CreatedBy = "mehmet", FirstEntry = "apple'ın declarative ui framework'ü. swift dilinde ui componentleri yazabilirsiniz. storyboard'lara gerek kalmıyor. hot reload ile hızlı development sağlıyor." },

            // .NET Platformu
            new { Title = "asp.net core nedir", CategorySlug = "dotnet-platformu", CreatedBy = "esma", FirstEntry = "asp.net core, microsoft'un açık kaynak, cross-platform web framework'üdür. modern web uygulamaları ve api'ler geliştirebilirsiniz. dependency injection, middleware pipeline ve modular yapısı ile çok güçlü bir framework. linux'ta da çalışması mükemmel." },
            new { Title = "entity framework core", CategorySlug = "dotnet-platformu", CreatedBy = "ahmet", FirstEntry = "orm framework olarak ef core çok popüler. code-first veya database-first yaklaşımları kullanabilirsiniz. migrations ile database schema değişikliklerini yönetebilirsiniz. linq ile type-safe sorgular yazarsınız." },
            new { Title = "minimal apis .net 6", CategorySlug = "dotnet-platformu", CreatedBy = "zeynep", FirstEntry = "minimal api'ler ile çok hızlı api geliştirme yapabilirsiniz. controller'lara gerek kalmıyor. functional programming yaklaşımı ile endpoint'leri tanımlayabilirsiniz. microservice'ler için ideal." },

            // Blazor
            new { Title = "blazor server vs webassembly", CategorySlug = "blazor", CreatedBy = "mehmet", FirstEntry = "blazor server'da kod server'da çalışır, signalr ile ui güncellemeleri yapılır. webassembly'de ise kod browser'da çalışır. server daha hızlı başlar ama network bağımlılığı var. wasm offline çalışabilir ama bundle size büyük." },
            new { Title = "blazor component lifecycle", CategorySlug = "blazor", CreatedBy = "esma", FirstEntry = "component'ler oninit, onparamset, onafterrender gibi lifecycle method'lara sahip. async versiyonları da var. state management için statehaschanged() kullanabilirsiniz. dispose pattern'i ile cleanup yapabilirsiniz." },
            new { Title = "blazor ile spa geliştirme", CategorySlug = "blazor", CreatedBy = "zeynep", FirstEntry = "single page application geliştirebilirsiniz. routing ile sayfa geçişleri yapılır. lazy loading ile performance optimize edilir. javascript interop ile js kütüphanelerini kullanabilirsiniz." },

            // Yapay Zeka
            new { Title = "machine learning giriş", CategorySlug = "yapay-zeka", CreatedBy = "ahmet", FirstEntry = "makine öğrenmesi supervised, unsupervised ve reinforcement learning olarak ayrılır. python ile sklearn, tensorflow kullanabilirsiniz. data preprocessing çok önemli. overfitting'den kaçınmalısınız." },
            new { Title = "chatgpt api kullanımı", CategorySlug = "yapay-zeka", CreatedBy = "mehmet", FirstEntry = "openai api'si ile uygulamalarınıza ai entegre edebilirsiniz. token limitlerine dikkat etmelisiniz. prompt engineering ile daha iyi sonuçlar alabilirsiniz. fine-tuning ile özelleştirebilirsiniz." },
            new { Title = "computer vision opencv", CategorySlug = "yapay-zeka", CreatedBy = "zeynep", FirstEntry = "görüntü işleme için opencv kütüphanesi kullanabilirsiniz. object detection, face recognition gibi işlemler yapabilirsiniz. deep learning model'leri ile entegre edilebilir." },

            // DevOps
            new { Title = "docker kullanımı", CategorySlug = "devops", CreatedBy = "esma", FirstEntry = "containerization ile uygulamalarınızı her ortamda aynı şekilde çalıştırabilirsiniz. dockerfile yazıp image oluşturup container olarak çalıştırıyorsunuz. development'tan production'a kadar tutarlılık sağlar." },
            new { Title = "kubernetes temelleri", CategorySlug = "devops", CreatedBy = "ahmet", FirstEntry = "container orchestration için kubernetes kullanabilirsiniz. pod'lar, service'ler, deployment'lar ile uygulamalarınızı scale edebilirsiniz. auto-scaling ve self-healing özellikleri var." },
            new { Title = "ci cd pipeline", CategorySlug = "devops", CreatedBy = "mehmet", FirstEntry = "continuous integration ve deployment ile otomatik build, test ve deploy süreçleri kurabilirsiniz. github actions, azure devops gibi araçlar kullanabilirsiniz. quality gate'ler ile kalite kontrol yapılır." },

            // Siber Güvenlik
            new { Title = "sql injection saldırıları", CategorySlug = "siber-guvenlik", CreatedBy = "zeynep", FirstEntry = "parameterized query'ler kullanarak sql injection'dan korunabilirsiniz. input validation çok önemli. orm araçları da bu konuda yardımcı olur. white-listing yaklaşımı kullanın." },
            new { Title = "https ve ssl sertifikaları", CategorySlug = "siber-guvenlik", CreatedBy = "esma", FirstEntry = "web uygulamalarında https zorunlu. ssl sertifikaları let's encrypt ile ücretsiz alabilirsiniz. tls 1.3 en güncel versiyon. certificate pinning ile ek güvenlik sağlayabilirsiniz." },
            new { Title = "authentication vs authorization", CategorySlug = "siber-guvenlik", CreatedBy = "ahmet", FirstEntry = "authentication kim olduğunuzu, authorization ne yapabileceğinizi belirler. jwt token'ları stateless authentication için kullanılır. oauth2 ve openid connect standartları var." },

            // Clean Code
            new { Title = "clean code prensipleri", CategorySlug = "clean-code", CreatedBy = "ahmet", FirstEntry = "temiz kod yazmak için solid prensipleri, anlamlı isimlendirme, küçük fonksiyonlar ve yorumları minimize etmek önemli. kodu yazan değil, okuyan kişi için yazmalıyız. refactoring sürekli yapılmalı." },
            new { Title = "solid prensipleri", CategorySlug = "clean-code", CreatedBy = "mehmet", FirstEntry = "single responsibility, open/closed, liskov substitution, interface segregation, dependency inversion. bu 5 prensip ile maintainable kod yazabilirsiniz. dependency injection bu prensipleri uygulamanıza yardımcı olur." },
            new { Title = "code review süreçleri", CategorySlug = "clean-code", CreatedBy = "zeynep", FirstEntry = "kod kalitesi için review süreçleri şart. pull request'lerde constructive feedback verin. automated tools ile static analysis yapın. takım standardları oluşturun." },

            // Design Patterns
            new { Title = "singleton pattern", CategorySlug = "design-patterns", CreatedBy = "esma", FirstEntry = "bir class'tan sadece bir instance oluşturulmasını sağlar. thread-safe implementation önemli. dependency injection ile daha iyi alternatifi var. global state yönetimi için kullanılır." },
            new { Title = "factory pattern kullanımı", CategorySlug = "design-patterns", CreatedBy = "ahmet", FirstEntry = "object creation logic'ini encapsulate eder. concrete class'ları bilmeye gerek kalmaz. interface'ler ile loose coupling sağlar. strategy pattern ile kombinlenebilir." },
            new { Title = "observer pattern", CategorySlug = "design-patterns", CreatedBy = "zeynep", FirstEntry = "one-to-many dependency tanımlar. subject değiştiğinde observer'lar otomatik bilgilendirilir. event-driven programming için kullanılır. loose coupling sağlar." },

            // Algoritmalar
            new { Title = "big o notation", CategorySlug = "algoritmalar", CreatedBy = "mehmet", FirstEntry = "algoritma complexity'sini ölçmek için kullanılır. o(1), o(log n), o(n), o(n²) gibi notasyonlar var. time ve space complexity ayrı ayrı değerlendirilir. optimization için önemli." },
            new { Title = "sorting algoritmaları", CategorySlug = "algoritmalar", CreatedBy = "esma", FirstEntry = "bubble sort, merge sort, quick sort gibi algoritmalar var. time complexity'leri farklı. stable sorting önemli. built-in sort method'ları genelde timsort kullanır." },
            new { Title = "binary search tree", CategorySlug = "algoritmalar", CreatedBy = "ahmet", FirstEntry = "hızlı arama için kullanılan tree yapısı. balanced tree'ler ile o(log n) arama sağlanır. inorder traversal ile sorted liste elde edilir. avl tree, red-black tree varyantları var." },

            // Yazılımda Kariyer
            new { Title = "junior developer rehberi", CategorySlug = "yazilimda-kariyer", CreatedBy = "zeynep", FirstEntry = "kodlama becerilerinizi geliştirin, sürekli öğrenin. github profilinizi aktif tutun. açık kaynak projelere katkıda bulunun. networking yapmayı ihmal etmeyin. mentorship alın." },
            new { Title = "teknik mülakat hazırlığı", CategorySlug = "yazilimda-kariyer", CreatedBy = "mehmet", FirstEntry = "algoritma ve veri yapıları çalışın. system design öğrenin. behavioral sorulara hazırlanın. mock interview yapın. star method ile cevaplarınızı yapılandırın." },
            new { Title = "freelance vs full time", CategorySlug = "yazilimda-kariyer", CreatedBy = "esma", FirstEntry = "freelance'da esneklik var ama gelir garantisi yok. full time'da benefits ve career path var. remote work her ikisinde de mümkün. kişisel hedeflerinize göre seçim yapın." },
            
            // Rastgele bölümünden eklenenler
            new { Title = "gelecek teknolojiler", CategorySlug = "yazilim-gelistirme", CreatedBy = "ahmet", FirstEntry = "yapay zeka, quantum computing, blockchain gibi teknolojiler geleceği şekillendiriyor. web3, metaverse, ar/vr alanları da hızla gelişiyor. kendimizi sürekli güncel tutmalıyız." },
            new { Title = "programlama dilleri", CategorySlug = "yazilim-gelistirme", CreatedBy = "zeynep", FirstEntry = "her programlama dilinin kendine has avantajları var. python data science için, javascript web için, go microservices için popüler. trend'leri takip etmek önemli ama temel konseptler aynı." },
            new { Title = "kariyer tavsiyeleri", CategorySlug = "yazilimda-kariyer", CreatedBy = "mehmet", FirstEntry = "sürekli öğrenmeyi bırakmayın. side project'ler yapın. community'ye katkıda bulunun. networking önemli. mentor bulun ve mentorluk yapın. soft skill'leri geliştirmeyi ihmal etmeyin." },
            
            // Daha fazla çeşitlilik için ek topic'ler
            new { Title = "web tasarım", CategorySlug = "web-gelistirme", CreatedBy = "esma", FirstEntry = "modern web tasarımında user experience çok önemli. responsive design, accessibility, performance optimize edilmeli. figma, sketch gibi araçlar kullanışlı." },
            new { Title = "yazılım geliştirme", CategorySlug = "yazilim-gelistirme", CreatedBy = "ahmet", FirstEntry = "kaliteli yazılım geliştirmek için planlama, tasarım, coding, testing ve deployment süreçlerini iyi yönetmek gerekir. agile metodolojiler çok yardımcı oluyor." },
            new { Title = "veritabanı tasarım", CategorySlug = "yazilim-gelistirme", CreatedBy = "zeynep", FirstEntry = "normalization, indexing, query optimization gibi konular kritik. sql ve nosql arasında doğru seçim yapmalı. performance monitoring unutulmamalı." },
            new { Title = "mobile development", CategorySlug = "mobil-gelistirme", CreatedBy = "mehmet", FirstEntry = "ios ve android native development yanında cross-platform çözümler de popüler. flutter, react native gibi framework'ler iyi seçenek." },
            new { Title = "microservices architecture", CategorySlug = "yazilim-gelistirme", CreatedBy = "esma", FirstEntry = "monolithic mimariye alternatif olarak microservices'ler ölçeklenebilirlik sağlıyor. container'lar, api gateway, service discovery önemli." },
            new { Title = "typescript kullanımı", CategorySlug = "web-gelistirme", CreatedBy = "ahmet", FirstEntry = "javascript'e type safety ekleyen typescript, büyük projelerde çok faydalı. interface'ler, generic'ler, decorator'lar güçlü özellikler." },
            new { Title = "linux sistem yönetimi", CategorySlug = "devops", CreatedBy = "zeynep", FirstEntry = "command line, shell scripting, process management, network configuration gibi konulara hakim olmak gerekir. debian, centos, ubuntu popüler dağıtımlar." },
            new { Title = "blockchain technology", CategorySlug = "yazilim-gelistirme", CreatedBy = "mehmet", FirstEntry = "distributed ledger, smart contracts, cryptocurrency gibi kavramlar önemli. ethereum, solidity öğrenmek başlangıç için iyi." }
        };

        foreach (var topicData in sampleTopics)
        {
            var category = categories.FirstOrDefault(c => c.Slug == topicData.CategorySlug);
            var user = users.FirstOrDefault(u => u.Username == topicData.CreatedBy);

            if (category == null || user == null) continue;

            var slug = GenerateSlug(topicData.Title);
            
            // Bu topic zaten var mı kontrol et
            var existingTopic = await _context.Topics.FirstOrDefaultAsync(t => t.Slug == slug);
            if (existingTopic != null)
            {
                _logger.LogInformation("Topic already exists: {Title}, skipping.", topicData.Title);
                continue;
            }
            
            var topic = new Topic
            {
                Title = topicData.Title,
                Slug = slug,
                CategoryId = category.Id,
                CreatedByUserId = user.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 30)),
                EntryCount = 1,
                ViewCount = Random.Shared.Next(10, 200)
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync(); // Save to get topic ID

            // İlk entry'yi oluştur
            var entry = new Entry
            {
                Content = topicData.FirstEntry,
                ContentHtml = topicData.FirstEntry.Replace("\n", "<br>"),
                TopicId = topic.Id,
                CreatedByUserId = user.Id,
                CreatedAt = topic.CreatedAt.AddMinutes(Random.Shared.Next(1, 60)),
                FavoriteCount = Random.Shared.Next(0, 15)
            };

            _context.Entries.Add(entry);
            
            // Topic'in son entry zamanını güncelle
            topic.LastEntryAt = entry.CreatedAt;

            // User'ın topic count'ını artır
            user.TopicCount++;
            
            _logger.LogInformation("Created new topic: {Title} in category {Category}", topicData.Title, category.Name);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Topics and entries seeding completed.");
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

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}