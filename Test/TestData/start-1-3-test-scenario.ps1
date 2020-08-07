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
    Write-Error "$testApiServerExePath does not 
    exist";
    Exit(1);
}

$agentExePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\Src\WatcherAgent\bin\Debug\netcoreapp3.1\WatcherAgent.exe";
if( !(Test-Path -Path $agentExePath -PathType Leaf ) )
{
    Write-Error "$agentExePath does not exist";
    Exit(1);
}

function StartProcess {
    param (
        [string] $exe,
        [string] $arg
    )

    Write-Host "Process: Starting... $exe";
    Write-Host "         Argument... $arg";

    $workingFolder = Split-Path -Path $exe -Parent;

    Start-Process -FilePath pwsh.exe -WorkingDirectory $workingFolder -ArgumentList "-NoExit -Command $exe $arg";
}

# Make sure the DB is initialized
& .\Load-TestData.ps1;

# Start API Test Servers
StartProcess -exe $testApiServerExePath -arg "Port=5010"
StartProcess -exe $testApiServerExePath -arg "Port=5020"
StartProcess -exe $testApiServerExePath -arg "Port=5030"

# Start agent
StartProcess -exe $agentExePath -arg "AgentId=agent_0";

# Start monitor
StartProcess -exe $watcherCmdExePath -arg "Monitor configFile=$ConfigFile";
