namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class SwingOut : Interpolation
    {
        private float scale;

        public SwingOut(float scale)
        {
            this.scale = scale;
        }

        public override float apply(float a)
        {
            a--;
            return a * a * ((scale + 1) * a + scale) + 1;
        }
    }
}
