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
        private Image backImg;
        private Sprite front;
        public Sprite Lever;
        private Image top;
        private bool brokenLever;
        private bool inCliffside;

        public Vector2 Start { get; private set; }

        public Vector2 Destination { get; private set; }

        public Vector2 Halfway { get; private set; }

        public Gondola(EntityData data, Vector2 offset)
            : base(data.Position + offset, 64f, 8f, true)
        {
            EnableAssistModeChecks = false;
            Add(front = GFX.SpriteBank.Create("gondola"));
            front.Play("idle");
            front.Origin = new Vector2(front.Width / 2f, 12f);
            front.Y = -52f;
            Add(top = new Image(GFX.Game["objects/gondola/top"]));
            top.Origin = new Vector2(top.Width / 2f, 12f);
            top.Y = -52f;
            Add(Lever = new Sprite(GFX.Game, "objects/gondola/lever"));
            Lever.Add("idle", "", 0.0f, new int[1]);
            Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
            Lever.Origin = new Vector2(front.Width / 2f, 12f);
            Lever.Y = -52f;
            Lever.Play("idle");
            (Collider as Hitbox).Position.X = (float) (-(double) Collider.Width / 2.0);
            Start = Position;
            Destination = offset + data.Nodes[0];
            Halfway = (Position + Destination) / 2f;
            Depth = -10500;
            inCliffside = data.Bool("active", true);
            SurfaceSoundIndex = 28;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(back = new Entity(Position));
            back.Depth = 9000;
            backImg = new Image(GFX.Game["objects/gondola/back"]);
            backImg.Origin = new Vector2(backImg.Width / 2f, 12f);
            backImg.Y = -52f;
            back.Add(backImg);
            scene.Add(LeftCliffside = new Entity(Position + new Vector2(-124f, 0.0f)));
            Image image1 = new Image(GFX.Game["objects/gondola/cliffsideLeft"]);
            image1.JustifyOrigin(0.0f, 1f);
            LeftCliffside.Add(image1);
            LeftCliffside.Depth = 8998;
            scene.Add(RightCliffside = new Entity(Destination + new Vector2(144f, -104f)));
            Image image2 = new Image(GFX.Game["objects/gondola/cliffsideRight"]);
            image2.JustifyOrigin(0.0f, 0.5f);
            image2.Scale.X = -1f;
            RightCliffside.Add(image2);
            RightCliffside.Depth = 8998;
            scene.Add(new Rope
            {
                Gondola = this
            });
            if (!inCliffside)
            {
                Position = Destination;
                Lever.Visible = false;
                UpdatePositions();
                JumpThru jumpThru = new JumpThru(Position + new Vector2((float) (-(double) Width / 2.0), -36f), (int) Width, true);
                jumpThru.SurfaceSoundIndex = 28;
                Scene.Add(jumpThru);
            }
            top.Rotation = Calc.Angle(Start, Destination);
        }

        public override void Update()
        {
            if (inCliffside)
            {
                float num = Math.Sign(Rotation) == Math.Sign(RotationSpeed) ? 8f : 6f;
                if (Math.Abs(Rotation) < 0.5)
                    num *= 0.5f;
                if (Math.Abs(Rotation) < 0.25)
                    num *= 0.5f;
                RotationSpeed += -Math.Sign(Rotation) * num * Engine.DeltaTime;
                Rotation += RotationSpeed * Engine.DeltaTime;
                Rotation = Calc.Clamp(Rotation, -0.4f, 0.4f);
                if (Math.Abs(Rotation) < 0.019999999552965164 && Math.Abs(RotationSpeed) < 0.20000000298023224)
                    Rotation = RotationSpeed = 0.0f;
            }
            UpdatePositions();
            base.Update();
        }

        private void UpdatePositions()
        {
            back.Position = Position;
            backImg.Rotation = Rotation;
            front.Rotation = Rotation;
            if (!brokenLever)
                Lever.Rotation = Rotation;
            top.Rotation = Calc.Angle(Start, Destination);
        }

        public Vector2 GetRotatedFloorPositionAt(float x, float y = 52f)
        {
            Vector2 vector = Calc.AngleToVector(Rotation + 1.57079637f, 1f);
            Vector2 vector2 = new Vector2(-vector.Y, vector.X);
            return Position + new Vector2(0.0f, -52f) + vector * y - vector2 * x;
        }

        public void BreakLever() => Add(new Coroutine(BreakLeverRoutine()));

        private IEnumerator BreakLeverRoutine()
        {
            brokenLever = true;
            Vector2 speed = new Vector2(240f, -130f);
            while (true)
            {
                Sprite lever = Lever;
                lever.Position += speed * Engine.DeltaTime;
                Lever.Rotation += 2f * Engine.DeltaTime;
                speed.Y += 400f * Engine.DeltaTime;
                yield return null;
            }
        }

        public void PullSides() => front.Play("pull");

        public void CancelPullSides() => front.Play("idle");

        private class Rope : Entity
        {
            public Gondola Gondola;

            public Rope() => Depth = 8999;

            public override void Render()
            {
                Vector2 vector2_1 = (Gondola.LeftCliffside.Position + new Vector2(40f, -12f)).Floor();
                Vector2 vector2_2 = (Gondola.RightCliffside.Position + new Vector2(-40f, -4f)).Floor();
                Vector2 vector2_3 = (vector2_2 - vector2_1).SafeNormalize();
                Vector2 vector2_4 = Gondola.Position + new Vector2(0.0f, -55f) - vector2_3 * 6f;
                Vector2 vector2_5 = Gondola.Position + new Vector2(0.0f, -55f) + vector2_3 * 6f;
                for (int index = 0; index < 2; ++index)
                {
                    Vector2 vector2_6 = Vector2.UnitY * index;
                    Draw.Line(vector2_1 + vector2_6, vector2_4 + vector2_6, Color.Black);
                    Draw.Line(vector2_5 + vector2_6, vector2_2 + vector2_6, Color.Black);
                }
            }
        }
    }
}
