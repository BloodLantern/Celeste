using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class WaveDashPlaybackTutorial
    {
        public Action OnRender;
        private bool hasUpdated;
        private float dashTrailTimer;
        private int dashTrailCounter;
        private bool dashing;
        private bool firstDash = true;
        private bool launched;
        private float launchedDelay;
        private float launchedTimer;
        private int tag;
        private Vector2 dashDirection0;
        private Vector2 dashDirection1;

        public PlayerPlayback Playback { get; private set; }

        public WaveDashPlaybackTutorial(
            string name,
            Vector2 offset,
            Vector2 dashDirection0,
            Vector2 dashDirection1)
        {
            List<Player.ChaserState> tutorial = PlaybackData.Tutorials[name];
            Playback = new PlayerPlayback(offset, PlayerSpriteMode.MadelineNoBackpack, tutorial);
            tag = Calc.Random.Next();
            this.dashDirection0 = dashDirection0;
            this.dashDirection1 = dashDirection1;
        }

        public void Update()
        {
            Playback.Update();
            Playback.Hair.AfterUpdate();
            if (Playback.Sprite.CurrentAnimationID == "dash" && Playback.Sprite.CurrentAnimationFrame == 0)
            {
                if (!dashing)
                {
                    dashing = true;
                    Celeste.Freeze(0.05f);
                    SlashFx.Burst(Playback.Center, (firstDash ? dashDirection0 : dashDirection1).Angle()).Tag = tag;
                    dashTrailTimer = 0.1f;
                    dashTrailCounter = 2;
                    CreateTrail();
                    if (firstDash)
                        launchedDelay = 0.15f;
                    firstDash = !firstDash;
                }
            }
            else
                dashing = false;
            if (dashTrailTimer > 0.0)
            {
                dashTrailTimer -= Engine.DeltaTime;
                if (dashTrailTimer <= 0.0)
                {
                    CreateTrail();
                    --dashTrailCounter;
                    if (dashTrailCounter > 0)
                        dashTrailTimer = 0.1f;
                }
            }
            if (launchedDelay > 0.0)
            {
                launchedDelay -= Engine.DeltaTime;
                if (launchedDelay <= 0.0)
                {
                    launched = true;
                    launchedTimer = 0.0f;
                }
            }
            if (launched)
            {
                float launchedTimer = this.launchedTimer;
                this.launchedTimer += Engine.DeltaTime;
                if (this.launchedTimer >= 0.5)
                {
                    launched = false;
                    this.launchedTimer = 0.0f;
                }
                else if (Calc.OnInterval(this.launchedTimer, launchedTimer, 0.15f))
                {
                    SpeedRing speedRing = Engine.Pooler.Create<SpeedRing>().Init(Playback.Center, (Playback.Position - Playback.LastPosition).Angle(), Color.White);
                    speedRing.Tag = tag;
                    Engine.Scene.Add(speedRing);
                }
            }
            hasUpdated = true;
        }

        public void Render(Vector2 position, float scale)
        {
            Matrix transformMatrix = Matrix.CreateScale(4f) * Matrix.CreateTranslation(position.X, position.Y, 0.0f);
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, transformMatrix);
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<TrailManager.Snapshot>())
            {
                if (entity.Tag == tag)
                    entity.Render();
            }
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<SlashFx>())
            {
                if (entity.Tag == tag && entity.Visible)
                    entity.Render();
            }
            foreach (Entity entity in Engine.Scene.Tracker.GetEntities<SpeedRing>())
            {
                if (entity.Tag == tag)
                    entity.Render();
            }
            if (Playback.Visible && hasUpdated)
                Playback.Render();
            if (OnRender != null)
                OnRender();
            Draw.SpriteBatch.End();
            Draw.SpriteBatch.Begin();
        }

        private void CreateTrail() => TrailManager.Add(Playback.Position, Playback.Sprite, Playback.Hair, Playback.Sprite.Scale, Player.UsedHairColor, 0).Tag = tag;
    }
}
