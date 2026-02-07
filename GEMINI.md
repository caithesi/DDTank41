# DDTank41 Project Context

## Project Overview

This project is a game server emulator and client migration effort for **DDTank** (a turn-based artillery game), version 3.0/4.1. 

**Dual Focus:**
1.  **Server Emulator:** A multi-server architecture (Center, Game, Fighting) implemented in **C# (.NET Framework 4.7.2)** using MSSQL.
2.  **Client Migration:** Porting the legacy Flash-based client to **Godot 4.x (.NET)**. This involves a "C# Bridge" strategy to reuse server-side physics and networking logic in the client for 100% synchronization.

**Key Characteristics:**
-   **Backend:** C# (.NET 4.7.2), SQL Server.
-   **Frontend:** Godot 4.x (C# & GDScript).
-   **Logic Sharing:** Shared C# library for deterministic physics and binary protocols.
-   **Localization:** Significant Vietnamese text in comments and strings.

## Architecture & Components

The solution is divided into several key projects/layers:

*   **DDTank.Shared**: The "Bridge" library. Contains shared logic for:
    -   **Networking:** `PacketIn`, `PacketOut`, and binary protocol definitions.
    -   **Physics:** `EulerVector` and deterministic trajectory math.
    -   **Terrain:** Bitmask-based destructible terrain logic (`Tile.cs`).
*   **Center.Server**: Central node for authentication and server coordination.
*   **Game.Server**: Primary game logic server (sessions, rooms, quests, events).
*   **Fighting.Server**: Dedicated battle processing server.
*   **SqlDataProvider**: Data Access Layer (DAL) for MSSQL.
*   **Bussiness**: Shared business logic and managers.
*   **Game.Base**: Core networking infrastructure.
*   **Game.Logic**: Battle mechanics and server-side physics implementation.

## Key Directories

*   `/DDTank.Shared`: Shared core logic for Client/Server parity.
*   `/Game.Server`: Main game server entry point.
*   `/Center.Server`: Central login and coordination server.
*   `/Fighting.Server`: Battle logic server.
*   `/Database`: MSSQL backup files (`.bak`).
*   `/plan`: Migration roadmap and process documentation.
*   `/architect`: Detailed technical specifications for Godot systems.
*   `/architect/legacy`: Deep-dive technical documentation of original C# systems (Physics, Terrain, Actions, Networking).

## Documentation Status

- [x] **Core Project Context:** Defined in `GEMINI.md`.
- [x] **Migration Roadmap:** Outlined in `/plan`.
- [x] **Legacy Architecture Deep-Dive:** Completed for Physics, Terrain, Actions, Networking, and Bombs in `/architect/legacy`.
- [x] **Terrain Destruction Specs:** Detailed implementation for Godot/C# in `architect/terrain_destruction.md`.
- [x] **Bomb System Specs:** Detailed implementation and binary format in `architect/legacy/bomb.md`.
- [ ] **Godot Implementation Specs:** In progress in `/architect`.

## Legacy Source Map (Migration Guide)

This section maps the original C# server components to their responsibilities. Use this as a reference when porting logic to the Godot client.

### 1. Core Logic & Physics (`Game.Logic`)
*   **Physics Engine:** `Phy/Maths/EulerVector.cs` ([Detailed Specs](architect/legacy/physics.md)) and `Phy/Object/Physics.cs`.
*   **Terrain System:** `Phy/Maps/Tile.cs` ([Detailed Specs](architect/terrain_destruction.md)) and `Phy/Maps/Map.cs`.
*   **Projectiles:** `Phy/Object/SimpleBomb.cs` (Bomb behavior) and `Phy/Object/Ball.cs`.
*   **Living Entities:** `Living.cs` and `Phy/Object/Player.cs` (Stats, state machine).
*   **Action Playback:** `Actions/*.cs` ([Detailed Specs](architect/legacy/actions.md)).

### 2. Networking & Protocol (`Game.Server` & `Game.Base`)
*   **Packet Handling:** `Game.Server/Packets/Client/` (Handlers for every client request).
*   **Protocol Definitions:** `Game.Server/Packets/ePackageType.cs` and various `*PackageType.cs` files.
*   **Serialization:** `Game.Base/PacketIn.cs` ([Detailed Specs](architect/legacy/networking.md)) and `GSPacketIn.cs`.
*   **Combat Script (Packet 91):** Handled in `Game.Logic` and dispatched via `Game.Server`.

### 3. Business & Data (`Bussiness` & `SqlDataProvider`)
*   **Managers:** `Bussiness/Managers/` (Shop, Quest, Item, Achievement data managers).
*   **Database Access:** `SqlDataProvider/` (CRUD operations for player data, items, and maps).
*   **Localization:** `Bussiness/LanguageMgr.cs`.

### 4. Game Systems (`Game.Server/Managers`)
*   **Room Management:** `Rooms/RoomMgr.cs` and `BaseRoom.cs`.
*   **Battle Matchmaking:** `Battle/BattleMgr.cs`.
*   **Special Systems:** `Pet/`, `Farm/`, `WorldBoss/`, `HotSpringRooms/`.

## Configuration

*   **Files:** Configuration is likely handled via `App.config` files in the respective server directories and code-mapped configuration classes (e.g., `GameServerConfig`).
*   **Logging:** Uses `log4net` with configuration in `logconfig.xml`.
*   **Hardcoded Values:** Be aware that some configuration (like IPs) might be hardcoded in classes like `GameServerConfig.cs` (e.g., `68.183.198.57`).

## Building and Running

**Prerequisites:**
*   **.NET SDK** (for building).
*   **Mono Runtime** (required to run .NET Framework 4.7.2 binaries on Linux) or **Windows**.
*   **SQL Server**: A running MSSQL instance with the databases restored from the `/Database` folder.

**Building:**
The project uses SDK-style `.csproj` files but targets `net472`.
```bash
dotnet build "DDTank 3.0.sln"
```
*Note: Due to Windows-specific references (e.g., `System.ServiceModel`), building on Linux might require specific setups or adjustments to the project files.*

**Running (Linux via Mono):**
```bash
mono Game.Server/bin/Debug/Game.Server.exe
```

## Development Notes

*   **Language:** The codebase contains Vietnamese comments and user-facing strings.
*   **Platform:** Originally designed for Windows. Paths and references in `.csproj` files may point to Windows system directories (e.g., `C:\WINDOWS\Microsoft.NET\...`).
*   **Status:** Appears to be a private server source, potentially based on leaked or community-maintained code (DDTank 3.0/4.1).
