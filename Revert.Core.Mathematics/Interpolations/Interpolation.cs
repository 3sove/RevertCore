namespace Revert.Core.Mathematics.Interpolations
{
    /// <summary>
    /// Converted from badlogic's LibGdx interpolation class in Java
    /// </summary>
    public class Interpolation
    {
        /** @param a Alpha value between 0 and 1. */
        public virtual float apply(float a)
        {
            return a;
        }

        /** @param a Alpha value between 0 and 1. */
        public float apply(float start, float end, float a)
        {
            return start + (end - start) * apply(a);
        }


        public static Linear linear = new Linear();
        public static Smooth smooth = new Smooth();
        public static Smooth2 smooth2 = new Smooth2();
        public static Smoother smoother = new Smoother();

        public static Pow pow2 = new Pow(2);
        /** Slow, then fast. */
        public static PowIn pow2In = new PowIn(2);
        public static PowIn slowFast = pow2In;
        /** Fast, then slow. */
        public static PowOut pow2Out = new PowOut(2);
        public static PowOut fastSlow = pow2Out;


        public static Pow pow3 = new Pow(3);
        public static PowIn pow3In = new PowIn(3);
        public static PowOut pow3Out = new PowOut(3);

        public static Pow pow4 = new Pow(4);
        public static PowIn pow4In = new PowIn(4);
        public static PowOut pow4Out = new PowOut(4);

        public static Pow pow5 = new Pow(5);
        public static PowIn pow5In = new PowIn(5);
        public static PowOut pow5Out = new PowOut(5);


        public static Exp exp10 = new Exp(2, 10);
        public static ExpIn exp10In = new ExpIn(2, 10);
        public static ExpOut exp10Out = new ExpOut(2, 10);

        public static Exp exp5 = new Exp(2, 5);
        public static ExpIn exp5In = new ExpIn(2, 5);
        public static ExpOut exp5Out = new ExpOut(2, 5);


        public static Elastic elastic = new Elastic(2, 10, 7, 1);
        public static ElasticIn elasticIn = new ElasticIn(2, 10, 6, 1);
        public static ElasticOut elasticOut = new ElasticOut(2, 10, 7, 1);

        public static Swing swing = new Swing(1.5f);
        public static SwingIn swingIn = new SwingIn(2f);
        public static SwingOut swingOut = new SwingOut(2f);

        public static Bounce bounce = new Bounce(4);
        public static BounceIn bounceIn = new BounceIn(4);
        public static BounceOut bounceOut = new BounceOut(4);

    }
}
