// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerPlayback
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class PlayerPlayback : Entity
    {
        public Vector2 LastPosition;
        public List<Player.ChaserState> Timeline;
        public PlayerSprite Sprite;
        public PlayerHair Hair;
        private Vector2 start;
        private float loopDelay;
        private float startDelay;
        public float TrimStart;
        public float TrimEnd;
        public readonly float Duration;
        private readonly float rangeMinX = float.MinValue;
        private readonly float rangeMaxX = float.MaxValue;
        private readonly bool ShowTrail;

        public Vector2 DashDirection { get; private set; }

        public float Time { get; private set; }

        public int FrameIndex { get; private set; }

        public int FrameCount => Timeline.Count;

        public PlayerPlayback(EntityData e, Vector2 offset)
            : this(e.Position + offset, PlayerSpriteMode.Playback, PlaybackData.Tutorials[e.Attr("tutorial")])
        {
            if (e.Nodes != null && e.Nodes.Length != 0)
            {
                rangeMinX = X;
                rangeMaxX = X;
                foreach (Vector2 vector2 in e.NodesOffset(offset))
                {
                    rangeMinX = Math.Min(rangeMinX, vector2.X);
                    rangeMaxX = Math.Max(rangeMaxX, vector2.X);
                }
            }
            startDelay = 1f;
        }

        public PlayerPlayback(
            Vector2 start,
            PlayerSpriteMode sprite,
            List<Player.ChaserState> timeline)
        {
            this.start = start;
            Collider = new Hitbox(8f, 11f, -4f, -11f);
            Timeline = timeline;
            Position = start;
            Time = 0.0f;
            FrameIndex = 0;
            Duration = timeline[timeline.Count - 1].TimeStamp;
            TrimStart = 0.0f;
            TrimEnd = Duration;
            Sprite = new PlayerSprite(sprite);
            Add(Hair = new PlayerHair(Sprite));
            Add(Sprite);
            Collider = new Hitbox(8f, 4f, -4f, -4f);
            if (sprite == PlayerSpriteMode.Playback)
            {
                ShowTrail = true;
            }

            Depth = 9008;
            SetFrame(0);
            for (int index = 0; index < 10; ++index)
            {
                Hair.AfterUpdate();
            }

            Visible = false;
            FrameIndex = Timeline.Count;
        }

        private void Restart()
        {
            _ = Audio.Play("event:/new_content/char/tutorial_ghost/appear", Position);
            Visible = true;
            Time = TrimStart;
            FrameIndex = 0;
            loopDelay = 0.25f;
            while ((double)Time > Timeline[FrameIndex].TimeStamp)
            {
                ++FrameIndex;
            }

            SetFrame(FrameIndex);
        }

        public void SetFrame(int index)
        {
            Player.ChaserState chaserState = Timeline[index];
            string currentAnimationId1 = Sprite.CurrentAnimationID;
            bool flag = Scene != null && CollideCheck<Solid>(Position + new Vector2(0.0f, 1f));
            _ = DashDirection;
            Position = start + chaserState.Position;
            if (chaserState.Animation != Sprite.CurrentAnimationID && chaserState.Animation != null && Sprite.Has(chaserState.Animation))
            {
                Sprite.Play(chaserState.Animation, true);
            }

            Sprite.Scale = chaserState.Scale;
            if (Sprite.Scale.X != 0.0)
            {
                Hair.Facing = (Facings)Math.Sign(Sprite.Scale.X);
            }

            Hair.Color = chaserState.HairColor;
            if (Sprite.Mode == PlayerSpriteMode.Playback)
            {
                Sprite.Color = Hair.Color;
            }

            DashDirection = chaserState.DashDirection;
            if (Scene == null)
            {
                return;
            }

            if (!flag && Scene != null && CollideCheck<Solid>(Position + new Vector2(0.0f, 1f)))
            {
                _ = Audio.Play("event:/new_content/char/tutorial_ghost/land", Position);
            }

            if (!(currentAnimationId1 != Sprite.CurrentAnimationID))
            {
                return;
            }

            string currentAnimationId2 = Sprite.CurrentAnimationID;
            int currentAnimationFrame = Sprite.CurrentAnimationFrame;
            if (currentAnimationId2 is "jumpFast" or "jumpSlow")
            {
                _ = Audio.Play("event:/new_content/char/tutorial_ghost/jump", Position);
            }
            else if (currentAnimationId2 == "dreamDashIn")
            {
                _ = Audio.Play("event:/new_content/char/tutorial_ghost/dreamblock_sequence", Position);
            }
            else if (currentAnimationId2 == "dash")
            {
                _ = DashDirection.Y != 0.0
                    ? Audio.Play("event:/new_content/char/tutorial_ghost/jump_super", Position)
                    : chaserState.Scale.X > 0.0
                        ? Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_right", Position)
                        : Audio.Play("event:/new_content/char/tutorial_ghost/dash_red_left", Position);
            }
            else if (currentAnimationId2 is "climbUp" or "climbDown" or "wallslide")
            {
                _ = Audio.Play("event:/new_content/char/tutorial_ghost/grab", Position);
            }
            else
            {
                if ((!currentAnimationId2.Equals("runSlow_carry") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("runFast") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("runSlow") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("walk") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("runStumble") || currentAnimationFrame != 6) && (!currentAnimationId2.Equals("flip") || currentAnimationFrame != 4) && (!currentAnimationId2.Equals("runWind") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("idleC") || Sprite.Mode != PlayerSpriteMode.MadelineNoBackpack || (currentAnimationFrame != 3 && currentAnimationFrame != 6 && currentAnimationFrame != 8 && currentAnimationFrame != 11)) && (!currentAnimationId2.Equals("carryTheoWalk") || (currentAnimationFrame != 0 && currentAnimationFrame != 6)) && (!currentAnimationId2.Equals("push") || (currentAnimationFrame != 8 && currentAnimationFrame != 15)))
                {
                    return;
                }

                _ = Audio.Play("event:/new_content/char/tutorial_ghost/footstep", Position);
            }
        }

        public override void Update()
        {
            if (startDelay > 0.0)
            {
                startDelay -= Engine.DeltaTime;
            }

            LastPosition = Position;
            base.Update();
            if (FrameIndex >= Timeline.Count - 1 || (double)Time >= TrimEnd)
            {
                if (Visible)
                {
                    _ = Audio.Play("event:/new_content/char/tutorial_ghost/disappear", Position);
                }

                Visible = false;
                Position = start;
                loopDelay -= Engine.DeltaTime;
                if (loopDelay <= 0.0)
                {
                    Player player = Scene?.Tracker.GetEntity<Player>();
                    if (player == null || ((double)player.X > rangeMinX && (double)player.X < rangeMaxX))
                    {
                        Restart();
                    }
                }
            }
            else if (startDelay <= 0.0)
            {
                SetFrame(FrameIndex);
                Time += Engine.DeltaTime;
                while (FrameIndex < Timeline.Count - 1 && (double)Time >= Timeline[FrameIndex + 1].TimeStamp)
                {
                    ++FrameIndex;
                }
            }
            if (!Visible || !ShowTrail || Scene == null || !Scene.OnInterval(0.1f))
            {
                return;
            }

            _ = TrailManager.Add(Position, Sprite, Hair, Sprite.Scale, Hair.Color, Depth + 1);
        }
    }
}
