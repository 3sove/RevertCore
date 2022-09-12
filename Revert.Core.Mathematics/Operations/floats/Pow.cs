namespace Revert.Core.Mathematics.Operations.floats
{
    public class Pow : FloatOperation
    {
        public Pow(float value) : base(value)
        {
        }

        public override float perform(float a, float b, float impact)
        {
            return a.interpolate(a.pow(b), impact);
        }
    }
}
