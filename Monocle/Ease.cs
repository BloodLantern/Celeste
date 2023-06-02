using System;

namespace Monocle
{
    public static class Ease
    {
        public static readonly Ease.Easer Linear = (Ease.Easer) (t => t);
        public static readonly Ease.Easer SineIn = (Ease.Easer) (t => (float) (-Math.Cos(1.5707963705062866 * (double) t) + 1.0));
        public static readonly Ease.Easer SineOut = (Ease.Easer) (t => (float) Math.Sin(1.5707963705062866 * (double) t));
        public static readonly Ease.Easer SineInOut = (Ease.Easer) (t => (float) (-Math.Cos(3.1415927410125732 * (double) t) / 2.0 + 0.5));
        public static readonly Ease.Easer QuadIn = (Ease.Easer) (t => t * t);
        public static readonly Ease.Easer QuadOut = Ease.Invert(Ease.QuadIn);
        public static readonly Ease.Easer QuadInOut = Ease.Follow(Ease.QuadIn, Ease.QuadOut);
        public static readonly Ease.Easer CubeIn = (Ease.Easer) (t => t * t * t);
        public static readonly Ease.Easer CubeOut = Ease.Invert(Ease.CubeIn);
        public static readonly Ease.Easer CubeInOut = Ease.Follow(Ease.CubeIn, Ease.CubeOut);
        public static readonly Ease.Easer QuintIn = (Ease.Easer) (t => t * t * t * t * t);
        public static readonly Ease.Easer QuintOut = Ease.Invert(Ease.QuintIn);
        public static readonly Ease.Easer QuintInOut = Ease.Follow(Ease.QuintIn, Ease.QuintOut);
        public static readonly Ease.Easer ExpoIn = (Ease.Easer) (t => (float) Math.Pow(2.0, 10.0 * ((double) t - 1.0)));
        public static readonly Ease.Easer ExpoOut = Ease.Invert(Ease.ExpoIn);
        public static readonly Ease.Easer ExpoInOut = Ease.Follow(Ease.ExpoIn, Ease.ExpoOut);
        public static readonly Ease.Easer BackIn = (Ease.Easer) (t => (float) ((double) t * (double) t * (2.7015800476074219 * (double) t - 1.7015800476074219)));
        public static readonly Ease.Easer BackOut = Ease.Invert(Ease.BackIn);
        public static readonly Ease.Easer BackInOut = Ease.Follow(Ease.BackIn, Ease.BackOut);
        public static readonly Ease.Easer BigBackIn = (Ease.Easer) (t => (float) ((double) t * (double) t * (4.0 * (double) t - 3.0)));
        public static readonly Ease.Easer BigBackOut = Ease.Invert(Ease.BigBackIn);
        public static readonly Ease.Easer BigBackInOut = Ease.Follow(Ease.BigBackIn, Ease.BigBackOut);
        public static readonly Ease.Easer ElasticIn = (Ease.Easer) (t =>
        {
            float num1 = t * t;
            float num2 = num1 * t;
            return (float) (33.0 * (double) num2 * (double) num1 + -59.0 * (double) num1 * (double) num1 + 32.0 * (double) num2 + -5.0 * (double) num1);
        });
        public static readonly Ease.Easer ElasticOut = (Ease.Easer) (t =>
        {
            float num3 = t * t;
            float num4 = num3 * t;
            return (float) (33.0 * (double) num4 * (double) num3 + -106.0 * (double) num3 * (double) num3 + 126.0 * (double) num4 + -67.0 * (double) num3 + 15.0 * (double) t);
        });
        public static readonly Ease.Easer ElasticInOut = Ease.Follow(Ease.ElasticIn, Ease.ElasticOut);
        private const float B1 = 0.363636374f;
        private const float B2 = 0.727272749f;
        private const float B3 = 0.545454562f;
        private const float B4 = 0.909090936f;
        private const float B5 = 0.8181818f;
        private const float B6 = 0.954545438f;
        public static readonly Ease.Easer BounceIn = (Ease.Easer) (t =>
        {
            t = 1f - t;
            if ((double) t < 0.36363637447357178)
                return (float) (1.0 - 121.0 / 16.0 * (double) t * (double) t);
            if ((double) t < 0.72727274894714355)
                return (float) (1.0 - (121.0 / 16.0 * ((double) t - 0.54545456171035767) * ((double) t - 0.54545456171035767) + 0.75));
            return (double) t < 0.90909093618392944 ? (float) (1.0 - (121.0 / 16.0 * ((double) t - 0.81818181276321411) * ((double) t - 0.81818181276321411) + 15.0 / 16.0)) : (float) (1.0 - (121.0 / 16.0 * ((double) t - 0.95454543828964233) * ((double) t - 0.95454543828964233) + 63.0 / 64.0));
        });
        public static readonly Ease.Easer BounceOut = (Ease.Easer) (t =>
        {
            if ((double) t < 0.36363637447357178)
                return 121f / 16f * t * t;
            if ((double) t < 0.72727274894714355)
                return (float) (121.0 / 16.0 * ((double) t - 0.54545456171035767) * ((double) t - 0.54545456171035767) + 0.75);
            return (double) t < 0.90909093618392944 ? (float) (121.0 / 16.0 * ((double) t - 0.81818181276321411) * ((double) t - 0.81818181276321411) + 15.0 / 16.0) : (float) (121.0 / 16.0 * ((double) t - 0.95454543828964233) * ((double) t - 0.95454543828964233) + 63.0 / 64.0);
        });
        public static readonly Ease.Easer BounceInOut = (Ease.Easer) (t =>
        {
            if ((double) t < 0.5)
            {
                t = (float) (1.0 - (double) t * 2.0);
                if ((double) t < 0.36363637447357178)
                    return (float) ((1.0 - 121.0 / 16.0 * (double) t * (double) t) / 2.0);
                if ((double) t < 0.72727274894714355)
                    return (float) ((1.0 - (121.0 / 16.0 * ((double) t - 0.54545456171035767) * ((double) t - 0.54545456171035767) + 0.75)) / 2.0);
                return (double) t < 0.90909093618392944 ? (float) ((1.0 - (121.0 / 16.0 * ((double) t - 0.81818181276321411) * ((double) t - 0.81818181276321411) + 15.0 / 16.0)) / 2.0) : (float) ((1.0 - (121.0 / 16.0 * ((double) t - 0.95454543828964233) * ((double) t - 0.95454543828964233) + 63.0 / 64.0)) / 2.0);
            }
            t = (float) ((double) t * 2.0 - 1.0);
            if ((double) t < 0.36363637447357178)
                return (float) (121.0 / 16.0 * (double) t * (double) t / 2.0 + 0.5);
            if ((double) t < 0.72727274894714355)
                return (float) ((121.0 / 16.0 * ((double) t - 0.54545456171035767) * ((double) t - 0.54545456171035767) + 0.75) / 2.0 + 0.5);
            return (double) t < 0.90909093618392944 ? (float) ((121.0 / 16.0 * ((double) t - 0.81818181276321411) * ((double) t - 0.81818181276321411) + 15.0 / 16.0) / 2.0 + 0.5) : (float) ((121.0 / 16.0 * ((double) t - 0.95454543828964233) * ((double) t - 0.95454543828964233) + 63.0 / 64.0) / 2.0 + 0.5);
        });

        public static Ease.Easer Invert(Ease.Easer easer) => (Ease.Easer) (t => 1f - easer(1f - t));

        public static Ease.Easer Follow(Ease.Easer first, Ease.Easer second) => (Ease.Easer) (t => (double) t > 0.5 ? (float) ((double) second((float) ((double) t * 2.0 - 1.0)) / 2.0 + 0.5) : first(t * 2f) / 2f);

        public static float UpDown(float eased) => (double) eased <= 0.5 ? eased * 2f : (float) (1.0 - ((double) eased - 0.5) * 2.0);

        public delegate float Easer(float t);
    }
}
