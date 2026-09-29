# AGENTS.md

Ponto de entrada obrigatório para qualquer IA trabalhando neste repositório. Leia por completo antes de tocar em código.

## 1. O que é este repositório

Backend (.NET 8) do **OrcaZap** — sistema de orçamentos para autônomos/pequenos negócios com integração de envio via WhatsApp. Cada pasta em `src/` é o seu próprio projeto AWS Lambda; não existe Controllers/ASP.NET Core Web API tradicional neste repo.

O repo é um espelho didático do padrão arquitetural do projeto **Cockpit MaisFidelidade** (`webmotors.cockpit.maisfidelidade.api`), aplicado a um domínio menor (Users / Customers / Services / Quotes) para estudo.

## 2. Stack técnica (confirmada em código)

- **.NET 8** — `<TargetFramework>net8.0</TargetFramework>` em todos os `.csproj`.
- **Arquitetura**: Lambda-por-função — um `.csproj` por Lambda (`<AWSProjectType>Lambda</AWSProjectType>`) dentro da solution `OrcaZap.sln` (23 projetos: 20 Lambdas + `orcazap.core` + `orcazap.tests` + `orcazap.local.host`).
- **Autenticação**: JWT próprio (HS256), assinado e validado dentro do repo (não há authorizer externo). Login em `orcazap.user.login`; hashing de senha via BCrypt (`core.Security.IPasswordHasher`). Toda Lambda protegida chama `AutenticacaoInvalida(req, _notificacoes)` (em `ClienteHelper`/`FunctionBase`) logo após `Startup()` e retorna 401 se inválido. Nenhum dado de Customer/Service/Quote é lido/gravado usando um `userId` vindo do client — sempre o `IdUsuario` do token (populado pela validação do JWT), inclusive nas queries de `consultar`/`atualizar`/`remover` (`WHERE ... AND user_id = @userId`, retornando 404 se o registro não pertence ao usuário).
- **Padrão de DI**: service locator estático — `core.DI.ResolvedorDependencia.ObterServico<T>()`, chamado de dentro do método `GetRepositories()` de cada `Function.cs`. Toda Lambda nova deve seguir esse padrão.
- **Acesso a dados**: Dapper em `src/orcazap.core/Infrastructure/Repositorios/MySql/` (ex.: `CustomerRepositorio.cs`). Conexão via `Func<int, MySqlDbConnection>` factory injetada pelo `ResolvedorDependencia`. Sem EF Core, sem Migrations — schema versionado em `mysql-server/schema.sql`.
- **Serialização**: `Amazon.Lambda.Serialization.SystemTextJson` com `SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>` para o parsing do request; corpo de resposta serializado com `Newtonsoft.Json` (`FunctionBase.CreateResponse`).
- **Trigger padrão**: HTTP via API Gateway V2 (`APIGatewayHttpApiV2ProxyRequest`/`Response`).
- **Testes**: xUnit + Moq + Moq.AutoMock (`test/orcazap.tests/orcazap.tests.csproj`).

## 3. Comandos reais

```bash
# build da solution inteira
dotnet restore OrcaZap.sln
dotnet build OrcaZap.sln

# build de uma Lambda isolada
dotnet build src/orcazap.customer.criar/customer.criar.csproj

# testes
dotnet test test/orcazap.tests/orcazap.tests.csproj

# deploy completo na AWS (Lambdas + API Gateway + RDS + front no S3/CloudFront) — ver docs/deploy.md
./deploy/deploy.ps1 -Env hml
```

**Rodar tudo local (dev):**
```bash
# 1. Subir MySQL local (uma vez por sessão)
mysql-server/start-mysql.bat
# 1a. Criar schema (idempotente)
mysql-server/bin/mysql.exe -uroot -p < mysql-server/schema.sql   # senha: DB_PASSWORD do .env

# 2. Subir o host local que roteia HTTP → FunctionHandler das 20 Lambdas
cd src/orcazap.local.host && dotnet run   # http://localhost:5000

# 3. Subir o front (outro terminal)
cd frontend && npm install && npm run dev  # http://localhost:5173
```

Credenciais MySQL em `.env` na raiz (dev local).

## 4. Estrutura de pastas real

```
src/
├── orcazap.core/                       domínio, services, repositórios compartilhados (referenciado por TODOS os Lambdas)
│   ├── DI/ResolvedorDependencia.cs     service locator estático
│   ├── FunctionBase.cs                  base compartilhada de toda Function (Startup, CreateResponse, tratamento de erro)
│   ├── Dominio/                         Users, Customers, Services, Quotes, Retorno, Enums
│   ├── Infrastructure/Repositorios/     repositórios Dapper
│   ├── Configuration/                   ConfigurationManager, DbSettings, HttpApiJsonSerializerContext
│   ├── Common/                          Notificações, Helpers (ClienteHelper: contexto do usuário + guard de autenticação), Attributes
│   ├── Security/                        IPasswordHasher (BCrypt), IJwtService (emissão/validação do JWT)
│   └── Log/                             ILogger, Logger, LogLevelSettings
├── orcazap.user.{criar,login,listar,consultar}/                 4 Lambdas
├── orcazap.customer.{criar,listar,consultar,atualizar,remover}/ 5 Lambdas
├── orcazap.service.{criar,listar,consultar,atualizar,remover}/  5 Lambdas
├── orcazap.quote.{criar,listar,consultar,atualizar.status,remover,whatsapp.link}/ 6 Lambdas
│                                        (Lambda nova: adicionar também em cloudformation/orcazap.yaml e no orcazap.local.host)
└── orcazap.local.host/                  DEV-ONLY — ASP.NET Core minimal API que despacha HTTP → FunctionHandler
                                         das 20 Lambdas (não vai pra prod; existe só pra rodar tudo local)
test/
└── orcazap.tests/                       projeto único referenciando os 21 projetos de produção
frontend/                                Vite + React + TypeScript (SPA; API em VITE_API_URL, default http://localhost:5000)
pipeline/                                Jenkins Groovy: testes → sam deploy → build do front → S3/CloudFront
cloudformation/orcazap.yaml              template SAM único (param EnvAlias=hml|prd): Lambdas, API GW, VPC, RDS, S3, CloudFront
deploy/deploy.ps1                        deploy local equivalente ao pipeline
mysql-server/                            servidor MySQL portable + schema.sql (fonte da verdade do schema)
docs/                                    especificações de features (padrão docs/spec/[slug]/)
```

## 5. Regras para a IA não alucinar

- **Nunca afirme que um repositório, service ou Lambda existe sem antes ler o arquivo real.**
- **Nunca invente contrato de request/response ou schema de tabela.** Verifique em `Function.cs`, repositório e `mysql-server/schema.sql`.
- **Todo Lambda novo deve seguir o padrão**: extender `FunctionBase`, obter dependências via `ResolvedorDependencia.ObterServico<T>()` dentro de `GetRepositories()`, responder com `CreateResponse(HttpStatusCode, RetornoBase)`, tratar erro com `LogErrorReturnInternalServerError`.
- **Convenção de nome de repositório**: `I<Entidade>Repositorio` + `<Entidade>Repositorio` (PT, sufixo `Repositorio`).
- **Ao referenciar código em respostas, cite `arquivo:linha`.**
- **Nunca grave secret, connection string ou token em código versionado.** Use `appsettings.<env>.json` local (ignorado por git) ou variáveis de ambiente da Lambda.

## 6. Convenções de commit

1. Sempre partir de `main`/`dev` atualizada.
2. Branch como `feature/<nome-descritivo>`.
3. Mensagem de commit com `## Problema`, `## O que mudou`, `## Testes`.

## 7. Ver também

- `GUIDELINE.md` — convenções de código deste repositório.
- `docs/spec/[slug]/` — especificações por feature (PRD + Tech Spec + Tasks).
