# Deployment of a VM (Linux or Windows) running ASTool 

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FASTool%2Fmaster%2FAzure%2F101-vm-astool-release-universal%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>
<a href="http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fflecoqui%2FASTool%2Fmaster%2FAzure%2F101-vm-astool-release-universal%2Fazuredeploy.json" target="_blank">
    <img src="http://armviz.io/visualizebutton.png"/>
</a>

# DEPLOYING THE VIRTUAL MACHINE FROM GITHUB

This template allows you to deploy a single virtual machine running ASTOOL. This virtual machine can run the following operating systems:
- Debian
- Ubuntu 
- Centos 
- Red Hat
- Windows Server 2016

![](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-release-universal/Docs/1-architecture.png)

This virtual machine will be deployed in the region associated with Resource Group. The size of the virtual machine is also a parameter of the Azure Resource Manager template.

During the installation of the virtual machine, ASTool will be installed. 
Once ASTool is installed, the installation program will:

- create a service to run automatically ASTool. By default this service will launch the Pull feature to capture the audio and video chunks of this sample Live asset: http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest during 3600 seconds.

The configuration files ([astool.linux.xml](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-universal/astool.linux.xml) for Linux and [astool.windows.xml](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-universal/astool.windows.xml) for Windows) will be stored under: /astool/config

This service will run simulatenously 2 captures, storing the audio and video chunks under /astool/dvr/test1 and /astool/dvr/test2.
The logs files will be available under /astool/log.

# DEPLOYING THE VIRTUAL MACHINE WITH AZURE CLI

With Azure CLI you can deploy this VM with 2 command lines:

## CREATE RESOURCE GROUP:
azure group create "ResourceGroupName" "DataCenterName"

For instance:

    azure group create astoolgrpeu2 eastus2

## DEPLOY THE VIRTUAL MACHINE:
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
        "windowsserver2016"
      ],
      "metadata": {
        "description": "The Operating System to be installed on the VM. Default value debian. Allowed values: debian,ubuntu,centos,redhat,nanoserver2016,windowsserver2016"
      }
    },





## DELETE THE RESOURCE GROUP:
azure group delete "ResourceGroupName" "DataCenterName"

For instance:

    azure group delete astoolgrpeu2 eastus2

# TESTING THE VIRTUAL MACHINE:
Once the VM has been deployed, you can open a remote session with the VM.

For instance for Linux VM:

     ssh VMAdmin@vmubus001.eastus2.cloudapp.azure.com

For Windows Server VM:

     mstsc /admin /v:vmwins001.eastus2.cloudapp.azure.com


Once connected, you can check whether the ASTool service is running.
If the 2 folders are not empty:

- /astool/dvr/test1
- /astool/dvr/test2

it means the ASTool service is running and started to store audio and video chunks.

Otherwise, you can use ASTool from the command shell. The Adaptive Streaming Tool (ASTool) is an Open Source command line tool supporting several features described below:

##  Push feature: 
This feature pushes a Smooth Streaming VOD asset towards Live ingestion point to emulate a Live Channel based on VOD Asset. The Live ingestion point can be either an IIS Media Services or an Azure Media Services ingestion point.

### Syntax

    ASTool --push     --input <inputLocalISMFile> --output <outputLiveUri>
            [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s> --loop <loopCounter>]
            [--name <service name> --counterperiod <periodinseconds>]
            [--tracefile <path> --tracesize <size in bytes> ]
            [--tracelevel <none|information|error|warning|verbose>]
            [--consolelevel <none|information|error|warning|verbose>]

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--input| string | null | Path to the local ISM file on the disk (mandatory option)|
|--ouput| string | null | Uri of the ingestion point (mandatory option)|
|--loop| int |0  | number of live loop when the value is 0, infinite loop|
|--minbitrate| int |0  | minimum bitrate of the video tracks to select|
|--maxbitrate| int |0  | maximum bitrate of the video tracks to select. When the value is 0, all the video tracks with a bitrate over minbitrate value are selected |
|--name| string | null  | name of the service, only used for the logs |
|--counterperiod &nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;| int |0  | period in seconds used to display the counters|
|--tracefile| string | null  | path of the file where the trace will be stored |
|--tracesize| int |0  | maximum size of the trace file|
|--tracelevel| string | information  | trace level: none (no log in the trace file), information, error, warning, verbose |
|--consolelevel| string | information  | console level: none (no log in the console), information, error, warning, verbose |


### Examples

Push a smooth streaming asset (C:\projects\VideoApp\metisser\metisser.ism) towards a local IIS Media Services ingestion point (http://localhost/VideoApp/Live/_live1.isml):

    ASTool.exe --push --input C:\projects\VideoApp\metisser\metisser.ism --output http://localhost/VideoApp/Live/_live1.isml --loop 0

The live stream can be played opening the url: http://localhost/VideoApp/Live/_live1.isml/manifest


Same exemple with Azure Media Services:

    ASTool.exe --push --input C:\projects\VideoApp\metisser\metisser.ism --output http://testsmoothlive-testamsmedia.channel.mediaservices.windows.net/ingest.isml --loop 0

The live stream can be played opening the url: http://testsmoothlive-testamsmedia.channel.mediaservices.windows.net/preview.isml/manifest



##  Pull feature: 
Create VOD asset from an existing Smooth Streaming VOD asset or a Live Smooth Streaming channel already online.


### Syntax

    ASTool --pull     --input <inputVODUri> --output <outputLocalDirectory>
            [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s>]
            [--maxduration <duration ms>]
            [--audiotrackname <name>  --texttrackname <name>]
            [--liveoffset <value in seconds>]
            [--name <service name> --counterperiod <periodinseconds>]
            [--tracefile <path> --tracesize <size in bytes> ]
            [--tracelevel <none|information|error|warning|verbose>]
            [--consolelevel <none|information|error|warning|verbose>]

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--input| string | null | Uri of the VOD stream or Live stream|
|--ouput| string | null | Path of the folder where the audio and video chunks will be stored|
|--minbitrate| int |0  | minimum bitrate of the video tracks to select|
|--maxbitrate| int |0  | maximum bitrate of the video tracks to select. When the value is 0, all the video tracks with a bitrate over minbitrate value are selected |
|--maxduration| int |0  | maximum duration of the capture in milliseconds |
|--audiotrackname&nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp;&nbsp;| string |null  | name of the audio track to capture, if this value is not set all the audio tracks are captured|
|--texttrackname| string |null  | name of the text track to capture, if this value is not set all the text tracks are captured|
|--liveoffset| int | 0  | the offset in seconds with the live position. If this value is not set, ASTool will start to capture the audio and video chunk at the beginning of the Live buffer defined in the smooth Streaming manifest|
|--name| string | null  | name of the service, used for the traces |
|--counterperiod| int |0  | period in seconds used to display the counters|
|--tracefile| string | null  | path of the file where the trace will be stored |
|--tracesize| int |0  | maximum size of the trace file|
|--tracelevel| string | information  | trace level: none (no log in the trace file), information, error, warning, verbose |
|--consolelevel| string | information  | console level: none (no log in the console), information, error, warning, verbose |

### Examples

Pull a smooth streaming VOD asset (http://localhost/VideoApp/metisser/metisser.ism/manifest) towards a folder on a local disk (C:\astool\testvod):

    ASTool.exe --pull --input http://localhost/VideoApp/metisser/metisser.ism/manifest --output C:\astool\testvod
    
The isma and ismv files are available under C:\astool\testvod\metisser folder and can be played with tool like VLC.


Same exemple with Live Stream:

Pull a smooth streaming Live stream  (http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest) towards a folder on a local disk (C:\astool\testvod) during 60 seconds:

    ASTool.exe --pull --input http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest  --output C:\astool\testdvr --maxduration 60000 


The isma and ismv files are available under C:\astool\testdvr\a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec folder and can be played with tool like VLC.



##  PullPush feature: 
Route an existing Live Stream towards an Azure Media Service Live ingestion point or an IIS Media Service ingestion point.

### Syntax

    ASTool --pullpush     --input <inputLiveUri> --output <outputLiveUri>
            [--minbitrate <bitrate b/s>  --maxbitrate <bitrate b/s>]
            [--maxduration <duration ms>]
            [--audiotrackname <name>  --texttrackname <name>]
            [--liveoffset <value in seconds>]
            [--name <service name> --counterperiod <periodinseconds>]
            [--tracefile <path> --tracesize <size in bytes> ]
            [--tracelevel <none|information|error|warning|verbose>]
            [--consolelevel <none|information|error|warning|verbose>]

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--input| string | null | Uri of the Live stream |
|--ouput| string | null | Uri of the output Live stream ingestion point |
|--minbitrate| int |0  | minimum bitrate of the video tracks to select|
|--maxbitrate| int |0  | maximum bitrate of the video tracks to select. When the value is 0, all the video tracks with a bitrate over minbitrate value are selected |
|--maxduration| int |0  | maximum duration of the capture in milliseconds |
|--audiotrackname&nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;| string |null  | name of the audio track to capture, if this value is not set all the audio tracks are captured|
|--texttrackname| string |null  | name of the text track to capture, if this value is not set all the text tracks are captured|
|--liveoffset| int | 0  | the offset in seconds with the live position. If this value is not set, ASTool will start to capture the audio and video chunk at the beginning of the Live buffer defined in the smooth Streaming manifest|
|--name| string | null  | name of the service, used for the traces |
|--counterperiod| int |0  | period in seconds used to display the counters|
|--tracefile| string | null  | path of the file where the trace will be stored |
|--tracesize| int |0  | maximum size of the trace file|
|--tracelevel| string | information  | trace level: none (no log in the trace file), information, error, warning, verbose |
|--consolelevel| string | information  | console level: none (no log in the console), information, error, warning, verbose |

### Examples

Pull and Push a Live smooth streaming asset from  (http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest) towards a local IIS Media Services Ingestion point (http://localhost/VideoApp/Live/_live1.isml), only the video tracks with a bitrate between 300kbps and 1Mpbs are routed:

    ASTool.exe --pullpush --input http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest --minbitrate 300000   --maxbitrate 1000000  --output http://localhost/VideoApp/Live/_live1.isml
    
The live stream can be played opening the url: http://localhost/VideoApp/Live/_live1.isml/manifest 


Same exemple with Azure Media Services:

    ASTool.exe --pullpush --input http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest --minbitrate 300000   --maxbitrate 1000000  --output http://testsmoothlive-testamsmedia.channel.mediaservices.windows.net/ingest.isml  

The live stream can be played opening the url: http://testsmoothlive-testamsmedia.channel.mediaservices.windows.net/preview.isml/manifest


## Running several features simultaneously: 
With ASTool it's possible with a single command line to instantiate several features simultaneously. In that case, the features are defined in an XML config file.
For instance a [Windows Configuration File](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-release-universal/astool.windows.xml) a  [Linux Configuration File](https://raw.githubusercontent.com/flecoqui/ASTool/master/Azure/101-vm-astool-release-universal/astool.linux.xml)

This XML file contains an ArrayOfOptions, each Options is defined with the following attributes:

| Attribute name | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|ASToolAction| string | null | Name of the feature to activate: Pull, Push PullPush |
|InputUri| string | null | Input Uri used by the feature |
|OutputUri| string | null | Output Uri used by the feature |
|LiveOffset| int | null |The offset in seconds with the live position. If this value is not set, ASTool will start to capture the audio and video chunk at the beginning of the Live buffer defined in the smooth Streaming manifest. Used by Pull and PullPush feature |
|Loop| int | 0 |Number of live loop when the value is 0, infinite loop. Used by Push feature|
|MinBitrate| int |0  | Minimum bitrate of the video tracks to select|
|MaxBitrate| int |0  | Maximum bitrate of the video tracks to select. When the value is 0, all the video tracks with a bitrate over minbitrate value are selected |
|MaxDuration| int |0  | Maximum duration of the capture in milliseconds |
|AudioTrackName&nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp; &nbsp; &nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;| string |null  | Name of the audio track to capture, if this value is not set all the audio tracks are captured|
|TextTrackName| string |null  | Name of the text track to capture, if this value is not set all the text tracks are captured|
|BufferSize| int | 1000000 | Maximum size of the buffer containing the audio and video chunks in memory   |
|ConfigFile| string | null | Not used currently |
|Name| string | null  | Name of the service, used for the traces |
|CounterPeriod| int |0  | Period in seconds used to display the counters|
|TraceFile| string | null  | Path of the file where the trace will be stored |
|TraceSize| int |0  | Maximum size of the trace file|
|TraceLevel| string | information  | Trace level: None (no log in the trace file), Information, Error, Warning, Verbose |
|ConsoleLevel| string | information  | Console level: None (no log in the console), Information, Error, Warning, Verbose |


Below the content of such file:

    <ArrayOfOptions xmlns="http://schemas.datacontract.org/2004/07/ASTool" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
    <Options>
        <ASToolAction>Pull</ASToolAction>
        <AudioTrackName/>
        <BufferSize>1000000</BufferSize>
        <ConfigFile/>
        <ConsoleLevel>Information</ConsoleLevel>
        <CounterPeriod>300</CounterPeriod>
        <InputUri>http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest</InputUri>
        <LiveOffset>10</LiveOffset>
        <Loop>0</Loop>
        <MaxBitrate>10000000</MaxBitrate>
        <MaxDuration>3600000</MaxDuration>
        <MinBitrate>100000</MinBitrate>
        <Name>PullService1</Name>
        <OutputUri>/astool/dvr/test1</OutputUri>
        <TextTrackName/>
        <TraceFile>/astool/log/TracePullService1.log</TraceFile>
        <TraceLevel>Information</TraceLevel>
        <TraceSize>524280</TraceSize>
    </Options>
    <Options>
        <ASToolAction>Pull</ASToolAction>
        <AudioTrackName/>
        <BufferSize>1000000</BufferSize>
        <ConfigFile/>
        <ConsoleLevel>Information</ConsoleLevel>
        <CounterPeriod>300</CounterPeriod>
        <InputUri>http://b028.wpc.azureedge.net/80B028/Samples/a38e6323-95e9-4f1f-9b38-75eba91704e4/5f2ce531-d508-49fb-8152-647eba422aec.ism/manifest</InputUri>
        <LiveOffset>0</LiveOffset>
        <Loop>0</Loop>
        <MaxBitrate>10000000</MaxBitrate>
        <MaxDuration>3600000</MaxDuration>
        <MinBitrate>100000</MinBitrate>
        <Name>PullService2</Name>
        <OutputUri>/astool/dvr/test2</OutputUri>
        <TextTrackName/>
        <TraceFile>/astool/log/TracePullService2.log</TraceFile>
        <TraceLevel>Information</TraceLevel>
        <TraceSize>524280</TraceSize>
    </Options>
    </ArrayOfOptions>


### Syntax

Launching ASTool to run several features defined in the XML configuration file.

    ASTool --import --configfile  <configFile>

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--configfile&nbsp;&nbsp; &nbsp; &nbsp; &nbsp;| string | null | Path to the XML config File which contains the information about the features to instantiate|

Exporting an XML configuration file which could be updated afterwards.

    ASTool --export --configfile  <configFile>

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--configfile&nbsp;&nbsp; &nbsp; &nbsp; &nbsp;| string | null | Path to the XML config File which will be created containing sample push, pull, pullpush feature|



### Examples

Launching the features defined in the configfile:

    ASTool.exe --import --configfile C:\astool\config\astool.windows.xml


##  Parse feature: 
Parsing isma and ismv files

### Syntax

    ASTool --parse    --input <inputLocalISMXFile> 
            [--tracefile <path> --tracesize <size in bytes> ]
            [--tracelevel <none|information|error|warning|verbose>]
            [--consolelevel <none|information|error|warning|verbose>]

| option | value type | default value | Description | 
| :--- | :--- | :--- |  :--- | 
|--input| string | null | Path to the local ISMV or ISMA file on the disk|
|--tracefile| string | null  | path of the file where the trace will be stored |
|--tracesize| int |0  | maximum size of the trace file|
|--tracelevel| string | information  | trace level: none (no log in the trace file), information, error, warning, verbose |
|--consolelevel&nbsp;  &nbsp; &nbsp;&nbsp; | string | information  | console level: none (no log in the console), information, error, warning, verbose |


### Examples

Parsing an ISMA file and displaying the MP4 boxes hierarchy:

    ASTool.exe --parse --input C:\ASTool\testdvr\5f2ce531-d508-49fb-8152-647eba422aec\Audio_0.isma

Parsing an ISMV file, displaying the MP4 boxes hierarchy and the content of each box in hexadecimal:

    ASTool.exe --parse --input C:\ASTool\testdvr\5f2ce531-d508-49fb-8152-647eba422aec\Video_0.ismv --consolelevel verbose

