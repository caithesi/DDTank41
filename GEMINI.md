# DDTank41 Project Context

## Project Overview

This project is a game server emulator for **DDTank** (a turn-based artillery game similar to Worms), version 3.0/4.1. It is a multi-server architecture implemented in **C#** using the **.NET Framework 4.7.2**. The codebase includes components for handling game logic, battles, central coordination, and database interactions.

**Key Characteristics:**
-   **Language:** C#
-   **Framework:** .NET Framework 4.7.2
-   **Architecture:** Distributed server model (Center, Game, Fighting).
-   **Database:** SQL Server (MSSQL).
-   **Localization:** Contains significant amounts of Vietnamese text in comments and strings.

## Architecture & Components

The solution is divided into several key projects/layers:

*   **Center.Server**: The central node responsible for user login, authentication, and coordinating multiple game servers. It likely handles the initial connection and world list.
*   **Game.Server**: The primary game logic server. It handles:
    -   Player sessions, rooms, and chat.
    -   PVE and PVP matchmaking.
    -   Item management, shops, and quests.
    -   Events (World Boss, Little Game, etc.).
*   **Fighting.Server**: A dedicated server for handling computationally intensive battle logic, likely offloading this from the Game Server.
*   **SqlDataProvider**: The Data Access Layer (DAL). It interfaces with the MSSQL database.
*   **Bussiness**: Contains shared business logic and managers used by the server components.
*   **Game.Base**: The core networking and packet handling infrastructure (base server/client classes).
*   **Game.Logic**: Game-specific logic rules, calculations, and mechanics.

## Key Directories

*   `/Game.Server`: Main entry point for the game logic server.
    *   `GameServer.cs`: Server startup and lifecycle management.
    *   `GameServerConfig.cs`: Configuration handling.
*   `/Center.Server`: Source for the central login/coordination server.
*   `/Fighting.Server`: Source for the battle processing server.
*   `/Database`: Contains database backup files (`.bak`) for restoring the game state.
*   `/Bussiness`: Shared business logic.
*   `/SqlDataProvider`: Database interaction code.

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
