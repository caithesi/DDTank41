using Godot;
using DDTank.Shared;
using System;
using System.Collections.Generic;

namespace DDTank.Godot.Example
{
    /// <summary>
    /// A manual tester for Destructible Terrain in Godot.
    /// Provides a flexible way to switch between a procedural circle and real .bomb files.
    /// 
    /// Usage:
    /// - LEFT CLICK: Dig a hole.
    /// - MOUSE WHEEL: Cycle between Circle Mode and all .bomb files in the folder.
    /// </summary>
    public partial class MapTest : Node2D
    {
        [Export] public NodePath TerrainSpritePath;
        [Export] public NodePath MapBridgePath; 
        [Export] public string BombFolder = "res://bomb/";
        [Export] public int CircleRadius = 30;

        private DDTankMap _mapBridge;
        private List<string> _bombFiles = new List<string>();
        private int _currentIndex = -1; // -1 = Circle Mode, 0+ = Bomb Files
        private Tile _currentMask;

        public override void _Ready()
        {
            GD.Print("MapTest: Initializing...");

            // 1. Get nodes
            Sprite2D sprite = GetNode<Sprite2D>(TerrainSpritePath);
            _mapBridge = GetNode<DDTankMap>(MapBridgePath);

            if (sprite == null || _mapBridge == null)
            {
                GD.PrintErr("MapTest: Sprite or Bridge not found! Check NodePaths.");
                return;
            }

            // 2. Initialize logical terrain (Solid)
            int w = (int)sprite.Texture.GetSize().X;
            int h = (int)sprite.Texture.GetSize().Y;
            Tile logic = new Tile(w, h, true);
            for (int i = 0; i < logic.Data.Length; i++) logic.Data[i] = 0xFF;

            _mapBridge.Initialize(logic, sprite);

            // 3. Scan for bomb files
            ScanBombFolder();
            
            // 4. Start with Circle Mode
            SwitchMode(-1);

            GD.Print("MapTest: READY.");
            GD.Print(" -> LEFT CLICK: Dig");
            GD.Print(" -> MOUSE WHEEL: Cycle between Circle and .bomb files");
        }

        private void ScanBombFolder()
        {
            using var dir = DirAccess.Open(BombFolder);
            if (dir != null)
            {
                dir.ListDirBegin();
                string file = dir.GetNext();
                while (file != "")
                {
                    if (!dir.CurrentIsDir() && file.EndsWith(".bomb"))
                    {
                        _bombFiles.Add(BombFolder + file);
                    }
                    file = dir.GetNext();
                }
            }
            GD.Print($"MapTest: Found {_bombFiles.Count} bomb files in {BombFolder}");
        }

        private void SwitchMode(int index)
        {
            // Wrap around logic
            if (index < -1) index = _bombFiles.Count - 1;
            if (index >= _bombFiles.Count) index = -1;

            _currentIndex = index;

            if (_currentIndex == -1)
            {
                _currentMask = CreateCircleMask(CircleRadius);
                GD.Print("[MODE: Procedural Circle]");
            }
            else
            {
                string path = _bombFiles[_currentIndex];
                // ProjectSettings.GlobalizePath is used because Tile.cs uses System.IO
                string globalPath = ProjectSettings.GlobalizePath(path);
                _currentMask = new Tile(globalPath, false);
                GD.Print($"[MODE: Bomb File] {path.GetFile()} ({_currentMask.Width}x{_currentMask.Height})");
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed)
            {
                // Left Click = Dig
                if (mb.ButtonIndex == MouseButton.Left)
                {
                    Vector2 pos = GetLocalMousePosition();
                    _mapBridge.Dig((int)pos.X, (int)pos.Y, _currentMask, null);
                }
                
                // Mouse Wheel = Cycle Shapes
                if (mb.ButtonIndex == MouseButton.WheelUp) SwitchMode(_currentIndex + 1);
                if (mb.ButtonIndex == MouseButton.WheelDown) SwitchMode(_currentIndex - 1);
            }
        }

        /// <summary>
        /// Procedurally creates a circular bitmask for testing.
        /// Matches the Big-Endian bit order (0x80 = leftmost pixel).
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
                        int bw = size / 8 + 1;
                        mask.Data[y * bw + x / 8] |= (byte)(0x80 >> (x % 8));
                    }
                }
            }
            return mask;
        }
    }
}