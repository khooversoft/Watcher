#
# Load Watcher test data
#


param (
    [string] $Folder = "Entities\*",

    [string] $ConfigFile = "..\..\..\Src\WatcherCmd\Configs\DevConfig.json"
)

$ErrorActionPreference = "Stop";

$watcherCmdExePath = Join-Path -Path $PSScriptRoot -ChildPath "..\..\..\Src\WatcherCmd\bin\Debug\netcoreapp3.1\WatcherCmd.exe";
if( !(Test-Path -Path $watcherCmdExePath -PathType Leaf ) )
{
    Write-Error "$watcherCmdExePath does not exist";
    Exit(1);
}

$Folder = Join-Path $PSScriptRoot $Folder;
$ConfigFile = Join-Path $PSScriptRoot $ConfigFile;

Write-Host "Loading from File=$Folder";
& $watcherCmdExePath import File=$Folder configFile=$ConfigFile;
