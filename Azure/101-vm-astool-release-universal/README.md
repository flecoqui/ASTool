# Deployment of a VM (Linux or Windows) running .Net Core and ASTool 

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FASTool%2Fmaster%2FAzure%2F101-vm-astool-release-universal%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FASTool%2Fmaster%2FAzure%2F101-vm-astool-release-universal%2Fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>


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

## DEPLOY THE VM:
azure group deployment create "ResourceGroupName" "DeploymentName"  -f azuredeploy.json -e azuredeploy.parameters.json

For instance:

    azure group deployment create astoolgrpeu2 depastooltest -f azuredeploy.json -e azuredeploy.parameters.json -vv

Beyond login/password, the input parameters are :</p>
configurationSize (Small: F1 and 128 GB data disk, Medium: F2 and 256 GB data disk, Large: F4 and 512 GB data disk, XLarge: F4 and 1024 GB data disk) : 

    "configurationSize": {
      "type": "string",
      "defaultValue": "Small",
      "allowedValues": [
        "Small",
        "Medium",
        "Large",
        "XLarge"
      ],
      "metadata": {
        "description": "Configuration Size: VM Size + Disk Size"
      }
    }

configurationOS (debian, ubuntu, centos, redhat, nano server 2016, windows server 2016): 

    "configurationOS": {
      "type": "string",
      "defaultValue": "debian",
      "allowedValues": [
        "debian",
        "ubuntu",
        "centos",
        "redhat",
        "nanoserver2016",
        "windowsserver2016"
      ],
      "metadata": {
        "description": "The Operating System to be installed on the VM. Default value debian. Allowed values: debian,ubuntu,centos,redhat,nanoserver2016,windowsserver2016"
      }
    },



## TEST THE VM:
Once the VM has been deployed, you can open a remote session with the VM.

For instance for Linux VM:

     ssh VMAdmin@vmubus001.eastus2.cloudapp.azure.com

For Windows Server VM:

     mstsc /admin /v:vmwins001.eastus2.cloudapp.azure.com

For Nano Server VM:

     Set-Item WSMan:\\localhost\\Client\\TrustedHosts vmnanos001.eastus2.cloudapp.azure.com </p>
     Enter-PSSession -ComputerName vmnanos001.eastus2.cloudapp.azure.com </p>


Once connected, you can use ASTOOL to test the following features:</p>
### Push: Push a Smooth Streaming asset towards a Live ingestion point to emulate a Live TV channel

            ASTool --push     --input <inputLocalISMFile> --output <outputLiveUri> 
                             [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --loop <loopCounter>]
                             [--name <service name>]
                             [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]
                             [--consolelevel <none|error|warning|verbose>]


For instance:

     ASTool --push --input C:\projects\VideoApp\metisser\metisser.ism --output http://localhost/VideoApp/Live/_live1.isml --loop 0


### Pull: Pull a Smooth Streaming asset (VOD or Live) and store the video, audio and text chunks on the local disk

            ASTool --pull     --input <inputVODUri>       --output <outputLocalDirectory> 
			                 [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --maxduration <duration ms>]
                             [--audiotrackname <name>  --texttrackname <name>]
                             [--name <service name>]
                             [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]
                             [--consolelevel <none|error|warning|verbose>]

For instance:

     ASTool --pull --input http://localhost/VideoApp/metisser/metisser.ism/manifest --output C:\temp\astool\testvod
     ASTool --pull --input http://localhost/VideoApp/Live/_live2.isml/manifest  --maxbitrate 1000000 --maxduration 30000 --liveoffset 10 --output C:\temp\astool\testdvr



### PullPush: Pull a Smooth Streaming asset (Live only) and push the video, audio and text chunks towards a Live ingestion point to emulate a Live TV channel 

            ASTool --pullpush --input <inputLiveUri>      --output <outputLiveUri>  
                             [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --maxduration <duration ms>]
                             [--audiotrackname <name>  --texttrackname <name>]
                             [--liveoffset <value in seconds>]
                             [--name <service name>]
                             [--tracefile <path> --tracesize <size in bytes> --tracelevel <none|error|warning|debug>]
                             [--consolelevel <none|error|warning|verbose>]
For instance:

     ASTool --input http://localhost/VideoApp/Live/_live2.isml/manifest  --maxbitrate 1000000   --output http://localhost/VideoApp/Live/_live1.isml


### Parse: Parse an isma or ismv file on disk 

            ASTool --parse    --input <inputLocalISMV|inputLocalISMA>  

For instance:

     ASTool --input  C:\temp\ASTool\test4\metisser\Audio_0.isma



</p>

## DELETE THE RESOURCE GROUP:
azure group delete "ResourceGroupName" "DataCenterName"

For instance:

    azure group delete astoolgrpeu2 eastus2
