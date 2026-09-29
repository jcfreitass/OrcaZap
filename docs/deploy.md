# Deploy na AWS

Um stack CloudFormation (SAM) por ambiente — `orcazap-hml` e `orcazap-prd` — definido em
`cloudformation/orcazap.yaml`. Front e back ficam no mesmo repo, mas são publicados separados:

| Parte | Vai para | Como |
|---|---|---|
| 20 Lambdas (`src/`) | AWS Lambda (.NET 8) + API Gateway HTTP | `sam build` + `sam deploy` |
| Banco | RDS MySQL 8.4 (`db.t4g.micro`) em VPC própria | criado pelo stack; schema via `mysql-server/schema.sql` |
| React (`frontend/`) | S3 privado + CloudFront (HTTPS) | `npm run build` com `VITE_API_URL` → `aws s3 sync` |

## Pré-requisitos (uma vez)

```powershell
winget install Amazon.AWSCLI
winget install Amazon.SAM-CLI
aws configure   # access key de um usuário IAM com permissão de CloudFormation/Lambda/API GW/RDS/EC2/S3/CloudFront/IAM
```

## Deploy

```powershell
./deploy/deploy.ps1 -Env hml
```

O script pede a senha do RDS e o JWT secret (ou lê `ORCAZAP_DB_PASSWORD` / `ORCAZAP_JWT_SECRET`).
Use sempre os mesmos valores para o mesmo ambiente. Gere o JWT secret com:

```powershell
-join ((1..32) | % { '{0:x2}' -f (Get-Random -Max 256) })
```

No fim ele imprime a URL do site, da API e o endpoint do banco. O primeiro deploy leva ~15 min (RDS e CloudFront).

## Primeiro deploy: criar as tabelas

O RDS nasce com o database `orcazap` vazio. Para acessar pelo DBeaver, faça o deploy passando seu IP:

```powershell
./deploy/deploy.ps1 -Env hml -AdminCidr "$((irm https://checkip.amazonaws.com).Trim())/32"
```

Conecte no DBeaver (host = `DatabaseEndpoint`, porta 3306, usuário `orcazap_admin`, senha do deploy)
e rode `mysql-server/schema.sql`. Depois, se quiser fechar o acesso, rode o deploy de novo sem `-AdminCidr`.

## Como a configuração chega nas Lambdas

`ConfigurationManager` lê `appsettings.json` → `appsettings.{env}.json` → variáveis de ambiente (a última vence).
O stack injeta em todas as Lambdas `connection_orcazap_mysql_read/write`, `jwt_secret` e `ASPNETCORE_ENVIRONMENT`,
e o API Gateway passa a stage variable `stageAlias` que o `FunctionBase.SetEnvironment` usa.
Nenhum segredo fica no repositório.

## Custos aproximados (us-east-1)

- RDS `db.t4g.micro` + 20 GB: ~US$ 15/mês (grátis no free tier nos primeiros 12 meses).
- Lambda, API Gateway, S3, CloudFront: centavos com pouco tráfego.
- Sem NAT Gateway: as Lambdas não acessam a internet, só o RDS.

## Remover um ambiente

```powershell
sam delete --stack-name orcazap-hml
```

O RDS gera um snapshot final antes de ser apagado (`DeletionPolicy: Snapshot`). Em `prd` o RDS tem
proteção contra exclusão — desligue no console antes.

## Jenkins

`pipeline/orcazap-{hml,prd}-api.groovy` fazem os mesmos passos. Cadastre as credenciais
(Secret text) `orcazap-<env>-db-password` e `orcazap-<env>-jwt-secret`.
