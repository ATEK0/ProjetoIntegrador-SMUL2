# Documentação de Backup MySQL

## Visão Geral

O projeto utiliza um único script de backup (`backup.sh`) que exporta as duas bases de dados MySQL (`dec` e `calc_service`) do container Docker `mysql_db`, comprime os ficheiros com gzip e aplica uma política de retenção automática.

### Bases de Dados

| Base de Dados   | Utilização                          |
|-----------------|-------------------------------------|
| `dec`           | Frontend ASP.NET (Portal)           |
| `calc_service`  | Backend Django (MS-CAFIN)           |

### Estrutura de Ficheiros

```
ProjetoIntegrador-SMUL2/
├── scripts/
│   └── backup/
│       └── backup.sh              # Script de backup
├── backups/
│   ├── dec_backup_20260613_020000.sql.gz
│   ├── calc_service_backup_20260613_020000.sql.gz
│   └── .gitignore
├── docker-compose.yml
└── .env
```

---

## Pré-requisitos

- Docker e Docker Compose instalados
- Container `mysql_db` em execução
- Bash disponível (Linux/macOS ou WSL/Git Bash no Windows)

---

## Executar Backup Manualmente

```bash
# Dar permissão de execução (apenas na primeira vez)
chmod +x scripts/backup/backup.sh

# Executar o backup
./scripts/backup/backup.sh
```

### Saída Esperada

```
=== Iniciando Backup MySQL [Sex Jun 13 02:00:01 UTC 2026] ===
A exportar base de dados 'dec'...
  SUCESSO: /caminho/backups/dec_backup_20260613_020000.sql.gz
A exportar base de dados 'calc_service'...
  SUCESSO: /caminho/backups/calc_service_backup_20260613_020000.sql.gz
A remover backups com mais de 7 dias...
=== Backup Concluído com Sucesso! ===
```

O script cria dois ficheiros por execução:
- `dec_backup_<timestamp>.sql.gz`
- `calc_service_backup_<timestamp>.sql.gz`

---

## Configuração como Cron Job

Para agendar o backup automaticamente, editar o crontab:

```bash
crontab -e
```

Adicionar uma das seguintes linhas (substituir o caminho absoluto):

```cron
# Backup diário às 02:00
0 2 * * * /caminho/para/ProjetoIntegrador-SMUL2/scripts/backup/backup.sh >> /caminho/para/ProjetoIntegrador-SMUL2/backups/backup.log 2>&1
```

### Referência Rápida de Cron

```
┌───────────── minuto (0 - 59)
│ ┌───────────── hora (0 - 23)
│ │ ┌───────────── dia do mês (1 - 31)
│ │ │ ┌───────────── mês (1 - 12)
│ │ │ │ ┌───────────── dia da semana (0 - 7, 0 e 7 = domingo)
│ │ │ │ │
* * * * *  comando
```

| Expressão          | Significado                  |
|--------------------|------------------------------|
| `0 2 * * *`        | Todos os dias às 02:00       |
| `0 */6 * * *`      | A cada 6 horas               |
| `0 3 * * 0`        | Domingos às 03:00            |
| `30 1 * * 1-5`     | Dias úteis às 01:30          |

### Verificar/Remover cron job

```bash
# Ver cron jobs ativos
crontab -l

# Remover todos os cron jobs
crontab -r
```

---

## Política de Retenção

O script remove automaticamente backups com mais de **7 dias**. Este valor pode ser alterado diretamente no script na variável `RETENTION_DAYS`.

---

## Restauro Manual

Para restaurar um backup manualmente:

```bash
# Restaurar a base de dados 'dec'
gunzip -c backups/dec_backup_20260613_020000.sql.gz | docker exec -i mysql_db mysql -u root -proot

# Restaurar a base de dados 'calc_service'
gunzip -c backups/calc_service_backup_20260613_020000.sql.gz | docker exec -i mysql_db mysql -u root -proot
```

> **⚠️ ATENÇÃO:** O restauro **substitui** todos os dados existentes na base de dados alvo.

---

## Configurações do Script

O script lê as credenciais do ficheiro `.env` na raiz do projeto. Caso não exista, usa os valores padrão:

| Variável             | Padrão      | Descrição                |
|----------------------|-------------|--------------------------|
| `MYSQL_CONTAINER`    | `mysql_db`  | Nome do container Docker |
| `MYSQL_USER`         | `root`      | Utilizador MySQL         |
| `MYSQL_ROOT_PASSWORD`| `root`      | Password MySQL           |

---

## Troubleshooting

### "Container não está em execução"

```bash
# Verificar containers em execução
docker ps

# Iniciar o container
docker compose --profile develop up -d mysql_db
```

### "Falha ao exportar a base de dados"

```bash
# Testar conexão e verificar se as bases de dados existem
docker exec mysql_db mysql -u root -proot -e "SHOW DATABASES;"
```

### "Permissão negada ao executar script"

```bash
chmod +x scripts/backup/backup.sh
```

---

## Comandos Úteis

```bash
# Listar backups existentes
ls -lh backups/*.sql.gz

# Ver tamanho das bases de dados
docker exec mysql_db mysql -u root -proot -e "
  SELECT table_schema AS 'Base de Dados',
         ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) AS 'Tamanho (MB)'
  FROM information_schema.tables
  WHERE table_schema IN ('dec', 'calc_service')
  GROUP BY table_schema;"

# Listar tabelas e contagem de registos
docker exec mysql_db mysql -u root -proot -e "
  SELECT TABLE_SCHEMA, TABLE_NAME, TABLE_ROWS
  FROM information_schema.tables
  WHERE table_schema IN ('dec', 'calc_service')
  ORDER BY TABLE_SCHEMA, TABLE_NAME;"
```
