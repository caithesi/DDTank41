# Modernizing Gameplay Playback (Replacing Package 91)

## Overview
The original "Package 91" is a binary-packed list of actions recorded by the server's physics simulation. To migrate this to Godot efficiently, we will move from a raw binary stream to a structured **Battle Timeline** approach using JSON or Protobuf, leveraging Godot's Signal and Tween systems.

## 1. Data Structure Transition
Instead of parsing a byte array with offsets, define a structured format that Godot can natively handle (C# classes that serialize to JSON/Protobuf).

### Proposed Action Event Structure (C# / Godot)
```csharp
public class BattleTimeline {
    public List<BattleEvent> Events { get; set; }
}

public class BattleEvent {
    public float TimeStamp { get; set; } // Seconds from turn start
    public string Type { get; set; }     // "BOMB", "DAMAGE", "FLY", "DIE"
    public Dictionary<string, object> Params { get; set; }
}
```

## 2. Server-Side Changes (`Game.Logic`)
Update `SimpleBomb.cs` and `Living.cs` to populate this new structure.
- **Action Recording**: Instead of writing to a `PacketOut`, add entries to a `List<BattleEvent>`.
- **Packet Send**: Serialize the list at the end of the simulation and send it to the client.

## 3. Client-Side Implementation (Godot)

### A. The "Timeline Player" Logic
Instead of a frame-based `ActionManager`, use Godot's `SceneTreeTimer` or `Tween` for deterministic timing.

```gdscript
# Godot Playback Strategy
func play_timeline(timeline_data: Dictionary):
    for event in timeline_data.events:
        # Create a timer that fires at the exact server-calculated time
        get_tree().create_timer(event.timestamp).timeout.connect(
            func(): execute_event(event)
        )

func execute_event(event: Dictionary):
    match event.type:
        "BOMB":
            spawn_explosion(event.params.x, event.params.y)
            terrain.dig(event.params.x, event.params.y)
            sound_manager.play("explosion")
        "DAMAGE":
            var player = players[event.params.target_id]
            player.show_damage(event.params.damage)
            player.update_hp(event.params.new_hp)
```

### B. Smoothing with Tweens
For continuous movements (like the bomb's flight), don't just set the position. Use the server's `vX`, `vY` and `Gravity` to recreate the curve, or use a series of `Position` updates and interpolate between them.

```gdscript
# Interpolating Trajectory
var tween = create_tween()
for pos in trajectory_points:
    tween.tween_property(bomb_sprite, "position", pos.point, pos.interval)
```

## 4. Key Improvements over Flash
1.  **Decoupled Rendering**: The logic runs on Timers/Tweens, meaning frame-rate drops won't desync the audio from the visuals (Flash often had audio/visual desync if the FPS dropped).
2.  **Debuggable Packets**: Using JSON/Protobuf makes it easy to log and inspect "what the server actually said" without a hex editor.
3.  **Signal Chaining**: Use Godot Signals (`explosion_finished`, `death_anim_complete`) to handle complex sequences where one event must wait for another's animation.

## 5. Implementation Timing & Dependencies

### Timing
This modernization should be implemented during **Phase 3 (UI & Gameplay)** of the migration. Specifically, it should start once the player can successfully enter a battle room but before any complex combat visuals (animations, sounds) are polished.

### Dependencies
1.  **Phase 1 (Networking)**: Godot must be able to receive TCP packets from the server.
2.  **Phase 2 (C# Bridge)**: The shared C# math and packet logic must be available in Godot to interpret the new timeline data.

### Development Strategy
**Do not** implement the legacy Flash-style `Package 91` binary parser in Godot first. Instead, move directly to this Timeline-based approach to avoid redundant work and technical debt.

## 6. Migration Steps
1.  **Protocol Definition**: Define the Protobuf/JSON schema for `BattleEvent`.
2.  **Server Wrapper**: Create a helper in `Game.Logic` to collect `BombAction` and convert them to the new format.
3.  **Godot Event Handler**: Implement the `execute_event` dispatcher in Godot.
4.  **Visual Polish**: Link `BOMB` events to Godot `GPUParticles2D` and `AudioStreamPlayer2D`.
