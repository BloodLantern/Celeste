// Decompiled with JetBrains decompiler
// Type: Celeste.FlingBird
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class FlingBird : Entity
    {
        public static ParticleType P_Feather;
        public const float SkipDist = 100f;
        public static readonly Vector2 FlingSpeed = new(380f, -100f);
        private Vector2 spriteOffset = new(0.0f, 8f);
        private readonly Sprite sprite;
        private FlingBird.States state;
        private Vector2 flingSpeed;
        private Vector2 flingTargetSpeed;
        private float flingAccel;
        private Color trailColor = Calc.HexToColor("639bff");
        private readonly EntityData entityData;
        private readonly SoundSource moveSfx;
        private int segmentIndex;
        public List<Vector2[]> NodeSegments;
        public List<bool> SegmentsWaiting;
        public bool LightningRemoved;

        public FlingBird(Vector2[] nodes, bool skippable)
            : base(nodes[0])
        {
            Depth = -1;
            Add(sprite = GFX.SpriteBank.Create("bird"));
            sprite.Play("hover");
            sprite.Scale.X = -1f;
            sprite.Position = spriteOffset;
            sprite.OnFrameChange = spr => BirdNPC.FlapSfxCheck(sprite);
            Collider = new Monocle.Circle(16f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(moveSfx = new SoundSource());
            NodeSegments = new List<Vector2[]>
            {
                nodes
            };
            SegmentsWaiting = new List<bool>
            {
                skippable
            };
            Add(new TransitionListener()
            {
                OnOut = t => sprite.Color = Color.White * (1f - Calc.Map(t, 0.0f, 0.4f))
            });
        }

        public FlingBird(EntityData data, Vector2 levelOffset)
            : this(data.NodesWithPosition(levelOffset), data.Bool("waiting"))
        {
            entityData = data;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            List<FlingBird> all = Scene.Entities.FindAll<FlingBird>();
            for (int index = all.Count - 1; index >= 0; --index)
            {
                if (all[index].entityData.Level.Name != entityData.Level.Name)
                {
                    all.RemoveAt(index);
                }
            }
            all.Sort((a, b) => Math.Sign(a.X - b.X));
            if (all[0] == this)
            {
                for (int index = 1; index < all.Count; ++index)
                {
                    NodeSegments.Add(all[index].NodeSegments[0]);
                    SegmentsWaiting.Add(all[index].SegmentsWaiting[0]);
                    all[index].RemoveSelf();
                }
            }
            if (SegmentsWaiting[0])
            {
                sprite.Play("hoverStressed");
                sprite.Scale.X = 1f;
            }
            Player entity = scene.Tracker.GetEntity<Player>();
            if (entity == null || (double)entity.X <= (double)X)
            {
                return;
            }

            RemoveSelf();
        }

        private void Skip()
        {
            state = FlingBird.States.Move;
            Add(new Coroutine(MoveRoutine()));
        }

        private void OnPlayer(Player player)
        {
            if (state != FlingBird.States.Wait || !player.DoFlingBird(this))
            {
                return;
            }

            flingSpeed = player.Speed * 0.4f;
            flingSpeed.Y = 120f;
            flingTargetSpeed = Vector2.Zero;
            flingAccel = 1000f;
            player.Speed = Vector2.Zero;
            state = FlingBird.States.Fling;
            Add(new Coroutine(DoFlingRoutine(player)));
            _ = Audio.Play("event:/new_content/game/10_farewell/bird_throw", Center);
        }

        public override void Update()
        {
            base.Update();
            if (state != FlingBird.States.Wait)
            {
                sprite.Position = Calc.Approach(sprite.Position, spriteOffset, 32f * Engine.DeltaTime);
            }

            switch (state)
            {
                case FlingBird.States.Wait:
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null && (double)entity.X - (double)X >= 100.0)
                    {
                        Skip();
                        break;
                    }
                    if (SegmentsWaiting[segmentIndex] && LightningRemoved)
                    {
                        Skip();
                        break;
                    }
                    if (entity == null)
                    {
                        break;
                    }

                    float num = Calc.ClampedMap((entity.Center - Position).Length(), 16f, 64f, 12f, 0.0f);
                    sprite.Position = Calc.Approach(sprite.Position, spriteOffset + ((entity.Center - Position).SafeNormalize() * num), 32f * Engine.DeltaTime);
                    break;
                case FlingBird.States.Fling:
                    if (flingAccel > 0.0)
                    {
                        flingSpeed = Calc.Approach(flingSpeed, flingTargetSpeed, flingAccel * Engine.DeltaTime);
                    }

                    Position += flingSpeed * Engine.DeltaTime;
                    break;
                case FlingBird.States.WaitForLightningClear:
                    if (Scene.Entities.FindFirst<Lightning>() != null && (double)X <= (Scene as Level).Bounds.Right)
                    {
                        break;
                    }

                    sprite.Scale.X = 1f;
                    state = FlingBird.States.Leaving;
                    Add(new Coroutine(LeaveRoutine()));
                    break;
            }
        }

        private IEnumerator DoFlingRoutine(Player player)
        {
            FlingBird flingBird = this;
            Level level = flingBird.Scene as Level;
            Vector2 screenSpaceFocusPoint = player.Position - level.Camera.Position;
            screenSpaceFocusPoint.X = Calc.Clamp(screenSpaceFocusPoint.X, 145f, 215f);
            screenSpaceFocusPoint.Y = Calc.Clamp(screenSpaceFocusPoint.Y, 85f, 95f);
            flingBird.Add(new Coroutine(level.ZoomTo(screenSpaceFocusPoint, 1.1f, 0.2f)));
            Engine.TimeRate = 0.8f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            while (flingBird.flingSpeed != Vector2.Zero)
            {
                yield return null;
            }

            flingBird.sprite.Play("throw");
            flingBird.sprite.Scale.X = 1f;
            flingBird.flingSpeed = new Vector2(-140f, 140f);
            flingBird.flingTargetSpeed = Vector2.Zero;
            flingBird.flingAccel = 1400f;
            yield return 0.1f;
            Celeste.Freeze(0.05f);
            flingBird.flingTargetSpeed = FlingBird.FlingSpeed;
            flingBird.flingAccel = 6000f;
            yield return 0.1f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Engine.TimeRate = 1f;
            level.Shake();
            flingBird.Add(new Coroutine(level.ZoomBack(0.1f)));
            player.FinishFlingBird();
            flingBird.flingTargetSpeed = Vector2.Zero;
            flingBird.flingAccel = 4000f;
            yield return 0.3f;
            flingBird.Add(new Coroutine(flingBird.MoveRoutine()));
        }

        private IEnumerator MoveRoutine()
        {
            FlingBird flingBird = this;
            flingBird.state = FlingBird.States.Move;
            flingBird.sprite.Play("fly");
            flingBird.sprite.Scale.X = 1f;
            _ = flingBird.moveSfx.Play("event:/new_content/game/10_farewell/bird_relocate");
            for (int nodeIndex = 1; nodeIndex < flingBird.NodeSegments[flingBird.segmentIndex].Length - 1; nodeIndex += 2)
            {
                Vector2 position = flingBird.Position;
                Vector2 anchor = flingBird.NodeSegments[flingBird.segmentIndex][nodeIndex];
                Vector2 to = flingBird.NodeSegments[flingBird.segmentIndex][nodeIndex + 1];
                yield return flingBird.MoveOnCurve(position, anchor, to);
            }
            ++flingBird.segmentIndex;
            bool atEnding = flingBird.segmentIndex >= flingBird.NodeSegments.Count;
            if (!atEnding)
            {
                Vector2 position = flingBird.Position;
                Vector2 anchor = flingBird.NodeSegments[flingBird.segmentIndex - 1][flingBird.NodeSegments[flingBird.segmentIndex - 1].Length - 1];
                Vector2 to = flingBird.NodeSegments[flingBird.segmentIndex][0];
                yield return flingBird.MoveOnCurve(position, anchor, to);
            }
            flingBird.sprite.Rotation = 0.0f;
            flingBird.sprite.Scale = Vector2.One;
            if (atEnding)
            {
                flingBird.sprite.Play("hoverStressed");
                flingBird.sprite.Scale.X = 1f;
                flingBird.state = FlingBird.States.WaitForLightningClear;
            }
            else
            {
                if (flingBird.SegmentsWaiting[flingBird.segmentIndex])
                {
                    flingBird.sprite.Play("hoverStressed");
                }
                else
                {
                    flingBird.sprite.Play("hover");
                }

                flingBird.sprite.Scale.X = -1f;
                flingBird.state = FlingBird.States.Wait;
            }
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator LeaveRoutine()
        {
            sprite.Scale.X = 1f;
            sprite.Play("fly", false, false);
            Vector2 vector = new((base.Scene as Level).Bounds.Right + 32, base.Y);
            yield return MoveOnCurve(Position, ((Position + vector) * 0.5f) - (Vector2.UnitY * 12f), vector);
            base.RemoveSelf();
            yield break;
        }

        private IEnumerator MoveOnCurve(Vector2 from, Vector2 anchor, Vector2 to)
        {
            FlingBird flingBird = this;
            SimpleCurve curve = new(from, to, anchor);
            float duration = curve.GetLengthParametric(32) / 500f;
            Vector2 was = from;
            for (float t = 0.016f; (double)t <= 1.0; t += Engine.DeltaTime / duration)
            {
                flingBird.Position = curve.GetPoint(t).Floor();
                flingBird.sprite.Rotation = Calc.Angle(curve.GetPoint(Math.Max(0.0f, t - 0.05f)), curve.GetPoint(Math.Min(1f, t + 0.05f)));
                flingBird.sprite.Scale.X = 1.25f;
                flingBird.sprite.Scale.Y = 0.7f;
                if ((double)(was - flingBird.Position).Length() > 32.0)
                {
                    TrailManager.Add(flingBird, flingBird.trailColor);
                    was = flingBird.Position;
                }
                yield return null;
            }
            flingBird.Position = to;
        }

        public override void Render()
        {
            base.Render();
        }

        private void DrawLine(Vector2 a, Vector2 anchor, Vector2 b)
        {
            new SimpleCurve(a, b, anchor).Render(Color.Red, 32);
        }

        private enum States
        {
            Wait,
            Fling,
            Move,
            WaitForLightningClear,
            Leaving,
        }
    }
}
