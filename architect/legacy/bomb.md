# Bomb System Architecture (Legacy)

This document describes the structure and logic of the bomb system in DDTank, focusing on how `.bomb` binary files, database metadata (`BallInfo`), and the physics engine interact.

## 1. Component Overview

The "Bomb" in DDTank is not a single file, but a synchronized package of data:

| Component | Source | Responsibility |
| :--- | :--- | :--- |
| **Bitmask (`.bomb`)** | Binary File | Defines the physical shape/size of the terrain destruction. |
| **Metadata (`BallInfo`)** | Database | Defines logic (Damage, Radius), Visuals (Particles), and Audio. |
| **Logic (`SimpleBomb`)** | C# Code | Handles the state machine (Flying -> Colliding -> Bombing). |

---

## 2. The `.bomb` Binary Format

`.bomb` files are raw bitmask containers used by the `Tile.cs` class. They do not contain any visual textures or audio; they only contain "solid vs. empty" data.

### File Structure
| Offset | Type | Description |
| :--- | :--- | :--- |
| `0x00` | `Int32` | **Width**: Width of the crater mask in pixels. |
| `0x04` | `Int32` | **Height**: Height of the crater mask in pixels. |
| `0x08` | `Byte[]` | **Bitmask Data**: `(Width / 8 + 1) * Height` bytes. |

### Bitmask Logic
- **1 (Bit Set)**: Represents a "solid" part of the hole. When applied via `Remove`, these pixels become transparent in the map.
- **0 (Bit Clear)**: Represents "empty" space. These areas of the mask have no effect on the target terrain.

## 3. Shape Variety & Irregular Craters

Unlike many games that use procedural geometric formulas (like circles), DDTank uses a **"Stamp" Philosophy** for terrain destruction.

### How Irregular Shapes are Handled
Because shapes are stored as raw bitmasks, the engine supports any arbitrary shape without extra math:
- **Serrated Circles**: The most common weapon crater, having jagged edges for a "burnt" aesthetic.
- **Rectangular/V-Shapes**: Used for specialized drills or weapons.
- **Thematic Shapes**: "Love" bombs can create heart-shaped holes because the `.bomb` file simply contains a heart-shaped bitmask.

### Rasterization Process
Shapes are created by converting images (PNGs) into bitmasks. If a pixel's Alpha value is above 100, it is considered "solid" (1) in the `.bomb` mask. This allows artists to design craters in Photoshop rather than developers writing complex geometry code.

### Bounding Box Optimization (`ScanBound`)
While the `.bomb` file has a fixed width and height (e.g., 60x60), the actual "solid" shape inside it might be smaller (e.g., a 20x20 jagged star).
- **Legacy Behavior**: Used the full file dimensions for bounds checking.
- **Modernized shared logic**: Implements a `ScanBound()` method that pre-scans the bitmask on initialization to find the tightest possible `Rectangle` covering only the "1" bits. This significantly reduces the iteration count in `Add`/`Remove` operations.

---

## 4. Metadata: `BallInfo`

Every weapon/bullet has a `BallID` which maps to a `BallInfo` entry. This bridges the physical hole with the player's experience.

### Key Fields
- **`Radii`**: The **Logic Radius**. This is often larger than the visual `.bomb` mask. It is used for circular damage detection.
- **`BombPartical`**: String name of the asset to play on the client (e.g., `bomb_fireball`).
- **`BombSound`**: String name of the `.wav`/`.mp3` to play on impact.
- **`HasTunnel`**: Boolean. If true, the engine looks for a corresponding `.bomb` file in the `bomb/` directory.

---

## 4. Execution Flow: The "Dig" Process

When a `SimpleBomb` collides with the ground (`CollideGround`), the following sequence occurs:

1.  **State Change**: The bomb stops moving and sets `m_bombed = true`.
2.  **Logic check**: `SimpleBomb` checks `m_info.IsSpecial()`.
    - If it's a **Normal** bomb: `digMap = true`.
    - If it's **Frozen/Fly/Cure**: `digMap = false` (No crater).
3.  **Terrain Modification**:
    - If `digMap` is true, the engine calls `m_map.Dig(x, y, m_shape, null)`.
    - `Tile.Remove` is executed, performing a bitwise `AND NOT` on the map's bitmask using the `.bomb` data.
4.  **Visual Sync**: The client receives a `BombAction` packet, triggers the particle effect, and updates its local `ImageTexture` to match the new bitmask.

---

## 5. Special Bomb Behaviors

| Type | Digs Map? | Result |
| :--- | :--- | :--- |
| **Normal** | Yes | Crater appears; damage dealt to `Radii`. |
| **FORZEN** | No | No crater; target gets "Frozen" effect (skips turns). |
| **FLY** | No | No crater; owner is teleported to collision `(x, y)`. |
| **CURE** | No | No crater; players in `Radii` gain HP. |

---

## 6. Porting Notes for Godot

- **Path Handling**: In Godot, use `res://assets/bombs/{ID}.bomb`.
- **Performance**: While the Server processes bitmasks once per explosion, the Godot client must update the `ImageTexture`. For large explosions, it is recommended to use `Image.BlitRect` with a pre-calculated transparency mask.
- **Parity**: Always use the `DDTank.Shared/Tile.cs` logic to ensure that a hole "looks" exactly where the server says it "is".
