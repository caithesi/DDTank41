using System;

namespace DDTank.Shared
{
    /// <summary>
    /// Represents a bitmask-based terrain tile or hole shape.
    /// Each byte in the data array represents 8 pixels (1 bit per pixel).
    /// </summary>
    public class Tile
    {
        private byte[] _data;
        private int _width;
        private int _height;
        private Rectangle _rect;
        private int _bw = 0; // Byte width
        private int _bh = 0; // Byte height
        private bool _digable;

        /// <summary>
        /// Gets the bounding rectangle of the tile.
        /// </summary>
        public Rectangle Bound => _rect;

        /// <summary>
        /// Gets the raw bitmask data. 1 = solid, 0 = empty.
        /// </summary>
        public byte[] Data => _data;

        /// <summary>
        /// Gets the width of the tile in pixels.
        /// </summary>
        public int Width => _width;

        /// <summary>
        /// Gets the height of the tile in pixels.
        /// </summary>
        public int Height => _height;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class with existing bitmask data.
        /// </summary>
        /// <param name="data">The bitmask data (8 pixels per byte).</param>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <param name="digable">Whether this tile can be modified by the Dig operation.</param>
        public Tile(byte[] data, int width, int height, bool digable)
        {
            _data = data;
            _width = width;
            _height = height;
            _digable = digable;
            _bw = _width / 8 + 1;
            _bh = _height;
            ScanBound();
        }

        /// <summary>
        /// Initializes an empty <see cref="Tile"/> with specified dimensions.
        /// </summary>
        /// <param name="width">Width in pixels.</param>
        /// <param name="height">Height in pixels.</param>
        /// <param name="digable">Whether this tile can be modified.</param>
        public Tile(int width, int height, bool digable)
        {
            _width = width;
            _height = height;
            _digable = digable;
            _bw = _width / 8 + 1;
            _bh = _height;
            _data = new byte[_bw * _bh];
            _rect = new Rectangle(0, 0, _width, _height);
        }

        /// <summary>
        /// Loads a <see cref="Tile"/> from a binary .bomb file.
        /// Expected format: [Int32 Width][Int32 Height][Raw Bitmask Data].
        /// </summary>
        /// <param name="file">Path to the .bomb file.</param>
        /// <param name="digable">Whether the loaded tile is digable.</param>
        public Tile(string file, bool digable)
        {
            using (var fs = System.IO.File.OpenRead(file))
            using (var reader = new System.IO.BinaryReader(fs))
            {
                _width = reader.ReadInt32();
                _height = reader.ReadInt32();
                _bw = _width / 8 + 1;
                _bh = _height;
                _data = reader.ReadBytes(_bw * _bh);
                _digable = digable;
                ScanBound();
            }
        }

        /// <summary>
        /// Scans the bitmask data to find the tightest bounding rectangle of solid pixels.
        /// Optimizes subsequent Dig and collision operations.
        /// </summary>
        private void ScanBound()
        {
            int minX = _width, minY = _height, maxX = 0, maxY = 0;
            bool found = false;

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (!IsEmpty(x, y))
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                        found = true;
                    }
                }
            }

            if (found)
                _rect = new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
            else
                _rect = new Rectangle(0, 0, 0, 0);
        }

        /// <summary>
        /// Perforates the tile using a surface mask and optionally adds a border.
        /// Typically used for weapon explosions.
        /// </summary>
        /// <param name="cx">Center X coordinate of the impact.</param>
        /// <param name="cy">Center Y coordinate of the impact.</param>
        /// <param name="surface">The shape of the hole to remove.</param>
        /// <param name="border">The shape of the border to add (optional).</param>
        public void Dig(int cx, int cy, Tile surface, Tile border)
        {
            if (_digable && surface != null)
            {
                // Calculate top-left corner so (cx, cy) is the center of the hole
                int x1 = (int)(cx - surface.Width / 2);
                int y1 = (int)(cy - surface.Height / 2);
                Remove(x1, y1, surface);

                if (border != null)
                {
                    x1 = (int)(cx - border.Width / 2);
                    y1 = (int)(cy - border.Height / 2);
                    Add(x1, y1, border);
                }
            }
        }

        /// <summary>
        /// Adds (ORs) another tile's solid pixels into this tile.
        /// Handles complex bit-shifting for sub-byte alignment.
        /// </summary>
        /// <param name="x">Target top-left X coordinate.</param>
        /// <param name="y">Target top-left Y coordinate.</param>
        /// <param name="tile">The tile to add.</param>
        protected void Add(int x, int y, Tile tile)
        {
            byte[] addData = tile._data;
            Rectangle rect = tile.Bound;
            rect.Offset(x, y);

            // Calculate intersection with current bounds
            int intersectX = Math.Max(rect.X, _rect.X);
            int intersectY = Math.Max(rect.Y, _rect.Y);
            int intersectW = Math.Min(rect.Right, _rect.Right) - intersectX;
            int intersectH = Math.Min(rect.Bottom, _rect.Bottom) - intersectY;

            if (intersectW > 0 && intersectH > 0)
            {
                int srcX = intersectX - x;
                int srcY = intersectY - y;

                int cx = srcX / 8; // Starting byte in source
                int cx2 = intersectX / 8; // Starting byte in destination
                int cy = srcY; // Starting row in source
                int cw = intersectW / 8 + 1; // Number of bytes to process per row
                int ch = intersectH;

                int b_offset = intersectX % 8; // Pixel offset within the byte

                for (int j = 0; j < ch; j++)
                {
                    int l_bits = 0;
                    for (int i = 0; i < cw + 1; i++)
                    {
                        int self_offset = (j + intersectY) * _bw + i + cx2;
                        if (self_offset >= _data.Length || i + cx2 >= _bw) break;

                        int tile_offset = (j + cy) * tile._bw + i + cx;
                        int src = (i < cw && tile_offset < addData.Length) ? addData[tile_offset] : 0;
                        
                        // Shift bits to align source byte with destination byte
                        int r_bits = src >> b_offset;
                        int combined = r_bits | l_bits;
                        
                        _data[self_offset] |= (byte)combined;

                        // Carry over remaining bits to the next byte
                        l_bits = src << (8 - b_offset);
                    }
                }
            }
        }

        /// <summary>
        /// Removes (AND NOTs) another tile's solid pixels from this tile.
        /// Handles complex bit-shifting for sub-byte alignment.
        /// </summary>
        /// <param name="x">Target top-left X coordinate.</param>
        /// <param name="y">Target top-left Y coordinate.</param>
        /// <param name="tile">The shape to remove.</param>
        protected void Remove(int x, int y, Tile tile)
        {
            byte[] addData = tile._data;
            Rectangle rect = tile.Bound;
            rect.Offset(x, y);

            int intersectX = Math.Max(rect.X, _rect.X);
            int intersectY = Math.Max(rect.Y, _rect.Y);
            int intersectW = Math.Min(rect.Right, _rect.Right) - intersectX;
            int intersectH = Math.Min(rect.Bottom, _rect.Bottom) - intersectY;

            if (intersectW > 0 && intersectH > 0)
            {
                int srcX = intersectX - x;
                int srcY = intersectY - y;

                int cx = srcX / 8;
                int cx2 = intersectX / 8;
                int cy = srcY;
                int cw = intersectW / 8 + 1;
                int ch = intersectH;

                int b_offset = intersectX % 8;

                for (int j = 0; j < ch; j++)
                {
                    int l_bits = 0;
                    for (int i = 0; i < cw + 1; i++)
                    {
                        int self_offset = (j + intersectY) * _bw + i + cx2;
                        if (self_offset >= _data.Length || i + cx2 >= _bw) break;

                        int tile_offset = (j + cy) * tile._bw + i + cx;
                        int src = (i < cw && tile_offset < addData.Length) ? addData[tile_offset] : 0;
                        
                        int r_bits = src >> b_offset;
                        int combined = r_bits | l_bits;

                        // Clear bits where the mask is 1
                        _data[self_offset] &= (byte)~combined;

                        l_bits = src << (8 - b_offset);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a specific pixel coordinate is empty (0) or out of bounds.
        /// </summary>
        /// <param name="x">X pixel coordinate.</param>
        /// <param name="y">Y pixel coordinate.</param>
        /// <returns>True if the pixel is empty or out of bounds, false otherwise.</returns>
        public bool IsEmpty(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                byte flag = (byte)(0x01 << (7 - x % 8));
                return (_data[y * _bw + x / 8] & flag) == 0;
            }
            return true;
        }
        
        /// <summary>
        /// Performs a quick check of the four corners of a rectangle to see if they are empty.
        /// </summary>
        /// <param name="rect">The rectangle to check.</param>
        /// <returns>True if all four corners are empty.</returns>
        public bool IsRectangleEmptyQuick(Rectangle rect)
        {
             if (IsEmpty(rect.Right, rect.Bottom) && 
                 IsEmpty(rect.Left, rect.Bottom) && 
                 IsEmpty(rect.Right, rect.Top) && 
                 IsEmpty(rect.Left, rect.Top)) return true;
             return false;
        }

        /// <summary>
        /// Creates a deep copy of this tile.
        /// </summary>
        /// <returns>A new <see cref="Tile"/> instance with cloned bitmask data.</returns>
        public Tile Clone()
        {
            return new Tile((byte[])_data.Clone(), _width, _height, _digable);
        }
    }
}