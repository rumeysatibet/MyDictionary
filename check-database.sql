-- MyDictionary veritabanı kontrolü
USE mydictionarydb;

-- Tüm tabloları listele
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Users tablosundaki kayıtları kontrol et
SELECT COUNT(*) as UserCount FROM Users;

-- Son 10 kullanıcıyı göster
SELECT TOP 10 
    Id, 
    Username, 
    Email, 
    IsEmailVerified, 
    CreatedAt,
    Gender,
    BirthDate
FROM Users 
ORDER BY CreatedAt DESC;

-- Email verification kayıtlarını kontrol et
SELECT COUNT(*) as EmailVerificationCount FROM EmailVerifications;

-- Son 5 email verification kaydını göster
SELECT TOP 5 
    Id,
    UserId,
    Email,
    VerificationCode,
    IsUsed,
    CreatedAt,
    ExpiresAt
FROM EmailVerifications
ORDER BY CreatedAt DESC;

-- User Agreement kayıtlarını kontrol et
SELECT COUNT(*) as UserAgreementCount FROM UserAgreements;

-- User Agreement Acceptance kayıtlarını kontrol et
SELECT COUNT(*) as AcceptanceCount FROM UserAgreementAcceptances;