# Palia Voice AI — Phase 1

Windows WPF/.NET 8 starter for local TTS benchmarking. The first milestone is deliberately limited to text input, voice selection, synthesis adapter, audio playback, and benchmark logging. OCR/game integration is not included yet.

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
- WPF UI with a Dialogue tab and a Capture tab
- Audio playback abstraction: included (NAudio)
- TTS abstraction and deterministic mock engine: included. CPU and CUDA stay unavailable until Kokoro is connected; selecting them does not silently run the mock
- Screen region capture, Palia window lookup, and Windows Graphics Capture of the game window
- English Windows OCR, text cleanup, and stable-line / duplicate detection
- Benchmark CSV: loaded on startup and saved under `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv`
- Capture settings: `%LOCALAPPDATA%\PaliaVoiceAI\capture.json`
- KokoroSharp adapter: still a follow-up. Do not install CUDA/cuDNN manually yet

## Tests
```powershell
dotnet test .\PaliaVoiceAI.sln
```

## Next step
Pin KokoroSharp and replace `MockTtsEngine` for Phase 1. For the Capture tab, launch Palia, use Preview until the region contains the dialogue, then start monitoring. The suggested bottom region is a starting guess, and in-game OCR has not been verified yet. The mock engine still does not synthesize speech.
