-- ============================================================
-- Script de inicialização do MySQL
-- Executado automaticamente na primeira vez que o container
-- mysql_db é criado (via docker-entrypoint-initdb.d)
-- ============================================================

-- Base de dados para o backend Django (MS-CAFIN)
CREATE DATABASE IF NOT EXISTS `calc_service`
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

-- A base de dados 'dec' (frontend ASP.NET) é criada
-- automaticamente pela variável MYSQL_DATABASE no docker-compose.
