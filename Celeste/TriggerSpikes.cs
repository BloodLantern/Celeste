// Decompiled with JetBrains decompiler
// Type: Celeste.TriggerSpikes
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class TriggerSpikes : Entity
    {
        private const float RetractTime = 6f;
        private const float DelayTime = 0.4f;
        private readonly TriggerSpikes.Directions direction;
        private Vector2 outwards;
        private Vector2 offset;
        private readonly PlayerCollider pc;
        private Vector2 shakeOffset;
        private TriggerSpikes.SpikeInfo[] spikes;
        private List<MTexture> dustTextures;
        private readonly List<MTexture> tentacleTextures;
        private Color[] tentacleColors;
        private readonly int size;

        public TriggerSpikes(Vector2 position, int size, TriggerSpikes.Directions direction)
            : base(position)
        {
            this.size = size;
            this.direction = direction;
            switch (direction)
            {
                case TriggerSpikes.Directions.Up:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
                    outwards = new Vector2(0.0f, -1f);
                    offset = new Vector2(0.0f, -1f);
                    Collider = new Hitbox(size, 4f, y: -4f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(new Func<Player, bool>(UpSafeBlockCheck)));
                    break;
                case TriggerSpikes.Directions.Down:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
                    outwards = new Vector2(0.0f, 1f);
                    Collider = new Hitbox(size, 4f);
                    break;
                case TriggerSpikes.Directions.Left:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
                    outwards = new Vector2(-1f, 0.0f);
                    Collider = new Hitbox(4f, size, -4f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(new Func<Player, bool>(SideSafeBlockCheck)));
                    break;
                case TriggerSpikes.Directions.Right:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
                    outwards = new Vector2(1f, 0.0f);
                    offset = new Vector2(1f, 0.0f);
                    Collider = new Hitbox(4f, size);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(new Func<Player, bool>(SideSafeBlockCheck)));
                    break;
            }
            Add(pc = new PlayerCollider(new Action<Player>(OnCollide)));
            Add(new StaticMover()
            {
                OnShake = new Action<Vector2>(OnShake),
                SolidChecker = new Func<Solid, bool>(IsRiding),
                JumpThruChecker = new Func<JumpThru, bool>(IsRiding)
            });
            Add(new DustEdge(new Action(RenderSpikes)));
            Depth = -50;
        }

        public TriggerSpikes(EntityData data, Vector2 offset, TriggerSpikes.Directions dir)
            : this(data.Position + offset, TriggerSpikes.GetSize(data, dir), dir)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Vector3[] edgeColors = DustStyles.Get(scene).EdgeColors;
            dustTextures = GFX.Game.GetAtlasSubtextures("danger/dustcreature/base");
            tentacleColors = new Color[edgeColors.Length];
            for (int index = 0; index < tentacleColors.Length; ++index)
            {
                tentacleColors[index] = Color.Lerp(new Color(edgeColors[index]), Color.DarkSlateBlue, 0.4f);
            }

            Vector2 vector2 = new(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            spikes = new TriggerSpikes.SpikeInfo[size / 4];
            for (int index = 0; index < spikes.Length; ++index)
            {
                spikes[index].Parent = this;
                spikes[index].Index = index;
                spikes[index].WorldPosition = Position + (vector2 * (2 + (index * 4)));
                spikes[index].ParticleTimerOffset = Calc.Random.NextFloat(0.25f);
                spikes[index].TextureIndex = Calc.Random.Next(dustTextures.Count);
                spikes[index].DustOutDistance = Calc.Random.Choose<int>(3, 4, 6);
                spikes[index].TentacleColor = Calc.Random.Next(tentacleColors.Length);
                spikes[index].TentacleFrame = Calc.Random.NextFloat(tentacleTextures.Count);
            }
        }

        private void OnShake(Vector2 amount)
        {
            shakeOffset += amount;
        }

        private bool UpSafeBlockCheck(Player player)
        {
            int num1 = 8 * (int)player.Facing;
            int val1_1 = (int)(((double)player.Left + num1 - (double)Left) / 4.0);
            int val1_2 = (int)(((double)player.Right + num1 - (double)Left) / 4.0);
            if (val1_2 < 0 || val1_1 >= spikes.Length)
            {
                return false;
            }

            int num2 = Math.Max(val1_1, 0);
            int num3 = Math.Min(val1_2, spikes.Length - 1);
            for (int index = num2; index <= num3; ++index)
            {
                if (spikes[index].Lerp >= 1.0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool SideSafeBlockCheck(Player player)
        {
            int val1_1 = (int)(((double)player.Top - (double)Top) / 4.0);
            int val1_2 = (int)(((double)player.Bottom - (double)Top) / 4.0);
            if (val1_2 < 0 || val1_1 >= spikes.Length)
            {
                return false;
            }

            int num1 = Math.Max(val1_1, 0);
            int num2 = Math.Min(val1_2, spikes.Length - 1);
            for (int index = num1; index <= num2; ++index)
            {
                if (spikes[index].Lerp >= 1.0)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnCollide(Player player)
        {
            GetPlayerCollideIndex(player, out int minIndex, out int maxIndex);
            if (maxIndex < 0 || minIndex >= spikes.Length)
            {
                return;
            }

            int num1 = Math.Max(minIndex, 0);
            int num2 = Math.Min(maxIndex, spikes.Length - 1);
            int index = num1;
            while (index <= num2 && !spikes[index].OnPlayer(player, outwards))
            {
                ++index;
            }
        }

        private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex)
        {
            minIndex = maxIndex = -1;
            switch (direction)
            {
                case TriggerSpikes.Directions.Up:
                    if (player.Speed.Y < 0.0)
                    {
                        break;
                    }

                    minIndex = (int)(((double)player.Left - (double)Left) / 4.0);
                    maxIndex = (int)(((double)player.Right - (double)Left) / 4.0);
                    break;
                case TriggerSpikes.Directions.Down:
                    if (player.Speed.Y > 0.0)
                    {
                        break;
                    }

                    minIndex = (int)(((double)player.Left - (double)Left) / 4.0);
                    maxIndex = (int)(((double)player.Right - (double)Left) / 4.0);
                    break;
                case TriggerSpikes.Directions.Left:
                    if (player.Speed.X < 0.0)
                    {
                        break;
                    }

                    minIndex = (int)(((double)player.Top - (double)Top) / 4.0);
                    maxIndex = (int)(((double)player.Bottom - (double)Top) / 4.0);
                    break;
                case TriggerSpikes.Directions.Right:
                    if (player.Speed.X > 0.0)
                    {
                        break;
                    }

                    minIndex = (int)(((double)player.Top - (double)Top) / 4.0);
                    maxIndex = (int)(((double)player.Bottom - (double)Top) / 4.0);
                    break;
            }
        }

        private bool PlayerCheck(int spikeIndex)
        {
            Player player = CollideFirst<Player>();
            if (player == null)
            {
                return false;
            }

            GetPlayerCollideIndex(player, out int minIndex, out int maxIndex);
            return minIndex <= spikeIndex + 1 && maxIndex >= spikeIndex - 1;
        }

        private static int GetSize(EntityData data, TriggerSpikes.Directions dir)
        {
            switch (dir)
            {
                case TriggerSpikes.Directions.Up:
                case TriggerSpikes.Directions.Down:
                    return data.Width;
                default:
                    _ = (int)(dir - 2);
                    return data.Height;
            }
        }

        public override void Update()
        {
            base.Update();
            for (int index = 0; index < spikes.Length; ++index)
            {
                spikes[index].Update();
            }
        }

        public override void Render()
        {
            base.Render();
            Vector2 vector2 = new(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            int count = tentacleTextures.Count;
            Vector2 one = Vector2.One;
            Vector2 justify = new(0.0f, 0.5f);
            if (direction == TriggerSpikes.Directions.Left)
            {
                one.X = -1f;
            }
            else if (direction == TriggerSpikes.Directions.Up)
            {
                one.Y = -1f;
            }

            if (direction is TriggerSpikes.Directions.Up or TriggerSpikes.Directions.Down)
            {
                justify = new Vector2(0.5f, 0.0f);
            }

            for (int index = 0; index < spikes.Length; ++index)
            {
                if (!spikes[index].Triggered)
                {
                    MTexture tentacleTexture = tentacleTextures[(int)(spikes[index].TentacleFrame % (double)count)];
                    Vector2 position = Position + (vector2 * (2 + (index * 4)));
                    tentacleTexture.DrawJustified(position + vector2, justify, Color.Black, one, 0.0f);
                    tentacleTexture.DrawJustified(position, justify, tentacleColors[spikes[index].TentacleColor], one, 0.0f);
                }
            }
            RenderSpikes();
        }

        private void RenderSpikes()
        {
            Vector2 vector2 = new(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            for (int index = 0; index < spikes.Length; ++index)
            {
                if (spikes[index].Triggered)
                {
                    dustTextures[spikes[index].TextureIndex].DrawCentered(Position + (outwards * (float)((spikes[index].Lerp * (double)spikes[index].DustOutDistance) - 4.0)) + (vector2 * (2 + (index * 4))), Color.White, 0.5f * spikes[index].Lerp, spikes[index].TextureRotation);
                }
            }
        }

        private bool IsRiding(Solid solid)
        {
            return direction switch
            {
                TriggerSpikes.Directions.Up => CollideCheckOutside(solid, Position + Vector2.UnitY),
                TriggerSpikes.Directions.Down => CollideCheckOutside(solid, Position - Vector2.UnitY),
                TriggerSpikes.Directions.Left => CollideCheckOutside(solid, Position + Vector2.UnitX),
                TriggerSpikes.Directions.Right => CollideCheckOutside(solid, Position - Vector2.UnitX),
                _ => false,
            };
        }

        private bool IsRiding(JumpThru jumpThru)
        {
            return direction == TriggerSpikes.Directions.Up && CollideCheck(jumpThru, Position + Vector2.UnitY);
        }

        public enum Directions
        {
            Up,
            Down,
            Left,
            Right,
        }

        private struct SpikeInfo
        {
            public TriggerSpikes Parent;
            public int Index;
            public Vector2 WorldPosition;
            public bool Triggered;
            public float RetractTimer;
            public float DelayTimer;
            public float Lerp;
            public float ParticleTimerOffset;
            public int TextureIndex;
            public float TextureRotation;
            public int DustOutDistance;
            public int TentacleColor;
            public float TentacleFrame;

            public void Update()
            {
                if (Triggered)
                {
                    if (DelayTimer > 0.0)
                    {
                        DelayTimer -= Engine.DeltaTime;
                        if (DelayTimer <= 0.0)
                        {
                            if (PlayerCheck())
                            {
                                DelayTimer = 0.05f;
                            }
                            else
                            {
                                _ = Audio.Play("event:/game/03_resort/fluff_tendril_emerge", WorldPosition);
                            }
                        }
                    }
                    else
                    {
                        Lerp = Calc.Approach(Lerp, 1f, 8f * Engine.DeltaTime);
                    }

                    TextureRotation += Engine.DeltaTime * 1.2f;
                }
                else
                {
                    Lerp = Calc.Approach(Lerp, 0.0f, 4f * Engine.DeltaTime);
                    TentacleFrame += Engine.DeltaTime * 12f;
                    if (Lerp > 0.0)
                    {
                        return;
                    }

                    Triggered = false;
                }
            }

            public bool PlayerCheck()
            {
                return Parent.PlayerCheck(Index);
            }

            public bool OnPlayer(Player player, Vector2 outwards)
            {
                if (!Triggered)
                {
                    _ = Audio.Play("event:/game/03_resort/fluff_tendril_touch", WorldPosition);
                    Triggered = true;
                    DelayTimer = 0.4f;
                    RetractTimer = 6f;
                }
                else if (Lerp >= 1.0)
                {
                    _ = player.Die(outwards);
                    return true;
                }
                return false;
            }
        }
    }
}
