# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
