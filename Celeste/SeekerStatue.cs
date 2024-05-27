using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class SeekerStatue : Entity
    {
        private Hatch hatch;
        private Sprite sprite;

        public SeekerStatue(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            SeekerStatue seekerStatue = this;
            Depth = 8999;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.Play("statue");
            sprite.OnLastFrame = f =>
            {
                if (!(f == nameof (SeekerStatue.hatch)))
                    return;
                seekerStatue.Scene.Add(new Seeker(data, offset)
                {
                    Light = {
                        Alpha = 0.0f
                    }
                });
                seekerStatue.RemoveSelf();
            };
            hatch = data.Enum<Hatch>(nameof (hatch));
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !(sprite.CurrentAnimationID == "statue"))
                return;
            bool flag = false;
            if (hatch == Hatch.Distance && (entity.Position - Position).Length() < 220.0)
                flag = true;
            else if (hatch == Hatch.PlayerRightOfX && entity.X > X + 32.0)
                flag = true;
            if (!flag)
                return;
            BreakOutParticles();
            sprite.Play("hatch");
            Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", Position);
            Alarm.Set(this, 0.8f, BreakOutParticles);
        }

        private void BreakOutParticles()
        {
            Level level = SceneAs<Level>();
            for (float direction = 0.0f; direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float) Math.PI / 90f, (float) Math.PI / 90f), Calc.Random.Range(12, 20));
                level.Particles.Emit(Seeker.P_BreakOut, position, direction);
            }
        }

        private enum Hatch
        {
            Distance,
            PlayerRightOfX,
        }
    }
}
