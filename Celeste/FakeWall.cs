// Decompiled with JetBrains decompiler
// Type: Celeste.FakeWall
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class FakeWall : Entity
    {
        private readonly FakeWall.Modes mode;
        private readonly char fillTile;
        private TileGrid tiles;
        private bool fade;
        private readonly EffectCutout cutout;
        private float transitionStartAlpha;
        private bool transitionFade;
        private EntityID eid;
        private readonly bool playRevealWhenTransitionedInto;

        public FakeWall(
            EntityID eid,
            Vector2 position,
            char tile,
            float width,
            float height,
            FakeWall.Modes mode)
            : base(position)
        {
            this.mode = mode;
            this.eid = eid;
            fillTile = tile;
            Collider = new Hitbox(width, height);
            Depth = -13000;
            Add(cutout = new EffectCutout());
        }

        public FakeWall(EntityID eid, EntityData data, Vector2 offset, FakeWall.Modes mode)
            : this(eid, data.Position + offset, data.Char("tiletype", '3'), data.Width, data.Height, mode)
        {
            playRevealWhenTransitionedInto = data.Bool("playTransitionReveal");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int tilesX = (int)Width / 8;
            int tilesY = (int)Height / 8;
            if (mode == FakeWall.Modes.Wall)
            {
                Level level = SceneAs<Level>();
                Rectangle tileBounds = level.Session.MapData.TileBounds;
                VirtualMap<char> solidsData = level.SolidsData;
                int x = ((int)X / 8) - tileBounds.Left;
                int y = ((int)Y / 8) - tileBounds.Top;
                tiles = GFX.FGAutotiler.GenerateOverlay(fillTile, x, y, tilesX, tilesY, solidsData).TileGrid;
            }
            else if (mode == FakeWall.Modes.Block)
            {
                tiles = GFX.FGAutotiler.GenerateBox(fillTile, tilesX, tilesY).TileGrid;
            }

            Add(tiles);
            Add(new TileInterceptor(tiles, false));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (CollideCheck<Player>())
            {
                tiles.Alpha = 0.0f;
                fade = true;
                cutout.Visible = false;
                if (playRevealWhenTransitionedInto)
                {
                    _ = Audio.Play("event:/game/general/secret_revealed", Center);
                }

                _ = SceneAs<Level>().Session.DoNotLoad.Add(eid);
            }
            else
            {
                Add(new TransitionListener()
                {
                    OnOut = new Action<float>(OnTransitionOut),
                    OnOutBegin = new Action(OnTransitionOutBegin),
                    OnIn = new Action<float>(OnTransitionIn),
                    OnInBegin = new Action(OnTransitionInBegin)
                });
            }
        }

        private void OnTransitionOutBegin()
        {
            if (Collide.CheckRect(this, SceneAs<Level>().Bounds))
            {
                transitionFade = true;
                transitionStartAlpha = tiles.Alpha;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionOut(float percent)
        {
            if (!transitionFade)
            {
                return;
            }

            tiles.Alpha = transitionStartAlpha * (1f - percent);
        }

        private void OnTransitionInBegin()
        {
            Level level = SceneAs<Level>();
            if (level.PreviousBounds.HasValue && Collide.CheckRect(this, level.PreviousBounds.Value))
            {
                transitionFade = true;
                tiles.Alpha = 0.0f;
            }
            else
            {
                transitionFade = false;
            }
        }

        private void OnTransitionIn(float percent)
        {
            if (!transitionFade)
            {
                return;
            }

            tiles.Alpha = percent;
        }

        public override void Update()
        {
            base.Update();
            if (fade)
            {
                tiles.Alpha = Calc.Approach(tiles.Alpha, 0.0f, 2f * Engine.DeltaTime);
                cutout.Alpha = tiles.Alpha;
                if (tiles.Alpha > 0.0)
                {
                    return;
                }

                RemoveSelf();
            }
            else
            {
                Player player = CollideFirst<Player>();
                if (player == null || player.StateMachine.State == 9)
                {
                    return;
                }

                _ = SceneAs<Level>().Session.DoNotLoad.Add(eid);
                fade = true;
                _ = Audio.Play("event:/game/general/secret_revealed", Center);
            }
        }

        public override void Render()
        {
            if (mode == FakeWall.Modes.Wall)
            {
                Level scene = Scene as Level;
                Rectangle bounds;
                if (scene.ShakeVector.X < 0.0)
                {
                    double x1 = (double)scene.Camera.X;
                    bounds = scene.Bounds;
                    double left1 = bounds.Left;
                    if (x1 <= left1)
                    {
                        double x2 = (double)X;
                        bounds = scene.Bounds;
                        double left2 = bounds.Left;
                        if (x2 <= left2)
                        {
                            tiles.RenderAt(Position + new Vector2(-3f, 0.0f));
                        }
                    }
                }
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

        public enum Modes
        {
            Wall,
            Block,
        }
    }
}
