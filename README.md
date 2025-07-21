# 📖 MyDictionary - Sosyal Sözlük Platformu

Modern bir sosyal sözlük platformu - kullanıcıların çeşitli konularda bilgi paylaşabileceği, tartışabileceği ve sosyalleşebileceği bir uygulama.

## 🏗️ Mimari

MyDictionary, .NET 9 ve Aspire orchestration kullanılarak geliştirilmiş microservices mimarisiyle tasarlanmıştır:

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│   MyDictionary.Web  │    │ MyDictionary.AppHost│    │MyDictionary.ApiService│
│   (Blazor Server)   │◄──►│  (Aspire Orchestr.) │◄──►│    (Web API)        │
│                     │    │                     │    │                     │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
                                       │
                           ┌─────────────────────┐
                           │MyDictionary.Service │
                           │     Defaults        │
                           │ (Shared Config)     │
                           └─────────────────────┘
                                       │
                              ┌─────────────────┐
                              │  SQL Server     │
                              │   LocalDB       │
                              └─────────────────┘
```

### 🧩 Projeler

- **MyDictionary.AppHost**: .NET Aspire orchestration host
- **MyDictionary.ApiService**: Backend REST API servisi  
- **MyDictionary.Web**: Blazor Server frontend uygulaması
- **MyDictionary.ServiceDefaults**: Paylaşılan servis konfigürasyonları

## ✨ Özellikler

### 👥 Kullanıcı Yönetimi
- JWT tabanlı authentication
- Kullanıcı kayıt/giriş sistemi
- Profil yönetimi ve fotoğraf yükleme
- Arkadaşlık sistemi ve takip özelliği
- Kullanıcı arama ve filtreleme

### 📚 İçerik Yönetimi
- **Kategoriler**: Konuları organize eden ana gruplar
- **Başlıklar (Topics)**: Tartışma konuları
- **Entry'ler**: Kullanıcıların başlıklara yazdığı içerikler
- **Favoriler**: Beğeni sistemi
- **Bağlantılar**: Entry'ler arası referanslar

### 💬 Sosyal Özellikler
- Direkt mesajlaşma sistemi
- Bildirim sistemi (arkadaşlık istekleri, yeni mesajlar)
- Kullanıcı engelleme sistemi
- Real-time güncellemeler

### 🔍 Keşif ve Navigasyon
- Dinamik sol sidebar menü:
  - **Sık kullanılanlar**: Entry sayısına göre popüler başlıklar
  - **Kategoriler**: İstatistiklerle birlikte kategori listesi
  - **Rastgele**: Keşif için rastgele başlıklar
- Gelişmiş arama funktionality
- Kategori bazlı filtreleme
- Sayfalama ve sıralama seçenekleri

### 📊 İstatistikler ve Analytics
- Topic görüntülenme sayıları
- Entry sayıları ve popülerlik metrikleri
- Kullanıcı aktivite istatistikleri
- Kategori bazlı analitikler

## 🛠️ Teknoloji Stack

### Backend
- **.NET 9**: En son framework
- **ASP.NET Core Web API**: REST API servisleri
- **Entity Framework Core**: ORM ve database yönetimi
- **SQL Server LocalDB**: Veritabanı
- **JWT Authentication**: Güvenli kimlik doğrulama
- **Swagger/OpenAPI**: API dokümantasyonu

### Frontend  
- **Blazor Server**: Server-side rendering
- **SignalR**: Real-time komunikasyon
- **Scoped CSS**: Component-based styling
- **JavaScript Interop**: Browser API'lerine erişim

### DevOps ve Tooling
- **.NET Aspire**: Microservice orchestration
- **McpProbe**: Debug ve monitoring
- **MailKit**: Email servisleri
- **BCrypt**: Güvenli şifreleme

## 🚀 Kurulum

### Ön Gereksinimler
- Visual Studio 2022 (17.8+)
- .NET 9 SDK
- SQL Server LocalDB

### 1. Repository'yi Clone Edin
```bash
git clone https://github.com/rumeysatibet/MyDictionary.git
cd MyDictionary
```

### 2. Geliştirme Ortamını Başlatın

#### Hızlı Başlatma (Önerilen)
```bash
dev-start.bat
```

#### Manuel Başlatma
```bash
dotnet clean
dotnet build
dotnet run --project MyDictionary.AppHost
```

### 3. Veritabanı Yönetimi

#### Migration Uygulama
```bash
dotnet ef database update --project MyDictionary.ApiService
```

#### Yeni Migration Oluşturma
```bash
dotnet ef migrations add MigrationName --project MyDictionary.ApiService
```

#### Veritabanını Sıfırlama (Geliştirme)
```bash
dotnet ef database drop --project MyDictionary.ApiService
```

### 4. Bireysel Servisleri Çalıştırma

#### Sadece API Servisi
```bash
dotnet run --project MyDictionary.ApiService
```

#### Sadece Web Frontend
```bash
dotnet run --project MyDictionary.Web
```

## 📋 API Endpoint'leri

### Authentication
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi
- `GET /api/auth/me` - Mevcut kullanıcı bilgileri

### Kategoriler
- `GET /api/categories` - Tüm kategoriler
- `GET /api/categories/with-stats` - İstatistiklerle kategoriler
- `GET /api/categories/{slug}` - Kategori detayı
- `GET /api/categories/{slug}/topics` - Kategori başlıkları

### Başlıklar (Topics)
- `GET /api/topics` - Başlık listesi
- `GET /api/topics/popular` - Popüler başlıklar
- `GET /api/topics/{id}` - Başlık detayı
- `GET /api/topics/slug/{slug}` - Slug ile başlık
- `POST /api/topics` - Yeni başlık oluştur

### Entry'ler
- `GET /api/entries` - Entry listesi
- `POST /api/entries` - Yeni entry oluştur
- `PUT /api/entries/{id}` - Entry güncelle
- `DELETE /api/entries/{id}` - Entry sil

### Kullanıcılar
- `GET /api/user/search` - Kullanıcı arama
- `GET /api/user/profile/{username}` - Profil görüntüle
- `POST /api/profile/upload-photo` - Profil fotoğrafı yükle

### Sosyal Özellikler
- `GET /api/notifications` - Bildirimler
- `POST /api/messages` - Mesaj gönder
- `GET /api/messages/conversations` - Konuşma listesi
- `POST /api/friends/request` - Arkadaşlık isteği

## 🗄️ Veritabanı Şeması

### Temel Tablolar
- **Users**: Kullanıcı bilgileri ve profil
- **Categories**: Ana kategori grupları
- **Topics**: Tartışma başlıkları
- **Entries**: Kullanıcı içerikleri
- **UserAgreements**: Platform sözleşmeleri

### Sosyal Tablolar
- **Friendships**: Arkadaşlık ilişkileri
- **FriendRequests**: Arkadaşlık istekleri
- **Messages/Conversations**: Mesajlaşma sistemi
- **Notifications**: Bildirim yönetimi

### İlişkisel Tablolar
- **EntryFavorites**: Beğeni sistemi
- **EntryLinks**: İçerik bağlantıları
- **UserBlocks**: Engelleme sistemi

## 🔧 Konfigürasyon

### API Servisi (Port: 5000)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "mydictionarydb"
  },
  "JwtSettings": {
    "Issuer": "MyDictionaryApi",
    "Audience": "MyDictionaryWeb", 
    "Key": "your-secret-key-here"
  }
}
```

### Web Servisi (Port: 5001)
```json
{
  "ApiServiceUrl": "https://localhost:5000",
  "Aspire": {
    "ServiceDiscovery": true
  }
}
```

## 🐛 Debug ve Monitoring

### McpProbe Integration
Proje, gelişmiş debugging için McpProbe entegrasyonu içerir:
- Real-time log monitoring
- Performance analysis
- DOM snapshot'ları
- Error tracking

### Debug Endpoint'leri
- `/api/auth/debug/users` - Test kullanıcıları
- `/debug` - Debug sayfası
- `/_probe` - McpProbe dashboard

### Batch Scriptleri
- `dev-start.bat` - Geliştirme başlatma
- `database-manager.bat` - Veritabanı yönetimi
- `test-database.bat` - Database testing
- `cleanup.bat` - Process temizleme

## 🔒 Güvenlik

### Authentication
- JWT Bearer token tabanlı
- Şifre SHA256 hash'leme
- Token expiration yönetimi
- Role-based authorization

### API Security
- CORS policy konfigürasyonu
- SQL injection koruması (EF Core)
- Input validation
- File upload güvenliği

### Production Notları
- JWT secret key değiştirilmeli
- CORS policy kısıtlanmalı
- HTTPS enforced olmalı
- Logging ve monitoring aktifleştirilmeli

## 📁 Proje Yapısı

```
MyDictionary/
├── 📁 MyDictionary.AppHost/          # Aspire orchestration
│   ├── Program.cs
│   └── Properties/launchSettings.json
│
├── 📁 MyDictionary.ApiService/        # Backend API
│   ├── 📁 Controllers/               # API Controllers
│   ├── 📁 Data/                     # DbContext
│   ├── 📁 Models/                   # Entity models
│   ├── 📁 Services/                 # Business logic
│   ├── 📁 Migrations/               # EF migrations
│   └── Program.cs
│
├── 📁 MyDictionary.Web/              # Frontend
│   ├── 📁 Components/
│   │   ├── 📁 Layout/               # Layout components
│   │   ├── 📁 Pages/                # Page components
│   │   └── 📁 Shared/               # Shared components
│   ├── 📁 Services/                 # Frontend services
│   ├── 📁 wwwroot/                  # Static files
│   └── Program.cs
│
├── 📁 MyDictionary.ServiceDefaults/   # Shared config
│   └── Extensions.cs
│
├── 📄 MyDictionary.sln               # Solution file
├── 📄 CLAUDE.md                      # Development guide
└── 📄 README.md                      # Bu dosya
```

## 🎯 Kullanım Senaryoları

### Yeni Kullanıcı Akışı
1. `/register` sayfasından kayıt ol
2. Email ile hesap onayı
3. Profil bilgilerini tamamla
4. İlgi alanlarına göre kategorileri keşfet
5. İlk entry'ni yaz veya beğeni bırak

### Günlük Kullanım
1. Ana sayfada son başlıkları incele
2. Sol menüden popüler başlıklara göz at
3. Entry yaz veya mevcut entry'leri beğen
4. Arkadaş edin ve mesajlaş
5. Bildirimleri kontrol et

### İçerik Oluşturma
1. "başlık aç" butonuna tıkla
2. Uygun kategoriyi seç
3. Başlık adını gir
4. İlk entry'yi yaz
5. Diğer kullanıcıların katkılarını bekle

## 🚧 Geliştirme Durumu

### ✅ Tamamlanan Özellikler
- [ ] Kullanıcı kayıt/giriş sistemi
- [ ] JWT Authentication
- [ ] Kategori ve başlık yönetimi
- [ ] Entry CRUD işlemleri
- [ ] Dinamik sidebar menüsü
- [ ] Kullanıcı arama
- [ ] Profil yönetimi
- [ ] Mesajlaşma sistemi
- [ ] Bildirim sistemi
- [ ] Favoriler sistemi

### 🔄 Devam Eden Çalışmalar
- [ ] Real-time güncellemeler (SignalR)
- [ ] Gelişmiş arama filtreleri
- [ ] Mobile responsive iyileştirmeleri
- [ ] Performance optimizasyonları

### 📋 Planlanan Özellikler
- [ ] Email bildirimleri
- [ ] Sosyal medya entegrasyonu
- [ ] Tema sistemi (Dark/Light mode)
- [ ] PWA desteği
- [ ] API rate limiting
- [ ] Caching mekanizması
- [ ] File upload limits ve validation
- [ ] Advanced user permissions

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasını inceleyiniz.

## 📧 İletişim

- GitHub: [@rumeysatibet](https://github.com/rumeysatibet)
- Email: rumeysatibet@example.com

## 🙏 Teşekkürler

- .NET Team için harika framework
- Aspire ekibine orchestration desteği için
- Blazor community'ye sürekli geliştirmeler için

---

**MyDictionary** - Modern sosyal sözlük deneyimi ✨
