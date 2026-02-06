# Phase 5: Assets, UI, and Sound System

## Goal
Import game assets, rebuild the UI, and implement the audio engine.

## What to do
1.  **SWF Extraction**: Convert Flash assets into Godot-compatible PNGs and Spritesheets.
2.  **Character System**: Implement the composite avatar rendering.
3.  **UI Reconstruction**: Build menus (Login, Room, Bag) using Godot nodes.
4.  **Sound System**: Implement a `SoundManager` for SFX and BGM.

## How to do it
### Asset Extraction
*   Use JPEXS to export SWF symbols as PNG.
*   For animations, export "Frame by Frame" as a spritesheet.

### UI: Replacing ComponentFactory
*   **Why drop ComponentFactory?** The original `ComponentFactory` was a workaround for Flash's lack of a robust UI editor, using XML to define layouts. Godot's **Scene System (.tscn)** is a superior, native version of this.
*   **Godot Approach**: Instead of XML, use Godot's visual editor to create UI components. Use `Themes` to define global styles (buttons, fonts) and save them as `.tres` files. This replaces the "Styles" section of the old XMLs.

### Sound System
*   **Audio Manager**: Create a global `SoundManager` singleton in Godot.
*   **SFX (Embedded)**: Import extracted `.wav` or `.ogg` files. Use an `AudioStreamPlayer` pool to handle simultaneous sounds (mimicking the 32-channel limit).
*   **BGM (Streaming)**: Use `AudioStreamPlayer` with the `Stream` property set to a file path. Since Godot doesn't natively play `.flv` audio well, convert BGM to `.ogg` or `.mp3` during extraction.
*   **Mapping**: Create a C# Dictionary to map IDs (e.g., `"008"`) to `AudioStream` resources, maintaining compatibility with server-sent sound IDs.

### Character Assembly
*   Create a `Character` node with child `Sprite2D` nodes: `BackHair`, `Body`, `Face`, `Eyes`, `Hair`, `Clothes`.
*   Set the `Modulate` property of these sprites to match the color hex codes sent by the server.

## What to expect
*   **Verification**: The game should look and sound like the original DDTank.
*   **Success Metric**: High-fidelity visuals, functional UI menus, and synchronized audio feedback for all interactions.