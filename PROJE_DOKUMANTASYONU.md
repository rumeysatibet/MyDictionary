# MyDictionary Projesi - Geliştirici Dokümantasyonu

## Proje Genel Bakış

MyDictionary, Türkiye'nin popüler sözlük sitelerinden ilham alınarak geliştirilmiş modern bir sosyal sözlük platformudur. Kullanıcılar çeşitli konularda entry'ler yazabilir, birbirleriyle arkadaşlık kurabilir, mesajlaşabilir ve bildirimler alabilirler.

## Teknoloji Stack'i

### Backend (.NET 9)
- **Framework**: ASP.NET Core 9.0
- **ORM**: Entity Framework Core
- **Veritabanı**: SQL Server (LocalDB)
- **Authentication**: JWT Token based
- **Architecture**: Clean Architecture + Repository Pattern

### Frontend (Blazor Server)
- **Framework**: Blazor Server (.NET 9)
- **UI**: Razor Components
- **Styling**: CSS + Custom Components
- **State Management**: Service-based approach

### Deployment & DevOps
- **Orchestration**: .NET Aspire
- **Development**: Visual Studio 2022
- **Source Control**: Git

## Proje Yapısı

```
MyDictionary/
├── MyDictionary.AppHost/           # Aspire Orchestration Host
├── MyDictionary.ServiceDefaults/   # Shared configurations
├── MyDictionary.ApiService/        # Backend API Service
│   ├── Controllers/                # API Controllers
│   ├── Models/                     # Data Models
│   ├── Data/                       # Database Context
│   ├── Services/                   # Business Logic Services
│   ├── DTOs/                       # Data Transfer Objects
│   └── Migrations/                 # EF Migrations
└── MyDictionary.Web/              # Frontend Blazor App
    ├── Components/
    │   ├── Pages/                  # Razor Pages
    │   └── Layout/                 # Layout Components
    └── Services/                   # Frontend Services
```

## Veritabanı Modeli

### Ana Tablolar

#### Users Tablosu
- **Id**: Primary Key
- **Username**: Unique kullanıcı adı
- **Email**: Unique email adresi
- **PasswordHash**: Şifrelenmiş şifre
- **BirthDate**: Doğum tarihi
- **Gender**: Cinsiyet (Enum: NotSpecified, Male, Female, Other)
- **Role**: Kullanıcı rolü (User, Admin)
- **Profile Fields**: About, ProfilePhotoUrl, FollowerCount, FollowingCount, EntryCount
- **Timestamps**: CreatedAt, LastLoginAt

#### Topics Tablosu
- **Id**: Primary Key
- **Title**: Başlık adı
- **CreatedByUserId**: Foreign Key (Users)
- **CreatedAt**: Oluşturulma zamanı

#### Entries Tablosu
- **Id**: Primary Key
- **Content**: Entry içeriği
- **TopicId**: Foreign Key (Topics)
- **CreatedByUserId**: Foreign Key (Users)
- **CreatedAt**: Oluşturulma zamanı
- **UpVotes/DownVotes**: Oy sayıları

#### Arkadaşlık Sistemi

**FriendRequests Tablosu**
- **Id**: Primary Key
- **SenderId**: Foreign Key (Users)
- **ReceiverId**: Foreign Key (Users)
- **Status**: Enum (Pending, Accepted, Rejected)
- **CreatedAt**: İstek zamanı

**Friendships Tablosu**
- **Id**: Primary Key
- **UserId**: Foreign Key (Users)
- **FriendId**: Foreign Key (Users)
- **CreatedAt**: Arkadaşlık zamanı

#### Mesajlaşma Sistemi

**Messages Tablosu**
- **Id**: Primary Key
- **SenderId**: Foreign Key (Users)
- **ReceiverId**: Foreign Key (Users)
- **Content**: Mesaj içeriği (Max 1000 karakter)
- **IsRead**: Okundu bilgisi
- **CreatedAt/ReadAt**: Zaman damgaları

**Conversations Tablosu**
- **Id**: Primary Key
- **User1Id/User2Id**: Foreign Keys (Users)
- **LastMessageId**: Foreign Key (Messages)
- **LastMessageAt**: Son mesaj zamanı

#### Bildirim Sistemi

**Notifications Tablosu**
- **Id**: Primary Key
- **UserId**: Foreign Key (Users) - Bildirimi alan
- **FromUserId**: Foreign Key (Users) - Bildirimi gönderen
- **Type**: Bildirim türü (FriendRequest, FriendRequestAccepted, Message, etc.)
- **Title/Message**: Bildirim metinleri
- **IsRead**: Okundu bilgisi
- **CreatedAt**: Oluşturulma zamanı

## API Endpoints

### Authentication Controller (/api/auth)
- `POST /register` - Kullanıcı kaydı
- `POST /login` - Giriş işlemi
- `POST /validate-token` - Token doğrulama
- `GET /user-agreement` - Kullanıcı sözleşmesi
- `GET /debug/users` - Debug: Tüm kullanıcılar

### User Controller (/api/user)
- `GET /search` - Kullanıcı arama
- `GET /{userId}/relationship` - Kullanıcı ilişki durumu

### Friends Controller (/api/friends)
- `POST /send-request` - Arkadaşlık isteği gönder
- `GET /requests` - Gelen/giden arkadaşlık istekleri
- `POST /{requestId}/accept` - İsteği kabul et
- `POST /{requestId}/reject` - İsteği reddet
- `GET /` - Arkadaş listesi

### Profile Controller (/api/profile)
- `GET /{username}` - Kullanıcı profili görüntüle
- `PUT /update` - Profil güncelle
- `POST /upload-photo` - Profil fotoğrafı yükle

### Messages Controller (/api/messages)
- `GET /conversations` - Konuşma listesi
- `GET /conversations/{userId}` - Belirli kullanıcıyla konuşma
- `POST /send` - Mesaj gönder
- `PUT /{messageId}/read` - Mesajı okundu işaretle

### Notifications Controller (/api/notifications)
- `GET /` - Bildirim listesi
- `PUT /{notificationId}/read` - Bildirimi okundu işaretle
- `PUT /mark-all-read` - Tümünü okundu işaretle
- `GET /unread-count` - Okunmamış bildirim sayısı

## Frontend Sayfaları

### Ana Sayfalar
- **Home.razor**: Ana sayfa - Son başlıklar ve güncellemeler
- **Login.razor**: Giriş sayfası
- **Register.razor**: Kayıt sayfası
- **Profile.razor**: Kullanıcı profil sayfası

### Layout Bileşenleri
- **MainLayout.razor**: Ana layout
- **Header.razor**: Üst menü ve navigasyon
- **Sidebar.razor**: Sol yan menü
- **Footer.razor**: Alt kısım

### Diğer Sayfalar
- **Contact.razor**: İletişim
- **Rules.razor**: Kurallar
- **UserAgreement.razor**: Kullanıcı sözleşmesi
- **Debug.razor**: Debug sayfası

## Önemli Servisler

### Backend Services
- **AuthService**: Kimlik doğrulama ve kullanıcı yönetimi
- **NotificationService**: Bildirim oluşturma ve yönetimi
- **DataSeedingService**: Test verisi oluşturma

### Frontend Services
- **AuthenticationStateService**: Frontend auth state yönetimi
- HttpClient factory kullanımı
- JWT token yönetimi

## Güvenlik Özellikleri

### Backend Güvenlik
- JWT Token tabanlı authentication
- Password hashing (BCrypt)
- CORS yapılandırması
- Input validation ve sanitization
- SQL Injection koruması (EF Core)

### Frontend Güvenlik
- Token-based authentication
- Secure local storage kullanımı
- CSRF koruması
- Input validation

## Veritabanı Migration'ları

1. **20250719074701_InitialCreate**: İlk veritabanı yapısı
2. **20250719200817_AddUserProfileFields**: Profil alanları eklendi
3. **20250719203635_AddFriendshipSystem**: Arkadaşlık sistemi
4. **20250719205748_AddNotifications**: Bildirim sistemi
5. **20250719210516_AddMessaging**: Mesajlaşma sistemi

## Geliştirme Ortamı Kurulumu

### Gereksinimler
- Visual Studio 2022 (17.8+)
- .NET 9 SDK
- SQL Server LocalDB
- Git

### Kurulum Adımları

1. **Projeyi klonlayın**
```bash
git clone <repository-url>
cd MyDictionary
```

2. **Dependencies'leri yükleyin**
```bash
dotnet restore
```

3. **Veritabanını oluşturun**
```bash
cd MyDictionary.ApiService
dotnet ef database update
```

4. **Projeyi çalıştırın**
```bash
cd .. # Ana dizine dön
dotnet run --project MyDictionary.AppHost
```

### Proje Çalıştırma
- **Development**: `dotnet run --project MyDictionary.AppHost`
- **API Only**: `dotnet run --project MyDictionary.ApiService`
- **Web Only**: `dotnet run --project MyDictionary.Web`

## Aspire Orchestration

Proje .NET Aspire kullanarak mikroservis mimarisinde yapılandırılmıştır:
- **AppHost**: Orchestration ve service discovery
- **ServiceDefaults**: Ortak konfigürasyonlar
- **ApiService**: Backend API (Port: 5000)
- **Web**: Frontend Blazor (Port: 5001)

## Test ve Debug

### Debug Endpoints
- `/api/auth/debug/users` - Tüm kullanıcıları listele
- `/debug` - Frontend debug sayfası

### Logging
- Structured logging kullanımı
- Console ve Debug output
- API çağrıları için detaylı loglar

## Dosya Yükleme

### Profil Fotoğrafları
- **Konum**: `wwwroot/uploads/profiles/`
- **Format**: JPG, PNG
- **Boyut Limiti**: 5MB
- **Adlandırma**: `{UserId}_{Timestamp}.{Extension}`

## Gelecek Geliştirmeler

### Planlanan Özellikler
1. **Entry Sistemi**: Topic ve Entry CRUD işlemleri
2. **Voting Sistemi**: Entry'lere oy verme
3. **Favorite Sistemi**: Entry favorileme
4. **Search**: Gelişmiş arama özellikleri
5. **Admin Panel**: Yönetici arayüzü
6. **Real-time Features**: SignalR ile gerçek zamanlı güncellemeler
7. **Mobile App**: React Native veya Flutter

### Teknik İyileştirmeler
1. **Caching**: Redis implementasyonu
2. **Performance**: Query optimization
3. **Security**: Rate limiting, advanced validation
4. **Monitoring**: Application Insights, Health checks
5. **Testing**: Unit ve Integration testleri
6. **Documentation**: API documentation (Swagger)

## Katkıda Bulunma

### Code Style
- C# coding conventions
- Clean code principles
- SOLID principles
- Repository pattern

### Git Workflow
- Feature branch kullanımı
- Meaningful commit messages
- Pull request reviews

## Sorun Giderme

### Yaygın Sorunlar
1. **Migration Hataları**: `dotnet ef database drop` sonrası `dotnet ef database update`
2. **Port Çakışması**: launchSettings.json'da port değiştir
3. **JWT Token Hataları**: Token expiry kontrolü
4. **CORS Hataları**: Backend CORS ayarları

### Log Konumları
- Console output
- Debug window
- Browser developer tools (Network tab)

---

Bu dokümantasyon, MyDictionary projesini geliştiren herkesin bilmesi gereken temel bilgileri içermektedir. Proje aktif geliştirme aşamasında olup, özellikler ve yapı sürekli gelişmektedir.