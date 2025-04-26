@description('The location for the resource(s) to be deployed.')
param location string = resourceGroup().location

resource azureaisearch 'Microsoft.Search/searchServices@2023-11-01' = {
  name: take('azureaisearch-${uniqueString(resourceGroup().id)}', 60)
  location: location
  properties: {
    hostingMode: 'default'
    disableLocalAuth: true
    partitionCount: 1
    replicaCount: 1
  }
  sku: {
    name: 'basic'
  }
  tags: {
    'aspire-resource-name': 'azureaisearch'
  }
}

output connectionString string = 'Endpoint=https://${azureaisearch.name}.search.windows.net'

output name string = azureaisearch.name