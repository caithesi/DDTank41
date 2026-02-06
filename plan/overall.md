# DDTank Migration: Overall Project Plan

## Architectural Vision
Migrate the legacy Flash-based DDTank client to **Godot 4.x** while maintaining 100% logic parity with the existing C# backend.

## Core Technical Decisions

### 1. Engine & Language Strategy: Hybrid Godot .NET
To leverage the existing C# codebase from the server and Flash (converted), we will use the **Godot .NET** version.

*   **Primary Framework**: Godot 4.x (.NET/Mono version).
*   **High-Level Logic & UI**: **GDScript**. This provides the "standard Godot" experience for rapid UI development, scene management, and animation.
*   **Core Game Logic & Math**: **C# (The "Bridge")**. We will wrap the original C# physics (`EulerVector`), networking packets (`PacketIn`/`PacketOut`), and protocol definitions in a C# library.
*   **Interoperability**: GDScript will call into C# classes for deterministic calculations (physics, damage) and packet serialization.

### 2. Rationale: Why C# / .NET?
*   **Binary Parity**: Ensures the custom binary protocol (TCP) is handled identically on both ends. We can reuse `PacketIn` and `PacketOut` logic without error-prone translations.
*   **Logic Sharing**: The server-side battle logic (physics, collision, damage) is in C#. Sharing this as a library ensures 100% synchronization.
*   **Performance**: C# handles complex packet serialization and physics math more efficiently than interpreted GDScript.
*   **Type Safety**: Prevents runtime bugs in complex combat scripts (`Packet 91`).

### 3. UI Migration Overstructure
We will replicate the Flash `ComponentFactory` pattern to handle the transition from ActionScript-based UI.

*   **Component Registry**: A Godot `Resource` or `Autoload` that maps legacy Flash UI component IDs to Godot `.tscn` files.
*   **Visual Mapping**: 
    *   `MovieClip` $\rightarrow$ `AnimationPlayer` + `Node2D`.
    *   `SimpleButton` $\rightarrow$ `TextureButton`.
    *   `TextField` $\rightarrow$ `Label` / `RichTextLabel`.
*   **Data Flow**: 
    *   **C# Bridge** receives raw packets $\rightarrow$ Decodes into Data Objects.
    *   **GDScript UI** listens for data signals $\rightarrow$ Updates visuals.

### 4. Synchronization Strategy
*   **Physics**: Shared C# math libraries between Server and Client to ensure trajectories match exactly.
*   **Communication**: Direct TCP sockets using the original binary protocol to avoid modifying the server.

## Migration Roadmap

### Phase 1: Infrastructure
*   Setup Godot 4 .NET project.
*   Extract `DDTank.Shared` C# library.
*   Implement TCP handshake in C#.

### Phase 2: Game World & Physics
*   Implement `EulerVector` in the C# bridge.
*   Map rendering and destructible terrain logic.

### Phase 3: UI & Gameplay
*   Rebuild Flash UI components in Godot.
*   ActionManager for combat script playback.

### Phase 4: Assets
*   Automated asset extraction from SWFs.
*   Character assembly system (skins, hair, equipment).



   1 # Install the dotnet plugin
   2 mise plugins install dotnet
   3
   4 # Install and use the SDK (Godot 4 works best with 8.0)
   5 mise use -g dotnet@8.0