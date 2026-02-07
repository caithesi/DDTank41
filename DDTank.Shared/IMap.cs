namespace DDTank.Shared
{
    /// <summary>
    /// Defines the contract for a game map, providing environment physics and collision detection.
    /// This interface allows the Shared logic to interact with different map implementations (Server or Godot).
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// Gets the gravity force applied to objects in this map.
        /// </summary>
        float Gravity { get; }

        /// <summary>
        /// Gets the current wind strength and direction.
        /// </summary>
        float Wind { get; }

        /// <summary>
        /// Gets the air resistance factor.
        /// </summary>
        float AirResistance { get; }
        
        /// <summary>
        /// Checks if a rectangular area is free of any solid terrain.
        /// </summary>
        /// <param name="rect">The area to check.</param>
        /// <returns>True if the rectangle is empty, false if it overlaps with solid terrain.</returns>
        bool IsRectangleEmpty(Rectangle rect);

        /// <summary>
        /// Checks if a point is outside the map boundaries.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <returns>True if the point is outside the map.</returns>
        bool IsOutMap(int x, int y);

        /// <summary>
        /// Finds all physical objects within a specific area, excluding a given object.
        /// </summary>
        /// <param name="rect">The search area.</param>
        /// <param name="except">The object to exclude from the search (e.g., the object doing the searching).</param>
        /// <returns>An array of <see cref="Physics"/> objects found.</returns>
        Physics[] FindPhysicalObjects(Rectangle rect, Physics except);

        /// <summary>
        /// Modifies the map terrain by removing a hole shape and optionally adding a border.
        /// </summary>
        /// <param name="cx">The center X of the impact.</param>
        /// <param name="cy">The center Y of the impact.</param>
        /// <param name="surface">The shape of the hole to create.</param>
        /// <param name="border">The shape of the border to add (optional).</param>
        void Dig(int cx, int cy, Tile surface, Tile border);
    }
}