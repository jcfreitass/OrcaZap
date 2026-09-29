using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using DotNetEnv;
using Microsoft.AspNetCore.Http;
using orcazap.local.host;

Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "dev");

var dir = new DirectoryInfo(AppContext.BaseDirectory);
while (dir != null && !File.Exists(Path.Combine(dir.FullName, ".env")))
    dir = dir.Parent;

if (dir != null)
{
    var envPath = Path.Combine(dir.FullName, ".env");
    Env.Load(envPath);
    var cs = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
    if (!string.IsNullOrWhiteSpace(cs))
    {
        Environment.SetEnvironmentVariable("connection_orcazap_mysql_read", cs);
        Environment.SetEnvironmentVariable("connection_orcazap_mysql_write", cs);
    }
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
    if (!string.IsNullOrWhiteSpace(jwtSecret))
        Environment.SetEnvironmentVariable("jwt_secret", jwtSecret);
    Console.WriteLine($"[host] .env carregado de {envPath}");
}
else
{
    Console.WriteLine("[host] .env não encontrado — usando appsettings.json");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:5173", "http://localhost:3000")
     .AllowAnyHeader()
     .AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

async Task Dispatch(
    HttpContext ctx,
    string routeKey,
    Func<APIGatewayHttpApiV2ProxyRequest, ILambdaContext, APIGatewayHttpApiV2ProxyResponse> handler,
    Dictionary<string, string> pathParameters = null)
{
    var req = await LambdaBridge.BuildRequest(ctx, routeKey, pathParameters);
    var response = handler(req, new FakeLambdaContext());
    await LambdaBridge.WriteResponse(ctx, response);
}

// Users
app.MapGet("/api/users", ctx =>
    Dispatch(ctx, "GET /api/users", new orcazap.user.listar.Function().FunctionHandler));

app.MapGet("/api/users/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "GET /api/users/{id}", new orcazap.user.consultar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapPost("/api/users", ctx =>
    Dispatch(ctx, "POST /api/users", new orcazap.user.criar.Function().FunctionHandler));

app.MapPost("/api/users/login", ctx =>
    Dispatch(ctx, "POST /api/users/login", new orcazap.user.login.Function().FunctionHandler));

// Customers
app.MapGet("/api/customers", ctx =>
    Dispatch(ctx, "GET /api/customers", new orcazap.customer.listar.Function().FunctionHandler));

app.MapGet("/api/customers/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "GET /api/customers/{id}", new orcazap.customer.consultar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapPost("/api/customers", ctx =>
    Dispatch(ctx, "POST /api/customers", new orcazap.customer.criar.Function().FunctionHandler));

app.MapPut("/api/customers/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "PUT /api/customers/{id}", new orcazap.customer.atualizar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapDelete("/api/customers/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "DELETE /api/customers/{id}", new orcazap.customer.remover.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

// Services
app.MapGet("/api/services", ctx =>
    Dispatch(ctx, "GET /api/services", new orcazap.service.listar.Function().FunctionHandler));

app.MapGet("/api/services/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "GET /api/services/{id}", new orcazap.service.consultar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapPost("/api/services", ctx =>
    Dispatch(ctx, "POST /api/services", new orcazap.service.criar.Function().FunctionHandler));

app.MapPut("/api/services/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "PUT /api/services/{id}", new orcazap.service.atualizar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapDelete("/api/services/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "DELETE /api/services/{id}", new orcazap.service.remover.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

// Quotes
app.MapGet("/api/quotes", ctx =>
    Dispatch(ctx, "GET /api/quotes", new orcazap.quote.listar.Function().FunctionHandler));

app.MapGet("/api/quotes/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "GET /api/quotes/{id}", new orcazap.quote.consultar.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapPost("/api/quotes", ctx =>
    Dispatch(ctx, "POST /api/quotes", new orcazap.quote.criar.Function().FunctionHandler));

app.MapPut("/api/quotes/{id}/status", (HttpContext ctx, string id) =>
    Dispatch(ctx, "PUT /api/quotes/{id}/status", new orcazap.quote.atualizar.status.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapDelete("/api/quotes/{id}", (HttpContext ctx, string id) =>
    Dispatch(ctx, "DELETE /api/quotes/{id}", new orcazap.quote.remover.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapGet("/api/quotes/{id}/whatsapp-link", (HttpContext ctx, string id) =>
    Dispatch(ctx, "GET /api/quotes/{id}/whatsapp-link", new orcazap.quote.whatsapp.link.Function().FunctionHandler,
        new Dictionary<string, string> { ["id"] = id }));

app.MapGet("/", () => Results.Ok(new { name = "orcazap.local.host", endpoints = "GET/POST/PUT/DELETE /api/{users,customers,services,quotes}" }));

app.Run("http://localhost:5000");
