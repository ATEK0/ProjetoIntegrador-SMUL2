#!/bin/bash
# Este script faz backup das bases de dados MySQL (dec e calc_service)
# que correm dentro do container Docker e pode ser agendado como Cron Job.
# ==============================================================================

# Obter o caminho absoluto da pasta do script e da raiz do projeto
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Carregar variáveis do .env se existir
if [ -f "$PROJECT_ROOT/.env" ]; then
    set -a
    source "$PROJECT_ROOT/.env"
    set +a
fi

# Configurações
CONTAINER_NAME="${MYSQL_CONTAINER:-mysql_db}"
DB_USER="${MYSQL_USER:-root}"
DB_PASSWORD="${MYSQL_ROOT_PASSWORD:-root}"
DATABASES=("dec" "calc_service")
BACKUP_DIR="${PROJECT_ROOT}/backups"
RETENTION_DAYS=7

# Criar a pasta de backups se não existir
mkdir -p "$BACKUP_DIR"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")

echo "=== Iniciando Backup MySQL [$(date)] ==="

# 1. Verificar se o container está em execução
if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "ERRO: O container '${CONTAINER_NAME}' não está em execução!"
    exit 1
fi

# 2. Fazer backup de cada base de dados
ERRORS=0
for DB_NAME in "${DATABASES[@]}"; do
    BACKUP_FILE="${BACKUP_DIR}/${DB_NAME}_backup_${TIMESTAMP}.sql.gz"

    echo "A exportar base de dados '${DB_NAME}'..."
    if docker exec "$CONTAINER_NAME" mysqldump -u"$DB_USER" -p"$DB_PASSWORD" "$DB_NAME" 2>/dev/null | gzip > "$BACKUP_FILE"; then
        echo "  SUCESSO: ${BACKUP_FILE}"
    else
        echo "  ERRO: Falha ao exportar '${DB_NAME}'!"
        rm -f "$BACKUP_FILE"
        ERRORS=$((ERRORS + 1))
    fi
done

# 3. Aplicar política de retenção (remover backups com mais de X dias)
echo "A remover backups com mais de ${RETENTION_DAYS} dias..."
find "$BACKUP_DIR" -name "*_backup_*.sql.gz" -type f -mtime +"$RETENTION_DAYS" -delete

if [ "$ERRORS" -gt 0 ]; then
    echo "=== Backup concluído com ${ERRORS} erro(s)! ==="
    exit 1
fi

echo "=== Backup Concluído com Sucesso! ==="
