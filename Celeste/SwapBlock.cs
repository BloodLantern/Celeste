using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class SwapBlock : Solid
    {
        public static ParticleType P_Move;
        private const float ReturnTime = 0.8f;
        public Vector2 Direction;
        public bool Swapping;
        public Themes Theme;
        private Vector2 start;
        private Vector2 end;
        private float lerp;
        private int target;
        private Rectangle moveRect;
        private float speed;
        private readonly float maxForwardSpeed;
        private readonly float maxBackwardSpeed;
        private float returnTimer;
        private float redAlpha = 1f;
        private readonly MTexture[,] nineSliceGreen;
        private readonly MTexture[,] nineSliceRed;
        private readonly MTexture[,] nineSliceTarget;
        private readonly Sprite middleGreen;
        private readonly Sprite middleRed;
        private PathRenderer path;
        private EventInstance moveSfx;
        private EventInstance returnSfx;
        private DisplacementRenderer.Burst burst;
        private float particlesRemainder;

        public SwapBlock(
            Vector2 position,
            float width,
            float height,
            Vector2 node,
            Themes theme)
            : base(position, width, height, false)
        {
            Theme = theme;
            start = Position;
            end = node;
            maxForwardSpeed = 360f / Vector2.Distance(start, end);
            maxBackwardSpeed = maxForwardSpeed * 0.4f;
            Direction.X = Math.Sign(end.X - start.X);
            Direction.Y = Math.Sign(end.Y - start.Y);
            Add(new DashListener()
            {
                OnDash = new Action<Vector2>(OnDash)
            });
            int x = (int) MathHelper.Min(X, node.X);
            int y = (int) MathHelper.Min(Y, node.Y);
            int num1 = (int) MathHelper.Max(X + Width, node.X + Width);
            int num2 = (int) MathHelper.Max(Y + Height, node.Y + Height);
            moveRect = new Rectangle(x, y, num1 - x, num2 - y);
            MTexture mtexture1;
            MTexture mtexture2;
            MTexture mtexture3;
            if (Theme == Themes.Moon)
            {
                mtexture1 = GFX.Game["objects/swapblock/moon/block"];
                mtexture2 = GFX.Game["objects/swapblock/moon/blockRed"];
                mtexture3 = GFX.Game["objects/swapblock/moon/target"];
            }
            else
            {
                mtexture1 = GFX.Game["objects/swapblock/block"];
                mtexture2 = GFX.Game["objects/swapblock/blockRed"];
                mtexture3 = GFX.Game["objects/swapblock/target"];
            }
            nineSliceGreen = new MTexture[3, 3];
            nineSliceRed = new MTexture[3, 3];
            nineSliceTarget = new MTexture[3, 3];
            for (int index1 = 0; index1 < 3; ++index1)
            {
                for (int index2 = 0; index2 < 3; ++index2)
                {
                    nineSliceGreen[index1, index2] = mtexture1.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
                    nineSliceRed[index1, index2] = mtexture2.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
                    nineSliceTarget[index1, index2] = mtexture3.GetSubtexture(new Rectangle(index1 * 8, index2 * 8, 8, 8));
                }
            }
            if (Theme == Themes.Normal)
            {
                Add(middleGreen = GFX.SpriteBank.Create("swapBlockLight"));
                Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRed"));
            }
            else if (Theme == Themes.Moon)
            {
                Add(middleGreen = GFX.SpriteBank.Create("swapBlockLightMoon"));
                Add(middleRed = GFX.SpriteBank.Create("swapBlockLightRedMoon"));
            }
            Add(new LightOcclude(0.2f));
            Depth = -9999;
        }

        public SwapBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Nodes[0] + offset, data.Enum<Themes>("theme"))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(path = new PathRenderer(this));
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            Audio.Stop(moveSfx);
            Audio.Stop(returnSfx);
        }

        private void OnDash(Vector2 direction)
        {
            Swapping = lerp < 1.0;
            target = 1;
            returnTimer = ReturnTime;
            burst = (Scene as Level).Displacement.AddBurst(Center, 0.2f, 0.0f, 16f);
            speed = lerp < 0.2f ? MathHelper.Lerp(maxForwardSpeed * 0.333f, maxForwardSpeed, lerp / 0.2f) : maxForwardSpeed;
            Audio.Stop(returnSfx);
            Audio.Stop(moveSfx);
            if (!Swapping)
                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
            else
                moveSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_move", Center);
        }

        public override void Update()
        {
            base.Update();
            if (returnTimer > 0f)
            {
                returnTimer -= Engine.DeltaTime;
                if (returnTimer <= 0f)
                {
                    target = 0;
                    speed = 0f;
                    returnSfx = Audio.Play("event:/game/05_mirror_temple/swapblock_return", Center);
                }
            }
            if (burst != null)
                burst.Position = Center;
            redAlpha = Calc.Approach(redAlpha, target == 1 ? 0f : 1f, Engine.DeltaTime * 32f);
            if (target == 0 && lerp == 0f)
            {
                middleRed.SetAnimationFrame(0);
                middleGreen.SetAnimationFrame(0);
            }
            speed = target != 1 ? Calc.Approach(speed, maxBackwardSpeed, maxBackwardSpeed / 1.5f * Engine.DeltaTime) : Calc.Approach(speed, maxForwardSpeed, maxForwardSpeed / 0.2f * Engine.DeltaTime);
            float lerpBefore = lerp;
            lerp = Calc.Approach(lerp, target, speed * Engine.DeltaTime);
            if (lerp != lerpBefore)
            {
                Vector2 liftSpeed = (end - start) * speed;
                Vector2 position1 = Position;
                if (target == 1)
                    liftSpeed = (end - start) * maxForwardSpeed;
                if (lerp < lerpBefore)
                    liftSpeed *= -1f;
                if (target == 1 && Scene.OnInterval(0.02f))
                    MoveParticles(end - start);
                MoveTo(Vector2.Lerp(start, end, lerp), liftSpeed);
                Vector2 position2 = Position;
                if (position1 != position2)
                {
                    Audio.Position(moveSfx, Center);
                    Audio.Position(returnSfx, Center);
                    if (Position == start && target == 0)
                    {
                        Audio.SetParameter(returnSfx, "end", 1f);
                        Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", Center);
                    }
                    else if (Position == end && target == 1)
                        Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", Center);
                }
            }
            if (Swapping && lerp >= 1f)
                Swapping = false;
            StopPlayerRunIntoAnimation = lerp is <= 0f or >= 1f;
        }

        private void MoveParticles(Vector2 normal)
        {
            Vector2 position;
            Vector2 vector2;
            float direction;
            float num;
            if (normal.X > 0.0)
            {
                position = CenterLeft;
                vector2 = Vector2.UnitY * (Height - 6f);
                direction = 3.14159274f;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.X < 0.0)
            {
                position = CenterRight;
                vector2 = Vector2.UnitY * (Height - 6f);
                direction = 0.0f;
                num = Math.Max(2f, Height / 14f);
            }
            else if (normal.Y > 0.0)
            {
                position = TopCenter;
                vector2 = Vector2.UnitX * (Width - 6f);
                direction = -1.57079637f;
                num = Math.Max(2f, Width / 14f);
            }
            else
            {
                position = BottomCenter;
                vector2 = Vector2.UnitX * (Width - 6f);
                direction = 1.57079637f;
                num = Math.Max(2f, Width / 14f);
            }
            this.particlesRemainder += num;
            int particlesRemainder = (int) this.particlesRemainder;
            this.particlesRemainder -= particlesRemainder;
            Vector2 positionRange = vector2 * 0.5f;
            SceneAs<Level>().Particles.Emit(SwapBlock.P_Move, particlesRemainder, position, positionRange, direction);
        }

        public override void Render()
        {
            Vector2 pos = Position + Shake;
            if (lerp != (double) target && speed > 0.0)
            {
                Vector2 vector2 = (end - start).SafeNormalize();
                if (target == 1)
                    vector2 *= -1f;
                float num = 16f * (speed / maxForwardSpeed);
                for (int index = 2; index < (double) num; index += 2)
                    DrawBlockStyle(pos + vector2 * index, Width, Height, nineSliceGreen, middleGreen, Color.White * (float) (1.0 - index / (double) num));
            }
            if (redAlpha < 1.0)
                DrawBlockStyle(pos, Width, Height, nineSliceGreen, middleGreen, Color.White);
            if (redAlpha <= 0.0)
                return;
            DrawBlockStyle(pos, Width, Height, nineSliceRed, middleRed, Color.White * redAlpha);
        }

        private void DrawBlockStyle(
            Vector2 pos,
            float width,
            float height,
            MTexture[,] ninSlice,
            Sprite middle,
            Color color)
        {
            int num1 = (int) ((double) width / 8.0);
            int num2 = (int) ((double) height / 8.0);
            ninSlice[0, 0].Draw(pos + new Vector2(0.0f, 0.0f), Vector2.Zero, color);
            ninSlice[2, 0].Draw(pos + new Vector2(width - 8f, 0.0f), Vector2.Zero, color);
            ninSlice[0, 2].Draw(pos + new Vector2(0.0f, height - 8f), Vector2.Zero, color);
            ninSlice[2, 2].Draw(pos + new Vector2(width - 8f, height - 8f), Vector2.Zero, color);
            for (int index = 1; index < num1 - 1; ++index)
            {
                ninSlice[1, 0].Draw(pos + new Vector2(index * 8, 0.0f), Vector2.Zero, color);
                ninSlice[1, 2].Draw(pos + new Vector2(index * 8, height - 8f), Vector2.Zero, color);
            }
            for (int index = 1; index < num2 - 1; ++index)
            {
                ninSlice[0, 1].Draw(pos + new Vector2(0.0f, index * 8), Vector2.Zero, color);
                ninSlice[2, 1].Draw(pos + new Vector2(width - 8f, index * 8), Vector2.Zero, color);
            }
            for (int x = 1; x < num1 - 1; ++x)
            {
                for (int y = 1; y < num2 - 1; ++y)
                    ninSlice[1, 1].Draw(pos + new Vector2(x, y) * 8f, Vector2.Zero, color);
            }
            if (middle == null)
                return;
            middle.Color = color;
            middle.RenderPosition = pos + new Vector2(width / 2f, height / 2f);
            middle.Render();
        }

        public enum Themes
        {
            Normal,
            Moon,
        }

        private class PathRenderer : Entity
        {
            private readonly SwapBlock block;
            private readonly MTexture pathTexture;
            private readonly MTexture clipTexture = new();
            private float timer;

            public PathRenderer(SwapBlock block)
                : base(block.Position)
            {
                this.block = block;
                Depth = 8999;
                pathTexture = GFX.Game["objects/swapblock/path" + (block.start.X == (double) block.end.X ? "V" : "H")];
                timer = Calc.Random.NextFloat();
            }

            public override void Update()
            {
                base.Update();
                timer += Engine.DeltaTime * 4f;
            }

            public override void Render()
            {
                if (block.Theme != Themes.Moon)
                {
                    for (int left = block.moveRect.Left; left < block.moveRect.Right; left += pathTexture.Width)
                    {
                        for (int top = block.moveRect.Top; top < block.moveRect.Bottom; top += pathTexture.Height)
                        {
                            pathTexture.GetSubtexture(0, 0, Math.Min(pathTexture.Width, block.moveRect.Right - left), Math.Min(pathTexture.Height, block.moveRect.Bottom - top), clipTexture);
                            clipTexture.DrawCentered(new Vector2(left + clipTexture.Width / 2, top + clipTexture.Height / 2), Color.White);
                        }
                    }
                }
                block.DrawBlockStyle(new Vector2(block.moveRect.X, block.moveRect.Y), block.moveRect.Width, block.moveRect.Height, block.nineSliceTarget, null, Color.White * (float) (0.5 * (0.5 + (Math.Sin(timer) + 1.0) * 0.25)));
            }
        }
    }
}
