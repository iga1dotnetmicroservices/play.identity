# Play.Identity

Play Economy Identity microservice

## Create and publish package

MacOS

```shell
version='1.0.6'
owner='iga1dotnetmicroservices'
gh_pat='[PAT HERE]'

dotnet pack src/Play.Identity.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/play.identity.git -o ../packages

dotnet nuget push ../packages/Play.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

Windows

```powershell
$version='1.0.6'
$owner='iga1dotnetmicroservices'
$gh_pat='[PAT HERE]'

dotnet pack src/Play.Identity.Contracts --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/play.identity.git -o ../packages

dotnet nuget push ../packages/Play.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image

MacOS

```shell
appname='iga1playeconomy'
export GH_OWNER='iga1dotnetmicroservices'
export GH_PAT='[PAT HERE]'
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/play.identity:$version" .
```

Windows

```powershell
$appname='iga1playeconomy'
$env:GH_OWNER='iga1dotnetmicroservices'
$env:GH_PAT='[PAT HERE]'
docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/play.identity:$version" .
```

## Run the docker image

MacOS

```shell
adminPass='[PASSWORD HERE]'
cosmosDbConnString='[CONN STRING HERE]'
docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e IdentitySettings__AdminUserPassword=$adminPass --network playinfra_default play.identity:$version
```

Windows

```powershell
$adminPass='[PASSWORD HERE]'
$cosmosDbConnString='[CONN STRING HERE]'
$serviceBusConnString='[CONN STRING HERE]'

docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" -e IdentitySettings__AdminUserPassword=$adminPass play.identity:$version
```

## Publishing the Docker image


MacOS

```shell
appname='iga1playeconomy'
az acr login --name $appname
docker push "$appname.azurecr.io/play.identity:$version"
```

Windows

```powershell
$appname='iga1playeconomy'
az acr login --name $appname
docker push "$appname.azurecr.io/play.identity:$version"
```

## Create the Kubernetes namespace

MacOS

```shell
namespace='identity'
kubectl create namespace $namespace
```

Windows

```powershell
$namespace='identity'
kubectl create namespace $namespace
```

## Create the Kubernetes secrets

MacOS

```shell
kubectl create secret generic identity-secrets --from-literal=cosmosdb-connectionstring=$cosmosDbConnString --from-literal=servicebus-connectionstring=$serviceBusConnString --from-literal=admin-password=$adminPass -n $namespace
```

Windows

```powershell
kubectl create secret generic identity-secrets --from-literal=cosmosdb-connectionstring=$cosmosDbConnString --from-literal=servicebus-connectionstring=$serviceBusConnString --from-literal=admin-password=$adminPass -n $namespace
```

## Create the Kubernetes pod

MacOS

```shell
kubectl apply -f ./kubernetes/identity.yaml -n $namespace
```

Windows

```powershell
kubectl apply -f ./kubernetes/identity.yaml -n $namespace
```

## Creating the pod managed identity


MacOS

```shell
az identity create --resource-group $appname --name $namespace

IDENTITY_RESOURCE_ID=$(az identity show -g $appname -n $namespace --query id -otsv)

az aks pod-identity add --resource-group $appname --cluster-name $appname --namespace $namespace --name $namespace --identity-resource-id $IDENTITY_RESOURCE_ID

```

Windows

```powershell
az identity create --resource-group $appname --name $namespace

$IDENTITY_RESOURCE_ID=az identity show -g $appname -n $namespace --query id -otsv

az aks pod-identity add --resource-group $appname --cluster-name $appname --namespace $namespace --name $namespace --identity-resource-id $IDENTITY_RESOURCE_ID

```

## Granting access to Key Vault secrets

MacOS

```shell
IDENTITY_CLIENT_ID=$(az identity show -g $appname -n $namespace --query clientId -otsv)
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```

Windows

```powershell
$IDENTITY_CLIENT_ID=az identity show -g $appname -n $namespace --query clientId -otsv
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```

