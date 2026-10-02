$ErrorActionPreference = 'Stop'

try {
    if (-not [Environment]::Is64BitOperatingSystem) {
        throw 'Revit 2024 requires 64-bit Windows.'
    }
    if (Get-Process -Name Revit -ErrorAction SilentlyContinue) {
        throw 'Close Revit before installing, then run Install.cmd again.'
    }

    $sourceDll = Join-Path $PSScriptRoot 'IFCInfo.dll'
    if (-not (Test-Path -LiteralPath $sourceDll -PathType Leaf)) {
        throw 'IFCInfo.dll is missing. Extract the entire ZIP before running Install.cmd.'
    }
    if ([string]::IsNullOrWhiteSpace($env:APPDATA)) {
        throw 'Windows APPDATA folder could not be found.'
    }

    $addinRoot = Join-Path $env:APPDATA 'Autodesk\Revit\Addins\2024'
    $dllDir = Join-Path $addinRoot 'IFCInfo'
    $targetDll = Join-Path $dllDir 'IFCInfo.dll'
    $manifestPath = Join-Path $addinRoot 'IFCInfo.addin'
    New-Item -ItemType Directory -Path $dllDir -Force | Out-Null
    Copy-Item -LiteralPath $sourceDll -Destination $targetDll -Force

    $escapedAssembly = [System.Security.SecurityElement]::Escape($targetDll)
    $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Command">
    <Name>IFC Info</Name>
    <Assembly>$escapedAssembly</Assembly>
    <AddInId>8875233F-C015-4400-9B76-8602141587CA</AddInId>
    <FullClassName>IFCInfo.IFCInfoCommand</FullClassName>
    <Text>IFC Info</Text>
    <VendorId>IFCI</VendorId>
    <VendorDescription>IFC link information utility</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
    [System.IO.File]::WriteAllText($manifestPath, $manifest, (New-Object System.Text.UTF8Encoding($false)))

    $sourceHash = (Get-FileHash -LiteralPath $sourceDll -Algorithm SHA256).Hash
    $installedHash = (Get-FileHash -LiteralPath $targetDll -Algorithm SHA256).Hash
    if ($sourceHash -ne $installedHash) {
        throw 'Installed DLL hash does not match the package.'
    }
    Write-Host "Installed IFC Info for Revit 2024: $manifestPath"
    Write-Host "DLL SHA256: $installedHash"
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}
