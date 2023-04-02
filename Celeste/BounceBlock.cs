// Decompiled with JetBrains decompiler
// Type: Celeste.BounceBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    /// <summary>
    /// Core block
    /// </summary>
    public class BounceBlock : Solid
    {
        public static ParticleType P_Reform;
        public static ParticleType P_FireBreak;
        public static ParticleType P_IceBreak;
        private const float WindUpDelay = 0f;
        private const float WindUpDist = 10f;
        private const float IceWindUpDist = 16f;
        private const float BounceDist = 24f;
        private const float LiftSpeedXMult = 0.75f;
        private const float RespawnTime = 1.6f;
        private const float WallPushTime = 0.1f;
        private const float BounceEndTime = 0.05f;
        private Vector2 bounceDir;
        private States state;
        private Vector2 startPos;
        private float moveSpeed;
        private float windUpStartTimer;
        private float windUpProgress;
        private bool iceMode;
        private bool iceModeNext;
        private float respawnTimer;
        private float bounceEndTimer;
        private Vector2 bounceLift;
        private float reappearFlash;
        private bool reformed = true;
        private Vector2 debrisDirection;
        private readonly List<Image> hotImages;
        private readonly List<Image> coldImages;
        private readonly Sprite hotCenterSprite;
        private readonly Sprite coldCenterSprite;

        public BounceBlock(Vector2 position, float width, float height)
            : base(position, width, height, false)
        {
            state = States.Waiting;
            startPos = Position;
            hotImages = BuildSprite(GFX.Game["objects/bumpblocknew/fire00"]);
            hotCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterFire");
            hotCenterSprite.Position = new Vector2(Width, Height) / 2f;
            hotCenterSprite.Visible = false;
            Add(hotCenterSprite);
            coldImages = BuildSprite(GFX.Game["objects/bumpblocknew/ice00"]);
            coldCenterSprite = GFX.SpriteBank.Create("bumpBlockCenterIce");
            coldCenterSprite.Position = new Vector2(Width, Height) / 2f;
            coldCenterSprite.Visible = false;
            Add(coldCenterSprite);
            Add(new CoreModeListener(new Action<Session.CoreModes>(OnChangeMode)));
        }

        public BounceBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        private List<Image> BuildSprite(MTexture source)
        {
            List<Image> imageList = new();
            int num1 = source.Width / 8;
            int num2 = source.Height / 8;
            for (int x = 0; x < Width; x += 8)
                for (int y = 0; y < Height; y += 8)
                {
                    int num3 = x != 0 ? (x < Width - 8 ? Calc.Random.Next(1, num1 - 1) : num1 - 1) : 0;
                    int num4 = y != 0 ? (y < Height - 8 ? Calc.Random.Next(1, num2 - 1) : num2 - 1) : 0;
                    Image image = new(source.GetSubtexture(num3 * 8, num4 * 8, 8, 8))
                    {
                        Position = new Vector2(x, y)
                    };
                    imageList.Add(image);
                    Add(image);
                }
            return imageList;
        }

        private void ToggleSprite()
        {
            hotCenterSprite.Visible = !iceMode;
            coldCenterSprite.Visible = iceMode;
            foreach (Component hotImage in hotImages)
                hotImage.Visible = !iceMode;

            foreach (Component coldImage in coldImages)
                coldImage.Visible = iceMode;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            iceModeNext = iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold;
            ToggleSprite();
        }

        private void OnChangeMode(Session.CoreModes coreMode)
        {
            iceModeNext = coreMode == Session.CoreModes.Cold;
        }

        private void CheckModeChange()
        {
            if (iceModeNext == iceMode)
                return;

            iceMode = iceModeNext;
            ToggleSprite();
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            if (state != States.Broken && reformed)
                base.Render();

            if (reappearFlash > 0)
            {
                float num1 = Ease.CubeOut(reappearFlash);
                float num2 = num1 * 2f;
                Draw.Rect(X - num2, Y - num2, Width + (num2 * 2f), Height + (num2 * 2f), Color.White * num1);
            }
            Position = position;
        }

        public override void Update()
        {
            base.Update();
            reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);
            if (state == States.Waiting)
            {
                CheckModeChange();
                moveSpeed = Calc.Approach(moveSpeed, 100f, 400f * Engine.DeltaTime);
                Vector2 position = Calc.Approach(ExactPosition, startPos, moveSpeed * Engine.DeltaTime);
                Vector2 liftSpeed = (position - ExactPosition).SafeNormalize(moveSpeed);
                liftSpeed.X *= LiftSpeedXMult;
                MoveTo(position, liftSpeed);
                windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
                Player player = WindUpPlayerCheck();
                if (player == null)
                    return;

                moveSpeed = 80f;
                windUpStartTimer = 0f;
                bounceDir = !iceMode ? (player.Center - Center).SafeNormalize() : -Vector2.UnitY;
                state = States.WindingUp;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (iceMode)
                {
                    StartShaking(0.2f);
                    _ = Audio.Play("event:/game/09_core/iceblock_touch", Center);
                }
                else
                    _ = Audio.Play("event:/game/09_core/bounceblock_touch", Center);
            }
            else if (state == States.WindingUp)
            {
                Player player = WindUpPlayerCheck();
                if (player != null)
                    bounceDir = !iceMode ? (player.Center - Center).SafeNormalize() : -Vector2.UnitY;

                if (windUpStartTimer > WindUpDelay)
                {
                    windUpStartTimer -= Engine.DeltaTime;
                    windUpProgress = Calc.Approach(windUpProgress, 0f, 1f * Engine.DeltaTime);
                }
                else
                {
                    moveSpeed = Calc.Approach(moveSpeed, iceMode ? 35f : 40f, 600f * Engine.DeltaTime);
                    float num = iceMode ? 1 / 3f : 1f;
                    Vector2 target = startPos - (bounceDir * (iceMode ? IceWindUpDist : WindUpDist));
                    Vector2 position = Calc.Approach(ExactPosition, target, moveSpeed * num * Engine.DeltaTime);
                    Vector2 liftSpeed = (position - ExactPosition).SafeNormalize(moveSpeed * num);
                    liftSpeed.X *= LiftSpeedXMult;
                    MoveTo(position, liftSpeed);
                    windUpProgress = Calc.ClampedMap(Vector2.Distance(ExactPosition, target), IceWindUpDist, 2f);
                    if (iceMode && Vector2.DistanceSquared(ExactPosition, target) <= 12)
                        StartShaking(WallPushTime);
                    else if (!iceMode && windUpProgress >= 0.5f)
                        StartShaking(WallPushTime);

                    if ((double)Vector2.DistanceSquared(ExactPosition, target) > 2)
                        return;

                    if (iceMode)
                        Break();
                    else
                        state = States.Bouncing;

                    moveSpeed = 0f;
                }
            }
            else if (state == States.Bouncing)
            {
                moveSpeed = Calc.Approach(moveSpeed, 140f, 800f * Engine.DeltaTime);
                Vector2 target = startPos + (bounceDir * BounceDist);
                Vector2 position = Calc.Approach(ExactPosition, target, moveSpeed * Engine.DeltaTime);
                bounceLift = (position - ExactPosition).SafeNormalize(Math.Min(moveSpeed * 3f, 200f));
                bounceLift.X *= LiftSpeedXMult;
                MoveTo(position, bounceLift);
                windUpProgress = 1f;
                if ((ExactPosition == target ? 1 : (iceMode ? 0 : (WindUpPlayerCheck() == null ? 1 : 0))) == 0)
                    return;

                debrisDirection = (target - startPos).SafeNormalize();
                state = States.BounceEnd;
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                moveSpeed = 0.0f;
                bounceEndTimer = BounceEndTime;
                ShakeOffPlayer(bounceLift);
            }
            else if (state == States.BounceEnd)
            {
                bounceEndTimer -= Engine.DeltaTime;
                if (bounceEndTimer > 0)
                    return;

                Break();
            }
            else
            {
                if (state != States.Broken)
                    return;

                Depth = 8990;
                reformed = false;
                if (respawnTimer > 0)
                    respawnTimer -= Engine.DeltaTime;
                else
                {
                    Vector2 position = Position;
                    Position = startPos;
                    if (!CollideCheck<Actor>() && !CollideCheck<Solid>())
                    {
                        CheckModeChange();
                        _ = Audio.Play(iceMode ? "event:/game/09_core/iceblock_reappear" : "event:/game/09_core/bounceblock_reappear", Center);
                        float duration = 0.35f;
                        for (int index1 = 0; index1 < Width; index1 += 8)
                            for (int index2 = 0; index2 < Height; index2 += 8)
                            {
                                Vector2 to = new(X + index1 + 4, Y + index2 + 4);
                                Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(to + ((to - Center).SafeNormalize() * 12f), to, iceMode, duration));
                            }
                        _ = Alarm.Set(this, duration, () =>
                        {
                            reformed = true;
                            reappearFlash = 0.6f;
                            EnableStaticMovers();
                            ReformParticles();
                        });
                        Depth = -9000;
                        MoveStaticMovers(Position - position);
                        Collidable = true;
                        state = States.Waiting;
                    }
                    else
                        Position = position;
                }
            }
        }

        private void ReformParticles()
        {
            Level level = SceneAs<Level>();
            for (int index = 0; index < Width; index += 4)
            {
                level.Particles.Emit(P_Reform, new Vector2(X + 2f + index + Calc.Random.Range(-1, 1), Y), (float) -Math.PI / 2);
                level.Particles.Emit(P_Reform, new Vector2(X + 2f + index + Calc.Random.Range(-1, 1), Bottom - 1f), (float) -Math.PI / 2);
            }
            for (int index = 0; index < Height; index += 4)
            {
                level.Particles.Emit(P_Reform, new Vector2(X, Y + 2f + index + Calc.Random.Range(-1, 1)), (float) Math.PI);
                level.Particles.Emit(P_Reform, new Vector2(Right - 1f, Y + 2f + index + Calc.Random.Range(-1, 1)), 0.0f);
            }
        }

        private Player WindUpPlayerCheck()
        {
            Player player = CollideFirst<Player>(Position - Vector2.UnitY);
            if (player != null && player.Speed.Y < 0)
                player = null;

            if (player == null)
            {
                player = CollideFirst<Player>(Position + Vector2.UnitX);
                if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Left)
                {
                    player = CollideFirst<Player>(Position - Vector2.UnitX);
                    if (player == null || player.StateMachine.State != 1 || player.Facing != Facings.Right)
                        player = null;
                }
            }
            return player;
        }

        private void ShakeOffPlayer(Vector2 liftSpeed)
        {
            Player player = WindUpPlayerCheck();
            if (player == null)
                return;

            player.StateMachine.State = 0;
            player.Speed = liftSpeed;
            player.StartJumpGraceTime();
        }

        private void Break()
        {
            if (!iceMode)
                _ = Audio.Play("event:/game/09_core/bounceblock_break", Center);

            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            state = States.Broken;
            Collidable = false;
            DisableStaticMovers();
            respawnTimer = RespawnTime;
            Vector2 direction1 = new(0.0f, 1f);
            if (!iceMode)
                direction1 = debrisDirection;

            Vector2 center = Center;
            for (int tileX = 0; tileX < Width; tileX += 8)
                for (int tileY = 0; tileY < Height; tileY += 8)
                {
                    if (iceMode)
                        direction1 = (new Vector2(X + tileX + 4, Y + tileY + 4) - center).SafeNormalize();

                    Scene.Add(Engine.Pooler.Create<BreakDebris>().Init(new Vector2(X + tileX + 4, Y + tileY + 4), direction1, iceMode));
                }
            float num = debrisDirection.Angle();
            Level level = SceneAs<Level>();
            for (int halfTileX = 0; halfTileX < Width; halfTileX += 4)
                for (int halfTileY = 0; halfTileY < Height; halfTileY += 4)
                {
                    Vector2 position = Position + new Vector2(2 + halfTileX, 2 + halfTileY) + Calc.Random.Range(-Vector2.One, Vector2.One);
                    float direction2 = iceMode ? (position - center).Angle() : num;
                    level.Particles.Emit(iceMode ? P_IceBreak : P_FireBreak, position, direction2);
                }
        }

        private enum States
        {
            Waiting,
            WindingUp,
            Bouncing,
            BounceEnd,
            Broken,
        }

        [Pooled]
        private class RespawnDebris : Entity
        {
            private Image sprite;
            private Vector2 from;
            private Vector2 to;
            private float percent;
            private float duration;

            public RespawnDebris Init(
                Vector2 from,
                Vector2 to,
                bool ice,
                float duration)
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? "objects/bumpblocknew/ice_rubble" : "objects/bumpblocknew/fire_rubble");
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                if (sprite == null)
                {
                    Add(sprite = new Image(texture));
                    _ = sprite.CenterOrigin();
                }
                else
                    sprite.Texture = texture;

                Position = this.from = from;
                percent = 0.0f;
                this.to = to;
                this.duration = duration;
                return this;
            }

            public override void Update()
            {
                if (percent > 1)
                    RemoveSelf();
                else
                {
                    percent += Engine.DeltaTime / duration;
                    Position = Vector2.Lerp(from, to, Ease.CubeIn(percent));
                    sprite.Color = Color.White * percent;
                }
            }

            public override void Render()
            {
                sprite.DrawOutline(Color.Black);
                base.Render();
            }
        }

        [Pooled]
        private class BreakDebris : Entity
        {
            private Image sprite;
            private Vector2 speed;
            private float percent;
            private float duration;

            public BreakDebris Init(Vector2 position, Vector2 direction, bool ice)
            {
                List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(ice ? "objects/bumpblocknew/ice_rubble" : "objects/bumpblocknew/fire_rubble");
                MTexture texture = Calc.Random.Choose(atlasSubtextures);
                if (sprite == null)
                {
                    Add(sprite = new Image(texture));
                    _ = sprite.CenterOrigin();
                }
                else
                    sprite.Texture = texture;

                Position = position;
                direction = Calc.AngleToVector(direction.Angle() + Calc.Random.Range(-0.1f, 0.1f), 1f);
                speed = direction * (ice ? Calc.Random.Range(20, 40) : Calc.Random.Range(120, 200));
                percent = 0.0f;
                duration = Calc.Random.Range(2, 3);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (percent >= 1)
                    RemoveSelf();
                else
                {
                    Position += speed * Engine.DeltaTime;
                    speed.X = Calc.Approach(speed.X, 0.0f, 180f * Engine.DeltaTime);
                    speed.Y += 200f * Engine.DeltaTime;
                    percent += Engine.DeltaTime / duration;
                    sprite.Color = Color.White * (1f - percent);
                }
            }

            public override void Render()
            {
                sprite.DrawOutline(Color.Black);
                base.Render();
            }
        }
    }
}
