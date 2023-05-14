// Decompiled with JetBrains decompiler
// Type: Celeste.Torch
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class Torch : Entity
    {
        public static ParticleType P_OnLight;
        public const float BloomAlpha = 0.5f;
        public const int StartRadius = 48;
        public const int EndRadius = 64;
        public static readonly Color Color = Color.Lerp(Color.White, Color.Cyan, 0.5f);
        public static readonly Color StartLitColor = Color.Lerp(Color.White, Color.Orange, 0.5f);
        private EntityID id;
        private bool lit;
        private readonly VertexLight light;
        private readonly BloomPoint bloom;
        private readonly bool startLit;
        private readonly Sprite sprite;

        public Torch(EntityID id, Vector2 position, bool startLit)
            : base(position)
        {
            this.id = id;
            this.startLit = startLit;
            Collider = new Hitbox(32f, 32f, -16f, -16f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(light = new VertexLight(Torch.Color, 1f, 48, 64));
            Add(bloom = new BloomPoint(0.5f, 8f));
            bloom.Visible = false;
            light.Visible = false;
            Depth = 2000;
            if (startLit)
            {
                light.Color = Torch.StartLitColor;
                Add(sprite = GFX.SpriteBank.Create("litTorch"));
            }
            else
            {
                Add(sprite = GFX.SpriteBank.Create("torch"));
            }
        }

        public Torch(EntityData data, Vector2 offset, EntityID id)
            : this(id, data.Position + offset, data.Bool(nameof(startLit)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!startLit && !SceneAs<Level>().Session.GetFlag(FlagName))
            {
                return;
            }

            bloom.Visible = light.Visible = true;
            lit = true;
            Collidable = false;
            sprite.Play("on");
        }

        private void OnPlayer(Player player)
        {
            if (lit)
            {
                return;
            }

            _ = Audio.Play("event:/game/05_mirror_temple/torch_activate", Position);
            lit = true;
            bloom.Visible = true;
            light.Visible = true;
            Collidable = false;
            sprite.Play("turnOn");
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.BackOut, start: true);
            tween.OnUpdate = t =>
            {
                light.Color = Color.Lerp(Color.White, Torch.Color, t.Eased);
                light.StartRadius = (float)(48.0 + ((1.0 - (double)t.Eased) * 32.0));
                light.EndRadius = (float)(64.0 + ((1.0 - (double)t.Eased) * 32.0));
                bloom.Alpha = (float)(0.5 + (0.5 * (1.0 - (double)t.Eased)));
            };
            Add(tween);
            SceneAs<Level>().Session.SetFlag(FlagName);
            SceneAs<Level>().ParticlesFG.Emit(Torch.P_OnLight, 12, Position, new Vector2(3f, 3f));
        }

        private string FlagName => "torch_" + id.Key;
    }
}
