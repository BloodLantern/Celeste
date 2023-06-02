using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class Gondola : Solid
    {
        public float Rotation;
        public float RotationSpeed;
        public Entity LeftCliffside;
        public Entity RightCliffside;
        private Entity back;
        private Monocle.Image backImg;
        private Sprite front;
        public Sprite Lever;
        private Monocle.Image top;
        private bool brokenLever;
        private bool inCliffside;

        public Vector2 Start { get; private set; }

        public Vector2 Destination { get; private set; }

        public Vector2 Halfway { get; private set; }

        public Gondola(EntityData data, Vector2 offset)
            : base(data.Position + offset, 64f, 8f, true)
        {
            this.EnableAssistModeChecks = false;
            this.Add((Component) (this.front = GFX.SpriteBank.Create("gondola")));
            this.front.Play("idle");
            this.front.Origin = new Vector2(this.front.Width / 2f, 12f);
            this.front.Y = -52f;
            this.Add((Component) (this.top = new Monocle.Image(GFX.Game["objects/gondola/top"])));
            this.top.Origin = new Vector2(this.top.Width / 2f, 12f);
            this.top.Y = -52f;
            this.Add((Component) (this.Lever = new Sprite(GFX.Game, "objects/gondola/lever")));
            this.Lever.Add("idle", "", 0.0f, new int[1]);
            this.Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
            this.Lever.Origin = new Vector2(this.front.Width / 2f, 12f);
            this.Lever.Y = -52f;
            this.Lever.Play("idle");
            (this.Collider as Hitbox).Position.X = (float) (-(double) this.Collider.Width / 2.0);
            this.Start = this.Position;
            this.Destination = offset + data.Nodes[0];
            this.Halfway = (this.Position + this.Destination) / 2f;
            this.Depth = -10500;
            this.inCliffside = data.Bool("active", true);
            this.SurfaceSoundIndex = 28;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(this.back = new Entity(this.Position));
            this.back.Depth = 9000;
            this.backImg = new Monocle.Image(GFX.Game["objects/gondola/back"]);
            this.backImg.Origin = new Vector2(this.backImg.Width / 2f, 12f);
            this.backImg.Y = -52f;
            this.back.Add((Component) this.backImg);
            scene.Add(this.LeftCliffside = new Entity(this.Position + new Vector2(-124f, 0.0f)));
            Monocle.Image image1 = new Monocle.Image(GFX.Game["objects/gondola/cliffsideLeft"]);
            image1.JustifyOrigin(0.0f, 1f);
            this.LeftCliffside.Add((Component) image1);
            this.LeftCliffside.Depth = 8998;
            scene.Add(this.RightCliffside = new Entity(this.Destination + new Vector2(144f, -104f)));
            Monocle.Image image2 = new Monocle.Image(GFX.Game["objects/gondola/cliffsideRight"]);
            image2.JustifyOrigin(0.0f, 0.5f);
            image2.Scale.X = -1f;
            this.RightCliffside.Add((Component) image2);
            this.RightCliffside.Depth = 8998;
            scene.Add((Entity) new Gondola.Rope()
            {
                Gondola = this
            });
            if (!this.inCliffside)
            {
                this.Position = this.Destination;
                this.Lever.Visible = false;
                this.UpdatePositions();
                JumpThru jumpThru = new JumpThru(this.Position + new Vector2((float) (-(double) this.Width / 2.0), -36f), (int) this.Width, true);
                jumpThru.SurfaceSoundIndex = 28;
                this.Scene.Add((Entity) jumpThru);
            }
            this.top.Rotation = Calc.Angle(this.Start, this.Destination);
        }

        public override void Update()
        {
            if (this.inCliffside)
            {
                float num = Math.Sign(this.Rotation) == Math.Sign(this.RotationSpeed) ? 8f : 6f;
                if ((double) Math.Abs(this.Rotation) < 0.5)
                    num *= 0.5f;
                if ((double) Math.Abs(this.Rotation) < 0.25)
                    num *= 0.5f;
                this.RotationSpeed += (float) -Math.Sign(this.Rotation) * num * Engine.DeltaTime;
                this.Rotation += this.RotationSpeed * Engine.DeltaTime;
                this.Rotation = Calc.Clamp(this.Rotation, -0.4f, 0.4f);
                if ((double) Math.Abs(this.Rotation) < 0.019999999552965164 && (double) Math.Abs(this.RotationSpeed) < 0.20000000298023224)
                    this.Rotation = this.RotationSpeed = 0.0f;
            }
            this.UpdatePositions();
            base.Update();
        }

        private void UpdatePositions()
        {
            this.back.Position = this.Position;
            this.backImg.Rotation = this.Rotation;
            this.front.Rotation = this.Rotation;
            if (!this.brokenLever)
                this.Lever.Rotation = this.Rotation;
            this.top.Rotation = Calc.Angle(this.Start, this.Destination);
        }

        public Vector2 GetRotatedFloorPositionAt(float x, float y = 52f)
        {
            Vector2 vector = Calc.AngleToVector(this.Rotation + 1.57079637f, 1f);
            Vector2 vector2 = new Vector2(-vector.Y, vector.X);
            return this.Position + new Vector2(0.0f, -52f) + vector * y - vector2 * x;
        }

        public void BreakLever() => this.Add((Component) new Coroutine(this.BreakLeverRoutine()));

        private IEnumerator BreakLeverRoutine()
        {
            this.brokenLever = true;
            Vector2 speed = new Vector2(240f, -130f);
            while (true)
            {
                Sprite lever = this.Lever;
                lever.Position = lever.Position + speed * Engine.DeltaTime;
                this.Lever.Rotation += 2f * Engine.DeltaTime;
                speed.Y += 400f * Engine.DeltaTime;
                yield return (object) null;
            }
        }

        public void PullSides() => this.front.Play("pull");

        public void CancelPullSides() => this.front.Play("idle");

        private class Rope : Entity
        {
            public Gondola Gondola;

            public Rope() => this.Depth = 8999;

            public override void Render()
            {
                Vector2 vector2_1 = (this.Gondola.LeftCliffside.Position + new Vector2(40f, -12f)).Floor();
                Vector2 vector2_2 = (this.Gondola.RightCliffside.Position + new Vector2(-40f, -4f)).Floor();
                Vector2 vector2_3 = (vector2_2 - vector2_1).SafeNormalize();
                Vector2 vector2_4 = this.Gondola.Position + new Vector2(0.0f, -55f) - vector2_3 * 6f;
                Vector2 vector2_5 = this.Gondola.Position + new Vector2(0.0f, -55f) + vector2_3 * 6f;
                for (int index = 0; index < 2; ++index)
                {
                    Vector2 vector2_6 = Vector2.UnitY * (float) index;
                    Draw.Line(vector2_1 + vector2_6, vector2_4 + vector2_6, Color.Black);
                    Draw.Line(vector2_5 + vector2_6, vector2_2 + vector2_6, Color.Black);
                }
            }
        }
    }
}
