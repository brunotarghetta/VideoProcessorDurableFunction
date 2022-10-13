# VideoProcessorDurableFunction

## Pluralsight

- https://app.pluralsight.com/library/courses/azure-durable-functions-fundamentals/table-of-contents

## Micrsoft

- https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp
- https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-create-first-csharp?pivots=code-editor-vscode


# Publish from Visual Studio

- https://learn.microsoft.com/en-us/training/modules/develop-test-deploy-azure-functions-with-visual-studio/4-publish-azure-functions

# Publish from CLI using ARM Templates

Prerequisites:

1. Install [azure-functions-core-tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools) 
2. Run cmd from root project 

**Login to azure** 
`az login`

**Set subscription if needed**
`az account set -s 49226763-8d58-4c18-9136-78c42344575a`

**Create a resource group**
`az group create -l eastus -n VideoProcessor-dev-rg2`

**Create a deployment with resources**
`az deployment group create --name videoProcessorDeployment  --template-file template.json --resource-group VideoProcessor-dev-rg2 --parameters @parameters.json`

**Get new azure web jobs storage connection string and replace in AzureWebJobsStorage WEBSITE_CONTENTAZUREFILECONNECTIONSTRING**
`az storage account show-connection-string --name videopro20221006devst --resource-group VideoProcessor-dev-rg2 --subscription 49226763-8d58-4c18-9136-78c42344575a`


**Add settings** 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "AzureWebJobsStorage=DefaultEndpointsProtocol=https;AccountName=videopro20221006devst;AccountKey=kZs+yUVM86k+VUSHheXy0a+mNNdHosyLgtrhyQ6Onup/xD8A4sd/fbK/p+dKzZmo5aQR8f3v2K3d+AStCD0buQ==;EndpointSuffix=core.windows.net"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "ApprovalEmail=bruno_targhetta@hotmail.com"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "FUNCTIONS_EXTENSION_VERSION=~4"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "FUNCTIONS_WORKER_RUNTIME=dotnet"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "Host=https://videopro20221006devsi.azurewebsites.net"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "SenderEmail=brunotarghetta@gmail.com"`
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "SendGridKey={Sendgrid key}"` 	
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "TranscodeBitRates=1010,2020,3030"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING=DefaultEndpointsProtocol=https;AccountName=videopro20221006devst;AccountKey=kZs+yUVM86k+VUSHheXy0a+mNNdHosyLgtrhyQ6Onup/xD8A4sd/fbK/p+dKzZmo5aQR8f3v2K3d+AStCD0buQ==;EndpointSuffix=core.windows.net"` 
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "WEBSITE_CONTENTSHARE=videoprocessor20221005180315"`
`az functionapp config appsettings set -n videopro20221006devsi -g VideoProcessor-dev-rg2 --settings "WEBSITE_RUN_FROM_PACKAGE=0"`

**Deploy function**
`func azure functionapp publish videopro20221006devsi --force`

**Test**
https://videopro20221006devsi.azurewebsites.net/api/processvideostarter?video=example.mpg

**Delete Resource group**
`az group delete -n VideoProcessor-dev-rg2`


# Publish from CLI using scripts

Prerequisites:

1. Install [azure-functions-core-tools](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cwindows%2Ccsharp%2Cportal%2Cbash#install-the-azure-functions-core-tools) 
2. Run powershell from root project 

`.\deploy-videoprocessor.ps1`