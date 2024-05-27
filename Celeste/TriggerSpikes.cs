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
        private Directions direction;
        private Vector2 outwards;
        private Vector2 offset;
        private PlayerCollider pc;
        private Vector2 shakeOffset;
        private SpikeInfo[] spikes;
        private List<MTexture> dustTextures;
        private List<MTexture> tentacleTextures;
        private Color[] tentacleColors;
        private int size;

        public TriggerSpikes(Vector2 position, int size, Directions direction)
            : base(position)
        {
            this.size = size;
            this.direction = direction;
            switch (direction)
            {
                case Directions.Up:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
                    outwards = new Vector2(0.0f, -1f);
                    offset = new Vector2(0.0f, -1f);
                    Collider = new Hitbox(size, 4f, y: -4f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(UpSafeBlockCheck));
                    break;
                case Directions.Down:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_v");
                    outwards = new Vector2(0.0f, 1f);
                    Collider = new Hitbox(size, 4f);
                    break;
                case Directions.Left:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
                    outwards = new Vector2(-1f, 0.0f);
                    Collider = new Hitbox(4f, size, -4f);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(SideSafeBlockCheck));
                    break;
                case Directions.Right:
                    tentacleTextures = GFX.Game.GetAtlasSubtextures("danger/triggertentacle/wiggle_h");
                    outwards = new Vector2(1f, 0.0f);
                    offset = new Vector2(1f, 0.0f);
                    Collider = new Hitbox(4f, size);
                    Add(new SafeGroundBlocker());
                    Add(new LedgeBlocker(SideSafeBlockCheck));
                    break;
            }
            Add(pc = new PlayerCollider(OnCollide));
            Add(new StaticMover
            {
                OnShake = OnShake,
                SolidChecker = IsRiding,
                JumpThruChecker = IsRiding
            });
            Add(new DustEdge(RenderSpikes));
            Depth = -50;
        }

        public TriggerSpikes(EntityData data, Vector2 offset, Directions dir)
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
                tentacleColors[index] = Color.Lerp(new Color(edgeColors[index]), Color.DarkSlateBlue, 0.4f);
            Vector2 vector2 = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            spikes = new SpikeInfo[size / 4];
            for (int index = 0; index < spikes.Length; ++index)
            {
                spikes[index].Parent = this;
                spikes[index].Index = index;
                spikes[index].WorldPosition = Position + vector2 * (2 + index * 4);
                spikes[index].ParticleTimerOffset = Calc.Random.NextFloat(0.25f);
                spikes[index].TextureIndex = Calc.Random.Next(dustTextures.Count);
                spikes[index].DustOutDistance = Calc.Random.Choose(3, 4, 6);
                spikes[index].TentacleColor = Calc.Random.Next(tentacleColors.Length);
                spikes[index].TentacleFrame = Calc.Random.NextFloat(tentacleTextures.Count);
            }
        }

        private void OnShake(Vector2 amount) => shakeOffset += amount;

        private bool UpSafeBlockCheck(Player player)
        {
            int num1 = 8 * (int) player.Facing;
            int val1_1 = (int) ((player.Left + (double) num1 - Left) / 4.0);
            int val1_2 = (int) ((player.Right + (double) num1 - Left) / 4.0);
            if (val1_2 < 0 || val1_1 >= spikes.Length)
                return false;
            int num2 = Math.Max(val1_1, 0);
            int num3 = Math.Min(val1_2, spikes.Length - 1);
            for (int index = num2; index <= num3; ++index)
            {
                if (spikes[index].Lerp >= 1.0)
                    return true;
            }
            return false;
        }

        private bool SideSafeBlockCheck(Player player)
        {
            int val1_1 = (int) ((player.Top - (double) Top) / 4.0);
            int val1_2 = (int) ((player.Bottom - (double) Top) / 4.0);
            if (val1_2 < 0 || val1_1 >= spikes.Length)
                return false;
            int num1 = Math.Max(val1_1, 0);
            int num2 = Math.Min(val1_2, spikes.Length - 1);
            for (int index = num1; index <= num2; ++index)
            {
                if (spikes[index].Lerp >= 1.0)
                    return true;
            }
            return false;
        }

        private void OnCollide(Player player)
        {
            int minIndex;
            int maxIndex;
            GetPlayerCollideIndex(player, out minIndex, out maxIndex);
            if (maxIndex < 0 || minIndex >= spikes.Length)
                return;
            int num1 = Math.Max(minIndex, 0);
            int num2 = Math.Min(maxIndex, spikes.Length - 1);
            int index = num1;
            while (index <= num2 && !spikes[index].OnPlayer(player, outwards))
                ++index;
        }

        private void GetPlayerCollideIndex(Player player, out int minIndex, out int maxIndex)
        {
            minIndex = maxIndex = -1;
            switch (direction)
            {
                case Directions.Up:
                    if (player.Speed.Y < 0.0)
                        break;
                    minIndex = (int) ((player.Left - (double) Left) / 4.0);
                    maxIndex = (int) ((player.Right - (double) Left) / 4.0);
                    break;
                case Directions.Down:
                    if (player.Speed.Y > 0.0)
                        break;
                    minIndex = (int) ((player.Left - (double) Left) / 4.0);
                    maxIndex = (int) ((player.Right - (double) Left) / 4.0);
                    break;
                case Directions.Left:
                    if (player.Speed.X < 0.0)
                        break;
                    minIndex = (int) ((player.Top - (double) Top) / 4.0);
                    maxIndex = (int) ((player.Bottom - (double) Top) / 4.0);
                    break;
                case Directions.Right:
                    if (player.Speed.X > 0.0)
                        break;
                    minIndex = (int) ((player.Top - (double) Top) / 4.0);
                    maxIndex = (int) ((player.Bottom - (double) Top) / 4.0);
                    break;
            }
        }

        private bool PlayerCheck(int spikeIndex)
        {
            Player player = CollideFirst<Player>();
            if (player == null)
                return false;
            int minIndex;
            int maxIndex;
            GetPlayerCollideIndex(player, out minIndex, out maxIndex);
            return minIndex <= spikeIndex + 1 && maxIndex >= spikeIndex - 1;
        }

        private static int GetSize(EntityData data, Directions dir)
        {
            switch (dir)
            {
                case Directions.Up:
                case Directions.Down:
                    return data.Width;
                default:
                    int num = (int) (dir - 2);
                    return data.Height;
            }
        }

        public override void Update()
        {
            base.Update();
            for (int index = 0; index < spikes.Length; ++index)
                spikes[index].Update();
        }

        public override void Render()
        {
            base.Render();
            Vector2 vector2 = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            int count = tentacleTextures.Count;
            Vector2 one = Vector2.One;
            Vector2 justify = new Vector2(0.0f, 0.5f);
            if (direction == Directions.Left)
                one.X = -1f;
            else if (direction == Directions.Up)
                one.Y = -1f;
            if (direction == Directions.Up || direction == Directions.Down)
                justify = new Vector2(0.5f, 0.0f);
            for (int index = 0; index < spikes.Length; ++index)
            {
                if (!spikes[index].Triggered)
                {
                    MTexture tentacleTexture = tentacleTextures[(int) (spikes[index].TentacleFrame % (double) count)];
                    Vector2 position = Position + vector2 * (2 + index * 4);
                    tentacleTexture.DrawJustified(position + vector2, justify, Color.Black, one, 0.0f);
                    tentacleTexture.DrawJustified(position, justify, tentacleColors[spikes[index].TentacleColor], one, 0.0f);
                }
            }
            RenderSpikes();
        }

        private void RenderSpikes()
        {
            Vector2 vector2 = new Vector2(Math.Abs(outwards.Y), Math.Abs(outwards.X));
            for (int index = 0; index < spikes.Length; ++index)
            {
                if (spikes[index].Triggered)
                    dustTextures[spikes[index].TextureIndex].DrawCentered(Position + outwards * (float) (spikes[index].Lerp * (double) spikes[index].DustOutDistance - 4.0) + vector2 * (2 + index * 4), Color.White, 0.5f * spikes[index].Lerp, spikes[index].TextureRotation);
            }
        }

        private bool IsRiding(Solid solid)
        {
            switch (direction)
            {
                case Directions.Up:
                    return CollideCheckOutside(solid, Position + Vector2.UnitY);
                case Directions.Down:
                    return CollideCheckOutside(solid, Position - Vector2.UnitY);
                case Directions.Left:
                    return CollideCheckOutside(solid, Position + Vector2.UnitX);
                case Directions.Right:
                    return CollideCheckOutside(solid, Position - Vector2.UnitX);
                default:
                    return false;
            }
        }

        private bool IsRiding(JumpThru jumpThru) => direction == Directions.Up && CollideCheck(jumpThru, Position + Vector2.UnitY);

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
                                DelayTimer = 0.05f;
                            else
                                Audio.Play("event:/game/03_resort/fluff_tendril_emerge", WorldPosition);
                        }
                    }
                    else
                        Lerp = Calc.Approach(Lerp, 1f, 8f * Engine.DeltaTime);
                    TextureRotation += Engine.DeltaTime * 1.2f;
                }
                else
                {
                    Lerp = Calc.Approach(Lerp, 0.0f, 4f * Engine.DeltaTime);
                    TentacleFrame += Engine.DeltaTime * 12f;
                    if (Lerp > 0.0)
                        return;
                    Triggered = false;
                }
            }

            public bool PlayerCheck() => Parent.PlayerCheck(Index);

            public bool OnPlayer(Player player, Vector2 outwards)
            {
                if (!Triggered)
                {
                    Audio.Play("event:/game/03_resort/fluff_tendril_touch", WorldPosition);
                    Triggered = true;
                    DelayTimer = 0.4f;
                    RetractTimer = 6f;
                }
                else if (Lerp >= 1.0)
                {
                    player.Die(outwards);
                    return true;
                }
                return false;
            }
        }
    }
}
