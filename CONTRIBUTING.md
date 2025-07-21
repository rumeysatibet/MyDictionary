# 🤝 Contributing to MyDictionary

MyDictionary projesine katkıda bulunduğunuz için teşekkür ederiz! Bu rehber, projeye nasıl katkıda bulunabileceğinizi açıklar.

## 📋 İçindekiler

- [Geliştirme Ortamını Kurma](#geliştirme-ortamını-kurma)
- [Katkı Süreci](#katkı-süreci)
- [Code Review](#code-review)
- [Commit Mesaj Formatı](#commit-mesaj-formatı)
- [Kodlama Standartları](#kodlama-standartları)
- [Issue Raporlama](#issue-raporlama)
- [Pull Request Süreci](#pull-request-süreci)

## 🛠️ Geliştirme Ortamını Kurma

### Ön Gereksinimler
- Visual Studio 2022 (17.8+)
- .NET 9 SDK
- SQL Server LocalDB
- Git

### Kurulum
1. Repository'yi fork edin
2. Local'e clone edin:
   ```bash
   git clone https://github.com/your-username/MyDictionary.git
   cd MyDictionary
   ```
3. Geliştirme ortamını başlatın:
   ```bash
   ./dev-start.bat
   ```

## 🔄 Katkı Süreci

### 1. Issue Oluşturun veya Mevcut Birini Seçin
- Büyük değişiklikler için önce issue açın
- Mevcut issue'ları kontrol edin
- Issue'u kendinize assign edin

### 2. Branch Oluşturun
```bash
git checkout -b feature/issue-123-add-amazing-feature
```

Branch isimlendirme konvansiyonu:
- `feature/issue-number-description` - Yeni özellikler
- `fix/issue-number-description` - Bug düzeltmeleri
- `docs/description` - Dokümantasyon
- `refactor/description` - Code refactoring
- `test/description` - Test ekleme/düzeltme

### 3. Değişikliklerinizi Yapın
- Küçük, mantıklı commit'ler yapın
- Her commit'in tek bir sorumluluğu olsun
- Test'lerinizi yazın (varsa)

### 4. Test Edin
```bash
dotnet build
dotnet test (eğer test varsa)
```

### 5. Pull Request Açın
- Açıklayıcı başlık kullanın
- Detaylı açıklama yazın
- İlgili issue'u link edin

## 📝 Commit Mesaj Formatı

Conventional Commits formatını kullanıyoruz:

```
<type>(<scope>): <description>

<body>

<footer>
```

### Type'lar:
- `feat`: Yeni özellik
- `fix`: Bug düzeltme
- `docs`: Dokümantasyon değişikliği
- `style`: Code formatı (whitespace, formatting, missing semicolons)
- `refactor`: Code refactoring
- `perf`: Performance iyileştirmesi
- `test`: Test ekleme/düzeltme
- `chore`: Build process veya auxiliary tools ve libraries

### Örnekler:
```bash
feat(auth): add JWT token refresh functionality

fix(sidebar): resolve dynamic menu loading issue

docs(readme): update installation instructions

refactor(api): simplify user authentication logic
```

## 🎯 Kodlama Standartları

### C# Kodlama Standartları
- Microsoft C# Coding Conventions'ı takip edin
- PascalCase for public members, camelCase for private fields
- Meaningful variable names kullanın
- SOLID principles'ları uygulayın
- Async/await pattern'ını doğru kullanın

### Blazor Komponentler
- PascalCase component names
- Props için `[Parameter]` attribute kullanın
- Lifecycle methods'ları doğru implement edin
- CSS scoping kullanın

### API Design
- RESTful conventions takip edin
- Consistent naming (kebab-case URLs)
- Proper HTTP status codes kullanın
- DTO pattern'ını uygulayın

## 🐛 Issue Raporlama

### Bug Report Template
```markdown
## 🐛 Bug Açıklaması
Kısa ve net açıklama

## 🔄 Yeniden Üretme Adımları
1. '...' sayfasına git
2. '...' butonuna tıkla
3. Hata görülür

## ✅ Beklenen Davranış
Ne olması gerektiği

## 📱 Ortam
- OS: [e.g. Windows 11]
- Browser: [e.g. Chrome 91]
- .NET Version: [e.g. .NET 9]
```

### Feature Request Template
```markdown
## 🚀 Özellik Talebi
Özelliğin açıklaması

## 🎯 Motivasyon
Neden bu özellik gerekli?

## 💡 Çözüm Önerisi
Nasıl implement edilebilir?

## 🔄 Alternatifler
Diğer seçenekler neler?
```

## 🔍 Code Review

### Review Kriterleri
- [ ] Kod standartlara uygun mu?
- [ ] Tests yazılmış mı?
- [ ] Performance impact var mı?
- [ ] Security concern'ler ele alınmış mı?
- [ ] Backward compatibility korunmuş mu?
- [ ] Dokümantasyon güncellenmesi gerekiyor mu?

### Review Süreci
1. Automated checks geçsin
2. En az bir maintainer review etsin
3. Tüm comments resolve edilsin
4. Merge edilsin

## 📋 Pull Request Süreci

### PR Template
```markdown
## 📋 Özet
Bu PR'da yapılan değişikliklerin kısa açıklaması

## 🔗 İlgili Issue
Fixes #123

## 🧪 Test Planı
- [ ] Manuel test yapıldı
- [ ] Unit tests yazıldı
- [ ] Integration tests çalıştırıldı

## 📸 Ekran Görüntüleri (varsa)
UI değişiklikleri için

## ✅ Checklist
- [ ] Kod self-review yapıldı
- [ ] Comments eklendi (gerekiyorsa)
- [ ] Dokümantasyon güncellendi
- [ ] Tests eklendi/güncellendi
```

## 🚫 Kabul Edilmeyecek Katkılar

- Kodlama standartlarına uymayan kodlar
- Test'i olmayan kritik değişiklikler
- Security vulnerability'leri olan kodlar
- Breaking change'ler (major version dışında)
- Scope'u çok büyük PR'lar

## 🆘 Yardım Gerekiyor mu?

- [GitHub Discussions](https://github.com/rumeysatibet/MyDictionary/discussions) kullanın
- Issue açarak soru sorun
- Maintainer'lara mention yapın

## 🎖️ Katkıda Bulunanlar

Tüm katkıda bulunanlar README.md'de listelenir ve teşekkür edilir.

---

**MyDictionary projesine katkıda bulunduğunuz için teşekkürler! 🙏**