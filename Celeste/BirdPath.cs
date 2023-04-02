// Decompiled with JetBrains decompiler
// Type: Celeste.BirdPath
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class BirdPath : Entity
    {
        private Vector2 start;
        private readonly Sprite sprite;
        private readonly Vector2[] nodes;
        private Color trailColor = Calc.HexToColor("639bff");
        private Vector2 target;
        private Vector2 speed;
        private readonly float maxspeed;
        private Vector2 lastTrail;
        private readonly float speedMult;
        private EntityID ID;
        private readonly bool onlyOnce;
        private readonly bool onlyIfLeft;

        public BirdPath(EntityID id, EntityData data, Vector2 offset)
            : this(id, data.Position + offset, data.NodesOffset(offset), data.Bool("only_once"), data.Bool(nameof(onlyIfLeft)), data.Float(nameof(speedMult), 1f))
        {
        }

        public BirdPath(
            EntityID id,
            Vector2 position,
            Vector2[] nodes,
            bool onlyOnce,
            bool onlyIfLeft,
            float speedMult)
        {
            Tag = (int)Tags.TransitionUpdate;
            ID = id;
            Position = position;
            start = position;
            this.nodes = nodes;
            this.onlyOnce = onlyOnce;
            this.onlyIfLeft = onlyIfLeft;
            this.speedMult = speedMult;
            maxspeed = 150f * speedMult;
            Add(sprite = GFX.SpriteBank.Create("bird"));
            sprite.Play("flyupRoll");
            _ = sprite.JustifyOrigin(0.5f, 0.75f);
            Add(new SoundSource("event:/new_content/game/10_farewell/bird_flyuproll")
            {
                RemoveOnOneshotEnd = true
            });
            Add(new Coroutine(Routine()));
        }

        public void WaitForTrigger()
        {
            Visible = Active = false;
        }

        public void Trigger()
        {
            Visible = Active = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!onlyOnce)
                return;
            _ = (Scene as Level).Session.DoNotLoad.Add(ID);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (!onlyIfLeft)
                return;

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || entity.X <= X)
                return;

            RemoveSelf();
        }

        private IEnumerator Routine()
        {
            BirdPath birdPath = this;
            Vector2 begin = birdPath.start;
            for (int i = 0; i <= birdPath.nodes.Length - 1; i += 2)
            {
                Vector2 node = birdPath.nodes[i];
                Vector2 next = birdPath.nodes[i + 1];
                SimpleCurve curve = new(begin, next, node);
                float duration = curve.GetLengthParametric(32) / birdPath.maxspeed;
                bool playedSfx = false;
                _ = birdPath.Position;
                for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * birdPath.speedMult / duration)
                {
                    birdPath.target = curve.GetPoint(p);
                    if ((double)p > 0.89999997615814209)
                    {
                        if (!playedSfx && birdPath.sprite.CurrentAnimationID != "flyupRoll")
                        {
                            birdPath.Add(new SoundSource("event:/new_content/game/10_farewell/bird_flyuproll")
                            {
                                RemoveOnOneshotEnd = true
                            });
                            playedSfx = true;
                        }
                        birdPath.sprite.Play("flyupRoll");
                    }
                    yield return null;
                }
                begin = next;
                _ = new Vector2();
                _ = new SimpleCurve();
            }
            birdPath.RemoveSelf();
        }

        public override void Update()
        {
            if ((Scene as Level).Transitioning)
            {
                foreach (Component component in (Entity)this)
                    if (component is SoundSource soundSource)
                        soundSource.UpdateSfxPosition();
            }
            else
            {
                base.Update();
                int num1 = Math.Sign(X - target.X);
                speed += (target - Position).SafeNormalize() * 800f * Engine.DeltaTime;
                if (speed.Length() > maxspeed)
                    speed = speed.SafeNormalize(maxspeed);

                Position += speed * Engine.DeltaTime;
                int num2 = Math.Sign(X - target.X);
                if (num1 != num2)
                    speed.X *= 0.75f;

                sprite.Rotation = 1.57079637f + Calc.AngleLerp(speed.Angle(), Calc.Angle(Position, target), 0.5f);
                if ((lastTrail - Position).Length() <= 32)
                    return;

                TrailManager.Add(this, trailColor);
                lastTrail = Position;
            }
        }

        public override void Render()
        {
            base.Render();
        }

        public override void DebugRender(Camera camera)
        {
            Vector2 begin = start;
            for (int i = 0; i < nodes.Length - 1; i += 2)
            {
                Vector2 node = nodes[i + 1];
                new SimpleCurve(begin, node, nodes[i]).Render(Color.Red * 0.25f, 32);
                begin = node;
            }
            Draw.Line(Position, Position + ((target - Position).SafeNormalize() * ((target - Position).Length() - 3f)), Color.Yellow);
            Draw.Circle(target, 3f, Color.Yellow, 16);
        }
    }
}
