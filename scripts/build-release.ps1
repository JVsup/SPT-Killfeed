[CmdletBinding()]
param(
    [string] $SptPath,

    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$projectPath = Join-Path $repositoryRoot 'src\SPT.Killfeed\SPT.Killfeed.csproj'
$localPropsPath = Join-Path $repositoryRoot 'Directory.Build.props.user'
$nugetConfigPath = Join-Path $repositoryRoot 'NuGet.Config'
$licensePath = Join-Path $repositoryRoot 'LICENSE'
$uiSourceRoot = Join-Path $repositoryRoot 'src\SPT.Killfeed\UI'
$uiFileNames = @(
    'Faction_BEAR.png'
    'Faction_Scav.png'
    'Faction_Unknown.png'
    'Faction_USEC.png'
)

if ([string]::IsNullOrWhiteSpace($SptPath) -and (Test-Path -LiteralPath $localPropsPath -PathType Leaf)) {
    [xml] $localProps = Get-Content -LiteralPath $localPropsPath -Raw
    $configuredPath = @($localProps.Project.PropertyGroup | ForEach-Object { $_.SptPath } | Where-Object { $_ })
    if ($configuredPath.Count -gt 0) {
        $SptPath = [string] $configuredPath[0]
    }
}

if ([string]::IsNullOrWhiteSpace($SptPath)) {
    throw 'SPT path is not configured. Pass -SptPath or create Directory.Build.props.user.'
}

$resolvedSptPath = (Resolve-Path -LiteralPath $SptPath).Path

$msbuildProperty = "-p:SptPath=$resolvedSptPath"
if (-not $NoRestore) {
    & dotnet restore $projectPath --configfile $nugetConfigPath $msbuildProperty
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }
}

& dotnet build $projectPath --configuration Release --no-restore $msbuildProperty '-p:ContinuousIntegrationBuild=true'
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

$pluginDll = Join-Path $repositoryRoot 'src\SPT.Killfeed\bin\Release\SPT.Killfeed.dll'
if (-not (Test-Path -LiteralPath $pluginDll -PathType Leaf)) {
    throw "Expected plugin output not found: $pluginDll"
}

if (-not (Test-Path -LiteralPath $licensePath -PathType Leaf)) {
    throw "Expected license not found: $licensePath"
}

foreach ($uiFileName in $uiFileNames) {
    $uiFile = Join-Path $uiSourceRoot $uiFileName
    if (-not (Test-Path -LiteralPath $uiFile -PathType Leaf)) {
        throw "Expected UI asset not found: $uiFile"
    }
}

$artifactsRoot = Join-Path $repositoryRoot 'artifacts'
$packageRoot = Join-Path $artifactsRoot 'package'
$pluginPackageRoot = Join-Path $packageRoot 'BepInEx\plugins\SPT.Killfeed'
$uiPackageRoot = Join-Path $pluginPackageRoot 'UI'
$distRoot = Join-Path $repositoryRoot 'dist'
$zipPath = Join-Path $distRoot 'SPT-Killfeed-4.1.0.zip'

foreach ($safePath in @($artifactsRoot, $distRoot)) {
    $fullPath = [System.IO.Path]::GetFullPath($safePath)
    if (-not $fullPath.StartsWith($repositoryRoot + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify a path outside the repository: $fullPath"
    }
}

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $uiPackageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $distRoot -Force | Out-Null

Copy-Item -LiteralPath $pluginDll -Destination $pluginPackageRoot
Copy-Item -LiteralPath $licensePath -Destination $pluginPackageRoot
foreach ($uiFileName in $uiFileNames) {
    Copy-Item -LiteralPath (Join-Path $uiSourceRoot $uiFileName) -Destination $uiPackageRoot
}

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal

$hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
Write-Host "Release created: $zipPath"
Write-Host "SHA256: $($hash.Hash)"
