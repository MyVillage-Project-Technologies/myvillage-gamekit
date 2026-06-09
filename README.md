# MyVillage GameKit

Unity SDK for building games on the **M-UNI Universe** platform.

GameKit provides the base classes, mission templates, and editor tooling you need to ship a Unity game to M-UNI. You write your gameplay using GameKit's building blocks; the M-UNI host loads your bundle, runs the lifecycle, and handles sessions, scoring, and rewards.

**Status:** v1.0.0-alpha.1 — API surface frozen; full template implementations land during M1 build-out.

---

## Why GameKit exists

Unity AssetBundles ship scenes, prefabs, and assets — but they can't ship custom C# code. To run a new game in M-UNI without a new app-store release, every script the game references must already be compiled into the host. GameKit is that pre-compiled set of pieces.

In return for using GameKit, your game:

- Ships through the [MyVillage CLI](https://github.com/MyVillage-Project-Technologies/MyVillageOS-CLI) without an M-UNI release
- Plugs into the platform's session, scoring, and reward systems automatically
- Gets a consistent pause / HUD / results UX out of the box

---

## Installation

You normally don't install this manually — the MyVillage CLI scaffolds a project with GameKit pre-installed:

```bash
npm install -g @myvillage/cli
myvillage login
myvillage create-game   # pick: Engine = Unity
```

If you want to add it to an existing Unity project, open `Packages/manifest.json` and add:

```json
{
  "dependencies": {
    "com.myvillage.gamekit": "https://github.com/MyVillage-Project-Technologies/myvillage-gamekit.git#v1.0.0-alpha.1"
  }
}
```

Requires **Unity 6000.0** or later.

---

## Quickstart

A minimal mission:

```csharp
using MyVillage.GameKit;

public class HelloMission : MissionBase
{
    protected override void OnBegin()
    {
        // Run for 5 seconds, then finish with a fake score.
        Invoke(nameof(Finish), 5f);
    }

    void Finish()
    {
        CompleteMission(finalScore: 100, correctAnswers: 1);
    }
}
```

1. Add this script to a GameObject in your scene.
2. Save the scene as `Assets/Scenes/HelloMission.unity`.
3. Build a bundle: **MyVillage → Build Mission Bundle**.
4. Deploy: `myvillage deploy`.

Your mission appears in M-UNI after admin approval.

---

## Core concepts

### `MissionBase`

The base class every mission extends. Provides a lifecycle (`Initialize → Begin → Complete/Fail`) and helpers for reporting scores, pausing, and ending the mission.

### `IMissionHost`

The contract the M-UNI host implements. Missions call `Host.LogEvent(...)`, `Host.RequestPause()`, etc. Missions never implement `IMissionHost` themselves — the host injects an instance during `Initialize`.

### `MissionConfig`

`ScriptableObject` base for data-driven missions. Subclass it to declare your game's content — quiz questions, level layouts, card sets — and ship the `.asset` in your bundle. Many games need no custom C# at all if a built-in template (`QuizMission`, `MatchMission`) matches their shape.

### Bundle convention

- One bundle = one Unity scene = one mission
- The scene contains exactly one GameObject with a `MissionBase` subclass component
- All script references in the scene must resolve to types in `MyVillage.GameKit` or Unity built-ins. The CLI's preflight rejects bundles with custom scripts.

---

## Built-in mission templates

Use these directly when they fit — no C# needed beyond filling in a config asset.

| Template | Config asset | Best for |
|---|---|---|
| `QuizMission` | `QuizMissionConfig` | Multiple-choice quizzes, trivia, vocab drills |
| `MatchMission` | `MatchMissionConfig` | Pair matching, memory games |

More templates (`RunnerMission`, `TimingMission`) ship in v1.1.

---

## Samples

The package ships discoverable samples via the Unity Package Manager UI:

- **Hello Mission** — the absolute minimum mission, verifies your project is wired up
- **Quiz Sample** — a complete quiz built with `QuizMission` + a config asset

Open Package Manager → MyVillage GameKit → Samples → Import.

---

## Lifecycle reference

```
   Idle  ──Initialize()──▶  Initialized  ──Begin()──▶  Running  ──CompleteMission()──▶  Completed
                                                            │
                                                            ├──FailMission()──▶  Failed
                                                            │
                                                            └──Host.RequestPause()──▶  Paused
                                                                      │
                                                                      └──Host.RequestResume()──▶  Running
```

| Hook | When | Override to... |
|---|---|---|
| `OnInitialize` | After scene loads, before gameplay | Wire references, prepare UI |
| `OnBegin` | When the host hands off control | Start gameplay |
| `OnPause` | When the host or player pauses | Suspend timers and input |
| `OnResume` | When pause ends | Resume from suspended state |
| `OnEnd` | Before terminal handoff (complete / fail) | Cleanup |

---

## Versioning

Semantic versioning. The M-UNI host declares which SDK versions it supports. Your deploy is rejected if the host can't run your bundle:

- **Patch** (1.0.x): bug fixes, no API changes
- **Minor** (1.x.0): additive API, fully backward compatible
- **Major** (x.0.0): breaking — requires an M-UNI host release before your games run

We treat v1.x as a long-lived contract. The CLI tells you when a newer SDK is available.

---

## Reporting issues

File issues at [github.com/MyVillage-Project-Technologies/myvillage-gamekit/issues](https://github.com/MyVillage-Project-Technologies/myvillage-gamekit/issues).

For platform-wide questions (CLI, M-UNI host, account access), see [myvillageproject.ai/developers](https://www.myvillageproject.ai/developers).

---

## License

Proprietary. See [LICENSE](./LICENSE). Use is permitted for building games for the M-UNI Universe platform.
