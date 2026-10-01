# Veroeffentlicht die Blazor-WebAssembly-App als statische Dateien nach docs/.
# Den Ordner docs/ dann bei einem statischen Hoster hochladen
# (z. B. Netlify: Site oeffnen -> Deploys -> Ordner per Drag & Drop).
#
# Aufruf:  ./publish.ps1                 (App liegt im Root der Domain)
#          ./publish.ps1 -BasePath /Sudoku/   (App liegt in einem Unterordner)

param([string]$BasePath = "/")

$ErrorActionPreference = "Stop"
$repoPath = $BasePath
$root     = $PSScriptRoot
$docs     = Join-Path $root "docs"
$publish  = Join-Path $root "SudokuWeb/bin/Release/net9.0/publish/wwwroot"

Write-Host "1/4  Release-Build wird erstellt..." -ForegroundColor Cyan
dotnet publish (Join-Path $root "SudokuWeb") -c Release | Out-Null

Write-Host "2/4  docs/ wird neu befuellt..." -ForegroundColor Cyan
if (Test-Path $docs) { Remove-Item $docs -Recurse -Force }
New-Item -ItemType Directory -Path $docs | Out-Null
Copy-Item (Join-Path $publish "*") $docs -Recurse -Force

Write-Host "3/4  base href wird auf $repoPath gesetzt..." -ForegroundColor Cyan
$indexPath = Join-Path $docs "index.html"
$html = Get-Content $indexPath -Raw
$html = $html -replace '<base href="/" />', "<base href=`"$repoPath`" />"
Set-Content $indexPath $html -Encoding utf8 -NoNewline

Write-Host "4/4  .nojekyll + 404.html werden erstellt..." -ForegroundColor Cyan
# .nojekyll: falls doch mal GitHub Pages genutzt wird (sonst ignoriert es _framework)
New-Item -ItemType File -Path (Join-Path $docs ".nojekyll") -Force | Out-Null
# 404.html: Fallback, damit Direktlinks / Reload funktionieren
Copy-Item $indexPath (Join-Path $docs "404.html") -Force

Write-Host "Fertig! docs/ ist bereit zum Pushen." -ForegroundColor Green
