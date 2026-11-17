param(
    [string]$InstallDir
)

$ErrorActionPreference = 'Stop'

function Close-PaliaVoiceApp {
    Add-Type -Namespace PaliaVoiceUninstall -Name Native -MemberDefinition '[DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);' -ErrorAction SilentlyContinue
    $procs = @(Get-Process -Name 'PaliaVoiceAI.App' -ErrorAction SilentlyContinue)
    foreach ($proc in $procs) {
        if ($proc.MainWindowHandle -ne [IntPtr]::Zero) {
            [PaliaVoiceUninstall.Native]::PostMessage($proc.MainWindowHandle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
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

$key = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Uninstall\PaliaVoiceAI'
if (-not $InstallDir) {
    $InstallDir = (Get-ItemProperty -Path $key -ErrorAction SilentlyContinue).InstallLocation
}
if (-not $InstallDir) {
    throw 'Palia Voice AI is not installed for this user.'
}

$exe = Join-Path $InstallDir 'PaliaVoiceAI.App.exe'
if (-not (Test-Path -LiteralPath $exe)) {
    throw "No installed app at $InstallDir"
}

Close-PaliaVoiceApp
$shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'Palia Voice AI.lnk'
if (Test-Path -LiteralPath $shortcut) {
    Remove-Item -LiteralPath $shortcut -Force
}
if (Test-Path -LiteralPath $key) {
    Remove-Item -LiteralPath $key -Recurse -Force
}

$quoted = $InstallDir.Replace("'", "''")
$tempScript = Join-Path $env:TEMP ('PaliaVoiceAI-remove-' + [guid]::NewGuid().ToString('N') + '.ps1')
Set-Content -LiteralPath $tempScript -Encoding UTF8 -Value @"
Start-Sleep -Seconds 2
Remove-Item -LiteralPath '$quoted' -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath '$($tempScript.Replace("'", "''"))' -Force -ErrorAction SilentlyContinue
"@
Start-Process -FilePath 'powershell.exe' -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', $tempScript) -WindowStyle Hidden
Write-Output "REMOVED=$InstallDir"
