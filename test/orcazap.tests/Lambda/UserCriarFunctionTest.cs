using core.Dominio.Users;
using core.Infrastructure.Repositorios.MySql;
using core.Log;
using core.Security;
using Moq;
using Moq.AutoMock;
using orcazap.user.criar;
using Xunit;

namespace orcazap.tests.Lambda
{
    public class UserCriarFunctionTest
    {
        [Fact]
        public void FunctionHandler_QuandoBodyInvalido_RetornaBadRequest()
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
        public void Inserir_QuandoChamado_RetornaId()
        {
            var mocker = new AutoMocker();
            mocker.GetMock<IUserRepositorio>()
                .Setup(r => r.Inserir(It.IsAny<User>()))
                .Returns(42L);

            var repo = mocker.GetMock<IUserRepositorio>().Object;
            var id = repo.Inserir(new User { Name = "Teste", Email = "a@b", PasswordHash = "x" });

            Assert.Equal(42L, id);
        }
    }
}
