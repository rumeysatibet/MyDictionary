-- LocalDB'deki MyDictionary.Dev veritabanını kontrol et
USE [MyDictionary.Dev];

-- Tüm tabloları listele
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Users tablosunu kontrol et
SELECT COUNT(*) as UserCount FROM Users;

-- Tablodaki tüm sütunları göster
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION;

-- Son 10 kullanıcıyı göster (varsa)
SELECT TOP 10 * FROM Users ORDER BY CreatedAt DESC;

-- Email verifications tablosunu kontrol et
SELECT COUNT(*) as EmailVerificationCount FROM EmailVerifications;

-- User agreements tablosunu kontrol et
SELECT COUNT(*) as UserAgreementCount FROM UserAgreements;

-- User agreement acceptances tablosunu kontrol et
SELECT COUNT(*) as AcceptanceCount FROM UserAgreementAcceptances;