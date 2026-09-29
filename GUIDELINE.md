# GUIDELINE.md

Convenções de código específicas deste repositório, baseadas em exemplos reais em `src/` e `test/`. Fluxo de desenvolvimento e visão geral: `AGENTS.md`.

## Nomenclatura

- **Lambdas/projetos**: pasta `src/orcazap.<dominio>.<acao>/` com `.csproj` sem o prefixo `orcazap.` (ex.: pasta `src/orcazap.customer.criar/` → `customer.criar.csproj`).
- **Interfaces de repositório**: `I<Entidade>Repositorio` / `<Entidade>Repositorio` (PT, sufixo `Repositorio`). Todos em `src/orcazap.core/Infrastructure/Repositorios/MySql/`.
- **Models/DTOs**: em `src/orcazap.core/Dominio/<Entidade>/` e `src/orcazap.core/Dominio/<Entidade>/DTO/`. DTOs de saída expõem método estático `DePara(entidade)` para converter da entidade.
- **Enums**: `src/orcazap.core/Dominio/Enums/` com prefixo `E` (ex.: `EQuoteStatus`).

## Padrão de Lambda/função

- **DI**: service locator estático — `core.DI.ResolvedorDependencia.ObterServico<T>()`, chamado de dentro do método `GetRepositories()` de cada `Function.cs`. Ver referência em `src/orcazap.customer.criar/Function.cs`.
- **Trigger**: HTTP via API Gateway V2 (`APIGatewayHttpApiV2ProxyRequest`/`APIGatewayHttpApiV2ProxyResponse`).
- **Assinatura fixa do handler**: `public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest req, ILambdaContext lambdaContext)`.
- **Fluxo padrão dentro do handler**:
  1. `SetEnvironment(req)` — carrega config e hidrata contexto do usuário (IP; `IdUsuario`/`EmailUsuario` só depois do guard de autenticação, ver abaixo).
  2. `Startup()` — resolve repositórios via `GetRepositories()`.
  3. **Se a Lambda exige login** (todas exceto `user.criar` e `user.login`): `if (AutenticacaoInvalida(req, _notificacoes)) return CreateResponse(HttpStatusCode.Unauthorized, new RetornoBase(HttpStatusCode.Unauthorized, _notificacoes.Notificacoes));` — valida o JWT (`Authorization: Bearer <token>`) e popula `IdUsuario`/`EmailUsuario`.
  4. Validar entrada; se inválida, `_notificacoes.AddNotificacao(...)` e retornar `CreateResponse(HttpStatusCode.BadRequest, ...)`.
  5. Executar operação de negócio.
  6. Retornar `CreateResponse(HttpStatusCode.OK, new RetornoBase(...))`.
  7. `catch` genérico → `LogErrorReturnInternalServerError(ex, _log, _notificacoes, "descrição da ação", lambdaContext)`.
- **Resposta/retorno**: `FunctionBase.CreateResponse(HttpStatusCode, object)` serializa com `Newtonsoft.Json`.
- **Construtor duplo**: `public Function()` sem parâmetros para runtime da AWS + `public Function(...deps, bool isUnitTest)` para testes injetarem mocks. Marque o construtor sem parâmetros como `[ExcludeFromCodeCoverage]`.

## Autenticação e escopo por dono

- **JWT próprio**: emitido por `core.Security.IJwtService` (HS256, claims `sub`=IdUsuario e `email`, expira em 8h), assinado com a chave `jwt_secret` (`core.Configuration.JwtSettings`, mesmo padrão de `DbSettings`). Validado em `ClienteHelper.AutenticacaoInvalida`, chamado por toda Lambda protegida.
- **Senha**: hash via `core.Security.IPasswordHasher` (BCrypt). Nunca comparar/gravar senha em texto puro nem usar hash sem salt.
- **`user.criar`/`user.login`** são as únicas Lambdas públicas (sem o guard) — `user.criar` já devolve um `AuthResponseDto { Token, User }` (login automático no cadastro).
- **Nunca confiar em `UserId`/`userId` vindo do client** (body ou query string). Toda operação de Customer/Service/Quote usa `IdUsuario` (do token validado) — nas Lambdas de `criar`, ao montar a entidade; nas de `listar`, como filtro fixo; nas de `consultar`/`atualizar`/`remover`, como parte do `WHERE` no repositório (`AND user_id = @userId`). Um registro de outro usuário (ou inexistente) responde 404 — não existe checagem explícita de "dono" com 403, o próprio filtro da query já garante o isolamento.
- **Toda Lambda nova que expõe dado de Customer/Service/Quote deve seguir esse mesmo escopo por dono** — não crie uma versão que aceite `userId` do cliente "por conveniência".

## Acesso a dados

- **Biblioteca**: Dapper (sem ORM/Migrations). Ver `src/orcazap.core/Infrastructure/Repositorios/MySql/CustomerRepositorio.cs`.
- **Conexão**: `Func<int, MySqlDbConnection>` factory injetada no construtor de cada repositório via `RepositorioBase`. `_conexao.ObterConexaoLeitura()` vs `_conexao.ObterConexaoEscrita()`.
- **Transação**: quando há múltiplos INSERTs relacionados (ex.: quote + quote_items), use `conn.BeginTransaction()` explicitamente — ver `QuoteRepositorio.Inserir`.
- **Schema**: `mysql-server/schema.sql` é a fonte da verdade. Sem Migrations.

## Tratamento de erro e log

- **Log**: `core.Log.ILogger` (`Logger` — wrapper console). `LogInfo`, `LogError`, `LogDebug`.
- **Try/catch**: relance com `throw;` para preservar a stack; deixe o catch genérico do `Function.cs` responder HTTP 500 via `LogErrorReturnInternalServerError`.
- **Resposta de erro ao cliente**: sempre mensagem genérica ("Ocorreu um erro ao tentar processar sua solicitação..."). Stack trace só no log interno, nunca na resposta HTTP.
- **Notificações**: use `NotificacoesAgrupador` (`AddNotificacao(chave, mensagem)`) para acumular mensagens de validação/negócio que voltam no `RetornoBase.Notificacoes`.

## Testes

- **Framework**: xUnit + Moq + Moq.AutoMock (`test/orcazap.tests/orcazap.tests.csproj`).
- **Convenção de nome**: `<Classe>Test.cs`, método `Metodo_Cenario_Resultado` (`[Fact]`/`[Theory]`).
- **Padrão**: use `TestHelper.CriarAutoMockerComNotificacoes()` e instancie a `Function` pelo construtor de teste (`isUnitTest: true`) para injetar mocks.
- **Rodar**: `dotnet test test/orcazap.tests/orcazap.tests.csproj`.

## Pipeline (Jenkins) e Deploy

- **Arquivos**: `pipeline/orcazap-{hml,prd}-api.groovy` + `pipeline/lib/{common.groovy,awsLambda.groovy}`.
- **Adicionar Lambda nova ao pipeline**: par `folderlambdaName`/`lambdaName` em cada `.groovy` (ex.: `[folderlambdaName:'orcazap.customer.criar', lambdaName:'lambda-orcazap-customer-criar']`).
- **Deploy manual (dev)**: `cd src/orcazap.<lambda>/ && dotnet lambda deploy-function` (requer AWS Lambda Tools instalado: `dotnet tool install -g Amazon.Lambda.Tools`).

## Host local de dev (`orcazap.local.host`)

- **Não vai pra prod.** É um `Sdk.Web` que existe só pra permitir rodar as 19 Lambdas localmente sem AWS Lambda Tools/SAM.
- **Fluxo**: cada `app.MapGet/Post/Put/Delete` monta um `APIGatewayHttpApiV2ProxyRequest` a partir do `HttpContext` (via `LambdaBridge.BuildRequest`), instancia a `Function` da Lambda alvo, chama `FunctionHandler` passando um `FakeLambdaContext`, e devolve o `APIGatewayHttpApiV2ProxyResponse` no `HttpResponse` (via `LambdaBridge.WriteResponse`).
- **Config**: `Program.cs` procura `.env` subindo diretórios a partir de `AppContext.BaseDirectory` e injeta `DB_CONNECTION_STRING` como env var `connection_orcazap_mysql_read`/`_write`, sobrescrevendo o `appsettings.json` via `AddEnvironmentVariables()`.
- **CORS**: liberado só para `http://localhost:5173` (Vite) e `http://localhost:3000`.
- **Ao adicionar Lambda nova**: (1) crie o Lambda em `src/orcazap.<x>/`, (2) adicione `<ProjectReference>` em `orcazap.local.host.csproj`, (3) mapeie a rota em `Program.cs`.

## Frontend (`frontend/`)

- **Stack**: Vite + React 18 + TypeScript strict. Sem lib de estado — `useState`/`useEffect` bastam.
- **Estrutura**: `src/api.ts` (fetch tipado agrupado por entidade), `src/types.ts` (contratos que espelham `RetornoBase`/DTOs do backend), `src/components/{Login,Users,Customers,Services,Quotes}Panel.tsx` (CRUD por entidade), `src/App.tsx` (nav de tabs + gate de autenticação).
- **Convenção de nomes**: PascalCase nos DTOs (`Id`, `Name`, `UserId`) porque o backend serializa com `Newtonsoft.Json` sem `CamelCasePropertyNamesContractResolver` — o front consome como veio.
- **Autenticação**: `App.tsx` só renderiza as abas com um token válido em `localStorage` (`orcazap_token`) — sem token, mostra `LoginPanel`. `api.ts` anexa `Authorization: Bearer <token>` em toda chamada e limpa o token num 401. Nenhum formulário de Customer/Service/Quote pede "usuário" — isso vem do token no backend.
- **Comandos**: `npm install`, `npm run dev` (http://localhost:5173), `npm run build` (type-check + bundle).

## Observações

- Nenhum Lambda deste repo depende de KMS, S3, SQS ou outros serviços AWS ainda — só API Gateway + MySQL. Ao adicionar integração AWS, criar um wrapper em `src/orcazap.core/Services/` (`IAwsService`) e registrar em `ResolvedorDependencia`.
- `appsettings.dev.json` local não deve ser versionado com credenciais reais — o `appsettings.json` versionado usa valores de desenvolvimento local (root/root@localhost).
