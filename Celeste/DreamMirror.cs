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
        private MTexture glassbg = GFX.Game["objects/mirror/glassbg"];
        private MTexture glassfg = GFX.Game["objects/mirror/glassfg"];
        private Sprite breakingGlass;
        private Hitbox hitbox;
        private VirtualRenderTarget mirror;
        private float shineAlpha = 0.5f;
        private float shineOffset;
        private Entity reflection;
        private PlayerSprite reflectionSprite;
        private PlayerHair reflectionHair;
        private float reflectionAlpha = 0.7f;
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
            this.Depth = 9500;
            this.Add((Component) (this.breakingGlass = GFX.SpriteBank.Create("glass")));
            this.breakingGlass.Play("idle");
            this.Add((Component) new BeforeRenderHook(new Action(this.BeforeRender)));
            foreach (MTexture atlasSubtexture in GFX.Game.GetAtlasSubtextures("objects/mirror/mirrormask"))
            {
                MTexture shard = atlasSubtexture;
                MirrorSurface surface = new MirrorSurface();
                surface.OnRender = (Action) (() => shard.DrawJustified(this.Position, new Vector2(0.5f, 1f), surface.ReflectionColor * (this.smashEnded ? 1f : 0.0f)));
                surface.ReflectionOffset = new Vector2((float) (9 + Calc.Random.Range(-4, 4)), (float) (4 + Calc.Random.Range(-2, 2)));
                this.Add((Component) surface);
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            this.smashed = this.SceneAs<Level>().Session.Inventory.DreamDash;
            if (this.smashed)
            {
                this.breakingGlass.Play("broken");
                this.smashEnded = true;
            }
            else
            {
                this.reflection = new Entity();
                this.reflectionSprite = new PlayerSprite(PlayerSpriteMode.Badeline);
                this.reflectionHair = new PlayerHair(this.reflectionSprite);
                this.reflectionHair.Color = BadelineOldsite.HairColor;
                this.reflectionHair.Border = Color.Black;
                this.reflection.Add((Component) this.reflectionHair);
                this.reflection.Add((Component) this.reflectionSprite);
                this.reflectionHair.Start();
                this.reflectionSprite.OnFrameChange = (Action<string>) (anim =>
                {
                    if (this.smashed || !this.CollideCheck<Player>())
                        return;
                    int currentAnimationFrame = this.reflectionSprite.CurrentAnimationFrame;
                    if ((!(anim == "walk") || currentAnimationFrame != 0 && currentAnimationFrame != 6) && (!(anim == "runSlow") || currentAnimationFrame != 0 && currentAnimationFrame != 6) && (!(anim == "runFast") || currentAnimationFrame != 0 && currentAnimationFrame != 6))
                        return;
                    Audio.Play("event:/char/badeline/footstep", this.Center);
                });
                this.Add((Component) (this.smashCoroutine = new Coroutine(this.InteractRoutine())));
            }
            Entity entity = new Entity(this.Position)
            {
                Depth = 9000
            };
            entity.Add((Component) (this.frame = new Monocle.Image(GFX.Game["objects/mirror/frame"])));
            this.frame.JustifyOrigin(0.5f, 1f);
            this.Scene.Add(entity);
            this.Collider = (Collider) (this.hitbox = new Hitbox((float) ((int) this.frame.Width - 16), (float) ((int) this.frame.Height + 32), (float) (-(int) this.frame.Width / 2 + 8), (float) (-(int) this.frame.Height - 32)));
        }

        public override void Update()
        {
            base.Update();
            if (this.reflection == null)
                return;
            this.reflection.Update();
            this.reflectionHair.Facing = (Facings) Math.Sign(this.reflectionSprite.Scale.X);
            this.reflectionHair.AfterUpdate();
        }

        private void BeforeRender()
        {
            if (this.smashed)
                return;
            Level scene = this.Scene as Level;
            Player entity = this.Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            if (this.autoUpdateReflection && this.reflection != null)
            {
                this.reflection.Position = new Vector2(this.X - entity.X, entity.Y - this.Y) + this.breakingGlass.Origin;
                this.reflectionSprite.Scale.X = (float) -(int) entity.Facing * Math.Abs(entity.Sprite.Scale.X);
                this.reflectionSprite.Scale.Y = entity.Sprite.Scale.Y;
                if (this.reflectionSprite.CurrentAnimationID != entity.Sprite.CurrentAnimationID && entity.Sprite.CurrentAnimationID != null && this.reflectionSprite.Has(entity.Sprite.CurrentAnimationID))
                    this.reflectionSprite.Play(entity.Sprite.CurrentAnimationID);
            }
            if (this.mirror == null)
                this.mirror = VirtualContent.CreateRenderTarget("dream-mirror", this.glassbg.Width, this.glassbg.Height);
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D) this.mirror);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            if (this.updateShine)
                this.shineOffset = (float) (this.glassfg.Height - (int) ((double) scene.Camera.Y * 0.800000011920929 % (double) this.glassfg.Height));
            this.glassbg.Draw(Vector2.Zero);
            if (this.reflection != null)
                this.reflection.Render();
            this.glassfg.Draw(new Vector2(0.0f, this.shineOffset), Vector2.Zero, Color.White * this.shineAlpha);
            this.glassfg.Draw(new Vector2(0.0f, this.shineOffset - (float) this.glassfg.Height), Vector2.Zero, Color.White * this.shineAlpha);
            Draw.SpriteBatch.End();
        }

        private IEnumerator InteractRoutine()
        {
            DreamMirror mirror = this;
            Player player = (Player) null;
            while (player == null)
            {
                player = mirror.Scene.Tracker.GetEntity<Player>();
                yield return (object) null;
            }
            while (!mirror.hitbox.Collide((Entity) player))
                yield return (object) null;
            mirror.hitbox.Width += 32f;
            mirror.hitbox.Position.X -= 16f;
            Audio.SetMusic((string) null);
            while (mirror.hitbox.Collide((Entity) player))
                yield return (object) null;
            mirror.Scene.Add((Entity) new CS02_Mirror(player, mirror));
        }

        public IEnumerator BreakRoutine(int direction)
        {
            DreamMirror dreamMirror = this;
            dreamMirror.autoUpdateReflection = false;
            dreamMirror.reflectionSprite.Play("runFast");
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
            while ((double) Math.Abs(dreamMirror.reflection.X - dreamMirror.breakingGlass.Width / 2f) > 3.0)
            {
                dreamMirror.reflection.X += (float) (direction * 32) * Engine.DeltaTime;
                yield return (object) null;
            }
            dreamMirror.reflectionSprite.Play("idle");
            yield return (object) 0.65f;
            dreamMirror.Add((Component) (dreamMirror.sfx = new SoundSource()));
            dreamMirror.sfx.Play("event:/game/02_old_site/sequence_mirror");
            yield return (object) 0.15f;
            dreamMirror.Add((Component) (dreamMirror.sfxSting = new SoundSource("event:/music/lvl2/dreamblock_sting_pt2")));
            Input.Rumble(RumbleStrength.Light, RumbleLength.FullSecond);
            dreamMirror.updateShine = false;
            while ((double) dreamMirror.shineOffset != 33.0 || (double) dreamMirror.shineAlpha < 1.0)
            {
                dreamMirror.shineOffset = Calc.Approach(dreamMirror.shineOffset, 33f, Engine.DeltaTime * 120f);
                dreamMirror.shineAlpha = Calc.Approach(dreamMirror.shineAlpha, 1f, Engine.DeltaTime * 4f);
                yield return (object) null;
            }
            dreamMirror.smashed = true;
            dreamMirror.breakingGlass.Play("break");
            yield return (object) 0.6f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            (dreamMirror.Scene as Level).Shake();
            for (float x = (float) (-(double) dreamMirror.breakingGlass.Width / 2.0); (double) x < (double) dreamMirror.breakingGlass.Width / 2.0; x += 8f)
            {
                for (float y = -dreamMirror.breakingGlass.Height; (double) y < 0.0; y += 8f)
                {
                    if (Calc.Random.Chance(0.5f))
                        (dreamMirror.Scene as Level).Particles.Emit(DreamMirror.P_Shatter, 2, dreamMirror.Position + new Vector2(x + 4f, y + 4f), new Vector2(8f, 8f), new Vector2(x, y).Angle());
                }
            }
            dreamMirror.smashEnded = true;
            dreamMirror.badeline = new BadelineDummy(dreamMirror.reflection.Position + dreamMirror.Position - dreamMirror.breakingGlass.Origin);
            dreamMirror.badeline.Floatness = 0.0f;
            for (int index = 0; index < dreamMirror.badeline.Hair.Nodes.Count; ++index)
                dreamMirror.badeline.Hair.Nodes[index] = dreamMirror.reflectionHair.Nodes[index];
            dreamMirror.Scene.Add((Entity) dreamMirror.badeline);
            dreamMirror.badeline.Sprite.Play("idle");
            dreamMirror.badeline.Sprite.Scale = dreamMirror.reflectionSprite.Scale;
            dreamMirror.reflection = (Entity) null;
            yield return (object) 1.2f;
            float speed = (float) -direction * 32f;
            dreamMirror.badeline.Sprite.Scale.X = (float) -direction;
            dreamMirror.badeline.Sprite.Play("runFast");
            while ((double) Math.Abs(dreamMirror.badeline.X - dreamMirror.X) < 60.0)
            {
                speed += (float) ((double) Engine.DeltaTime * (double) -direction * 128.0);
                dreamMirror.badeline.X += speed * Engine.DeltaTime;
                yield return (object) null;
            }
            dreamMirror.badeline.Sprite.Play("jumpFast");
            while ((double) Math.Abs(dreamMirror.badeline.X - dreamMirror.X) < 128.0)
            {
                speed += (float) ((double) Engine.DeltaTime * (double) -direction * 128.0);
                dreamMirror.badeline.X += speed * Engine.DeltaTime;
                dreamMirror.badeline.Y -= (float) ((double) Math.Abs(speed) * (double) Engine.DeltaTime * 0.800000011920929);
                yield return (object) null;
            }
            dreamMirror.badeline.RemoveSelf();
            dreamMirror.badeline = (BadelineDummy) null;
            yield return (object) 1.5f;
        }

        public void Broken(bool wasSkipped)
        {
            this.updateShine = false;
            this.smashed = true;
            this.smashEnded = true;
            this.breakingGlass.Play("broken");
            if (wasSkipped && this.badeline != null)
                this.badeline.RemoveSelf();
            if (wasSkipped && this.sfx != null)
                this.sfx.Stop();
            if (!wasSkipped || this.sfxSting == null)
                return;
            this.sfxSting.Stop();
        }

        public override void Render()
        {
            if (this.smashed)
                this.breakingGlass.Render();
            else
                Draw.SpriteBatch.Draw((Texture2D) this.mirror.Target, this.Position - this.breakingGlass.Origin, Color.White * this.reflectionAlpha);
            this.frame.Render();
        }

        public override void SceneEnd(Scene scene)
        {
            this.Dispose();
            base.SceneEnd(scene);
        }

        public override void Removed(Scene scene)
        {
            this.Dispose();
            base.Removed(scene);
        }

        private void Dispose()
        {
            if (this.mirror != null)
                this.mirror.Dispose();
            this.mirror = (VirtualRenderTarget) null;
        }
    }
}
