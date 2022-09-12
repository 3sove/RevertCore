using Revert.Core.Mathematics.Interpolations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revert.Core.Graphics
{
    public static class Daylight
    {
        public static float GetLightness(float time)
        {
            if (time < 0f || time > 1f) return 1f;

            if (time < .5f)
                return Interpolation.smoother.apply(time * 2);

            var timeDifference = time - .5f;
            return Interpolation.smoother.apply(1 - timeDifference * 2);
        }
    }
}
