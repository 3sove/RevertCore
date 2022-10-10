namespace Revert.Core.Mathematics.Interpolations
{
    /** By Ken Perlin. */
    public class Smoother : Interpolation
    {
        public override float apply(float a)
        {
            return a * a * a * (a * (a * 6 - 15) + 10);
        }
    };
}
