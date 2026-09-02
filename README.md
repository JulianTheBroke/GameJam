# GameJam

Unity 6000.6.0f1

Open `Assets/Scenes/SampleScene.unity` and hit Play.

## Controls

P1: WASD + Space  
P2: Arrows + Enter

Gamepads work too.

## What it does

Two players connected by a tether.

- Stay close = one camera, move together
- Stretch the tether too far = it snaps, screen splits in two
- Walk back close = tether reconnects, camera merges again

There's a demo puzzle in the scene: both players stand on the plates to open the door.

## Scene stuff

- `GameManager` — input and tether
- `Players/Player1` and `Player2` — move these in the editor to change spawn spots
- `CameraRig` — handles the cameras
- `Level` — build the room here

## Adding puzzles

Copy `Plate_P1` and `Plate_P2` from `Level/PuzzleDemo`.  
Put a `CoopDoor` on a cube and drag the plates into it in the Inspector.
