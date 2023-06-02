using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class FireBarrier : Entity
    {
        public static ParticleType P_Deactivate;
        private LavaRect Lava;
        private Solid solid;
        private SoundSource idleSfx;

        public FireBarrier(Vector2 position, float width, float height)
            : base(position)
        {
            this.Tag = (int) Tags.TransitionUpdate;
            this.Collider = (Collider) new Hitbox(width, height);
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
            this.Add((Component) new CoreModeListener(new Action<Session.CoreModes>(this.OnChangeMode)));
            this.Add((Component) (this.Lava = new LavaRect(width, height, 4)));
            this.Lava.SurfaceColor = RisingLava.Hot[0];
            this.Lava.EdgeColor = RisingLava.Hot[1];
            this.Lava.CenterColor = RisingLava.Hot[2];
            this.Lava.SmallWaveAmplitude = 2f;
            this.Lava.BigWaveAmplitude = 1f;
            this.Lava.CurveAmplitude = 1f;
            this.Depth = -8500;
            this.Add((Component) (this.idleSfx = new SoundSource()));
            this.idleSfx.Position = new Vector2(this.Width, this.Height) / 2f;
        }

        public FireBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add((Entity) (this.solid = new Solid(this.Position + new Vector2(2f, 3f), this.Width - 4f, this.Height - 5f, false)));
            this.Collidable = this.solid.Collidable = this.SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            if (!this.Collidable)
                return;
            this.idleSfx.Play("event:/env/local/09_core/lavagate_idle");
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            this.Collidable = this.solid.Collidable = mode == Session.CoreModes.Hot;
            if (!this.Collidable)
            {
                Level level = this.SceneAs<Level>();
                Vector2 center = this.Center;
                for (int index1 = 0; (double) index1 < (double) this.Width; index1 += 4)
                {
                    for (int index2 = 0; (double) index2 < (double) this.Height; index2 += 4)
                    {
                        Vector2 position = this.Position + new Vector2((float) (index1 + 2), (float) (index2 + 2)) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                        level.Particles.Emit(FireBarrier.P_Deactivate, position, (position - center).Angle());
                    }
                }
                this.idleSfx.Stop();
            }
            else
                this.idleSfx.Play("event:/env/local/09_core/lavagate_idle");
        }

        private void OnPlayer(Player player) => player.Die((player.Center - this.Center).SafeNormalize());

        public override void Update()
        {
            if ((this.Scene as Level).Transitioning)
            {
                if (this.idleSfx == null)
                    return;
                this.idleSfx.UpdateSfxPosition();
            }
            else
                base.Update();
        }

        public override void Render()
        {
            if (!this.Collidable)
                return;
            base.Render();
        }
    }
}
