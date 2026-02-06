# DDTank.Shared

This library contains the core game logic, physics, and networking protocols extracted from the original DDTank C# source. It is designed to be shared between the Godot 4.x client and any C# backend to ensure 100% logic parity.

## Features
- **Deterministic Physics**: Euler integration for projectiles (`EulerVector`, `BombObject`).
- **Networking**: Custom binary packet serialization (`PacketIn`, `GSPacketIn`).
- **Protocol**: Shared enum for packet codes (`ePackageType`).
- **Cross-Platform Geometry**: Custom `Point` and `Rectangle` structs (replaces `System.Drawing`).

## How to use in Godot

### 1. Installation
Copy the `DDTank.Shared` folder into your Godot project root. Then, add the following to your Godot project's `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="DDTank.Shared/DDTank.Shared.csproj" />
</ItemGroup>
```

### 2. Implementing Physics
To make a projectile fly, you need to implement the `IMap` interface so the physics engine knows where the floor and walls are.

```csharp
using DDTank.Shared;

public partial class MyGodotMap : Node2D, IMap {
    public float Gravity => 9.8f;
    public float Wind => 0.0f;
    public float AirResistance => 0.1f;

    public bool IsRectangleEmpty(Rectangle rect) {
        // Implement collision check with your Godot TileMap or Shaders
        return true; 
    }

    public bool IsOutMap(int x, int y) => y > 2000;
    
    public Physics[] FindPhysicalObjects(Rectangle rect, Physics except) => new Physics[0];
}
```

### 3. Shooting a Bomb
Create a script for your bomb sprite:

```csharp
using Godot;
using DDTank.Shared;

public partial class GodotBomb : Sprite2D {
    private BombObject _sim;

    public override void _Ready() {
        // mass, gravityFactor, windFactor, airResistFactor
        _sim = new BombObject(0, 1.0f, 1.0f, 1.0f, 1.0f);
        _sim.SetMap(GetParent<IMap>());
        _sim.SetXY((int)Position.X, (int)Position.Y);
        _sim.SetSpeedXY(500, -500); // Shoot!
        _sim.StartMoving();
    }

    public override void _Process(double delta) {
        if (_sim.IsMoving) {
            _sim.Update((float)delta);
            Position = new Vector2(_sim.X, _sim.Y);
        }
    }
}
```

### 4. Networking
Use `GSPacketIn` to read and write packets compatible with the original server:

```csharp
var pkg = new GSPacketIn((short)ePackageType.LOGIN);
pkg.WriteString("username");
pkg.WriteString("password");
pkg.WriteHeader();
// Send pkg.Buffer over a TcpClient
```
