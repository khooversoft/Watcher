#
# Load Watcher test data
#


param (
    [string] $Folder = "Entities\*",

    [string] $ConfigFile = "..\..\Src\WatcherCmd\Configs\DevConfig.json"
)

$ErrorActionPreference = "Stop";

$watcherCmdExePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\Src\WatcherCmd\bin\Debug\netcoreapp3.1\WatcherCmd.exe";
if( !(Test-Path -Path $watcherCmdExePath -PathType Leaf ) )
{
    Write-Error "$watcherCmdExePath does not exist";
    Exit(1);
}

$testApiServerExePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\Test\TestRestApiServer\bin\Debug\netcoreapp3.1\TestRestApiServer.exe";
if( !(Test-Path -Path $testApiServerExePath -PathType Leaf ) )
{
    Write-Error "$testApiServerExePath does not exist";
    Exit(1);
}

$agentExePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\Test\TestRestApiServer\bin\Debug\netcoreapp3.1\TestRestApiServer.exe";
if( !(Test-Path -Path $agentExePath -PathType Leaf ) )
{
    Write-Error "$agentExePath does not exist";
    Exit(1);
}

# Start API Test Servers
Start-Process -FilePath pwsh.exe -ArgumentList "-NoExit -Command $testApiServerExePath Port=5010";
Start-Process -FilePath pwsh.exe -ArgumentList "-NoExit -Command $testApiServerExePath Port=5020";
Start-Process -FilePath pwsh.exe -ArgumentList "-NoExit -Command $testApiServerExePath Port=5030";

Start-Process -FilePath pwsh.exe -ArgumentList "-NoExit -Command $agentExePath AgentId=agent_0";

# Start-Process -FilePath pwsh.exe -ArgumentList "-NoExit -Command $watcherCmdExePath Monitor configFile=$ConfigFile";
