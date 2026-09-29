// Utilitários comuns para os pipelines Jenkins do OrcaZap.
def restoreBuildTest() {
    sh 'dotnet restore OrcaZap.sln'
    sh 'dotnet build OrcaZap.sln --no-restore -c Release'
    sh 'dotnet test test/orcazap.tests/orcazap.tests.csproj --no-build -c Release'
}

// Build + deploy das 20 Lambdas, API Gateway, RDS e CloudFront (cloudformation/orcazap.yaml).
// Espera DB_PASSWORD e JWT_SECRET no ambiente (withCredentials).
def samDeploy(String stackName, String envAlias, String region) {
    sh 'sam build --template-file cloudformation/orcazap.yaml --parallel'
    sh """
        sam deploy --stack-name ${stackName} --region ${region} --resolve-s3 \\
          --capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND \\
          --no-confirm-changeset --no-fail-on-empty-changeset \\
          --parameter-overrides EnvAlias=${envAlias} DbPassword="\$DB_PASSWORD" JwtSecret="\$JWT_SECRET"
    """
}

def stackOutput(String stackName, String region, String key) {
    return sh(returnStdout: true, script: """
        aws cloudformation describe-stacks --stack-name ${stackName} --region ${region} \\
          --query "Stacks[0].Outputs[?OutputKey=='${key}'].OutputValue" --output text
    """).trim()
}

// Build do React apontando para a API do stack e upload para o S3 + invalidação do CloudFront.
def deployFrontend(String stackName, String region) {
    def apiUrl = stackOutput(stackName, region, 'ApiUrl')
    def bucket = stackOutput(stackName, region, 'FrontendBucketName')
    def distributionId = stackOutput(stackName, region, 'FrontendDistributionId')

    dir('frontend') {
        sh 'npm ci'
        withEnv(["VITE_API_URL=${apiUrl}"]) {
            sh 'npm run build'
        }
    }
    sh "aws s3 sync frontend/dist s3://${bucket} --delete --region ${region}"
    sh "aws cloudfront create-invalidation --distribution-id ${distributionId} --paths '/*'"
}

return this
