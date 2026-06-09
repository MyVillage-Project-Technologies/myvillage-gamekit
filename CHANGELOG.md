# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
