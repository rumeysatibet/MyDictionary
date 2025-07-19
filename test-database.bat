@echo off
echo MyDictionary Database Test...
echo.

echo 1. Veritabanlarini listele:
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "SELECT name FROM sys.databases"

echo.
echo 2. MyDictionary.Dev tabloları:
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"

echo.
echo 3. Users tablosu kayit sayisi:
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT COUNT(*) as UserCount FROM Users"

echo.
echo 4. Users tablosu yapisini goster:
sqlcmd -S "(localdb)\MSSQLLocalDB" -d "MyDictionary.Dev" -Q "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users'"

pause