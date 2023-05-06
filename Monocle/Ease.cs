// Decompiled with JetBrains decompiler
// Type: Monocle.Ease
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace Monocle
{
    public static class Ease
    {
        public static readonly Easer Linear = t => t;
        public static readonly Easer SineIn = t => (float) -Math.Cos(Math.PI / 2 * t) + 1;
        public static readonly Easer SineOut = t => (float) Math.Sin(Math.PI / 2 * t);
        public static readonly Easer SineInOut = t => (float) -Math.Cos(Math.PI * t) / 2 + 0.5f;
        public static readonly Easer QuadIn = t => t * t;
        public static readonly Easer QuadOut = Invert(QuadIn);
        public static readonly Easer QuadInOut = Follow(QuadIn, QuadOut);
        public static readonly Easer CubeIn = t => t * t * t;
        public static readonly Easer CubeOut = Invert(CubeIn);
        public static readonly Easer CubeInOut = Follow(CubeIn, CubeOut);
        public static readonly Easer QuintIn = t => t * t * t * t * t;
        public static readonly Easer QuintOut = Invert(QuintIn);
        public static readonly Easer QuintInOut = Follow(QuintIn, QuintOut);
        public static readonly Easer ExpoIn = t => (float) Math.Pow(2, 10 * (t - 1));
        public static readonly Easer ExpoOut = Invert(ExpoIn);
        public static readonly Easer ExpoInOut = Follow(ExpoIn, ExpoOut);
        public static readonly Easer BackIn = t => t * t * ((2.7f * t) - 1.7f);
        public static readonly Easer BackOut = Invert(BackIn);
        public static readonly Easer BackInOut = Follow(BackIn, BackOut);
        public static readonly Easer BigBackIn = t => t * t * ((4 * t) - 3);
        public static readonly Easer BigBackOut = Invert(BigBackIn);
        public static readonly Easer BigBackInOut = Follow(BigBackIn, BigBackOut);
        public static readonly Easer ElasticIn = t =>
        {
            float sqr = t * t;
            float cubic = sqr * t;
            return (33 * cubic * sqr) + (-59 * sqr * sqr) + (32 * cubic) + (-5 * sqr);
        };
        public static readonly Easer ElasticOut = t =>
        {
            float sqr = t * t;
            float cubic = sqr * t;
            return (33 * cubic * sqr) + (-106 * sqr * sqr) + (126 * cubic) + (-67 * sqr) + (15 * t);
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
            return t < B1
                ? (1 - (121 / 16 * t * t))
                : t < B2
                ? (1 - ((121 / 16 * (t - B3) * (t - B3)) + 0.75f))
                : t < B4 ? (1 - ((121 / 16 * (t - B5) * (t - B5)) + (15 / 16))) : (1 - ((121 / 16 * (t - B6) * (t - B6)) + (63 / 64)));
        };
        public static readonly Easer BounceOut = t =>
        {
            return t < B1
                ? 121f / 16f * t * t
                : t < B2
                ? ((121 / 16 * (t - B3) * (t - B3)) + 0.75f)
                : t < B4 ? ((121 / 16 * (t - B5) * (t - B5)) + (15 / 16)) : ((121 / 16 * (t - B6) * (t - B6)) + (63 / 64));
        };
        public static readonly Easer BounceInOut = t =>
        {
            if (t < 0.5)
            {
                t = (1 - (t * 2));
                return t < B1
                    ? ((1 - (121 / 16 * t * t)) / 2)
                    : t < B2
                    ? ((1 - ((121 / 16 * (t - B3) * (t - B3)) + 0.75f)) / 2)
                    : t < B4 ? ((1 - ((121 / 16 * (t - B5) * (t - B5)) + (15 / 16))) / 2) : ((1 - ((121 / 16 * (t - B6) * (t - B6)) + (63 / 64))) / 2);
            }
            t = ((t * 2) - 1);
            return t < B1
                ? ((121 / 16 * t * t / 2) + 0.5f)
                : t < B2
                ? ((((121 / 16 * (t - B3) * (t - B3)) + 0.75f) / 2) + 0.5f)
                : t < B4 ? ((((121 / 16 * (t - B5) * (t - B5)) + (15 / 16)) / 2) + 0.5f) : ((((121 / 16 * (t - B6) * (t - B6)) + (63 / 64)) / 2) + 0.5f);
        };

        public static Easer Invert(Easer easer)
        {
            return t => 1f - easer(1f - t);
        }

        public static Easer Follow(Easer first, Easer second)
        {
            return t => t > 0.5f ? (second((t * 2) - 1) / 2) + 0.5f : first(t * 2f) / 2f;
        }

        public static float UpDown(float eased)
        {
            return eased <= 0.5f ? eased * 2f : 1 - ((eased - 0.5f) * 2);
        }

        public delegate float Easer(float t);
    }
}
