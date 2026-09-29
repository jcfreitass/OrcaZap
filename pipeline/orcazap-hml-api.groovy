// Jenkins pipeline stub — homologação
// Loop over projectsToBuild, build + deploy each Lambda.
def projectsToBuild = [
    [folderlambdaName:'orcazap.user.criar',                lambdaName:'lambda-orcazap-user-criar'],
    [folderlambdaName:'orcazap.user.login',                lambdaName:'lambda-orcazap-user-login'],
    [folderlambdaName:'orcazap.user.listar',               lambdaName:'lambda-orcazap-user-listar'],
    [folderlambdaName:'orcazap.user.consultar',            lambdaName:'lambda-orcazap-user-consultar'],
    [folderlambdaName:'orcazap.customer.criar',            lambdaName:'lambda-orcazap-customer-criar'],
    [folderlambdaName:'orcazap.customer.listar',           lambdaName:'lambda-orcazap-customer-listar'],
    [folderlambdaName:'orcazap.customer.consultar',        lambdaName:'lambda-orcazap-customer-consultar'],
    [folderlambdaName:'orcazap.customer.atualizar',        lambdaName:'lambda-orcazap-customer-atualizar'],
    [folderlambdaName:'orcazap.customer.remover',          lambdaName:'lambda-orcazap-customer-remover'],
    [folderlambdaName:'orcazap.service.criar',             lambdaName:'lambda-orcazap-service-criar'],
    [folderlambdaName:'orcazap.service.listar',            lambdaName:'lambda-orcazap-service-listar'],
    [folderlambdaName:'orcazap.service.consultar',         lambdaName:'lambda-orcazap-service-consultar'],
    [folderlambdaName:'orcazap.service.atualizar',         lambdaName:'lambda-orcazap-service-atualizar'],
    [folderlambdaName:'orcazap.service.remover',           lambdaName:'lambda-orcazap-service-remover'],
    [folderlambdaName:'orcazap.quote.criar',               lambdaName:'lambda-orcazap-quote-criar'],
    [folderlambdaName:'orcazap.quote.listar',              lambdaName:'lambda-orcazap-quote-listar'],
    [folderlambdaName:'orcazap.quote.consultar',           lambdaName:'lambda-orcazap-quote-consultar'],
    [folderlambdaName:'orcazap.quote.atualizar.status',    lambdaName:'lambda-orcazap-quote-atualizar-status'],
    [folderlambdaName:'orcazap.quote.remover',             lambdaName:'lambda-orcazap-quote-remover'],
    [folderlambdaName:'orcazap.quote.whatsapp.link',       lambdaName:'lambda-orcazap-quote-whatsapp-link'],
]

pipeline {
    agent any
    environment {
        AWS_REGION = 'us-east-1'
        ENV_ALIAS  = 'hml'
    }
    stages {
        stage('Restore + Build + Test') {
            steps {
                sh 'dotnet restore OrcaZap.sln'
                sh 'dotnet build OrcaZap.sln --no-restore -c Release'
                sh 'dotnet test test/orcazap.tests/orcazap.tests.csproj --no-build -c Release'
            }
        }
        stage('Deploy Lambdas') {
            steps {
                script {
                    projectsToBuild.each { p ->
                        dir("src/${p.folderlambdaName}") {
                            sh "dotnet lambda deploy-function ${p.lambdaName} --config-file aws-lambda-tools-defaults.json"
                        }
                    }
                }
            }
        }
    }
}
