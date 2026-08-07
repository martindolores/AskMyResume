// Provisions the Milestone 1 hosting stack for AskMyResume: Log Analytics,
// a Container Apps Environment, a Container App, and a Key Vault holding the
// Gemini API key. Every resource here is chosen to stay inside always-free
// tiers — see the cost guardrails table in README.md before changing SKUs,
// replica counts, or adding a workload profile.

@description('Region for all resources.')
param location string = resourceGroup().location

@description('Short name used as the base for resource names.')
param appName string = 'askmyresume'

@description('Container image to deploy, e.g. ghcr.io/<owner>/<repo>:<tag>. Assumed to be a public GHCR package — no registry credentials are configured. If the package is private, add a `registries` block with a pull secret before deploying.')
param containerImage string = 'ghcr.io/martindolores/askmyresume:latest'

@description('Azure AD object ID (not a secret) of the principal that should be able to set/rotate the Gemini API key after deployment, via `az keyvault secret set`. Leave empty to skip granting this and assign the "Key Vault Secrets Officer" role manually instead.')
#disable-next-line secure-secrets-in-params
param keyVaultSecretsOfficerPrincipalId string = ''

var uniqueSuffix = uniqueString(resourceGroup().id)
var keyVaultName = '${take(appName, 11)}-kv-${take(uniqueSuffix, 6)}'
var geminiApiKeySecretName = 'gemini-api-key'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${appName}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// System-assigned identities on Container Apps can't have their Key Vault
// role assignment created until the container app itself exists, which
// creates an ordering problem: the container app's secrets block needs to
// reference an identity that already has read access, but that identity's
// principalId doesn't exist until the container app is deployed. A
// user-assigned identity sidesteps this — it exists (and can be granted the
// role) before the container app that will use it is ever created.
resource containerAppIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${appName}-identity'
  location: location
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
  }
}

var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource containerAppKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerAppIdentity.id, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: containerAppIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var keyVaultSecretsOfficerRoleId = 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7'

resource deployerKeyVaultAccess 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(keyVaultSecretsOfficerPrincipalId)) {
  name: guid(keyVault.id, keyVaultSecretsOfficerPrincipalId, keyVaultSecretsOfficerRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsOfficerRoleId)
    principalId: keyVaultSecretsOfficerPrincipalId
    principalType: 'User'
  }
}

// No `workloadProfiles` here — that's what keeps this a Consumption-only
// environment. Adding a Dedicated workload profile bills per node-hour
// regardless of usage; see the cost guardrails table in README.md.
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: '${appName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${containerAppIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      secrets: [
        {
          name: geminiApiKeySecretName
          keyVaultUrl: '${keyVault.properties.vaultUri}secrets/${geminiApiKeySecretName}'
          identity: containerAppIdentity.id
        }
      ]
    }
    template: {
      containers: [
        {
          name: appName
          image: containerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'GEMINI_API_KEY'
              secretRef: geminiApiKeySecretName
            }
          ]
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
  dependsOn: [
    containerAppKeyVaultAccess
  ]
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
output keyVaultName string = keyVault.name
output geminiApiKeySecretName string = geminiApiKeySecretName
output logAnalyticsWorkspaceName string = logAnalytics.name
