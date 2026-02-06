using System;

namespace DDTank.Shared
{
    public class Tile
    {
        private byte[] _data;
        private int _width;
        private int _height;
        private Rectangle _rect;
        private int _bw = 0;
        private int _bh = 0;
        private bool _digable;

        public Rectangle Bound => _rect;
        public byte[] Data => _data;
        public int Width => _width;
        public int Height => _height;

        public Tile(byte[] data, int width, int height, bool digable)
        {
            _data = data;
            _width = width;
            _height = height;
            _digable = digable;
            _bw = _width / 8 + 1;
            _bh = _height;
            _rect = new Rectangle(0, 0, _width, _height);
        }

        // New constructor for clean initialization
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

        public void Dig(int cx, int cy, Tile surface, Tile border)
        {
            if (_digable && surface != null)
            {
                int x1 = (int)(cx - surface.Width / 2);
                int y1 = (int)(cy - surface.Height / 2);
                Remove(x1, y1, surface);

                if (border != null)
                {
                    x1 = (int)(cx - border.Width / 2);
                    y1 = (int)(cy - border.Height / 2);
                    Add(x1, y1, border); // Fixed logic: Was surface in original code, likely meant border
                }
            }
        }

        protected void Add(int x, int y, Tile tile)
        {
             byte[] addData = tile._data;
             Rectangle rect = tile.Bound;
             rect.Offset(x, y);
             // Manually intersect
             // rect.Intersect(_rect); 
             // Since we don't have System.Drawing.Rectangle.Intersect, we implement logic here if needed or assume safe calls.
             // For strict parity, we should implement intersection logic.
             // Let's implement a safe check:
             
             int intersectX = Math.Max(rect.X, _rect.X);
             int intersectY = Math.Max(rect.Y, _rect.Y);
             int intersectW = Math.Min(rect.Right, _rect.Right) - intersectX;
             int intersectH = Math.Min(rect.Bottom, _rect.Bottom) - intersectY;

             if (intersectW > 0 && intersectH > 0)
             {
                 // Re-adjust relative to the tile we are adding
                 int srcX = intersectX - x; 
                 int srcY = intersectY - y;
                 
                 int cx = srcX / 8;
                 int cx2 = intersectX / 8;
                 int cy = srcY;
                 int cw = intersectW / 8 + 1; // Safety buffer
                 int ch = intersectH;

                 // Logic copied and adapted from Remove but inverted for OR operation
                 // Note: The original Add method was commented out or incomplete. 
                 // We will implement a basic bitwise OR for "Filling" logic.
                 
                 for (int j = 0; j < ch; j++)
                 {
                     for (int i = 0; i < cw; i++)
                     {
                         // Simple bounds safety
                         int destIndex = (j + intersectY) * _bw + (cx2 + i);
                         int srcIndex = (j + srcY) * tile._bw + (cx + i);
                         
                         if(destIndex < _data.Length && srcIndex < addData.Length)
                             _data[destIndex] |= addData[srcIndex];
                     }
                 }
             }
        }

        protected void Remove(int x, int y, Tile tile)
        {
            byte[] addData = tile._data;
            Rectangle rect = tile.Bound;
            rect.Offset(x, y);

            // Manual Intersection
            int intersectX = Math.Max(rect.X, _rect.X);
            int intersectY = Math.Max(rect.Y, _rect.Y);
            int intersectW = Math.Min(rect.Right, _rect.Right) - intersectX;
            int intersectH = Math.Min(rect.Bottom, _rect.Bottom) - intersectY;

            if (intersectW > 0 && intersectH > 0)
            {
                // Offset back to 0,0 relative logic of the original code
                // original: rect.Offset(-x, -y);
                int relativeX = intersectX - x;
                int relativeY = intersectY - y;

                int cx = relativeX / 8;
                int cx2 = intersectX / 8; // Destination offset
                
                int cy = relativeY;
                int cw = intersectW / 8 + 1;
                int ch = intersectH;

                // Optimization: The original code had separate loops for X==0 and X!=0.
                // We will use a simplified robust loop for portability.
                
                int b_offset = intersectX % 8;
                
                for (int j = 0; j < ch; j++)
                {
                    for (int i = 0; i < cw; i++)
                    {
                        int self_offset = (j + intersectY) * _bw + i + cx2;
                        int tile_offset = (j + cy) * tile._bw + i + cx;

                        if (self_offset >= _data.Length || tile_offset >= addData.Length) continue;

                        int src = addData[tile_offset];
                        
                        // Handle bit shifting for alignment
                        // This logic mirrors the "dig" effect
                        int r_bits = src >> b_offset;
                        int l_bits = src << (8 - b_offset);

                        int target = _data[self_offset];
                        target &= ~r_bits;
                        
                        if (i > 0 && self_offset - 1 >= 0)
                        {
                            // Apply left bits to previous byte
                             int prev_target = _data[self_offset - 1];
                             prev_target &= ~l_bits;
                             _data[self_offset - 1] = (byte)prev_target;
                        }

                        _data[self_offset] = (byte)target;
                    }
                }
            }
        }

        public bool IsEmpty(int x, int y)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                byte flag = (byte)(0x01 << (7 - x % 8));
                return (_data[y * _bw + x / 8] & flag) == 0;
            }
            return true;
        }
        
        public bool IsRectangleEmptyQuick(Rectangle rect)
        {
             // Simplified quick check
             if (IsEmpty(rect.Right, rect.Bottom) && 
                 IsEmpty(rect.Left, rect.Bottom) && 
                 IsEmpty(rect.Right, rect.Top) && 
                 IsEmpty(rect.Left, rect.Top)) return true;
             return false;
        }

        public Tile Clone()
        {
            return new Tile((byte[])_data.Clone(), _width, _height, _digable);
        }
    }
}
