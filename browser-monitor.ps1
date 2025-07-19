# Tarayıcı izleme ve otomatik temizlik scripti
# Bu script tarayıcı kapatıldığında MyDictionary process'lerini temizler

Write-Host "Tarayıcı izleme başlatılıyor..." -ForegroundColor Green

$browserProcesses = @("chrome", "firefox", "msedge", "iexplore")
$monitoringActive = $true

while ($monitoringActive) {
    $activeBrowsers = @()
    
    foreach ($browser in $browserProcesses) {
        $process = Get-Process -Name $browser -ErrorAction SilentlyContinue
        if ($process) {
            $activeBrowsers += $browser
        }
    }
    
    if ($activeBrowsers.Count -eq 0) {
        Write-Host "Tarayıcı bulunamadı, MyDictionary process'leri temizleniyor..." -ForegroundColor Yellow
        
        # Cleanup MyDictionary processes
        $myDictProcesses = Get-Process -Name "*MyDictionary*" -ErrorAction SilentlyContinue
        if ($myDictProcesses) {
            $myDictProcesses | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host "MyDictionary process'leri temizlendi." -ForegroundColor Green
        }
        
        $monitoringActive = $false
        break
    }
    
    Start-Sleep -Seconds 5
}

Write-Host "İzleme tamamlandı." -ForegroundColor Blue