// Decompiled with JetBrains decompiler
// Type: Celeste.Payphone
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class Payphone : Entity
    {
        public static ParticleType P_Snow;
        public static ParticleType P_SnowB;
        public bool Broken;
        public Sprite Sprite;
        public Monocle.Image Blink;
        private VertexLight light;
        private BloomPoint bloom;
        private float lightFlickerTimer;
        private float lightFlickerFor = 0.1f;
        private int lastFrame;
        private SoundSource buzzSfx;

        public Payphone(Vector2 pos)
            : base(pos)
        {
            this.Depth = 1;
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("payphone")));
            this.Sprite.Play("idle");
            this.Add((Component) (this.Blink = new Monocle.Image(GFX.Game["cutscenes/payphone/blink"])));
            this.Blink.Origin = this.Sprite.Origin;
            this.Blink.Visible = false;
            this.Add((Component) (this.light = new VertexLight(new Vector2(-6f, -45f), Color.White, 1f, 8, 96)));
            this.light.Spotlight = true;
            this.light.SpotlightDirection = new Vector2(0.0f, 1f).Angle();
            this.Add((Component) (this.bloom = new BloomPoint(new Vector2(-6f, -45f), 0.8f, 8f)));
            this.Add((Component) (this.buzzSfx = new SoundSource()));
            this.buzzSfx.Play("event:/env/local/02_old_site/phone_lamp");
            this.buzzSfx.Param("on", 1f);
        }

        public override void Update()
        {
            base.Update();
            if (!this.Broken)
            {
                this.lightFlickerTimer -= Engine.DeltaTime;
                if ((double) this.lightFlickerTimer <= 0.0)
                {
                    if (this.Scene.OnInterval(0.025f))
                    {
                        bool flag = (double) Calc.Random.NextFloat() > 0.5;
                        this.light.Visible = flag;
                        this.bloom.Visible = flag;
                        this.Blink.Visible = !flag;
                        this.buzzSfx.Param("on", flag ? 1f : 0.0f);
                    }
                    if ((double) this.lightFlickerTimer < -(double) this.lightFlickerFor)
                    {
                        this.lightFlickerTimer = Calc.Random.Choose<float>(0.4f, 0.6f, 0.8f, 1f);
                        this.lightFlickerFor = Calc.Random.Choose<float>(0.1f, 0.2f, 0.05f);
                        this.light.Visible = true;
                        this.bloom.Visible = true;
                        this.Blink.Visible = false;
                        this.buzzSfx.Param("on", 1f);
                    }
                }
            }
            else
            {
                this.Blink.Visible = this.bloom.Visible = this.light.Visible = false;
                this.buzzSfx.Param("on", 0.0f);
            }
            if (this.Sprite.CurrentAnimationID == "eat" && this.Sprite.CurrentAnimationFrame == 5 && this.lastFrame != this.Sprite.CurrentAnimationFrame)
            {
                Level level = this.SceneAs<Level>();
                level.ParticlesFG.Emit(Payphone.P_Snow, 10, level.Camera.Position + new Vector2(236f, 152f), new Vector2(10f, 0.0f));
                level.ParticlesFG.Emit(Payphone.P_SnowB, 8, level.Camera.Position + new Vector2(236f, 152f), new Vector2(6f, 0.0f));
                level.DirectionalShake(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            if (this.Sprite.CurrentAnimationID == "eat" && this.Sprite.CurrentAnimationFrame == this.Sprite.CurrentAnimationTotalFrames - 5 && this.lastFrame != this.Sprite.CurrentAnimationFrame)
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            this.lastFrame = this.Sprite.CurrentAnimationFrame;
        }
    }
}
