# OLOMU SURVIVAL — PROJECT BRAIN FILE
_Last updated: night session, Aug 22 2026. Read this first._

## WHAT THIS PROJECT IS
Westland-Survival-style 3D third-person survival RPG set in Olomu, Delta State, Nigeria (Urhobo culture).
Unity 6000.3.22f1, Built-in RP, Android-first (Redmi Note 11 Pro). Ported from frozen Godot reference.
Owner: mkmulla19-boop (GitHub). Studio name: Mkmulla Game Studio.

## KEY PATHS
- Unity project: C:\Users\ASUS PC\Documents\Unity Projects\Olomu Survival
- GitHub repo: github.com/mkmulla19-boop/Olomu-Unity (public, master)
- Design refs: Documents\Olomu Design References\ (+ Downloads\olomu idea.jpeg)
- Character source GLB: Documents\Codex\2026-08-18\hey-codex\olomu-survival\assets\characters\olomu_player\
- Promo videos: Documents\Olomu-Survival-Promo\
- Promo frames: C:\ProgramData\olomu_promo

## COMMANDS THAT WORK
- Unity batch: $args='-batchmode -quit -nographics -projectPath "C:\Users\ASUS PC\Documents\Unity Projects\Olomu Survival" -executeMethod BuildScript.BuildAndroidTest -logFile "C:\ProgramData\olomu-X.log"'; Start-Process "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe" $args pattern (pre-quote args!)
- BuildAndroidTest = scene auto-build if missing + branding + APK to Builds\OlomuSurvival-test.apk
- BuildWindowsPromo = standalone exe for self-recording promo capture
- Delete Assets\Scenes\OlomuVillage.unity to force scene rebuild (all world gen is code: Assets\Editor\OlomuSceneBuilder.cs)
- adb: %LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe (device VWJZX4PRU86549WK; flaky — retry, check USB debugging + File Transfer mode)
- ffmpeg (NO png/aac decoders!): "C:\Program Files\BlueStacks_nxt\ffmpeg.exe" — use BMP inputs, libopenh264 encoder, wav audio
- Blender headless: "C:\Program Files\Blender Foundation\Blender 5.2\blender.exe" -b --python script.py (EEVEE broken headless; use CYCLES CPU; numpy OK)
- Gemini vision API WORKS with user key (model gemini-flash-latest ONLY; others 404). Key stored in chat history only — ask user.
- Harness quirk: commands randomly fail with ChildProcess.kill — just retry same command.

## CURRENT STATE (all pushed to git cacff3a+)
- PLAYABLE APK installed on phone. Cinematic entrance (Title→Establish→Life→Chaos→Father→Escape→Handoff) w/ original Afrobeats audio bed.
- Combat: EnemyAI (patrol/chase/attack), player Health, ATTACK button, loot drops. Raiders convert from cinematic actors to live enemies at handoff OR skip.
- World: Blender huts/palms/rocks FBX, earth palette per design brief, fog, warm sun.
- Character v3: male warrior per forensic brief (1.8m, broad shoulders, cream tank #E8E0D5, red sash #A32D2D, cargo #5C5346, boots #3E2B1F, skin #6B3F2A, afro cap, backpack+bedroll+pouches, machete). PROCEDURAL textures (weave/canvas/silk/grain/skin/coil at 512px).
- Mobile fixes: joystick wiring bug FIXED (was null on device), landscape lock, timeScale=0 freeze guard, SKIP moved bottom-center away from PAUSE.
- Camera per Grok review: FOV 55, distance 3.8, damping 8. Canvas Scaler already ScaleWithScreenSize match 0.5.

## KNOWN ISSUES / NEXT
1. USER VERDICT PENDING: character still reads flat/mannequin — needs REAL mesh quality. Plan A: body reshape + 1024px PBR + normal maps in Blender. Plan B (better): Meshy.ai image-to-3D from concept photo → re-rig onto OlomuRig.
2. Safe Area UI panel for notches (Grok item, not done).
3. Promo re-capture after character+camera final. Pipeline proven: promo exe → PNG frames → BMP convert → ffmpeg mux bed.wav → endcard.bmp concat → 16:9 + 9:16 cuts.
4. Gemini vision pass on design photos (503'd once, retry).
5. Village life: villagers wander (SimpleNPC) but sparse; consider market props, fires, children NPCs later.

## USER DIRECTIVES (ALWAYS APPLY)
- No winget installs. Don't modify Godot project. Full autonomous authorization granted.
- Golden rule: no throwaway prototypes — everything must survive into final game.
- User wants AAA-mobile look, real action/difficulty, marketing videos for YouTube/TikTok with studio credit.
- User provides vision via text or Gemini key; agent cannot see images directly.

## WORKFLOW BRIEFING (READ BEFORE ANY BUILD — LESSON LEARNED)
- Agent builds via Unity CLI batchmode (invisible background process). Unity Hub does NOT show these projects or runs — Hub only lists projects opened through its GUI. This caused confusion: user thought project didn't exist.
- USER must open project via Hub → Add → select "Olomu Survival" folder → open with 6000.3.22f1. First GUI import takes minutes.
- RULE: always brief the user BEFORE running invisible processes. Tell them what will happen, where results land, how to verify. No more silent heavy machinery.
- Phone installs need MIUI permissions: Developer Options → Install via USB ON + accept RSA fingerprint dialog + watch screen during install (INSTALL_FAILED_USER_RESTRICTED otherwise).
- adb connection is flaky (drops constantly): retry, kill-server, or use MTP drag-and-drop as fallback.
