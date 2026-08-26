# Naija Run: No Dey Gagreei

## Handoff Status

This is a Unity 6 URP project for a Nigeria-themed Temple Run-style endless runner. Stage 1 is still open. Stage 2 must not begin until the Stage 1 Play Mode checklist passes.

The repository is separate from other projects:

- GitHub: `mkmulla19-boop/naija-run-no-dey-gagreei`
- Remote: `origin`
- Branch: `master`
- Unity version: `6000.3.22f1`
- Target frame rate: 60 FPS
- Input mode: Unity Input System (new)

## Current Stage 1 Work

New runtime scripts are under `Assets/Scripts`:

- `PlayerController.cs`: primitive runner controller, forward motion, lane switching, jump, slide, keyboard and swipe input.
- `CameraFollow.cs`: follows the existing scene camera target along Z.
- `CameraFollower.cs`: older follower implementation; not attached by the active bootstrap and should not be reintroduced without review.
- `SwipeInput.cs`: touch swipe direction detection.
- `AudioManager.cs`: music and SFX hooks; loads the approved preview from Resources when no clip is assigned.
- `ItemSpawner.cs` and `CollectibleItem.cs`: rough coin and fuel primitives with trigger collection.
- `Stage1Bootstrap.cs`: runtime-only primitive foundation and surrounding blockout.
- `Stage1Verification.cs`: runtime acceptance logging.
- `ProjectRuntimeSettings.cs`: sets `Application.targetFrameRate = 60` and disables VSync.

Editor setup:

- `Assets/Editor/SetupProjectFolders.cs` adds the requested folder-generation menu at `Tools/NaijaRun/Generate Folder Structure`.

The existing `Assets/Scenes/SampleScene.unity` is intentionally not rewritten. At runtime, `Stage1Bootstrap` runs after scene load, reuses the existing tagged `Main Camera`, and generates test geometry.

## Verified Coordinates

- Unity scale: 1 unit = 1 metre.
- Player root: `(0, 0, 0)`.
- Lane centers: `X = -3, 0, 3`.
- Playable road boundary: `X = -4.5` to `X = 4.5`.
- Player controller: height `1.8`, radius `0.35`, center Y `0.9`.
- Jump height: `2.2`; gravity magnitude `30`; jump velocity approximately `11.489125`.
- Slide height: `0.9`; slide center Y `0.45`; duration `0.8` seconds.
- Forward speed: `10` m/s.
- Camera start: position `(0, 3.5, -6)`, rotation `(18, 0, 0)`, FOV `60`.
- Camera follow offset: `(0, 3.5, -6)`, smooth speed `10`.
- Sidewalks: X `-5.5` and `5.5`.
- Market blockouts: X `-7` and `7`.
- Signs: X `-8.5` and `8.5`.
- Palms: X `-10` and `10`.
- Buildings: X `-12` and `12`.
- Test crate: `(0, 0.75, 42)`.
- Test barricade: `(3, 0.6, 66)`.
- Test coin: `(0, 1, 12)`.
- Test fuel: `(3, 1, 24)`.

## Audio

The approved preview is at `Assets/Resources/Audio/NaijaRun_voice_preview.mp3`. It is a separate mix made from the user-provided song and voice recordings, with voice sections at the beginning, middle, and end and the backing song reduced beneath them.

Original recordings remain outside the repository in the user's Music folder:

- `mk voice.aac`: replacement voice.
- `vc.mpeg`: original mixed song.

The original files must remain untouched. The preview has been approved by the user for Unity testing. No claim is made that the original mixed vocal was perfectly removed; the preview uses ducking under the replacement voice.

## Asset and Design Rules

- No paid assets.
- No generic stock character as the final Efe character.
- No final character work in Stage 1.
- Exact visual environment assets are not currently present.
- Current scenery is a primitive blockout only: sidewalks, stalls, canopies, palms, signs, lamps, buildings, lanes, crate, and barricade.
- Future environment assets must be checked for source, license, size, Unity/URP compatibility, scale, pivot, and collision before import.
- Decorative scenery stays outside the road; lane objects are intentional obstacles or collectibles.

## Remaining Stage 1 Gate

The user must run Unity Play Mode and verify:

1. Exactly one Main Camera is active.
2. Player remains visible while moving forward.
3. Camera follows Player_Efe along positive Z.
4. A/D or Left/Right switches between the three lanes without overshoot.
5. Space jumps.
6. S or Down Arrow slides and restores the collider.
7. Crate and barricade are visible and collide correctly.
8. Coin and fuel are on lane centers and collect with the Player tag.
9. Approved audio plays.
10. Console contains no red Stage 1 errors and the verification messages pass.

Do not mark the live test passed based only on static compilation. Automated batch validation was previously blocked by the already-open Unity editor, so the in-editor Play Mode result is still required.

## Next Action

Wait for the user's Play Mode report. Repair only Stage 1 issues found in that report. Do not begin Stage 2, final character modeling, final environment art, progression, or additional downloads before Stage 1 is explicitly accepted.