// Decompiled with JetBrains decompiler
// Type: Celeste.ResortMirror
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class ResortMirror : Entity
    {
        private bool smashed;
        private Monocle.Image bg;
        private Monocle.Image frame;
        private readonly MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
        private Sprite breakingGlass;
        private VirtualRenderTarget mirror;
        private float shineAlpha = 0.7f;
        private float mirrorAlpha = 0.7f;
        private BadelineDummy evil;
        private bool shardReflection;

        public ResortMirror(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(new BeforeRenderHook(new Action(BeforeRender)));
            Depth = 9500;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            smashed = SceneAs<Level>().Session.GetFlag("oshiro_resort_suite");
            Entity entity = new(Position)
            {
                Depth = 9000
            };
            entity.Add(frame = new Monocle.Image(GFX.Game["objects/mirror/resortframe"]));
            _ = frame.JustifyOrigin(0.5f, 1f);
            Scene.Add(entity);
            MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
            int w = (int)frame.Width - 2;
            int h = (int)frame.Height - 12;
            if (!smashed)
            {
                mirror = VirtualContent.CreateRenderTarget("resort-mirror", w, h);
            }
            else
            {
                glassbg = GFX.Game["objects/mirror/glassbreak09"];
            }

            Add(bg = new Monocle.Image(glassbg.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h)));
            _ = bg.JustifyOrigin(0.5f, 1f);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask");
            MTexture temp = new();
            foreach (MTexture mtexture in atlasSubtextures)
            {
                MTexture shard = mtexture;
                MirrorSurface surface = new();
                surface.OnRender = () => shard.GetSubtexture((glassbg.Width - w) / 2, glassbg.Height - h, w, h, temp).DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (shardReflection ? 1f : 0.0f));
                surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
                Add(surface);
            }
        }

        public void EvilAppear()
        {
            Add(new Coroutine(EvilAppearRoutine()));
            Add(new Coroutine(FadeLights()));
        }

        private IEnumerator EvilAppearRoutine()
        {
            evil = new BadelineDummy(new Vector2(mirror.Width + 8, mirror.Height));
            yield return evil.WalkTo(mirror.Width / 2);
        }

        private IEnumerator FadeLights()
        {
            Level level = SceneAs<Level>();
            while (level.Lighting.Alpha != 0.34999999403953552)
            {
                level.Lighting.Alpha = Calc.Approach(level.Lighting.Alpha, 0.35f, Engine.DeltaTime * 0.1f);
                yield return null;
            }
        }

        public IEnumerator SmashRoutine()
        {
            ResortMirror resortMirror = this;
            yield return resortMirror.evil.FloatTo(new Vector2(resortMirror.mirror.Width / 2, resortMirror.mirror.Height - 8));
            resortMirror.breakingGlass = GFX.SpriteBank.Create("glass");
            resortMirror.breakingGlass.Position = new Vector2(resortMirror.mirror.Width / 2, resortMirror.mirror.Height);
            resortMirror.breakingGlass.Play("break");
            resortMirror.breakingGlass.Color = Color.White * resortMirror.shineAlpha;
            Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
            while (resortMirror.breakingGlass.CurrentAnimationID == "break")
            {
                if (resortMirror.breakingGlass.CurrentAnimationFrame == 7)
                {
                    resortMirror.SceneAs<Level>().Shake();
                }

                resortMirror.shineAlpha = Calc.Approach(resortMirror.shineAlpha, 1f, Engine.DeltaTime * 2f);
                resortMirror.mirrorAlpha = Calc.Approach(resortMirror.mirrorAlpha, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
            resortMirror.SceneAs<Level>().Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            for (float x = (float)(-(double)resortMirror.breakingGlass.Width / 2.0); (double)x < (double)resortMirror.breakingGlass.Width / 2.0; x += 8f)
            {
                for (float y = -resortMirror.breakingGlass.Height; (double)y < 0.0; y += 8f)
                {
                    if (Calc.Random.Chance(0.5f))
                    {
                        (resortMirror.Scene as Level).Particles.Emit(DreamMirror.P_Shatter, 2, resortMirror.Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
                    }
                }
            }
            resortMirror.shardReflection = true;
            resortMirror.evil = null;
        }

        public void Broken()
        {
            evil = null;
            smashed = true;
            shardReflection = true;
            MTexture mtexture = GFX.Game["objects/mirror/glassbreak09"];
            bg.Texture = mtexture.GetSubtexture((int)(mtexture.Width - (double)bg.Width) / 2, mtexture.Height - (int)bg.Height, (int)bg.Width, (int)bg.Height);
        }

        private void BeforeRender()
        {
            if (smashed || mirror == null)
            {
                return;
            }

            Level level = SceneAs<Level>();
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)mirror);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            NPC first = Scene.Entities.FindFirst<NPC>();
            if (first != null)
            {
                Vector2 renderPosition = first.Sprite.RenderPosition;
                first.Sprite.RenderPosition = renderPosition - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, -4f);
                first.Sprite.Render();
                first.Sprite.RenderPosition = renderPosition;
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                Vector2 position = entity.Position;
                entity.Position = position - Position + new Vector2(mirror.Width / 2, mirror.Height) + new Vector2(8f, 0.0f);
                Vector2 vector2 = entity.Position - position;
                for (int index = 0; index < entity.Hair.Nodes.Count; ++index)
                {
                    entity.Hair.Nodes[index] += vector2;
                }

                entity.Render();
                for (int index = 0; index < entity.Hair.Nodes.Count; ++index)
                {
                    entity.Hair.Nodes[index] -= vector2;
                }

                entity.Position = position;
            }
            if (evil != null)
            {
                evil.Update();
                evil.Hair.Facing = (Facings)Math.Sign(evil.Sprite.Scale.X);
                evil.Hair.AfterUpdate();
                evil.Render();
            }
            if (breakingGlass != null)
            {
                breakingGlass.Color = Color.White * shineAlpha;
                breakingGlass.Update();
                breakingGlass.Render();
            }
            else
            {
                int y = -(int)((double)level.Camera.Y * 0.800000011920929 % glassfg.Height);
                glassfg.DrawJustified(new Vector2(mirror.Width / 2, y), new Vector2(0.5f, 1f), Color.White * shineAlpha);
                glassfg.DrawJustified(new Vector2(mirror.Height / 2, y - glassfg.Height), new Vector2(0.5f, 1f), Color.White * shineAlpha);
            }
            Draw.SpriteBatch.End();
        }

        public override void Render()
        {
            bg.Render();
            if (!smashed)
            {
                Draw.SpriteBatch.Draw((RenderTarget2D)mirror, Position + new Vector2(-mirror.Width / 2, -mirror.Height), Color.White * mirrorAlpha);
            }

            frame.Render();
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        private void Dispose()
        {
            mirror?.Dispose();
            mirror = null;
        }
    }
}
