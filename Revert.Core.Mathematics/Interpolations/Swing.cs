namespace Revert.Core.Mathematics.Interpolations
{
    public class Swing : Interpolation
    {
        private float scale;

        public Swing(float scale)
        {
            this.scale = scale * 2;
        }

        public override float apply(float a)
        {
            if (a <= 0.5f)
            {
                a *= 2;
                return a * a * ((scale + 1) * a - scale) / 2;
            }
            a--;
            a *= 2;
            return a * a * ((scale + 1) * a + scale) / 2 + 1;
        }
    }
}
