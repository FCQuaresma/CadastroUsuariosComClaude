# CadastroUsuariosComClaude

API de gerenciamento de usuários (cadastro, edição, exclusão lógica, listagem) com autenticação JWT baseada em roles (`Admin`, `User`), consumida por um frontend em Angular.

Projeto de estudo: backend em **.NET 8** com arquitetura em camadas e **Dapper** (sem EF Core), frontend em **Angular 18**.

## Stack

**Backend**
- .NET 8 / C# 12
- Dapper + Microsoft.Data.SqlClient (SQL Server)
- Autenticação JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- FluentValidation
- BCrypt.Net-Next (hash de senha, workFactor 12)
- Swashbuckle (Swagger)

**Frontend**
- Angular 18 (standalone components)
- PrimeNG + PrimeFlex
- RxJS

## Arquitetura (backend)

```
UserManagementApi
├── UserManagementApi.Api            → Controllers, Program.cs, Swagger, JWT, middlewares
├── UserManagementApi.Application    → ViewModels/DTOs, AppServices (orquestram casos de uso)
├── UserManagementApi.Services       → Regras de negócio, validações (FluentValidation), hash de senha
├── UserManagementApi.Domain         → Entidades, interfaces de repositório
├── UserManagementApi.Data           → Dapper, ConnectionFactory, Repositories (SQL explícito)
├── UserManagementApi.CrossCutting   → ApiResponse<T>, exceções customizadas
└── database/                        → Scripts SQL versionados
```

Fluxo de uma requisição: `Controller → AppService → Service → Repository → SQL Server`.

## Como rodar

### Pré-requisitos
- .NET 8 SDK
- Node.js + npm
- SQL Server local/instância existente (o projeto não usa Docker)

### Backend

1. Crie o banco e rode o script `database/01-create-users-table.sql`.
2. Configure a connection string em `UserManagementApi.Api/appsettings.json` (ou `appsettings.Development.json`) se o seu SQL Server não for `localhost\SQLEXPRESS`.
3. Configure a chave JWT via user-secrets (nunca commitar a chave):
   ```bash
   cd UserManagementApi.Api
   dotnet user-secrets set "Jwt:Key" "uma-chave-secreta-bem-grande-aqui"
   ```
4. Rode a API:
   ```bash
   dotnet run --project UserManagementApi.Api
   ```
5. Swagger disponível em `http://localhost:5006/swagger` (a porta pode variar conforme `launchSettings.json`).

### Primeiro acesso (usuário admin)

O script SQL não cria nenhum usuário, e o endpoint `POST /api/User` exige role `Admin` para criar novos usuários — ou seja, não há como se auto-cadastrar como admin pela API. Para o primeiro acesso, insira o usuário admin diretamente no banco com uma senha já hasheada em BCrypt (workFactor 12):

```sql
INSERT INTO Users (Nome, Email, SenhaHash, Role, Ativo)
VALUES ('Admin', 'admin@exemplo.com', '<hash-bcrypt-da-senha>', 'Admin', 1);
```

### Frontend

```bash
cd AprendendoClaudeAngular
npm install
npm start
```

Acesse em `http://localhost:4200`. A URL da API é configurada em `src/environments/environment.ts` / `environment.development.ts`.

## Segurança

- Senhas sempre com hash BCrypt, nunca texto puro.
- Exclusão de usuário sempre lógica (`Ativo = 0`), nunca `DELETE` físico.
- Toda query Dapper é parametrizada.
- Exceções de negócio tratadas centralmente pelo `ExceptionMiddleware` — sem stack trace exposto ao cliente.
- CORS restrito à origem do frontend (`http://localhost:4200` em dev).

## Estado atual

- [x] Backend: CRUD de usuários, autenticação JWT com roles
- [x] Frontend: login, listagem e formulário de usuários, guards de rota por role
- [ ] Testes automatizados (xUnit)
- [ ] Refresh token
- [ ] Deploy do backend (Vercel não suporta .NET nativamente — em avaliação)
