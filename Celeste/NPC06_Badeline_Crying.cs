using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class NPC06_Badeline_Crying : NPC
    {
        private bool started;
        private Image white;
        private BloomPoint bloom;
        private VertexLight light;
        public SoundSource LoopingSfx;
        private List<Orb> orbs = new List<Orb>();

        public NPC06_Badeline_Crying(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Add(Sprite = GFX.SpriteBank.Create("badeline_boss"));
            Sprite.Play("scaredIdle");
            Add(white = new Image(GFX.Game["characters/badelineBoss/calm_white"]));
            white.Color = Color.White * 0.0f;
            white.Origin = Sprite.Origin;
            white.Position = Sprite.Position;
            Add(bloom = new BloomPoint(new Vector2(0.0f, -6f), 0.0f, 16f));
            Add(light = new VertexLight(new Vector2(0.0f, -6f), Color.White, 1f, 24, 64));
            Add(LoopingSfx = new SoundSource("event:/char/badeline/boss_idle_ground"));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!Session.GetFlag("badeline_connection"))
                return;
            FinalBossStarfield finalBossStarfield = (scene as Level).Background.Get<FinalBossStarfield>();
            if (finalBossStarfield != null)
                finalBossStarfield.Alpha = 0.0f;
            foreach (Entity entity in Scene.Tracker.GetEntities<ReflectionTentacles>())
                entity.RemoveSelf();
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (started || entity == null || entity.X <= X - 32.0)
                return;
            Scene.Add(new CS06_BossEnd(entity, this));
            started = true;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            foreach (Entity orb in orbs)
                orb.RemoveSelf();
        }

        public IEnumerator TurnWhite(float duration)
        {
            NPC06_Badeline_Crying c06BadelineCrying = this;
            float alpha = 0.0f;
            while (alpha < 1.0)
            {
                alpha += Engine.DeltaTime / duration;
                c06BadelineCrying.white.Color = Color.White * alpha;
                c06BadelineCrying.bloom.Alpha = alpha;
                yield return null;
            }
            c06BadelineCrying.Sprite.Visible = false;
        }

        public IEnumerator Disperse()
        {
            NPC06_Badeline_Crying c06BadelineCrying = this;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            float size = 1f;
            while (c06BadelineCrying.orbs.Count < 8)
            {
                float to = size - 0.125f;
                while (size > (double) to)
                {
                    c06BadelineCrying.white.Scale = Vector2.One * size;
                    c06BadelineCrying.light.Alpha = size;
                    c06BadelineCrying.bloom.Alpha = size;
                    size -= Engine.DeltaTime;
                    yield return null;
                }
                Orb orb = new Orb(c06BadelineCrying.Position);
                orb.Target = c06BadelineCrying.Position + new Vector2(-16f, -40f);
                c06BadelineCrying.Scene.Add(orb);
                c06BadelineCrying.orbs.Add(orb);
            }
            yield return 3.25f;
            int i = 0;
            foreach (Orb orb in c06BadelineCrying.orbs)
            {
                orb.Routine.Replace(orb.CircleRoutine((float) (i / 8.0 * 6.2831854820251465)));
                ++i;
                yield return 0.2f;
            }
            yield return 2f;
            foreach (Orb orb in c06BadelineCrying.orbs)
                orb.Routine.Replace(orb.AbsorbRoutine());
            yield return 1f;
        }

        private class Orb : Entity
        {
            public Image Sprite;
            public BloomPoint Bloom;
            private float ease;
            public Vector2 Target;
            public Coroutine Routine;

            public float Ease
            {
                get => ease;
                set
                {
                    ease = value;
                    Sprite.Scale = Vector2.One * ease;
                    Bloom.Alpha = ease;
                }
            }

            public Orb(Vector2 position)
                : base(position)
            {
                Add(Sprite = new Image(GFX.Game["characters/badeline/orb"]));
                Add(Bloom = new BloomPoint(0.0f, 32f));
                Add(Routine = new Coroutine(FloatRoutine()));
                Sprite.CenterOrigin();
                Depth = -10001;
            }

            public IEnumerator FloatRoutine()
            {
                Orb orb = this;
                Vector2 speed = Vector2.Zero;
                orb.Ease = 0.2f;
                while (true)
                {
                    Vector2 target = orb.Target + Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 16f + Calc.Random.NextFloat(40f));
                    float reset = 0.0f;
                    while (reset < 1.0 && (target - orb.Position).Length() > 8.0)
                    {
                        speed += (target - orb.Position).SafeNormalize() * 420f * Engine.DeltaTime;
                        if (speed.Length() > 90.0)
                            speed = speed.SafeNormalize(90f);
                        orb.Position += speed * Engine.DeltaTime;
                        reset += Engine.DeltaTime;
                        orb.Ease = Calc.Approach(orb.Ease, 1f, Engine.DeltaTime * 4f);
                        yield return null;
                    }
                    target = new Vector2();
                }
            }

            public IEnumerator CircleRoutine(float offset)
            {
                Orb orb = this;
                Vector2 from = orb.Position;
                float ease = 0.0f;
                Player player = orb.Scene.Tracker.GetEntity<Player>();
                while (player != null)
                {
                    Vector2 vector2 = player.Center + Calc.AngleToVector(orb.Scene.TimeActive * 2f + offset, 24f);
                    ease = Calc.Approach(ease, 1f, Engine.DeltaTime * 2f);
                    orb.Position = from + (vector2 - from) * Monocle.Ease.CubeInOut(ease);
                    yield return null;
                }
            }

            public IEnumerator AbsorbRoutine()
            {
                Orb orb = this;
                Player entity = orb.Scene.Tracker.GetEntity<Player>();
                if (entity != null)
                {
                    Vector2 from = orb.Position;
                    Vector2 to = entity.Center;
                    for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime)
                    {
                        float num = Monocle.Ease.BigBackIn(p);
                        orb.Position = from + (to - from) * num;
                        orb.Ease = (float) (0.20000000298023224 + (1.0 - num) * 0.800000011920929);
                        yield return null;
                    }
                }
            }
        }
    }
}
