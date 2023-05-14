// Decompiled with JetBrains decompiler
// Type: Celeste.SeekerStatue
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class SeekerStatue : Entity
    {
        private readonly SeekerStatue.Hatch hatch;
        private readonly Sprite sprite;

        public SeekerStatue(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            SeekerStatue seekerStatue = this;
            Depth = 8999;
            Add(sprite = GFX.SpriteBank.Create("seeker"));
            sprite.Play("statue");
            sprite.OnLastFrame = f =>
            {
                if (!(f == nameof(hatch)))
                {
                    return;
                }

                seekerStatue.Scene.Add(new Seeker(data, offset)
                {
                    Light = {
                        Alpha = 0.0f
                    }
                });
                seekerStatue.RemoveSelf();
            };
            hatch = data.Enum<SeekerStatue.Hatch>(nameof(hatch));
        }

        public override void Update()
        {
            base.Update();
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !(sprite.CurrentAnimationID == "statue"))
            {
                return;
            }

            bool flag = false;
            if (hatch == SeekerStatue.Hatch.Distance && (double)(entity.Position - Position).Length() < 220.0)
            {
                flag = true;
            }
            else if (hatch == SeekerStatue.Hatch.PlayerRightOfX && (double)entity.X > (double)X + 32.0)
            {
                flag = true;
            }

            if (!flag)
            {
                return;
            }

            BreakOutParticles();
            sprite.Play("hatch");
            _ = Audio.Play("event:/game/05_mirror_temple/seeker_statue_break", Position);
            _ = Alarm.Set(this, 0.8f, new Action(BreakOutParticles));
        }

        private void BreakOutParticles()
        {
            Level level = SceneAs<Level>();
            for (float direction = 0.0f; (double)direction < 6.2831854820251465; direction += 0.17453292f)
            {
                Vector2 position = Center + Calc.AngleToVector(direction + Calc.Random.Range(-1f * (float)Math.PI / 90f, (float)Math.PI / 90f), Calc.Random.Range(12, 20));
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
