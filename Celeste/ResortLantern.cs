// Decompiled with JetBrains decompiler
// Type: Celeste.ResortLantern
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class ResortLantern : Entity
    {
        private readonly Monocle.Image holder;
        private readonly Sprite lantern;
        private float collideTimer;
        private int mult;
        private readonly Wiggler wiggler;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private float alphaTimer;
        private readonly SoundSource sfx;

        public ResortLantern(Vector2 position)
            : base(position)
        {
            Collider = new Hitbox(8f, 8f, -4f, -4f);
            Depth = 2000;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            holder = new Monocle.Image(GFX.Game["objects/resortLantern/holder"]);
            _ = holder.CenterOrigin();
            Add(holder);
            lantern = new Sprite(GFX.Game, "objects/resortLantern/");
            lantern.AddLoop(nameof(light), nameof(lantern), 0.3f, 0, 0, 1, 2, 1);
            lantern.Play(nameof(light));
            lantern.Origin = new Vector2(7f, 7f);
            lantern.Position = new Vector2(-1f, -5f);
            Add(lantern);
            wiggler = Wiggler.Create(2.5f, 1.2f, v => lantern.Rotation = (float)((double)v * mult * (Math.PI / 180.0) * 30.0));
            wiggler.StartZero = true;
            Add(wiggler);
            Add(light = new VertexLight(Color.White, 0.95f, 32, 64));
            Add(bloom = new BloomPoint(0.8f, 8f));
            Add(sfx = new SoundSource());
        }

        public ResortLantern(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!CollideCheck<Solid>(Position + (Vector2.UnitX * 8f)))
            {
                return;
            }

            holder.Scale.X = -1f;
            lantern.Scale.X = -1f;
            lantern.X += 2f;
        }

        public override void Update()
        {
            base.Update();
            if (collideTimer > 0.0)
            {
                collideTimer -= Engine.DeltaTime;
            }

            alphaTimer += Engine.DeltaTime;
            bloom.Alpha = light.Alpha = (float)(0.949999988079071 + (Math.Sin(alphaTimer * 1.0) * 0.05000000074505806));
        }

        private void OnPlayer(Player player)
        {
            if (collideTimer <= 0.0)
            {
                if (!(player.Speed != Vector2.Zero))
                {
                    return;
                }

                _ = sfx.Play("event:/game/03_resort/lantern_bump");
                collideTimer = 0.5f;
                mult = Calc.Random.Choose<int>(1, -1);
                wiggler.Start();
            }
            else
            {
                collideTimer = 0.5f;
            }
        }
    }
}
