﻿using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class Lightning : Entity
    {
        public static ParticleType P_Shatter;
        public const string Flag = "disable_lightning";
        public float Fade;
        private bool disappearing;
        private float toggleOffset;
        public int VisualWidth;
        public int VisualHeight;

        public Lightning(Vector2 position, int width, int height, Vector2? node, float moveTime)
            : base(position)
        {
            VisualWidth = width;
            VisualHeight = height;
            Collider = new Hitbox(width - 2, height - 2, 1f, 1f);
            Add(new PlayerCollider(OnPlayer));
            if (node.HasValue)
                Add(new Coroutine(MoveRoutine(position, node.Value, moveTime)));
            toggleOffset = Calc.Random.NextFloat();
        }

        public Lightning(EntityData data, Vector2 levelOffset)
            : this(data.Position + levelOffset, data.Width, data.Height, data.FirstNodeNullable(levelOffset), data.Float("moveTime"))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Tracker.GetEntity<LightningRenderer>().Track(this);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            scene.Tracker.GetEntity<LightningRenderer>().Untrack(this);
        }

        public override void Update()
        {
            if (Collidable && Scene.OnInterval(0.25f, toggleOffset))
                ToggleCheck();
            if (!Collidable && Scene.OnInterval(0.05f, toggleOffset))
                ToggleCheck();
            base.Update();
        }

        public void ToggleCheck() => Collidable = Visible = InView();

        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return X + (double) Width > camera.X - 16.0 && Y + (double) Height > camera.Y - 16.0 && X < camera.X + 320.0 + 16.0 && Y < camera.Y + 180.0 + 16.0;
        }

        private void OnPlayer(Player player)
        {
            if (disappearing || SaveData.Instance.Assists.Invincible)
                return;
            int num = Math.Sign(player.X - X);
            if (num == 0)
                num = -1;
            player.Die(Vector2.UnitX * num);
        }

        private IEnumerator MoveRoutine(Vector2 start, Vector2 end, float moveTime)
        {
            while (true)
            {
                yield return Move(start, end, moveTime);
                yield return Move(end, start, moveTime);
            }
        }

        private IEnumerator Move(Vector2 start, Vector2 end, float moveTime)
        {
            Lightning lightning = this;
            float at = 0.0f;
            while (true)
            {
                lightning.Position = Vector2.Lerp(start, end, Ease.SineInOut(at));
                if (at < 1.0)
                {
                    yield return null;
                    at = MathHelper.Clamp(at + Engine.DeltaTime / moveTime, 0.0f, 1f);
                }
                else
                    break;
            }
        }

        private void Shatter()
        {
            if (Scene == null)
                return;
            for (int x = 4; x < (double) Width; x += 8)
            {
                for (int y = 4; y < (double) Height; y += 8)
                    SceneAs<Level>().ParticlesFG.Emit(Lightning.P_Shatter, 1, TopLeft + new Vector2(x, y), Vector2.One * 3f);
            }
        }

        public static IEnumerator PulseRoutine(Level level)
        {
            float t;
            for (t = 0.0f; t < 1.0; t += Engine.DeltaTime * 8f)
            {
                Lightning.SetPulseValue(level, t);
                yield return null;
            }
            for (t = 1f; t > 0.0; t -= Engine.DeltaTime * 8f)
            {
                Lightning.SetPulseValue(level, t);
                yield return null;
            }
            Lightning.SetPulseValue(level, 0.0f);
        }

        private static void SetPulseValue(Level level, float t)
        {
            BloomRenderer bloom = level.Bloom;
            LightningRenderer entity = level.Tracker.GetEntity<LightningRenderer>();
            Glitch.Value = MathHelper.Lerp(0.0f, 0.075f, t);
            double num = MathHelper.Lerp(1f, 1.2f, t);
            bloom.Strength = (float) num;
            entity.Fade = t * 0.2f;
        }

        private static void SetBreakValue(Level level, float t)
        {
            BloomRenderer bloom = level.Bloom;
            LightningRenderer entity = level.Tracker.GetEntity<LightningRenderer>();
            Glitch.Value = MathHelper.Lerp(0.0f, 0.15f, t);
            double num = MathHelper.Lerp(1f, 1.5f, t);
            bloom.Strength = (float) num;
            entity.Fade = t * 0.6f;
        }

        public static IEnumerator RemoveRoutine(Level level, Action onComplete = null)
        {
            List<Lightning> blocks = level.Entities.FindAll<Lightning>();
            foreach (Lightning lightning in new List<Lightning>(blocks))
            {
                lightning.disappearing = true;
                if (lightning.Right < (double) level.Camera.Left || lightning.Bottom < (double) level.Camera.Top || lightning.Left > (double) level.Camera.Right || lightning.Top > (double) level.Camera.Bottom)
                {
                    blocks.Remove(lightning);
                    lightning.RemoveSelf();
                }
            }
            LightningRenderer entity1 = level.Tracker.GetEntity<LightningRenderer>();
            entity1.StopAmbience();
            entity1.UpdateSeeds = false;
            float t;
            for (t = 0.0f; t < 1.0; t += Engine.DeltaTime * 4f)
            {
                Lightning.SetBreakValue(level, t);
                yield return null;
            }
            Lightning.SetBreakValue(level, 1f);
            level.Shake();
            for (int index = blocks.Count - 1; index >= 0; --index)
                blocks[index].Shatter();
            for (t = 0.0f; t < 1.0; t += Engine.DeltaTime * 8f)
            {
                Lightning.SetBreakValue(level, 1f - t);
                yield return null;
            }
            Lightning.SetBreakValue(level, 0.0f);
            foreach (Entity entity2 in blocks)
                entity2.RemoveSelf();
            FlingBird first = level.Entities.FindFirst<FlingBird>();
            if (first != null)
                first.LightningRemoved = true;
            if (onComplete != null)
                onComplete();
        }
    }
}
