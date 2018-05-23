
#usage install-software-windows.ps1 dnsname

param
(
      [string]$dnsName = $null,
	  [string]$astoolConfigFile = $null
)


#Create Source folder
$source = 'C:\source' 
If (!(Test-Path -Path $source -PathType Container)) {New-Item -Path $source -ItemType Directory | Out-Null} 
function WriteLog($msg)
{
Write-Host $msg
$msg >> c:\source\install.log
}
function WriteDateLog
{
date >> c:\source\install.log
}
if(!$dnsName) {
 WriteLog "DNSName not specified" 
 throw "DNSName not specified"
}
function DownloadAndUnzip($sourceUrl,$DestinationDir ) 
{
    $TempPath = [System.IO.Path]::GetTempFileName()
    if (($sourceUrl -as [System.URI]).AbsoluteURI -ne $null)
    {
        $handler = New-Object System.Net.Http.HttpClientHandler
        $client = New-Object System.Net.Http.HttpClient($handler)
        $client.Timeout = New-Object System.TimeSpan(0, 30, 0)
        $cancelTokenSource = [System.Threading.CancellationTokenSource]::new()
        $responseMsg = $client.GetAsync([System.Uri]::new($sourceUrl), $cancelTokenSource.Token)
        $responseMsg.Wait()
        if (!$responseMsg.IsCanceled)
        {
            $response = $responseMsg.Result
            if ($response.IsSuccessStatusCode)
            {
                $downloadedFileStream = [System.IO.FileStream]::new($TempPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
                $copyStreamOp = $response.Content.CopyToAsync($downloadedFileStream)
                $copyStreamOp.Wait()
                $downloadedFileStream.Close()
                if ($copyStreamOp.Exception -ne $null)
                {
                    throw $copyStreamOp.Exception
                }
            }
        }
    }
    else
    {
        throw "Cannot copy from $sourceUrl"
    }
    [System.IO.Compression.ZipFile]::ExtractToDirectory($TempPath, $DestinationDir)
    Remove-Item $TempPath
}
function Download($sourceUrl,$DestinationDir ) 
{
    $TempPath = [System.IO.Path]::GetTempFileName()
    if (($sourceUrl -as [System.URI]).AbsoluteURI -ne $null)
    {
        $handler = New-Object System.Net.Http.HttpClientHandler
        $client = New-Object System.Net.Http.HttpClient($handler)
        $client.Timeout = New-Object System.TimeSpan(0, 30, 0)
        $cancelTokenSource = [System.Threading.CancellationTokenSource]::new()
        $responseMsg = $client.GetAsync([System.Uri]::new($sourceUrl), $cancelTokenSource.Token)
        $responseMsg.Wait()
        if (!$responseMsg.IsCanceled)
        {
            $response = $responseMsg.Result
            if ($response.IsSuccessStatusCode)
            {
                $downloadedFileStream = [System.IO.FileStream]::new($TempPath, [System.IO.FileMode]::Create, [System.IO.FileAccess]::Write)
                $copyStreamOp = $response.Content.CopyToAsync($downloadedFileStream)
                $copyStreamOp.Wait()
                $downloadedFileStream.Close()
                if ($copyStreamOp.Exception -ne $null)
                {
                    throw $copyStreamOp.Exception
                }
            }
        }
    }
    else
    {
        throw "Cannot copy from $sourceUrl"
    }
    [System.IO.Compression.ZipFile]::ExtractToDirectory($TempPath, $DestinationDir)
    Remove-Item $TempPath
}
function Expand-ZIPFile($file, $destination) 
{ 
    $shell = new-object -com shell.application 
    $zip = $shell.NameSpace($file) 
    foreach($item in $zip.items()) 
    { 
        # Unzip the file with 0x14 (overwrite silently) 
        $shell.Namespace($destination).copyhere($item, 0x14) 
    } 
} 
WriteDateLog
WriteLog "Downloading dotnet-install.ps1" 
$url = 'https://dot.net/v1/dotnet-install.ps1' 
$EditionId = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name 'EditionID').EditionId
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	Download $url $source 
	WriteLog "dotnet-install.ps1 copied" 
}
else
{
	$webClient = New-Object System.Net.WebClient  
	$webClient.DownloadFile($url,$source + "\dotnet-install.ps1" )  
	WriteLog "dotnet-install.ps1 copied" 
}


WriteDateLog
WriteLog "Downloading github" 
$url = 'https://github.com/git-for-windows/git/releases/download/v2.17.0.windows.1/Git-2.17.0-64-bit.exe' 
$EditionId = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name 'EditionID').EditionId
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	Download $url $source 
	WriteLog "Git-2.17.0-64-bit.exe copied" 
}
else
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	$webClient = New-Object System.Net.WebClient  
	$webClient.DownloadFile($url,$source + "\Git-2.17.0-64-bit.exe" )  
	WriteLog "Git-2.17.0-64-bit.exe copied" 
}


WriteLog "Configuring firewall" 
function Add-FirewallRulesNano
{
New-NetFirewallRule -Name "HTTP" -DisplayName "HTTP" -Protocol TCP -LocalPort 80 -Action Allow -Enabled True
New-NetFirewallRule -Name "HTTPS" -DisplayName "HTTPS" -Protocol TCP -LocalPort 443 -Action Allow -Enabled True
New-NetFirewallRule -Name "WINRM1" -DisplayName "WINRM TCP/5985" -Protocol TCP -LocalPort 5985 -Action Allow -Enabled True
New-NetFirewallRule -Name "WINRM2" -DisplayName "WINRM TCP/5986" -Protocol TCP -LocalPort 5986 -Action Allow -Enabled True
}
function Add-FirewallRules
{
New-NetFirewallRule -Name "HTTP" -DisplayName "HTTP" -Protocol TCP -LocalPort 80 -Action Allow -Enabled True
New-NetFirewallRule -Name "HTTPS" -DisplayName "HTTPS" -Protocol TCP -LocalPort 443 -Action Allow -Enabled True
New-NetFirewallRule -Name "RDP" -DisplayName "RDP TCP/3389" -Protocol TCP -LocalPort 3389 -Action Allow -Enabled True
}
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	Add-FirewallRulesNano
}
else
{
	Add-FirewallRules
}
WriteLog "Firewall configured" 


WriteLog "Installing .Net Core" 
& "C:\source\dotnet-install.ps1" --version 2.1.200
WriteLog ".Net Core installed" 

WriteLog "Installing Git" 
Start-Process -FilePath "c:\source\Git-2.17.0-64-bit.exe" -Wait -ArgumentList "/VERYSILENT","/SUPPRESSMSGBOXES","/NORESTART","/NOCANCEL","/SP-","/LOG"

$count=0
while ((!(Test-Path "C:\Program Files\Git\bin\git.exe"))-and($count -lt 20)) { Start-Sleep 10; $count++}
WriteLog "git Installed" 

WriteLog "Building ASTOOL" 
mkdir \git
mkdir \git\config

WriteDateLog
WriteLog "Downloading astool.xml" 
$url = $astoolConfigFile 
$EditionId = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name 'EditionID').EditionId
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	Download $url "\git\config"
	WriteLog "astool.windows.xml copied" 
}
else
{
	$webClient = New-Object System.Net.WebClient  
	$webClient.DownloadFile($url, "\git\config\astool.windows.xml" )  
	WriteLog "astool.windows.xml copied" 
}

mkdir \git\dvr
mkdir \git\dvr\test1
mkdir \git\dvr\test2
cd \git
cd c:\git
Start-Process -FilePath "C:\Program Files\Git\bin\git.exe" -Wait -ArgumentList "clone","https://github.com/flecoqui/ASTool.git"

cd ASTool\cs\ASTool\ASTool
Start-Process -FilePath "$env:USERPROFILE\AppData\Local\Microsoft\dotnet\dotnet.exe" -Wait -ArgumentList  "publish", "c:\git\ASTool\cs\ASTool\ASTool","--self-contained", "-c", "Release", "-r", "win10-x64","--output","c:\git\ASTool\cs\ASTool\ASTool\bin"
cd c:\git\ASTool\cs\ASTool\ASTool\bin
#astool --help
Start-Process -FilePath "c:\git\ASTool\cs\ASTool\ASTool\bin\ASTool.exe" -Wait -ArgumentList "--help"
WriteLog "ASTOOL built" 

WriteLog "Installing ASTOOL as a service" 
Start-Process -FilePath "c:\git\ASTool\cs\ASTool\ASTool\bin\ASTool.exe" -Wait -ArgumentList "--install", "--configfile", "c:\git\config\astool.windows.xml"
WriteLog "ASTOOL Installed" 

WriteLog "Starting ASTOOL as a service" 
Start-Process -FilePath "c:\git\ASTool\cs\ASTool\ASTool\bin\ASTool.exe" -Wait -ArgumentList "--start"
WriteLog "ASTOOL started" 

WriteLog "Initialization completed !" 
