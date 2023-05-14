// Decompiled with JetBrains decompiler
// Type: Celeste.DreamMirror
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class DreamMirror : Entity
    {
        public static ParticleType P_Shatter;
        private Monocle.Image frame;
        private readonly MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
        private readonly MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
        private readonly Sprite breakingGlass;
        private Hitbox hitbox;
        private VirtualRenderTarget mirror;
        private float shineAlpha = 0.5f;
        private float shineOffset;
        private Entity reflection;
        private PlayerSprite reflectionSprite;
        private PlayerHair reflectionHair;
        private readonly float reflectionAlpha = 0.7f;
        private bool autoUpdateReflection = true;
        private BadelineDummy badeline;
        private bool smashed;
        private bool smashEnded;
        private bool updateShine = true;
        private Coroutine smashCoroutine;
        private SoundSource sfx;
        private SoundSource sfxSting;

        public DreamMirror(Vector2 position)
            : base(position)
        {
            Depth = 9500;
            Add(breakingGlass = GFX.SpriteBank.Create("glass"));
            breakingGlass.Play("idle");
            Add(new BeforeRenderHook(new Action(BeforeRender)));
            foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask"))
            {
                MTexture shard = atlasSubtexture;
                MirrorSurface surface = new();
                surface.OnRender = () => shard.DrawJustified(Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (smashEnded ? 1f : 0.0f));
                surface.ReflectionOffset = new Vector2(9 + Calc.Random.Range(-4, 4), 4 + Calc.Random.Range(-2, 2));
                Add(surface);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            smashed = SceneAs<Level>().Session.Inventory.DreamDash;
            if (smashed)
            {
                breakingGlass.Play("broken");
                smashEnded = true;
            }
            else
            {
                reflection = new Entity();
                reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
                reflectionHair = new PlayerHair(reflectionSprite)
                {
                    Color = BadelineOldsite.HairColor,
                    Border = Color.Black
                };
                reflection.Add(reflectionHair);
                reflection.Add(reflectionSprite);
                reflectionHair.Start();
                reflectionSprite.OnFrameChange = anim =>
                {
                    if (smashed || !CollideCheck<Player>())
                    {
                        return;
                    }

                    int currentAnimationFrame = reflectionSprite.CurrentAnimationFrame;
                    if ((!(anim == "walk") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!(anim == "runSlow") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!(anim == "runFast") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)))
                    {
                        return;
                    }

                    _ = Audio.Play("event:/char/badeline/footstep", Center);
                };
                Add(smashCoroutine = new Coroutine(InteractRoutine()));
            }
            Entity entity = new(Position)
            {
                Depth = 9000
            };
            entity.Add(frame = new Monocle.Image(GFX.Game["objects/mirror/frame"]));
            _ = frame.JustifyOrigin(0.5f, 1f);
            Scene.Add(entity);
            Collider = hitbox = new Hitbox((int)frame.Width - 16, (int)frame.Height + 32, (-(int)frame.Width / 2) + 8, -(int)frame.Height - 32);
        }

        public override void Update()
        {
            base.Update();
            if (reflection == null)
            {
                return;
            }

            reflection.Update();
            reflectionHair.Facing = (Facings)Math.Sign(reflectionSprite.Scale.X);
            reflectionHair.AfterUpdate();
        }

        private void BeforeRender()
        {
            if (smashed)
            {
                return;
            }

            Level scene = Scene as Level;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            if (autoUpdateReflection && reflection != null)
            {
                reflection.Position = new Vector2(X - entity.X, entity.Y - Y) + breakingGlass.Origin;
                reflectionSprite.Scale.X = -(int)entity.Facing * Math.Abs(entity.Sprite.Scale.X);
                reflectionSprite.Scale.Y = entity.Sprite.Scale.Y;
                if (reflectionSprite.CurrentAnimationID != entity.Sprite.CurrentAnimationID && entity.Sprite.CurrentAnimationID != null && reflectionSprite.Has(entity.Sprite.CurrentAnimationID))
                {
                    reflectionSprite.Play(entity.Sprite.CurrentAnimationID);
                }
            }
            mirror ??= VirtualContent.CreateRenderTarget("dream-mirror", glassbg.Width, glassbg.Height);
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)mirror);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (updateShine)
            {
                shineOffset = glassfg.Height - (int)((double)scene.Camera.Y * 0.800000011920929 % glassfg.Height);
            }

            glassbg.Draw(Vector2.Zero);
            reflection?.Render();
            glassfg.Draw(new Vector2(0.0f, shineOffset), Vector2.Zero, Color.White * shineAlpha);
            glassfg.Draw(new Vector2(0.0f, shineOffset - glassfg.Height), Vector2.Zero, Color.White * shineAlpha);
            Draw.SpriteBatch.End();
        }

        private IEnumerator InteractRoutine()
        {
            DreamMirror mirror = this;
            Player player = null;
            while (player == null)
            {
                player = mirror.Scene.Tracker.GetEntity<Player>();
                yield return null;
            }
            while (!mirror.hitbox.Collide(player))
            {
                yield return null;
            }

            mirror.hitbox.Width += 32f;
            mirror.hitbox.Position.X -= 16f;
            _ = Audio.SetMusic(null);
            while (mirror.hitbox.Collide(player))
            {
                yield return null;
            }

            mirror.Scene.Add(new CS02_Mirror(player, mirror));
        }

        public IEnumerator BreakRoutine(int direction)
        {
            DreamMirror dreamMirror = this;
            dreamMirror.autoUpdateReflection = false;
            dreamMirror.reflectionSprite.Play("runFast");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            while ((double)Math.Abs(dreamMirror.reflection.X - (dreamMirror.breakingGlass.Width / 2f)) > 3.0)
            {
                dreamMirror.reflection.X += direction * 32 * Engine.DeltaTime;
                yield return null;
            }
            dreamMirror.reflectionSprite.Play("idle");
            yield return 0.65f;
            dreamMirror.Add(dreamMirror.sfx = new SoundSource());
            _ = dreamMirror.sfx.Play("event:/game/02_old_site/sequence_mirror");
            yield return 0.15f;
            dreamMirror.Add(dreamMirror.sfxSting = new SoundSource("event:/music/lvl2/dreamblock_sting_pt2"));
            Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
            dreamMirror.updateShine = false;
            while (dreamMirror.shineOffset != 33.0 || dreamMirror.shineAlpha < 1.0)
            {
                dreamMirror.shineOffset = Calc.Approach(dreamMirror.shineOffset, 33f, Engine.DeltaTime * 120f);
                dreamMirror.shineAlpha = Calc.Approach(dreamMirror.shineAlpha, 1f, Engine.DeltaTime * 4f);
                yield return null;
            }
            dreamMirror.smashed = true;
            dreamMirror.breakingGlass.Play("break");
            yield return 0.6f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            (dreamMirror.Scene as Level).Shake();
            for (float x = (float)(-(double)dreamMirror.breakingGlass.Width / 2.0); (double)x < (double)dreamMirror.breakingGlass.Width / 2.0; x += 8f)
            {
                for (float y = -dreamMirror.breakingGlass.Height; (double)y < 0.0; y += 8f)
                {
                    if (Calc.Random.Chance(0.5f))
                    {
                        (dreamMirror.Scene as Level).Particles.Emit(DreamMirror.P_Shatter, 2, dreamMirror.Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
                    }
                }
            }
            dreamMirror.smashEnded = true;
            dreamMirror.badeline = new BadelineDummy(dreamMirror.reflection.Position + dreamMirror.Position - dreamMirror.breakingGlass.Origin)
            {
                Floatness = 0.0f
            };
            for (int index = 0; index < dreamMirror.badeline.Hair.Nodes.Count; ++index)
            {
                dreamMirror.badeline.Hair.Nodes[index] = dreamMirror.reflectionHair.Nodes[index];
            }

            dreamMirror.Scene.Add(dreamMirror.badeline);
            dreamMirror.badeline.Sprite.Play("idle");
            dreamMirror.badeline.Sprite.Scale = dreamMirror.reflectionSprite.Scale;
            dreamMirror.reflection = null;
            yield return 1.2f;
            float speed = -direction * 32f;
            dreamMirror.badeline.Sprite.Scale.X = -direction;
            dreamMirror.badeline.Sprite.Play("runFast");
            while ((double)Math.Abs(dreamMirror.badeline.X - dreamMirror.X) < 60.0)
            {
                speed += (float)((double)Engine.DeltaTime * -direction * 128.0);
                dreamMirror.badeline.X += speed * Engine.DeltaTime;
                yield return null;
            }
            dreamMirror.badeline.Sprite.Play("jumpFast");
            while ((double)Math.Abs(dreamMirror.badeline.X - dreamMirror.X) < 128.0)
            {
                speed += (float)((double)Engine.DeltaTime * -direction * 128.0);
                dreamMirror.badeline.X += speed * Engine.DeltaTime;
                dreamMirror.badeline.Y -= (float)((double)Math.Abs(speed) * (double)Engine.DeltaTime * 0.800000011920929);
                yield return null;
            }
            dreamMirror.badeline.RemoveSelf();
            dreamMirror.badeline = null;
            yield return 1.5f;
        }

        public void Broken(bool wasSkipped)
        {
            updateShine = false;
            smashed = true;
            smashEnded = true;
            breakingGlass.Play("broken");
            if (wasSkipped && badeline != null)
            {
                badeline.RemoveSelf();
            }

            if (wasSkipped && sfx != null)
            {
                _ = sfx.Stop();
            }

            if (!wasSkipped || sfxSting == null)
            {
                return;
            }

            _ = sfxSting.Stop();
        }

        public override void Render()
        {
            if (smashed)
            {
                breakingGlass.Render();
            }
            else
            {
                Draw.SpriteBatch.Draw(mirror.Target, Position - breakingGlass.Origin, Color.White * reflectionAlpha);
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
