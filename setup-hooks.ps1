# ============================================================
# Setup dos Git Hooks e ferramentas de lint
# Correr uma vez apos clonar o repositorio
# ============================================================

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  Setup de Git Hooks e Lint Tools" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# --- Configurar Git para usar a pasta .githooks ---
Write-Host "[1/3] A configurar Git para usar .githooks/..." -ForegroundColor Yellow
git config core.hooksPath .githooks
Write-Host "      [OK] core.hooksPath = .githooks" -ForegroundColor Green
Write-Host ""

# --- Instalar ferramentas Python (flake8 + black) ---
Write-Host "[2/3] A instalar flake8 e black..." -ForegroundColor Yellow
pip install flake8 black --quiet
if ($LASTEXITCODE -eq 0) {
    Write-Host "      [OK] flake8 e black instalados." -ForegroundColor Green
} else {
    Write-Host "      [AVISO] Falha ao instalar. Certifica-te que tens Python/pip no PATH." -ForegroundColor Red
}
Write-Host ""

# --- Verificar dotnet ---
Write-Host "[3/3] A verificar .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = $null
try {
    $dotnetVersion = dotnet --version 2>$null
} catch {}

if ($dotnetVersion) {
    Write-Host "      [OK] .NET SDK $dotnetVersion detetado." -ForegroundColor Green
} else {
    Write-Host "      [AVISO] .NET SDK nao encontrado. Necessario para lint do Portal." -ForegroundColor Red
}

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "  Setup concluido!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""
Write-Host "O pre-commit hook ira agora executar automaticamente:" -ForegroundColor White
Write-Host "  - flake8 + black (Python/Django)" -ForegroundColor White
Write-Host "  - dotnet format (C#/ASP.NET)" -ForegroundColor White
Write-Host ""
Write-Host "Para testar manualmente: git commit --allow-empty -m 'test hook'" -ForegroundColor Yellow
Write-Host ""
