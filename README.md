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
- Capture tab: Palia window lookup, region capture, English Windows OCR, text cleanup, and duplicate suppression. In-game OCR has not been verified.
- Benchmark CSV: `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv`
- Capture settings: `%LOCALAPPDATA%\PaliaVoiceAI\capture.json`

## Tests
```powershell
dotnet test .\PaliaVoiceAI.sln
```

Release test run on 2026-11-17: 24 passed. Debug and Release builds completed with 0 warnings.

## Voice library
The window opens on VoiceBox. Each card is a villager with an assigned Kokoro voice. Change the voice or pace from the panel on the right, press the microphone to preview, and turn on Apply voice so a captured line uses that villager. A card with no readable name uses Unknown. The villager list, default voice ids, and avatar file names live in `npc-voices.json` beside the program (the project copy is `src/PaliaVoiceAI.Core/Npc/npc-voices.json`). If `%LOCALAPPDATA%\PaliaVoiceAI\npc-voices.json` exists and is newer, that copy is used instead. Favorites and custom assignments are still saved in `%LOCALAPPDATA%\PaliaVoiceAI\voices.json`. Avatars are the wiki profile icons in the `Avatars` folder. Taylin has no profile icon on the wiki, so that card keeps the drawn portrait.

These are stock Kokoro voices, not the original performers.

## Next step
Launch Palia, use Settings to preview the dialogue region, then turn on Live capture. The name-plate match was checked against a saved Kenyatta card, not against the live game. Listening quality is still open.
