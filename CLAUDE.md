# CLAUDE.md

Contexto do projeto para o Claude Code (ou qualquer instância do Claude) trabalhar de forma consistente neste repositório.

## Modo de colaboração — LEIA ANTES DE QUALQUER AÇÃO

Este projeto é usado para **aprendizado**. Eu (desenvolvedor) sou iniciante e quero entender fundamentos, arquitetura e clean code — não apenas ter código pronto gerado. Por isso, valem as regras abaixo, que têm prioridade sobre qualquer outra instrução implícita de "ser produtivo" ou "resolver rápido":

1. **Nunca crie, edite ou apague arquivos sem eu pedir explicitamente e confirmar.** Não presuma que "a tarefa pede" a criação de um arquivo — pergunte antes.
2. **Nada de mudanças em lote.** Trabalhamos uma peça por vez (uma entidade, um repositório, um endpoint). Não adiante etapas futuras do checklist sozinho.
3. **Antes de mostrar código, explique o raciocínio**: por que essa classe existe, por que fica nessa camada, que problema ela resolve, que alternativas existiam. O código vem depois da explicação, não no lugar dela.
4. **Prefira perguntar "posso seguir para a próxima etapa?" a simplesmente seguir.** Meu silêncio não é permissão.
5. **Se eu escrever código com erro ou fora do padrão da arquitetura, aponte o problema e explique a correção — não corrija por mim silenciosamente.**
6. **Evite jargão sem explicação.** Se usar um termo técnico novo (ex: "injeção de dependência", "camada de domínio"), explique em 1-2 frases na primeira vez.
7. Este arquivo (`CLAUDE.md`) em si só deve ser alterado por mim ou com minha aprovação explícita linha a linha.
8. Sempre que uma nova classe/arquivo for criada, antes de mostrar o código relembre qual é a função da camada onde ela está sendo colocada (`Domain`, `Application`, `Services`, `Data`, `Api`, `CrossCutting`) — não presuma que já ficou memorizado de conversas anteriores.

## Visão geral

**UserManagementApi** — API de gerenciamento de usuários (cadastrar, editar, excluir, listar) com autenticação JWT baseada em roles (`Admin`, `User`).

Frontend: **Angular** consumindo a API.
Backend: **.NET 8**, arquitetura em camadas, **Dapper** (sem EF Core), **SQL Server**.

Este projeto segue a arquitetura do tutorial interno `SrvAppDynamicTutorial V3`, adaptada para o domínio de usuários. **Docker não está em uso no momento** — SQL Server local/instância existente.

## Arquitetura (backend)

```
UserManagementApi
├── UserManagementApi.Api            → Controllers, Program.cs, Swagger, JWT, middlewares
├── UserManagementApi.Application    → ViewModels/DTOs, AppServices (orquestram casos de uso)
├── UserManagementApi.Services       → Regras de negócio, validações (FluentValidation), hash de senha
├── UserManagementApi.Domain         → Entidades, interfaces de repositório, PagedRequest/PagedResult
├── UserManagementApi.Data           → Dapper, ConnectionFactory, Repositories (SQL explícito)
├── UserManagementApi.CrossCutting   → ApiResponse<T>, exceções customizadas
└── database/                        → Scripts SQL versionados
```

**Fluxo de dependência (regra dura, não violar):**
```
Api → Application, Services, Data, CrossCutting
Application → Services, Domain, CrossCutting
Services → Domain, CrossCutting
Data → Domain, CrossCutting
Domain → nenhuma dependência de infraestrutura
CrossCutting → nenhuma dependência de infraestrutura
```

Domain nunca pode referenciar Data, Application ou Api.

## Fluxo de uma requisição

```
Controller → AppService → Service → Repository → SQL Server
```

- **Controller**: fina. Recebe HTTP, delega, devolve `ApiResponse<T>`. Nunca contém SQL ou regra de negócio.
- **AppService**: converte DTO/ViewModel ↔ entidade, chama Service, empacota em `ApiResponse<T>`.
- **Service**: regra de negócio, validações (FluentValidation), hash de senha.
- **Repository**: SQL explícito via Dapper, sempre parametrizado.

## Convenções de código

- C# 12 / .NET 8, nullable reference types habilitado.
- Entidades e ViewModels em português nos nomes de domínio (`Nome`, `Email`, `Ativo`), mas nomes técnicos de classes/métodos em inglês (`UserService`, `CreateAsync`).
- DTOs de entrada/saída como `record`.
- Exclusão sempre lógica (`Ativo = 0`), nunca `DELETE` físico.
- Toda query Dapper usa parâmetros nomeados (`@Id`, `@Email`) — SQL concatenado com input do usuário é proibido.
- Exceções de negócio: `NotFoundException` (404) e `BusinessException` (400), tratadas pelo `ExceptionMiddleware` central — não usar `try/catch` espalhado nos controllers.
- Toda resposta da API passa por `ApiResponse<T>.Ok(...)` ou `ApiResponse<T>.Fail(...)`.

## Segurança (não negociável)

- Senhas **sempre** com hash BCrypt (`workFactor: 12`), nunca texto puro — inclusive para o usuário admin inicial.
- Coluna do banco chama-se `SenhaHash`, nunca `Password` em claro.
- JWT: chave (`Jwt:Key`) nunca commitada — usar `dotnet user-secrets` em dev e variável de ambiente/secret manager em produção.
- Endpoints protegidos com `[Authorize(Roles = "...")]`; leitura permitida a `Admin,User`, escrita restrita a `Admin`.
- CORS restrito ao domínio do frontend Angular (`http://localhost:4200` em dev).
- Erros internos nunca vazam stack trace ao cliente — só mensagem genérica + log via Serilog.
- Sem Docker por ora: connection string aponta para instância local/on-premise do SQL Server.

## Comandos

```bash
# Restaurar, compilar, rodar
dotnet restore
dotnet build
dotnet run --project UserManagementApi.Api

# Migrations/schema: não usamos EF migrations (Dapper puro).
# Alterações de schema vão em novo script numerado em database/, ex: database/02-add-coluna-x.sql

# Swagger
http://localhost:5169/swagger

# Login de teste
POST http://localhost:5169/api/Auth/login
{ "email": "admin@exemplo.com", "senha": "SenhaForte123" }
```

## Frontend (Angular)

```
src/app/
├── core/auth/       → auth.service, auth.guard, role.guard, jwt.interceptor
├── features/users/  → user-list, user-form, user.service
└── shared/
```

- Todas as respostas da API vêm no formato `ApiResponse<T>` (`{ success, message, data, errors }`) — os services Angular devem desembrulhar `data` antes de repassar ao componente.
- Rotas de escrita (`/users/new`, `/users/edit/:id`) protegidas por `roleGuard(['Admin'])`.
- Token JWT decodificado no client só para leitura de claims (role, exp) — nunca confiar nele para autorização real; a autorização de verdade é sempre validada no backend.

## O que evitar

- Não colocar SQL ou regra de negócio em Controllers.
- Não usar EF Core (o projeto usa Dapper deliberadamente).
- Não introduzir Docker sem alinhar antes — está fora de escopo por ora.
- Não expor `SenhaHash` em nenhuma ViewModel/DTO de resposta.
- Não fazer `DELETE` físico de usuário.

## Estado atual / próximos passos

- [x] Estrutura de camadas definida (no papel — nenhum arquivo de código criado ainda)
- [x] CRUD de usuários (backend) especificado (no papel)
- [x] JWT com roles especificado (no papel)
- [ ] Implementação real, camada por camada, com explicação — começando por Domain
- [ ] Frontend Angular consumindo a API (ajustado ao formato `ApiResponse<T>`)
- [ ] Testes automatizados (xUnit no backend)
- [ ] Refresh token (avaliar necessidade)

**Próxima etapa sugerida**: criar a solution e os projetos vazios (sem código ainda), entendendo por que cada camada é um projeto `.csproj` separado — só avançar para a entidade `User` depois disso ficar claro.
