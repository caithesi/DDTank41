# DDTank Flash to Godot Migration Overview

This document outlines the high-level strategy for migrating the DDTank ActionScript 3 (AS3) Flash client to the Godot Engine (4.x) using C#.

## Strategic Approach: The "C# Bridge"
Since the backend is implemented in .NET Framework 4.7.2, we will use **Godot Mono (C#)** to allow direct logic sharing between the server and the client. This eliminates the need to reimplement complex physics and math, ensuring 100% synchronization.

## Migration Phases

### [Phase 1: Foundation & Networking](networking_and_foundation.md)
*   Setup Godot 4 C# Project.
*   Establish a shared C# library for Physics and Protocols.
*   Implement TCP Networking and Packet serialization.

### [Phase 2: Deterministic Physics](deterministic_physics.md)
*   Port the `EulerVector` and `Physics` simulation from Server/Flash to Godot.
*   Validate trajectory consistency between Client and Server.

### [Phase 3: Destructible Terrain & Visuals](destructible_terrain_visuals.md)
*   Implement the Map rendering system (Sky, Ground, Stone layers).
*   Replicate the "Alpha Masking" destruction system using Godot Shaders or Viewports.

### [Phase 4: Gameplay Loop (Playback Script)](gameplay_loop_playback.md)
*   Implement the `ActionManager` to handle Packet 91 (Combat Script).
*   Create the sequential playback system for animations and effects.

### [Phase 5: Assets & UI](assets_ui_reconstruction.md)
*   Extract assets from SWF files using JPEXS.
*   Rebuild the UI using Godot Control nodes.
*   Implement the dynamic Character Assembly system.
