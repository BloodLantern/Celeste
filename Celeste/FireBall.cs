using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class FireBall : Entity
    {
        public static ParticleType P_FireTrail;
        public static ParticleType P_IceTrail;
        public static ParticleType P_IceBreak;
        private const float FireSpeed = 60f;
        private const float IceSpeed = 30f;
        private const float IceSpeedMult = 0.5f;
        private Vector2[] nodes;
        private int amount;
        private int index;
        private float offset;
        private float[] lengths;
        private float speed;
        private float speedMult;
        private float percent;
        private bool iceMode;
        private bool broken;
        private float mult;
        private bool notCoreMode;
        private SoundSource trackSfx;
        private Sprite sprite;
        private Wiggler hitWiggler;
        private Vector2 hitDir;

        public FireBall(
            Vector2[] nodes,
            int amount,
            int index,
            float offset,
            float speedMult,
            bool notCoreMode)
        {
            Tag = (int) Tags.TransitionUpdate;
            Collider = new Circle(6f);
            this.nodes = nodes;
            this.amount = amount;
            this.index = index;
            this.offset = offset;
            mult = speedMult;
            this.notCoreMode = notCoreMode;
            lengths = new float[nodes.Length];
            for (int index1 = 1; index1 < lengths.Length; ++index1)
                lengths[index1] = lengths[index1 - 1] + Vector2.Distance(nodes[index1 - 1], nodes[index1]);
            speed = 60f / lengths[lengths.Length - 1] * mult;
            percent = index != 0 ? index / (float) amount : 0.0f;
            percent += 1f / amount * offset;
            percent %= 1f;
            Position = GetPercentPosition(percent);
            Add(new PlayerCollider(OnPlayer));
            Add(new PlayerCollider(OnBounce, new Hitbox(16f, 6f, -8f, -3f)));
            Add(new CoreModeListener(OnChangeMode));
            Add(sprite = GFX.SpriteBank.Create("fireball"));
            Add(hitWiggler = Wiggler.Create(1.2f, 2f));
            hitWiggler.StartZero = true;
            if (index != 0)
                return;
            Add(trackSfx = new SoundSource());
        }

        public FireBall(EntityData data, Vector2 offset)
            : this(data.NodesWithPosition(offset), data.Int(nameof (amount), 1), 0, data.Float(nameof (offset)), data.Float(nameof (speed), 1f), data.Bool(nameof (notCoreMode)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            iceMode = SceneAs<Level>().CoreMode == Session.CoreModes.Cold || notCoreMode;
            speedMult = iceMode ? 0.0f : 1f;
            sprite.Play(iceMode ? "ice" : "hot", randomizeFrame: true);
            if (this.index == 0)
            {
                for (int index = 1; index < amount; ++index)
                    Scene.Add(new FireBall(nodes, amount, index, offset, mult, notCoreMode));
            }
            if (trackSfx == null || iceMode)
                return;
            PositionTrackSfx();
            trackSfx.Play("event:/env/local/09_core/fireballs_idle");
        }

        public override void Update()
        {
            if ((Scene as Level).Transitioning)
            {
                PositionTrackSfx();
            }
            else
            {
                base.Update();
                speedMult = Calc.Approach(speedMult, iceMode ? 0.5f : 1f, 2f * Engine.DeltaTime);
                percent += speed * speedMult * Engine.DeltaTime;
                if (percent >= 1.0)
                {
                    percent %= 1f;
                    if (broken && nodes[nodes.Length - 1] != nodes[0])
                    {
                        broken = false;
                        Collidable = true;
                        sprite.Play(iceMode ? "ice" : "hot", randomizeFrame: true);
                    }
                }
                Position = GetPercentPosition(percent);
                PositionTrackSfx();
                if (broken || !Scene.OnInterval(iceMode ? 0.08f : 0.05f))
                    return;
                SceneAs<Level>().ParticlesBG.Emit(iceMode ? FireBall.P_IceTrail : FireBall.P_FireTrail, 1, Center, Vector2.One * 4f);
            }
        }

        public void PositionTrackSfx()
        {
            if (trackSfx == null)
                return;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
                return;
            Vector2? nullable = new Vector2?();
            for (int index = 1; index < nodes.Length; ++index)
            {
                Vector2 vector2_1 = Calc.ClosestPointOnLine(nodes[index - 1], nodes[index], entity.Center);
                if (nullable.HasValue)
                {
                    Vector2 vector2_2 = vector2_1 - entity.Center;
                    double num1 = vector2_2.Length();
                    vector2_2 = nullable.Value - entity.Center;
                    double num2 = vector2_2.Length();
                    if (num1 >= num2)
                        continue;
                }
                nullable = vector2_1;
            }
            if (!nullable.HasValue)
                return;
            trackSfx.Position = nullable.Value - Position;
            trackSfx.UpdateSfxPosition();
        }

        public override void Render()
        {
            sprite.Position = hitDir * hitWiggler.Value * 8f;
            if (!broken)
                sprite.DrawOutline(Color.Black);
            base.Render();
        }

        private void OnPlayer(Player player)
        {
            if (!iceMode && !broken)
            {
                KillPlayer(player);
            }
            else
            {
                if (!iceMode || broken || player.Bottom <= Y + 4.0)
                    return;
                KillPlayer(player);
            }
        }

        private void KillPlayer(Player player)
        {
            Vector2 direction = (player.Center - Center).SafeNormalize();
            if (player.Die(direction) == null)
                return;
            hitDir = direction;
            hitWiggler.Start();
        }

        private void OnBounce(Player player)
        {
            if (!iceMode || broken || player.Bottom > Y + 4.0 || player.Speed.Y < 0.0)
                return;
            Audio.Play("event:/game/09_core/iceball_break", Position);
            sprite.Play("shatter");
            broken = true;
            Collidable = false;
            player.Bounce((int) (Y - 2.0));
            SceneAs<Level>().Particles.Emit(FireBall.P_IceBreak, 18, Center, Vector2.One * 6f);
        }

        private void OnChangeMode(Session.CoreModes mode)
        {
            iceMode = mode == Session.CoreModes.Cold;
            if (!broken)
                sprite.Play(iceMode ? "ice" : "hot", randomizeFrame: true);
            if (index != 0 || trackSfx == null)
                return;
            if (iceMode)
            {
                trackSfx.Stop();
            }
            else
            {
                PositionTrackSfx();
                trackSfx.Play("event:/env/local/09_core/fireballs_idle");
            }
        }

        private Vector2 GetPercentPosition(float percent)
        {
            if (percent <= 0.0)
                return nodes[0];
            if (percent >= 1.0)
                return nodes[nodes.Length - 1];
            float length = lengths[lengths.Length - 1];
            float num = length * percent;
            int index = 0;
            while (index < lengths.Length - 1 && lengths[index + 1] <= (double) num)
                ++index;
            float min = lengths[index] / length;
            float max = lengths[index + 1] / length;
            float amount = Calc.ClampedMap(percent, min, max);
            return Vector2.Lerp(nodes[index], nodes[index + 1], amount);
        }
    }
}
