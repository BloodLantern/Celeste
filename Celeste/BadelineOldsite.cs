// Decompiled with JetBrains decompiler
// Type: Celeste.BadelineOldsite
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
    [Tracked(false)]
    public class BadelineOldsite : Entity
    {
        public static ParticleType P_Vanish;
        public static readonly Color HairColor = Calc.HexToColor("9B3FB5");
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        private LightOcclude occlude;
        private bool ignorePlayerAnim;
        private readonly int index;
        private Player player;
        private bool following;
        private readonly float followBehindTime;
        private readonly float followBehindIndexDelay;
        public bool Hovering;
        private float hoveringTimer;
        private readonly Dictionary<string, SoundSource> loopingSounds = new();
        private readonly List<SoundSource> inactiveLoopingSounds = new();

        public BadelineOldsite(Vector2 position, int index)
            : base(position)
        {
            this.index = index;
            Depth = -1;
            Collider = new Hitbox(6f, 6f, -3f, -7f);
            Collidable = false;
            Sprite = new PlayerSprite(PlayerSpriteMode.Badeline);
            Sprite.Play("fallSlow", true);
            Hair = new PlayerHair(Sprite)
            {
                Color = Color.Lerp(BadelineOldsite.HairColor, Color.White, index / 6f),
                Border = Color.Black
            };
            Add(Hair);
            Add(Sprite);
            Visible = false;
            followBehindTime = 1.55f;
            followBehindIndexDelay = 0.4f * index;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
        }

        public BadelineOldsite(EntityData data, Vector2 offset, int index)
            : this(data.Position + offset, index)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = SceneAs<Level>().Session;
            if (session.GetLevelFlag("11") && session.Area.Mode == AreaMode.Normal)
            {
                RemoveSelf();
            }
            else if (!session.GetLevelFlag("3") && session.Area.Mode == AreaMode.Normal)
            {
                RemoveSelf();
            }
            else if (!session.GetFlag("evil_maddy_intro") && session.Level == "3" && session.Area.Mode == AreaMode.Normal)
            {
                Hovering = false;
                Visible = true;
                Hair.Visible = false;
                Sprite.Play("pretendDead");
                if (session.Area.Mode == AreaMode.Normal)
                {
                    session.Audio.Music.Event = null;
                    session.Audio.Apply();
                }
                Scene.Add(new CS02_BadelineIntro(this));
            }
            else
            {
                Add(new Coroutine(StartChasingRoutine(Scene as Level)));
            }
        }

        public IEnumerator StartChasingRoutine(Level level)
        {
            BadelineOldsite badelineOldsite = this;
            badelineOldsite.Hovering = true;
            while ((badelineOldsite.player = badelineOldsite.Scene.Tracker.GetEntity<Player>()) == null || badelineOldsite.player.JustRespawned)
            {
                yield return null;
            }

            Vector2 to = badelineOldsite.player.Position;
            yield return badelineOldsite.followBehindIndexDelay;
            if (!badelineOldsite.Visible)
            {
                badelineOldsite.PopIntoExistance(0.5f);
            }

            badelineOldsite.Sprite.Play("fallSlow");
            badelineOldsite.Hair.Visible = true;
            badelineOldsite.Hovering = false;
            if (level.Session.Area.Mode == AreaMode.Normal)
            {
                level.Session.Audio.Music.Event = "event:/music/lvl2/chase";
                level.Session.Audio.Apply();
            }
            yield return badelineOldsite.TweenToPlayer(to);
            badelineOldsite.Collidable = true;
            badelineOldsite.following = true;
            badelineOldsite.Add(badelineOldsite.occlude = new LightOcclude());
            if (level.Session.Level == "2")
            {
                badelineOldsite.Add(new Coroutine(badelineOldsite.StopChasing()));
            }
        }

        private IEnumerator TweenToPlayer(Vector2 to)
        {
            BadelineOldsite badelineOldsite = this;
            _ = Audio.Play("event:/char/badeline/level_entry", badelineOldsite.Position, "chaser_count", badelineOldsite.index);
            Vector2 from = badelineOldsite.Position;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, badelineOldsite.followBehindTime - 0.1f, true);
            tween.OnUpdate = delegate (Tween t)
            {
                badelineOldsite.Position = Vector2.Lerp(from, to, t.Eased);
                if (to.X != from.X)
                {
                    badelineOldsite.Sprite.Scale.X = Math.Abs(badelineOldsite.Sprite.Scale.X) * Math.Sign(to.X - from.X);
                }
                badelineOldsite.Trail();
            };
            base.Add(tween);
            yield return tween.Duration;
            yield break;
        }

        private IEnumerator StopChasing()
        {
            BadelineOldsite badelineOldsite = this;
            Level level = badelineOldsite.SceneAs<Level>();
            int boundsRight = level.Bounds.X + 148;
            int boundsBottom = level.Bounds.Y + 168 + 184;
            while ((double)badelineOldsite.X != boundsRight || (double)badelineOldsite.Y != boundsBottom)
            {
                yield return null;
                if ((double)badelineOldsite.X > boundsRight)
                {
                    badelineOldsite.X = boundsRight;
                }

                if ((double)badelineOldsite.Y > boundsBottom)
                {
                    badelineOldsite.Y = boundsBottom;
                }
            }
            badelineOldsite.following = false;
            badelineOldsite.ignorePlayerAnim = true;
            badelineOldsite.Sprite.Play("laugh");
            badelineOldsite.Sprite.Scale.X = 1f;
            yield return 1f;
            _ = Audio.Play("event:/char/badeline/disappear", badelineOldsite.Position);
            _ = level.Displacement.AddBurst(badelineOldsite.Center, 0.5f, 24f, 96f, 0.4f);
            level.Particles.Emit(P_Vanish, 12, badelineOldsite.Center, Vector2.One * 6f);
            badelineOldsite.RemoveSelf();
        }

        public override void Update()
        {
            if (player != null && player.Dead)
            {
                Sprite.Play("laugh");
                Sprite.X = (float)Math.Sin(hoveringTimer) * 4;
                Hovering = true;
                hoveringTimer += Engine.DeltaTime * 2f;
                Depth = -12500;
                foreach (KeyValuePair<string, SoundSource> loopingSound in loopingSounds)
                {
                    _ = loopingSound.Value.Stop();
                }

                Trail();
            }
            else
                if (following && player.GetChasePosition(Scene.TimeActive, followBehindTime + followBehindIndexDelay, out Player.ChaserState chaseState))
            {
                Position = Calc.Approach(Position, chaseState.Position, 500f * Engine.DeltaTime);
                if (!ignorePlayerAnim && chaseState.Animation != Sprite.CurrentAnimationID && chaseState.Animation != null && Sprite.Has(chaseState.Animation))
                {
                    Sprite.Play(chaseState.Animation, true);
                }

                if (!ignorePlayerAnim)
                {
                    Sprite.Scale.X = Math.Abs(Sprite.Scale.X) * (float)chaseState.Facing;
                }

                for (int index = 0; index < chaseState.Sounds; ++index)
                {
                    if (chaseState[index].Action == Player.ChaserStateSound.Actions.Oneshot)
                    {
                        _ = Audio.Play(chaseState[index].Event, Position, chaseState[index].Parameter, chaseState[index].ParameterValue, "chaser_count", this.index);
                    }
                    else if (chaseState[index].Action == Player.ChaserStateSound.Actions.Loop && !loopingSounds.ContainsKey(chaseState[index].Event))
                    {
                        SoundSource soundSource;
                        if (inactiveLoopingSounds.Count > 0)
                        {
                            soundSource = inactiveLoopingSounds[0];
                            inactiveLoopingSounds.RemoveAt(0);
                        }
                        else
                        {
                            Add(soundSource = new SoundSource());
                        }

                        _ = soundSource.Play(chaseState[index].Event, "chaser_count", this.index);
                        loopingSounds.Add(chaseState[index].Event, soundSource);
                    }
                    else if (chaseState[index].Action == Player.ChaserStateSound.Actions.Stop)
                    {
                        if (loopingSounds.TryGetValue(chaseState[index].Event, out SoundSource soundSource))
                        {
                            _ = soundSource.Stop();
                            _ = loopingSounds.Remove(chaseState[index].Event);
                            inactiveLoopingSounds.Add(soundSource);
                        }
                    }
                }
                Depth = chaseState.Depth;
                Trail();
            }
            if (Sprite.Scale.X != 0.0)
            {
                Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
            }

            if (Hovering)
            {
                hoveringTimer += Engine.DeltaTime;
                Sprite.Y = (float)Math.Sin(hoveringTimer * 2) * 4;
            }
            else
            {
                Sprite.Y = Calc.Approach(Sprite.Y, 0.0f, Engine.DeltaTime * 4f);
            }

            if (occlude != null)
            {
                occlude.Visible = !CollideCheck<Solid>();
            }

            base.Update();
        }

        private void Trail()
        {
            if (!Scene.OnInterval(0.1f))
            {
                return;
            }

            TrailManager.Add(this, Player.NormalHairColor);
        }

        private void OnPlayer(Player player)
        {
            _ = player.Die((player.Position - Position).SafeNormalize());
        }

        /*private void Die()
        {
            RemoveSelf();
        }*/

        private void PopIntoExistance(float duration)
        {
            Visible = true;
            Sprite.Scale = Vector2.Zero;
            Sprite.Color = Color.Transparent;
            Hair.Visible = true;
            Hair.Alpha = 0.0f;
            Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.CubeIn, duration, true);
            tween.OnUpdate = t =>
            {
                Sprite.Scale = Vector2.One * t.Eased;
                Sprite.Color = Color.White * t.Eased;
                Hair.Alpha = t.Eased;
            };
            Add(tween);
        }

        /*private bool OnGround(int dist = 1)
        {
            for (int y = 1; y <= dist; ++y)
            {
                if (CollideCheck<Solid>(Position + new Vector2(0.0f, y)))
                {
                    return true;
                }
            }
            return false;
        }*/
    }
}
