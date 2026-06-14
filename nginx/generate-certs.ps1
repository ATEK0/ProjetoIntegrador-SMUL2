# ============================================================
# Gerar certificado TLS auto-assinado para localhost
# Usa OpenSSL dentro de um container Docker (Alpine)
# ============================================================

$ErrorActionPreference = "Stop"

$CertsDir = Join-Path $PSScriptRoot "certs"

# Criar diretório de certificados se não existir
if (-not (Test-Path $CertsDir)) {
    New-Item -ItemType Directory -Path $CertsDir | Out-Null
    Write-Host "[+] Diretorio 'nginx/certs/' criado." -ForegroundColor Green
}

# Verificar se já existem certificados
if ((Test-Path (Join-Path $CertsDir "selfsigned.crt")) -and (Test-Path (Join-Path $CertsDir "selfsigned.key"))) {
    Write-Host "[!] Certificados ja existem em 'nginx/certs/'. Para regenerar, apague os ficheiros existentes." -ForegroundColor Yellow
    exit 0
}

Write-Host "[*] A gerar certificado auto-assinado com OpenSSL (via Docker)..." -ForegroundColor Cyan

# Criar ficheiro de configuração OpenSSL temporário
$OpenSSLConf = @"
[req]
default_bits       = 2048
prompt             = no
default_md         = sha256
distinguished_name = dn
x509_extensions    = v3_req

[dn]
C  = PT
ST = Lisboa
L  = Lisboa
O  = SMUL2-Dev
CN = localhost

[v3_req]
subjectAltName = @alt_names
keyUsage       = digitalSignature, keyEncipherment
extendedKeyUsage = serverAuth

[alt_names]
DNS.1 = localhost
DNS.2 = api.localhost
DNS.3 = *.localhost
IP.1  = 127.0.0.1
IP.2  = ::1
"@

$TempConfPath = Join-Path $CertsDir "openssl.cnf"
$OpenSSLConf | Out-File -FilePath $TempConfPath -Encoding UTF8 -NoNewline

# Converter caminho para formato Docker (Windows → Linux mount)
$DockerCertsPath = $CertsDir -replace '\\', '/' -replace '^([A-Za-z]):', '/$1'
$DockerCertsPath = $DockerCertsPath.Substring(0, 1) + $DockerCertsPath.Substring(1, 1).ToLower() + $DockerCertsPath.Substring(2)

# Executar OpenSSL via Docker
docker run --rm `
    -v "${DockerCertsPath}:/certs" `
    alpine/openssl `
    req -x509 -nodes -days 365 -newkey rsa:2048 `
    -keyout /certs/selfsigned.key `
    -out /certs/selfsigned.crt `
    -config /certs/openssl.cnf

if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Erro ao gerar certificados!" -ForegroundColor Red
    exit 1
}

# Remover ficheiro de configuração temporário
Remove-Item -Path $TempConfPath -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "[OK] Certificados gerados com sucesso!" -ForegroundColor Green
Write-Host "     - nginx/certs/selfsigned.crt (certificado)" -ForegroundColor White
Write-Host "     - nginx/certs/selfsigned.key (chave privada)" -ForegroundColor White
Write-Host ""
Write-Host "[!] NOTA: Este e um certificado auto-assinado." -ForegroundColor Yellow
Write-Host "    O browser vai mostrar um aviso de seguranca - e esperado." -ForegroundColor Yellow
