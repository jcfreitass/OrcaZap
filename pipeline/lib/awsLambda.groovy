// Deploy helper para Lambdas via `dotnet lambda deploy-function`.
def deployLambda(String folder, String name) {
    dir("src/${folder}") {
        sh "dotnet lambda deploy-function ${name} --config-file aws-lambda-tools-defaults.json"
    }
}

return this
