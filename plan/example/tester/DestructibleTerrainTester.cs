using System;
using DDTank.Shared;

namespace DDTank.Test
{
    /// <summary>
    /// A tester class to verify the Destructible Terrain logic in DDTank.Shared.
    /// It simulates map creation, terrain digging, and collision detection.
    /// </summary>
    public class DestructibleTerrainTester
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== Starting Destructible Terrain Test ===");
            
            try 
            {
                TestSimpleDig();
                TestSubByteAlignment();
                TestIndestructibleLayer();
                TestOutMap();
                
                Console.WriteLine("
=== All Tests Passed Successfully ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"
!!! Test Failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Tests a basic dig operation on a solid map.
        /// </summary>
        public static void TestSimpleDig()
        {
            Console.WriteLine("Running: TestSimpleDig...");

            // 1. Create a solid 100x100 map
            int width = 100;
            int height = 100;
            Tile ground = CreateSolidTile(width, height, true);
            Map map = new Map(ground, null);

            // Verify initial state
            if (map.IsEmpty(50, 50)) throw new Exception("Point (50,50) should be solid initially.");

            // 2. Create a 20x20 circular hole mask
            Tile hole = CreateCircleTile(20);

            // 3. Dig at the center (50, 50)
            map.Dig(50, 50, hole, null);

            // 4. Verify the hole
            // Center should be empty
            if (!map.IsEmpty(50, 50)) throw new Exception("Center (50,50) should be empty after digging.");
            
            // Edge of the hole (50 + 9, 50) should be empty (radius is 10)
            if (!map.IsEmpty(59, 50)) throw new Exception("Point (59,50) should be empty after digging.");
            
            // Just outside the hole (50 + 11, 50) should still be solid
            if (map.IsEmpty(61, 50)) throw new Exception("Point (61,50) should still be solid.");

            Console.WriteLine("  - OK: Basic dig and IsEmpty check.");
        }

        /// <summary>
        /// Tests digging at coordinates that are not aligned to 8-pixel boundaries.
        /// This verifies the complex bit-shifting logic in Tile.Remove.
        /// </summary>
        public static void TestSubByteAlignment()
        {
            Console.WriteLine("Running: TestSubByteAlignment...");

            Tile ground = CreateSolidTile(100, 100, true);
            Map map = new Map(ground, null);

            // Dig at (53, 50). 53 % 8 = 5 (Not aligned)
            Tile hole = CreateCircleTile(10); // 10x10 hole
            map.Dig(53, 50, hole, null);

            if (!map.IsEmpty(53, 50)) throw new Exception("Center (53,50) should be empty.");
            if (!map.IsEmpty(53 - 4, 50)) throw new Exception("Left edge of hole (49,50) should be empty.");
            if (!map.IsEmpty(53 + 4, 50)) throw new Exception("Right edge of hole (57,50) should be empty.");
            if (map.IsEmpty(53 + 6, 50)) throw new Exception("Outside right (59,50) should be solid.");

            Console.WriteLine("  - OK: Sub-byte alignment shift logic.");
        }

        /// <summary>
        /// Tests that digging does not affect indestructible layers.
        /// </summary>
        public static void TestIndestructibleLayer()
        {
            Console.WriteLine("Running: TestIndestructibleLayer...");

            Tile ground = CreateSolidTile(100, 100, true);
            // Create a small indestructible "anchor" in the middle
            Tile dead = new Tile(100, 100, false);
            FillRect(dead, 45, 45, 10, 10); // 10x10 solid block in dead layer

            Map map = new Map(ground, dead);

            // Dig a large hole that covers the anchor
            Tile largeHole = CreateCircleTile(40);
            map.Dig(50, 50, largeHole, null);

            // Ground should be empty at (50,50)
            if (!ground.IsEmpty(50, 50)) throw new Exception("Ground layer should be empty at (50,50).");
            
            // BUT Map.IsEmpty should return FALSE because the 'dead' layer is solid there
            if (map.IsEmpty(50, 50)) throw new Exception("Map.IsEmpty should be FALSE because of indestructible layer.");
            
            // Outside the anchor but inside the hole should be empty
            if (!map.IsEmpty(30, 50)) throw new Exception("Point (30,50) should be empty (only ground was there).");

            Console.WriteLine("  - OK: Indestructible layer protection.");
        }

        /// <summary>
        /// Tests boundary conditions.
        /// </summary>
        public static void TestOutMap()
        {
            Console.WriteLine("Running: TestOutMap...");

            Tile ground = CreateSolidTile(100, 100, true);
            Map map = new Map(ground, null);

            if (!map.IsOutMap(-1, 50)) throw new Exception("(-1, 50) should be out of map.");
            if (!map.IsOutMap(100, 50)) throw new Exception("(100, 50) should be out of map.");
            if (!map.IsOutMap(50, 101)) throw new Exception("(50, 101) should be out of map (below bottom).");
            if (map.IsOutMap(50, -10)) throw new Exception("(50, -10) should NOT be out of map (above top is valid for projectiles).");

            Console.WriteLine("  - OK: Boundary checks.");
        }

        // --- Helper Methods ---

        private static Tile CreateSolidTile(int w, int h, bool digable)
        {
            Tile tile = new Tile(w, h, digable);
            byte[] data = tile.Data;
            for (int i = 0; i < data.Length; i++) data[i] = 0xFF; // All solid
            return tile;
        }

        private static Tile CreateCircleTile(int size)
        {
            Tile tile = new Tile(size, size, false);
            int r = size / 2;
            int cx = r;
            int cy = r;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double dist = Math.Sqrt(Math.Pow(x - cx, 2) + Math.Pow(y - cy, 2));
                    if (dist < r)
                    {
                        SetPixel(tile, x, y, true);
                    }
                }
            }
            return tile;
        }

        private static void FillRect(Tile tile, int x, int y, int w, int h)
        {
            for (int j = y; j < y + h; j++)
            {
                for (int i = x; i < x + w; i++)
                {
                    SetPixel(tile, i, j, true);
                }
            }
        }

        private static void SetPixel(Tile tile, int x, int y, bool solid)
        {
            if (x < 0 || x >= tile.Width || y < 0 || y >= tile.Height) return;
            
            int bw = tile.Width / 8 + 1;
            int byteIdx = y * bw + x / 8;
            byte bitMask = (byte)(0x01 << (7 - x % 8));

            if (solid)
                tile.Data[byteIdx] |= bitMask;
            else
                tile.Data[byteIdx] &= (byte)~bitMask;
        }
    }
}