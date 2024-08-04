using System;

namespace Monocle
{
    public static class Ease
    {
        public static readonly Easer Linear = t => t;
        public static readonly Easer SineIn = t => -(float) Math.Cos(Math.PI / 2 * t) + 1f;
        public static readonly Easer SineOut = t => (float) Math.Sin(Math.PI / 2 * t);
        public static readonly Easer SineInOut = t => -(float) Math.Cos(Math.PI * t) / 2f + 0.5f;
        public static readonly Easer QuadIn = t => t * t;
        public static readonly Easer QuadOut = Invert(QuadIn);
        public static readonly Easer QuadInOut = Follow(QuadIn, QuadOut);
        public static readonly Easer CubeIn = t => t * t * t;
        public static readonly Easer CubeOut = Invert(CubeIn);
        public static readonly Easer CubeInOut = Follow(CubeIn, CubeOut);
        public static readonly Easer QuintIn = t => t * t * t * t * t;
        public static readonly Easer QuintOut = Invert(QuintIn);
        public static readonly Easer QuintInOut = Follow(QuintIn, QuintOut);
        public static readonly Easer ExpoIn = t => (float) Math.Pow(2f, 10f * (t - 1f));
        public static readonly Easer ExpoOut = Invert(ExpoIn);
        public static readonly Easer ExpoInOut = Follow(ExpoIn, ExpoOut);
        public static readonly Easer BackIn = t => t * t * (2.70158f * t - 1.70158f);
        public static readonly Easer BackOut = Invert(BackIn);
        public static readonly Easer BackInOut = Follow(BackIn, BackOut);
        public static readonly Easer BigBackIn = t => t * t * (4f * t - 3f);
        public static readonly Easer BigBackOut = Invert(BigBackIn);
        public static readonly Easer BigBackInOut = Follow(BigBackIn, BigBackOut);
        public static readonly Easer ElasticIn = t =>
        {
            float tSquare = t * t;
            float tCube = tSquare * t;
            return 33f * tCube * tSquare + -59f * tSquare * tSquare + 32f * tCube + -5f * tSquare;
        };
        public static readonly Easer ElasticOut = t =>
        {
            float tSquare = t * t;
            float tCube = tSquare * t;
            return 33f * tCube * tSquare + -106f * tSquare * tSquare + 126f * tCube + -67f * tSquare + 15f * t;
        };
        public static readonly Easer ElasticInOut = Follow(ElasticIn, ElasticOut);

        private const float B1 = 0.363636374f;
        private const float B2 = 0.727272749f;
        private const float B3 = 0.545454562f;
        private const float B4 = 0.909090936f;
        private const float B5 = 0.8181818f;
        private const float B6 = 0.954545438f;

        public static readonly Easer BounceIn = t =>
        {
            t = 1f - t;
            if (t < B1)
                return 1f - 121f / 16f * t * t;
            if (t < B2)
                return 1f - (121f / 16f * (t - B3) * (t - B3) + 0.75f);
            return t < B4 ? (1f - (121f / 16f * (t - B5) * (t - B5) + 15f / 16f)) : (1f - (121f / 16f * (t - B6) * (t - B6) + 63f / 64f));
        };
        public static readonly Easer BounceOut = t =>
        {
            if (t < B1)
                return 121f / 16f * t * t;
            if (t < B2)
                return 121f / 16f * (t - B3) * (t - B3) + 0.75f;
            return t < B4 ? (121f / 16f * (t - B5) * (t - B5) + 15f / 16f) : (121f / 16f * (t - B6) * (t - B6) + 63f / 64f);
        };
        public static readonly Easer BounceInOut = t =>
        {
            if (t < 0.5)
            {
                t = (1f - t * 2f);
                if (t < B1)
                    return (1f - 121f / 16f * t * t) / 2f;
                if (t < B2)
                    return (1f - (121f / 16f * (t - B3) * (t - B3) + 0.75f)) / 2f;
                return t < B4 ? ((1f - (121f / 16f * (t - B5) * (t - B5) + 15f / 16f)) / 2f) : ((1f - (121f / 16f * (t - B6) * (t - B6) + 63f / 64f)) / 2f);
            }
            t = (t * 2f - 1f);
            if (t < B1)
                return 121f / 16f * t * t / 2f + 0.5f;
            if (t < B2)
                return (121f / 16f * (t - B3) * (t - B3) + 0.75f) / 2f + 0.5f;
            return t < B4 ? ((121f / 16f * (t - B5) * (t - B5) + 15f / 16f) / 2f + 0.5f) : ((121f / 16f * (t - B6) * (t - B6) + 63f / 64f) / 2f + 0.5f);
        };

        public static Easer Invert(Easer easer) => t => 1f - easer(1f - t);

        public static Easer Follow(Easer first, Easer second) => t => t > 0.5f ? second(t * 2f - 1f) / 2f + 0.5f : first(t * 2f) / 2f;

        public static float UpDown(float eased) => eased <= 0.5f ? eased * 2f : (float) (1f - (eased - 0.5) * 2f);

        public delegate float Easer(float t);
    }
}
