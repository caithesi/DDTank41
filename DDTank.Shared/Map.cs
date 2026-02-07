using System;
using System.Collections.Generic;

namespace DDTank.Shared
{
    /// <summary>
    /// Represents the game world, managing terrain layers and physical objects.
    /// It handles collision detection between objects and the terrain.
    /// </summary>
    public class Map : IMap
    {
        protected Tile _layer1; // Ground (Destructible)
        protected Tile _layer2; // Indestructible objects/terrain
        protected Rectangle _bound;
        protected float _gravity = 9.8f;
        protected float _wind = 0.0f;
        protected float _airResistance = 0.0f;
        
        private HashSet<Physics> _physicsObjects;

        /// <summary>
        /// Gets or sets the gravity strength.
        /// </summary>
        public float Gravity { get => _gravity; set => _gravity = value; }

        /// <summary>
        /// Gets or sets the current wind strength.
        /// </summary>
        public float Wind { get => _wind; set => _wind = value; }

        /// <summary>
        /// Gets or sets the air resistance factor.
        /// </summary>
        public float AirResistance { get => _airResistance; set => _airResistance = value; }
        
        /// <summary>
        /// Gets the primary destructible terrain layer.
        /// </summary>
        public Tile Ground => _layer1;

        /// <summary>
        /// Gets the indestructible terrain layer.
        /// </summary>
        public Tile DeadTile => _layer2;

        /// <summary>
        /// Gets the map boundaries.
        /// </summary>
        public Rectangle Bound => _bound;

        /// <summary>
        /// Initializes a new instance of the <see cref="Map"/> class.
        /// </summary>
        /// <param name="layer1">The primary destructible terrain.</param>
        /// <param name="layer2">The indestructible terrain layer.</param>
        public Map(Tile layer1, Tile layer2)
        {
            _layer1 = layer1;
            _layer2 = layer2;
            _physicsObjects = new HashSet<Physics>();

            if (_layer1 != null)
            {
                _bound = new Rectangle(0, 0, _layer1.Width, _layer1.Height);
            }
            else if (_layer2 != null)
            {
                _bound = new Rectangle(0, 0, _layer2.Width, _layer2.Height);
            }
        }

        /// <summary>
        /// Applies a destruction mask to the primary terrain layer.
        /// </summary>
        /// <param name="cx">Impact center X.</param>
        /// <param name="cy">Impact center Y.</param>
        /// <param name="surface">Hole shape.</param>
        /// <param name="border">Border shape (optional).</param>
        public void Dig(int cx, int cy, Tile surface, Tile border)
        {
            if (_layer1 != null)
            {
                _layer1.Dig(cx, cy, surface, border);
            }
        }

        /// <summary>
        /// Checks if a pixel coordinate is empty in all layers.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>True if all layers are empty at the given point.</returns>
        public bool IsEmpty(int x, int y)
        {
            if (_layer1 != null && !_layer1.IsEmpty(x, y)) return false;
            if (_layer2 != null) return _layer2.IsEmpty(x, y);
            return true;
        }

        /// <summary>
        /// Checks if a rectangular area is free of any terrain (all layers).
        /// </summary>
        /// <param name="rect">The area to check.</param>
        /// <returns>True if no solid pixels are found in the area.</returns>
        public bool IsRectangleEmpty(Rectangle rect)
        {
            if (_layer1 != null && !_layer1.IsRectangleEmptyQuick(rect)) return false;
            if (_layer2 != null) return _layer2.IsRectangleEmptyQuick(rect);
            return true;
        }

        /// <summary>
        /// Checks if a point is outside the map boundaries.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>True if outside the left/right bounds or below the bottom.</returns>
        public bool IsOutMap(int x, int y)
        {
            if (x >= _bound.Left && x < _bound.Right)
            {
                return y > _bound.Bottom;
            }
            return true;
        }

        /// <summary>
        /// Registers a physical object with this map.
        /// </summary>
        /// <param name="phy">The physical object to add.</param>
        public void AddPhysical(Physics phy)
        {
            phy.SetMap(this);
            lock (_physicsObjects)
            {
                _physicsObjects.Add(phy);
            }
        }

        /// <summary>
        /// Unregisters a physical object from this map.
        /// </summary>
        /// <param name="phy">The physical object to remove.</param>
        public void RemovePhysical(Physics phy)
        {
            phy.SetMap(null);
            lock (_physicsObjects)
            {
                _physicsObjects.Remove(phy);
            }
        }

        /// <summary>
        /// Searches for physical objects (like players or bombs) that intersect with a rectangle.
        /// </summary>
        /// <param name="rect">The search area.</param>
        /// <param name="except">An object to ignore during the search.</param>
        /// <returns>An array of intersecting physical objects.</returns>
        public Physics[] FindPhysicalObjects(Rectangle rect, Physics except)
        {
            List<Physics> list = new List<Physics>();
            lock (_physicsObjects)
            {
                foreach (Physics phy in _physicsObjects)
                {
                    if (phy.IsLiving && phy != except)
                    {
                        Rectangle b1 = phy.Bound;
                        b1.Offset(phy.X, phy.Y);
                        if (b1.IntersectsWith(rect))
                        {
                            list.Add(phy);
                        }
                    }
                }
            }
            return list.ToArray();
        }
    }
}