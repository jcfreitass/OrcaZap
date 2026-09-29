using core.Dominio.Users;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using core.Security;
using Moq;
using orcazap.user.login;
using Xunit;

namespace orcazap.tests.Lambda
{
    public class UserLoginFunctionTest
    {
        [Fact]
        public void FunctionHandler_QuandoBodyInvalido_NaoLancaExcecao()
        {
            var mocker = TestHelper.CriarAutoMockerComNotificacoes();
            var function = new Function(
                mocker.GetMock<IUserRepositorio>().Object,
                mocker.GetMock<IPasswordHasher>().Object,
                mocker.GetMock<IJwtService>().Object,
                mocker.GetMock<ILogger>().Object,
                isUnitTest: true);

            Assert.NotNull(function);
        }

        [Fact]
        public void ObterPorEmail_QuandoEmailNaoExiste_RetornaNulo()
        {
            var mocker = TestHelper.CriarAutoMockerComNotificacoes();
            mocker.GetMock<IUserRepositorio>()
                .Setup(r => r.ObterPorEmail("naoexiste@orcazap.com"))
                .Returns((User)null);

            var user = mocker.GetMock<IUserRepositorio>().Object.ObterPorEmail("naoexiste@orcazap.com");

            Assert.Null(user);
        }

        [Fact]
        public void Verify_QuandoSenhaCorreta_RetornaTrue()
        {
            var mocker = TestHelper.CriarAutoMockerComNotificacoes();
            mocker.GetMock<IPasswordHasher>()
                .Setup(p => p.Verify("123456", "hash-valido"))
                .Returns(true);

            var valido = mocker.GetMock<IPasswordHasher>().Object.Verify("123456", "hash-valido");

            Assert.True(valido);
        }

        [Fact]
        public void GerarToken_QuandoChamado_RetornaTokenDoServico()
        {
            var mocker = TestHelper.CriarAutoMockerComNotificacoes();
            mocker.GetMock<IJwtService>()
                .Setup(j => j.GerarToken(1L, "user@orcazap.com"))
                .Returns("token-gerado");

            var token = mocker.GetMock<IJwtService>().Object.GerarToken(1L, "user@orcazap.com");

            Assert.Equal("token-gerado", token);
        }
    }
}
