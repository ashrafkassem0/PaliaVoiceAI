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
- WPF UI and benchmark result display: included
- Audio playback abstraction: included (NAudio)
- TTS abstraction and deterministic mock engine: included
- KokoroSharp adapter: intentionally isolated as a follow-up integration task. Its API/model packaging must be pinned and validated against the selected package version before shipping.
- CUDA: not enabled in this starter. Do not install CUDA/cuDNN manually yet. First pin the exact KokoroSharp/ONNX Runtime versions and validate their provider dependencies.
- Benchmark CSV: saved under `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv`

## Next step
Replace `MockTtsEngine` with the pinned KokoroSharp implementation, then run CPU and CUDA benchmarks on the target laptop. The mock engine validates the UI/audio/benchmark pipeline only; it does not synthesize speech.
