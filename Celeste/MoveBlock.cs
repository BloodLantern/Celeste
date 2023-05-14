// Decompiled with JetBrains decompiler
// Type: Celeste.MoveBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class MoveBlock : Solid
    {
        public static ParticleType P_Activate;
        public static ParticleType P_Break;
        public static ParticleType P_Move;
        private const float Accel = 300f;
        private const float MoveSpeed = 60f;
        private const float FastMoveSpeed = 75f;
        private const float SteerSpeed = 50.2654839f;
        private const float MaxAngle = 0.7853982f;
        private const float NoSteerTime = 0.2f;
        private const float CrashTime = 0.15f;
        private const float CrashResetTime = 0.1f;
        private const float RegenTime = 3f;
        private readonly bool canSteer;
        private readonly bool fast;
        private readonly MoveBlock.Directions direction;
        private readonly float homeAngle;
        private readonly int angleSteerSign;
        private Vector2 startPosition;
        private MoveBlock.MovementState state;
        private bool leftPressed;
        private bool rightPressed;
        private bool topPressed;
        private float speed;
        private float targetSpeed;
        private float angle;
        private float targetAngle;
        private Player noSquish;
        private readonly List<Monocle.Image> body = new();
        private readonly List<Monocle.Image> topButton = new();
        private readonly List<Monocle.Image> leftButton = new();
        private readonly List<Monocle.Image> rightButton = new();
        private readonly List<MTexture> arrows = new();
        private MoveBlock.Border border;
        private Color fillColor = MoveBlock.idleBgFill;
        private float flash;
        private readonly SoundSource moveSfx;
        private bool triggered;
        private static readonly Color idleBgFill = Calc.HexToColor("474070");
        private static readonly Color pressedBgFill = Calc.HexToColor("30b335");
        private static readonly Color breakingBgFill = Calc.HexToColor("cc2541");
        private float particleRemainder;

        public MoveBlock(
            Vector2 position,
            int width,
            int height,
            MoveBlock.Directions direction,
            bool canSteer,
            bool fast)
            : base(position, width, height, false)
        {
            Depth = -1;
            startPosition = position;
            this.canSteer = canSteer;
            this.direction = direction;
            this.fast = fast;
            switch (direction)
            {
                case MoveBlock.Directions.Left:
                    homeAngle = targetAngle = angle = 3.14159274f;
                    angleSteerSign = -1;
                    break;
                case MoveBlock.Directions.Up:
                    homeAngle = targetAngle = angle = -1.57079637f;
                    angleSteerSign = 1;
                    break;
                case MoveBlock.Directions.Down:
                    homeAngle = targetAngle = angle = 1.57079637f;
                    angleSteerSign = -1;
                    break;
                default:
                    homeAngle = targetAngle = angle = 0.0f;
                    angleSteerSign = 1;
                    break;
            }
            int num1 = width / 8;
            int num2 = height / 8;
            MTexture mtexture1 = GFX.Game["objects/moveBlock/base"];
            MTexture mtexture2 = GFX.Game["objects/moveBlock/button"];
            if (canSteer && (direction == MoveBlock.Directions.Left || direction == MoveBlock.Directions.Right))
            {
                for (int index = 0; index < num1; ++index)
                {
                    int num3 = index == 0 ? 0 : (index < num1 - 1 ? 1 : 2);
                    AddImage(mtexture2.GetSubtexture(num3 * 8, 0, 8, 8), new Vector2(index * 8, -4f), 0.0f, new Vector2(1f, 1f), topButton);
                }
                mtexture1 = GFX.Game["objects/moveBlock/base_h"];
            }
            else if (canSteer && (direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down))
            {
                for (int index = 0; index < num2; ++index)
                {
                    int num4 = index == 0 ? 0 : (index < num2 - 1 ? 1 : 2);
                    AddImage(mtexture2.GetSubtexture(num4 * 8, 0, 8, 8), new Vector2(-4f, index * 8), 1.57079637f, new Vector2(1f, -1f), leftButton);
                    AddImage(mtexture2.GetSubtexture(num4 * 8, 0, 8, 8), new Vector2(((num1 - 1) * 8) + 4, index * 8), 1.57079637f, new Vector2(1f, 1f), rightButton);
                }
                mtexture1 = GFX.Game["objects/moveBlock/base_v"];
            }
            for (int x = 0; x < num1; ++x)
            {
                for (int y = 0; y < num2; ++y)
                {
                    int num5 = x == 0 ? 0 : (x < num1 - 1 ? 1 : 2);
                    int num6 = y == 0 ? 0 : (y < num2 - 1 ? 1 : 2);
                    AddImage(mtexture1.GetSubtexture(num5 * 8, num6 * 8, 8, 8), new Vector2(x, y) * 8f, 0.0f, new Vector2(1f, 1f), body);
                }
            }
            arrows = GFX.Game.GetAtlasSubtextures("objects/moveBlock/arrow");
            Add(moveSfx = new SoundSource());
            Add(new Coroutine(Controller()));
            UpdateColors();
            Add(new LightOcclude(0.5f));
        }

        public MoveBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Enum<MoveBlock.Directions>(nameof(direction)), data.Bool(nameof(canSteer), true), data.Bool(nameof(fast)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(border = new MoveBlock.Border(this));
        }

        private IEnumerator Controller()
        {
            MoveBlock moveBlock = this;
            while (true)
            {
                moveBlock.triggered = false;
                moveBlock.state = MoveBlock.MovementState.Idling;
                while (!moveBlock.triggered && !moveBlock.HasPlayerRider())
                {
                    yield return null;
                }

                _ = Audio.Play("event:/game/04_cliffside/arrowblock_activate", moveBlock.Position);
                moveBlock.state = MoveBlock.MovementState.Moving;
                moveBlock.StartShaking(0.2f);
                moveBlock.ActivateParticles();
                yield return 0.2f;
                moveBlock.targetSpeed = moveBlock.fast ? 75f : 60f;
                _ = moveBlock.moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                _ = moveBlock.moveSfx.Param("arrow_stop", 0.0f);
                moveBlock.StopPlayerRunIntoAnimation = false;
                float crashTimer = 0.15f;
                float crashResetTimer = 0.1f;
                float noSteerTimer = 0.2f;
                while (true)
                {
                    if (moveBlock.canSteer)
                    {
                        moveBlock.targetAngle = moveBlock.homeAngle;
                        bool flag = moveBlock.direction is MoveBlock.Directions.Right or MoveBlock.Directions.Left ? moveBlock.HasPlayerOnTop() : moveBlock.HasPlayerClimbing();
                        if (flag && (double)noSteerTimer > 0.0)
                        {
                            noSteerTimer -= Engine.DeltaTime;
                        }

                        if (flag)
                        {
                            if ((double)noSteerTimer <= 0.0)
                            {
                                moveBlock.targetAngle = moveBlock.direction is MoveBlock.Directions.Right or MoveBlock.Directions.Left ? moveBlock.homeAngle + (0.7853982f * moveBlock.angleSteerSign * Input.MoveY.Value) : moveBlock.homeAngle + (0.7853982f * moveBlock.angleSteerSign * Input.MoveX.Value);
                            }
                        }
                        else
                        {
                            noSteerTimer = 0.2f;
                        }
                    }
                    if (moveBlock.Scene.OnInterval(0.02f))
                    {
                        moveBlock.MoveParticles();
                    }

                    moveBlock.speed = Calc.Approach(moveBlock.speed, moveBlock.targetSpeed, 300f * Engine.DeltaTime);
                    moveBlock.angle = Calc.Approach(moveBlock.angle, moveBlock.targetAngle, 50.2654839f * Engine.DeltaTime);
                    Vector2 vector = Calc.AngleToVector(moveBlock.angle, moveBlock.speed);
                    Vector2 vec = vector * Engine.DeltaTime;
                    bool flag1;
                    if (moveBlock.direction is MoveBlock.Directions.Right or MoveBlock.Directions.Left)
                    {
                        flag1 = moveBlock.MoveCheck(vec.XComp());
                        moveBlock.noSquish = moveBlock.Scene.Tracker.GetEntity<Player>();
                        _ = moveBlock.MoveVCollideSolids(vec.Y, false);
                        moveBlock.noSquish = null;
                        moveBlock.LiftSpeed = vector;
                        if (moveBlock.Scene.OnInterval(0.03f))
                        {
                            if (vec.Y > 0.0)
                            {
                                moveBlock.ScrapeParticles(Vector2.UnitY);
                            }
                            else if (vec.Y < 0.0)
                            {
                                moveBlock.ScrapeParticles(-Vector2.UnitY);
                            }
                        }
                    }
                    else
                    {
                        flag1 = moveBlock.MoveCheck(vec.YComp());
                        moveBlock.noSquish = moveBlock.Scene.Tracker.GetEntity<Player>();
                        _ = moveBlock.MoveHCollideSolids(vec.X, false);
                        moveBlock.noSquish = null;
                        moveBlock.LiftSpeed = vector;
                        if (moveBlock.Scene.OnInterval(0.03f))
                        {
                            if (vec.X > 0.0)
                            {
                                moveBlock.ScrapeParticles(Vector2.UnitX);
                            }
                            else if (vec.X < 0.0)
                            {
                                moveBlock.ScrapeParticles(-Vector2.UnitX);
                            }
                        }
                        if (moveBlock.direction == MoveBlock.Directions.Down && (double)moveBlock.Top > moveBlock.SceneAs<Level>().Bounds.Bottom + 32)
                        {
                            flag1 = true;
                        }
                    }
                    if (flag1)
                    {
                        _ = moveBlock.moveSfx.Param("arrow_stop", 1f);
                        crashResetTimer = 0.1f;
                        if ((double)crashTimer > 0.0)
                        {
                            crashTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        _ = moveBlock.moveSfx.Param("arrow_stop", 0.0f);
                        if ((double)crashResetTimer > 0.0)
                        {
                            crashResetTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            crashTimer = 0.15f;
                        }
                    }
                    Level scene = moveBlock.Scene as Level;
                    double left1 = (double)moveBlock.Left;
                    Rectangle bounds = scene.Bounds;
                    double left2 = bounds.Left;
                    if (left1 >= left2)
                    {
                        double top1 = (double)moveBlock.Top;
                        bounds = scene.Bounds;
                        double top2 = bounds.Top;
                        if (top1 >= top2)
                        {
                            double right1 = (double)moveBlock.Right;
                            bounds = scene.Bounds;
                            double right2 = bounds.Right;
                            if (right1 <= right2)
                            {
                                yield return null;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                _ = Audio.Play("event:/game/04_cliffside/arrowblock_break", moveBlock.Position);
                _ = moveBlock.moveSfx.Stop();
                moveBlock.state = MoveBlock.MovementState.Breaking;
                moveBlock.speed = moveBlock.targetSpeed = 0.0f;
                moveBlock.angle = moveBlock.targetAngle = moveBlock.homeAngle;
                moveBlock.StartShaking(0.2f);
                moveBlock.StopPlayerRunIntoAnimation = true;
                yield return 0.2f;
                moveBlock.BreakParticles();
                List<MoveBlock.Debris> debris = new();
                for (int index1 = 0; index1 < (double)moveBlock.Width; index1 += 8)
                {
                    for (int index2 = 0; index2 < (double)moveBlock.Height; index2 += 8)
                    {
                        Vector2 vector2 = new(index1 + 4f, index2 + 4f);
                        MoveBlock.Debris debris1 = Engine.Pooler.Create<MoveBlock.Debris>().Init(moveBlock.Position + vector2, moveBlock.Center, moveBlock.startPosition + vector2);
                        debris.Add(debris1);
                        moveBlock.Scene.Add(debris1);
                    }
                }
                moveBlock.MoveStaticMovers(moveBlock.startPosition - moveBlock.Position);
                moveBlock.DisableStaticMovers();
                moveBlock.Position = moveBlock.startPosition;
                moveBlock.Visible = moveBlock.Collidable = false;
                yield return 2.2f;
                foreach (MoveBlock.Debris debris2 in debris)
                {
                    debris2.StopMoving();
                }

                while (moveBlock.CollideCheck<Actor>() || moveBlock.CollideCheck<Solid>())
                {
                    yield return null;
                }

                moveBlock.Collidable = true;
                EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                Coroutine routine;
                moveBlock.Add(routine = new Coroutine(moveBlock.SoundFollowsDebrisCenter(instance, debris)));
                foreach (MoveBlock.Debris debris3 in debris)
                {
                    debris3.StartShaking();
                }

                yield return 0.2f;
                foreach (MoveBlock.Debris debris4 in debris)
                {
                    debris4.ReturnHome(0.65f);
                }

                yield return 0.6f;
                routine.RemoveSelf();
                foreach (Entity entity in debris)
                {
                    entity.RemoveSelf();
                }

                _ = Audio.Play("event:/game/04_cliffside/arrowblock_reappear", moveBlock.Position);
                moveBlock.Visible = true;
                moveBlock.EnableStaticMovers();
                moveBlock.speed = moveBlock.targetSpeed = 0.0f;
                moveBlock.angle = moveBlock.targetAngle = moveBlock.homeAngle;
                moveBlock.noSquish = null;
                moveBlock.fillColor = MoveBlock.idleBgFill;
                moveBlock.UpdateColors();
                moveBlock.flash = 1f;
            }
        }

        private IEnumerator SoundFollowsDebrisCenter(
            EventInstance instance,
            List<MoveBlock.Debris> debris)
        {
            while (true)
            {
                _ = (int)instance.getPlaybackState(out PLAYBACK_STATE state);
                if (state != PLAYBACK_STATE.STOPPED)
                {
                    Vector2 zero = Vector2.Zero;
                    foreach (MoveBlock.Debris debri in debris)
                    {
                        zero += debri.Position;
                    }

                    Audio.Position(instance, zero / debris.Count);
                    yield return null;
                }
                else
                {
                    break;
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (canSteer)
            {
                bool flag1 = (direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down) && CollideCheck<Player>(Position + new Vector2(-1f, 0.0f));
                bool flag2 = (direction == MoveBlock.Directions.Up || direction == MoveBlock.Directions.Down) && CollideCheck<Player>(Position + new Vector2(1f, 0.0f));
                bool flag3 = (direction == MoveBlock.Directions.Left || direction == MoveBlock.Directions.Right) && CollideCheck<Player>(Position + new Vector2(0.0f, -1f));
                foreach (GraphicsComponent graphicsComponent in topButton)
                {
                    graphicsComponent.Y = flag3 ? 2f : 0.0f;
                }

                foreach (GraphicsComponent graphicsComponent in leftButton)
                {
                    graphicsComponent.X = flag1 ? 2f : 0.0f;
                }

                foreach (GraphicsComponent graphicsComponent in rightButton)
                {
                    graphicsComponent.X = Width + (flag2 ? -2f : 0.0f);
                }

                if ((flag1 && !leftPressed) || (flag3 && !topPressed) || (flag2 && !rightPressed))
                {
                    _ = Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
                }

                if ((!flag1 && leftPressed) || (!flag3 && topPressed) || (!flag2 && rightPressed))
                {
                    _ = Audio.Play("event:/game/04_cliffside/arrowblock_side_release", Position);
                }

                leftPressed = flag1;
                rightPressed = flag2;
                topPressed = flag3;
            }
            if (moveSfx != null && moveSfx.Playing)
            {
                _ = moveSfx.Param("arrow_influence", (int)Math.Floor(((-(double)(Calc.AngleToVector(angle, 1f) * new Vector2(-1f, 1f)).Angle() + 6.2831854820251465) % 6.2831854820251465 / 6.2831854820251465 * 8.0) + 0.5) + 1);
            }

            border.Visible = Visible;
            flash = Calc.Approach(flash, 0.0f, Engine.DeltaTime * 5f);
            UpdateColors();
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            triggered = true;
        }

        public override void MoveHExact(int move)
        {
            if (noSquish != null && ((move < 0 && (double)noSquish.X < (double)X) || (move > 0 && (double)noSquish.X > (double)X)))
            {
                while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + (Vector2.UnitX * move)))
                {
                    move -= Math.Sign(move);
                }
            }
            base.MoveHExact(move);
        }

        public override void MoveVExact(int move)
        {
            if (noSquish != null && move < 0 && (double)noSquish.Y <= (double)Y)
            {
                while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + (Vector2.UnitY * move)))
                {
                    move -= Math.Sign(move);
                }
            }
            base.MoveVExact(move);
        }

        private bool MoveCheck(Vector2 speed)
        {
            if (speed.X != 0.0)
            {
                if (!MoveHCollideSolids(speed.X, false))
                {
                    return false;
                }

                for (int index1 = 1; index1 <= 3; ++index1)
                {
                    for (int index2 = 1; index2 >= -1; index2 -= 2)
                    {
                        if (!CollideCheck<Solid>(Position + new Vector2(Math.Sign(speed.X), index1 * index2)))
                        {
                            MoveVExact(index1 * index2);
                            MoveHExact(Math.Sign(speed.X));
                            return false;
                        }
                    }
                }
                return true;
            }
            if (speed.Y == 0.0 || !MoveVCollideSolids(speed.Y, false))
            {
                return false;
            }

            for (int index3 = 1; index3 <= 3; ++index3)
            {
                for (int index4 = 1; index4 >= -1; index4 -= 2)
                {
                    if (!CollideCheck<Solid>(Position + new Vector2(index3 * index4, Math.Sign(speed.Y))))
                    {
                        MoveHExact(index3 * index4);
                        MoveVExact(Math.Sign(speed.Y));
                        return false;
                    }
                }
            }
            return true;
        }

        private void UpdateColors()
        {
            Color color = MoveBlock.idleBgFill;
            if (state == MoveBlock.MovementState.Moving)
            {
                color = MoveBlock.pressedBgFill;
            }
            else if (state == MoveBlock.MovementState.Breaking)
            {
                color = MoveBlock.breakingBgFill;
            }

            fillColor = Color.Lerp(fillColor, color, 10f * Engine.DeltaTime);
            foreach (GraphicsComponent graphicsComponent in topButton)
            {
                graphicsComponent.Color = fillColor;
            }

            foreach (GraphicsComponent graphicsComponent in leftButton)
            {
                graphicsComponent.Color = fillColor;
            }

            foreach (GraphicsComponent graphicsComponent in rightButton)
            {
                graphicsComponent.Color = fillColor;
            }
        }

        private void AddImage(
            MTexture tex,
            Vector2 position,
            float rotation,
            Vector2 scale,
            List<Monocle.Image> addTo)
        {
            Monocle.Image image = new(tex)
            {
                Position = position + new Vector2(4f, 4f)
            };
            _ = image.CenterOrigin();
            image.Rotation = rotation;
            image.Scale = scale;
            Add(image);
            addTo?.Add(image);
        }

        private void SetVisible(List<Monocle.Image> images, bool visible)
        {
            foreach (Component image in images)
            {
                image.Visible = visible;
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            foreach (Component component in leftButton)
            {
                component.Render();
            }

            foreach (Component component in rightButton)
            {
                component.Render();
            }

            foreach (Component component in topButton)
            {
                component.Render();
            }

            Draw.Rect(X + 3f, Y + 3f, Width - 6f, Height - 6f, fillColor);
            foreach (Component component in body)
            {
                component.Render();
            }

            Draw.Rect(Center.X - 4f, Center.Y - 4f, 8f, 8f, fillColor);
            if (state != MoveBlock.MovementState.Breaking)
            {
                arrows[Calc.Clamp((int)Math.Floor(((-(double)angle + 6.2831854820251465) % 6.2831854820251465 / 6.2831854820251465 * 8.0) + 0.5), 0, 7)].DrawCentered(Center);
            }
            else
            {
                GFX.Game["objects/moveBlock/x"].DrawCentered(Center);
            }

            float num = flash * 4f;
            Draw.Rect(X - num, Y - num, Width + (num * 2f), Height + (num * 2f), Color.White * flash);
            Position = position;
        }

        private void ActivateParticles()
        {
            bool flag1 = direction is MoveBlock.Directions.Down or MoveBlock.Directions.Up;
            int num = !canSteer || !flag1 ? (!CollideCheck<Player>(Position - Vector2.UnitX) ? 1 : 0) : 0;
            bool flag2 = (!canSteer || !flag1) && !CollideCheck<Player>(Position + Vector2.UnitX);
            bool flag3 = !canSteer | flag1 && !CollideCheck<Player>(Position - Vector2.UnitY);
            if (num != 0)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)((double)Height / 2.0), CenterLeft, Vector2.UnitY * (Height - 4f) * 0.5f, 3.14159274f);
            }

            if (flag2)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)((double)Height / 2.0), CenterRight, Vector2.UnitY * (Height - 4f) * 0.5f, 0.0f);
            }

            if (flag3)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)((double)Width / 2.0), TopCenter, Vector2.UnitX * (Width - 4f) * 0.5f, -1.57079637f);
            }

            SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)((double)Width / 2.0), BottomCenter, Vector2.UnitX * (Width - 4f) * 0.5f, 1.57079637f);
        }

        private void BreakParticles()
        {
            Vector2 center = Center;
            for (int index1 = 0; index1 < (double)Width; index1 += 4)
            {
                for (int index2 = 0; index2 < (double)Height; index2 += 4)
                {
                    Vector2 position = Position + new Vector2(2 + index1, 2 + index2);
                    SceneAs<Level>().Particles.Emit(MoveBlock.P_Break, 1, position, Vector2.One * 2f, (position - center).Angle());
                }
            }
        }

        private void MoveParticles()
        {
            Vector2 position;
            Vector2 vector2;
            float direction;
            float num;
            if (this.direction == MoveBlock.Directions.Right)
            {
                position = CenterLeft + Vector2.UnitX;
                vector2 = Vector2.UnitY * (Height - 4f);
                direction = 3.14159274f;
                num = Height / 32f;
            }
            else if (this.direction == MoveBlock.Directions.Left)
            {
                position = CenterRight;
                vector2 = Vector2.UnitY * (Height - 4f);
                direction = 0.0f;
                num = Height / 32f;
            }
            else if (this.direction == MoveBlock.Directions.Down)
            {
                position = TopCenter + Vector2.UnitY;
                vector2 = Vector2.UnitX * (Width - 4f);
                direction = -1.57079637f;
                num = Width / 32f;
            }
            else
            {
                position = BottomCenter;
                vector2 = Vector2.UnitX * (Width - 4f);
                direction = 1.57079637f;
                num = Width / 32f;
            }
            this.particleRemainder += num;
            int particleRemainder = (int)this.particleRemainder;
            this.particleRemainder -= particleRemainder;
            Vector2 positionRange = vector2 * 0.5f;
            if (particleRemainder <= 0)
            {
                return;
            }

            SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Move, particleRemainder, position, positionRange, direction);
        }

        private void ScrapeParticles(Vector2 dir)
        {
            _ = Collidable ? 1 : 0;
            Collidable = false;
            if (dir.X != 0.0)
            {
                float x = dir.X <= 0.0 ? Left - 1f : Right;
                for (int index = 0; index < (double)Height; index += 8)
                {
                    Vector2 vector2 = new(x, Top + 4f + index);
                    if (Scene.CollideCheck<Solid>(vector2))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
                    }
                }
            }
            else
            {
                float y = dir.Y <= 0.0 ? Top - 1f : Bottom;
                for (int index = 0; index < (double)Width; index += 8)
                {
                    Vector2 vector2 = new(Left + 4f + index, y);
                    if (Scene.CollideCheck<Solid>(vector2))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
                    }
                }
            }
            Collidable = true;
        }

        public enum Directions
        {
            Left,
            Right,
            Up,
            Down,
        }

        private enum MovementState
        {
            Idling,
            Moving,
            Breaking,
        }

        private class Border : Entity
        {
            public MoveBlock Parent;

            public Border(MoveBlock parent)
            {
                Parent = parent;
                Depth = 1;
            }

            public override void Update()
            {
                if (Parent.Scene != Scene)
                {
                    RemoveSelf();
                }

                base.Update();
            }

            public override void Render()
            {
                Draw.Rect((float)((double)Parent.X + Parent.Shake.X - 1.0), (float)((double)Parent.Y + Parent.Shake.Y - 1.0), Parent.Width + 2f, Parent.Height + 2f, Color.Black);
            }
        }

        [Pooled]
        private class Debris : Actor
        {
            private readonly Monocle.Image sprite;
            private Vector2 home;
            private Vector2 speed;
            private bool shaking;
            private bool returning;
            private float returnEase;
            private float returnDuration;
            private SimpleCurve returnCurve;
            private bool firstHit;
            private float alpha;
            private readonly Collision onCollideH;
            private readonly Collision onCollideV;
            private float spin;

            public Debris()
                : base(Vector2.Zero)
            {
                Tag = (int)Tags.TransitionUpdate;
                Collider = new Hitbox(4f, 4f, -2f, -2f);
                Add(sprite = new Monocle.Image(Calc.Random.Choose<MTexture>(GFX.Game.GetAtlasSubtextures("objects/moveblock/debris"))));
                _ = sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                onCollideH = c => speed.X = (float)(-(double)speed.X * 0.5);
                onCollideV = c =>
                {
                    if (firstHit || speed.Y > 50.0)
                    {
                        _ = Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0.0f, 600f));
                    }

                    speed.Y = (double)speed.Y is <= 0.0 or >= 40.0 ? (float)(-(double)speed.Y * 0.25) : 0.0f;
                    firstHit = false;
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public MoveBlock.Debris Init(Vector2 position, Vector2 center, Vector2 returnTo)
            {
                Collidable = true;
                Position = position;
                speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
                home = returnTo;
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                returning = false;
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose<int>(1, -1);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (!returning)
                {
                    if (Collidable)
                    {
                        speed.X = Calc.Approach(speed.X, 0.0f, Engine.DeltaTime * 100f);
                        if (!OnGround())
                        {
                            speed.Y += 400f * Engine.DeltaTime;
                        }

                        _ = MoveH(speed.X * Engine.DeltaTime, onCollideH);
                        _ = MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                    }
                    if (shaking && Scene.OnInterval(0.05f))
                    {
                        sprite.X = Calc.Random.Next(3) - 1;
                        sprite.Y = Calc.Random.Next(3) - 1;
                    }
                }
                else
                {
                    Position = returnCurve.GetPoint(Ease.CubeOut(returnEase));
                    returnEase = Calc.Approach(returnEase, 1f, Engine.DeltaTime / returnDuration);
                    sprite.Scale = Vector2.One * (float)(1.0 + (returnEase * 0.5));
                }
                if ((Scene as Level).Transitioning)
                {
                    alpha = Calc.Approach(alpha, 0.0f, Engine.DeltaTime * 4f);
                    sprite.Color = Color.White * alpha;
                }
                sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
            }

            public void StopMoving()
            {
                Collidable = false;
            }

            public void StartShaking()
            {
                shaking = true;
            }

            public void ReturnHome(float duration)
            {
                if (Scene != null)
                {
                    Camera camera = (Scene as Level).Camera;
                    if ((double)X < (double)camera.X)
                    {
                        X = camera.X - 8f;
                    }

                    if ((double)Y < (double)camera.Y)
                    {
                        Y = camera.Y - 8f;
                    }

                    if ((double)X > (double)camera.X + 320.0)
                    {
                        X = (float)((double)camera.X + 320.0 + 8.0);
                    }

                    if ((double)Y > (double)camera.Y + 180.0)
                    {
                        Y = (float)((double)camera.Y + 180.0 + 8.0);
                    }
                }
                returning = true;
                returnEase = 0.0f;
                returnDuration = duration;
                Vector2 vector2 = (home - Position).SafeNormalize();
                returnCurve = new SimpleCurve(Position, home, ((Position + home) / 2f) + (new Vector2(vector2.Y, -vector2.X) * (Calc.Random.NextFloat(16f) + 16f) * Calc.Random.Facing()));
            }
        }
    }
}
