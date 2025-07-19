-- SQL Server Management Studio'da çalıştırılacak komutlar

-- 1. Tüm veritabanlarını listele
SELECT name FROM sys.databases
WHERE name LIKE '%mydictionary%' OR name LIKE '%MyDictionary%';

-- 2. Eğer veritabanı varsa, ona bağlan
USE mydictionarydb;

-- 3. Veritabanındaki tüm tabloları listele
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 4. Users tablosundaki kayıtları kontrol et
SELECT * FROM Users;

-- 5. Email verification kayıtlarını kontrol et
SELECT * FROM EmailVerifications;

-- 6. User Agreement kayıtlarını kontrol et
SELECT * FROM UserAgreements;

-- 7. User Agreement Acceptance kayıtlarını kontrol et
SELECT * FROM UserAgreementAcceptances;