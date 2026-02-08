# Migration Project Plan

This directory tracks the roadmap, process, and technical notes for the DDTank migration to Godot.

## Folder Structure

### `/process`
The active implementation roadmap. Sequential steps for the current development phase.
- **migrate_target_overview.md**: High-level summary of the implementation steps.
- **1_infra_prepare.md**: Setting up the C# Bridge and project structure.
- **2_godot_terrain_implementation.md**: Implementing the `Tile` and `Map` logic in Godot.
- **3_test_dd_map.md**: Testing basic terrain destruction parity.
- **4_test_explosion_weapon.md**: Data-driven testing with real ball shapes and sounds.

### `/example`
Code snippets and implementation references for the Godot frontend.
- **DDTankMap.cs**: Example bridge between `DDTank.Shared` and Godot visuals.
- **tester/**: Interactive testing scripts (`MapTest`, `MapTestWithBallInfo`).

### `/migrate`
Thematic migration guides focusing on specific system groups.
- **assets_ui_reconstruction.md**: Porting Flash UI to Godot Control nodes.
- **deterministic_physics.md**: Ensuring 100% sync between Server and Client.
- **godot_overview.md**: General Godot project configuration and best practices.
- **modernizing_package_91.md**: Strategy for handling the complex Combat Script.

### `/prepare`
Prerequisites and environment setup guides.
- **ubuntu.md**: Linux development environment setup.

### `/note`
Technical logs and temporary research notes.
- **destroy_map_test.md**: Notes on early map destruction experiments.

## Root Files
- **overall.md**: The master roadmap for the entire project.
