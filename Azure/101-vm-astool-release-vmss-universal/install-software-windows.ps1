
#usage install-software-windows.ps1 dnsname

param
(
      [string]$dnsName = $null,
	  [string]$astoolConfigFile = $null
)


#Create folders
mkdir \astool
mkdir \astool\release
mkdir \astool\dvr
mkdir \astool\dvr\test1
mkdir \astool\dvr\test2
mkdir \astool\config
mkdir \astool\log

$source = 'C:\astool\log' 
If (!(Test-Path -Path $source -PathType Container)) {New-Item -Path $source -ItemType Directory | Out-Null} 

function WriteLog($msg)
{
Write-Host $msg
$msg >> C:\astool\log\install.log
}
function WriteDateLog
{
date >> C:\astool\log\install.log
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

WriteDateLog
WriteLog "Downloading ASTOOL"  
$url = 'https://github.com/flecoqui/ASTool/raw/master/Releases/LatestRelease.win.zip' 
$EditionId = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name 'EditionID').EditionId
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	DownloadAndUnzip $url "\astool\release" 
	WriteLog "ASTOOL Installed" 
}
else
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	$webClient = New-Object System.Net.WebClient  
	$webClient.DownloadFile($url,"\astool\release\LatestRelease.win.zip" )  
	WriteLog "Installing ASTOOL"  
	# Function to unzip file contents 
	Expand-ZIPFile -file "c:\astool\release\LatestRelease.win.zip" -destination "c:\astool\release" 
	WriteLog "ASTOOL Installed" 
}


WriteDateLog
WriteLog "Downloading astool.xml" 
$url = $astoolConfigFile 
$EditionId = (Get-ItemProperty -Path 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion' -Name 'EditionID').EditionId
if (($EditionId -eq "ServerStandardNano") -or
    ($EditionId -eq "ServerDataCenterNano") -or
    ($EditionId -eq "NanoServer") -or
    ($EditionId -eq "ServerTuva")) {
	Download $url "\astool\config"
	WriteLog "astool.windows.xml copied" 
}
else
{
    [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
	$webClient = New-Object System.Net.WebClient  
	$webClient.DownloadFile($url, "\astool\config\astool.windows.xml" )  
	WriteLog "astool.windows.xml copied" 
}



WriteLog "Installing ASTOOL as a service" 
Start-Process -FilePath "c:\astool\release\publish\ASTool.exe" -Wait -ArgumentList "--install", "--configfile", "c:\astool\config\astool.windows.xml"
WriteLog "ASTOOL Installed" 

WriteLog "Starting ASTOOL as a service" 
Start-Process -FilePath "c:\astool\release\publish\ASTool.exe" -Wait -ArgumentList "--start"
WriteLog "ASTOOL started" 

WriteLog "Initialization completed !" 
