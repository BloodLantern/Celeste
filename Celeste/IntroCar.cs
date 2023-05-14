// Decompiled with JetBrains decompiler
// Type: Celeste.IntroCar
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class IntroCar : JumpThru
    {
        private readonly Monocle.Image bodySprite;
        private Entity wheels;
        private readonly float startY;
        private bool didHaveRider;

        public IntroCar(Vector2 position)
            : base(position, 25, true)
        {
            startY = position.Y;
            Depth = 1;
            Add(bodySprite = new Monocle.Image(GFX.Game["scenery/car/body"]));
            bodySprite.Origin = new Vector2(bodySprite.Width / 2f, bodySprite.Height);
            Collider = new ColliderList(new Collider[2]
            {
                 new Hitbox(25f, 4f, -15f, -17f),
                 new Hitbox(19f, 4f, 8f, -11f)
            });
            SurfaceSoundIndex = 2;
        }

        public IntroCar(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Monocle.Image image = new(GFX.Game["scenery/car/wheels"]);
            image.Origin = new Vector2(image.Width / 2f, image.Height);
            wheels = new Entity(Position)
            {
                image
            };
            wheels.Depth = 3;
            scene.Add(wheels);
            Level level = scene as Level;
            if (level.Session.Area.ID != 0)
            {
                return;
            }

            IntroPavement introPavement = new(new Vector2(level.Bounds.Left, Y), (int)((double)X - level.Bounds.Left - 48.0))
            {
                Depth = -10001
            };
            level.Add(introPavement);
            level.Add(new IntroCarBarrier(Position + new Vector2(32f, 0.0f), -10, Color.White));
            level.Add(new IntroCarBarrier(Position + new Vector2(41f, 0.0f), 5, Color.DarkGray));
        }

        public override void Update()
        {
            bool flag = HasRider();
            if ((double)Y > startY && (!flag || (double)Y > startY + 1.0))
            {
                MoveV(-10f * Engine.DeltaTime);
            }

            if ((((double)Y > startY ? 0 : (!didHaveRider ? 1 : 0)) & (flag ? 1 : 0)) != 0)
            {
                MoveV(2f);
            }

            if (didHaveRider && !flag)
            {
                _ = Audio.Play("event:/game/00_prologue/car_up", Position);
            }

            didHaveRider = flag;
            base.Update();
        }

        public override int GetLandSoundIndex(Entity entity)
        {
            _ = Audio.Play("event:/game/00_prologue/car_down", Position);
            return -1;
        }
    }
}
