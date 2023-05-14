// Decompiled with JetBrains decompiler
// Type: Celeste.WaterFall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class WaterFall : Entity
    {
        private float height;
        private Water water;
        private Solid solid;
        private SoundSource loopingSfx;
        private SoundSource enteringSfx;

        public WaterFall(Vector2 position)
            : base(position)
        {
            Depth = -9999;
            Tag = (int)Tags.TransitionUpdate;
        }

        public WaterFall(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Level scene1 = Scene as Level;
            bool flag = false;
            for (height = 8f; (double)Y + height < scene1.Bounds.Bottom && (water = Scene.CollideFirst<Water>(new Rectangle((int)X, (int)((double)Y + height), 8, 8))) == null && ((solid = Scene.CollideFirst<Solid>(new Rectangle((int)X, (int)((double)Y + height), 8, 8))) == null || !solid.BlockWaterfalls); solid = null)
            {
                height += 8f;
            }

            if (water != null && !Scene.CollideCheck<Solid>(new Rectangle((int)X, (int)((double)Y + height), 8, 16)))
            {
                flag = true;
            }

            Add(loopingSfx = new SoundSource());
            _ = loopingSfx.Play("event:/env/local/waterfall_small_main");
            Add(enteringSfx = new SoundSource());
            _ = enteringSfx.Play(flag ? "event:/env/local/waterfall_small_in_deep" : "event:/env/local/waterfall_small_in_shallow");
            enteringSfx.Position.Y = height;
            Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
        }

        public override void Update()
        {
            loopingSfx.Position.Y = Calc.Clamp((Scene as Level).Camera.Position.Y + 90f, Y, height);
            if (water != null && Scene.OnInterval(0.3f))
            {
                water.TopSurface.DoRipple(new Vector2(X + 4f, water.Y), 0.75f);
            }

            if (water != null || solid != null)
            {
                Vector2 position = new(X + 4f, (float)((double)Y + height + 2.0));
                (Scene as Level).ParticlesFG.Emit(Water.P_Splash, 1, position, new Vector2(8f, 2f), new Vector2(0.0f, -1f).Angle());
            }
            base.Update();
        }

        public void RenderDisplacement()
        {
            Draw.Rect(X, Y, 8f, height, new Color(0.5f, 0.5f, 0.8f, 1f));
        }

        public override void Render()
        {
            if (water == null || water.TopSurface == null)
            {
                Draw.Rect(X + 1f, Y, 6f, height, Water.FillColor);
                Draw.Rect(X - 1f, Y, 2f, height, Water.SurfaceColor);
                Draw.Rect(X + 7f, Y, 2f, height, Water.SurfaceColor);
            }
            else
            {
                Water.Surface topSurface = water.TopSurface;
                float num = height + water.TopSurface.Position.Y - water.Y;
                for (int index = 0; index < 6; ++index)
                {
                    Draw.Rect((float)((double)X + index + 1.0), Y, 1f, num - topSurface.GetSurfaceHeight(new Vector2(X + 1f + index, water.Y)), Water.FillColor);
                }

                Draw.Rect(X - 1f, Y, 2f, num - topSurface.GetSurfaceHeight(new Vector2(X, water.Y)), Water.SurfaceColor);
                Draw.Rect(X + 7f, Y, 2f, num - topSurface.GetSurfaceHeight(new Vector2(X + 8f, water.Y)), Water.SurfaceColor);
            }
        }
    }
}
