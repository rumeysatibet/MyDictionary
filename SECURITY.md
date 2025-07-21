# 🔒 Security Policy

MyDictionary projesi güvenliğini ciddiye alır. Bu dokümanda güvenlik açıklarını nasıl raporlayacağınızı ve hangi versiyonların güvenlik güncellemesi aldığını bulabilirsiniz.

## 🛡️ Desteklenen Versiyonlar

Şu anda güvenlik güncellemesi alan versiyonlar:

| Version | Supported          |
| ------- | ------------------ |
| 1.0.x   | ✅ Yes            |
| < 1.0   | ❌ No             |

## 🚨 Güvenlik Açığı Raporlama

### Özel Raporlama
Güvenlik açıklarını **PUBLIC ISSUE OLARAK AÇMAYIN**. Bunun yerine:

1. **Email**: rumeysatibet@example.com adresine "SECURITY" başlığıyla email gönderin
2. **GitHub Security Advisory**: [Security tab](https://github.com/rumeysatibet/MyDictionary/security) üzerinden private advisory oluşturun

### Rapor İçeriği
Güvenlik raporunuz şunları içermelidir:

```
## Güvenlik Açığı Tipi
[XSS, SQL Injection, CSRF, Authentication Bypass, etc.]

## Etkilenen Komponent
[Hangi dosya/method/endpoint]

## Açıklama
[Detaylı açıklama]

## Yeniden Üretme Adımları
1. [Adım 1]
2. [Adım 2]
3. [Sonuç]

## Etki
[Hangi veriler/sistemler etkilenir]

## Çözüm Önerisi
[Varsa çözüm önerisi]
```

## ⏱️ Yanıt Süreci

- **24 saat**: İlk yanıt ve onay
- **72 saat**: Detaylı değerlendirme
- **7 gün**: Düzeltme planı
- **30 gün**: Düzeltme ve yayınlama (kritik olmayanlarda daha uzun olabilir)

## 🔐 Güvenlik Önlemleri

### Backend (API)
- ✅ JWT Authentication
- ✅ SQL Injection koruması (EF Core)
- ✅ Password hashing (SHA256)
- ✅ Input validation
- ✅ CORS policy
- ⚠️ Rate limiting (planned)
- ⚠️ HTTPS enforce (production'da gerekli)

### Frontend (Blazor)
- ✅ XSS koruması (Blazor built-in)
- ✅ CSRF protection
- ✅ Secure cookie settings
- ✅ Input sanitization
- ⚠️ Content Security Policy (planned)

### Database
- ✅ Parameterized queries
- ✅ Connection string encryption
- ✅ Minimum permissions principle
- ⚠️ Database encryption (planned)

### File Upload
- ✅ File type validation
- ✅ Size limits
- ✅ Secure storage path
- ⚠️ Virus scanning (planned)

## 🚫 Bilinen Güvenlik Sınırlamaları

### Development Ortamı
- JWT secret key default değerde (production'da değiştirilmeli)
- CORS "AllowAll" policy (production'da kısıtlanmalı)
- Debug endpoints aktif (production'da kapatılmalı)
- LocalDB kullanımı (production'da SQL Server)

### Production Önerileri
```json
{
  "JwtSettings": {
    "Key": "your-strong-secret-key-256-bit-minimum",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Cors": {
    "AllowedOrigins": ["https://yourdomain.com"]
  },
  "Logging": {
    "Level": "Warning"
  }
}
```

## 🎯 Güvenlik Kontrolü

### Manual Security Checklist
- [ ] Authentication bypass denemeleri
- [ ] Authorization kontrollerinin testi
- [ ] Input validation testleri
- [ ] XSS ve injection testleri
- [ ] File upload güvenlik testleri
- [ ] Session management testleri
- [ ] Password policy testleri

### Automated Security Tools
- **Dependency scanning**: GitHub Dependabot
- **Code scanning**: GitHub CodeQL (planned)
- **SAST**: SonarQube integration (planned)
- **Vulnerability scanning**: OWASP ZAP (planned)

## 🔄 Güvenlik Güncellemeleri

Güvenlik güncellemeleri şu şekilde duyurulur:
1. GitHub Security Advisory
2. Release Notes
3. Email (kritik güncellemeler için)

### Güncelleme Kategorileri
- **Critical**: Hemen güncellenmeli (0-day exploits)
- **High**: 7 gün içinde güncellenmeli
- **Medium**: 30 gün içinde güncellenmeli  
- **Low**: Bir sonraki minor release'de

## 📚 Güvenlik Kaynakları

### OWASP Top 10 (2021)
- [A01:2021 – Broken Access Control](https://owasp.org/Top10/A01_2021-Broken_Access_Control/)
- [A02:2021 – Cryptographic Failures](https://owasp.org/Top10/A02_2021-Cryptographic_Failures/)
- [A03:2021 – Injection](https://owasp.org/Top10/A03_2021-Injection/)
- [A07:2021 – Identification and Authentication Failures](https://owasp.org/Top10/A07_2021-Identification_and_Authentication_Failures/)

### .NET Security Guidelines
- [Microsoft Security Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/security/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [Blazor Security](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/)

## 🏆 Bug Bounty Program

Şu anda resmi bir bug bounty programımız yok, ancak güvenlik açığı raporları için:
- ⭐ GitHub'da özel teşekkür
- 📝 SECURITY.md'de katkıda bulunanlar listesi
- 🎉 LinkedIn'de özel mention (istege bağlı)

## 📞 İletişim

Güvenlikle ilgili sorular için:
- **Email**: rumeysatibet@example.com
- **Subject**: [SECURITY] Your Subject
- **PGP Key**: (henüz mevcut değil)

---

**Sorumlu açıklama politikasına uygun hareket etmeniz için teşekkür ederiz! 🙏**