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
dotnet publish .\src\PaliaVoiceAI.App\PaliaVoiceAI.App.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\win-x64 --nologo
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\installer\Install.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$env:LOCALAPPDATA\Programs\PaliaVoiceAI\Uninstall.ps1"
```

Last full check on 2026-11-17: 48 tests passed (Core 45, OCR 3). The CPU package published 526 files to `artifacts\win-x64`, including `coreclr.dll`, version 0.2.0.0. It installed for this user, the window title was `Palia Voice`, and uninstall removed the program, the Start menu shortcut, and the uninstall key. The Kokoro model stayed in the user profile. Another PC was not used.

---

# Current snapshot

| Phase | Close condition | State |
| --- | --- | --- |
| 1. Kokoro speech | Real local speech on CPU, then CUDA if it is available, with a repeatable measurement | CPU speech was measured again while the Palia process was running. CUDA still falls back to CPU. The user heard the two `af_bella` pace files and did not describe the sound. Other voices were not heard. FPS beside the game was not measured. |
| 2. OCR and capture | Read a new dialogue line once, and do not read the same line again | The live window was found and one Einar line was spoken once. The same line was not seen again inside the duplicate window, because the card advanced. |
| 3. NPC voices | Pick the right voice profile for known dialogue, and do something clear when the speaker is unknown | Einar's name plate selected Einar. Daiya, named only in the sentence, was not selected. Pace `0.85` is longer than `1.15` for `af_bella`. A card with no name plate has not been captured. |
| 4. Queue and cache | No overlapping speech, and no second generation when text, voice, and settings match | Checked in Core tests. A new line waits until the current line finishes playing. The same text, voice, pace, and model was not generated again. Stop drops a line that is still waiting. A live Kokoro sentence was not replayed from the cache, and Palia was not running. |
| 5. Settings and reliability | Settings, logging, and a long play session | Partial. Backend, threads, the Voicestudio voice, and volume restore from `settings.json`. The log stores an error type and a timing, and a spoken line was not written. A long play session was not run. |
| 6. Installer | Another Windows PC can install and remove it | Partial. This PC installed and removed the self-contained CPU package. Another PC was not used. The NVIDIA runtime was not packaged. |

**Do this next:** Open checks are listed in `PaliaVoiceAI-Open-Checks.md`. The largest ones are a second PC, a nameless dialogue card, the duplicate line, and a long play session. Do not start work that is not on that list unless the user asks.

---

# Next tasks

## T1 — Prove pace changes the audio

**Status:** Done. The same line and voice `af_bella` were generated while the Palia process was running. Pace `0.85` produced 5,084 ms of audio (244,096 bytes, generation 2,208 ms). Pace `1.15` produced 4,086 ms (196,158 bytes, generation 994 ms). The slower pace is the longer file. WAV header durations match those numbers. On 2026-11-17 the user said they heard both files. They did not describe which sounded longer or judge the voice.

**Done when:** The same line and the same voice are generated at pace `0.85` and `1.15`. The slower pace produces a longer WAV. Write both durations and the voice id in the Phase 3 status. Do not write that someone listened unless they did.

**Where:** `src/PaliaVoiceAI.TTS/KokoroTtsEngine.cs` (`Speed` only; there is no pitch control). The UI slider is `0.85`–`1.15`. The engine clamps to `0.7`–`1.4`.

**Kokoro has no pitch setting in this integration.** Do not add a fake pitch control. The Phase 3 line "speed and pitch if the engine supports them" is satisfied by speed. Pitch stays unsupported.

## T2 — Live speaker check

**Status:** Partial. Einar was selected from a live card. A card with no name plate has not been captured. On 2026-11-17 the user said to skip this check and start Phase 4.

**Done when:** A real dialogue card whose first plate is a known name selects that villager. A card with no name plate selects Unknown. A name that only appears inside a sentence does not select that villager. Paste the OCR text into the Phase 3 status.

**2026-11-17 live card.** Process `PaliaClientSteam-Win64-Shipping`, title `Palia`, client 1536×960 at (0, 0). Region `105,550` size `780×350` (the 1920×1200 capture frame is the same client at 125% scale). Raw OCR:

```
Einar I -
Sometimes I like to go watch the familial
unit known as Daiya eat dinner at night.
```

Speaker `einar`, voice `bm_lewis`. Daiya is only in the sentence and was not selected. Nobody has shown a card with no name plate.

**Where:** `SpeakerIdentifier.FindNpcId` in `src/PaliaVoiceAI.Core/Npc/NpcProfiles.cs`, called from `MainWindow.Capture.cs`.

## T3 — Live OCR close for Phase 2

**Status:** Partial. The window, region, and one spoken line are recorded. The duplicate suppression was not observed. On 2026-11-17 the user said to skip this check and start Phase 4.

**Done when:** The game window is found, the dialogue region contains the card, one new line is spoken or copied once, and the same line is suppressed inside the duplicate window. Record the process name, the region, and the text.

**2026-11-17 live read.** Same window and region as T2. The card image contained Einar's dialogue. The cleaned line was copied once and spoken once with Kokoro on CPU: generation 2,905 ms, audio 5,494 ms, 263,762 bytes, voice `bm_lewis`. The card then advanced to new lines, so the tracker reported changes and not a duplicate. Leave one line up, close it, and open that same line again within 8 seconds.

**Start from:** on a 1920×1200 frame, region `x=131, y=688, width=975, height=438`. That came from the Kenyatta screenshot (1024×576 crop at 70,330 size 520×210). Display scaling on this PC was 125% (a 1280×820 window was about 1600×1025 physical pixels). Re-check the region against the live window; do not assume the numbers still match.

**Expected cleaned line from that fixture, including the spelling:** `I'd wish you a hoppu Luna New Year, but I'm so ready for it to be over already.`

## T4 — Phase 4 queue

**Status:** Done. The user said to skip T2 and T3 and start this task. Core tests on 2026-11-17: two submitted lines produced `prepare, generate, play` for the first line before the second line was prepared. The second generate did not start while the first line was playing, and playback concurrency stayed at 1. The same text, voice `bm_lewis`, pace `1.00`, and model hash was delivered from the cache the second time (`FromCache` true, one generate). Pace `0.85` was generated again. Stop while the first line was playing cancelled that line and the waiting line, and the waiting line was not prepared. `DeleteOlderThan` removed one file dated two days earlier and left the newer file. `DeleteAll` removed the file that remained. A key containing `../` was not stored. Release build of the app succeeded with 0 warnings. Kokoro was not asked to speak a cached line in this check, and Palia was not running.

**Done when:** A new line cancels or waits on the previous one with no overlap, and a repeated line with the same voice, pace, and model is not generated again.

**Rule:** a new line waits. The current line finishes generating and playing before the next line starts. Stop drops every waiting line, stops playback, and does not play a result that comes back after Stop. Kokoro still cannot be cut mid-call. That call is not played if Stop already happened.

**Cache key:** trimmed text, voice id, pace rounded to two decimals and clamped to `0.70`–`1.40`, lowercase SHA-256 of the model file, engine version `KokoroSharp 0.8.4`, content type `audio/wav`. Thread count and CPU/CUDA are not part of the key. Files are `{key}.wav` in `%LOCALAPPDATA%\PaliaVoiceAI\cache`. Settings has **Delete stored lines**. `SpeechCache.DeleteOlderThan` is not run on a timer, because the cache size is still undecided. Mock lines use hash `mock` and version `mock-1`, so they do not share files with Kokoro.

## T5 — Phase 5 settings and log

**Status:** Partial. The user said to run Phase 5. Settings and the log were checked on 2026-11-17. A long play session was not run.

**Done when:** Backend, threads, the Voicestudio voice, and volume restore on launch. Errors and timings are logged without the dialogue line. A long play session has no crash and no unbounded memory growth.

**2026-11-17 check.** Core tests: a missing settings file is CPU, 3 threads, `af_bella`, volume 100. `Cuda`, 4 threads, `bm_lewis`, and volume 40 round-tripped. A broken file loads those starting values. The sentence `Hello! Welcome to Kilima Village. How can I help you today?` was not kept when it was offered as a voice id. The same sentence was passed to the log as the error text and the voice id. The file contained `error speech Exception` and `voice=unknown`, with the character count and no `Kilima`. A log capped at 700 bytes with 2 older files kept `session.log`, `session.1.log`, and `session.2.log`, each at or under 700 bytes. Writing to a log path that is a file did not throw. The app cap is 512 KB, with the same two older files. The window title was `Palia Voice`. After `WM_CLOSE`, `%LOCALAPPDATA%\PaliaVoiceAI\settings.json` was backend `Cuda`, threads 4, voice `bm_lewis`, and volume 40, and those were the only fields. `capture.json` and `voices.json` were unchanged. No log line was written on that launch because nothing failed. The volume was not listened to. A crash was not provoked, so the handler that keeps the window open was not watched. CUDA was not requested. Palia was not running.

## T6 — Phase 6 CPU package

**Status:** Partial. This PC installed and removed the package. Another PC was not used. The NVIDIA runtime was not packaged, because the CUDA and cuDNN versions are not pinned.

**Done when:** A self-contained CPU build installs and uninstalls, the model download rejects a bad hash, license notices ship beside the program, and another Windows PC can install and remove it.

**2026-11-17 check.** 48 tests passed (Core 45, OCR 3). A 3-byte download was deleted. A file with the wrong hash was deleted and was not moved. A matching hash was moved. The model already on disk is 325,508,342 bytes and its SHA-256 is `0CFD5E79AAB70A3D8C1A57DC639835110DDB32C9F5FF4FDD1F4DB202EA43BB05`, the same value pinned in `KokoroModelLocator.ExpectedSha256`. Publish wrote 526 files to `artifacts\win-x64`, with `coreclr.dll`, `onnxruntime.dll`, `THIRD-PARTY-NOTICES.txt`, and `LICENSE.txt`. File version `0.2.0.0`. Windows build on this PC is 26300. The installer requires 19041. Install went to `%LOCALAPPDATA%\Programs\PaliaVoiceAI`. The Start menu shortcut was `Palia Voice AI.lnk`. The uninstall key was `HKCU\Software\Microsoft\Windows\CurrentVersion\Uninstall\PaliaVoiceAI`. The installed exe opened with the title `Palia Voice`. `WM_CLOSE` closed it. Uninstall then removed the program folder, the shortcut, and the registry key. `%LOCALAPPDATA%\PaliaVoiceAI\models\kokoro.onnx` was still there.

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
7. Later, reuse a stored WAV when the line and the settings match. The cache key and the folder exist. A live Kokoro replay from that folder was not measured.

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
- A finished installer for another PC. This PC installed and removed the CPU package.
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
- One TTS generation at a time. The next line waits until the current line has finished playing.
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
| FR-010 | Audio queue | Order lines and do not overlap them | A new line waits. Core test: playback concurrency 1, and the second line started only after the first play finished |
| FR-011 | Stop now | Stop playback and cancel queued work | Stop drops waiting lines and stops playback. An in-flight Kokoro call still runs until the sentence ends, and that result is not played if Stop already happened |
| FR-012 | Audio cache | Reuse audio when text, voice, and settings match | Core test: the second identical line was `FromCache` and was not generated. Pace `0.85` was generated again. Not measured with Kokoro |
| FR-013 | Benchmark | Record generation time, audio length, RTF, and backend | One CPU row exists. The CSV is reloaded |
| FR-014 | Remember settings | Restore local settings on launch | Capture settings, voice overrides, the model path, and `settings.json` do this. The window restored backend `Cuda`, 4 threads, `bm_lewis`, and volume 40 |
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
| Concurrent TTS jobs | 1 | The queue does not start the next generation until the current line has finished playing. Kokoro itself is not interrupted mid-call |
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
| SET-010 | Logging | Done | `SessionLog` writes an error type and a timing under `%LOCALAPPDATA%\PaliaVoiceAI\logs`. A spoken line was not in the test file. The launch wrote no line because nothing failed |
| SET-011 | Unit tests | Done | 2026-11-17: Core 45, OCR 3, all passed |
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
| UI-008 | Stop | Partial | Stops playback and drops waiting lines. Does not interrupt Kokoro mid-synthesis. A result that finishes after Stop is not played |
| UI-009 | Benchmark grid | Partial | Shows the rows loaded from the CSV |
| UI-010 | Errors | Partial | Exception text is shown in the status line. The log stores the exception type. A thrown error was not used to watch the handler that keeps the window open |
| UI-011 | Settings | Partial | Capture region, preview, monitor, and playback volume are on the Settings page. Backend, threads, and the Voicestudio voice restore from `settings.json`. There is no audio-device picker |
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
| CORE-008 | Speech queue | Done | `SpeechQueue`. A new line waits. Tested with a stand-in player |
| CORE-009 | Voice profile | Done | `NpcProfile`, `VoiceLibraryState`, `VoiceProfileStore` |
| CORE-010 | NPC identity | Done | Id, name, role, voice, pace, avatar, gender. Matching is the name plate, not a portrait detector |
| CORE-011 | Drop an old line | Partial | A newer line waits. Stop drops waiting lines. Kokoro is not cancelled mid-call |
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
| TTS-009 | Listening review | Partial | The user heard `af_bella` at paces `0.85` and `1.15`. No other voice was heard, and no quality note was given |
| TTS-010 | Long text | Not started | |
| TTS-011 | Streaming | Not started | |
| TTS-012 | Cancel generation | Partial | Stop drops waiting lines and does not play a result that finishes after Stop. The Kokoro call itself finishes |
| TTS-013 | Keep the model locally | Done | See TTS-006 |
| TTS-014 | Check the model hash before distribution | Done | A short file and a wrong hash are deleted. The file on disk is 325,508,342 bytes and matches `0CFD5E79AAB70A3D8C1A57DC639835110DDB32C9F5FF4FDD1F4DB202EA43BB05`. It was not downloaded again |
| TTS-015 | License review for redistribution | Partial | `THIRD-PARTY-NOTICES.txt` shipped beside the installed exe. The package licenses that were read are recorded there. OpenTK's nuspec names no license. Palia's terms were not read |

Model URL: `https://github.com/Lyrcaxis/KokoroSharpBinaries/releases/download/v2.0.0/kokoro.onnx`. The locator also accepts `kokoro.onnx` or `kokoro-v1.0.onnx` beside the exe, in the working directory, or in Downloads, and uses that file in place. It does not copy a 310MB file. Download uses user agent `PaliaVoiceAI`, a 30-minute timeout, and a `.download` temp file removed on failure. `KokoroModelLocator.AcceptDownload` rejects a file under 50MB and a file whose SHA-256 is not `0cfd5e79aab70a3d8c1a57dc639835110ddb32c9f5ff4fdd1f4db202ea43bb05`. An existing file is not hashed again on every launch.

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
| AUD-004 | Stop | Partial | Stop drops the queue and stops playback. Mid-synthesis Kokoro is not cut |
| AUD-005 | Choose a device | Not started | Default output |
| AUD-006 | Queue | Done | `SpeechQueue` awaits one playback, then starts the next line |
| AUD-007 | No overlap | Done | Core test kept playback concurrency at 1. The real `AudioPlayer` was not the stand-in in that test |
| AUD-008 | Crossfade | Deferred | Not a first-release requirement |
| AUD-009 | Volume | Partial | `settings.json` stores 0–100. The window saved 40 and `AudioPlayer.Volume` is that percent divided by 100. It was not listened to |
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
| OCR-002 | Capture | Partial | A 24×24 GDI copy worked. Windows Graphics Capture read the live Palia window after the factory cast was fixed. The duplicate close is still open |
| OCR-003 | Find the Palia window | Done | `PaliaClientSteam-Win64-Shipping`, title `Palia`, client 1536×960 at (0, 0) |
| OCR-004 | Dialogue region | Partial | Suggested `105,550` `780×350` on that client. The crop contained Einar's card. The same line was not repeated |
| OCR-005 | Read text | Partial | Live OCR read Einar's line, then later lines on the same card. A nameless card was not read |
| OCR-006 | Clean text | Done | `TextNormalizer` and its tests |
| OCR-007 | Detect change | Done | Start, change, and end in `DialogueTracker` |
| OCR-008 | Skip the same text | Done | Duplicate window is unit-tested |
| OCR-009 | Unclear text | Partial | Empty text and text that is still changing are not accepted. Windows OCR does not give one confidence score |
| OCR-010 | OCR on the live game | Partial | Einar's card was captured and read. Duplicate suppression was not observed |

Process names, in order: `PaliaClientSteam-Win64-Shipping`, `PaliaClient-Win64-Shipping`, the test builds, `PaliaClientSteam`, `Palia`, then an exact window title `Palia`. The client must be at least 640×480. `IsBorderRequired` is false when the OS build is 22000 or newer.

Speaker rules already in code:

- A plate is the name, or the name plus trailing badge noise (`Kenyatta 0 0`, `Einar I -`, `Einar • VI -`). A real word after the name is not noise.
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
| NPC-004 | Read the name from the screen | Partial | Live OCR `Einar I -` selected `einar`. A card with no name plate has not been tried |
| NPC-005 | Identification rules | Done | Plate only. A mentioned name does not match |
| NPC-006 | Unknown speaker | Done | Unknown card and `af_bella`, unless the user overrode it |
| NPC-007 | Per-character voice and pace | Partial | Voice edits are saved. The user heard `af_bella` at `0.85` and `1.15`. They gave no quality note |
| NPC-008 | Tests | Partial | Kenyatta, Eshe, Nai'o, and the Einar badge lines. Live test: Einar's card selected `einar`. No nameless card yet |

## 7.10 Storage

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| DATA-001 | SQLite | Decided only | No package and no database. Do not add SQLite unless a task needs it |
| DATA-002 | Create the database | Not started | |
| DATA-003 | Store voice profiles | Partial | Roster JSON plus `voices.json`. Not SQLite |
| DATA-004 | Store settings | Done | `capture.json` and `settings.json`. The window restored backend `Cuda`, 4 threads, `bm_lewis`, and volume 40 |
| DATA-005 | Audio cache | Done | `%LOCALAPPDATA%\PaliaVoiceAI\cache\{key}.wav`. A repeated test line was not generated again |
| DATA-006 | Cache key | Done | Text, voice, pace, model SHA-256, engine version, and `audio/wav` |
| DATA-007 | Cache cleanup | Partial | `DeleteOlderThan` and `DeleteAll` were tested. Settings deletes every stored line. Nothing deletes files on a timer |
| DATA-008 | Schema migration | Not started | |

## 7.11 Distribution

| ID | Task | Status | Detail |
| --- | --- | --- | --- |
| DIST-001 | Design for later distribution | Done | Per-user CPU folder under `%LOCALAPPDATA%\Programs\PaliaVoiceAI`. No admin install. User data stays in `%LOCALAPPDATA%\PaliaVoiceAI` |
| DIST-002 | Self-contained publish | Done | `dotnet publish` `-c Release -r win-x64 --self-contained true` wrote 526 files to `artifacts\win-x64`. `coreclr.dll` is in that folder. Version 0.2.0.0 |
| DIST-003 | CPU installer | Done | `installer\Install.ps1` copied that folder, added the Start menu shortcut, and wrote the HKCU uninstall key. Windows build 26300 passed the 19041 check |
| DIST-004 | NVIDIA installer | Not started | CUDA and cuDNN versions are not pinned. Do not add a GPU package until they are |
| DIST-005 | Model download | Partial | First-run download exists and now rejects a bad hash. There is no manager window |
| DIST-006 | File hash check | Done | See TTS-014 |
| DIST-007 | OS requirement check | Done | The installer stops below Windows build 19041. This PC is build 26300 |
| DIST-008 | Auto-update | Not started | |
| DIST-009 | Uninstaller | Done | `installer\Uninstall.ps1` removed the program folder, the shortcut, and the registry key. The model file remained |
| DIST-010 | Third-party notices | Partial | `THIRD-PARTY-NOTICES.txt` and `LICENSE.txt` were beside the installed exe. OpenTK's nuspec names no license |
| DIST-011 | Test on a second PC | Not started | |
| DIST-012 | Code signing | Deferred | Decide before any public release |

---

# 8. What the app does today

The window opens on VoiceBox. Each card is a villager. The right panel sets that card's Kokoro voice and pace, and the microphone previews it. Apply voice speaks a newly captured line with the matched card. Hear yourself only gates automatic playback; the microphone and Generate still play. Live capture toggles the monitor.

Voicestudio still has backend, voice, threads, the dialogue box, Generate, Play, and Stop. Those three choices are restored from `settings.json`. Settings has the capture region, preview, monitor, playback volume, and **Delete stored lines**. A new line waits until the current one finishes. Stop drops every line that is still waiting. An OCR or speech failure stays on the status line. A dispatcher failure is logged by type and marked handled so the window stays open. That handler was not watched with a thrown error.

Files:

| File | Role |
| --- | --- |
| `%LOCALAPPDATA%\PaliaVoiceAI\models\kokoro.onnx` | Kokoro model |
| `%LOCALAPPDATA%\PaliaVoiceAI\voices.json` | User voice, pace, favorites, selected id, Apply voice, Hear yourself |
| `%LOCALAPPDATA%\PaliaVoiceAI\npc-voices.json` | Optional newer copy of the roster |
| `%LOCALAPPDATA%\PaliaVoiceAI\capture.json` | Region and monitor settings |
| `%LOCALAPPDATA%\PaliaVoiceAI\settings.json` | Backend, CPU threads, Voicestudio voice, playback volume |
| `%LOCALAPPDATA%\PaliaVoiceAI\logs\session.log` | Error types and timings. Older copies are `session.1.log` and `session.2.log`. Dialogue text is not written |
| `%LOCALAPPDATA%\PaliaVoiceAI\cache\*.wav` | Stored lines. The file name is the cache key |
| `%LOCALAPPDATA%\PaliaVoiceAI\benchmarks.csv` | Benchmark rows |
| `%LOCALAPPDATA%\PaliaVoiceAI\last-capture.bmp` | Last capture |
| `npc-voices.json` beside the exe | Shipped roster |
| `Avatars\*.png` beside the exe | Wiki profile icons |
| `THIRD-PARTY-NOTICES.txt` and `LICENSE.txt` beside the exe | License notices |
| `installer\Install.ps1` and `installer\Uninstall.ps1` | Per-user install and removal |

Limits that are still true:

1. CUDA does not run on this ONNX package.
2. Mock is silent and is not a quality sample.
3. Mock RTF is not a TTS measurement.
4. The model is downloaded into the user profile. It is not inside the installer.
5. Speaker matching is proven on text, not on a live Palia window.
6. A repeated line is stored in the cache. That reuse was tested with stand-in audio, not with a live Kokoro sentence.
7. The CPU package installs and removes on this PC. Another PC has not installed it, and the package is not signed.
8. Stop drops waiting lines and stops playback. It does not cut Kokoro mid-sentence. The finished call is not played if Stop already happened.
9. Playback volume is saved. It was not compared by ear.
10. A long play session has not been run. The log cap was tested at 700 bytes, not by filling the 512 KB app cap.

---

# 9. Phases

## Phase 1 — Kokoro in the app

**Close when:** Kokoro speaks on CPU, CUDA is tried when the runtime can do it, and the numbers can be measured again.

**2026-11-17:** CPU is closed. The first sentence, voice, timings, hash, and playback are in section 7.4. A second CPU measurement ran while `PaliaClientSteam-Win64-Shipping` was running: voice `af_bella`, pace `0.85` audio 5,084 ms, pace `1.15` audio 4,086 ms. CUDA was requested again and fell back to CPU because `onnxruntime` has no `OrtSessionOptionsAppendExecutionProvider_CUDA`. The user heard the two `af_bella` pace files and did not describe them. Other voices were not heard. FPS was not measured. A reconnect notice was on the dialogue card during that run.

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
| P1-009 | CUDA speech | Partial. Fallback only. The missing entry point was recorded again on 2026-11-17 |
| P1-010 | One CPU measurement | Done. Repeated at two paces while Palia's process was running |
| P1-011 | Listen and compare voices | Partial. The user heard the two `af_bella` pace files. Other English voices were not compared by ear |
| P1-012 | Run beside Palia | Partial. Generation ran while the game process was open. FPS was not measured |

## Phase 2 — OCR and capture

**Close when:** A new dialogue line is captured once, and the same text is not sent to TTS again inside the duplicate window.

**2026-11-17:** Stability, cleanup, and duplicate logic are unit-tested. Windows OCR read drawn English text. A small GDI capture worked. The live Palia window was captured later the same day: process `PaliaClientSteam-Win64-Shipping`, client 1536×960, region `105,550` `780×350`, and one Einar line was spoken once. The same line did not return inside the duplicate window, so this phase is not closed. Window capture had to ask combase for the graphics-capture factory. The CsWinRT wrapper could not be cast to that COM interface.

## Phase 3 — NPC voice profiles

**Close when:** Known dialogue selects that villager's profile, and an unidentified speaker has an obvious fallback.

**2026-11-17:** The text side of that close is in place and unit-tested. Unknown is the fallback. The window can change the voice. Pace was compared (T1): `af_bella` at `0.85` is 5,084 ms and at `1.15` is 4,086 ms. Live selection: Einar's plate selected `einar` (see T2). A card with no name plate has not been captured. Mapping-file voice suggestions were not applied.

Work items inside the phase:

- NPC model, profile, roster file, stable voice id, UI editing, and Unknown: done.
- Speed: compared. Voice `af_bella`, line `Hello! Welcome to Kilima Village. How can I help you today?` Pace `0.85` was 5,084 ms. Pace `1.15` was 4,086 ms. Files are `%LOCALAPPDATA%\PaliaVoiceAI\phase1\bella-pace-085.wav` and `bella-pace-115.wav`. The user heard both on 2026-11-17 and did not describe the sound.
- Pitch: not supported by this Kokoro call. Do not fake it.

## Phase 4 — Queue and cache

- One speaker at a time.
- A new line waits. Stop drops every line that is still waiting. Pause and resume were not added.
- Cache key: trimmed text, voice id, pace to two decimals, model SHA-256, engine version `KokoroSharp 0.8.4`, content type `audio/wav`.
- Stored files are `%LOCALAPPDATA%\PaliaVoiceAI\cache\{key}.wav`. Settings deletes them. `DeleteOlderThan` is available and is not on a timer.

**Close when:** Speech does not overlap, and a stored line is not generated again when the text, voice, pace, and model match.

**2026-11-17:** The user skipped the remaining live checks and said to start this phase. Core tests observed the close: the second line did not generate or play until the first playback finished, playback concurrency stayed at 1, and the same text, voice `bm_lewis`, pace `1.00`, and model hash was `FromCache` on the second delivery. Pace `0.85` generated again. Stop cancelled the playing line and the waiting line. Kokoro was not asked to replay a stored line, and Palia was not running. Types: `SpeechQueue`, `SpeechCache`, `SpeechIdentity`, `SpeechCacheKey`, `ModelFileHash`, `AudioPlayer.PlayAsync`.

## Phase 5 — Settings and reliability

- The rest of the settings, logging, and a long session.
- CUDA failure must keep the CPU path. That fallback already exists. It was not requested again on 2026-11-17.

**Close when:** Settings restore on launch, errors and timings are logged without the dialogue line, and a long play session has no crash and no unbounded memory growth.

**2026-11-17:** The user said to run this phase. `AppSettingsStore` and `SessionLog` are in Core. The window restored backend `Cuda`, 4 threads, voice `bm_lewis`, and volume 40 from `settings.json`. The log test omitted `Hello! Welcome to Kilima Village. How can I help you today?` and rolled a 700-byte file over to two older files. See T5. The long play session was not run, so this phase is not closed. Types: `AppSettings`, `AppSettingsStore`, `SessionLog`, `SpeechTiming`. The volume slider is on the Settings page. `AudioPlayer.Volume` takes 0–1.

## Phase 6 — Packaging

- CPU publish, optional NVIDIA runtime, model download, hash check, installer, license notices, a clean install on another PC, and uninstall.

**Close when:** Another Windows PC can install the CPU package and remove it.

**2026-11-17:** The user said to complete this phase. The CPU publish, hash check, notices, installer, and uninstaller were checked on this PC. See T6. Types: `KokoroModelLocator.AcceptDownload`. Scripts: `installer\Install.ps1` and `installer\Uninstall.ps1`. The NVIDIA runtime was not packaged. Another PC was not used, so this phase is not closed. The remaining checks are in `PaliaVoiceAI-Open-Checks.md`.

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
  -> Cache lookup
  -> Kokoro if there was no cache hit
  -> Wait until the current line has finished playing
  -> Play
```

Rules:

- Do not send every captured frame to TTS.
- Wait for the stable-read count before treating a line as new.
- Remove duplicates inside the duplicate window.
- The cache key is the trimmed text, voice id, pace, model hash, engine version, and `audio/wav`. The text alone is not enough.
- A new line waits. It does not start until the current line has finished playing.
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
| R-012 | Long lines feel late | The queue waits, so a fast conversation can lag behind the card. The cache skips a repeated line. Neither was measured on a long Kokoro line |

Still undecided:

1. Which ONNX Runtime GPU package, if CUDA stays a goal. The CPU package is 1.22.0.
2. CUDA or DirectML. The current package uses CPU.
3. The live dialogue region. T3.
4. Whether Unknown should stay silent when Apply voice is on. Today it speaks with `af_bella`.
5. Final voice casting. Only `af_bella` has been heard. Do not change the JSON ids as part of a cleanup.
6. Cache size and eviction.
7. Update mechanism. The CPU installer is the per-user folder in T6.
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
| Speech does not overlap | Queue tested. A new line waits. Not heard against live Kokoro |
| Stop cuts the current audio | Playback yes. Waiting lines are dropped. Mid-synthesis Kokoro still finishes and is not played if Stop already happened |
| Cache works | Tested with stand-in audio. A live Kokoro replay was not measured |
| FPS gates | Not measured |
| Another PC can install it | This PC installed and removed the CPU package. Another PC has not |
| Licenses reviewed for distribution | Notices shipped. OpenTK's nuspec names no license. Palia's terms were not read |

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
| 2026-11-17 | Live Palia capture. Window `PaliaClientSteam-Win64-Shipping`, region `105,550` `780×350`. Einar's line was spoken once with `bm_lewis` (2,905 ms generation, 5,494 ms audio). The duplicate and a nameless card were not captured. Name plates now ignore friendship-badge OCR. 32 tests passed. |
| 2026-11-17 | Pace check. `af_bella` at `0.85` was 5,084 ms and at `1.15` was 4,086 ms. CUDA fell back to CPU again. The Palia process was running. Listening and FPS were not confirmed. |
| 2026-11-17 | The user heard the two `af_bella` pace files. They did not describe the sound. Other voices and FPS remain unconfirmed. |
| 2026-11-17 | The user cannot perform the remaining in-game checks. The duplicate line and a nameless card stay open. |
| 2026-11-17 | The user said to skip T2 and T3 and start Phase 4. `SpeechQueue` waits. The same text, voice, pace, and model was not generated again. Stop drops a waiting line. Cache files are in `%LOCALAPPDATA%\PaliaVoiceAI\cache`. 40 tests passed (Core 37, OCR 3). Release build had 0 warnings. Kokoro cache replay and Palia were not part of this check. |
| 2026-11-17 | The user said to run Phase 5. `settings.json` restored backend `Cuda`, 4 threads, `bm_lewis`, and volume 40 after the window closed. The log omitted the spoken line and rolled over at a 700-byte test cap. 46 tests passed (Core 43, OCR 3). Release build had 0 warnings. A long play session, a provoked crash, and a volume listening check were not run. |
| 2026-11-17 | The user said to complete Phase 6. The self-contained CPU package installed and uninstalled on this PC. The model hash matched the pinned SHA-256, and a wrong hash was rejected in a test. 48 tests passed (Core 45, OCR 3). Another PC, the NVIDIA runtime, and a signed package were not part of this check. |

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
