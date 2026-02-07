# Legacy Actions: Sequential Playback

Combat in DDTank is not real-time. The server calculates the entire turn's outcome and sends a sequence of events (Packet 91) to the client. The client uses an **Action System** to play these back chronologically.

## 1. BaseAction Architecture
Found in `Game.Logic/Actions/BaseAction.cs`, this system manages "when" an animation or logic step should occur.

### Timing Properties
- `m_tick`: The start time (in absolute ticks).
- `m_finishDelay`: Additional wait time after the action logic finishes.
- `m_finishTick`: When the action is considered fully complete and the next can begin.

### Execution Flow
1. **`Execute(game, current_tick)`**: Called every frame.
2. If `current_tick >= m_tick`, it calls `ExecuteImp`.
3. **`ExecuteImp`**: Contains the specific logic (e.g., "Start Walking Animation", "Spawn Particle").
4. **`Finish(tick)`**: Sets the `m_finishTick` to `current_tick + m_finishDelay`.

## 2. Common Action Types
- **`LivingShootAction`**: Controls the firing animation and projectile spawn.
- **`LivingSayAction`**: Displays a chat bubble over a character.
- **`LivingMoveToAction`**: Coordinates movement between two points.
- **`BombAction`**: Handles the projectile flight and eventual explosion.

## 3. Implementation in Godot
- **`ActionManager`**: A central coordinator (likely a Node) that maintains a queue of `IAction` objects.
- **Signals**: Use Godot signals to notify the UI or other systems when an action starts or finishes.
- **Delta vs Ticks**: The legacy system uses `long` ticks (milliseconds). Godot uses `double` delta. For parity, it is recommended to maintain a millisecond-based `CurrentTick` variable within the `ActionManager`.
