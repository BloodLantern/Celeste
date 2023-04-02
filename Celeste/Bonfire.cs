// Decompiled with JetBrains decompiler
// Type: Celeste.Bonfire
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class Bonfire : Entity
    {
        private Mode mode;
        private readonly Sprite sprite;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private readonly Wiggler wiggle;
        private float brightness;
        private float multiplier;
        public bool Activated;
        private readonly SoundSource loopSfx;

        public Bonfire(Vector2 position, Mode mode)
        {
            Tag = (int)Tags.TransitionUpdate;
            Depth = -5;
            Add(loopSfx = new SoundSource());
            Add(sprite = GFX.SpriteBank.Create("campfire"));
            Add(light = new VertexLight(new Vector2(0.0f, -6f), Color.PaleVioletRed, 1f, 32, 64));
            Add(bloom = new BloomPoint(new Vector2(0.0f, -6f), 1f, 32f));
            Add(wiggle = Wiggler.Create(0.2f, 4f, f => light.Alpha = bloom.Alpha = Math.Min(1f, brightness + (f * 0.25f)) * multiplier));
            Position = position;
            this.mode = mode;
        }

        public Bonfire(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Enum<Mode>(nameof(mode)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SetMode(mode);
        }

        public void SetMode(Mode mode)
        {
            this.mode = mode;
            switch (mode)
            {
                case Mode.Lit:
                    if (Activated)
                    {
                        _ = Audio.Play("event:/env/local/campfire_start", Position);
                        _ = loopSfx.Play("event:/env/local/campfire_loop");
                        sprite.Play(SceneAs<Level>().Session.Dreaming ? "startDream" : "start");
                        break;
                    }
                    _ = loopSfx.Play("event:/env/local/campfire_loop");
                    sprite.Play(SceneAs<Level>().Session.Dreaming ? "burnDream" : "burn");
                    break;
                case Mode.Smoking:
                    sprite.Play("smoking");
                    break;
                default:
                    sprite.Play("idle");
                    bloom.Alpha = light.Alpha = brightness = 0f;
                    break;
            }
            Activated = true;
        }

        public override void Update()
        {
            if (mode == Mode.Lit)
            {
                multiplier = Calc.Approach(multiplier, 1f, Engine.DeltaTime * 2f);
                if (Scene.OnInterval(0.25f))
                {
                    brightness = 0.5f + Calc.Random.NextFloat(0.5f);
                    wiggle.Start();
                }
            }
            base.Update();
        }

        public enum Mode
        {
            Unlit,
            Lit,
            Smoking,
        }
    }
}
