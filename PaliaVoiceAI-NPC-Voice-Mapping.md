# Palia Voice AI --- NPC Character & Kokoro Voice Mapping

**Version:** 1.0\
**Updated:** 17 November 2026\
**Status:** Proposed default voice profiles --- pending in-game
listening validation\
**Language target:** English (US default, with selected British voices
where a deliberate character distinction is useful)\
**TTS model:** Kokoro 82M

> **Important scope and accuracy note**
>
> This catalogue covers the named Villager/NPC roster exposed by the
> current Palia community wiki's Villager category (29 entries). It is
> not a claim to enumerate every creature, quest-only entity, historical
> person, family member mentioned in lore, or unnamed NPC in the entire
> game world. Palia's broader NPC category is larger and includes
> non-villager and non-speaking entities. Those should be added in a
> separate expansion once their in-game dialogue and identity are
> confirmed.
>
> The game does not publish an exact numerical age for every character.
> Therefore, age labels below mean **apparent / narrative age band**,
> not a canonical age. Where gender, species, or age is not reliably
> established in accessible character references, it is explicitly
> marked **unknown / needs confirmation** rather than guessed.
>
> Kokoro voice IDs encode language/accent and model speaker identity,
> not a canonical age or personality. The voice descriptions and
> character-to-voice pairings below are **our production direction and
> proposed defaults**, not official voice casting and not a claim that
> Kokoro's speaker is literally a child, elderly person, robot, or the
> original game's voice actor. Every assignment must be auditioned in
> the actual game dialogue before being frozen.

------------------------------------------------------------------------

# 1. Voice mapping policy

## 1.1 What the Kokoro IDs mean

For English Kokoro voice IDs:

-   `af_` = American English, female speaker.
-   `am_` = American English, male speaker.
-   `bf_` = British English, female speaker.
-   `bm_` = British English, male speaker.

The prefix tells us the model's language/accent and speaker gender
category. It does **not** formally specify age, pitch range, emotional
temperament, or character archetype.

Kokoro's voice catalogue includes, among others, `af_bella`,
`af_nicole`, `af_sarah`, `af_heart`, `af_aoede`, `am_michael`,
`am_fenrir`, `am_puck`, `am_onyx`, `am_liam`, `bm_george`, and
`bf_emma`. Voice quality ratings found in the model's voice catalogue
are estimates of training-data quality/quantity, not objective acting or
character-fit scores.

## 1.2 Meaning of the voice direction fields

  -----------------------------------------------------------------------
  Field                               Meaning
  ----------------------------------- -----------------------------------
  `voice_id`                          Default Kokoro speaker ID to use

  `accent`                            Accent implied by the selected
                                      Kokoro ID

  `delivery`                          Acting direction for the TTS output

  `pitch_direction`                   Relative pitch direction;
                                      descriptive, not guaranteed to be
                                      directly controllable

  `speed`                             Starting speed multiplier; 1.0 is
                                      normal speed. Tune after listening

  `emotion`                           Delivery style to encourage through
                                      punctuation/text preprocessing or
                                      supported controls

  `confidence`                        Confidence in the identity/age
                                      description, not confidence in TTS
                                      quality
  -----------------------------------------------------------------------

**Important:** Kokoro does not provide a guaranteed independent "child
voice", "old man voice", "robot voice", or emotion switch just because a
profile says so. The default speaker embedding supplies the underlying
voice identity. `speed` and text segmentation can help shape delivery,
but do not turn an adult voice into a convincing child voice or create
authentic robotic synthesis. Do not assume arbitrary pitch shifting is
available in the current KokoroSharp integration until it is implemented
and tested.

## 1.3 Selection principles

1.  Keep each character's default voice stable across every dialogue.
2.  Avoid assigning the same voice to several prominent characters
    unless there is a clear reason.
3.  Prioritize intelligibility and natural English over exaggerated
    stereotypes.
4.  Use lower/slower delivery for mature, reserved, authoritative
    characters; brighter/faster delivery for energetic younger
    characters.
5.  Use controlled, measured delivery for Galdur characters; do not add
    artificial glitch effects in the default profile.
6.  Do not imitate the original voice actors.
7.  Use the profile's default `speed` only as an initial tuning value.
    Test values between 0.90 and 1.10 first.
8.  If a character's canon gender or species is unclear, keep it
    explicitly unknown and choose a temporary neutral profile only after
    verifying the in-game presentation.

------------------------------------------------------------------------

# 2. Master roster and default assignments

The table is a quick index. Full descriptions and reasoning follow in
section 3.

  ------------------------------------------------------------------------------------------
  Character   Gender         Species /      Apparent age Proposed Kokoro Delivery
                             group          band         default         
  ----------- -------------- -------------- ------------ --------------- -------------------
  Ashura      Male           Majiri         Older adult  `am_michael`    Warm, mature,
                                                                         grounded

  Auni        Male           Majiri         Teenager     `am_puck`       Bright, youthful,
                                                                         quick

  Badruu      Male           Majiri         Adult        `am_fenrir`     Playful,
                                                                         expressive, warm

  Caleri      Female         Majiri         Adult        `bf_emma`       Precise, composed,
                                                                         intellectual

  Chayne      Male           Majiri         Adult        `bm_george`     Calm, gentle,
                                                                         reflective

  Delaila     Female         Majiri         Adult /      `af_sarah`      Warm, maternal,
                                            mature                       practical

  Einar       Male-coded /   Galdur         Ageless /    `am_onyx`       Measured, even,
              Galdur         construct      artificial                   synthetic-neutral
              identity                                                   

  Elouisa     Female         Majiri         Adult        `af_aoede`      Curious, animated,
                                                                         quirky

  Eshe        Female         Majiri         Mature adult `bf_isabella`   Formal, controlled,
                                                                         authoritative

  Eshelon     Unknown        Unknown /      Unknown      `TBD`           Do not assign until
                             needs                                       verified
                             confirmation                                

  Hassian     Male           Majiri         Young adult  `am_liam`       Reserved, low-key,
                                                                         restrained

  Hekla       Female-coded / Galdur         Ageless /    `af_nicole`     Calm, clear,
              Galdur         construct      artificial                   caretaker-like
              identity                                                   

  Hodari      Male           Majiri         Adult        `am_michael`    Low, steady,
                                                                         protective

  Jel         Male           Majiri         Young adult  `bm_lewis`      Refined,
                                            / adult                      expressive,
                                                                         theatrical

  Jina        Female         Majiri         Young adult  `af_bella`      Clear, thoughtful,
                                                                         curious

  Kenli       Male           Majiri         Adult /      `am_echo`       Polite, slightly
                                            mature                       hesitant, gentle

  Kenyatta    Female         Majiri         Young adult  `af_nova`       Confident,
                                                                         energetic, direct

  Nai'o       Male           Majiri         Young adult  `am_eric`       Open, friendly,
                                                                         earnest

  Najuma      Female         Majiri         Child /      `af_sky`        Soft, bright,
                                            young teen   (temporary)     careful

  Reth        Male           Majiri         Young adult  `am_puck`       Lively, cheeky,
                                                         (temporary;     warm
                                                         review          
                                                         collision with  
                                                         Auni)           

  Sifuu       Female         Majiri         Adult        `af_heart`      Strong, resonant,
                                                                         confident

  Subira      Female         Majiri         Adult /      `bf_emma`       Controlled,
                                            mature       (temporary;     observant, formal
                                                         review          
                                                         collision with  
                                                         Caleri)         

  Tamala      Female         Majiri         Adult /      `af_nicole`     Smooth, composed,
                                            mature       (temporary;     enigmatic
                                                         review          
                                                         collision with  
                                                         Hekla)          

  Tau         Male-coded     Plumehound     Animal /     `TBD`           No human speech
                                            non-human                    profile unless
                                            companion                    actual spoken lines
                                                                         require it

  Taylin      Unknown        Unknown /      Unknown      `TBD`           Do not assign until
                             needs                                       verified
                             confirmation                                

  The         Unknown /      Galdur         Ageless /    `TBD`           Neutral synthetic
  Gardener    non-human                     artificial                   profile after
              construct                                                  dialogue
                                                                         verification

  Tish        Female         Majiri         Young adult  `af_heart`      Bright, gentle,
                                                         (temporary;     optimistic
                                                         review          
                                                         collision with  
                                                         Sifuu)          

  Ulfe        Unknown        Unknown /      Unknown      `TBD`           Do not assign until
                             needs                                       verified
                             confirmation                                

  Zeki        Male           Grimalkin      Adult /      `am_onyx`       Clever, dry,
                                            mature       (temporary;     measured
                                                         review          
                                                         collision with  
                                                         Einar)          
  ------------------------------------------------------------------------------------------

**Assignment caveat:** This table is a first-pass sound direction, not a
validated final cast. Repeated IDs are flagged for review because Kokoro
has a finite voice catalogue. Do not solve collisions by randomly
changing voices; audition candidate alternatives and compare them in the
same dialogue lines.

------------------------------------------------------------------------

# 3. Individual character profiles

## 3.1 Ashura

-   **Name:** Ashura
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Older adult / mature adult. Exact numerical age
    not confirmed.
-   **Role:** Innkeeper at Ormuu's Horn; former soldier.
-   **Character impression:** A dependable, experienced, supportive
    figure. Calm authority with warmth; has a history that suggests
    emotional depth without needing an exaggerated "old man" caricature.
-   **Default Kokoro voice:** `am_michael`
-   **Accent:** American English.
-   **Delivery direction:** Warm, mature, grounded, reassuring.
    Moderate-low perceived pitch; deliberate phrasing; avoid sounding
    frail or excessively gravelly.
-   **Starting speed:** `0.96`
-   **Why this voice:** A male American voice is a suitable starting
    point for a grounded, mature innkeeper. The voice ID itself does not
    encode old age; the slower pace and measured punctuation are part of
    our acting direction.
-   **Confidence:** Gender/species/role high; exact age band
    approximate; voice fit untested.

## 3.2 Auni

-   **Name:** Auni
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Young teenager.
-   **Role:** Mail carrier and insect catcher; youngest son of Badruu
    and Delaila.
-   **Character impression:** Energetic, curious, optimistic,
    mischievous, eager to prove himself.
-   **Default Kokoro voice:** `am_puck`
-   **Accent:** American English.
-   **Delivery direction:** Bright, lively, quick, friendly; slightly
    higher perceived pitch than adult male profiles. Keep articulation
    crisp and avoid making him sound like a very young child.
-   **Starting speed:** `1.06`
-   **Why this voice:** A lighter, more animated male profile is a
    reasonable temporary fit for a teenage character. Kokoro does not
    guarantee a convincing adolescent voice, so this must be tested
    carefully.
-   **Confidence:** Gender/species/teenage characterization high; voice
    fit untested.

## 3.3 Badruu

-   **Name:** Badruu
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Adult / mature adult.
-   **Role:** Farmer; former bard; father of Auni and Nai'o.
-   **Character impression:** Sociable, playful, pun-loving, expressive,
    affectionate.
-   **Default Kokoro voice:** `am_fenrir`
-   **Accent:** American English.
-   **Delivery direction:** Friendly and animated with a storyteller's
    rhythm. Give jokes a light pause; avoid making every sentence sound
    like a comedy performance.
-   **Starting speed:** `1.02`
-   **Why this voice:** A distinct male speaker with a more expressive
    direction helps separate him from quieter adult male characters.
-   **Confidence:** Gender/species/role high; exact age approximate;
    voice fit untested.

## 3.4 Caleri

-   **Name:** Caleri
-   **Gender:** Female
-   **Species / group:** Majiri
-   **Apparent age:** Adult.
-   **Role:** Librarian.
-   **Character impression:** Intellectual, disciplined, precise,
    serious, sometimes impatient.
-   **Default Kokoro voice:** `bf_emma`
-   **Accent:** British English.
-   **Delivery direction:** Clear, composed, precise diction; restrained
    emotion; measured pace. Do not make her sound hostile by default.
-   **Starting speed:** `0.96`
-   **Why this voice:** A British female voice provides a deliberate
    distinction from warmer American female profiles. Accent is a design
    choice, not established canon.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.5 Chayne

-   **Name:** Chayne
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Adult.
-   **Role:** Priest and spiritual guide.
-   **Character impression:** Patient, compassionate, reflective,
    welcoming, thoughtful.
-   **Default Kokoro voice:** `bm_george`
-   **Accent:** British English.
-   **Delivery direction:** Gentle, calm, reflective, evenly paced.
    Avoid excessive solemnity; he should remain approachable.
-   **Starting speed:** `0.94`
-   **Why this voice:** A measured British male profile is a useful
    contrast to the more casual American male profiles.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.6 Delaila

-   **Name:** Delaila
-   **Gender:** Female
-   **Species / group:** Majiri
-   **Apparent age:** Adult / mature adult.
-   **Role:** Farmer; mother of Auni and Nai'o; wife of Badruu.
-   **Character impression:** Practical, caring, confident, maternal,
    direct.
-   **Default Kokoro voice:** `af_sarah`
-   **Accent:** American English.
-   **Delivery direction:** Warm and natural, with a practical, grounded
    rhythm. Friendly but capable of firm emphasis.
-   **Starting speed:** `0.98`
-   **Why this voice:** A clear female American profile is a reasonable
    base for a warm, mature family character.
-   **Confidence:** Gender/species/role high; exact age approximate;
    voice fit untested.

## 3.7 Einar

-   **Name:** Einar
-   **Gender:** Male-coded / uses a male character presentation; the
    character is a Galdur construct, not a biological human.
-   **Species / group:** Galdur.
-   **Apparent age:** Ageless / artificial. Do not classify as a child
    or elderly human.
-   **Role:** Fisher; Galdur with a philosophical interest in "Oneness".
-   **Character impression:** Literal, thoughtful, socially unusual,
    gentle, curious, deliberate.
-   **Default Kokoro voice:** `am_onyx`
-   **Accent:** American English.
-   **Delivery direction:** Very even pacing, clear enunciation,
    restrained emotional variation, no exaggerated robot glitch. A
    slightly lower, steady presentation can suggest a constructed being
    without sacrificing intelligibility.
-   **Starting speed:** `0.94`
-   **Why this voice:** A stable male voice can support a measured
    synthetic persona. The "robot" quality must come from performance
    direction, not from a claim that this Kokoro speaker is a robot
    voice.
-   **Confidence:** Galdur identity and character presentation high;
    gender terminology should follow the game's pronouns/dialogue; voice
    fit untested.

## 3.8 Elouisa

-   **Name:** Elouisa
-   **Gender:** Female
-   **Species / group:** Majiri
-   **Apparent age:** Adult.
-   **Role:** Researcher / enthusiast interested in unusual creatures
    and mysteries.
-   **Character impression:** Imaginative, excitable, curious,
    eccentric, enthusiastic.
-   **Default Kokoro voice:** `af_aoede`
-   **Accent:** American English.
-   **Delivery direction:** Animated, curious, expressive pitch
    movement; lively but intelligible. Use pauses before surprising
    theories.
-   **Starting speed:** `1.03`
-   **Why this voice:** A brighter female profile is a reasonable
    contrast with the formal and reserved women in the roster.
-   **Confidence:** Gender/species high; role and voice fit should be
    reviewed against current dialogue.

## 3.9 Eshe

-   **Name:** Eshe
-   **Gender:** Female
-   **Species / group:** Majiri
-   **Apparent age:** Mature adult.
-   **Role:** Magistrate; part of Kilima's governing leadership.
-   **Character impression:** Formal, authoritative, disciplined,
    socially polished, demanding.
-   **Default Kokoro voice:** `bf_isabella`
-   **Accent:** British English.
-   **Delivery direction:** Controlled, formal, precise; confident and
    measured. Avoid making her sound villainous or cold in every line.
-   **Starting speed:** `0.96`
-   **Why this voice:** A formal British female profile creates a clear
    distinction for a high-status official. The accent is a creative
    default, not canon.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.10 Eshelon

-   **Name:** Eshelon
-   **Gender:** Unknown / needs confirmation.
-   **Species / group:** Unknown / needs confirmation.
-   **Apparent age:** Unknown.
-   **Role and speaking status:** The current Villager category lists
    the name, but the available sources used for this mapping did not
    establish enough reliable information about the character's playable
    dialogue, gender, species, or age.
-   **Default Kokoro voice:** `TBD — do not assign`
-   **Delivery direction:** Wait until the character's in-game model,
    pronouns, and actual spoken lines are confirmed.
-   **Confidence:** Insufficient. This is intentionally left unassigned.

## 3.11 Hassian

-   **Name:** Hassian
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Young adult.
-   **Role:** Hunter; often works independently.
-   **Character impression:** Reserved, guarded, serious, independent,
    emotionally restrained, but caring beneath the surface.
-   **Default Kokoro voice:** `am_liam`
-   **Accent:** American English.
-   **Delivery direction:** Restrained, calm, slightly low-key, short
    direct phrasing. Avoid making him sound angry in neutral
    conversation.
-   **Starting speed:** `0.94`
-   **Why this voice:** A less exuberant male profile with restrained
    delivery fits the intended contrast with Badruu and Auni.
-   **Confidence:** Gender/species/role high; exact age approximate;
    voice fit untested.

## 3.12 Hekla

-   **Name:** Hekla
-   **Gender:** Female-coded / uses a feminine character presentation;
    she is a Galdur construct.
-   **Species / group:** Galdur.
-   **Apparent age:** Ageless / artificial.
-   **Role:** Caretaker / nanny and archaeological assistant.
-   **Character impression:** Patient, dutiful, attentive, precise,
    protective.
-   **Default Kokoro voice:** `af_nicole`
-   **Accent:** American English.
-   **Delivery direction:** Calm, clear, even, reassuring. Keep the
    delivery composed and slightly formal; no mechanical sound effects
    by default.
-   **Starting speed:** `0.96`
-   **Why this voice:** A clear, controlled female voice is a useful
    starting point for a caretaker-like Galdur.
-   **Confidence:** Galdur identity and role high; gender wording should
    follow in-game pronouns; voice fit untested.

## 3.13 Hodari

-   **Name:** Hodari
-   **Gender:** Male
-   **Species / group:** Majiri
-   **Apparent age:** Adult.
-   **Role:** Miner; father of Najuma.
-   **Character impression:** Responsible, protective, practical,
    steady, reserved but caring.
-   **Default Kokoro voice:** `am_michael`
-   **Accent:** American English.
-   **Delivery direction:** Low-key, grounded, steady, warm underneath a
    practical exterior. Avoid over-deepening or artificially aging the
    voice.
-   **Starting speed:** `0.95`
-   **Why this voice:** A stable adult male profile suits a protective
    father and working miner. Compare against Ashura in listening tests
    to ensure they remain distinguishable.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.14 Jel

-   **Name:** Jel
-   **Gender:** Male
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult / adult.
-   **Role:** Tailor and fashion-oriented artisan.
-   **Character impression:** Artistic, expressive, refined, sensitive,
    theatrical, attentive to aesthetics.
-   **Default Kokoro voice:** `bm_lewis`
-   **Accent:** British English.
-   **Delivery direction:** Refined, expressive, articulate, with gentle
    theatrical emphasis. Do not exaggerate into parody.
-   **Starting speed:** `1.00`
-   **Why this voice:** A British male option distinguishes him from the
    American male cast. The refined delivery is an acting direction
    rather than a canonical accent.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.15 Jina

-   **Name:** Jina
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult.
-   **Role:** Archaeologist and researcher.
-   **Character impression:** Curious, intelligent, thoughtful, slightly
    hesitant in social situations, sincere.
-   **Default Kokoro voice:** `af_bella`
-   **Accent:** American English.
-   **Delivery direction:** Clear, thoughtful, curious, warm. Use a
    natural conversational pace; avoid making her sound overly confident
    or overly timid.
-   **Starting speed:** `0.98`
-   **Why this voice:** A clear, expressive female voice can communicate
    curiosity while remaining easy to understand.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.16 Kenli

-   **Name:** Kenli
-   **Gender:** Male
-   **Species / group:** Majiri.
-   **Apparent age:** Adult / mature adult.
-   **Role:** Mayor of Kilima.
-   **Character impression:** Polite, well-meaning, somewhat awkward or
    hesitant, friendly, sometimes overwhelmed by responsibility.
-   **Default Kokoro voice:** `am_echo`
-   **Accent:** American English.
-   **Delivery direction:** Polite, gentle, slightly hesitant; moderate
    pace and clear diction. Do not portray him as incompetent.
-   **Starting speed:** `0.98`
-   **Why this voice:** A softer male profile provides contrast with
    more forceful or reserved male characters.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.17 Kenyatta

-   **Name:** Kenyatta
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult.
-   **Role:** Clerk; daughter of Eshe and Kenli.
-   **Character impression:** Independent, energetic, confident,
    rebellious, direct, sometimes impatient with expectations.
-   **Default Kokoro voice:** `af_nova`
-   **Accent:** American English.
-   **Delivery direction:** Confident, brisk, direct, lively. Keep the
    voice natural rather than aggressive.
-   **Starting speed:** `1.04`
-   **Why this voice:** A brighter and more forward female profile helps
    distinguish her from her mother Eshe.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.18 Nai'o

-   **Name:** Nai'o
-   **Gender:** Male
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult.
-   **Role:** Farmer; older brother of Auni.
-   **Character impression:** Hardworking, kind-hearted, earnest,
    friendly, sometimes not especially introspective.
-   **Default Kokoro voice:** `am_eric`
-   **Accent:** American English.
-   **Delivery direction:** Open, friendly, sincere, uncomplicated and
    relaxed. Avoid making him sound childish.
-   **Starting speed:** `1.00`
-   **Why this voice:** A straightforward male voice provides a natural
    fit for a friendly young adult farmer.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.19 Najuma

-   **Name:** Najuma
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Child / young teen. The game's Almanac describes
    her as a young girl.
-   **Role:** Miner and inventor; daughter of Hodari.
-   **Character impression:** Shy but brave, intelligent, inventive,
    thoughtful, determined.
-   **Default Kokoro voice:** `af_sky` --- temporary only.
-   **Accent:** American English.
-   **Delivery direction:** Gentle, bright, careful, youthful. Avoid
    treating the character as a toddler. Use clear articulation and a
    slightly quicker rhythm when she is excited about inventions.
-   **Starting speed:** `1.02`
-   **Why this voice:** A lighter female profile is the closest initial
    direction among the selected Kokoro English voices. It is not a
    verified child voice and may sound too mature. If it fails the
    listening test, mark the character as requiring a different
    model/voice rather than forcing a bad match.
-   **Confidence:** Gender/species/young age high; voice fit low and
    requires careful audition.

## 3.20 Reth

-   **Name:** Reth
-   **Gender:** Male
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult.
-   **Role:** Chef at Ormuu's Horn; works with Ashura.
-   **Character impression:** Sociable, quick-witted, playful,
    street-smart, warm, sometimes evasive.
-   **Default Kokoro voice:** `am_puck` --- provisional; currently also
    proposed for Auni.
-   **Accent:** American English.
-   **Delivery direction:** Lively, cheeky, conversational, quick but
    clear. Use playful timing without making every line sound comedic.
-   **Starting speed:** `1.04`
-   **Why this voice:** The lively delivery suits his banter. However,
    the same voice ID is proposed for Auni, so this is a known
    collision. Before release, audition alternatives such as `am_liam`,
    `am_fenrir`, or `am_michael` and assign unique profiles if possible.
-   **Confidence:** Gender/species/role high; voice fit untested;
    duplicate assignment must be resolved.

## 3.21 Sifuu

-   **Name:** Sifuu
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Adult.
-   **Role:** Blacksmith; skilled and physically strong.
-   **Character impression:** Strong, confident, direct, brave, warm,
    energetic.
-   **Default Kokoro voice:** `af_heart`
-   **Accent:** American English.
-   **Delivery direction:** Confident, resonant, energetic, assertive
    without sounding harsh. Strong emphasis on decisive statements.
-   **Starting speed:** `1.00`
-   **Why this voice:** A strong, expressive female profile is a useful
    starting point for a confident blacksmith.
-   **Confidence:** Gender/species/role high; voice fit untested.

## 3.22 Subira

-   **Name:** Subira
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Adult / mature adult.
-   **Role:** Watcher from The Order, sent to investigate unusual events
    in Kilima.
-   **Character impression:** Observant, disciplined, intelligent,
    formal, controlled, cautious.
-   **Default Kokoro voice:** `bf_emma` --- provisional; currently also
    proposed for Caleri.
-   **Accent:** British English.
-   **Delivery direction:** Formal, controlled, analytical, measured,
    with subtle warmth when trust develops.
-   **Starting speed:** `0.95`
-   **Why this voice:** A composed female profile fits the role of an
    investigator. The British accent is a creative distinction, not
    canon. This duplicates Caleri's proposed ID and must be reviewed.
-   **Confidence:** Gender/species/role high; voice fit untested;
    duplicate assignment must be resolved.

## 3.23 Tamala

-   **Name:** Tamala
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Adult / mature adult. Exact numerical age is not
    confirmed here.
-   **Role:** Alchemist living in Bahari Bay.
-   **Character impression:** Enigmatic, self-assured, composed,
    independent, sometimes teasing or manipulative.
-   **Default Kokoro voice:** `af_nicole` --- provisional; currently
    also proposed for Hekla.
-   **Accent:** American English.
-   **Delivery direction:** Smooth, composed, confident, slightly
    enigmatic. Avoid turning every line into a whisper or villain
    performance.
-   **Starting speed:** `0.96`
-   **Why this voice:** A controlled female profile can provide a
    contrast to energetic characters. The assignment duplicates Hekla
    and must be resolved.
-   **Confidence:** Gender/species/role high; voice fit untested;
    duplicate assignment must be resolved.

## 3.24 Tau

-   **Name:** Tau
-   **Gender:** Male-coded in character references; verify the game's
    pronouns and dialogue.
-   **Species / group:** Plumehound (non-human animal companion).
-   **Apparent age:** Animal age category not safely mapped to human
    child/adult/elder. Treat as a dog-like companion, not a human.
-   **Role:** Hassian's companion.
-   **Character impression:** Loyal, affectionate, expressive through
    animal behavior.
-   **Default Kokoro voice:** `TBD — no human TTS voice by default`
-   **Delivery direction:** Do not synthesize ordinary human speech for
    Tau unless the game actually gives him spoken dialogue. If the
    character communicates through barks/whines or non-verbal sounds,
    use licensed/original sound effects rather than Kokoro human speech.
-   **Confidence:** Species/companion role high; gender and any
    spoken-dialogue status should be verified from the current game
    build.

## 3.25 Taylin

-   **Name:** Taylin
-   **Gender:** Unknown / needs confirmation.
-   **Species / group:** Unknown / needs confirmation.
-   **Apparent age:** Unknown.
-   **Role and speaking status:** Listed in the current Villager index,
    but reliable details sufficient to assign gender, species, age, and
    a spoken voice were not confirmed in the sources used for this
    document.
-   **Default Kokoro voice:** `TBD — do not assign`
-   **Delivery direction:** Wait for a verified character profile and
    in-game spoken dialogue.
-   **Confidence:** Insufficient. Intentionally unassigned.

## 3.26 The Gardener

-   **Name:** The Gardener
-   **Gender:** Unknown / non-human identity; do not assign a human
    gender without confirming pronouns in-game.
-   **Species / group:** Galdur construct.
-   **Apparent age:** Ageless / artificial.
-   **Role:** Galdur creator / ancient construct associated with the
    Galdur story.
-   **Character impression:** Ancient, controlled, unusual, possibly
    formal or detached depending on the specific dialogue.
-   **Default Kokoro voice:**
    `TBD — provisional synthetic-neutral profile after dialogue review`
-   **Delivery direction:** If spoken lines are confirmed, test a
    measured neutral voice with precise timing. Do not add robotic
    distortion by default.
-   **Confidence:** Galdur identity high; exact pronouns, voice
    direction and spoken line scope require verification.

## 3.27 Tish

-   **Name:** Tish
-   **Gender:** Female
-   **Species / group:** Majiri.
-   **Apparent age:** Young adult.
-   **Role:** Carpenter; Jel's sister.
-   **Character impression:** Kind, optimistic, creative, gentle,
    encouraging, sociable.
-   **Default Kokoro voice:** `af_heart` --- provisional; currently also
    proposed for Sifuu.
-   **Accent:** American English.
-   **Delivery direction:** Bright, warm, gentle, optimistic. Softer and
    less forceful than Sifuu's proposed direction.
-   **Starting speed:** `1.02`
-   **Why this voice:** A warm female voice is a reasonable starting
    point. Because the same voice ID is proposed for Sifuu, test
    `af_sarah`, `af_bella`, or `af_aoede` as alternatives and avoid a
    duplicate if the difference is not convincing.
-   **Confidence:** Gender/species/role high; voice fit untested;
    duplicate assignment must be resolved.

## 3.28 Ulfe

-   **Name:** Ulfe
-   **Gender:** Unknown / needs confirmation.
-   **Species / group:** Unknown / needs confirmation.
-   **Apparent age:** Unknown.
-   **Role and speaking status:** Listed in the Villager category, but
    this document's sources did not establish enough reliable details
    for a gender/age/species classification and a defensible voice
    assignment.
-   **Default Kokoro voice:** `TBD — do not assign`
-   **Delivery direction:** Verify current in-game appearance, pronouns,
    dialogue and species first.
-   **Confidence:** Insufficient. Intentionally unassigned.

## 3.29 Zeki

-   **Name:** Zeki
-   **Gender:** Male
-   **Species / group:** Grimalkin (cat-like humanoid species in Palia).
-   **Apparent age:** Adult / mature adult; exact age not confirmed.
-   **Role:** General store owner and merchant.
-   **Character impression:** Clever, shrewd, commercially minded,
    witty, persuasive, sometimes sly.
-   **Default Kokoro voice:** `am_onyx` --- provisional; currently also
    proposed for Einar.
-   **Accent:** American English.
-   **Delivery direction:** Confident, smooth, measured, lightly
    sardonic. Keep diction clear and avoid exaggerated "villain" acting.
-   **Starting speed:** `0.98`
-   **Why this voice:** A measured male profile can support a clever
    merchant persona. The current assignment duplicates Einar and should
    be resolved after auditioning `am_echo`, `am_michael`, or
    `am_fenrir`.
-   **Confidence:** Gender/species/role high; exact age approximate;
    voice fit untested; duplicate assignment must be resolved.

------------------------------------------------------------------------

# 4. Voice ID inventory and quality considerations

The following are English Kokoro speaker IDs that can be considered
during audition. The model's own voice catalogue documents the available
speaker IDs and provides subjective training-data quality estimates.
Those grades should not be mistaken for an age or personality
classification.

## American English --- Female

  -----------------------------------------------------------------------
  ID                                  Use in this project
  ----------------------------------- -----------------------------------
  `af_heart`                          Strong / warm female candidate

  `af_bella`                          Expressive female candidate

  `af_nicole`                         Female candidate; test for clear,
                                      composed delivery

  `af_aoede`                          Alternative expressive female
                                      candidate

  `af_sarah`                          Alternative natural female
                                      candidate

  `af_nova`                           Brighter female candidate

  `af_sky`                            Temporary lighter female candidate;
                                      not a guaranteed child voice

  `af_alloy`                          Alternative to audition

  `af_jessica`                        Alternative to audition; quality
                                      grade is lower in the cited voice
                                      catalogue

  `af_kore`                           Alternative to audition

  `af_river`                          Alternative to audition
  -----------------------------------------------------------------------

## American English --- Male

  -----------------------------------------------------------------------
  ID                                  Use in this project
  ----------------------------------- -----------------------------------
  `am_michael`                        Mature / grounded candidate

  `am_fenrir`                         Expressive / energetic candidate

  `am_puck`                           Lively candidate

  `am_onyx`                           Measured / lower-presenting
                                      candidate

  `am_liam`                           Reserved / conversational candidate

  `am_eric`                           Natural conversational alternative

  `am_echo`                           Softer alternative

  `am_adam`                           Available but lower quality grade
                                      in the cited catalogue; audition
                                      before use

  `am_santa`                          Special-purpose candidate; not a
                                      default general NPC voice
  -----------------------------------------------------------------------

## British English --- Female

  ID              Use in this project
  --------------- --------------------------------
  `bf_emma`       Main composed female candidate
  `bf_isabella`   Formal alternative
  `bf_alice`      Alternative to audition
  `bf_lily`       Alternative to audition

## British English --- Male

  ID            Use in this project
  ------------- --------------------------
  `bm_george`   Calm, measured candidate
  `bm_lewis`    Refined alternative
  `bm_fable`    Alternative to audition
  `bm_daniel`   Alternative to audition

**Quality warning:** The published quality grades are not a guarantee of
how a voice sounds on a particular dialogue. Actual listening tests with
Palia's lines are mandatory. A lower catalogue grade does not
automatically mean a voice is unusable, and a higher grade does not
guarantee a character match.

------------------------------------------------------------------------

# 5. Duplicate voice assignments to resolve

The first-pass map contains deliberate provisional duplicates. They must
be reviewed before the NPC voice map is marked production-ready.

  -----------------------------------------------------------------------
  Kokoro ID               Characters currently    Required action
                          mapped                  
  ----------------------- ----------------------- -----------------------
  `am_michael`            Ashura, Hodari          Compare the two
                                                  in-game; consider
                                                  `am_echo` or
                                                  `am_fenrir` for one

  `am_puck`               Auni, Reth              Separate the younger
                                                  energetic character
                                                  from the adult chef

  `am_onyx`               Einar, Zeki             Einar may need measured
                                                  synthetic-neutral; Zeki
                                                  needs merchant
                                                  personality

  `bf_emma`               Caleri, Subira          Distinguish librarian
                                                  from Watcher; test
                                                  `bf_isabella`

  `af_nicole`             Hekla, Tamala           Distinguish caretaker
                                                  Galdur from enigmatic
                                                  alchemist

  `af_heart`              Sifuu, Tish             Distinguish strong
                                                  blacksmith from gentle
                                                  carpenter
  -----------------------------------------------------------------------

A duplicate is acceptable only if: - The characters rarely speak near
one another, **and** - Their delivery settings make them clearly
distinguishable, **and** - Listening tests show no confusing similarity.

Otherwise, assign unique voice IDs.

------------------------------------------------------------------------

# 6. Suggested profile configuration format

The application should eventually load NPC profiles from JSON rather
than hard-code them in the UI. Example schema:

``` json
{
  "characterId": "ashura",
  "displayName": "Ashura",
  "gender": "male",
  "species": "Majiri",
  "apparentAgeBand": "older_adult",
  "ageIsCanon": false,
  "kokoro": {
    "language": "en-US",
    "voiceId": "am_michael",
    "speed": 0.96
  },
  "delivery": {
    "style": "warm_mature_grounded",
    "pitchDirection": "moderately_low",
    "emotion": ["warm", "reassuring", "calm"]
  },
  "status": "proposed",
  "validation": {
    "auditionedInGame": false,
    "approvedByUser": false
  }
}
```

## 6.1 Recommended enum values

``` text
Gender:
  male
  female
  non_binary
  unknown
  male_coded
  female_coded

Species:
  majiri
  galdur
  grimalkin
  plumehound
  human
  unknown
  other

ApparentAgeBand:
  child
  young_teen
  teenager
  young_adult
  adult
  mature_adult
  older_adult
  elder
  ageless_construct
  animal
  unknown

ProfileStatus:
  proposed
  needs_review
  auditioned
  approved
  disabled
```

------------------------------------------------------------------------

# 7. Voice audition and approval workflow

A profile must not become the production default merely because it has
been written in this file.

For every character:

1.  Generate the same 3--5 representative English lines with the
    proposed voice.
2.  Include a neutral line, an excited line, and a serious/emotional
    line when available.
3.  Listen with the same playback volume and output device.
4.  Compare against at least one alternative Kokoro voice of the same
    gender/accent family.
5.  Check pronunciation of the character's name and Palia-specific
    terms.
6.  Check that the voice remains understandable at the selected speed.
7.  Approve, revise, or leave as `needs_review`.
8.  Record the approved voice ID and test date in the configuration.

## 7.1 Per-character approval checklist

-   [ ] Gender / pronouns checked against in-game references.
-   [ ] Species checked.
-   [ ] Apparent age band checked; canonical age not invented.
-   [ ] Role and personality checked.
-   [ ] Default Kokoro voice generated.
-   [ ] Alternative voice generated.
-   [ ] Pronunciation checked.
-   [ ] Speed checked.
-   [ ] Voice distinct from nearby NPCs.
-   [ ] User approved the profile.

------------------------------------------------------------------------

# 8. Known limitations and future additions

1.  **Not all 104 entries in the broad NPC category are speaking
    villagers.** The broader NPC index includes family members, quest
    figures, creatures, historical/lore names, and other entities. This
    file deliberately prioritizes the named Villager roster.
2.  **Age is often not a canonical number.** Apparent age bands are used
    only when character presentation or official descriptions support
    them.
3.  **Kokoro has no guaranteed child/elder/robot class.** A speaker ID
    is not an age classifier.
4.  **Voice assignments are creative defaults.** They are not official
    Palia voice casting and do not reproduce original voice actors.
5.  **Some entries are intentionally TBD.** Do not fill unknown
    gender/species/age by guesswork.
6.  **This document's Kokoro IDs are recommendations, not the runtime
    assignment.** The app loads `npc-voices.json`. That file keeps the
    voice IDs already used by the app. Proposed IDs in this document
    were not copied over. Names, roles, and avatar paths can be edited
    in the JSON file.
7.  **Non-speaking characters should not be forced to speak.** Tau and
    other animal/creature characters should use non-verbal effects only
    if the game provides corresponding communication.
8.  **New characters and patches:** the roster must be reviewed when
    Palia adds new villagers or changes character information.

------------------------------------------------------------------------

# 9. Sources

The following sources were used to establish the roster and available
Kokoro speaker IDs. The wiki is community-maintained; verify details
against the current game build where possible.

-   Palia Official Wiki (community-maintained) --- Villager category,
    roster: https://palia.wiki.gg/wiki/Category:Villager
-   Palia Official Wiki --- Villagers:
    https://palia.wiki.gg/wiki/Villagers
-   Palia Official Wiki --- Villager element/personality types:
    https://palia.wiki.gg/wiki/Villagers/Element_Types
-   Palia Official Wiki --- NPC category:
    https://palia.wiki.gg/wiki/Category:NPC
-   Palia Official Wiki --- Auni: https://palia.wiki.gg/wiki/Auni
-   Palia Official Wiki --- Auni biography:
    https://palia.wiki.gg/wiki/Auni/Biography
-   Palia Official Wiki --- Ashura: https://palia.wiki.gg/wiki/Ashura
-   Palia Official Wiki --- Najuma: https://palia.wiki.gg/wiki/Najuma
-   Palia Official Wiki --- Subira: https://palia.wiki.gg/wiki/Subira
-   Paliabase --- Villagers and roles: https://paliabase.com/villagers
-   Kokoro 82M model card --- language and voice ID list:
    https://huggingface.co/hexgrad/Kokoro-82M
-   Kokoro voice catalogue --- voice IDs and subjective training-data
    quality notes:
    https://huggingface.co/hexgrad/Kokoro-82M/blob/main/VOICES.md

------------------------------------------------------------------------

# 10. Document status

  -----------------------------------------------------------------------
  Area                                Status
  ----------------------------------- -----------------------------------
  Named Villager roster collected     Done for the 29-entry Villager
                                      category snapshot

  Broad 104-entry NPC category fully  Not done
  profiled                            

  Gender/species/age for every        Partial; unknowns explicitly marked
  villager verified                   

  Proposed Kokoro defaults            Drafted

  Kokoro voices auditioned            Not done

  Duplicate voice assignments         Not done
  resolved                            

  In-game pronunciation testing       Not done

  JSON runtime profiles implemented   Roster loads from npc-voices.json.
                                      Recommended voice IDs here were
                                      not applied

  NPC detection linked to profiles    Not done

  Production voice map approved       Not done
  -----------------------------------------------------------------------

**Final rule:** Until the audition and approval workflow is complete,
all voice IDs in this document are `proposed defaults`, not final
approved casting.
