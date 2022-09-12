namespace Revert.Core.Mathematics.Operations.floats
{
    public class Root : FloatOperation
    {
        public Root(float value) : base(value)
        {
        }

        public override float perform(float a, float b, float impact)
        {
            return a.interpolate(a.pow(1f / b), impact);
        }
    }
}
