# Palia Voice AI — Open checks

**Date:** 2026-11-17

What was already checked stays in `PaliaVoiceAI-Project-Master.md`. This file is only the work that is still open. Do not mark a row done here until the check named on that row has been seen.

## Needs Palia on screen

| ID | Still open | Done when |
| --- | --- | --- |
| T2 | Einar's plate selected `einar`. A card with no name is not spoken. A live nameless card has not been captured. | A live card with no name plate is ignored and is not spoken. Paste the OCR text into the master. |
| T3 | The same line was not seen again inside the 8 second duplicate window, because the Einar card advanced. | Leave one line up, close it, and open that same line again within 8 seconds. Record the process name, region, and text. |
| OCR-004 | The live region `105,550` `780×350` contained Einar's card. It has not been checked against a later session. | The crop still contains the whole card on the window that is actually open. |
| BM-001 | Palia alone for 10 minutes was not measured. | Average FPS and 1% low are written down for that scene. |
| BM-006 | Palia plus CPU speech was not measured. Generation once ran while the process was open. FPS was not. | FPS and 1% low are compared with BM-001. |
| BM-008 | OCR cost beside the game was not measured. | The scene is timed and the numbers are written down. |
| BM-009 | The full capture, OCR, and speech loop was not measured beside the game. | The scene is timed and the numbers are written down. |
| BM-010 | A 30–60 minute play session was not run. | No crash, and memory does not keep growing. This also closes the rest of T5. |
| NFR-001 | The 5% FPS gate was not run. | The gate is measured, or the miss is written with the numbers. |
| NFR-002 | The 10% 1% low gate was not run. | The gate is measured, or the miss is written with the numbers. |

## Needs someone to listen

| ID | Still open | Done when |
| --- | --- | --- |
| T1 note | The user heard `af_bella` at paces `0.85` and `1.15` and did not describe the sound. | A note says whether the slower pace sounds longer, and whether the voice is acceptable. |
| TTS-008 | Only `af_bella` has been synthesized. | The other English voice ids in the combo have each produced a WAV, or the ones that fail are named. |
| TTS-009 | No quality note exists for any voice except that the two Bella pace files were heard. | The listening note names the voice ids and what was wrong or acceptable. |
| AUD-009 | Volume 40 was saved. It was not heard. | The same line at two volumes is heard, and the lower setting is quieter. |
| AUD-010 | A long playback was not left running. | Playback of several stored lines finishes without a hang. |
| R-007 | Names may be pronounced badly. | A listening note names the words. Do not spell-correct game text such as `hoppu`. |

## Needs another Windows PC

| ID | Still open | Done when |
| --- | --- | --- |
| DIST-011 | The CPU package was installed and removed only on the reference PC. | A second Windows 10 or 11 x64 PC, build 19041 or newer, installs with `installer\Install.ps1`, opens the window, and removes it with `Uninstall.ps1`. The model downloads on first speech or is already present. |

## Packaging still open

| ID | Still open | Done when |
| --- | --- | --- |
| DIST-004 | No NVIDIA package. The CPU ONNX Runtime build has no CUDA entry point. CUDA and cuDNN versions are not pinned. | The versions are written in the master before any GPU package is added. Then one sentence speaks on that GPU build, or the fallback to CPU is recorded again. |
| DIST-005 | There is no window that shows the model download separately from the first speech. | A download failure is visible without digging through the status line, and a bad hash is still rejected. |
| DIST-008 | There is no updater. | An update path is chosen and checked, or the decision is to ship without one. |
| DIST-010 | `THIRD-PARTY-NOTICES.txt` shipped. OpenTK.Audio.OpenAL 5.0.0-pre.13 does not name a license in its nuspec. | The OpenTK license is read from the project that published that package and added to the notice. |
| DIST-012 | The package is not signed. | A signing decision is written down before any public release. |
| TTS-015 | Palia's terms were not read. | The terms are read before the program is given to anyone else to use with the game. |
| R-010 | Game terms are still unread. | Same check as TTS-015. |

## Not built

| ID | Still open | Done when |
| --- | --- | --- |
| UI-012 | There is no model-folder picker. The locator searches known folders and downloads into LocalAppData. | The user can point at a `kokoro.onnx` and that file is the one that speaks. |
| UI-013 | There is no audio-device picker. Playback uses the default device. | The chosen device is the one that plays, and the choice is still selected after a restart. |
| UI-014 | There is no global stop hotkey. | A chosen key stops playback while the game window is in front. |
| UI-015 | There is no light theme. | A light theme can be selected and is still selected after a restart, or the decision is that dark stays the only theme. |
| TTS-010 | A long line has not been generated. | One long line finishes, and the time and audio length are written down. |
| TTS-011 | Speech is not streamed. First audio is the full file. | The first audible sample starts before the whole file is finished, or the decision is to keep full-file playback. |
| ONNX-003 | No GPU package. | Same gate as DIST-004. |
| ONNX-008 | GPU memory is not monitored. | A CUDA run records VRAM, after a GPU build exists. |
| ONNX-011 | GPU inference has not run on this PC. | One CUDA sentence is spoken, or the missing entry point is recorded again after a GPU package is added. |
| BEN-005 | CPU use during speech was not sampled. | One generate records CPU percent. |
| BEN-008 | RAM during speech was not sampled. | One generate records working set before and after. |
| BEN-012 | The 20-line English set does not exist. | The file exists and contains short lines, medium lines, long lines, punctuation, names, places, and numbers. |
| BEN-013 | Only 3 threads were measured for the first sentence. Pace checks used the same engine path and were not a 2/3/4 comparison. | The same line is generated at 2, 3, and 4 threads, and the three times are written down. |
| SET-012 | There is no CI build. | A push builds the solution and runs the tests. |
| DATA-001 | SQLite was only an idea. JSON files are the store. | Leave this until a task needs a database. |
| AUD-008 | Crossfade was deferred. | Leave it out of the first release. |

## Already coded, and the live half is still open

| ID | What the code does | What has not been seen |
| --- | --- | --- |
| UI-008 | Stop drops waiting lines and stops playback. Kokoro is not cut mid-call. | A live Kokoro sentence that finishes after Stop is not played. The queue test used a stand-in. |
| UI-010 | A dispatcher failure is marked handled. | A thrown error in the running window, and the window staying open. |
| CORE-011 | A newer line waits. | The same wait with a live Kokoro sentence. |
| TTS-012 | The result after Stop is dropped. | The Kokoro call itself still runs to the end. That was not timed on a long line. |
| P1-009 | CUDA falls back to CPU. | A GPU package that actually speaks. |
| P1-012 | Generation ran while `PaliaClientSteam-Win64-Shipping` was open. | FPS beside the game. |
| NPC-007 | Voice and pace edits are saved. | A listening note for a villager other than the Bella pace files. |
| DATA-007 | Settings deletes every stored line. `DeleteOlderThan` works and is not on a timer. | A cache-size decision, and whether old files should be deleted on a timer. |
| R-012 | The queue waits, so a fast conversation can lag. The cache skips a repeated line. | Either behavior measured on a long Kokoro line. |
