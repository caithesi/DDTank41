# Architecture Documentation

This directory contains technical specifications and architectural mappings for the DDTank migration project. It bridges the gap between the legacy C#/.NET Framework code and the modern Godot/C# implementation.

## Folder Structure

### `/legacy`
Contains deep-dives into the original game systems. This is the primary reference for how the game worked in Flash/C#.
- **actions.md**: Detailed protocol for Package 91 (Combat Script) and action playback logic.
- **bomb.md**: Analysis of the `.bomb` binary format and `BallInfo` metadata.
- **networking.md**: Specification of `PacketIn`/`PacketOut` and binary serialization patterns.
- **physics.md**: Deterministic trajectory math and `EulerVector` implementation.
- **terrain.md**: Legacy bitmask manipulation logic for destructible maps.

## Root Files
- **bomb_godot.md**: Implementation plan for the Bomb system in Godot.
- **dll.md**: Strategy for utilizing original C# DLLs/Logic within the Godot bridge.
- **frontend_gameplay.md**: Core gameplay loop and state management in Godot.
- **frontend_gameplay_visual.md**: Mapping logic between physical states and visual representations.
- **sound.md**: Audio infrastructure, asset origin (SWF), and migration strategy.
- **terrain_destruction.md**: Godot-specific implementation of the bitmask destruction system.
- **visual.md**: General visual architecture and shader usage.
