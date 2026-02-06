# Phase 1: Foundation & Networking

## Goal
Establish a robust C# environment in Godot and connect it to the existing DDTank backend.

## What to do
1.  **Project Initialization**: Create a Godot 4.x (C# version) project.
2.  **Logic Extraction**: Identify and isolate the core math and protocol code from the existing C# solution.
3.  **Basic Connection**: Create a `NetworkManager` that can perform a handshake with the `Center.Server`.

## How to do it
*   **Shared Library**: Create a new `.csproj` (Targeting `netstandard2.0` or `net6.0`) called `DDTank.Shared`.
    *   Copy `Game.Base/Packets/PacketIn.cs` and `PacketOut.cs`.
    *   Copy `Bussiness/Protocol` definitions.
*   **Godot Network Node**:
    *   Create a `Node` named `NetworkManager`.
    *   Use `System.Net.Sockets.TcpClient` for the connection.
    *   Implement a loop to read the packet header (2 bytes length, 2 bytes checksum) and the body.
*   **Packet Handling**:
    *   Create a signal-based system: `packet_received(int code, PacketIn pkg)`.

## What to expect
*   **Verification**: You should be able to launch Godot, call `Connect("127.0.0.1", 9202)`, and see a "Connected" log.
*   **Success Metric**: Receiving the first packet from the server (usually a version check or encryption key) and parsing its header correctly.
