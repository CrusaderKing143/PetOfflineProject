# Pet Offline

Pet Offline is a two-level, Editor-playable Unity teaching project built for
Unity **2021.3.8f1**, the **Built-in Render Pipeline**, and the **Legacy Input
Manager**. A first playthrough is designed to take roughly 12–15 minutes.

The project keeps `StartPanel` loaded as the persistent shell. `Main1` and
`Main2` are loaded additively and contain world objects only.

## Open and play

1. Open the project with Unity `2021.3.8f1`.
2. If the generated scenes or data need to be refreshed, run
   `Pet Offline > Install Playable Prototype` once.
3. Open `Assets/Scenes/StartPanel.unity`.
4. Enter Play Mode and use the English title menu.

Do not start Play Mode from `Main1` or `Main2`; they intentionally have no
Camera, AudioListener, EventSystem, or Canvas.

## Controls

| Input | Action |
| --- | --- |
| `W / A / S / D` | Move along the isometric axes |
| `E` | Pick up, drop, interact, or advance dialogue |
| `Space` | Bark |
| Hold `Shift` | Lie down / sunbathe in a sun zone |
| `Q` | Short dash while empty-handed |
| `Esc` | Pause or resume during world gameplay |

The dash lasts about `0.25s`, uses `2.5x` movement speed, and has a `1s`
cooldown. It is disabled while carrying, lying down, sliding, in an automatic
performance, paused, or while input is locked.

## Story flow

- **Day 1 / Main1:** move the shoes to Camera A, return the pillow, answer boss
  calls, avoid Camera B while carrying the current task item, then bark from
  Latte's bed. Continuing the report starts an in-world ending performance;
  Day 1 is saved only after that performance finishes.
- **Day 2 / Main2:** complete the first sun session, respond at the feeder,
  redirect the robot with a banana, learn about the backup camera, and finish
  the living-room sun session. The report leads to `Restore Connection` or
  `Keep Quiet`, and progress is saved only after the selected ending finishes.

`Continue` is disabled until Day 1 is complete. After that it always starts
Day 2 from its opening, including after the story has already been completed.

## Save rules

Story progress is stored as versioned JSON in `PlayerPrefs` under
`PetOffline.StorySave`. It stores only:

- Day 1 completion;
- Day 2 completion;
- the final choice.

It never stores an in-level position or partial task state. `Return Title`
keeps progress. Confirmed `New Game` and ending `Restart` clear it.

## Architecture

Runtime code is split into five assemblies:

- `PetOffline.Core`: contracts, `GameSession`, additive scene flow, input, and
  versioned story persistence;
- `PetOffline.Gameplay`: player/world components, Day 1 and Day 2 flow state,
  runtime orchestration, and ScriptableObject configuration types;
- `PetOffline.UI`: ViewModel-only presenters and high-level commands;
- `PetOffline.Editor`: repeatable scene/data installation and validation;
- `PetOffline.Tests.Editor`: Edit Mode flow and persistence tests.

Gameplay and UI both depend on Core and do not reference one another.

## Validation and tests

- Run `Pet Offline > Validate Project` to check scene order, assembly
  dependencies, required references, the unique persistent Camera and
  AudioListener, world-scene isolation, missing scripts, camera origins,
  robot patrol clearance, and forbidden full-background animation dependencies.
- Open `Window > General > Test Runner`, select **EditMode**, and run all tests.
- Use a `1920x1080` or other 16:9 Game view when visually checking colliders,
  sight lines, foreground sorting, and UI layout.

Before handing off a playable revision, run this Play Mode smoke pass:

1. Confirm the title video loops with its own audio, then double-click each
   title/report/choice button and verify only one command runs.
2. Disable and re-enable `UIRoot` during gameplay; the world must keep moving
   and the restored UI must redraw the current objective and progress.
3. Complete Day 1, including a Camera B local reset and a boss call; verify the
   report does not save or load Day 2 until the in-world performance finishes.
4. Complete Day 2 through the banana/robot impact, backup-camera lesson, final
   sun session, and both endings; verify each ending saves only after it ends.
5. Return to title and enter each level again to confirm pause state, dialogue,
   event subscriptions, robot paths, and camera alerts do not accumulate.

The three Build Settings entries must remain in this order:

1. `Assets/Scenes/StartPanel.unity`
2. `Assets/Scenes/Main1.unity`
3. `Assets/Scenes/Main2.unity`

## Known limits

- Editor play only; no Windows build is included.
- English only; no localization pipeline is included.
- No mid-level save, manual object pushing, NavMesh, Cinemachine, Input System,
  external font, or external gameplay art/audio.
- The opening video's own audio is the only audio source. Gameplay is silent.
- Full-size 240-frame background animation is deliberately excluded. Both
  levels use static backgrounds plus independent animated mechanisms to avoid
  Editor memory spikes.
- Collider shapes, sight cones, and animation direction mapping are calibrated
  for the current artwork and fixed orthographic 16:9 view.
