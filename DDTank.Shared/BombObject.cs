using DDTank.Shared.Maths;
using System;

namespace DDTank.Shared
{
    public class BombObject : Physics
    {
        private float m_mass;
        private float m_gravityFactor;
        private float m_windFactor;
        private float m_airResitFactor;

        private float m_arf; // air resistance force
        private float m_gf;  // gravity force
        private float m_wf;  // wind force

        private EulerVector m_vx;
        private EulerVector m_vy;

        public float vX => m_vx.x1;
        public float vY => m_vy.x1;

        public BombObject(int id, float mass, float gravityFactor, float windFactor, float airResitFactor)
            : base(id)
        {
            m_mass = mass;
            m_gravityFactor = gravityFactor;
            m_windFactor = windFactor;
            m_airResitFactor = airResitFactor;
            m_vx = new EulerVector(0, 0, 0f);
            m_vy = new EulerVector(0, 0, 0f);
            m_rect = new Rectangle(-3, -3, 6, 6);
        }

        public override void SetMap(IMap map)
        {
            base.SetMap(map);
            UpdateAGW();
        }

        public override void SetXY(int x, int y)
        {
            base.SetXY(x, y);
            m_vx.x0 = x;
            m_vy.x0 = y;
        }

        public void SetSpeedXY(float vx, float vy)
        {
            m_vx.x1 = vx;
            m_vy.x1 = vy;
        }

        private void UpdateAGW()
        {
            if (m_map != null)
            {
                m_arf = m_map.AirResistance * m_airResitFactor;
                m_gf = m_map.Gravity * m_gravityFactor * m_mass;
                m_wf = m_map.Wind * m_windFactor;
            }
        }

        protected Point CompleteNextMovePoint(float dt)
        {
            m_vx.ComputeOneEulerStep(m_mass, m_arf, m_wf, dt);
            m_vy.ComputeOneEulerStep(m_mass, m_arf, m_gf, dt);
            return new Point((int)m_vx.x0, (int)m_vy.x0);
        }

        public void MoveTo(int px, int py)
        {
            if (m_map == null || (px == m_x && py == m_y)) return;

            int dx = px - m_x;
            int dy = py - m_y;
            bool useX = Math.Abs(dx) > Math.Abs(dy);
            int steps = useX ? Math.Abs(dx) : Math.Abs(dy);
            int direction = (useX ? dx : dy) > 0 ? 1 : -1;

            for (int i = 1; i <= steps; i += 3)
            {
                int curX, curY;
                if (useX)
                {
                    curX = m_x + i * direction;
                    curY = (steps == 0) ? m_y : m_y + i * direction * dy / steps;
                }
                else
                {
                    curY = m_y + i * direction;
                    curX = (steps == 0) ? m_x : m_x + i * direction * dx / steps;
                }

                Rectangle nextRect = m_rect;
                nextRect.Offset(curX, curY);

                Physics[] collisions = m_map.FindPhysicalObjects(nextRect, this);
                if (collisions.Length > 0)
                {
                    base.SetXY(curX, curY);
                    CollideObjects(collisions);
                }
                else if (!m_map.IsRectangleEmpty(nextRect))
                {
                    base.SetXY(curX, curY);
                    CollideGround();
                }
                else if (m_map.IsOutMap(curX, curY))
                {
                    base.SetXY(curX, curY);
                    FlyoutMap();
                }

                if (!m_isLiving || !m_isMoving) return;
            }
            base.SetXY(px, py);
        }

        protected virtual void CollideGround() => StopMoving();
        protected virtual void CollideObjects(Physics[] list) { }
        protected virtual void FlyoutMap() { StopMoving(); Die(); }

        public virtual void Update(float dt)
        {
            if (m_isMoving && m_isLiving)
            {
                Point nextPoint = CompleteNextMovePoint(dt);
                MoveTo(nextPoint.X, nextPoint.Y);
            }
        }
    }
}
