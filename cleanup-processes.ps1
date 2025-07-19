# MyDictionary Process Cleanup Script
# Bu script tüm MyDictionary ile ilgili process'leri temizler

Write-Host "MyDictionary Process Cleanup başlatılıyor..." -ForegroundColor Yellow

# MyDictionary process'lerini bul ve sonlandır
$processes = Get-Process -Name "*MyDictionary*" -ErrorAction SilentlyContinue

if ($processes.Count -gt 0) {
    Write-Host "Bulunan MyDictionary process'leri:" -ForegroundColor Green
    foreach ($process in $processes) {
        Write-Host "  - $($process.Name) (PID: $($process.Id))"
    }
    
    Write-Host "Process'ler sonlandırılıyor..." -ForegroundColor Yellow
    $processes | Stop-Process -Force -ErrorAction SilentlyContinue
    
    Start-Sleep -Seconds 2
    Write-Host "Temizlik tamamlandı!" -ForegroundColor Green
} else {
    Write-Host "MyDictionary process'i bulunamadı." -ForegroundColor Blue
}

# Portları kontrol et
Write-Host "Port kullanımı kontrol ediliyor..." -ForegroundColor Yellow
$ports = @(17222, 22122, 5000, 5001, 7000, 7001)

foreach ($port in $ports) {
    $connection = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($connection) {
        Write-Host "Port $port kullanımda: PID $($connection.OwningProcess)" -ForegroundColor Red
        $portProcess = Get-Process -Id $connection.OwningProcess -ErrorAction SilentlyContinue
        if ($portProcess) {
            Write-Host "  Process: $($portProcess.Name)" -ForegroundColor Red
        }
    }
}

Write-Host "Cleanup işlemi tamamlandı!" -ForegroundColor Green