# Rotas do Projeto Portal (Dinheiro em Casa)

Este documento lista as rotas (endpoints) existentes no projeto ASP.NET MVC.

## Account
Rotas relacionadas à autenticação e registo de utilizadores.

* **Login:** `/Account/Login`
  * Rota para a página de início de sessão.
* **Registo:** `/Account/Register`
  * Rota para a página de registo de novos utilizadores.

## Dashboard
Rotas para os diferentes painéis de controlo, baseados nos papéis (roles) dos utilizadores.

* **Admin:** `/Dashboard/Admin`
  * Dashboard para os administradores da plataforma.
* **Aluno:** `/Dashboard/Aluno`
  * Dashboard para os alunos (estudantes).
* **Professor:** `/Dashboard/Professor`
  * Dashboard para os professores.

## Home
Rotas principais da aplicação.

* **Index:** `/Home/Index` ou `/`
  * Página principal (Home) da plataforma.
