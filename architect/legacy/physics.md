# Legacy Physics: Euler Integration

The physics system in DDTank uses a simplified **Euler Method** for numerical integration to calculate projectile trajectories and entity movement. This approach ensures that given the same initial conditions and time steps, the server and client (Flash or Godot) produce identical results.

## 1. The EulerVector Class
Located at `Game.Logic/Phy/Maths/EulerVector.cs`, this class is the atomic unit of motion.

### State Variables
- `x0`: Position (float)
- `x1`: Velocity (float)
- `x2`: Acceleration (float)

### Integration Logic
The core calculation happens in `ComputeOneEulerStep(float m, float af, float f, float dt)`:

```csharp
// f: Applied Force (Gravity, Wind, etc.)
// af: Air Friction / Resistance
// m: Mass
// dt: Delta Time (usually 0.04s for 40ms ticks)

x2 = (f - af * x1) / m; // Calculate new acceleration
x1 += x2 * dt;          // Update velocity
x0 += x1 * dt;          // Update position
```

## 2. Determinism Requirements
To maintain parity during migration to Godot:
- **Fixed Time Step**: Always use a fixed `dt`. DDTank traditionally runs at **25 FPS (40ms ticks)**.
- **Precision**: Use `float` as the original does. Godot's `float` is 64-bit by default in C#; ensure compatibility if the server specifically relies on 32-bit `System.Single` precision.
- **Coordinate System**: Both DDTank and Godot use **Y-down** (positive Y is towards the bottom of the screen).

## 3. Forces in DDTank
- **Gravity**: A constant force applied downwards on `x2`.
- **Wind**: A force applied horizontally, often varying per turn.
- **Air Resistance**: Proportional to the current velocity, acting as a damping factor.
