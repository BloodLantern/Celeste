// Decompiled with JetBrains decompiler
// Type: Celeste.FinalBossMovingBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked(false)]
    public class FinalBossMovingBlock : Solid
    {
        public static ParticleType P_Stop;
        public static ParticleType P_Break;
        public int BossNodeIndex;
        private float startDelay;
        private int nodeIndex;
        private readonly Vector2[] nodes;
        private readonly TileGrid sprite;
        private readonly TileGrid highlight;
        private Coroutine moveCoroutine;
        private bool isHighlighted;

        public FinalBossMovingBlock(Vector2[] nodes, float width, float height, int bossNodeIndex)
            : base(nodes[0], width, height, false)
        {
            BossNodeIndex = bossNodeIndex;
            this.nodes = nodes;
            int newSeed = Calc.Random.Next();
            Calc.PushRandom(newSeed);
            sprite = GFX.FGAutotiler.GenerateBox('g', (int)Width / 8, (int)Height / 8).TileGrid;
            Add(sprite);
            Calc.PopRandom();
            Calc.PushRandom(newSeed);
            highlight = GFX.FGAutotiler.GenerateBox('G', (int)((double)Width / 8.0), (int)Height / 8).TileGrid;
            highlight.Alpha = 0.0f;
            Add(highlight);
            Calc.PopRandom();
            Add(new TileInterceptor(sprite, false));
            Add(new LightOcclude());
        }

        public FinalBossMovingBlock(EntityData data, Vector2 offset)
            : this(data.NodesWithPosition(offset), data.Width, data.Height, data.Int(nameof(nodeIndex)))
        {
        }

        public override void OnShake(Vector2 amount)
        {
            base.OnShake(amount);
            sprite.Position = amount;
        }

        public void StartMoving(float delay)
        {
            startDelay = delay;
            Add(moveCoroutine = new Coroutine(MoveSequence()));
        }

        private IEnumerator MoveSequence()
        {
            FinalBossMovingBlock finalBossMovingBlock1 = this;
            while (true)
            {
                FinalBossMovingBlock finalBossMovingBlock = finalBossMovingBlock1;
                finalBossMovingBlock1.StartShaking(0.2f + finalBossMovingBlock1.startDelay);
                if (!finalBossMovingBlock1.isHighlighted)
                {
                    for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / (float)(0.20000000298023224 + finalBossMovingBlock1.startDelay + 0.20000000298023224))
                    {
                        finalBossMovingBlock1.highlight.Alpha = Ease.CubeIn(p);
                        finalBossMovingBlock1.sprite.Alpha = 1f - finalBossMovingBlock1.highlight.Alpha;
                        yield return null;
                    }
                    finalBossMovingBlock1.highlight.Alpha = 1f;
                    finalBossMovingBlock1.sprite.Alpha = 0.0f;
                    finalBossMovingBlock1.isHighlighted = true;
                }
                else
                {
                    yield return (float)(0.20000000298023224 + finalBossMovingBlock1.startDelay + 0.20000000298023224);
                }

                finalBossMovingBlock1.startDelay = 0.0f;
                ++finalBossMovingBlock1.nodeIndex;
                finalBossMovingBlock1.nodeIndex %= finalBossMovingBlock1.nodes.Length;
                Vector2 from = finalBossMovingBlock1.Position;
                Vector2 to = finalBossMovingBlock1.nodes[finalBossMovingBlock1.nodeIndex];
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, 0.8f, true);
                tween.OnUpdate = t => finalBossMovingBlock.MoveTo(Vector2.Lerp(from, to, t.Eased));
                tween.OnComplete = t =>
                {
                    if (finalBossMovingBlock.CollideCheck<SolidTiles>(finalBossMovingBlock.Position + ((to - from).SafeNormalize() * 2f)))
                    {
                        _ = Audio.Play("event:/game/06_reflection/fallblock_boss_impact", finalBossMovingBlock.Center);
                        finalBossMovingBlock.ImpactParticles(to - from);
                    }
                    else
                    {
                        finalBossMovingBlock.StopParticles(to - from);
                    }
                };
                finalBossMovingBlock1.Add(tween);
                yield return 0.8f;
            }
        }

        private void StopParticles(Vector2 moved)
        {
            Level level = SceneAs<Level>();
            float direction = moved.Angle();
            if (moved.X > 0.0)
            {
                Vector2 vector2 = new(Right - 1f, Top);
                for (int index = 0; index < (double)Height; index += 4)
                {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, vector2 + (Vector2.UnitY * (2 + index + Calc.Random.Range(-1, 1))), direction);
                }
            }
            else if (moved.X < 0.0)
            {
                Vector2 vector2 = new(Left, Top);
                for (int index = 0; index < (double)Height; index += 4)
                {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, vector2 + (Vector2.UnitY * (2 + index + Calc.Random.Range(-1, 1))), direction);
                }
            }
            if (moved.Y > 0.0)
            {
                Vector2 vector2 = new(Left, Bottom - 1f);
                for (int index = 0; index < (double)Width; index += 4)
                {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, vector2 + (Vector2.UnitX * (2 + index + Calc.Random.Range(-1, 1))), direction);
                }
            }
            else
            {
                if (moved.Y >= 0.0)
                {
                    return;
                }

                Vector2 vector2 = new(Left, Top);
                for (int index = 0; index < (double)Width; index += 4)
                {
                    level.Particles.Emit(FinalBossMovingBlock.P_Stop, vector2 + (Vector2.UnitX * (2 + index + Calc.Random.Range(-1, 1))), direction);
                }
            }
        }

        private void BreakParticles()
        {
            Vector2 center = Center;
            for (int index1 = 0; index1 < (double)Width; index1 += 4)
            {
                for (int index2 = 0; index2 < (double)Height; index2 += 4)
                {
                    Vector2 position = Position + new Vector2(2 + index1, 2 + index2);
                    SceneAs<Level>().Particles.Emit(FinalBossMovingBlock.P_Break, 1, position, Vector2.One * 2f, (position - center).Angle());
                }
            }
        }

        private void ImpactParticles(Vector2 moved)
        {
            if (moved.X < 0.0)
            {
                Vector2 vector2 = new(0.0f, 2f);
                for (int index = 0; index < (double)Height / 8.0; ++index)
                {
                    Vector2 point = new(Left - 1f, Top + 4f + (index * 8));
                    if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 0.0f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 0.0f);
                    }
                }
            }
            else if (moved.X > 0.0)
            {
                Vector2 vector2 = new(0.0f, 2f);
                for (int index = 0; index < (double)Height / 8.0; ++index)
                {
                    Vector2 point = new(Right + 1f, Top + 4f + (index * 8));
                    if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 3.14159274f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 3.14159274f);
                    }
                }
            }
            if (moved.Y < 0.0)
            {
                Vector2 vector2 = new(2f, 0.0f);
                for (int index = 0; index < (double)Width / 8.0; ++index)
                {
                    Vector2 point = new(Left + 4f + (index * 8), Top - 1f);
                    if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, 1.57079637f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, 1.57079637f);
                    }
                }
            }
            else
            {
                if (moved.Y <= 0.0)
                {
                    return;
                }

                Vector2 vector2 = new(2f, 0.0f);
                for (int index = 0; index < (double)Width / 8.0; ++index)
                {
                    Vector2 point = new(Left + 4f + (index * 8), Bottom + 1f);
                    if (!Scene.CollideCheck<Water>(point) && Scene.CollideCheck<Solid>(point))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point + vector2, -1.57079637f);
                        SceneAs<Level>().ParticlesFG.Emit(CrushBlock.P_Impact, point - vector2, -1.57079637f);
                    }
                }
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            base.Render();
            if ((double)highlight.Alpha is > 0.0 and < 1.0)
            {
                int num = (int)((1.0 - highlight.Alpha) * 16.0);
                Rectangle rect = new((int)X, (int)Y, (int)Width, (int)Height);
                rect.Inflate(num, num);
                Draw.HollowRect(rect, Color.Lerp(Color.Purple, Color.Pink, 0.7f));
            }
            Position = position;
        }

        private void Finish()
        {
            Vector2 from = CenterRight + (Vector2.UnitX * 10f);
            for (int index1 = 0; index1 < (double)Width / 8.0; ++index1)
            {
                for (int index2 = 0; index2 < (double)Height / 8.0; ++index2)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2(4 + (index1 * 8), 4 + (index2 * 8)), 'f').BlastFrom(from));
                }
            }
            BreakParticles();
            DestroyStaticMovers();
            RemoveSelf();
        }

        public void Destroy(float delay)
        {
            if (Scene == null)
            {
                return;
            }

            if (moveCoroutine != null)
            {
                Remove(moveCoroutine);
            }

            if ((double)delay <= 0.0)
            {
                Finish();
            }
            else
            {
                StartShaking(delay);
                _ = Alarm.Set(this, delay, new Action(Finish));
            }
        }
    }
}
