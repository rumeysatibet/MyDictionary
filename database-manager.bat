@echo off
:menu
cls
echo ========================================
echo   MyDictionary Database Manager
echo ========================================
echo.
echo 1. Veritabani durumunu kontrol et
echo 2. Kullanicilari listele
echo 3. Yeni kayitlari goster
echo 4. Tum tablolari listele
echo 5. Email verification kayitlarini goster
echo 6. Cikis
echo.
set /p choice="Seciminizi yapin (1-6): "

if "%choice%"=="1" goto check_db
if "%choice%"=="2" goto list_users
if "%choice%"=="3" goto show_new_users
if "%choice%"=="4" goto list_tables
if "%choice%"=="5" goto show_emails
if "%choice%"=="6" goto exit

goto menu

:check_db
echo.
echo === Veritabani Durumu ===
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases WHERE name = 'MyDictionary.Dev'"
echo.
pause
goto menu

:list_users
echo.
echo === Tum Kullanicilar ===
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT Id, Username, Email, CreatedAt FROM Users ORDER BY CreatedAt DESC"
echo.
pause
goto menu

:show_new_users
echo.
echo === Son 5 Kullanici ===
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT TOP 5 Id, Username, Email, IsEmailVerified, CreatedAt FROM Users ORDER BY CreatedAt DESC"
echo.
pause
goto menu

:list_tables
echo.
echo === Tum Tablolar ===
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME"
echo.
pause
goto menu

:show_emails
echo.
echo === Email Verification Kayitlari ===
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT TOP 5 Email, VerificationCode, IsUsed, CreatedAt FROM EmailVerifications ORDER BY CreatedAt DESC"
echo.
pause
goto menu

:exit
echo Cikiliyor...
exit