using core.Common.Notificacao.NotificacoesAgrupador;
using Moq.AutoMock;

namespace orcazap.tests
{
    public static class TestHelper
    {
        public static AutoMocker CriarAutoMockerComNotificacoes()
        {
            var mocker = new AutoMocker();
            mocker.Use(new NotificacoesAgrupador());
            return mocker;
        }
    }
}
