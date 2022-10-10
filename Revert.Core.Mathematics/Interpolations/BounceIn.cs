namespace Revert.Core.Mathematics.Interpolations
{
    public class BounceIn : BounceOut
    {
        public BounceIn(float[] widths, float[] heights) : base(widths, heights)
        {
        }

        public BounceIn(int bounces) : base(bounces)
        {
        }

        public override float apply(float a)
        {
            return 1 - base.apply(1 - a);
        }
    }
}
