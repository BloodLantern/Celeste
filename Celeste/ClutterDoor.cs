// Decompiled with JetBrains decompiler
// Type: Celeste.ClutterDoor
// Assembly: Celeste, Version=1, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class ClutterDoor : Solid
    {
        public ClutterBlock.Colors Color;
        private readonly Sprite sprite;
        private readonly Wiggler wiggler;

        public ClutterDoor(EntityData data, Vector2 offset, Session session)
            : base(data.Position + offset, data.Width, data.Height, false)
        {
            Color = data.Enum("type", ClutterBlock.Colors.Green);
            SurfaceSoundIndex = 20;
            Tag = (int) Tags.TransitionUpdate;
            Add(sprite = GFX.SpriteBank.Create("ghost_door"));
            sprite.Position = new Vector2(Width, Height) / 2f;
            sprite.Play("idle");
            OnDashCollide = new DashCollision(OnDashed);
            Add(wiggler = Wiggler.Create(0.6f, 3f, f => sprite.Scale = Vector2.One * (1 - f * 0.2f)));
            if (IsLocked(session))
                return;

            InstantUnlock();
        }

        public override void Update()
        {
            Level scene = Scene as Level;
            if (scene.Transitioning && CollideCheck<Player>())
            {
                Visible = false;
                Collidable = false;
            }
            else if (!Collidable && IsLocked(scene.Session) && !CollideCheck<Player>())
            {
                Visible = true;
                Collidable = true;
                wiggler.Start();
                _ = Audio.Play("event:/game/03_resort/forcefield_bump", Position);
            }
            base.Update();
        }

        public bool IsLocked(Session session)
        {
            return !session.GetFlag("oshiro_clutter_door_open") || IsComplete(session);
        }

        public bool IsComplete(Session session)
        {
            return session.GetFlag("oshiro_clutter_cleared_" + (int) Color);
        }

        public IEnumerator UnlockRoutine()
        {
            ClutterDoor clutterDoor = this;
            Camera camera = clutterDoor.SceneAs<Level>().Camera;
            Vector2 from = camera.Position;
            Vector2 to = clutterDoor.CameraTarget();
            float p;
            if ((from - to).Length() > 8)
                for (p = 0f; p < 1; p += Engine.DeltaTime)
                {
                    camera.Position = from + ((to - from) * Ease.CubeInOut(p));
                    yield return null;
                }
            else
            {
                yield return 0.2f;
            }

            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            _ = Audio.Play("event:/game/03_resort/forcefield_vanish", clutterDoor.Position);
            clutterDoor.sprite.Play("open");
            clutterDoor.Collidable = false;
            for (p = 0f; p < 0.4f; p += Engine.DeltaTime)
            {
                camera.Position = clutterDoor.CameraTarget();
                yield return null;
            }
        }

        public void InstantUnlock()
        {
            Visible = Collidable = false;
        }

        private Vector2 CameraTarget()
        {
            Level level = SceneAs<Level>();
            Vector2 vector2 = Position - (new Vector2(320f, 180f) / 2f);
            ref Vector2 local1 = ref vector2;
            float x = vector2.X;
            Rectangle bounds1 = level.Bounds;
            float left = bounds1.Left;
            bounds1 = level.Bounds;
            float max1 = bounds1.Right - 320;
            float num1 = MathHelper.Clamp(x, left, max1);
            local1.X = num1;
            ref Vector2 local2 = ref vector2;
            float y = vector2.Y;
            Rectangle bounds2 = level.Bounds;
            float top = bounds2.Top;
            bounds2 = level.Bounds;
            float max2 = bounds2.Bottom - 180;
            float num2 = MathHelper.Clamp(y, top, max2);
            local2.Y = num2;
            return vector2;
        }

        private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            wiggler.Start();
            _ = Audio.Play("event:/game/03_resort/forcefield_bump", Position);
            return DashCollisionResults.Bounce;
        }
    }
}
