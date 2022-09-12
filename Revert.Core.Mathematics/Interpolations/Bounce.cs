namespace Revert.Core.Mathematics.Interpolations
{
    //

    public class Bounce : BounceOut
    {
        public Bounce(float[] widths, float[] heights) : base(widths, heights)
        {
        }

        public Bounce(int bounces) : base(bounces)
        {
        }

        private float doBounce(float a)
        {
            float test = a + widths[0] / 2;
            if (test < widths[0]) return test / (widths[0] / 2) - 1;
            return base.apply(a);
        }

        public override float apply(float a)
        {
            if (a <= 0.5f) return (1 - doBounce(1 - a * 2)) / 2;
            return doBounce(a * 2 - 1) / 2 + 0.5f;
        }
    }
}
