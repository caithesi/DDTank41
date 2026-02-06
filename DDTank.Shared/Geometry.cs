using System;

namespace DDTank.Shared
{
    public struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point Empty => new Point(0, 0);

        public double Distance(Point other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int Left => X;
        public int Top => Y;
        public int Right => X + Width;
        public int Bottom => Y + Height;

        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        public bool IntersectsWith(Rectangle other)
        {
            return !(other.Left >= Right ||
                     other.Right <= Left ||
                     other.Top >= Bottom ||
                     other.Bottom <= Top);
        }
    }
}
