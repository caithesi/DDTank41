# Ubuntu Development Environment Setup

To develop the Godot-based DDTank client on Ubuntu, you must prepare the system to handle both the graphical engine and the .NET runtime.

## 1. Engine Version
**Requirement**: You MUST use the **Godot Engine - .NET** (formerly Mono) version. 
*   The "Standard" version will not work as it cannot execute the C# "Bridge" logic required for networking and physics.

## 2. .NET SDK (via `mise`)
To keep the system clean and avoid version conflicts, it is recommended to use `mise` (or `asdf`) to manage the .NET SDK.

### Why use a `mise` plugin?
`mise` uses a plugin-based architecture to delegate the "how-to-install" logic. The `dotnet` plugin is required because:
*   **Isolation:** It installs the SDK in your home directory, avoiding "dependency hell" and conflicts with system-wide `apt` packages.
*   **Environment Config:** It automatically manages critical variables like `DOTNET_ROOT` and `MSBuildSDKsPath`, which Godot requires to locate the C# compiler.
*   **Reproducibility:** It allows pinning specific SDK versions for the project, ensuring consistency between development and CI/CD environments.

```bash
# Install the dotnet plugin
mise plugins install dotnet

# Install and set the global version to 8.0
mise use -g dotnet@8.0
```

## 3. System Graphics & Hardware "Glue"
While `mise` handles the SDK, specific system libraries are required to interface with the GPU (Vulkan/OpenGL) and the windowing system (X11). These should be installed via `apt`.

```bash
# Essential "Glue" and Graphics libraries
sudo apt update && sudo apt install -y \
    libfontconfig1 \
    libdbus-1-3 \
    libxcursor1 \
    libxinerama1 \
    libxrandr1 \
    libxi6 \
    libgl1 \
    libvulkan1 \
    mesa-vulkan-drivers \
    vulkan-tools
```

## 4. Hardware Requirements
*   **Preferred**: Vulkan 1.0+ compatible hardware.
*   **Fallback**: If running in a Virtual Machine or on older hardware without Vulkan support, start Godot in **Compatibility Mode** (OpenGL 3.3).

## 5. Verification
After installation, verify the environment:
```bash
# Check .NET SDK
dotnet --version

# Check Vulkan support
vulkaninfo | grep "VULKAN_INFO"
```
