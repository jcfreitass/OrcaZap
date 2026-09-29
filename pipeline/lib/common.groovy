// Utilitários comuns para os pipelines Jenkins do OrcaZap.
def restoreBuildTest() {
    sh 'dotnet restore OrcaZap.sln'
    sh 'dotnet build OrcaZap.sln --no-restore -c Release'
    sh 'dotnet test test/orcazap.tests/orcazap.tests.csproj --no-build -c Release'
}

return this
