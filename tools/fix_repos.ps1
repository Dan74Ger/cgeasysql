$files = Get-ChildItem src\CGEasy.Core\Repositories\*.cs,src\CGEasy.Core\Services\*.cs
$count = 0

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw -Encoding UTF8
    $modified = $false
    
    if ($content -match 'LiteDbContext') {
        $content = $content -replace 'private readonly LiteDbContext','private readonly CGEasyDbContext'
        $content = $content -replace 'LiteDbContext _context','CGEasyDbContext _context'
        $content = $content -replace 'LiteDbContext context\)','CGEasyDbContext context)'
        $modified = $true
    }
    
    if ($modified) {
        Set-Content $file.FullName -Value $content -NoNewline -Encoding UTF8
        Write-Host "OK $($file.Name)" -ForegroundColor Green
        $count++
    }
}

Write-Host "`nTotale: $count file sistemati" -ForegroundColor Cyan

