using System;

namespace DDTank.Shared
{
    /// <summary>
    /// Represents an x- and y-coordinate pair in 2D space.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// The X-coordinate.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y-coordinate.
        /// </summary>
        public int Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Point"/> struct.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets a point with coordinates (0, 0).
        /// </summary>
        public static Point Empty => new Point(0, 0);

        /// <summary>
        /// Calculates the Euclidean distance to another point.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <returns>The distance as a double.</returns>
        public double Distance(Point other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }
    }

    /// <summary>
    /// Stores a set of four integers that represent the location and size of a rectangle.
    /// </summary>
    public struct Rectangle
    {
        /// <summary>
        /// The X-coordinate of the upper-left corner.
        /// </summary>
        public int X;

        /// <summary>
        /// The Y-coordinate of the upper-left corner.
        /// </summary>
        public int Y;

        /// <summary>
        /// The width of the rectangle.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the rectangle.
        /// </summary>
        public int Height;

        /// <summary>
        /// Initializes a new instance of the <see cref="Rectangle"/> struct.
        /// </summary>
        /// <param name="x">The x-coordinate of the upper-left corner.</param>
        /// <param name="y">The y-coordinate of the upper-left corner.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Gets the x-coordinate of the left edge of this rectangle.
        /// </summary>
        public int Left => X;

        /// <summary>
        /// Gets the y-coordinate of the top edge of this rectangle.
        /// </summary>
        public int Top => Y;

        /// <summary>
        /// Gets the x-coordinate of the right edge of this rectangle.
        /// </summary>
        public int Right => X + Width;

        /// <summary>
        /// Gets the y-coordinate of the bottom edge of this rectangle.
        /// </summary>
        public int Bottom => Y + Height;

        /// <summary>
        /// Adjusts the location of this rectangle by the specified amount.
        /// </summary>
        /// <param name="x">The amount to offset the x-coordinate.</param>
        /// <param name="y">The amount to offset the y-coordinate.</param>
        public void Offset(int x, int y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        /// Determines if this rectangle intersects with another rectangle.
        /// </summary>
        /// <param name="other">The rectangle to test.</param>
        /// <returns>True if there is any intersection, otherwise false.</returns>
        public bool IntersectsWith(Rectangle other)
        {
            return !(other.Left >= Right ||
                     other.Right <= Left ||
                     other.Top >= Bottom ||
                     other.Bottom <= Top);
        }
    }
}