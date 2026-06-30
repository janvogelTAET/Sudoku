# Veroeffentlicht die Blazor-WebAssembly-App als statische Dateien nach docs/,
# damit GitHub Pages sie ausliefern kann.
#
# Aufruf:  ./publish.ps1
# Danach:  git add docs && git commit -m "update" && git push
#
# Wenn deine Freundin spielt:  https://janvogeltaet.github.io/Sudoku-with-C-and-WPF/

$ErrorActionPreference = "Stop"
$repoPath = "/Sudoku-with-C-and-WPF/"   # Unterordner-Pfad auf GitHub Pages
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
# .nojekyll: damit GitHub Pages den _framework-Ordner (mit _) nicht ignoriert
New-Item -ItemType File -Path (Join-Path $docs ".nojekyll") -Force | Out-Null
# 404.html: Fallback, damit Direktlinks / Reload funktionieren
Copy-Item $indexPath (Join-Path $docs "404.html") -Force

Write-Host "Fertig! docs/ ist bereit zum Pushen." -ForegroundColor Green
