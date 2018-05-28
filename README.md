<img src="Docs/ASTOOL_logo.png">


# What is ASTOOL?

<img src="Docs/ASTOOL_architecture.png" width="600">

# Features area

The Adaptive Streaming Tool (ASTool) is an Open Source tool supporting several features:
####  Push: 
push VOD asset towards Live ingestion point to emulate a Live Channel based on VOD Asset
####  Pull: 
Create VOD asset from an existing VOD asset already online.
####  PullPush: 
Route an existing Live Stream towards Azure Media Service Live ingestion point.
####  Parse: 
Fmp4 parser


This template allows you to deploy a simple VM running: </p>
#### Debian: .Net Core and ASTOOL,
#### Ubuntu: .Net Core and ASTOOL, 
#### Centos: .Net Core and ASTOOL, 
#### Red Hat: .Net Core and ASTOOL,
#### Windows Server 2016: .Net Core and ASTOOL,
This will VM will be deployed in the region associated with Resource Group and the VM Size is one of the parameter.
With Azure CLI you can deploy this VM with 2 command lines:

![](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-release-universal/Docs/1-architecture.png)

## CREATE RESOURCE GROUP:
azure group create "ResourceGroupName" "DataCenterName"

For instance:

    azure group create astoolgrpeu2 eastus2

# Getting Started


https://docs.microsoft.com/en-us/dotnet/core/windows-prerequisites?tabs=netcore2x


https://docs.microsoft.com/en-us/dotnet/core/macos-prerequisites?tabs=netcore2x 


https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites?tabs=netcore2x



