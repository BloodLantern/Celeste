// Decompiled with JetBrains decompiler
// Type: Celeste.NPC
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
    public class NPC : Entity
    {
        public const string MetTheo = "MetTheo";
        public const string TheoKnowsName = "TheoKnowsName";
        public const float TheoMaxSpeed = 48f;
        public Sprite Sprite;
        public TalkComponent Talker;
        public VertexLight Light;
        public Level Level;
        public SoundSource PhoneTapSfx;
        public float Maxspeed = 80f;
        public string MoveAnim = "";
        public string IdleAnim = "";
        public bool MoveY = true;
        public bool UpdateLight = true;
        private readonly List<Entity> temp = new();

        public Session Session => Level.Session;

        public NPC(Vector2 position)
        {
            Position = position;
            Depth = 1000;
            Collider = new Hitbox(8f, 8f, -4f, -8f);
            Add(new MirrorReflection());
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level = scene as Level;
        }

        public override void Update()
        {
            base.Update();
            if (UpdateLight && Light != null)
            {
                Rectangle bounds = Level.Bounds;
                Light.Alpha = Calc.Approach(Light.Alpha, (double)X <= bounds.Left - 16 || (double)Y <= bounds.Top - 16 || (double)X >= bounds.Right + 16 || (double)Y >= bounds.Bottom + 16 || Level.Transitioning ? 0.0f : 1f, Engine.DeltaTime * 2f);
            }
            if (Sprite != null && Sprite.CurrentAnimationID == "usePhone")
            {
                if (PhoneTapSfx == null)
                {
                    Add(PhoneTapSfx = new SoundSource());
                }

                if (PhoneTapSfx.Playing)
                {
                    return;
                }

                _ = PhoneTapSfx.Play("event:/char/theo/phone_taps_loop");
            }
            else
            {
                if (PhoneTapSfx == null || !PhoneTapSfx.Playing)
                {
                    return;
                }

                _ = PhoneTapSfx.Stop();
            }
        }

        public void SetupTheoSpriteSounds()
        {
            Sprite.OnFrameChange = anim =>
        {
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            if ((anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 6)) || (anim == "run" && (currentAnimationFrame == 0 || currentAnimationFrame == 4)))
            {
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                if (platformByPriority == null)
                {
                    return;
                }

                _ = Audio.Play("event:/char/madeline/footstep", Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
            }
            else if (anim == "crawl" && currentAnimationFrame == 0)
            {
                if (Level.Transitioning)
                {
                    return;
                }

                _ = Audio.Play("event:/char/theo/resort_crawl", Position);
            }
            else
            {
                if (!(anim == "pullVent") || currentAnimationFrame != 0)
                {
                    return;
                }

                _ = Audio.Play("event:/char/theo/resort_vent_tug", Position);
            }
        };
        }

        public void SetupGrannySpriteSounds()
        {
            Sprite.OnFrameChange = anim =>
        {
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            if (anim == "walk" && (currentAnimationFrame == 0 || currentAnimationFrame == 4))
            {
                Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(CollideAll<Platform>(Position + Vector2.UnitY, temp));
                if (platformByPriority == null)
                {
                    return;
                }

                _ = Audio.Play("event:/char/madeline/footstep", Center, "surface_index", platformByPriority.GetStepSoundIndex(this));
            }
            else
            {
                if (!(anim == "walk") || currentAnimationFrame != 2)
                {
                    return;
                }

                _ = Audio.Play("event:/char/granny/cane_tap", Position);
            }
        };
        }

        public IEnumerator PlayerApproachRightSide(
            Player player,
            bool turnToFace = true,
            float? spacing = null)
        {
            yield return PlayerApproach(player, turnToFace, spacing, new int?(1));
        }

        public IEnumerator PlayerApproachLeftSide(
            Player player,
            bool turnToFace = true,
            float? spacing = null)
        {
            yield return PlayerApproach(player, turnToFace, spacing, new int?(-1));
        }

        public IEnumerator PlayerApproach(
            Player player,
            bool turnToFace = true,
            float? spacing = null,
            int? side = null)
        {
            NPC npc = this;
            if (!side.HasValue)
            {
                side = new int?(Math.Sign(player.X - npc.X));
            }

            int? nullable1 = side;
            int num = 0;
            if (nullable1.GetValueOrDefault() == num & nullable1.HasValue)
            {
                side = new int?(1);
            }

            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            if (spacing.HasValue)
            {
                Player player1 = player;
                float x1 = npc.X;
                nullable1 = side;
                float? nullable2 = nullable1.HasValue ? new float?(nullable1.GetValueOrDefault()) : new float?();
                float? nullable3 = spacing;
                float? nullable4 = nullable2.HasValue & nullable3.HasValue ? new float?(nullable2.GetValueOrDefault() * nullable3.GetValueOrDefault()) : new float?();
                float? nullable5;
                if (!nullable4.HasValue)
                {
                    nullable3 = new float?();
                    nullable5 = nullable3;
                }
                else
                {
                    nullable5 = new float?(x1 + nullable4.GetValueOrDefault());
                }

                nullable3 = nullable5;
                int x2 = (int)nullable3.Value;
                yield return player1.DummyWalkToExact(x2);
            }
            else if ((double)Math.Abs(npc.X - player.X) < 12.0 || Math.Sign(player.X - npc.X) != side.Value)
            {
                Player player2 = player;
                float x3 = npc.X;
                nullable1 = side;
                float? nullable6 = nullable1.HasValue ? new float?(nullable1.GetValueOrDefault() * 12) : new float?();
                int x4 = (int)(nullable6.HasValue ? new float?(x3 + nullable6.GetValueOrDefault()) : new float?()).Value;
                yield return player2.DummyWalkToExact(x4);
            }
            player.Facing = (Facings)(-side.Value);
            if (turnToFace && npc.Sprite != null)
            {
                npc.Sprite.Scale.X = side.Value;
            }

            yield return null;
        }

        // ISSUE: reference to a compiler-generated field
        public IEnumerator PlayerApproach48px()
        {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            yield return PlayerApproach(entity, true, new float?(48), null);
            yield break;
        }

        public IEnumerator PlayerLeave(Player player, float? to = null)
        {
            if (to.HasValue)
            {
                yield return player.DummyWalkToExact((int)to.Value);
            }

            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            yield return null;
        }

        public IEnumerator MoveTo(
            Vector2 target,
            bool fadeIn = false,
            int? turnAtEndTo = null,
            bool removeAtEnd = false)
        {
            NPC npc = this;
            if (removeAtEnd)
            {
                npc.Tag |= (int)Tags.TransitionUpdate;
            }

            if (Math.Sign(target.X - npc.X) != 0 && npc.Sprite != null)
            {
                npc.Sprite.Scale.X = Math.Sign(target.X - npc.X);
            }
            _ = (target - npc.Position).SafeNormalize();
            float alpha = fadeIn ? 0.0f : 1f;
            if (npc.Sprite != null && npc.Sprite.Has(npc.MoveAnim))
            {
                npc.Sprite.Play(npc.MoveAnim);
            }

            float speed = 0.0f;
            while ((npc.MoveY && npc.Position != target) || (!npc.MoveY && (double)npc.X != target.X))
            {
                speed = Calc.Approach(speed, npc.Maxspeed, 160f * Engine.DeltaTime);
                if (npc.MoveY)
                {
                    npc.Position = Calc.Approach(npc.Position, target, speed * Engine.DeltaTime);
                }
                else
                {
                    npc.X = Calc.Approach(npc.X, target.X, speed * Engine.DeltaTime);
                }

                if (npc.Sprite != null)
                {
                    npc.Sprite.Color = Color.White * alpha;
                }

                alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime);
                yield return null;
            }
            if (npc.Sprite != null && npc.Sprite.Has(npc.IdleAnim))
            {
                npc.Sprite.Play(npc.IdleAnim);
            }

            while ((double)alpha < 1.0)
            {
                if (npc.Sprite != null)
                {
                    npc.Sprite.Color = Color.White * alpha;
                }

                alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime);
                yield return null;
            }
            if (turnAtEndTo.HasValue && npc.Sprite != null)
            {
                npc.Sprite.Scale.X = turnAtEndTo.Value;
            }

            if (removeAtEnd)
            {
                npc.Scene.Remove(npc);
            }

            yield return null;
        }

        public void MoveToAndRemove(Vector2 target)
        {
            Add(new Coroutine(MoveTo(target, removeAtEnd: true)));
        }
    }
}
