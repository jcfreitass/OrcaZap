using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using core;
using core.Common.Notificacao.NotificacoesAgrupador;
using core.Configuration;
using core.DI;
using core.Dominio.Retorno;
using core.Dominio.Users;
using core.Dominio.Users.DTO;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using core.Security;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Net;

[assembly: LambdaSerializer(typeof(SourceGeneratorLambdaJsonSerializer<HttpApiJsonSerializerContext>))]

namespace orcazap.user.criar
{
    public class Function : FunctionBase
    {
        private IUserRepositorio _userRepositorio;
        private IPasswordHasher _passwordHasher;
        private IJwtService _jwtService;
        private ILogger _log;
        private NotificacoesAgrupador _notificacoes;
        private readonly bool _isUnitTest;

        [ExcludeFromCodeCoverage]
        public Function() { }

        public Function(IUserRepositorio userRepositorio, IPasswordHasher passwordHasher, IJwtService jwtService, ILogger log, bool isUnitTest)
        {
            _userRepositorio = userRepositorio;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _log = log;
            _isUnitTest = isUnitTest;
        }

        public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest req, ILambdaContext lambdaContext)
        {
            SetEnvironment(req);
            Startup();

            try
            {
                var dto = JsonConvert.DeserializeObject<CreateUserDto>(req.Body ?? string.Empty);
                if (InvalidRequest(dto, _notificacoes, _log) ||
                    string.IsNullOrWhiteSpace(dto?.Name) ||
                    string.IsNullOrWhiteSpace(dto?.Email) ||
                    string.IsNullOrWhiteSpace(dto?.Password))
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "Nome, e-mail e senha são obrigatórios.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                if (_userRepositorio.ObterPorEmail(dto.Email) != null)
                {
                    _notificacoes.AddNotificacao("VALIDACAO", "Já existe um usuário cadastrado com este e-mail.");
                    return CreateResponse(HttpStatusCode.BadRequest, new RetornoBase(HttpStatusCode.BadRequest, _notificacoes.Notificacoes));
                }

                var user = new User
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    PasswordHash = _passwordHasher.Hash(dto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                user.Id = _userRepositorio.Inserir(user);

                var token = _jwtService.GerarToken(user.Id, user.Email);

                return CreateResponse(HttpStatusCode.Created, new RetornoBase(HttpStatusCode.Created, _notificacoes.Notificacoes, AuthResponseDto.Criar(token, user)));
            }
            catch (Exception ex)
            {
                return LogErrorReturnInternalServerError(ex, _log, _notificacoes, "criar usuário", lambdaContext);
            }
        }

        [ExcludeFromCodeCoverage]
        protected override void GetRepositories()
        {
            if (!_isUnitTest)
            {
                _userRepositorio = ResolvedorDependencia.ObterServico<IUserRepositorio>();
                _passwordHasher = ResolvedorDependencia.ObterServico<IPasswordHasher>();
                _jwtService = ResolvedorDependencia.ObterServico<IJwtService>();
                _log = ResolvedorDependencia.ObterServico<ILogger>();
            }
            _notificacoes = ResolvedorDependencia.ObterServico<NotificacoesAgrupador>();
        }
    }
}
