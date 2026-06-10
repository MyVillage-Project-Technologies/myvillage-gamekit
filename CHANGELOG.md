# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0-alpha.4] — 2026-06-10

Multi-platform build support in the editor. Pairs with @myvillage/cli
v1.60.0 (single `myvillage deploy` ships every built platform) and the
ed-platform's per-platform artifact storage.

### Added
- **`MyVillage → Build All Platforms`** menu item. Builds AssetBundles
  for iOS, Android, and WebGL in sequence, dropping each into
  `builds/<platform>/`. Switches the active build target as needed and
  restores it when done.
- **Shortcut**: `Cmd/Ctrl+Shift+B` is bound to `Build Mission Bundle`
  (current platform) for fast iteration.

### Changed
- `BundleBuilder` is now a real implementation, not a placeholder. The
  previous version popped a "lands in M1" dialog; this version actually
  invokes `BuildPipeline.BuildAssetBundles` with the GameKit convention
  (every `.unity` file under `Assets/Scenes/` becomes a bundle named
  after the filename, lowercased).

## [1.1.0-alpha.3] — 2026-06-09

Scaffold UX fix. Closes the "press Play and nothing happens" gap that
hit devs after `myvillage create-game` because no GameObject had a
MockMissionHost attached.

### Added
- `Editor/StarterSceneCreator.cs` — menu item
  **`MyVillage → Create Starter Mission Scene`**. Builds a fresh scene
  with a "Mission" GameObject carrying MockMissionHost, auto-attaches
  the dev's MissionBase subclass if exactly one is found in the
  project, saves the scene, and adds it to Build Settings.

This is the lever the CLI scaffold now points at — see CLI v1.59.0.

### Notes
- Pairs with `MyVillageOS-CLI` v1.59.0, which updates the scaffolded
  `MyMission.cs` and README to tell the dev to click the new menu item
  as step 1 after opening the project in Unity.

## [1.1.0-alpha.2] — 2026-06-09

Part 1 of GameKit M3: in-editor preview. Lets devs play their mission
end-to-end from Unity's Play button — no bundle build, no deploy, no
waiting for review.

### Added
- `MockMissionHost` MonoBehaviour (`Runtime/MockHost/MockMissionHost.cs`).
  Drop on any GameObject in your mission scene; on Play, it finds the
  scene's `MissionBase`, implements `IMissionHost` against it, injects
  itself as the Object-scope `missionHost` variable for Visual Scripting
  Custom Units, calls `Initialize` then `Begin`, and shows a real
  `ResultPanel` when the mission completes. Optional debug overlay
  shows host event flow at the bottom of the screen.
- AddComponentMenu entry: "MyVillage/Mock Mission Host".

### Notes
- Mock host has Inspector fields for `MissionConfig`, `gameSlug`,
  `missionId`, `autoBegin`, and `showDebugOverlay`. Sensible defaults so
  it works without any configuration if your mission doesn't read config.
- Safe to leave attached in shipped bundles — the real host's
  `MissionHostAdapter` overwrites the `missionHost` variable so the
  mock never runs in production. Preflight allows it because the class
  lives in `MyVillage.GameKit` (already allowlisted).
- Part 2 of M3 (standalone GameKit Player app) ships in a separate repo.

## [1.1.0-alpha.1] — 2026-06-09

First Visual Scripting integration alpha. Lets community developers ship
games built as node graphs without writing C#. Companion to GameKit M2
on the platform side (preflight extension, backend allowlist update, host
runtime integration).

### Added

- New assembly **`MyVillage.GameKit.VisualScripting`** under
  `Runtime/VisualScripting/`. References Unity.VisualScripting.Core +
  Unity.VisualScripting.Flow + MyVillage.GameKit. Locked-down asmdef
  (overrideReferences, no unsafe code).
- **5 host Custom Units** under `MyVillage/Mission` category in the VS
  fuzzy finder:
    - `Complete Mission` — calls IMissionHost.CompleteMission
    - `Fail Mission` — calls IMissionHost.FailMission(reason)
    - `Report Progress` — calls IMissionHost.ReportProgress
    - `Request Pause` — calls IMissionHost.RequestPause / Resume
    - `Log Event` — calls IMissionHost.LogEvent(name, props)
- **3 config-reader Custom Units** under `MyVillage/Config`:
    - `Read Config (String) / (Int) / (Float)` — reflective read of public
      fields on the active MissionConfig. Safe alternative to a raw
      GetMember node.
- `HostResolver` helper — every unit reads IMissionHost from the
  Object-scope graph variable `"missionHost"` (set by the host at bundle
  load). Returns null when running outside the host (e.g. editor preview)
  so graphs don't throw; logs a one-time warning.

### Notes
- No changes to existing `MyVillage.GameKit` runtime API. M1 code
  compiles unchanged.
- The 4 Visual Scripting reflection nodes (`InvokeMember`, `GetMember`,
  `SetMember`, `Expose`) are blocked at preflight time — see the M2
  design doc on the host repo for details.

## [1.0.0-alpha.4] — 2026-06-09

First playable templates and UI widgets.

### Added
- `QuizMission` is now a complete implementation. Configure a
  `QuizMissionConfig` with questions, choices, and per-question timing
  and the mission builds its own UI, runs the round, awards a score,
  and reports the result — no prefab or extra C# required.
- `MatchMission` is now a complete implementation. Configure a
  `MatchMissionConfig` with a grid size and a set of card-face sprites
  and the mission builds the grid, handles flip/match/mismatch, and
  ends when all pairs are matched.
- `MissionHUD` builds a programmatic Canvas with score, timer, title,
  and a pause button wired to `IMissionHost`. Use `MissionHUD.Create(host)`
  for the auto-built version, or attach to a custom Canvas and call
  `Bind()` for a designer-driven version.
- `ResultPanel` builds a programmatic end-of-mission summary panel.
  Use `ResultPanel.Create(host)` then `Show(result, onContinue)`.

### Implementation notes
- Templates use the legacy `UnityEngine.UI.Text` + `Image` to avoid an
  explicit TextMeshPro dependency. v1.1 will swap to TMP for typography.
- UI is built at runtime from C# — devs don't need to create or edit
  prefabs to ship a working mission. Looks plain; functions correctly.
- All textures, fonts, and colors are sourced from Unity built-ins or
  named constants — the SDK ships no binary assets.

### Notes
- No API breaks from alpha.3. Existing code compiles unchanged.

## [1.0.0-alpha.3] — 2026-06-09

Retargets the SDK at Unity 6.3 LTS, replacing the 6.0 LTS baseline.

### Changed
- `unity` field: `"6000.0"` → `"6000.3"`. Minimum supported editor is now
  the newer Unity 6.3 LTS line.
- `unityRelease` field: `"76f1"` → `"12f1"`. Recommended editor version is
  6000.3.12f1 — matches the M-UNI Universe host.

### Notes
- No Runtime/Editor API changes. Source-compatible with alpha.2.
- Existing devs on Unity 6.0 LTS who installed alpha.2 will see an
  incompatibility warning if they upgrade to alpha.3. Either upgrade
  the editor to 6.3 LTS or pin to alpha.2 in `Packages/manifest.json`.

## [1.0.0-alpha.2] — 2026-06-09

Establishes this package as the single source of truth for the target Unity
Editor version. Tools that scaffold projects against the SDK (notably the
MyVillage CLI) read these fields and stay in sync without their own
hardcoded constants.

### Added
- `unityRelease: "76f1"` field in `package.json`. Combined with the existing
  `unity: "6000.0"`, this names the recommended Unity Editor version
  (6000.0.76f1) — the latest Unity 6 LTS at time of release.

### Changed
- No code or API changes. The Runtime/Editor assembly contract is unchanged
  from `v1.0.0-alpha.1`.

## [1.0.0-alpha.1] — 2026-06-09

Initial alpha. Defines the API surface for the v1.0 contract.

### Added
- `MissionBase` abstract MonoBehaviour with lifecycle (`Idle` → `Initialized` → `Running` → `Paused` → `Completed`/`Failed`)
- `IMissionHost` interface — host contract; missions call methods on this
- `MissionConfig` ScriptableObject base
- `MissionResult` and `MissionProgress` data types
- `MissionLifecycleState` enum
- `MissionTimer` utility (pausable, resumable)
- `QuizMission` + `QuizMissionConfig` template (contract; full implementation pending in v1.0)
- `MatchMission` + `MatchMissionConfig` template (contract; full implementation pending in v1.0)
- `MissionHUD` and `ResultPanel` UI placeholders
- `BundleBuilder` editor menu placeholder
- Locked-down asmdef for the Runtime assembly (no engine references beyond `UnityEngine`)
- Sample placeholders: `HelloMission`, `QuizSample`

### Notes
- This alpha is scaffolding only. Mission templates, UI prefabs, and the bundle builder land during M1.
- The API surface defined here is the v1.0 contract — additive changes only until v2.0.
