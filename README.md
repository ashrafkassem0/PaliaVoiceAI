# Palia Voice AI

Windows WPF/.NET 8 app that reads a dialogue region and speaks it with local Kokoro TTS. Cloud TTS is not used.

## Prerequisites
- Windows 10/11 x64
- .NET 8 SDK
- Visual Studio 2022 (Desktop development with .NET) or `dotnet` CLI

## Build
```powershell
dotnet restore
dotnet build .\PaliaVoiceAI.sln
dotnet run --project .\src\PaliaVoiceAI.App\PaliaVoiceAI.App.csproj
```

## Current implementation status
- Dialogue tab defaults to CPU. Generate speaks with Kokoro 82M. If `kokoro.onnx` is missing, the first run downloads it (about 310 MB) to `%LOCALAPPDATA%\PaliaVoiceAI\models\kokoro.onnx` and then uses that file.
- Voices next to the app include `af_bella`, `af_nicole`, `am_michael`, and `bm_george`. `af_bella` produced a real 16-bit, 24 kHz, mono WAV. The other three have not been spoken yet.
- CUDA is listed and falls back to CPU on this build. The CPU ONNX Runtime package has no CUDA entry point. CUDA and cuDNN were not installed.
- Mock remains available and stays silent.
- A measured CPU run, 3 threads, voice `af_bella`, after the model was already on disk: generation 1,598 ms, audio 4,510 ms, RTF 0.354.
- Capture: Palia window lookup, region capture, English Windows OCR, text cleanup, and duplicate suppression. One live Einar card was read. The duplicate close and a nameless card are still open.
- Benchmark CSV: `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv`
- Capture settings: `%LOCALAPPDATA%\PaliaVoiceAI\capture.json`

## Tests
```powershell
dotnet test .\PaliaVoiceAI.sln
```

48 tests passed on 2026-11-17 (Core 45, OCR 3).

## Install
The model is not in the package. The first speech downloads it.

```powershell
dotnet publish .\src\PaliaVoiceAI.App\PaliaVoiceAI.App.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\win-x64
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\installer\Install.ps1
```

That copies the CPU build to `%LOCALAPPDATA%\Programs\PaliaVoiceAI` and adds a Start menu shortcut. Removal:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\Programs\PaliaVoiceAI\Uninstall.ps1"
```

Uninstall leaves `%LOCALAPPDATA%\PaliaVoiceAI` in place, including the model and settings. This was checked on the reference PC. Another PC has not installed it.

## Voice library
The window opens on VoiceBox. Each card is a villager with an assigned Kokoro voice. Change the voice or pace from the panel on the right, press the microphone to preview, and turn on Apply voice so a captured line uses that villager. A card with no readable name is not spoken. The villager list, default voice ids, and avatar file names live in `npc-voices.json` beside the program (the project copy is `src/PaliaVoiceAI.Core/Npc/npc-voices.json`). If `%LOCALAPPDATA%\PaliaVoiceAI\npc-voices.json` exists and is newer, that copy is used instead. Favorites and custom assignments are still saved in `%LOCALAPPDATA%\PaliaVoiceAI\voices.json`. Avatars are the wiki profile icons in the `Avatars` folder. Taylin has no profile icon on the wiki, so that card keeps the drawn portrait.

These are stock Kokoro voices, not the original performers.

## Next step
The checks that are still open are in `PaliaVoiceAI-Open-Checks.md`. The package on this PC can be installed and removed. A second PC has not tried it.
