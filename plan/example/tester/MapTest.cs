using Godot;
using DDTank.Shared;
using System;

namespace DDTank.Godot.Example
{
    /// <summary>
    /// A manual tester for Destructible Terrain in Godot.
    /// To use this:
    /// 1. Create a 2D Scene in Godot.
    /// 2. Add a Sprite2D with a large terrain texture (e.g., a landscape).
    /// 3. Set the Sprite2D's 'Centered' property to false and position it at (0,0).
    /// 4. Attach this script to the root node or a parent node.
    /// </summary>
    public partial class MapTest : Node2D
    {
        [Export]
        public NodePath TerrainSpritePath;

        [Export]
        public NodePath MapBridgePath; // Allow assigning an existing bridge node

        [Export]
        public string BombFilePath;

        [Export]
        public int CircleRadius = 30;

        private DDTankMap _mapBridge;
        private Tile _bombMask;

        public override void _Ready()
        {
            GD.Print("MapTest: Initializing...");

            // 1. Get the Sprite2D
            Sprite2D sprite = GetNode<Sprite2D>(TerrainSpritePath);
            if (sprite == null)
            {
                GD.PrintErr("MapTest: TerrainSpritePath not set or invalid!");
                return;
            }

            // 2. Initialize logical terrain
            int width = (int)sprite.Texture.GetSize().X;
            int height = (int)sprite.Texture.GetSize().Y;
            Tile terrainLogic = new Tile(width, height, true);
            // Fill with solid by default
            for (int i = 0; i < terrainLogic.Data.Length; i++) terrainLogic.Data[i] = 0xFF;

            // 3. Setup/Find the Bridge
            if (MapBridgePath != null && !MapBridgePath.IsEmpty)
            {
                _mapBridge = GetNode<DDTankMap>(MapBridgePath);
            }
            
            if (_mapBridge == null)
            {
                GD.Print("MapTest: No bridge assigned, creating a dynamic one...");
                _mapBridge = new DDTankMap();
                AddChild(_mapBridge);
            }

            _mapBridge.Initialize(terrainLogic, sprite);

            // 4. Prepare a test "Bomb" mask
            if (!string.IsNullOrEmpty(BombFilePath))
            {
                GD.Print($"MapTest: Attempting to load bomb mask from {BombFilePath}");
                _bombMask = CreateTileFromBombFile(BombFilePath);
                if (_bombMask == null)
                {
                    GD.PrintErr("MapTest: Failed to load specified bomb file. Digging will be disabled.");
                }
            }
            else
            {
                GD.Print($"MapTest: No BombFilePath provided, creating procedural circle mask (Radius: {CircleRadius}).");
                _bombMask = CreateCircleMask(CircleRadius);
            }

            GD.Print("MapTest: Ready! Left-click on the terrain to dig holes.");
        }

        public override void _Input(InputEvent @event)
        {
            // Detect Mouse Left Click
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                if (_bombMask == null)
                {
                    GD.PrintErr("MapTest: No bomb mask available to dig!");
                    return;
                }

                Vector2 pos = GetLocalMousePosition();
                
                GD.Print($"MapTest: Digging at {(int)pos.X}, {(int)pos.Y}");
                
                // Trigger the Dig operation on the bridge
                // This updates both the bitmask (for physics) and the texture (for visuals)
                _mapBridge.Dig((int)pos.X, (int)pos.Y, _bombMask, null);
            }
        }

        /// <summary>
        /// Loads a Tile mask from a binary .bomb file.
        /// Fails explicitly if the file cannot be loaded.
        /// </summary>
        private Tile CreateTileFromBombFile(string path)
        {
            try
            {
                // If it's a Godot path (res://), we need to map it to a global path for System.IO
                string globalPath = ProjectSettings.GlobalizePath(path);
                
                if (!System.IO.File.Exists(globalPath))
                {
                    GD.PrintErr($"MapTest: Bomb file not found at {globalPath}");
                    return null;
                }

                return new Tile(globalPath, false);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"MapTest: Error loading bomb file: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Procedurally creates a circular bitmask for testing.
        /// </summary>
        private Tile CreateCircleMask(int radius)
        {
            int size = radius * 2;
            Tile mask = new Tile(size, size, false);
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double dist = Math.Sqrt(Math.Pow(x - radius, 2) + Math.Pow(y - radius, 2));
                    if (dist < radius)
                    {
                        // Set the bit to solid (1) in the mask.
                        // This uses the same bit-packing logic as the original DDTank.
                        int bw = size / 8 + 1;
                        int byteIdx = y * bw + x / 8;
                        byte bitMask = (byte)(0x01 << (7 - x % 8));
                        mask.Data[byteIdx] |= bitMask;
                    }
                }
            }
            return mask;
        }
    }
}
