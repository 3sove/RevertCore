namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class SwingIn : Interpolation
    {
        private float scale;

        public SwingIn(float scale)
        {
            this.scale = scale;
        }

        public override float apply(float a)
        {
            return a * a * ((scale + 1) * a - scale);
        }
    }
}
