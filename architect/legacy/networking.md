# Legacy Networking: Binary Protocol & Security

DDTank uses a custom binary protocol over TCP. It is designed for low overhead and includes a tiered encryption system.

## 1. Packet Structure (`PacketIn.cs`)
The protocol uses **Big-Endian** byte order for all multi-byte primitives (Short, Int, Long).

### Primitive Mapping
- **Short (16-bit)**: `(byte)(val >> 8)`, `(byte)(val & 0xFF)`
- **Int (32-bit)**: `(byte)(val >> 24)`, `(byte)(val >> 16)`, `(byte)(val >> 8)`, `(byte)(val & 0xFF)`
- **String**: Prefixed by a `Short` length, followed by UTF-8 bytes, and terminated with a null byte (`\0`).

## 2. Encryption Layers
The server and client use XOR-based rolling keys.

- **Layer 1 (Standard)**: A simple XOR with a static or session key.
- **Layer 2 (Rolling)**: Uses `CopyTo3`, where each byte's encryption key depends on the previous byte's encrypted value.

```csharp
// Rolling Key Logic (Simplified)
key[i % 8] = (byte)((key[i % 8] + dst[i - 1]) ^ i);
dst[i] = (byte)((m_buffer[index] ^ key[i % 8]) + dst[i - 1]);
```

## 3. The Handshake
1. Client connects to `Center.Server`.
2. Server sends an encryption seed.
3. Client initializes its local `PacketIn` and `PacketOut` crypto state.
4. Client sends login credentials.

## 4. Godot Implementation Details
- **`DDTank.Shared`**: Reusing `PacketIn.cs` and `GSPacketIn.cs` in the Godot project is critical to avoid re-implementing the complex rolling-key crypto.
- **Async Sockets**: Use `System.Net.Sockets.TcpClient` in an async loop to avoid blocking the Godot main thread.
- **Packet Dispatching**: Once a full packet is read (Header + Body), dispatch it to a `PacketProcessor` that triggers Godot signals based on the `Code`.