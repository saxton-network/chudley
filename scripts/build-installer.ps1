param([string] $InnoCompiler)

$ErrorActionPreference = 'Stop'
& "$PSScriptRoot/publish-windows.ps1" -BuildInstaller -InnoCompiler $InnoCompiler
if ($LASTEXITCODE -and $LASTEXITCODE -ne 0) { throw 'Chudley installer build failed' }
