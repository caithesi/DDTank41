namespace DDTank.Shared.Maths
{
    public class EulerVector
    {
        public float x0; // Position
        public float x1; // Velocity
        public float x2; // Acceleration

        public EulerVector(float x0, float x1, float x2)
        {
            this.x0 = x0;
            this.x1 = x1;
            this.x2 = x2;
        }

        public void Clear()
        {
            x0 = 0f;
            x1 = 0f;
            x2 = 0f;
        }

        public void ClearMotion()
        {
            x1 = 0f;
            x2 = 0f;
        }

        public void ComputeOneEulerStep(float mass, float airResistance, float force, float dt)
        {
            x2 = (force - airResistance * x1) / mass;
            x1 += x2 * dt;
            x0 += x1 * dt;
        }

        public override string ToString()
        {
            return $"x:{x0}, v:{x1}, a:{x2}";
        }
    }
}
