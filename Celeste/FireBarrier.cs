using Microsoft.Xna.Framework;
using Monocle;

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
            Tag = (int) Tags.TransitionUpdate;
            Collider = new Hitbox(width, height);
            Add(new PlayerCollider(OnPlayer));
            Add(new CoreModeListener(OnChangeMode));
            Add(Lava = new LavaRect(width, height, 4));
            Lava.SurfaceColor = RisingLava.Hot[0];
            Lava.EdgeColor = RisingLava.Hot[1];
            Lava.CenterColor = RisingLava.Hot[2];
            Lava.SmallWaveAmplitude = 2f;
            Lava.BigWaveAmplitude = 1f;
            Lava.CurveAmplitude = 1f;
            Depth = -8500;
            Add(idleSfx = new SoundSource());
            idleSfx.Position = new Vector2(Width, Height) / 2f;
        }

        public FireBarrier(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(solid = new Solid(Position + new Vector2(2f, 3f), Width - 4f, Height - 5f, false));
            Collidable = solid.Collidable = SceneAs<Level>().CoreMode == Session.CoreModes.Hot;
            if (!Collidable)
                return;
            idleSfx.Play("event:/env/local/09_core/lavagate_idle");
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            Collidable = solid.Collidable = mode == Session.CoreModes.Hot;
            if (!Collidable)
            {
                Level level = SceneAs<Level>();
                Vector2 center = Center;
                for (int index1 = 0; index1 < (double) Width; index1 += 4)
                {
                    for (int index2 = 0; index2 < (double) Height; index2 += 4)
                    {
                        Vector2 position = Position + new Vector2(index1 + 2, index2 + 2) + Calc.Random.Range(-Vector2.One * 2f, Vector2.One * 2f);
                        level.Particles.Emit(FireBarrier.P_Deactivate, position, (position - center).Angle());
                    }
                }
                idleSfx.Stop();
            }
            else
                idleSfx.Play("event:/env/local/09_core/lavagate_idle");
        }

        private void OnPlayer(Player player) => player.Die((player.Center - Center).SafeNormalize());

        public override void Update()
        {
            if ((Scene as Level).Transitioning)
            {
                if (idleSfx == null)
                    return;
                idleSfx.UpdateSfxPosition();
            }
            else
                base.Update();
        }

        public override void Render()
        {
            if (!Collidable)
                return;
            base.Render();
        }
    }
}
