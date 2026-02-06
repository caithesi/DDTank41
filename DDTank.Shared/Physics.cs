namespace DDTank.Shared
{
    public abstract class Physics
    {
        protected int m_id;
        protected bool m_isLiving;
        protected bool m_isMoving;
        protected IMap? m_map;
        protected Rectangle m_rect;
        protected int m_x;
        protected int m_y;

        public int Id => m_id;
        public int X => m_x;
        public int Y => m_y;
        public bool IsLiving => m_isLiving;
        public bool IsMoving => m_isMoving;
        public Rectangle Bound => m_rect;

        public Physics(int id)
        {
            m_id = id;
            m_rect = new Rectangle(-5, -5, 10, 10);
            m_isLiving = true;
        }

        public virtual void SetMap(IMap map)
        {
            m_map = map;
        }

        public virtual void SetXY(int x, int y)
        {
            m_x = x;
            m_y = y;
        }

        public virtual void StartMoving()
        {
            if (m_map != null)
            {
                m_isMoving = true;
            }
        }

        public virtual void StopMoving()
        {
            m_isMoving = false;
        }

        public virtual void Die()
        {
            StopMoving();
            m_isLiving = false;
        }
    }
}
