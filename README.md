# GameJam

Unity 6000.6.0f1

Open `Assets/Scenes/SampleScene.unity` and hit Play.

The course lives under `Level` in the scene. Edit rooms, plates, doors, and checkpoints there.

## Controls

P1: WASD + Space, Left Shift to yank, E to ping  
P2: Arrows + Enter, Right Shift to yank, Right Ctrl to ping

Gamepads: stick + South jump, West yank, North ping.

## How it plays

Two robots share a tether.

- **Linked** — one camera, slower, yank pulls your partner in
- **Split** — cameras divide, higher and closer, faster move, higher jump, ping marks your spot
- Stretch too far and the tether snaps. Walk back together to reconnect.

## The rooms

1. **Stay linked** — both plates, don't snap. Yank if someone lags.
2. **Snap on purpose** — orange pads are too far and too high while linked. Ping, split-jump, latch the pads, meet in the cyan circle.
3. **Cut the beam** — stand on opposite banks so the tether breaks the red line.
4. **Yank the gap** — one player takes the left catwalk. Reconnect, then hold yank to reel the other under the low ceiling.
5. **Finale** — plates plus the beam, then the gold pad together.

Falling respawns both players at the last opened door (`Level/Checkpoints`).
