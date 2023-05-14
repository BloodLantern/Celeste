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
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private float lightFlickerTimer;
        private float lightFlickerFor = 0.1f;
        private int lastFrame;
        private readonly SoundSource buzzSfx;

        public Payphone(Vector2 pos)
            : base(pos)
        {
            Depth = 1;
            Add(Sprite = GFX.SpriteBank.Create("payphone"));
            Sprite.Play("idle");
            Add(Blink = new Monocle.Image(GFX.Game["cutscenes/payphone/blink"]));
            Blink.Origin = Sprite.Origin;
            Blink.Visible = false;
            Add(light = new VertexLight(new Vector2(-6f, -45f), Color.White, 1f, 8, 96));
            light.Spotlight = true;
            light.SpotlightDirection = new Vector2(0.0f, 1f).Angle();
            Add(bloom = new BloomPoint(new Vector2(-6f, -45f), 0.8f, 8f));
            Add(buzzSfx = new SoundSource());
            _ = buzzSfx.Play("event:/env/local/02_old_site/phone_lamp");
            _ = buzzSfx.Param("on", 1f);
        }

        public override void Update()
        {
            base.Update();
            if (!Broken)
            {
                lightFlickerTimer -= Engine.DeltaTime;
                if (lightFlickerTimer <= 0.0)
                {
                    if (Scene.OnInterval(0.025f))
                    {
                        bool flag = (double)Calc.Random.NextFloat() > 0.5;
                        light.Visible = flag;
                        bloom.Visible = flag;
                        Blink.Visible = !flag;
                        _ = buzzSfx.Param("on", flag ? 1f : 0.0f);
                    }
                    if (lightFlickerTimer < -(double)lightFlickerFor)
                    {
                        lightFlickerTimer = Calc.Random.Choose<float>(0.4f, 0.6f, 0.8f, 1f);
                        lightFlickerFor = Calc.Random.Choose<float>(0.1f, 0.2f, 0.05f);
                        light.Visible = true;
                        bloom.Visible = true;
                        Blink.Visible = false;
                        _ = buzzSfx.Param("on", 1f);
                    }
                }
            }
            else
            {
                Blink.Visible = bloom.Visible = light.Visible = false;
                _ = buzzSfx.Param("on", 0.0f);
            }
            if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == 5 && lastFrame != Sprite.CurrentAnimationFrame)
            {
                Level level = SceneAs<Level>();
                level.ParticlesFG.Emit(Payphone.P_Snow, 10, level.Camera.Position + new Vector2(236f, 152f), new Vector2(10f, 0.0f));
                level.ParticlesFG.Emit(Payphone.P_SnowB, 8, level.Camera.Position + new Vector2(236f, 152f), new Vector2(6f, 0.0f));
                level.DirectionalShake(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            }
            if (Sprite.CurrentAnimationID == "eat" && Sprite.CurrentAnimationFrame == Sprite.CurrentAnimationTotalFrames - 5 && lastFrame != Sprite.CurrentAnimationFrame)
            {
                Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            }

            lastFrame = Sprite.CurrentAnimationFrame;
        }
    }
}
