# ProjetoIntegrador-SMUL2

Este repositório contém atualmente 2 componentes ativos:

1. `MS-CAFIN/calc_service` - microserviço em Django
2. `Portal` - frontend em ASP.NET Core MVC

## Estrutura do projeto

```text
ProjetoIntegrador-SMUL2/
|- MS-CAFIN/calc_service
|- Portal
|- Docs
```

## Pré-requisitos

- Docker e Docker Compose (para correr o projeto de forma simplificada)
- Python 3.12+ e pip (caso corras o backend localmente)
- .NET SDK 10.0 (caso corras o frontend localmente)

Notas importantes:

- Em Docker, a base de dados usada é PostgreSQL. Se correres o projeto localmente via comando direto, o Django cria e usar um SQLite local por defeito.
- O frontend está configurado para `net10.0`; versões mais antigas do SDK .NET não compilam o projeto.

## Portas

- `MS-CAFIN`: `http://127.0.0.1:8000`
- `Portal` (HTTP): `http://localhost:5000`
- `Portal` (HTTPS): `https://localhost:5001`

## 1. Executar o MS-CAFIN (Django)

Entre na pasta do serviço:

```bash
cd MS-CAFIN/calc_service
```

Criar e ativar um ambiente virtual (opcional):

```bash
python3 -m venv .venv
source .venv/bin/activate
```

Instalar Django:

```bash
pip install django==6.0.4
```

Executar migrações:

```bash
python manage.py migrate
```

Arrancar o servidor:

```bash
python manage.py runserver 127.0.0.1:8000
```

URLs de acesso:

- `http://127.0.0.1:8000/`
- `http://127.0.0.1:8000/admin/`

## 2. Executar o Portal (ASP.NET Core MVC)

Entre na pasta do frontend:

```bash
cd Portal
```

Restaurar dependências e arrancar a aplicação:

```bash
dotnet restore
dotnet run --launch-profile http
```

Para usar HTTPS:

```bash
dotnet run --launch-profile https
```

## Arranque rápido dos 2 servidores

Abra 2 terminais separados.

Terminal 1 (Django):

```bash
cd MS-CAFIN/calc_service
python3 -m venv .venv
source .venv/bin/activate
pip install django==6.0.4
python manage.py migrate
python manage.py runserver 127.0.0.1:8000
```

Terminal 2 (ASP.NET):

```bash
cd Portal
dotnet restore
dotnet run --launch-profile http
```

## 3. Executar com Docker Compose

O projeto suporta **Docker Compose** para correr o backend (Django), o frontend (ASP.NET) e a base de dados (PostgreSQL) simultaneamente, isolando todas as dependências do teu sistema.

Para iniciar tudo de uma vez com as imagens do GitHub Registry:

```bash
docker-compose up -d
```

Se quiseres compilar as imagens a partir do teu código fonte local, descomenta as propriedades `build` e `context` presentes no ficheiro `docker-compose.yml` e executa:

```bash
docker-compose up -d --build
```

Os serviços ficarão disponíveis em:
- **Frontend (Portal)**: `http://localhost:8080`
- **Backend (MS-CAFIN)**: `http://localhost:8000`

Para parar todos os contentores:
```bash
docker-compose down
```

## Integração Contínua (CI/CD)

O repositório utiliza **GitHub Actions** para automação, cujos workflows estão guardados na diretoria `.github/workflows/`:

1. **CI - Django Microserviço (`ci-django.yml`)**:
   - É ativado em pushes/pull requests para as pastas do backend (`MS-CAFIN/**`).
   - Garante que a formatação do código (com *black* e *flake8*) está correta e executa testes ao Django.
   - Na *branch main*, cria e publica a imagem Docker no GitHub Container Registry (`ghcr.io`).

2. **CI - ASP.NET Portal (`ci-aspnet.yml`)**:
   - É ativado em pushes/pull requests para a diretoria frontend (`Portal/**`).
   - Garante que a aplicação .NET 10 compila sem erros, verifica regras de estilo (`dotnet format`) e executa testes unitários/integrados.
   - Na *branch main*, cria e publica a imagem Docker no GitHub Container Registry (`ghcr.io`).

## Estado atual do repositório

- Existe 1 projeto Django (`MS-CAFIN/calc_service`), já com `requirements.txt` e `Dockerfile`.
- Existe 1 projeto ASP.NET Core MVC (`Portal`), já atualizado para `.NET 10.0` e com `Dockerfile`.
- O ficheiro `docker-compose.yml` está na raiz preparado para orquestrar os dois projetos.
