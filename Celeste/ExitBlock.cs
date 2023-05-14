// Decompiled with JetBrains decompiler
// Type: Celeste.ExitBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class ExitBlock : Solid
    {
        private TileGrid tiles;
        private readonly TransitionListener tl;
        private readonly EffectCutout cutout;
        private float startAlpha;
        private readonly char tileType;

        public ExitBlock(Vector2 position, float width, float height, char tileType)
            : base(position, width, height, true)
        {
            Depth = -13000;
            this.tileType = tileType;
            tl = new TransitionListener
            {
                OnOutBegin = new Action(OnTransitionOutBegin),
                OnInBegin = new Action(OnTransitionInBegin)
            };
            Add(tl);
            Add(cutout = new EffectCutout());
            SurfaceSoundIndex = SurfaceIndex.TileToIndex[tileType];
            EnableAssistModeChecks = false;
        }

        public ExitBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Char(nameof(tileType), '3'))
        {
        }

        private void OnTransitionOutBegin()
        {
            if (!Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                return;
            }

            tl.OnOut = new Action<float>(OnTransitionOut);
            startAlpha = tiles.Alpha;
        }

        private void OnTransitionOut(float percent)
        {
            cutout.Alpha = tiles.Alpha = MathHelper.Lerp(startAlpha, 0.0f, percent);
            cutout.Update();
        }

        private void OnTransitionInBegin()
        {
            if (!Collide.CheckRect(this, SceneAs<Level>().PreviousBounds.Value) || CollideCheck<Player>())
            {
                return;
            }

            cutout.Alpha = 0.0f;
            tiles.Alpha = 0.0f;
            tl.OnIn = new Action<float>(OnTransitionIn);
        }

        private void OnTransitionIn(float percent)
        {
            cutout.Alpha = tiles.Alpha = percent;
            cutout.Update();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = SceneAs<Level>();
            Rectangle tileBounds = level.Session.MapData.TileBounds;
            VirtualMap<char> solidsData = level.SolidsData;
            int x = (int)((double)X / 8.0) - tileBounds.Left;
            int y = (int)((double)Y / 8.0) - tileBounds.Top;
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            tiles = GFX.FGAutotiler.GenerateOverlay(tileType, x, y, tilesX, tilesY, solidsData).TileGrid;
            Add(tiles);
            Add(new TileInterceptor(tiles, false));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CollideCheck<Player>())
            {
                return;
            }

            cutout.Alpha = tiles.Alpha = 0.0f;
            Collidable = false;
        }

        public override void Update()
        {
            base.Update();
            if (Collidable)
            {
                cutout.Alpha = tiles.Alpha = Calc.Approach(tiles.Alpha, 1f, Engine.DeltaTime);
            }
            else
            {
                if (CollideCheck<Player>())
                {
                    return;
                }

                Collidable = true;
                _ = Audio.Play("event:/game/general/passage_closed_behind", Center);
            }
        }

        public override void Render()
        {
            if (tiles.Alpha >= 1.0)
            {
                Level scene = Scene as Level;
                if (scene.ShakeVector.X < 0.0 && (double)scene.Camera.X <= scene.Bounds.Left && (double)X <= scene.Bounds.Left)
                {
                    tiles.RenderAt(Position + new Vector2(-3f, 0.0f));
                }

                Rectangle bounds;
                if (scene.ShakeVector.X > 0.0)
                {
                    double num1 = (double)scene.Camera.X + 320.0;
                    bounds = scene.Bounds;
                    double right1 = bounds.Right;
                    if (num1 >= right1)
                    {
                        double num2 = (double)X + (double)Width;
                        bounds = scene.Bounds;
                        double right2 = bounds.Right;
                        if (num2 >= right2)
                        {
                            tiles.RenderAt(Position + new Vector2(3f, 0.0f));
                        }
                    }
                }
                if (scene.ShakeVector.Y < 0.0)
                {
                    double y1 = (double)scene.Camera.Y;
                    bounds = scene.Bounds;
                    double top1 = bounds.Top;
                    if (y1 <= top1)
                    {
                        double y2 = (double)Y;
                        bounds = scene.Bounds;
                        double top2 = bounds.Top;
                        if (y2 <= top2)
                        {
                            tiles.RenderAt(Position + new Vector2(0.0f, -3f));
                        }
                    }
                }
                if (scene.ShakeVector.Y > 0.0)
                {
                    double num3 = (double)scene.Camera.Y + 180.0;
                    bounds = scene.Bounds;
                    double bottom1 = bounds.Bottom;
                    if (num3 >= bottom1)
                    {
                        double num4 = (double)Y + (double)Height;
                        bounds = scene.Bounds;
                        double bottom2 = bounds.Bottom;
                        if (num4 >= bottom2)
                        {
                            tiles.RenderAt(Position + new Vector2(0.0f, 3f));
                        }
                    }
                }
            }
            base.Render();
        }
    }
}
