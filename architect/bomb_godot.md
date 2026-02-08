# Bomb & Weapon Implementation (Godot)

This document outlines the proposed implementation for weapons and bombs in the Godot client, focusing on sound, radius, and crater logic.

## 1. Data Model (C# Resources)

To mirror the legacy SQL structure, we use Godot's `Resource` system. This allows for easy editing in the Inspector and efficient loading.

### BallProperties (BallInfo)
Represents the physical and audio properties of a projectile.
```csharp
using Godot;

public partial class BallProperties : Resource 
{
    [Export] public int Id;
    [Export] public AudioStream ShootSound;
    [Export] public AudioStream BombSound;
    [Export] public int Radii; // Logic radius for damage
    [Export] public string CraterPath; // Path to .bomb bitmask file
    [Export] public bool HasTunnel;
}
```

### WeaponMapping (BallConfigInfo)
Maps a weapon template to its various ball states.
```csharp
public partial class WeaponMapping : Resource 
{
    [Export] public int TemplateId;
    [Export] public BallProperties Common;
    [Export] public BallProperties Special; // "Pow"
}
```

## 2. Audio Strategy

- **SoundManager**: A global singleton that caches `AudioStream` objects.
- **Triggering**:
    - `ShootSound`: Played by the `Player` or `Bomb` node immediately upon instantiation.
    - `BombSound`: Played by the `Bomb` node at the moment of collision/explosion.

## 3. Explosion & Crater Logic

### Radius (`Radii`)
The `Radii` property from `BallProperties` should be used for:
1.  **Damage Calculation**: Circular area check for living entities.
2.  **Visual Shake**: Determining the intensity of screen shake based on proximity.

### Crater (Terrain Destruction)
- **Shared Logic**: Reuse `DDTank.Shared/Tile.cs` to load and process the `.bomb` bitmask.
- **Visual Update**:
    - The `Map` node in Godot should hold a `Texture2D` representing the terrain.
    - When a bomb explodes, the bitmask is applied to the terrain `Image` using `AND NOT` logic (parity with server).
    - Call `texture.Update(image)` to refresh the visuals.

## 4. Implementation Steps (Phased)

1.  **Database Bridge**: Create a tool/script to export `BallInfo` and `BallConfigInfo` from MSSQL into `.tres` (Godot Resource) files.
2.  **Sound Mapping**: Map the legacy numeric sound names (e.g., "064") to the new Godot asset paths.
3.  **Bomb Node**: Implement a `SimpleBomb.tscn` that accepts a `BallProperties` resource and handles its own sound playback and explosion signal.
4.  **Terrain Integration**: Connect the `Bomb.Exploded` signal to the `Map.Dig` method, passing the `CraterPath`.
