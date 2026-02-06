using System;
using System.Collections.Generic;

namespace DDTank.Shared
{
    public class Map : IMap
    {
        protected Tile _layer1; // Ground
        protected Tile _layer2; // Indestructible objects or other layers
        protected Rectangle _bound;
        protected float _gravity = 9.8f;
        protected float _wind = 0.0f;
        protected float _airResistance = 0.0f;
        
        private HashSet<Physics> _physicsObjects;

        public float Gravity { get => _gravity; set => _gravity = value; }
        public float Wind { get => _wind; set => _wind = value; }
        public float AirResistance { get => _airResistance; set => _airResistance = value; }
        
        public Tile Ground => _layer1;
        public Tile DeadTile => _layer2;
        public Rectangle Bound => _bound;

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

        public void Dig(int cx, int cy, Tile surface, Tile border)
        {
            if (_layer1 != null)
            {
                _layer1.Dig(cx, cy, surface, border);
            }
            // Usually we don't dig layer 2 (DeadTile) but logic can be added here
        }

        public bool IsEmpty(int x, int y)
        {
            if (_layer1 != null && !_layer1.IsEmpty(x, y)) return false;
            if (_layer2 != null) return _layer2.IsEmpty(x, y);
            return true;
        }

        public bool IsRectangleEmpty(Rectangle rect)
        {
            if (_layer1 != null && !_layer1.IsRectangleEmptyQuick(rect)) return false;
            if (_layer2 != null) return _layer2.IsRectangleEmptyQuick(rect);
            return true;
        }

        public bool IsOutMap(int x, int y)
        {
            if (x >= _bound.X && x <= _bound.Width)
            {
                return y > _bound.Height;
            }
            return true;
        }

        public void AddPhysical(Physics phy)
        {
            phy.SetMap(this);
            lock (_physicsObjects)
            {
                _physicsObjects.Add(phy);
            }
        }

        public void RemovePhysical(Physics phy)
        {
            phy.SetMap(null);
            lock (_physicsObjects)
            {
                _physicsObjects.Remove(phy);
            }
        }

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
