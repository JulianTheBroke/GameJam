# GameJam — Connect Sandbox

Unity **6000.6.0f1** — open `Assets/Scenes/SampleScene.unity` and press Play.

2-player platformer sandbox built around a **tether connection**. Linked = coordinate together (Portal 2 vibe). Broken apart = split screen and explore independently (It Takes Two vibe).

## Controls

| | P1 | P2 |
|---|---|---|
| Move | WASD | Arrows |
| Jump | Space | Enter |

## Gameplay loop

1. **Stay linked** — shared camera, tether-relative movement, slower but coordinated
2. **Solve together** — both stand on pressure plates to open doors
3. **Stretch too far** — tether breaks, split screen, faster independent movement
4. **Reconnect** — get close again to link back up

### Linked movement
- Forward/back = along the tether
- Left/right = across the tether
- Best for positioning on plates and moving as a pair

### Split movement
- Normal camera-relative controls
- Faster speed + better air control for solo platforming

## Scene hierarchy

```
GameManager      ← input + tether
Players
  Player1
  Player2
CameraRig
Level
  Ground / Platform
  PuzzleDemo       ← example coop door (both plates required)
Directional Light
```

## Scripts

| Script | Purpose |
|---|---|
| `GameManager` | Wires input + connection |
| `PlayerConnection` | Tether, link/break, pull |
| `PlatformerController` | Movement (linked vs split) |
| `DynamicSplitCamera` | Merge / split cameras |
| `PressurePlate` | Turns on when a player stands on it |
| `CoopDoor` | Opens when all linked plates are held |

## Building puzzles

1. Duplicate `Plate_P1` / `Plate_P2` under `Level`
2. Add a cube with `CoopDoor`, assign the plates in Inspector
3. Toggle `Require Linked` if the door should only work while connected

`Assets/Prefabs/Player.prefab` is there if you need extra players.
