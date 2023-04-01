// Decompiled with JetBrains decompiler
// Type: Celeste.Backdrop
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public abstract class Backdrop
    {
        public bool UseSpritebatch = true;
        public string Name;
        public HashSet<string> Tags = new();
        public Vector2 Position;
        public Vector2 Scroll = Vector2.One;
        public Vector2 Speed;
        public Color Color = Color.White;
        public bool LoopX = true;
        public bool LoopY = true;
        public bool FlipX;
        public bool FlipY;
        public Fader FadeX;
        public Fader FadeY;
        public float FadeAlphaMultiplier = 1f;
        public float WindMultiplier;
        public HashSet<string> ExcludeFrom;
        public HashSet<string> OnlyIn;
        public string OnlyIfFlag;
        public string OnlyIfNotFlag;
        public string AlsoIfFlag;
        public bool? Dreaming;
        public bool Visible;
        public bool InstantIn = true;
        public bool InstantOut;
        public bool ForceVisible;
        public BackdropRenderer Renderer;

        public Backdrop()
        {
            Visible = true;
        }

        public bool IsVisible(Level level)
        {
            return ForceVisible || ((string.IsNullOrEmpty(OnlyIfNotFlag) || !level.Session.GetFlag(OnlyIfNotFlag)) && ((!string.IsNullOrEmpty(AlsoIfFlag) && level.Session.GetFlag(AlsoIfFlag)) || ((!Dreaming.HasValue || Dreaming.Value == level.Session.Dreaming) && (string.IsNullOrEmpty(OnlyIfFlag) || level.Session.GetFlag(OnlyIfFlag)) && (ExcludeFrom == null || !ExcludeFrom.Contains(level.Session.Level)) && (OnlyIn == null || OnlyIn.Contains(level.Session.Level)))));
        }

        public virtual void Update(Scene scene)
        {
            Level level = scene as Level;
            if (level.Transitioning)
            {
                if (InstantIn && IsVisible(level))
                {
                    Visible = true;
                }

                if (!InstantOut || IsVisible(level))
                {
                    return;
                }

                Visible = false;
            }
            else
            {
                Visible = IsVisible(level);
            }
        }

        public virtual void BeforeRender(Scene scene)
        {
        }

        public virtual void Render(Scene scene)
        {
        }

        public virtual void Ended(Scene scene)
        {
        }

        public class Fader
        {
            private readonly List<Segment> Segments = new();

            public Fader Add(float posFrom, float posTo, float fadeFrom, float fadeTo)
            {
                Segments.Add(new Segment()
                {
                    PositionFrom = posFrom,
                    PositionTo = posTo,
                    From = fadeFrom,
                    To = fadeTo
                });
                return this;
            }

            public float Value(float position)
            {
                float num = 1f;
                foreach (Segment segment in Segments)
                {
                    num *= Calc.ClampedMap(position, segment.PositionFrom, segment.PositionTo, segment.From, segment.To);
                }

                return num;
            }

            private struct Segment
            {
                public float PositionFrom;
                public float PositionTo;
                public float From;
                public float To;
            }
        }
    }
}
