# ProjetoIntegrador-SMUL2

Este repositório contém atualmente 2 componentes ativos:

1. `MS-CAFIN/calc_service` - microserviço em Django
2. `Portal` - frontend em ASP.NET Core MVC

## Estrutura do projeto

```text
ProjetoIntegrador-SMUL2/
|- MS-CAFIN/calc_service
|- Portal
```

## Pré-requisitos

- Python 3.12+ recomendado
- pip
- .NET SDK 10.0

Notas importantes:

- O projeto Django usa SQLite local. A base de dados é criada após as migrações.
- Não existe ficheiro de dependências Python (`requirements.txt`) no repositório.
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

Criar e ativar um ambiente virtual:

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

## Estado atual do repositório

- Existe 1 projeto Django (`MS-CAFIN/calc_service`).
- Existe 1 projeto ASP.NET Core MVC (`Portal`).
- Não existe ainda ficheiro de dependências Python no repositório.
