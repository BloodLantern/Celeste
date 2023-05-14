// Decompiled with JetBrains decompiler
// Type: Celeste.Checkpoint
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Checkpoint : Entity
    {
        private const float LightAlpha = 0.8f;
        private const float BloomAlpha = 0.5f;
        private const float TargetFade = 0.5f;
        private Image image;
        private Sprite sprite;
        private Sprite flash;
        private VertexLight light;
        private BloomPoint bloom;
        private bool triggered;
        private float sine = (float)Math.PI / 2;
        private float fade = 1f;
        private readonly string bg;
        public Vector2 SpawnOffset;

        public Checkpoint(Vector2 position, string bg = "", Vector2? spawnTarget = null)
            : base(position)
        {
            Depth = 9990;
            SpawnOffset = spawnTarget.HasValue ? spawnTarget.Value - Position : Vector2.Zero;
            this.bg = bg;
        }

        public Checkpoint(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Attr(nameof(bg)), data.FirstNodeNullable(new Vector2?(offset)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level scene1 = Scene as Level;
            int id1 = scene1.Session.Area.ID;
            string id2 = !string.IsNullOrWhiteSpace(bg) ? "objects/checkpoint/bg/" + bg : "objects/checkpoint/bg/" + id1;
            if (GFX.Game.Has(id2))
            {
                Add(image = new Image(GFX.Game[id2]));
                _ = image.JustifyOrigin(0.5f, 1f);
            }
            Add(sprite = GFX.SpriteBank.Create("checkpoint_highlight"));
            sprite.Play("off");
            Add(flash = GFX.SpriteBank.Create("checkpoint_flash"));
            flash.Visible = false;
            flash.Color = Color.White * 0.6f;
            if (!SaveData.Instance.HasCheckpoint(scene1.Session.Area, scene1.Session.Level))
            {
                return;
            }

            TurnOn(false);
        }

        public override void Update()
        {
            if (!triggered)
            {
                Level scene = Scene as Level;
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && !scene.Transitioning)
                {
                    if (!entity.CollideCheck<CheckpointBlockerTrigger>() && SaveData.Instance.SetCheckpoint(scene.Session.Area, scene.Session.Level))
                    {
                        scene.AutoSave();
                        TurnOn(true);
                    }
                    triggered = true;
                }
            }
            if (triggered && sprite.CurrentAnimationID == "on")
            {
                sine += Engine.DeltaTime * 2f;
                fade = Calc.Approach(fade, 0.5f, Engine.DeltaTime);
                sprite.Color = Color.White * (0.5f + ((1 + (float)Math.Sin(sine)) / 2f * 0.5f)) * fade;
            }
            base.Update();
        }

        private void TurnOn(bool animate)
        {
            triggered = true;
            Add(light = new VertexLight(Color.White, 0f, 16, 32));
            Add(bloom = new BloomPoint(0f, 16f));
            light.Position = new Vector2(0f, -8f);
            bloom.Position = new Vector2(0f, -8f);
            flash.Visible = true;
            flash.Play("flash", true);
            if (animate)
            {
                sprite.Play("turnOn");
                Add(new Coroutine(EaseLightsOn()));
                fade = 1f;
            }
            else
            {
                fade = 0.5f;
                sprite.Play("on");
                sprite.Color = Color.White * 0.5f;
                light.Alpha = 0.8f;
                bloom.Alpha = 0.5f;
            }
        }

        private IEnumerator EaseLightsOn()
        {
            float lightStartRadius = light.StartRadius;
            float lightEndRadius = light.EndRadius;
            for (float p = 0f; p < 1; p += Engine.DeltaTime * 0.5f)
            {
                float num = Ease.BigBackOut(p);
                light.Alpha = 0.8f * num;
                light.StartRadius = (int)(lightStartRadius + (Calc.YoYo(p) * 8));
                light.EndRadius = (int)(lightEndRadius + (Calc.YoYo(p) * 16));
                bloom.Alpha = 0.5f * num;
                yield return null;
            }
        }
    }
}
