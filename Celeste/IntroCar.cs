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
        private Monocle.Image bodySprite;
        private Entity wheels;
        private float startY;
        private bool didHaveRider;

        public IntroCar(Vector2 position)
            : base(position, 25, true)
        {
            this.startY = position.Y;
            this.Depth = 1;
            this.Add((Component) (this.bodySprite = new Monocle.Image(GFX.Game["scenery/car/body"])));
            this.bodySprite.Origin = new Vector2(this.bodySprite.Width / 2f, this.bodySprite.Height);
            this.Collider = (Collider) new ColliderList(new Collider[2]
            {
                (Collider) new Hitbox(25f, 4f, -15f, -17f),
                (Collider) new Hitbox(19f, 4f, 8f, -11f)
            });
            this.SurfaceSoundIndex = 2;
        }

        public IntroCar(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Monocle.Image image = new Monocle.Image(GFX.Game["scenery/car/wheels"]);
            image.Origin = new Vector2(image.Width / 2f, image.Height);
            this.wheels = new Entity(this.Position);
            this.wheels.Add((Component) image);
            this.wheels.Depth = 3;
            scene.Add(this.wheels);
            Level level = scene as Level;
            if (level.Session.Area.ID != 0)
                return;
            IntroPavement introPavement = new IntroPavement(new Vector2((float) level.Bounds.Left, this.Y), (int) ((double) this.X - (double) level.Bounds.Left - 48.0));
            introPavement.Depth = -10001;
            level.Add((Entity) introPavement);
            level.Add((Entity) new IntroCarBarrier(this.Position + new Vector2(32f, 0.0f), -10, Color.White));
            level.Add((Entity) new IntroCarBarrier(this.Position + new Vector2(41f, 0.0f), 5, Color.DarkGray));
        }

        public override void Update()
        {
            bool flag = this.HasRider();
            if ((double) this.Y > (double) this.startY && (!flag || (double) this.Y > (double) this.startY + 1.0))
                this.MoveV(-10f * Engine.DeltaTime);
            if ((((double) this.Y > (double) this.startY ? 0 : (!this.didHaveRider ? 1 : 0)) & (flag ? 1 : 0)) != 0)
                this.MoveV(2f);
            if (this.didHaveRider && !flag)
                Audio.Play("event:/game/00_prologue/car_up", this.Position);
            this.didHaveRider = flag;
            base.Update();
        }

        public override int GetLandSoundIndex(Entity entity)
        {
            Audio.Play("event:/game/00_prologue/car_down", this.Position);
            return -1;
        }
    }
}
