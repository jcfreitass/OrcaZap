using core.Dominio.Customers;
using core.Infrastructure.Repositorios.MySql;
using Moq;
using Moq.AutoMock;
using Xunit;

namespace orcazap.tests.Services
{
    public class CustomerRepositorioContratoTest
    {
        [Fact]
        public void ObterTodos_QuandoSemUserId_RetornaLista()
        {
            var mocker = new AutoMocker();
            mocker.GetMock<ICustomerRepositorio>()
                .Setup(r => r.ObterTodos(null))
                .Returns(new List<Customer> { new() { Id = 1, Name = "Fulano" } });

            var repo = mocker.GetMock<ICustomerRepositorio>().Object;
            var lista = repo.ObterTodos();

            Assert.Single(lista);
        }
    }
}
