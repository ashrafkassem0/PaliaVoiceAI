# Palia Voice AI — NPC voices

`src/PaliaVoiceAI.Core/Npc/npc-voices.json` is the runtime source for character metadata and voice ids. This file describes that roster. It is not a second assignment list.

Kokoro does not provide pitch control. `pitchDirection` is descriptive only. Every configured `speed` is `1.0`. A first-pass note suggested other starting speeds; those numbers were not copied into the JSON.

No assignment below is listening-approved. `af_bella` has been heard as a voice sample, not as an approved character cast.

## Status words

| Status | Meaning |
| --- | --- |
| Provisional | `voiceId` is in the JSON and is a Kokoro voice in this app. It has not been approved by listening. |
| Unassigned | `voiceId` is empty on purpose. The app must not fill it with `af_bella` or any other default. |
| Confirmed | Not used. Nothing in this roster is confirmed. |

`af_aoede` is a real Kokoro voice in this build (`voices/af_aoede.npy`) and is in the voice list. No character uses it. The first-pass note that proposed it for Elouisa was not applied.

## Roster

Unknown is a fallback card, not one of the 29 villagers. A captured line with no readable name is not spoken.

| NPC | Id | Gender | Age group | Species | Personality | Voice ID | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Ashura | ashura | Male | Adult | Majiri | Dependable, experienced, calm, and warm | `am_michael` | Provisional |
| Auni | auni | Male | Teen | Majiri | Energetic, curious, optimistic, mischievous | — | Unassigned |
| Badruu | badruu | Male | Adult | Majiri | Sociable, playful, pun-loving, affectionate | `am_puck` | Provisional |
| Caleri | caleri | Female | Adult | Majiri | Intellectual, disciplined, precise, serious | `af_kore` | Provisional |
| Chayne | chayne | Male | Adult | Majiri | Patient, compassionate, reflective, welcoming | `am_adam` | Provisional |
| Delaila | delaila | Female | Adult | Majiri | Practical, caring, confident, direct | `bf_lily` | Provisional |
| Einar | einar | Male | Unknown | Galdur | Literal, thoughtful, gentle, deliberate | `bm_lewis` | Provisional |
| Elouisa | elouisa | Female | Adult | Majiri | Imaginative, excitable, curious, eccentric | `af_nova` | Provisional |
| Eshe | eshe | Female | Adult | Majiri | Formal, authoritative, disciplined, demanding | `bf_emma` | Provisional |
| Eshelon | eshelon | Unknown | Unknown | Unknown | Not established | — | Unassigned |
| Hassian | hassian | Male | Young adult | Majiri | Reserved, guarded, serious, independent | `am_fenrir` | Provisional |
| Hekla | hekla | Female | Unknown | Galdur | Patient, dutiful, attentive, protective | `bf_isabella` | Provisional |
| Hodari | hodari | Male | Adult | Majiri | Responsible, protective, practical, steady | `bm_daniel` | Provisional |
| Jel | jel | Male | Young adult | Majiri | Artistic, expressive, refined, sensitive | `bm_george` | Provisional |
| Jina | jina | Female | Young adult | Majiri | Curious, thoughtful, sincere, slightly hesitant | `af_heart` | Provisional |
| Kenli | kenli | Male | Adult | Majiri | Polite, well-meaning, somewhat hesitant | `am_onyx` | Provisional |
| Kenyatta | kenyatta | Female | Young adult | Majiri | Independent, energetic, confident, direct | `af_sky` | Provisional |
| Nai'o | naio | Male | Young adult | Majiri | Hardworking, kind, earnest, friendly | `am_eric` | Provisional |
| Najuma | najuma | Female | Child | Majiri | Shy but brave, inventive, determined | `af_jessica` | Provisional |
| Reth | reth | Male | Young adult | Majiri | Sociable, quick-witted, playful, sometimes evasive | `am_echo` | Provisional |
| Sifuu | sifuu | Female | Adult | Majiri | Strong, confident, direct, energetic | `af_nicole` | Provisional |
| Subira | subira | Female | Adult | Majiri | Observant, disciplined, formal, cautious | `af_bella` | Provisional |
| Tamala | tamala | Female | Adult | Majiri | Enigmatic, self-assured, composed | `bf_alice` | Provisional |
| Tau | tau | Unknown | Unknown | Plumehound | Loyal animal companion, not a speaking villager | — | Unassigned |
| Taylin | taylin | Unknown | Unknown | Unknown | Not established | — | Unassigned |
| The Gardener | gardener | Unknown | Unknown | Galdur | Ancient, controlled, formal or detached | — | Unassigned |
| Tish | tish | Female | Young adult | Majiri | Kind, optimistic, gentle, encouraging | `af_sarah` | Provisional |
| Ulfe | ulfe | Unknown | Unknown | Unknown | Not established | — | Unassigned |
| Zeki | zeki | Male | Adult | Grimalkin | Clever, shrewd, witty, sometimes sly | `am_liam` | Provisional |

Age notes that are not exact:

- Ashura is described as an older adult. Exact age is not confirmed, so the stored band is Adult, not Elder.
- Najuma is a child or young teen. The cited Almanac line calls her a young girl. Exact age is not a number. Kokoro has no child voice, so her voice confidence is Low.
- Einar and Hekla are ageless Galdur constructs. Their age band is Unknown, not a human adult or elder band. The stored gender is the presentation already on the roster.
- The Gardener is an ageless Galdur. Pronouns were not verified, so gender stays Unknown.
- Tau is a Plumehound. References call the character male-coded, but pronouns were not verified here.
- Jel is young adult or adult. The stored band is Young adult.
- Chayne's stored role remains Apothecary. The first-pass note called him a priest and spiritual guide; that role label was not changed.

`pitchDirection` for every configured voice is descriptive only. The JSON text says Kokoro has no pitch control.

## Voices in this app

The profile list can also select Unassigned. The studio list cannot.

American women: `af_aoede`, `af_bella`, `af_heart`, `af_jessica`, `af_kore`, `af_nicole`, `af_nova`, `af_sarah`, `af_sky`.

British women: `bf_alice`, `bf_emma`, `bf_isabella`, `bf_lily`.

American men: `am_adam`, `am_echo`, `am_eric`, `am_fenrir`, `am_liam`, `am_michael`, `am_onyx`, `am_puck`.

British men: `bm_daniel`, `bm_george`, `bm_lewis`.

These ids exist as Kokoro files but are not in the app list, and no character uses them: `af_alloy`, `af_river`, `am_santa`, `bm_fable`. They were not added just to mirror a catalogue.

## Unassigned on purpose

| NPC | Why there is no voice |
| --- | --- |
| Auni | Identity is documented. The first-pass note proposed `am_puck` and it was not applied. |
| Eshelon | Gender, species, age, and spoken lines were not established. |
| Tau | Plumehound companion. Do not synthesize human speech unless the game gives him spoken lines. |
| Taylin | Gender, species, age, and a voice were not established. No profile image is shipped. |
| The Gardener | Galdur identity is documented. Pronouns and a voice were not approved. |
| Ulfe | Gender, species, age, and a voice were not established. |

## First-pass proposals that are not configured

These ids appeared in an earlier draft. They are not the runtime voice.

| NPC | Configured | Earlier proposal, not configured |
| --- | --- | --- |
| Auni | — | `am_puck` |
| Badruu | `am_puck` | `am_fenrir` |
| Caleri | `af_kore` | `bf_emma` |
| Chayne | `am_adam` | `bm_george` |
| Delaila | `bf_lily` | `af_sarah` |
| Einar | `bm_lewis` | `am_onyx` |
| Elouisa | `af_nova` | `af_aoede` |
| Eshe | `bf_emma` | `bf_isabella` |
| Hassian | `am_fenrir` | `am_liam` |
| Hekla | `bf_isabella` | `af_nicole` |
| Hodari | `bm_daniel` | `am_michael` |
| Jel | `bm_george` | `bm_lewis` |
| Jina | `af_heart` | `af_bella` |
| Kenli | `am_onyx` | `am_echo` |
| Kenyatta | `af_sky` | `af_nova` |
| Najuma | `af_jessica` | `af_sky` |
| Reth | `am_echo` | `am_puck` |
| Sifuu | `af_nicole` | `af_heart` |
| Subira | `af_bella` | `bf_emma` |
| Tamala | `bf_alice` | `af_nicole` |
| Tish | `af_sarah` | `af_heart` |
| Zeki | `am_liam` | `am_onyx` |

Ashura (`am_michael`) and Nai'o (`am_eric`) use the same id the first pass proposed. Both are still provisional.

The 23 configured villagers each have a different voice id. Unknown also uses `af_bella`, the same id as Subira. That fallback card is separate from the villager cast.

## Still needs a person to listen

Every provisional voice. In particular:

- Najuma: the fit was already marked low because Kokoro has no child voice.
- Subira: `af_bella` has been heard as audio, not approved as her voice.
- Elouisa: `af_aoede` is available if someone chooses it later. It is not her configured voice.
- British ids are a creative accent, not canon.

Do not mark any of these confirmed until the same lines have been heard and a person keeps or changes the id in `npc-voices.json`.
