
#usage install-software-windows.ps1 dnsname

param
(
      [string]$dnsName = $null,
	  [string]$adminUser
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

https://github.com/git-for-windows/git/releases/download/v2.17.0.windows.1/Git-2.17.0-64-bit.exe

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
PowerShell -NoProfile -ExecutionPolicy Bypass -Command "C:\temp\ASTool\dotnet-install.ps1"
WriteLog ".Net Core installed" 

WriteLog "Installing Git" 
Git-2.17.0-64-bit.exe  /VERYSILENT
WriteLog "Git installed" 

WriteLog "Building ASTOOL" 
md \git
cd \git
git clone https://github.com/flecoqui/ASTool.git
cd ASTool\cs\ASTool\ASTool
dotnet publish -c Release -r ubuntu.16.10-x64
cd bin\Release\netcoreapp2.0\ubuntu.16.10-x64\publish
astool --help
WriteLog "ASTOOL built" 

WriteLog "Installing ASTOOL as a service" 
sc.exe create ASTOOL binpath= "cmd.exe /c c:\git\ASTool\cs\ASTool\ASTool\bin\Release\netcoreapp2.0\ubuntu.16.10-x64\publish\ASTool.exe -s -D" type= own start= auto DisplayName= "ASTOOL"
WriteLog "ASTOOL Installed" 

WriteLog "Initialization completed !" 
WriteLog "Rebooting !" 
Restart-Computer -Force       
