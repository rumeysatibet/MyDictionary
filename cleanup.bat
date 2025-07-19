@echo off
echo MyDictionary Process Cleanup...

taskkill /im MyDictionary.Web.exe /f >nul 2>&1
taskkill /im MyDictionary.ApiService.exe /f >nul 2>&1
taskkill /im MyDictionary.AppHost.exe /f >nul 2>&1

echo Process cleanup tamamlandi!

echo Port kontrolu...
netstat -ano | findstr :17222
netstat -ano | findstr :22122

echo Cleanup islemi tamamlandi!
pause