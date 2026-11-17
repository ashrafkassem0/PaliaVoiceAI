# Palia Voice AI — Project master

**Version:** 0.2 — English working copy  
**Updated:** 2026-11-17  
**App:** Windows desktop  
**Stack:** C# / .NET 8 / WPF / ONNX Runtime / Kokoro TTS / NAudio  
**Reference PC:** ASUS ROG Strix G16 G614PH-RV161W

This file is the source of truth. A task is not done because it is described here. Update the status only after the behavior was checked, and write what the check showed.

## How to execute work from this file

1. Read **Current snapshot** and **Next tasks**. Do the first task that is not blocked.
2. Stay inside the task's **Done when** line. Do not start the next phase while the current close condition is still open, unless the user says to.
3. After the check, change that task's status, name the files, and add one changelog row. Record the result (counts, timings, the exact text), not only that a command ran.
4. `dotnet restore` or a successful NuGet install is not proof that a feature works.
5. Do not mark live in-game behavior done unless Palia was actually running.

### Hard rules

- Read the game from the screen only. Do not read Palia's memory, inject a DLL, or change game files.
- No cloud TTS. CPU is the path that must keep working. Do not install CUDA Toolkit or cuDNN until the GPU package versions are pinned in this file. If CUDA is requested and the provider is missing, fall back to CPU and say so.
- Do not claim a Kokoro voice is the original actor.
- Do not replace `voiceId` values from `PaliaVoiceAI-NPC-Voice-Mapping.md`. That file is a recommendation. The live assignments are in `npc-voices.json`.
- Do not invent a child voice. Villagers with an empty `voiceId` stay unassigned until the user sets one.
- Shell is PowerShell. Do not chain commands with `&&`. Use `; if ($LASTEXITCODE -eq 0)`.
- If the Release exe is locked, close `PaliaVoiceAI.App` with `WM_CLOSE` (`0x0010`) before rebuilding. Do not force-kill it when the close message works.
- Do not commit unless the user asks.

### Commands

```powershell
dotnet test .\PaliaVoiceAI.sln --nologo
dotnet build .\src\PaliaVoiceAI.App\PaliaVoiceAI.App.csproj -c Release --nologo
```

Last full check on 2026-11-17: 29 tests passed (Core 26, OCR 3). Release build of the app succeeded with 0 warnings. The voice-library window listed Kenyatta, Eshe, Auni, Taylin, The Gardener, Ulfe, and Unassigned. The portrait pixels inside that window were not looked at.

---

# Current snapshot

| Phase | Close condition | State |
| --- | --- | --- |
| 1. Kokoro speech | Real local speech on CPU, then CUDA if it is available, with a repeatable measurement | CPU is closed. CUDA is not available on the installed ONNX package and falls back to CPU. Listening quality and "while Palia is running" were not measured. |
| 2. OCR and capture | Read a new dialogue line once, and do not read the same line again | Logic is tested. Live Palia capture is not. |
| 3. NPC voices | Pick the right voice profile for known dialogue, and do something clear when the speaker is unknown | Name-plate matching is unit-tested. The window can change and save a voice. Pace is sent to Kokoro and has not been heard. Live detection inside Palia is not done. |
| 4. Queue and cache | No overlapping speech, and no second generation when text, voice, and settings match | Not started. |
| 5. Settings and reliability | Settings, logging, and a long play session | Not started, except the capture settings that already save. |
| 6. Installer | Another Windows PC can install and remove it | Not started. |

**Do this next:** task T1 below. It is the remaining Phase 3 check that does not need the game.

---

# Next tasks

## T1 — Prove pace changes the audio

**Status:** Partial. The slider saves a pace and `KokoroTtsEngine` passes `KokoroTTSPipelineConfig.Speed`. Nobody has compared two files.

**Done when:** The same line and the same voice are generated at pace `0.85` and `1.15`. The slower pace produces a longer WAV. Write both durations and the voice id in the Phase 3 status. Do not write that someone listened unless they did.

**Where:** `src/PaliaVoiceAI.TTS/KokoroTtsEngine.cs` (`Speed` only; there is no pitch control). The UI slider is `0.85`–`1.15`. The engine clamps to `0.7`–`1.4`.

**Kokoro has no pitch setting in this integration.** Do not add a fake pitch control. The Phase 3 line "speed and pitch if the engine supports them" is satisfied by speed. Pitch stays unsupported.

## T2 — Live speaker check

**Status:** Blocked until Palia is running.

**Done when:** A real dialogue card whose first plate is a known name selects that villager. A card with no name plate selects Unknown. A name that only appears inside a sentence does not select that villager. Paste the OCR text into the Phase 3 status.

**Where:** `SpeakerIdentifier.FindNpcId` in `src/PaliaVoiceAI.Core/Npc/NpcProfiles.cs`, called from `MainWindow.Capture.cs`.

## T3 — Live OCR close for Phase 2

**Status:** Blocked until Palia is running.

**Done when:** The game window is found, the dialogue region contains the card, one new line is spoken or copied once, and the same line is suppressed inside the duplicate window. Record the process name, the region, and the text.

**Start from:** on a 1920×1200 frame, region `x=131, y=688, width=975, height=438`. That came from the Kenyatta screenshot (1024×576 crop at 70,330 size 520×210). Display scaling on this PC was 125% (a 1280×820 window was about 1600×1025 physical pixels). Re-check the region against the live window; do not assume the numbers still match.

**Expected cleaned line from that fixture, including the spelling:** `I'd wish you a hoppu Luna New Year, but I'm so ready for it to be over already.`

## T4 — Phase 4 queue

**Status:** Not started. Begin only after T1 is recorded, or when the user says to skip T1.

**Done when:** A new line cancels or waits on the previous one with no overlap, and a repeated line with the same voice, pace, and model is not generated again.

---

# 1. What the program is

Palia Voice AI is a separate Windows program. It reads an NPC dialogue card from the screen and speaks it with a local voice.

Target loop:

1. Capture the dialogue area.
2. Read it with OCR.
3. Keep the line only after the text has stopped changing, and drop a duplicate.
4. If the name plate is a known villager, use that profile. Otherwise use Unknown.
5. Speak it with local Kokoro, one generation at a time.
6. Play the WAV without touching the game.
7. Later, reuse a stored WAV when the line and the settings match. That cache does not exist yet.

## 1.1 Principles

- **Local-first.** Speech is generated on this PC after the model files are downloaded.
- **Game-safe.** Screen reading only.
- **Modular.** Capture, OCR, TTS, audio, and UI are separate projects.
- **Performance-aware.** Keeping Palia smooth matters as much as fast speech. This has not been measured with the game running.
- **CPU always works.** CUDA is optional.
- **No false voice claims.** These are licensed TTS voices, not the game's actors.

## 1.2 Not in the first release

- No replacement of Palia's own audio files.
- No DLL injection and no memory reads.
- No voice cloning.
- No cloud TTS during the intended run.
- CUDA speech on this machine. The request falls back to CPU.
- A finished installer.
- SQLite. Profiles and settings are JSON files today.

---

# 2. Reference PC

| Item | Value |
| --- | --- |
| Machine | ASUS ROG Strix G16 |
| Model | G614PH-RV161W |
| CPU | AMD Ryzen 9 8940HX |
| GPU | NVIDIA GeForce RTX 5050 Laptop, 8GB |
| RAM | 32GB DDR5-5200 |
| Disk | 1TB PCIe 4.0 NVMe |
| Panel | 16 in, 1920×1200, 165Hz |
| OS | Windows 11, build 10.0.26300 |
| SDK seen on 2026-11-17 | .NET SDK 10.0.404, building `net8.0` |
| Performance priority | Balanced: game smoothness and speech speed together |

The panel is 1920×1200, not 1920×1080. Any FPS benchmark must record the resolution the user actually plays at.

## 2.1 Resource policy

- CPU is the guaranteed path.
- CUDA may be tried on the RTX 5050 only after the package versions are written down here.
- Do not turn CUDA on by default before FPS, 1% low, and VRAM are measured.
- One TTS generation at a time until a queue exists.
- Cache audio only when the cache key includes every setting that changes the sound.

---

# 3. Acceptance

## 3.1 Functional

| ID | Requirement | Acceptance | Now |
| --- | --- | --- | --- |
| FR-001 | Type English text | A typed line can be sent to TTS | Done in Voicestudio |
| FR-002 | Choose a voice | Pick from the Kokoro English list | Done |
| FR-003 | Local speech | A playable WAV with no cloud API | Done on CPU |
| FR-004 | Play and stop | Play the WAV; stop playback | Play is done. Stop stops playback and a download. An in-flight Kokoro call runs until the sentence ends |
| FR-005 | Choose a backend | CPU or CUDA when both exist | CPU works. CUDA falls back to CPU. Mock stays silent |
| FR-006 | OCR the dialogue | Acceptable text from the card | Tested on a drawn image and the Kenyatta fixture. Not tested on the live game |
| FR-007 | Skip duplicates | Do not generate the same text again inside the duplicate window | Unit-tested. Default window 8 seconds. Not tested live |
| FR-008 | Identify the NPC | Match the speaker, or make the unknown case obvious | Name plate only. Unit-tested |
| FR-009 | Voice profiles | Each NPC keeps its own voice and pace | Roster in `npc-voices.json`. User overrides in `voices.json` |
| FR-010 | Audio queue | Order lines and do not overlap them | Not started |
| FR-011 | Stop now | Stop playback and cancel queued work | Partial. See FR-004 |
| FR-012 | Audio cache | Reuse audio when text, voice, and settings match | Not started |
| FR-013 | Benchmark | Record generation time, audio length, RTF, and backend | One CPU row exists. The CSV is reloaded |
| FR-014 | Remember settings | Restore local settings on launch | Capture settings, voice overrides, and the model path do this |
| FR-015 | Find the game | Detect Palia or let the user capture manually | Detector is tested against process-name rules. A running game was not checked |
| FR-016 | Set the capture region | Use a chosen region or a saved one | Saved in `capture.json`. Needs a live check |
| FR-017 | Do-not-disturb | Turn OCR or speech off without exiting | Live capture and Apply voice can be switched off |

## 3.2 Non-functional

These numbers are proposed gates. They are not measured results.

| ID | Requirement | Proposed gate |
| --- | --- | --- |
| NFR-001 | Game FPS | Average FPS drop of 5% or less in the acceptance scene |
| NFR-002 | 1% low | Drop of 10% or less |
| NFR-003 | Time to first audio | Measure it. Aim for under one second when that is possible |
| NFR-004 | Stability | A long play session with no crash and no unbounded memory growth |
| NFR-005 | Privacy | Do not send screenshots or dialogue text to an external server |
| NFR-006 | Distribution | An installer and written dependency list |
| NFR-007 | Replaceable TTS | Another engine can be added behind `ITtsEngine` without rewriting Core |
| NFR-008 | Errors | An OCR or TTS failure does not close the app |
| NFR-009 | Cancel | An old line can be dropped when the dialogue moves on |
| NFR-010 | Logs | Errors and timings, without storing extra personal data |

---

# 4. Technical decisions

| Decision | Choice | Why |
| --- | --- | --- |
| Language | C# | Windows integration |
| Framework | .NET 8 | Target runtime. SDK 10 can build it |
| UI | WPF | Desktop app |
| Shape | Several projects in one solution | Swap one layer without rewriting the others |
| Audio | NAudio 2.2.1 | Play WAV data |
| TTS | Kokoro 82M float32 | Small local multi-voice model |
| TTS library | KokoroSharp 0.8.4 and KokoroSharp.CPU 0.8.4 | The CPU package omits voice assets, so the app also references KokoroSharp and copies `voices\*.npy` |
| Runtime | Microsoft.ML.OnnxRuntime 1.22.0 | Comes with the CPU package |
| OCR | Windows OCR, English | Fewer extra dependencies for the first version |
| Profiles and settings | JSON files | Already in use. SQLite was only a later idea |
| Packaging | A Windows installer later | After the features exist |

## 4.1 No Python in the current app

C# owns the window, capture, dialogue rules, profiles, playback, and startup. Reconsider Python only if a measured benchmark shows that another TTS model needs it.

## 4.2 Kokoro is the engine in use, not a finished product choice

It has produced one measured CPU sentence. Still open before a public build: listening quality, the other English voices, long lines, ONNX GPU compatibility, and a license pass for redistribution.

A restored NuGet package does not mean the GPU path works.

## 4.3 CUDA policy

- CPU stays available.
- CUDA is optional and off unless the user chooses it.
- Do not install CUDA or cuDNN by hand before the versions are written in this file.
- Do not treat DirectML and CUDA as interchangeable without a test.
- On 2026-11-17 the CPU `onnxruntime.dll` did not export `OrtSessionOptionsAppendExecutionProvider_CUDA`. The engine reported that, set `ActiveBackend` to Cpu, and the UI said the run used CPU.

## 4.4 Voice assignments

`src/PaliaVoiceAI.Core/Npc/npc-voices.json` is the roster. It is copied next to the exe. If `%LOCALAPPDATA%\PaliaVoiceAI\npc-voices.json` exists and is newer, that copy wins.

`%LOCALAPPDATA%\PaliaVoiceAI\voices.json` stores the user's voice, pace, favorite, and selected card. An old override beats the roster default. Do not delete that file to "reset" a voice unless the user asks.

Empty `voiceId` means Unassigned. Preview and auto-speak say that no voice is set. They do not pick a substitute.

Known defaults that must stay unless the user edits the JSON: Kenyatta `af_sky`, Eshe `bf_emma`, Tish `af_sarah`, Jina `af_heart`, Sifuu `af_nicole`, Najuma `af_jessica`, Tamala `bf_alice`, Caleri `af_kore`, Elouisa `af_nova`, Subira `af_bella`, Hekla `bf_isabella`, Delaila `bf_lily`, Ashura `am_michael`, Reth `am_echo`, Hassian `am_fenrir`, Hodari `bm_daniel`, Jel `bm_george`, Zeki `am_liam`, Chayne `am_adam`, Einar `bm_lewis`, Badruu `am_puck`, Kenli `am_onyx`, Nai'o `am_eric`, Unknown `af_bella`. Auni, Eshelon, Tau, Taylin, The Gardener, and Ulfe have no voice yet.

Avatars are `src/PaliaVoiceAI.App/Avatars/{id}.png`, copied to `Avatars\` beside the exe. Twenty-eight wiki profile icons are there. Taylin has no profile icon on the wiki, so that card uses the drawn portrait. Nai'o is `naio.png`. The Gardener is `gardener.png`.

---

# 5. Starting benchmark settings

These are starting points, not tuned results.

| Setting | Start | State |
| --- | --- | --- |
| CPU intra-op threads | 3 | Used for the measured CPU sentence |
| CPU inter-op threads | 1 | Set in the engine |
| Graph optimization | Enable all | Set in the engine |
| Concurrent TTS jobs | 1 | Design rule. A second click cancels the tracked request, but Kokoro itself is not interrupted mid-call |
| CUDA memory arena | 1GB as a probe only | Not used. Not a cap on total VRAM |
| OCR poll | 400 ms | Code default |
| Stable reads before a line is accepted | 2 | Code default |
| Duplicate window | 8 s | Code default |
| Dialogue region on 1920×1200 | 131, 688, 975×438 | From one Kenyatta screenshot. Not confirmed live |

---

# 6. Solution shape

What exists:

```text
PaliaVoiceAI.sln
├── src/PaliaVoiceAI.App          WPF voice library, studio, capture settings
├── src/PaliaVoiceAI.Core         models, NPC roster, speaker id, paths
├── src/PaliaVoiceAI.Capture      game-window detect, GDI, Windows Graphics Capture
├── src/PaliaVoiceAI.OCR          Windows OCR, text cleanup, dialogue tracker
├── src/PaliaVoiceAI.TTS          KokoroTtsEngine, model download, MockTtsEngine
├── src/PaliaVoiceAI.Audio        NAudio playback
└── tests/                        Core and OCR
```

Not created yet: `PaliaVoiceAI.Infrastructure`, a TTS test project, a performance test project, `docs/`, `installers/`, and a `models/` folder in the repo. The ONNX file lives in the user profile, not in git.

Target framework for App, Capture, OCR, and OCR tests: `net8.0-windows10.0.22621.0` with minimum version `10.0.19041.0`. Core, TTS, and Core tests: `net8.0`. Audio: `net8.0-windows`.

There is no dependency-injection container. `MainWindow` constructs the engines.

---

# 7. Status ledger

**Done** — the named behavior was checked.  
**Partial** — some of it works, and the rest is named in the details.  
**Not started** — no code for it.  
**Not verified** — code or a decision exists, and the behavior was not checked.  
**Decided only** — a choice was written down, and the feature was not built.  
**Deferred** — left for a later phase on purpose.

## 7.1 Solution setup

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| SET-001 | Create the solution | Done | `PaliaVoiceAI.sln`. Debug and Release both map to the projects |
| SET-002 | Core project | Done | `PaliaVoiceAI.Core` |
| SET-003 | TTS project | Done | Mock and `KokoroTtsEngine`. KokoroSharp.CPU 0.8.4 |
| SET-004 | Audio project | Done | NAudio played the measured Kokoro WAV |
| SET-005 | WPF app | Done | Voice library UI |
| SET-006 | Capture project | Done | Window detect, GDI, Windows Graphics Capture. Live game not tested |
| SET-007 | OCR project | Done | `WindowsOcrEngine` |
| SET-008 | Infrastructure project | Not started | JSON files cover the current settings |
| SET-009 | Dependency injection | Not started | The window creates the engines |
| SET-010 | Logging | Not started | Failures go to status text |
| SET-011 | Unit tests | Done | 2026-11-17: Core 26, OCR 3, all passed |
| SET-012 | CI build | Not started | No pipeline |
| SET-013 | Build on Windows | Done | Release app build on 2026-11-17, 0 warnings |
| SET-014 | Launch the WPF window | Partial | The voice library opened and UI Automation saw the villager names. Capture was not clicked against Palia |

## 7.2 UI

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| UI-001 | Main window | Done | Dark voice library: VoiceBox, Soundboard, Voicestudio, Settings |
| UI-002 | Dialogue box | Done | Multiline box in Voicestudio |
| UI-003 | Voice list | Done | Kokoro English voices. The combo shows the label, such as Bella |
| UI-004 | Backend choice | Done | Default CPU. Mock is silent. CUDA falls back to CPU |
| UI-005 | CPU threads | Done | Passed as `IntraOpNumThreads` for Kokoro. Mock ignores it |
| UI-006 | Generate | Done | Kokoro for CPU/CUDA, Mock for the silent choice |
| UI-007 | Play | Done | Last WAV. The Kokoro file was 16-bit PCM, 24000 Hz, mono |
| UI-008 | Stop | Partial | Stops playback and cancels the download token. Does not interrupt Kokoro mid-synthesis |
| UI-009 | Benchmark grid | Partial | Shows the rows loaded from the CSV |
| UI-010 | Errors | Partial | Exception text is shown in the status line |
| UI-011 | Settings | Partial | Capture region, preview, and monitor are on the Settings page. There is no general settings form |
| UI-012 | Model-folder picker | Not started | The locator searches known folders and downloads into LocalAppData |
| UI-013 | Audio device picker | Not started | Default output device |
| UI-014 | Global stop hotkey | Not started | |
| UI-015 | Theme | Partial | Dark theme is the app. There is no light theme |

## 7.3 Core

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| CORE-001 | `TtsBackend` | Done | Mock, Cpu, Cuda |
| CORE-002 | TTS options | Done | Backend and thread counts. Speed is a `GenerateAsync` argument |
| CORE-003 | `TtsResult` | Done | Bytes, content type, timings, engine name |
| CORE-004 | RTF | Done | Generation time / audio duration |
| CORE-005 | `BenchmarkRecord` | Done | |
| CORE-006 | `ITtsEngine` | Done | Initialize, Generate, Dispose |
| CORE-007 | `IOcrEngine` | Done | Implemented by `WindowsOcrEngine` |
| CORE-008 | `IAudioQueue` | Not started | Phase 4 |
| CORE-009 | Voice profile | Done | `NpcProfile`, `VoiceLibraryState`, `VoiceProfileStore` |
| CORE-010 | NPC identity | Done | Id, name, role, voice, pace, avatar, gender. Matching is the name plate, not a portrait detector |
| CORE-011 | Drop an old line | Partial | A newer `SpeakAsync` supersedes the previous request number. Kokoro is not cancelled mid-call |
| CORE-012 | Duplicates | Done | `DialogueTracker` and its tests |

## 7.4 TTS

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| TTS-001 | Use Kokoro 82M | Done | CPU speech was produced |
| TTS-002 | Mock engine | Done | Silent WAV for the pipeline |
| TTS-003 | `ITtsEngine` | Done | |
| TTS-004 | Pin KokoroSharp | Done | 0.8.4 for both packages. ONNX Runtime 1.22.0 |
| TTS-005 | Download the model | Done | First run saved 325,508,342 bytes to `%LOCALAPPDATA%\PaliaVoiceAI\models\kokoro.onnx` |
| TTS-006 | Load the model | Done | Second run used the local file and did not download again |
| TTS-007 | Real speech | Done | CPU WAV, then playback |
| TTS-008 | Try the English voices | Partial | `af_bella` spoke. The other ids in the combo have not been synthesized |
| TTS-009 | Listening review | Not started | Needs a person to listen |
| TTS-010 | Long text | Not started | |
| TTS-011 | Streaming | Not started | |
| TTS-012 | Cancel generation | Partial | Stop cancels the token around the call. The Kokoro call itself finishes |
| TTS-013 | Keep the model locally | Done | See TTS-006 |
| TTS-014 | Check the model hash before distribution | Not started | The downloaded file's SHA-256 was recorded. The downloader does not yet reject a mismatch. Hash: `0CFD5E79AAB70A3D8C1A57DC639835110DDB32C9F5FF4FDD1F4DB202EA43BB05` |
| TTS-015 | License review for redistribution | Not verified | KokoroSharp is MIT. The model and voices are Apache-2.0 according to the package notes. A distribution review is still open |

Model URL: `https://github.com/Lyrcaxis/KokoroSharpBinaries/releases/download/v2.0.0/kokoro.onnx`. The locator also accepts `kokoro.onnx` or `kokoro-v1.0.onnx` beside the exe, in the working directory, or in Downloads, and uses that file in place. It does not copy a 310MB file. Download uses user agent `PaliaVoiceAI`, a 30-minute timeout, a `.download` temp file removed on failure, and it rejects a file under 50MB.

Measured CPU line, voice `af_bella`, 3 threads, after the model was already present:

`Hello! Welcome to Kilima Village. How can I help you today?`

Generation 1,598 ms. Audio 4,510 ms. RTF 0.354. File 216,508 bytes. Peak sample 17,845. 16-bit PCM, 24000 Hz, mono. NAudio opened it and the Windows player played it. The cold run just after download was 2,040 ms, RTF 0.452.

## 7.5 ONNX and GPU

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| ONNX-001 | Use ONNX Runtime | Done | 1.22.0 through the CPU package |
| ONNX-002 | CPU package | Done | The measured sentence used it |
| ONNX-003 | GPU package | Not started | Do not add it until the versions are pinned |
| ONNX-004 | CUDA compatibility check | Done | The CPU DLL has no CUDA entry point. CUDA and cuDNN were not installed |
| ONNX-005 | CPU session | Done | Intra-op from the UI, inter-op 1, full optimization. It spoke |
| ONNX-006 | CUDA provider | Partial | Reflection finds the managed method. The native DLL lacks `OrtSessionOptionsAppendExecutionProvider_CUDA` |
| ONNX-007 | CPU fallback | Done | `ActiveBackend` becomes Cpu |
| ONNX-008 | GPU memory monitor | Not started | |
| ONNX-009 | Thread count is real | Partial | Kokoro uses intra-op threads. Mock does not |
| ONNX-010 | Session startup | Done | The CPU session started and spoke |
| ONNX-011 | GPU inference | Not verified | Not run on this PC |
| ONNX-012 | CPU inference | Done | See the measurement in section 7.4 |

## 7.6 Audio

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| AUD-001 | NAudio | Done | 2.2.1 |
| AUD-002 | `AudioPlayer` | Done | Played the Kokoro WAV from memory |
| AUD-003 | Play generated audio | Done | The real WAV, not only the silent mock |
| AUD-004 | Stop | Partial | Stop and dispose exist. Mid-synthesis Kokoro is not cut |
| AUD-005 | Choose a device | Not started | Default output |
| AUD-006 | Queue | Not started | Phase 4 |
| AUD-007 | No overlap | Not started | Phase 4 |
| AUD-008 | Crossfade | Deferred | Not a first-release requirement |
| AUD-009 | Volume | Not started | |
| AUD-010 | Long playback test | Not verified | |

## 7.7 Benchmark

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| BEN-001 | Record model | Done | |
| BEN-002 | RTF | Done | |
| BEN-003 | Save generation time | Partial | The CPU measurement above was recorded. There is no 20-sentence set |
| BEN-004 | CSV | Partial | `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv` is loaded on startup and rewritten. A bad header is moved to `.bak` |
| BEN-005 | CPU utilization | Not started | |
| BEN-006 | GPU utilization | Not started | |
| BEN-007 | VRAM | Not started | |
| BEN-008 | RAM | Not started | |
| BEN-009 | FPS | Not started | Needs an external capture or a trusted counter |
| BEN-010 | 1% low FPS | Not started | |
| BEN-011 | First-audio latency | Not started | Only full-file generation time exists. No streaming |
| BEN-012 | 20-sentence set | Not started | |
| BEN-013 | Compare 2/3/4 threads | Not started | Only 3 threads were measured |
| BEN-014 | Compare CPU and CUDA | Not started | CUDA cannot run on this package |
| BEN-015 | Benchmark while Palia runs | Not started | |
| BEN-016 | Final report | Not started | |

## 7.8 OCR and capture

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| OCR-001 | Windows OCR | Done | English. If it is missing: Settings, Time & language, Language & region, English optical character recognition |
| OCR-002 | Capture | Partial | A 24×24 GDI copy worked. Windows Graphics Capture is written and was not used on a live Palia window |
| OCR-003 | Find the Palia window | Partial | Rules are tested. A running process was not inspected |
| OCR-004 | Dialogue region | Partial | The Kenyatta suggestion matches the bottom-left card in that screenshot. It still needs a live pass |
| OCR-005 | Read text | Partial | Read "Kilima" from a drawn image, and the Kenyatta fixture after cleanup. Not from the live UI |
| OCR-006 | Clean text | Done | `TextNormalizer` and its tests |
| OCR-007 | Detect change | Done | Start, change, and end in `DialogueTracker` |
| OCR-008 | Skip the same text | Done | Duplicate window is unit-tested |
| OCR-009 | Unclear text | Partial | Empty text and text that is still changing are not accepted. Windows OCR does not give one confidence score |
| OCR-010 | OCR on the live game | Not verified | Needs Palia and a look at the capture |

Process names, in order: `PaliaClientSteam-Win64-Shipping`, `PaliaClient-Win64-Shipping`, the test builds, `PaliaClientSteam`, `Palia`, then an exact window title `Palia`. The client must be at least 640×480. `IsBorderRequired` is false when the OS build is 22000 or newer.

Speaker rules already in code:

- A plate is the name, or the name plus trailing number groups (`Kenyatta 0 0`).
- Matching stops at the first spoken sentence. A name inside a sentence is not the speaker.
- `Eshe` then `Skip` then the line matches Eshe. `Nai'o` matches id `naio`.
- Lines that start with `(`, `[`, or `chat with ` are prompts, not speech.
- Do not spell-correct `hoppu`.

## 7.9 NPC voices

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| NPC-001 | Voice profile model | Done | `NpcProfile` and `VoiceLibraryState` |
| NPC-002 | NPC list | Done | 29 villagers plus Unknown, from `npc-voices.json` |
| NPC-003 | Bind a voice id | Done | Default in the roster JSON. User change in `voices.json` |
| NPC-004 | Read the name from the screen | Partial | From the name plate in OCR text. Not tried inside the game |
| NPC-005 | Identification rules | Done | Plate only. A mentioned name does not match |
| NPC-006 | Unknown speaker | Done | Unknown card and `af_bella`, unless the user overrode it |
| NPC-007 | Per-character voice and pace | Partial | Voice edits are saved. Pace is sent to Kokoro. T1 has not compared two files |
| NPC-008 | Tests | Partial | Kenyatta, Eshe, and Nai'o unit tests, plus the roster test that Kenyatta stays `af_sky`. No live test |

## 7.10 Storage

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| DATA-001 | SQLite | Decided only | No package and no database. Do not add SQLite unless a task needs it |
| DATA-002 | Create the database | Not started | |
| DATA-003 | Store voice profiles | Partial | Roster JSON plus `voices.json`. Not SQLite |
| DATA-004 | Store settings | Partial | `capture.json`. No general settings service |
| DATA-005 | Audio cache | Not started | Phase 4 |
| DATA-006 | Cache key | Not started | Must include text, voice, pace, model hash, and engine version |
| DATA-007 | Cache cleanup | Not started | |
| DATA-008 | Schema migration | Not started | |

## 7.11 Distribution

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| DIST-001 | Design for later distribution | Decided only | No installer |
| DIST-002 | Self-contained publish | Not started | |
| DIST-003 | CPU installer | Not started | |
| DIST-004 | NVIDIA installer | Not started | |
| DIST-005 | Model download | Partial | First-run download exists. There is no manager window |
| DIST-006 | File hash check | Not started | See TTS-014 |
| DIST-007 | OS requirement check | Not started | |
| DIST-008 | Auto-update | Not started | |
| DIST-009 | Uninstaller | Not started | |
| DIST-010 | Third-party notices | Not started | Required before distribution |
| DIST-011 | Test on a second PC | Not started | |
| DIST-012 | Code signing | Deferred | Decide before any public release |

---

# 8. What the app does today

The window opens on VoiceBox. Each card is a villager. The right panel sets that card's Kokoro voice and pace, and the microphone previews it. Apply voice speaks a newly captured line with the matched card. Hear yourself only gates automatic playback; the microphone and Generate still play. Live capture toggles the monitor.

Voicestudio still has backend, voice, threads, the dialogue box, Generate, Play, and Stop. Settings has the capture region, preview, and monitor.

Files:

| File | Role |
| --- | --- |
| `%LOCALAPPDATA%\PaliaVoiceAI\models\kokoro.onnx` | Kokoro model |
| `%LOCALAPPDATA%\PaliaVoiceAI\voices.json` | User voice, pace, favorites, selected id, Apply voice, Hear yourself |
| `%LOCALAPPDATA%\PaliaVoiceAI\npc-voices.json` | Optional newer copy of the roster |
| `%LOCALAPPDATA%\PaliaVoiceAI\capture.json` | Region and monitor settings |
| `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv` | Benchmark rows |
| `%LOCALAPPDATA%\PaliaVoiceAI\last-capture.bmp` | Last capture |
| `npc-voices.json` beside the exe | Shipped roster |
| `Avatars\*.png` beside the exe | Wiki profile icons |

Limits that are still true:

1. CUDA does not run on this ONNX package.
2. Mock is silent and is not a quality sample.
3. Mock RTF is not a TTS measurement.
4. The model is downloaded into the user profile. It is not inside an installer.
5. Speaker matching is proven on text, not on a live Palia window.
6. There is no audio cache.
7. This build is not a distributable release.
8. Stop does not cut Kokoro mid-sentence.

---

# 9. Phases

## Phase 1 — Kokoro in the app

**Close when:** Kokoro speaks on CPU, CUDA is tried when the runtime can do it, and the numbers can be measured again.

**2026-11-17:** CPU is closed. The sentence, voice, timings, hash, and playback are in section 7.4. CUDA fell back to CPU. Listening quality was not judged. Impact while Palia is running was not measured.

| ID | Task | Status |
| --- | --- | --- |
| P1-001 | WPF solution | Done |
| P1-002 | Wire the window to Kokoro | Done |
| P1-003 | `ITtsEngine` | Done |
| P1-004 | Silent mock | Done |
| P1-005 | Pin KokoroSharp 0.8.4 | Done |
| P1-006 | Local `kokoro.onnx` | Done |
| P1-007 | `KokoroTtsEngine` | Done |
| P1-008 | CPU speech | Done |
| P1-009 | CUDA speech | Partial. Fallback only |
| P1-010 | One CPU measurement | Done |
| P1-011 | Listen and compare voices | Not started |
| P1-012 | Run beside Palia | Not started |

## Phase 2 — OCR and capture

**Close when:** A new dialogue line is captured once, and the same text is not sent to TTS again inside the duplicate window.

**2026-11-17:** Stability, cleanup, and duplicate logic are unit-tested. Windows OCR read drawn English text. A small GDI capture worked. The Palia window itself was not captured, so this phase is not closed.

## Phase 3 — NPC voice profiles

**Close when:** Known dialogue selects that villager's profile, and an unidentified speaker has an obvious fallback.

**2026-11-17:** The text side of that close is in place and unit-tested. Unknown is the fallback. The window can change the voice. Pace is wired and not compared (T1). Live selection is T2. Mapping-file voice suggestions were not applied.

Work items inside the phase:

- NPC model, profile, roster file, stable voice id, UI editing, and Unknown: done.
- Speed: wired, waiting on T1.
- Pitch: not supported by this Kokoro call. Do not fake it.

## Phase 4 — Queue and cache

- One speaker at a time.
- A clear rule for an old line when a new line arrives.
- Stop, and pause or resume only if a task asks for them.
- A stable cache key and a place to delete old files.

**Close when:** Speech does not overlap, and a stored line is not generated again when the text, voice, pace, and model match.

## Phase 5 — Settings and reliability

- The rest of the settings, logging, and a long session.
- CUDA failure must keep the CPU path. That fallback already exists.

## Phase 6 — Packaging

- CPU publish, optional NVIDIA runtime, model download, hash check, installer, license notices, a clean install on another PC, and uninstall.

---

# 10. Benchmark protocol

Use this when a task says to measure. Do not fill the result table with guesses.

## 10.1 Record every session

Machine model, CPU, GPU name and VRAM, RAM, Windows build, NVIDIA driver, KokoroSharp version, ONNX Runtime version, CUDA/cuDNN versions if used, model hash, game resolution and graphics settings, Windows and Armoury Crate power mode, whether the laptop is plugged in, and what else is running.

## 10.2 Scenes

| ID | Scene | Purpose |
| --- | --- | --- |
| BM-001 | Palia alone for 10 minutes | Baseline |
| BM-002 | TTS CPU, 2 threads, game closed | CPU cost |
| BM-003 | TTS CPU, 3 threads, game closed | CPU cost. One sentence at 3 threads is already recorded |
| BM-004 | TTS CPU, 4 threads, game closed | CPU cost |
| BM-005 | TTS CUDA, game closed | GPU cost. Blocked until a GPU build exists |
| BM-006 | Palia plus TTS CPU | Effect on the game |
| BM-007 | Palia plus TTS CUDA | Effect on the game |
| BM-008 | Palia plus OCR only | OCR cost |
| BM-009 | Palia plus OCR plus TTS | Full loop |
| BM-010 | 30–60 minutes of play | Stability and memory |

## 10.3 Text set

Create at least 20 English lines: very short, medium, long, punctuation, names and places, and a few numbers. Compare CPU and CUDA with the same lines, the same voice, and the same settings. That file does not exist yet.

## 10.4 Metrics

- **Generation time:** from the request to a finished WAV.
- **First-audio latency:** from the accepted dialogue line to the first playable audio. Without streaming, that is the full file time.
- **Audio duration:** length of the WAV.
- **RTF:** generation time / audio duration. Below 1 means generation is faster than playback.
- **Game:** average FPS, 1% low, frame-time consistency, GPU, VRAM, CPU, RAM.

## 10.5 Proposed gates

| Metric | Gate |
| --- | --- |
| Average FPS | 5% drop or less |
| 1% low | 10% drop or less |
| First audio | Under one second when possible |
| Overlapping audio | None |
| Lost lines in the test set | None |
| Crash in 30 minutes | None |
| Unbounded memory growth | None |

Do not mark a gate passed until that scene was run.

---

# 11. Dialogue path

```text
Screen capture
  -> OCR
  -> Text normalization
  -> Wait until the text is stable
  -> Drop a duplicate
  -> Name-plate match, else Unknown
  -> Voice and pace from the roster, then the user override
  -> Cache lookup          (not built)
  -> Kokoro if there was no cache hit
  -> Queue                 (not built)
  -> Play
```

Rules:

- Do not send every captured frame to TTS.
- Wait for the stable-read count before treating a line as new.
- Remove duplicates inside the duplicate window.
- A future cache key needs the normalized text, voice id, pace, model hash, output format, and engine version. The text alone is not enough.
- Do not use an LLM to guess the speaker when the name plate is missing.
- If the speaker is unclear, use Unknown. Do not assign a random voice.
- Keep the spoken line separate from the name plate and from UI prompts.

---

# 12. Safety

- Capture only what is on screen.
- Do not read the Palia process memory.
- Do not inject code or change game files.
- Do not try to bypass anti-cheat.
- Do not upload screenshots or dialogue in the local mode.
- Read Palia's terms before distributing the program or telling other people to use it with the game.
- Respect the TTS model, voice, and library licenses.
- Do not market these voices as the original actors.

---

# 13. Open risks

| ID | Risk | What to do |
| --- | --- | --- |
| R-001 | KokoroSharp and a future ONNX GPU build may not match | Pin both versions and speak one sentence before writing more GPU code |
| R-002 | CUDA and cuDNN mismatch | Install only the versions named for that ONNX build |
| R-003 | GPU speech hurts the game | Compare CPU and CUDA while Palia is running before enabling CUDA |
| R-004 | OCR reads UI that is not dialogue | Keep the region tight and the cleanup rules |
| R-005 | OCR drops part of a line | The stable-read count is the current protection. Add a merge only if a live capture shows a split line |
| R-006 | Wrong speaker | Trust the name plate. Unknown is the fallback |
| R-007 | A name is pronounced badly | Listening test. Do not spell-correct game text such as `hoppu` |
| R-008 | The installer is huge because of the model | Keep the on-demand download |
| R-009 | Other PCs differ | CPU must work. GPU stays optional |
| R-010 | Game terms | Review before distribution |
| R-011 | Redistribution rights | Finish TTS-015 and ship notices |
| R-012 | Long lines feel late | Queue and cache come after a listening check, not before |

Still undecided:

1. Which ONNX Runtime GPU package, if CUDA stays a goal. The CPU package is 1.22.0.
2. CUDA or DirectML. The current package uses CPU.
3. The live dialogue region. T3.
4. Whether Unknown should stay silent when Apply voice is on. Today it speaks with `af_bella`.
5. Final voice casting. Only `af_bella` has been heard. Do not change the JSON ids as part of a cleanup.
6. Cache size and eviction.
7. Installer and update mechanism.
8. Which FPS tool to trust.

---

# 14. First release

| Check | State |
| --- | --- |
| Starts on this Windows 11 PC | Done |
| Kokoro speaks locally | Done |
| CPU path works | Done |
| NVIDIA path works or falls back to CPU | Fallback done. GPU speech not done |
| OCR reads a live Palia card | Not done |
| The same line is not read again for no reason | Logic tested. Live use not done |
| Known dialogue selects the right profile | Text fixtures only |
| Each villager has a profile | Done. Six villagers have no voice id yet |
| Speech does not overlap | Not done |
| Stop cuts the current audio | Playback yes. Mid-synthesis no |
| Cache works | Not done |
| FPS gates | Not measured |
| Another PC can install it | Not done |
| Licenses reviewed for distribution | Not done |

---

# 15. Changelog

| Date | Update |
| --- | --- |
| 2026-11-17 | Project defined: local TTS for Palia NPC dialogue. C#, .NET 8, WPF, Kokoro 82M, ONNX Runtime. Reference PC recorded. CPU is the required path and CUDA is optional. |
| 2026-11-17 | Phase 1 starter: WPF, Core, silent Mock, NAudio. Release solution mapping fixed. Benchmark CSV is kept and reloaded. Stop cancels a download and stops playback. CPU and CUDA no longer fall through to Mock. |
| 2026-11-17 | Phase 2: capture, Windows OCR, cleanup, stability, and duplicates. 24 tests passed at that point. Live Palia OCR was not run. |
| 2026-11-17 | Kokoro CPU speech. First run downloaded the model. After that, 1,598 ms generation, 4,510 ms audio, RTF 0.354, voice `af_bella`. CUDA fell back to CPU. |
| 2026-11-17 | Phase 3 voice library. Name-plate matching tested on text. Then the roster moved to `npc-voices.json` with 28 wiki icons. Existing voice ids were kept. 29 tests passed. Taylin has no wiki icon. Portrait pixels were not reviewed by eye. |
| 2026-11-17 | This document was rewritten in English. Stale rows that called finished CPU speech, voice profiles, and playback "not started" were aligned with the checks above. Open work is T1, T2, T3, then T4. |

---

# 16. How to update this document

When a task is finished:

1. Change its status to Partial or Done.
2. Name the files or types that changed.
3. Name the check that was run.
4. Write the result: counts, timings, or the text that was read.
5. Add a changelog row with the date.
6. Leave the task Partial if the live game, a listening check, or a second PC was required and did not happen.

Do not write Done for a behavior that was not observed.
