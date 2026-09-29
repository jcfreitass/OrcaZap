<#
.SYNOPSIS
  Deploy completo do OrcaZap na AWS: testes -> Lambdas + API + RDS + CloudFront (SAM) -> build e upload do front.

.EXAMPLE
  ./deploy/deploy.ps1 -Env hml
  ./deploy/deploy.ps1 -Env prd -AdminCidr 200.10.20.30/32

.NOTES
  Pré-requisitos: AWS CLI (com `aws configure` feito), SAM CLI, .NET 8 SDK, Node.js.
  Segredos: lidos de $env:ORCAZAP_DB_PASSWORD / $env:ORCAZAP_JWT_SECRET; se vazios, o script pergunta.
  Use SEMPRE os mesmos valores em cada deploy do mesmo ambiente (trocar a senha aqui troca a do RDS).
#>
param(
    [Parameter(Mandatory)][ValidateSet('hml', 'prd')][string]$Env,
    [string]$Region = 'us-east-1',
    [string]$AdminCidr = '',
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent
$stack = "orcazap-$Env"

function Invoke-Checked([string]$what, [scriptblock]$cmd) {
    Write-Host "==> $what" -ForegroundColor Cyan
    & $cmd
    if ($LASTEXITCODE -ne 0) { throw "Falhou: $what (exit $LASTEXITCODE)" }
}

function Read-Secret([string]$envVar, [string]$prompt) {
    $value = [Environment]::GetEnvironmentVariable($envVar)
    if ($value) { return $value }
    $secure = Read-Host -AsSecureString $prompt
    return [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure))
}

foreach ($tool in 'aws', 'sam', 'dotnet', 'npm') {
    if (-not (Get-Command $tool -ErrorAction SilentlyContinue)) { throw "'$tool' não encontrado no PATH." }
}

$dbPassword = Read-Secret 'ORCAZAP_DB_PASSWORD' "Senha do RDS ($Env)"
$jwtSecret = Read-Secret 'ORCAZAP_JWT_SECRET' "JWT secret ($Env)"

Push-Location $root
try {
    if (-not $SkipTests) {
        Invoke-Checked 'dotnet test' { dotnet test test/orcazap.tests/orcazap.tests.csproj -c Release --nologo }
    }

    Invoke-Checked 'sam build' { sam build --template-file cloudformation/orcazap.yaml --parallel --cached }

    $overrides = @("EnvAlias=$Env", "DbPassword=$dbPassword", "JwtSecret=$jwtSecret", "AdminCidr=$AdminCidr")
    Invoke-Checked "sam deploy ($stack)" {
        sam deploy --stack-name $stack --region $Region --resolve-s3 `
            --capabilities CAPABILITY_IAM CAPABILITY_AUTO_EXPAND `
            --no-confirm-changeset --no-fail-on-empty-changeset `
            --parameter-overrides $overrides
    }

    $outputs = @{}
    (aws cloudformation describe-stacks --stack-name $stack --region $Region --query 'Stacks[0].Outputs' --output json | ConvertFrom-Json) |
        ForEach-Object { $outputs[$_.OutputKey] = $_.OutputValue }

    $env:VITE_API_URL = $outputs.ApiUrl
    Push-Location frontend
    try {
        Invoke-Checked 'npm ci' { npm ci }
        Invoke-Checked 'npm run build' { npm run build }
    } finally { Pop-Location; Remove-Item Env:VITE_API_URL }

    Invoke-Checked 'upload do front' { aws s3 sync frontend/dist "s3://$($outputs.FrontendBucketName)" --delete --region $Region }
    Invoke-Checked 'invalidar cache do CloudFront' {
        aws cloudfront create-invalidation --distribution-id $outputs.FrontendDistributionId --paths '/*' | Out-Null
    }

    Write-Host ''
    Write-Host "Deploy $Env concluído." -ForegroundColor Green
    Write-Host "  Site : $($outputs.FrontendUrl)"
    Write-Host "  API  : $($outputs.ApiUrl)"
    Write-Host "  Banco: $($outputs.DatabaseEndpoint)  (rode mysql-server/schema.sql nele no primeiro deploy)"
} finally {
    Pop-Location
}
