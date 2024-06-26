﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste
{
    [Tracked]
    public class TrailManager : Entity
    {
        private static BlendState MaxBlendState = new BlendState
        {
            ColorSourceBlend = Blend.DestinationAlpha,
            AlphaSourceBlend = Blend.DestinationAlpha
        };
        private const int size = 64;
        private const int columns = 8;
        private const int rows = 8;
        private Snapshot[] snapshots = new Snapshot[64];
        private VirtualRenderTarget buffer;
        private bool dirty;

        public TrailManager()
        {
            Tag = (int) Tags.Global;
            Depth = 10;
            Add(new BeforeRenderHook(BeforeRender));
            Add(new MirrorReflection());
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (buffer != null)
                buffer.Dispose();
            buffer = null;
        }

        private void BeforeRender()
        {
            if (!dirty)
                return;
            if (buffer == null)
                buffer = VirtualContent.CreateRenderTarget("trail-manager", 512, 512);
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, LightingRenderer.OccludeBlendState);
            for (int index = 0; index < snapshots.Length; ++index)
            {
                if (snapshots[index] != null && !snapshots[index].Drawn)
                    Draw.Rect(index % 8 * 64, index / 8 * 64, 64f, 64f, Color.Transparent);
            }
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
            for (int index1 = 0; index1 < snapshots.Length; ++index1)
            {
                if (snapshots[index1] != null && !snapshots[index1].Drawn)
                {
                    Snapshot snapshot = snapshots[index1];
                    Vector2 vector2 = new Vector2((float) ((index1 % 8 + 0.5) * 64.0), (float) ((index1 / 8 + 0.5) * 64.0)) - snapshot.Position;
                    if (snapshot.Hair != null)
                    {
                        for (int index2 = 0; index2 < snapshot.Hair.Nodes.Count; ++index2)
                            snapshot.Hair.Nodes[index2] += vector2;
                        snapshot.Hair.Render();
                        for (int index3 = 0; index3 < snapshot.Hair.Nodes.Count; ++index3)
                            snapshot.Hair.Nodes[index3] -= vector2;
                    }
                    Vector2 scale = snapshot.Sprite.Scale;
                    snapshot.Sprite.Scale = snapshot.SpriteScale;
                    Image sprite1 = snapshot.Sprite;
                    sprite1.Position += vector2;
                    snapshot.Sprite.Render();
                    snapshot.Sprite.Scale = scale;
                    Image sprite2 = snapshot.Sprite;
                    sprite2.Position -= vector2;
                    snapshot.Drawn = true;
                }
            }
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, TrailManager.MaxBlendState);
            Draw.Rect(0.0f, 0.0f, buffer.Width, buffer.Height, new Color(1f, 1f, 1f, 1f));
            Draw.SpriteBatch.End();
            dirty = false;
        }

        public static void Add(
            Entity entity,
            Color color,
            float duration = 1f,
            bool frozenUpdate = false,
            bool useRawDeltaTime = false)
        {
            Image sprite = (Image) entity.Get<PlayerSprite>() ?? entity.Get<Sprite>();
            PlayerHair hair = entity.Get<PlayerHair>();
            TrailManager.Add(entity.Position, sprite, hair, sprite.Scale, color, entity.Depth + 1, duration, frozenUpdate, useRawDeltaTime);
        }

        public static void Add(Entity entity, Vector2 scale, Color color, float duration = 1f)
        {
            Image sprite = (Image) entity.Get<PlayerSprite>() ?? entity.Get<Sprite>();
            PlayerHair hair = entity.Get<PlayerHair>();
            TrailManager.Add(entity.Position, sprite, hair, scale, color, entity.Depth + 1, duration);
        }

        public static void Add(Vector2 position, Image image, Color color, int depth, float duration = 1f) => TrailManager.Add(position, image, null, image.Scale, color, depth, duration);

        public static Snapshot Add(
            Vector2 position,
            Image sprite,
            PlayerHair hair,
            Vector2 scale,
            Color color,
            int depth,
            float duration = 1f,
            bool frozenUpdate = false,
            bool useRawDeltaTime = false)
        {
            TrailManager manager = Engine.Scene.Tracker.GetEntity<TrailManager>();
            if (manager == null)
            {
                manager = new TrailManager();
                Engine.Scene.Add(manager);
            }
            for (int index = 0; index < manager.snapshots.Length; ++index)
            {
                if (manager.snapshots[index] == null)
                {
                    Snapshot snapshot = Engine.Pooler.Create<Snapshot>();
                    snapshot.Init(manager, index, position, sprite, hair, scale, color, duration, depth, frozenUpdate, useRawDeltaTime);
                    manager.snapshots[index] = snapshot;
                    manager.dirty = true;
                    Engine.Scene.Add(snapshot);
                    return snapshot;
                }
            }
            return null;
        }

        public static void Clear()
        {
            TrailManager entity = Engine.Scene.Tracker.GetEntity<TrailManager>();
            if (entity == null)
                return;
            for (int index = 0; index < entity.snapshots.Length; ++index)
            {
                if (entity.snapshots[index] != null)
                    entity.snapshots[index].RemoveSelf();
            }
        }

        [Pooled]
        [Tracked]
        public class Snapshot : Entity
        {
            public TrailManager Manager;
            public Image Sprite;
            public Vector2 SpriteScale;
            public PlayerHair Hair;
            public int Index;
            public Color Color;
            public float Percent;
            public float Duration;
            public bool Drawn;
            public bool UseRawDeltaTime;

            public Snapshot() => Add(new MirrorReflection());

            public void Init(
                TrailManager manager,
                int index,
                Vector2 position,
                Image sprite,
                PlayerHair hair,
                Vector2 scale,
                Color color,
                float duration,
                int depth,
                bool frozenUpdate,
                bool useRawDeltaTime)
            {
                Tag = (int) Tags.Global;
                if (frozenUpdate)
                    Tag |= (int) Tags.FrozenUpdate;
                Manager = manager;
                Index = index;
                Position = position;
                Sprite = sprite;
                SpriteScale = scale;
                Hair = hair;
                Color = color;
                Percent = 0.0f;
                Duration = duration;
                Depth = depth;
                Drawn = false;
                UseRawDeltaTime = useRawDeltaTime;
            }

            public override void Update()
            {
                if (Duration <= 0.0)
                {
                    if (!Drawn)
                        return;
                    RemoveSelf();
                }
                else
                {
                    if (Percent >= 1.0)
                        RemoveSelf();
                    Percent += (UseRawDeltaTime ? Engine.RawDeltaTime : Engine.DeltaTime) / Duration;
                }
            }

            public override void Render()
            {
                VirtualRenderTarget buffer = Manager.buffer;
                Rectangle rectangle = new Rectangle(Index % 8 * 64, Index / 8 * 64, 64, 64);
                float num = Duration > 0.0 ? (float) (0.75 * (1.0 - Ease.CubeOut(Percent))) : 1f;
                if (buffer == null)
                    return;
                Draw.SpriteBatch.Draw((RenderTarget2D) buffer, Position, rectangle, Color * num, 0.0f, new Vector2(64f, 64f) * 0.5f, Vector2.One, SpriteEffects.None, 0.0f);
            }

            public override void Removed(Scene scene)
            {
                if (Manager != null)
                    Manager.snapshots[Index] = null;
                base.Removed(scene);
            }
        }
    }
}
