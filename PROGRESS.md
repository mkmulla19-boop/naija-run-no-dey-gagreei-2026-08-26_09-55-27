# OLOMU SURVIVAL — PROJECT BRAIN FILE
_Last updated: Aug 23 2026 (placement-fix session). Read this first — it is the handover document for any model/agent._

## WHAT THIS PROJECT IS
Westland-Survival-style 3D third-person survival RPG set in Olomu, Delta State, Nigeria (Urhobo culture).
Unity 6000.3.22f1, Built-in RP, Android-first (Redmi Note 11 Pro). Ported from frozen Godot reference.
Owner: mkmulla19-boop (GitHub). Studio name: Mkmulla Game Studio.

## ⚠️ KEY PATHS (UPDATED AUG 23 — OLD PATHS ARE DEAD)
- **Unity project ROOT: `C:\Users\ASUS PC\Documents\Olomu-Survival-Promo`** ← the folder itself IS the project (Assets/ProjectSettings/Library at root)
- OLD path `Documents\Unity Projects\Olomu Survival` = DELETED. Do not reference.
- GitHub repo: github.com/mkmulla19-boop/Olomu-Unity (public, master)
- Design refs: `Documents\Olomu Design References\`, `Documents\Olomu-Reference\`
- Grok patch zip: `Downloads\Olomu_Full_Setup.zip` (evaluated — see below)
- Promo videos: inside project root (3 mp4 files, harmless to Unity)
- D:\ drive = phone-style storage (Music/DCIM) — ZERO Unity content, never touch
- OneDrive\Documents = 2 word docs only, irrelevant

## 🔑 UNITY HUB REGISTRATION (solved Aug 23 — repeat if ever lost)
Symptom: Hub shows empty project list / "open a project that has Unity" error.
Root cause found: Hub's database table `projects` was EMPTY.
Fix method:
1. Close Hub fully (`Stop-Process -Name 'Unity Hub' -Force`)
2. Use sqlite3 bundled with Unity:
   `"C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\sqlite3.exe"`
3. DB: `C:\Users\ASUS PC\AppData\Roaming\UnityHub\hub.db`, schema:
   `projects(path TEXT PRIMARY KEY, data TEXT NOT NULL, updated_at INTEGER NOT NULL)`
4. INSERT row with minimal JSON `{"isFavorite":true,"lastOpened":<ms>,"name":"..."}` → on next launch Hub ACCEPTS and auto-enriches it (version, buildTarget Android, git info).
5. PowerShell inline SQL mangles quotes — write .sql file and pipe: `Get-Content fix.sql -Raw | & sqlite3.exe hub.db`
6. Known transient error when opening card mid-sync: `ERROR.EDITOR_ALREADY_IN_LIST` — full restart of Hub clears it.
DB backup kept at: `Temp\opencode\hub_backup.db`

## COMMANDS THAT WORK
- Editor direct launch (bypasses Hub): `Start-Process "C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Unity.exe" -ArgumentList '-projectPath','"C:\Users\ASUS PC\Documents\Olomu-Survival-Promo"'`
- Batch build (when data allows): same exe with `-batchmode -quit -nographics -projectPath <above> -executeMethod BuildScript.BuildAndroidTest -logFile "C:\ProgramData\olomu-X.log"` (pre-quote args!)
- Scene rebuild: menu **Olomu → Build Village Scene** (all world gen is code: `Assets\Editor\OlomuSceneBuilder.cs`). Delete `Assets\Scenes\OlomuVillage.unity` to force fresh rebuild.
- adb (works): `"C:\Program Files\Unity\Hub\Editor\6000.3.22f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"` device VWJZX4PRU86549WK; flaky — kill-server + retry; MIUI needs Install-via-USB permission.
- Zip tool: `tar.exe -a -c -f out.zip -C parent foldername` (fast, handles GBs; used for 678MB full-project zip pushed to phone Documents).
- Python: BROKEN on this machine (WindowsApps stub). Need SQL?→ Unity's sqlite3.exe. Need zip?→ tar.exe.
- Harness quirk: commands randomly fail with ChildProcess.kill — retry the SAME command.

## SESSION LOG — AUG 23 2026 (placement surgery)
User diagnosis (correct): game logic perfect, PLACEMENT MATH was the disease.
Audit method: distance math between every actor spawn/waypoint vs structure footprints (hut r≈2.2–2.5m, fire stones r=1.4 no-collider, piles box/sphere ~0.75m).

Violations found & FIXED in `Assets\Editor\OlomuSceneBuilder.cs`:
1. Father spawned INSIDE Hut#3: gap 0.5m (needed ~3.8m). FIX: spawn (-1.5,1.05,13.5), waypoints (-1,10.5)→(0.8,9.2) straight clear corridor.
2. Raider cutscene mids passed through campfire flame (0.7m). FIX: all 5 mids rerouted to ≥2.9m ring around fire(1.5,1.5).
3. Wilderness scatter had NO clearance check → trees/rocks could spawn on huts/paths/each other. FIX: new `SpotClear()` + `occupied` registry (huts/fire/piles self-register), 2.5m min gap, guards raised 800→2000 / 400→1200, path corridors excluded (river path z∈[0.2,3.8] x<-4... see SpotClear code).

FIXED in same file:
4. **Ground had NO collider** (Prim() strips colliders from ALL primitives incl. Ground!) → `PlaceOnGround()` raycast hit nothing = THE SINKING BUG ROOT CAUSE. FIX: `ground.AddComponent<MeshCollider>()` after Prim.

FIXED in `Assets\Scripts\ThirdPersonController.cs` (Grok camera values ported):
5. cameraDistance 4.5→**5.0**, headHeight 1.6→**1.45**, NEW shoulderOffsetX **0.45** applied to cam localPosition.x, cameraSmoothness 8→6 (Grok damping feel), FOV 55 unchanged.

## GROK PACKAGE EVALUATION (Downloads\Olomu_Full_Setup.zip) — DO NOT IMPORT FILES AS-IS
| File | Verdict |
|---|---|
| CameraSetupHelper.cs | ☠️ requires Cinemachine package (not installed) — would break compilation of ALL scripts. VALUES were ported manually instead. |
| EnvironmentColliderFixer.cs | redundant + inferior (default-size boxes vs our AddBoundsCollider bounds-fit). Skip. |
| SimpleInteract.cs | keyboard-E only, dead code on phone (we have touch GATHER + Interactor). Skip. |
Extraction copy: `Temp\opencode\fullsetup\Olomu_Full_Setup\`

## CURRENT STATE (Updated: Universal Cross-Platform & Modern Camera Session)
- **Universal Dual-Input Engine (Phone + PC Hybrid)**:
  - **Phone/Android**: Full touch virtual joystick, right-screen orbit drag, on-screen action pads, and real-time Safe-Area adaptation (supporting 16:9, 18:9, 19.5:9, 20:9 notches/punch-holes like Redmi Note 11 Pro).
  - **PC/Desktop Test-Run**: Full native keyboard + mouse mapping (`WASD` movement, `Space` jump, `E`/`F` gather/attack/interact, `1`/`H` eat, `2`/`J` drink, `Esc`/`P` pause/resume, Mouse look via right-click or right-side click).
  - Adaptive prompt labels showing keyboard shortcut hints `[E]`, `[SPACE]`, `[1]`, `[2]` on PC while keeping clean touch labels on Mobile.
- **Modern Camera System ("Latest Model Game View")**:
  - Spherecast obstacle collision avoidance (smoothly pushes camera forward if trees/huts block view, eliminating wall-clipping).
  - Dynamic Sprint FOV expansion (55° → 61° on running).
  - Over-the-shoulder third-person framing (shoulder offset 0.45m, head height 1.45m).
  - Universal mobile-standard 60 FPS target (`Application.targetFrameRate = 60`).
- **Blender 5.2 GPU Fixed**: Windows DirectX User GPU Preferences configured (`GpuPreference=2;`) for dedicated NVIDIA execution.
- **Settings Panel Integrated**: Master, Music, and SFX/Ambience sliders added to Pause Menu and dynamically wired to `AudioDirector.cs` with `PlayerPrefs` persistence.
- **Actors Restructured (Capsules Eliminated)**:
  - Father & Villagers: Now instantiate rigged character models (`olomu_player_male.fbx`) with animators and natural height scaling.
  - Raiders & Patrols: Now instantiate warrior models (`olomu_ai_warrior.fbx`) with running/combat animation states and `EnemyAI`.
  - Wild Dogs: Restructured into articulated anatomical quadruped models (torso, neck, head with snout/ears, four legs, tail).
- **Compilation**: All C# scripts compile with **0 errors** on Unity 6 (`6000.3.22f1`).

## KNOWN ISSUES / NEXT QUEUE
1. **In-Editor Scene Bake**: Open project in Unity Editor and select **`Olomu → Build Village Scene`** to bake all the updated models, settings UI, and clear actor corridors into `OlomuVillage.unity`.
2. **Promo Re-Capture / APK Build**: Build the updated APK when ready to test the new visual models and audio settings directly on the Redmi Note 11 Pro.
3. **Village Life Expansion**: Traditional African props (clay pots, woven baskets, yam storage barns, drying racks).

## USER DIRECTIVES (ALWAYS APPLY)
- **LOW DATA MODE: NO app builds, NO downloads, NO large uploads unless explicitly asked.**
- No winget installs. Don't modify Godot project. Full autonomous authorization otherwise.
- Brief user BEFORE any invisible/heavy process. Say what happens, where results land, how to verify.
- Golden rule: no throwaway prototypes — everything survives into final game.
- Phone adb drops constantly: retry pattern = kill-server → devices → retry command.
