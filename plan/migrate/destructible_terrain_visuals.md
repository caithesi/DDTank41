# Phase 3: Destructible Terrain & Visuals

## Goal
Render the map and implement the terrain destruction (digging) system.

## What to do
1.  **Map Layering**: Create a scene that loads the Sky, Middle, and Ground textures.
2.  **Destruction Engine**: Implement the "Hole" system where explosions remove parts of the terrain.

## How to do it
*   **Layering**: Use `ParallaxBackground` for the Sky and Middle layers. Use a standard `Sprite2D` for the Ground.
*   **Terrain Digging (Texture Masking)**:
    *   Place the Ground sprite inside a `SubViewport`.
    *   To "dig", instantiate a "Crater" sprite (a simple circle with a subtraction blend mode or alpha=0) inside that same Viewport.
    *   Use the resulting `ViewportTexture` as the display texture for the terrain.
*   **Collision Sync**:
    *   The `DDTank.Shared` physics engine needs to know where the holes are.
    *   Mirror the server's `Map.cs` logic: use a bitmask or a 2D array of `Tile` objects (0 for empty, 1 for solid) to represent the collision. Update this grid whenever a "Dig" occurs.

## What to expect
*   **Verification**: Clicking on the screen should create a hole in the terrain.
*   **Success Metric**: A projectile hitting the terrain creates a visual hole and subsequent projectiles pass *through* that hole.
