param(
    [string]$Source,
    [string]$InstallDir,
    [switch]$CheckOnly
)

$ErrorActionPreference = 'Stop'
$MinimumBuild = 19041

function Close-PaliaVoiceApp {
    Add-Type -Namespace PaliaVoiceInstall -Name Native -MemberDefinition '[DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);' -ErrorAction SilentlyContinue
    $procs = @(Get-Process -Name 'PaliaVoiceAI.App' -ErrorAction SilentlyContinue)
    foreach ($proc in $procs) {
        if ($proc.MainWindowHandle -ne [IntPtr]::Zero) {
            [PaliaVoiceInstall.Native]::PostMessage($proc.MainWindowHandle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        }
    }

    $deadline = (Get-Date).AddSeconds(10)
    while ((Get-Date) -lt $deadline) {
        if (-not (Get-Process -Name 'PaliaVoiceAI.App' -ErrorAction SilentlyContinue)) {
            return
        }
        Start-Sleep -Milliseconds 200
    }

    if (Get-Process -Name 'PaliaVoiceAI.App' -ErrorAction SilentlyContinue) {
        throw 'Palia Voice AI is still open. Close it, then run this again.'
    }
}

$build = [int](Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion').CurrentBuild
Write-Output "Windows build $build. Minimum is $MinimumBuild."
if ($build -lt $MinimumBuild) {
    throw "Windows build $build is older than $MinimumBuild."
}
if ($CheckOnly) {
    exit 0
}

if (-not $Source) {
    $Source = Join-Path (Split-Path $PSScriptRoot -Parent) 'artifacts\win-x64'
}
if (-not $InstallDir) {
    $InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\PaliaVoiceAI'
}

$exe = Join-Path $Source 'PaliaVoiceAI.App.exe'
if (-not (Test-Path -LiteralPath $exe)) {
    throw "The CPU package was not found at $exe"
}

$voice = Join-Path $Source 'voices\af_bella.npy'
if (-not (Test-Path -LiteralPath $voice)) {
    throw "The CPU package is missing voices\af_bella.npy. Kokoro cannot speak until that file is next to the app."
}

Close-PaliaVoiceApp
New-Item -ItemType Directory -Force -Path $InstallDir | Out-Null
& robocopy $Source $InstallDir /MIR /NFL /NDL /NJH /NJS /nc /ns /np
if ($LASTEXITCODE -ge 8) {
    throw "Copy failed with robocopy code $LASTEXITCODE."
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Uninstall.ps1') -Destination (Join-Path $InstallDir 'Uninstall.ps1') -Force

$installedExe = Join-Path $InstallDir 'PaliaVoiceAI.App.exe'
$version = (Get-Item -LiteralPath $installedExe).VersionInfo.FileVersion
$shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'Palia Voice AI.lnk'
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortcut($shortcut)
$link.TargetPath = $installedExe
$link.WorkingDirectory = $InstallDir
$link.Description = 'Palia Voice AI'
$link.Save()

$uninstall = "powershell.exe -NoProfile -ExecutionPolicy Bypass -File `"$InstallDir\Uninstall.ps1`""
$key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\PaliaVoiceAI'
New-Item -Path $key -Force | Out-Null
New-ItemProperty -Path $key -Name DisplayName -Value 'Palia Voice AI' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $key -Name DisplayVersion -Value $version -PropertyType String -Force | Out-Null
New-ItemProperty -Path $key -Name Publisher -Value 'Palia Voice AI' -PropertyType String -Force | Out-Null
New-ItemProperty -Path $key -Name InstallLocation -Value $InstallDir -PropertyType String -Force | Out-Null
New-ItemProperty -Path $key -Name UninstallString -Value $uninstall -PropertyType String -Force | Out-Null
New-ItemProperty -Path $key -Name NoModify -Value 1 -PropertyType DWord -Force | Out-Null
New-ItemProperty -Path $key -Name NoRepair -Value 1 -PropertyType DWord -Force | Out-Null

Write-Output "INSTALLED=$InstallDir"
Write-Output "VERSION=$version"
Write-Output "SHORTCUT=$shortcut"
