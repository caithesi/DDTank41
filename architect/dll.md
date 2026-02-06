# Managed Dependencies (libdll)

This document describes the external managed DLL dependencies stored in the `libdll` directory and their roles within the DDTank server architecture.

## 1. Overview
The `libdll` directory acts as a local repository for compiled .NET assemblies (DLLs) that are not managed via NuGet. These libraries provide essential low-level functionality such as compression, JSON parsing, and utility helpers used across the distributed server environment.

---

## 2. Core Dependencies

### Zlib.net (`zlib.net.dll`)
- **Purpose**: Provides Zlib compression algorithms.
- **Primary Usage**: Used by `Game.Base` for network packet compression.
- **Technical Detail**: When the server sends large data packets (e.g., player item lists or map data) to the Flash client, it compresses them to reduce bandwidth usage. The Flash client then decompresses these using its internal `zlib` capabilities.

### Newtonsoft.Json (`Newtonsoft.Json.dll`)
- **Purpose**: High-performance JSON framework for .NET.
- **Primary Usage**: Used in `Bussiness`, `Game.Server`, and `SqlDataProvider`.
- **Technical Detail**: Handles serialization of game objects for database storage (in some BLOB fields), parsing of JSON-based configuration files, and facilitating communication with web-based APIs (e.g., login and payment callbacks).

### Lsj Utility Libraries (`Lsj.Util.JSON.dll`, `Lsj.Util.Dynamic.dll`)
- **Purpose**: Third-party utility helpers common in the DDTank/Gunny emulation community.
- **Primary Usage**: General utility tasks and legacy JSON handling.
- **Technical Detail**: These libraries often provide extensions for dynamic object manipulation and specialized JSON parsing that pre-dates the project's full migration to Newtonsoft.Json.

---

## 3. Integration Mechanism

### Project References
DLLs in `libdll` are referenced in `.csproj` files using relative `HintPath` elements:
```xml
<Reference Include="zlib.net">
  <HintPath>..\libdll\zlib.net.dll</HintPath>
</Reference>
```

### Runtime Resolution
During the build process, these DLLs are copied to the output `bin/` directory. For services like `Road.Service`, binding redirects are sometimes defined in `App.config` to ensure the correct versions are loaded:
```xml
<dependentAssembly>
  <assemblyIdentity name="Lsj.Util" publicKeyToken="0f183bc795c57a20" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
</dependentAssembly>
```

---

## 4. Other Dependency Locations
In addition to `libdll`, some specific projects maintain their own local `lib` folders:
- `Tank.Request/lib/`: Contains `Ajax.dll` and WebAPI-specific dependencies.
- `packages/`: Standard NuGet package storage (e.g., `log4net`, `protobuf-net`).
