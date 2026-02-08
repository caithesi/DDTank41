using Godot;
using DDTank.Shared;
using System;
using System.Collections.Generic;

namespace DDTank.Godot.Example
{
    public class BallInfoMock
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Crater { get; set; }
        public string BombSound { get; set; }
    }

    /// <summary>
    /// A manual tester that links Ball metadata (IDs, Sounds) to Terrain Destruction.
    /// </summary>
    public partial class MapTestWithBallInfo : Node2D
    {
        [Export] public NodePath TerrainSpritePath;
        [Export] public NodePath MapBridgePath; 
        [Export] public string BombFolder = "res://bomb/";
        [Export] public string SoundFolder = "res://sound/";

        private DDTankMap _mapBridge;
        private List<BallInfoMock> _testBalls = new List<BallInfoMock>();
        private int _currentBallIndex = 0;
        private Tile _currentMask;
        private AudioStreamPlayer _audioPlayer;

        public override void _Ready()
        {
            GD.Print("MapTestWithBallInfo: Initializing with Mock Balls...");

            // 1. Setup Audio
            _audioPlayer = new AudioStreamPlayer();
            AddChild(_audioPlayer);

            // 2. Setup Mock Data (Representative samples from BallList.xml)
            _testBalls.Add(new BallInfoMock { ID = 1, Name = "Normal", Crater = "1", BombSound = "093" });
            _testBalls.Add(new BallInfoMock { ID = 4, Name = "Large", Crater = "4", BombSound = "088" });
            _testBalls.Add(new BallInfoMock { ID = 20, Name = "Thunder", Crater = "20", BombSound = "095" });
            _testBalls.Add(new BallInfoMock { ID = 22, Name = "Medical", Crater = "22", BombSound = "087" });

            // 3. Get nodes
            Sprite2D sprite = GetNode<Sprite2D>(TerrainSpritePath);
            _mapBridge = GetNode<DDTankMap>(MapBridgePath);

            if (sprite == null || _mapBridge == null)
            {
                GD.PrintErr("MapTestWithBallInfo: Sprite or Bridge not found!");
                return;
            }

            // 4. Initialize logical terrain (Solid)
            int w = (int)sprite.Texture.GetSize().X;
            int h = (int)sprite.Texture.GetSize().Y;
            Tile logic = new Tile(w, h, true);
            for (int i = 0; i < logic.Data.Length; i++) logic.Data[i] = 0xFF;

            _mapBridge.Initialize(logic, sprite);

            // 5. Select first ball
            SwitchBall(0);

            GD.Print("MapTestWithBallInfo: READY.");
            GD.Print(" -> LEFT CLICK: Dig & Play Sound");
            GD.Print(" -> MOUSE WHEEL: Cycle Balls");
        }

        private void SwitchBall(int index)
        {
            if (index < 0) index = _testBalls.Count - 1;
            if (index >= _testBalls.Count) index = 0;

            _currentBallIndex = index;
            BallInfoMock ball = _testBalls[_currentBallIndex];

            // Load Bomb Mask
            string bombPath = $"{BombFolder}{ball.Crater}.bomb";
            string globalPath = ProjectSettings.GlobalizePath(bombPath);
            
            if (FileAccess.FileExists(bombPath))
            {
                _currentMask = new Tile(globalPath, false);
                GD.Print($"[BALL: {ball.Name}] ID:{ball.ID} Crater:{ball.Crater} Sound:{ball.BombSound}");
            }
            else
            {
                GD.PrintErr($"[ERROR] Bomb file not found: {bombPath}. Falling back to circle.");
                _currentMask = CreateCircleMask(30);
            }
        }

        /// <summary>
        /// Attempts to find and play the bomb sound.
        /// Handles both pure IDs (093.mp3) and decompiled naming ([ID]_Sound093.mp3).
        /// </summary>
        private void PlayBombSound(string soundId)
        {
            string soundPath = FindSoundPath(soundId);
            
            if (soundPath != null)
            {
                AudioStream stream = GD.Load<AudioStream>(soundPath);
                _audioPlayer.Stream = stream;
                _audioPlayer.Play();
                GD.Print($" -> Playing Sound: {soundId} (Found at {soundPath})");
            }
            else
            {
                GD.Print($" -> [MOCK] Play Sound: {soundId} (No matching file in {SoundFolder})");
            }
        }

        private string FindSoundPath(string soundId)
        {
            // 1. Check for exact match (e.g. res://sound/093.mp3 or .wav)
            string[] extensions = { ".mp3", ".wav", ".ogg" };
            foreach (var ext in extensions)
            {
                string path = $"{SoundFolder}{soundId}{ext}";
                if (ResourceLoader.Exists(path)) return path;
            }

            // 2. Check for decompiled pattern (e.g. res://sound/*_Sound093.mp3)
            using var dir = DirAccess.Open(SoundFolder);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if (!dir.CurrentIsDir() && fileName.Contains($"Sound{soundId}"))
                    {
                        return SoundFolder + fileName;
                    }
                    fileName = dir.GetNext();
                }
            }

            return null;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb && mb.Pressed)
            {
                if (mb.ButtonIndex == MouseButton.Left)
                {
                    Vector2 pos = GetLocalMousePosition();
                    
                    // 1. Dig Map
                    _mapBridge.Dig((int)pos.X, (int)pos.Y, _currentMask, null);
                    
                    // 2. Play Sound
                    PlayBombSound(_testBalls[_currentBallIndex].BombSound);
                }
                
                if (mb.ButtonIndex == MouseButton.WheelUp) SwitchBall(_currentBallIndex + 1);
                if (mb.ButtonIndex == MouseButton.WheelDown) SwitchBall(_currentBallIndex - 1);
            }
        }

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