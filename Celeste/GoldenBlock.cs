using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked]
    public class GoldenBlock : Solid
    {
        private MTexture[,] nineSlice;
        private Image berry;
        private float startY;
        private float yLerp;
        private float sinkTimer;
        private float renderLerp;

        public GoldenBlock(Vector2 position, float width, float height)
            : base(position, width, height, false)
        {
            startY = Y;
            berry = new Image(GFX.Game["collectables/goldberry/idle00"]);
            berry.CenterOrigin();
            berry.Position = new Vector2(width / 2f, height / 2f);
            MTexture mtexture = GFX.Game["objects/goldblock"];
            nineSlice = new MTexture[3, 3];
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                    nineSlice[index1, index2] = mtexture.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
            }
            Depth = -10000;
            Add(new LightOcclude());
            Add(new MirrorSurface());
            SurfaceSoundIndex = 32;
        }

        public GoldenBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Visible = false;
            Collidable = false;
            renderLerp = 1f;
            bool flag = false;
            foreach (Strawberry strawberry in scene.Entities.FindAll<Strawberry>())
            {
                if (strawberry.Golden && strawberry.Follower.Leader != null)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
                return;
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (!Visible)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.X > X - 80.0)
                {
                    Visible = true;
                    Collidable = true;
                    renderLerp = 1f;
                }
            }
            if (Visible)
                renderLerp = Calc.Approach(renderLerp, 0.0f, Engine.DeltaTime * 3f);
            if (HasPlayerRider())
                sinkTimer = 0.1f;
            else if (sinkTimer > 0.0)
                sinkTimer -= Engine.DeltaTime;
            yLerp = sinkTimer <= 0.0 ? Calc.Approach(yLerp, 0.0f, 1f * Engine.DeltaTime) : Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
            MoveToY(MathHelper.Lerp(startY, startY + 12f, Ease.SineInOut(yLerp)));
        }

        private void DrawBlock(Vector2 offset, Color color)
        {
            float num1 = (float) (Collider.Width / 8.0 - 1.0);
            float num2 = (float) (Collider.Height / 8.0 - 1.0);
            for (int val1_1 = 0; val1_1 <= (double) num1; ++val1_1)
            {
                for (int val1_2 = 0; val1_2 <= (double) num2; ++val1_2)
                    nineSlice[val1_1 < (double) num1 ? Math.Min(val1_1, 1) : 2, val1_2 < (double) num2 ? Math.Min(val1_2, 1) : 2].Draw(Position + offset + Shake + new Vector2(val1_1 * 8, val1_2 * 8), Vector2.Zero, color);
            }
        }

        public override void Render()
        {
            Vector2 vector2 = new Vector2(0.0f, (float) ((Scene as Level).Bounds.Bottom - (double) startY + 32.0) * Ease.CubeIn(renderLerp));
            Vector2 position = Position;
            Position += vector2;
            DrawBlock(new Vector2(-1f, 0.0f), Color.Black);
            DrawBlock(new Vector2(1f, 0.0f), Color.Black);
            DrawBlock(new Vector2(0.0f, -1f), Color.Black);
            DrawBlock(new Vector2(0.0f, 1f), Color.Black);
            DrawBlock(Vector2.Zero, Color.White);
            berry.Color = Color.White;
            berry.RenderPosition = Center;
            berry.Render();
            Position = position;
        }
    }
}
