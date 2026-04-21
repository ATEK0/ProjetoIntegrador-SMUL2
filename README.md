# ProjetoIntegrador-SMUL2

Este repositório contém 3 componentes independentes:

1. `MS-CAFIN/calc_service` - microserviço em Django
2. `PortalBackend/api` - backend em Django
3. `PortalFrontend` - frontend em ASP.NET Core MVC

Este README descreve como preparar e executar todos os servidores em ambiente local.

## Estrutura do projeto

```text
ProjetoIntegrador-SMUL2/
|- MS-CAFIN/calc_service
|- PortalBackend/api
|- PortalFrontend
```

## Pré-requisitos

- Python 3.12+ recomendado
- pip
- .NET SDK 10.0

Notas importantes (pré-development):

- Os dois projetos Django foram gerados com Django 6.0.4 e não existe atualmente um ficheiro `requirements.txt` no repositório.
- O frontend está configurado para `net10.0`. Um SDK .NET 9 não consegue compilar nem executar este projeto.
- Os projetos Django usam SQLite local, por isso a base de dados é criada localmente após as migrações.

## Portas

- `MS-CAFIN`: `http://127.0.0.1:8000`
- `PortalBackend`: `http://127.0.0.1:8001`
- `PortalFrontend`: `http://localhost:5000`

## 1. Executar o MS-CAFIN

Entre na pasta do serviço:

```bash
cd MS-CAFIN/calc_service
```

Criar e ativar um ambiente virtual:

```bash
python3 -m venv .venv
source .venv/bin/activate
```

Instalar o Django:

```bash
pip install django==6.0.4
```

Executar as migrações:

```bash
python manage.py migrate
```

Arrancar o servidor:

```bash
python manage.py runserver 127.0.0.1:8000
```

URL de acesso:

- `http://127.0.0.1:8000/`
- `http://127.0.0.1:8000/admin/`

## 2. Executar o PortalBackend

Entrar na pasta do backend:

```bash
cd PortalBackend/api
```

Criar e ativar um ambiente virtual:

```bash
python3 -m venv .venv
source .venv/bin/activate
```

Instalar o Django:

```bash
pip install django==6.0.4
```

Executar as migrações:

```bash
python manage.py migrate
```

Arrancar o servidor:

```bash
python manage.py runserver 127.0.0.1:8001
```

URL de acesso:

- `http://127.0.0.1:8001/`
- `http://127.0.0.1:8001/admin/`

## 3. Executar o PortalFrontend

Entre na pasta do frontend:

```bash
cd PortalFrontend
```

Restaurar as dependências e arranque a aplicação:

```bash
dotnet restore
dotnet run
```

A aplicação arranca com estes endpoints:

- `http://localhost:5000`
- `https://localhost:5001`

Para usar o perfil HTTP:

```bash
dotnet run --launch-profile http
```

Para usar o perfil HTTPS:

```bash
dotnet run --launch-profile https
```

## Arranque rápido dos 3 servidores

Abra 3 terminais separados.

Terminal 1:

```bash
cd MS-CAFIN/calc_service
python3 -m venv .venv
source .venv/bin/activate
pip install django==6.0.4
python manage.py migrate
python manage.py runserver 127.0.0.1:8000
```

Terminal 2:

```bash
cd PortalBackend/api
python3 -m venv .venv
source .venv/bin/activate
pip install django==6.0.4
python manage.py migrate
python manage.py runserver 127.0.0.1:8001
```

Terminal 3:

```bash
cd PortalFrontend
dotnet restore
dotnet run --launch-profile http
```

## Estado atual do repositório


- o `PortalFrontend` é o único projeto presente na solução `.sln`
- os dois projetos Django têm apenas a rota padrão de administração ativa
- não existe ainda ficheiro de dependências Python no repositório
