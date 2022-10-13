# This script creates all the infrastrcutre using Azure CLI commands
# Prerequisites:
# - Azure CLI https://docs.microsoft.com/en-us/cli/azure/install-azure-cli
# - Azure Functions Core Tools https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=windows%2Ccsharp%2Cbash#install-the-azure-functions-core-tools
# More information https://markheath.net/post/deploying-azure-functions-with-azure-cli

# step 1 - log in 
az login

# step 2 - ensure you are using the correct subscription
az account set -s "49226763-8d58-4c18-9136-78c42344575a"

# step 3 - pick unique names
$RESOURCE_GROUP = "VideoProcessor-dev-rg3"
$FUNCTION_APP_NAME = "videopro221012funcname"
$STORAGE_ACCOUNT_NAME = "videopro221012stoaname"
$APP_INSIGHTS_NAME = "videopro221012appinsname"
$LOCATION = "eastus"

# step 4 - create the resource group
az group create -n $RESOURCE_GROUP -l $LOCATION

# step 5 - create the storage account
az storage account create -n $STORAGE_ACCOUNT_NAME -l $LOCATION -g $RESOURCE_GROUP --sku Standard_LRS

# step 6 - create an Application Insights Instance
az resource create `
  -g $RESOURCE_GROUP -n $APP_INSIGHTS_NAME `
  --resource-type "Microsoft.Insights/components" `
  --properties '{\"Application_Type\":\"web\"}'


# step 7 - create the function app, connected to the storage account and app insights
az functionapp create `
  -n $FUNCTION_APP_NAME `
  --storage-account $STORAGE_ACCOUNT_NAME `
  --consumption-plan-location $LOCATION `
  --app-insights $APP_INSIGHTS_NAME `
  --runtime dotnet `
  --functions-version 4 `
  --os-type Windows `
  -g $RESOURCE_GROUP

$key=$(az storage account keys list --account-name $STORAGE_ACCOUNT_NAME --resource-group $RESOURCE_GROUP --query [0].value -o tsv)

# step 8 - (optional - publish any settings)
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "AzureWebJobsStorage=DefaultEndpointsProtocol=https;AccountName=$STORAGE_ACCOUNT_NAME;AccountKey=$key;EndpointSuffix=core.windows.net" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "ApprovalEmail=bruno_targhetta@hotmail.com" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "FUNCTIONS_EXTENSION_VERSION=~4" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "FUNCTIONS_WORKER_RUNTIME=dotnet" "Host=https://$FUNCTION_APP_NAME.azurewebsites.net" 	
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "Host=https://$FUNCTION_APP_NAME.azurewebsites.net" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "SenderEmail=brunotarghetta@gmail.com" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "SendGridKey={SendGridKey}" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "TranscodeBitRates=1010,2020,3030" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING=DefaultEndpointsProtocol=https;AccountName=$STORAGE_ACCOUNT_NAME;AccountKey=$key;EndpointSuffix=core.windows.net" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "WEBSITE_CONTENTSHARE=$FUNCTION_APP_NAME" 
az functionapp config appsettings set -n $FUNCTION_APP_NAME -g $RESOURCE_GROUP ` --settings "WEBSITE_RUN_FROM_PACKAGE=0" 	


# step 9 - publish the applciation code
func azure functionapp publish $FUNCTION_APP_NAME --force

# CLEANUP ##SG.		XY2t8IEDQf66heqxZ3IJVw.		wV1VWCpS9kfPH4diM1WPJdQ3HznvsP_58GqqZ1j7PLw##