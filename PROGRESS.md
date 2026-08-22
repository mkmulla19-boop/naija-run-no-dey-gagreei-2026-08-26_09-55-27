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

## CURRENT STATE (pushed to git after this session's commit)
- Playable APK build13 on phone (older than today's fixes — rebuild LATER when user has data).
- Scene file `Assets\Scenes\OlomuVillage.unity` still contains OLD placements → **must run Olomu→Build Village Scene once in editor to regenerate with fixes.**
- Cinematic entrance, combat, loot, survival needs, mobile HUD all intact and untouched.
- Character: olomu_ai_warrior.fbx rigged; clips sourced from olomu_player_male.fbx (animator). meshy/tripo variants exist untested.

## KNOWN ISSUES / NEXT QUEUE (in user's priority order)
1. **SETTINGS PANEL missing** (user said "sitting", means settings): pause panel has only RESUME+SAVE; AudioDirector volumes hardcoded (0.55 ambience etc.). Task: add Settings section to pause panel — music/SFX sliders wired to AudioDirector sources + quality toggle. USER CONFIRMED WANTED, not yet built.
2. Safe-Area UI for notches (Grok item, pending).
3. Promo re-capture AFTER verifying placement fixes in editor (pipeline proven, needs data-free local run OK).
4. Character realism verdict still open (Plan B meshy re-rig exists as olomu_meshy_warrior.fbx).
5. Village life expansion (market props etc.) — later.

## USER DIRECTIVES (ALWAYS APPLY)
- **LOW DATA MODE (Aug 23): NO app builds, NO downloads, NO large uploads unless explicitly asked. Verification happens via Unity Hub GUI locally.**
- No winget installs. Don't modify Godot project. Full autonomous authorization otherwise.
- Brief user BEFORE any invisible/heavy process. Say what happens, where results land, how to verify.
- Golden rule: no throwaway prototypes — everything survives into final game.
- User sends files via chat/phone pipelines that can CORRUPT (duplicated class seen Aug 23) — always verify file integrity on disk before trusting pasted content.
- Phone adb drops constantly: retry pattern = kill-server → devices → retry command.
