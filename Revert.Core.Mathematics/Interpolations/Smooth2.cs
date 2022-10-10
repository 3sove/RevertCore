namespace Revert.Port.LibGDX.Mathematics.Interpolations
{
    public class Smooth2 : Interpolation
    {
        public override float apply(float a)
        {
            a = a * a * (3 - 2 * a);
            return a * a * (3 - 2 * a);
        }
    };
}
