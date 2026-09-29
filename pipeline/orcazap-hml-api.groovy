// Jenkins pipeline — homologação
// Mesmos passos de deploy/deploy.ps1: testes -> SAM (Lambdas + API + RDS + CloudFront) -> front no S3.
// Credenciais Jenkins (Secret text): orcazap-hml-db-password, orcazap-hml-jwt-secret.
def common

pipeline {
    agent any
    environment {
        AWS_REGION = 'us-east-1'
        ENV_ALIAS  = 'hml'
        STACK_NAME = "orcazap-${ENV_ALIAS}"
    }
    stages {
        stage('Restore + Build + Test') {
            steps {
                script {
                    common = load 'pipeline/lib/common.groovy'
                    common.restoreBuildTest()
                }
            }
        }
        stage('Deploy backend (SAM)') {
            steps {
                withCredentials([
                    string(credentialsId: "orcazap-${ENV_ALIAS}-db-password", variable: 'DB_PASSWORD'),
                    string(credentialsId: "orcazap-${ENV_ALIAS}-jwt-secret",  variable: 'JWT_SECRET'),
                ]) {
                    script { common.samDeploy(env.STACK_NAME, env.ENV_ALIAS, env.AWS_REGION) }
                }
            }
        }
        stage('Deploy frontend') {
            steps {
                script { common.deployFrontend(env.STACK_NAME, env.AWS_REGION) }
            }
        }
    }
}
