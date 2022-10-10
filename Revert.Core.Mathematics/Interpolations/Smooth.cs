namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    /** Aka "smoothstep". */
    public class Smooth : Interpolation
    {
        public override float apply(float a)
        {
            return a * a * (3 - 2 * a);
        }
    };
}
