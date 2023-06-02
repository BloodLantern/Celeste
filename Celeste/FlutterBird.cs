using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked(false)]
    public class FlutterBird : Entity
    {
        private static readonly Color[] colors = new Color[4]
        {
            Calc.HexToColor("89fbff"),
            Calc.HexToColor("f0fc6c"),
            Calc.HexToColor("f493ff"),
            Calc.HexToColor("93baff")
        };
        private Sprite sprite;
        private Vector2 start;
        private Coroutine routine;
        private bool flyingAway;
        private SoundSource tweetingSfx;
        private SoundSource flyawaySfx;

        public FlutterBird(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Depth = -9999;
            this.start = this.Position;
            this.Add((Component) (this.sprite = GFX.SpriteBank.Create("flutterbird")));
            this.sprite.Color = Calc.Random.Choose<Color>(FlutterBird.colors);
            this.Add((Component) (this.routine = new Coroutine(this.IdleRoutine())));
            this.Add((Component) (this.flyawaySfx = new SoundSource()));
            this.Add((Component) (this.tweetingSfx = new SoundSource()));
            this.tweetingSfx.Play("event:/game/general/birdbaby_tweet_loop");
        }

        public override void Update()
        {
            this.sprite.Scale.X = Calc.Approach(this.sprite.Scale.X, (float) Math.Sign(this.sprite.Scale.X), 4f * Engine.DeltaTime);
            this.sprite.Scale.Y = Calc.Approach(this.sprite.Scale.Y, 1f, 4f * Engine.DeltaTime);
            base.Update();
        }

        private IEnumerator IdleRoutine()
        {
            FlutterBird flutterBird = this;
            while (true)
            {
                Player player = flutterBird.Scene.Tracker.GetEntity<Player>();
                float delay = 0.25f + Calc.Random.NextFloat(1f);
                float p;
                for (p = 0.0f; (double) p < (double) delay; p += Engine.DeltaTime)
                {
                    if (player != null && (double) Math.Abs(player.X - flutterBird.X) < 48.0 && (double) player.Y > (double) flutterBird.Y - 40.0 && (double) player.Y < (double) flutterBird.Y + 8.0)
                        flutterBird.FlyAway(Math.Sign(flutterBird.X - player.X), Calc.Random.NextFloat(0.2f));
                    yield return (object) null;
                }
                Audio.Play("event:/game/general/birdbaby_hop", flutterBird.Position);
                Vector2 target = flutterBird.start + new Vector2(Calc.Random.NextFloat(8f) - 4f, 0.0f);
                flutterBird.sprite.Scale.X = (float) Math.Sign(target.X - flutterBird.Position.X);
                SimpleCurve bezier = new SimpleCurve(flutterBird.Position, target, (flutterBird.Position + target) / 2f - Vector2.UnitY * 14f);
                for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 4f)
                {
                    flutterBird.Position = bezier.GetPoint(p);
                    yield return (object) null;
                }
                flutterBird.sprite.Scale.X = (float) Math.Sign(flutterBird.sprite.Scale.X) * 1.4f;
                flutterBird.sprite.Scale.Y = 0.6f;
                flutterBird.Position = target;
                player = (Player) null;
                target = new Vector2();
                bezier = new SimpleCurve();
            }
        }

        private IEnumerator FlyAwayRoutine(int direction, float delay)
        {
            FlutterBird flutterBird = this;
            Level level = flutterBird.Scene as Level;
            yield return (object) delay;
            flutterBird.sprite.Play("fly");
            flutterBird.sprite.Scale.X = (float) -direction * 1.25f;
            flutterBird.sprite.Scale.Y = 1.25f;
            level.ParticlesFG.Emit(Calc.Random.Choose<ParticleType>(ParticleTypes.Dust), flutterBird.Position, -1.57079637f);
            Vector2 from = flutterBird.Position;
            Vector2 to = flutterBird.Position + new Vector2((float) (direction * 4), -8f);
            for (float p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 3f)
            {
                flutterBird.Position = from + (to - from) * Ease.CubeOut(p);
                yield return (object) null;
            }
            from = new Vector2();
            to = new Vector2();
            flutterBird.Depth = -10001;
            flutterBird.sprite.Scale.X = -flutterBird.sprite.Scale.X;
            to = new Vector2((float) direction, -4f) * 8f;
            while ((double) flutterBird.Y + 8.0 > (double) level.Bounds.Top)
            {
                to += new Vector2((float) (direction * 64), (float) sbyte.MinValue) * Engine.DeltaTime;
                flutterBird.Position = flutterBird.Position + to * Engine.DeltaTime;
                if (flutterBird.Scene.OnInterval(0.1f) && (double) flutterBird.Y > (double) level.Camera.Top + 32.0)
                {
                    foreach (Entity entity in flutterBird.Scene.Tracker.GetEntities<FlutterBird>())
                    {
                        if ((double) Math.Abs(flutterBird.X - entity.X) < 48.0 && (double) Math.Abs(flutterBird.Y - entity.Y) < 48.0 && !(entity as FlutterBird).flyingAway)
                            (entity as FlutterBird).FlyAway(direction, Calc.Random.NextFloat(0.25f));
                    }
                }
                yield return (object) null;
            }
            to = new Vector2();
            flutterBird.Scene.Remove((Entity) flutterBird);
        }

        public void FlyAway(int direction, float delay)
        {
            if (this.flyingAway)
                return;
            this.tweetingSfx.Stop();
            this.flyingAway = true;
            this.flyawaySfx.Play("event:/game/general/birdbaby_flyaway");
            this.Remove((Component) this.routine);
            this.Add((Component) (this.routine = new Coroutine(this.FlyAwayRoutine(direction, delay))));
        }
    }
}
