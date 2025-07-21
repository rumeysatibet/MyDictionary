# 📋 Changelog

Tüm önemli değişiklikler bu dosyada dokümante edilir.

Format [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) standardına dayanır ve bu proje [Semantic Versioning](https://semver.org/spec/v2.0.0.html) kullanır.

## [Unreleased]

### 🎯 Planlanan
- Real-time güncellemeler (SignalR)
- Email bildirim sistemi
- Tema sistemi (Dark/Light mode)
- PWA desteği
- Advanced search filters
- Mobile app (React Native/Flutter)

## [1.0.0] - 2025-01-21

### ✨ Added
- **Kullanıcı Yönetimi**
  - JWT tabanlı authentication sistemi
  - Kullanıcı kayıt/giriş functionality
  - Profil yönetimi ve fotoğraf yükleme
  - Kullanıcı arama ve filtreleme

- **İçerik Sistemi**
  - Kategori tabanlı organizasyon
  - Topic (başlık) oluşturma ve yönetimi
  - Entry CRUD işlemleri
  - Favoriler sistemi
  - Entry'ler arası bağlantılar

- **Sosyal Özellikler**
  - Arkadaşlık istekleri ve yönetimi
  - Direkt mesajlaşma sistemi
  - Bildirim sistemi
  - Kullanıcı engelleme

- **UI/UX**
  - Responsive Blazor Server frontend
  - Dinamik sol sidebar menüsü
  - Real-time popüler içerik gösterimi
  - Modern ve temiz tasarım

- **Backend API**
  - RESTful API endpoints
  - Entity Framework Core ile database yönetimi
  - Swagger/OpenAPI dokümantasyonu
  - Comprehensive error handling

- **DevOps**
  - .NET Aspire orchestration
  - McpProbe debugging integration
  - Automated database migration
  - Development scripts ve tooling

### 🏗️ Technical Architecture
- **.NET 9** framework
- **Blazor Server** frontend
- **ASP.NET Core Web API** backend
- **SQL Server LocalDB** database
- **Entity Framework Core** ORM
- **JWT Authentication**
- **Aspire Microservices** orchestration

### 🔒 Security
- Password hashing (SHA256)
- SQL injection protection
- XSS protection (Blazor built-in)
- CSRF protection
- File upload security
- Input validation

### 📊 Database Schema
- **Users**: User profiles ve authentication
- **Categories**: Content organization
- **Topics**: Discussion topics
- **Entries**: User-generated content
- **Friendships**: Social connections
- **Messages**: Private messaging
- **Notifications**: System notifications
- **EntryFavorites**: Like system
- **UserAgreements**: Terms of service

### 🛠️ Development Tools
- Comprehensive README.md
- Contributing guidelines
- Security policy
- MIT License
- Professional .gitignore
- Development automation scripts

### 📱 Features Implemented
- ✅ User registration/authentication
- ✅ Profile management
- ✅ Content creation (categories, topics, entries)
- ✅ Social features (friends, messages, notifications)
- ✅ Dynamic menu system
- ✅ Search functionality
- ✅ Admin features
- ✅ File upload system
- ✅ Responsive design

### 🧪 Testing
- Manual testing completed
- API endpoint testing
- UI/UX flow testing
- Cross-browser compatibility
- Mobile responsive testing

### 📖 Documentation
- Complete README.md with setup instructions
- API documentation via Swagger
- Code comments ve inline documentation
- Contributing guidelines
- Security policy
- This changelog

## [0.9.0] - 2025-01-20

### ✨ Added
- Backend API temel yapısı
- Database schema ve migrations
- Authentication sisteminin temelleri

### 🔄 Changed
- Project structure iyileştirmeleri
- Configuration management

### 🐛 Fixed
- Initial setup issues
- Database connection problems

## [0.8.0] - 2025-01-19

### ✨ Added
- Blazor frontend projesi
- Temel layout ve componentler
- Navigation sistemi

### 🔧 Technical Debt
- Initial project setup
- Development environment configuration

## [0.1.0] - 2025-01-18

### ✨ Added
- Initial project creation
- .NET Aspire integration
- Basic project structure
- Development tools setup

---

## 🔖 Version Legend

- **Major** (X.0.0): Breaking changes, büyük özellik eklentileri
- **Minor** (0.X.0): Yeni özellikler, backward compatible
- **Patch** (0.0.X): Bug fixes ve küçük iyileştirmeler

## 📝 Change Categories

- **✨ Added**: Yeni özellikler
- **🔄 Changed**: Mevcut functionality değişiklikleri  
- **❌ Deprecated**: Yakında kaldırılacak özellikler
- **🗑️ Removed**: Kaldırılan özellikler
- **🐛 Fixed**: Bug fixes
- **🔒 Security**: Güvenlik güncellemeleri
- **🔧 Technical Debt**: Teknik iyileştirmeler
- **📖 Documentation**: Dokümantasyon güncellemeleri
- **🧪 Testing**: Test ekleme/güncellemeleri
- **🏗️ Infrastructure**: DevOps ve infrastructure değişiklikleri