# Phase 2: Deterministic Physics

## Goal
Ensure the client calculates projectiles and movement exactly like the server.

## What to do
1.  **Port Physics Logic**: Move the `EulerVector` and `Physics` math into the shared library.
2.  **Trajectory Validation**: Build a tool to compare server-calculated paths vs client-calculated paths.

## How to do it
*   **Move Files**: Move `Game.Logic/Phy/Maths/EulerVector.cs` and `Game.Logic/Phy/Object/Physics.cs` to the `DDTank.Shared` library.
*   **Coordinate System**: Godot uses Y-down by default. DDTank also uses Y-down (standard for 2D Flash). Ensure gravity and wind vectors match.
*   **Test Scene**:
    *   Create a Godot scene with a "Shooter" and a "Target".
    *   Input a force (e.g., 50) and angle (e.g., 45).
    *   Run the simulation in Godot and print the coordinates `(x, y)` every 40ms.
    *   Compare these values with the server's `SimpleBomb.cs` debug logs.

## What to expect
*   **Verification**: The projectile path in Godot should overlap perfectly with the server's simulation.
*   **Success Metric**: Zero deviation in floating-point coordinates over a 5-second flight path.
