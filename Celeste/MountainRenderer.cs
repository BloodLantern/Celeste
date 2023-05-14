// Decompiled with JetBrains decompiler
// Type: Celeste.MountainRenderer
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;

namespace Celeste
{
    public class MountainRenderer : Monocle.Renderer
    {
        public bool ForceNearFog;
        public Action OnEaseEnd;
        public static readonly Vector3 RotateLookAt = new(0.0f, 7f, 0.0f);
        private const float rotateDistance = 15f;
        private const float rotateYPosition = 3f;
        private bool rotateAroundCenter;
        private bool rotateAroundTarget;
        private readonly float rotateAroundTargetDistance;
        private float rotateTimer = 1.57079637f;
        private const float DurationDivisor = 3f;
        public MountainCamera UntiltedCamera;
        public MountainModel Model;
        public bool AllowUserRotation = true;
        private Vector2 userOffset;
        private bool inFreeCameraDebugMode;
        private float percent = 1f;
        private float duration = 1f;
        private MountainCamera easeCameraFrom;
        private MountainCamera easeCameraTo;
        private float easeCameraRotationAngleTo;
        private float timer;
        private float door;

        public MountainCamera Camera => Model.Camera;

        public bool Animating { get; private set; }

        public int Area { get; private set; }

        public MountainRenderer()
        {
            Model = new MountainModel();
            GotoRotationMode();
        }

        public void Dispose()
        {
            Model.Dispose();
        }

        public override void Update(Scene scene)
        {
            timer += Engine.DeltaTime;
            Model.Update();
            userOffset += ((AllowUserRotation ? -Input.MountainAim.Value * 0.8f : Vector2.Zero) - userOffset) * (1f - (float)Math.Pow(0.0099999997764825821, (double)Engine.DeltaTime));
            if (!rotateAroundCenter)
            {
                if (Area == 8)
                {
                    userOffset.Y = Math.Max(0.0f, userOffset.Y);
                }

                if (Area == 7)
                {
                    userOffset.X = Calc.Clamp(userOffset.X, -0.4f, 0.4f);
                }
            }
            if (!inFreeCameraDebugMode)
            {
                if (rotateAroundCenter)
                {
                    rotateTimer -= Engine.DeltaTime * 0.1f;
                    Model.Camera.Position += (new Vector3((float)Math.Cos(rotateTimer) * 15f, 3f, (float)Math.Sin(rotateTimer) * 15f) - Model.Camera.Position) * (1f - (float)Math.Pow(0.10000000149011612, (double)Engine.DeltaTime));
                    Model.Camera.Target = MountainRenderer.RotateLookAt + (Vector3.Up * userOffset.Y);
                    Model.Camera.Rotation = Quaternion.Slerp(Model.Camera.Rotation, new Quaternion().LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up), Engine.DeltaTime * 4f);
                    UntiltedCamera = Camera;
                }
                else
                {
                    if (Animating)
                    {
                        percent = Calc.Approach(percent, 1f, Engine.DeltaTime / duration);
                        float num = Ease.SineOut(percent);
                        Model.Camera.Position = GetBetween(easeCameraFrom.Position, easeCameraTo.Position, num);
                        Model.Camera.Target = GetBetween(easeCameraFrom.Target, easeCameraTo.Target, num);
                        Vector3 vector1 = easeCameraFrom.Rotation.Forward();
                        Vector3 vector2 = easeCameraTo.Rotation.Forward();
                        float length = Calc.LerpClamp(vector1.XZ().Length(), vector2.XZ().Length(), num);
                        Vector2 vector3 = Calc.AngleToVector(MathHelper.Lerp(vector1.XZ().Angle(), easeCameraRotationAngleTo, num), length);
                        float y = Calc.LerpClamp(vector1.Y, vector2.Y, num);
                        Model.Camera.Rotation = new Quaternion().LookAt(new Vector3(vector3.X, y, vector3.Y), Vector3.Up);
                        if (percent >= 1.0)
                        {
                            rotateTimer = new Vector2(Model.Camera.Position.X, Model.Camera.Position.Z).Angle();
                            Animating = false;
                            OnEaseEnd?.Invoke();
                        }
                    }
                    else if (rotateAroundTarget)
                    {
                        rotateTimer -= Engine.DeltaTime * 0.1f;
                        float num = (new Vector2(easeCameraTo.Target.X, easeCameraTo.Target.Z) - new Vector2(easeCameraTo.Position.X, easeCameraTo.Position.Z)).Length();
                        Model.Camera.Position += (new Vector3(easeCameraTo.Target.X + ((float)Math.Cos(rotateTimer) * num), easeCameraTo.Position.Y, easeCameraTo.Target.Z + ((float)Math.Sin(rotateTimer) * num)) - Model.Camera.Position) * (1f - (float)Math.Pow(0.10000000149011612, (double)Engine.DeltaTime));
                        Model.Camera.Target = easeCameraTo.Target + (Vector3.Up * userOffset.Y * 2f) + (Vector3.Left * userOffset.X * 2f);
                        Model.Camera.Rotation = Quaternion.Slerp(Model.Camera.Rotation, new Quaternion().LookAt(Model.Camera.Position, Model.Camera.Target, Vector3.Up), Engine.DeltaTime * 4f);
                        UntiltedCamera = Camera;
                    }
                    else
                    {
                        Model.Camera.Rotation = easeCameraTo.Rotation;
                        Model.Camera.Target = easeCameraTo.Target;
                    }
                    UntiltedCamera = Camera;
                    if (userOffset != Vector2.Zero && !rotateAroundTarget)
                    {
                        Vector3 vector3_1 = Model.Camera.Rotation.Left() * userOffset.X * 0.25f;
                        Vector3 vector3_2 = Model.Camera.Rotation.Up() * userOffset.Y * 0.25f;
                        Model.Camera.LookAt(Model.Camera.Position + Model.Camera.Rotation.Forward() + vector3_1 + vector3_2);
                    }
                }
            }
            else
            {
                Vector3 to = Vector3.Transform(Vector3.Forward, Model.Camera.Rotation.Conjugated());
                Model.Camera.Rotation = Model.Camera.Rotation.LookAt(Vector3.Zero, to, Vector3.Up);
                Vector3 axis = Vector3.Transform(Vector3.Left, Model.Camera.Rotation.Conjugated());
                Vector3 vector3_3 = new(0.0f, 0.0f, 0.0f);
                if (MInput.Keyboard.Check(Keys.W))
                {
                    vector3_3 += to;
                }

                if (MInput.Keyboard.Check(Keys.S))
                {
                    vector3_3 -= to;
                }

                if (MInput.Keyboard.Check(Keys.D))
                {
                    vector3_3 -= axis;
                }

                if (MInput.Keyboard.Check(Keys.A))
                {
                    vector3_3 += axis;
                }

                if (MInput.Keyboard.Check(Keys.Q))
                {
                    vector3_3 += Vector3.Up;
                }

                if (MInput.Keyboard.Check(Keys.Z))
                {
                    vector3_3 += Vector3.Down;
                }

                Model.Camera.Position += vector3_3 * (MInput.Keyboard.Check(Keys.LeftShift) ? 0.5f : 5f) * Engine.DeltaTime;
                if (MInput.Mouse.CheckLeftButton)
                {
                    MouseState state = Mouse.GetState();
                    int x = Engine.Graphics.GraphicsDevice.Viewport.Width / 2;
                    int y = Engine.Graphics.GraphicsDevice.Viewport.Height / 2;
                    int num1 = state.X - x;
                    int num2 = state.Y - y;
                    Model.Camera.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, num1 * 0.1f * Engine.DeltaTime);
                    Model.Camera.Rotation *= Quaternion.CreateFromAxisAngle(axis, -num2 * 0.1f * Engine.DeltaTime);
                    Mouse.SetPosition(x, y);
                }
                if (Area >= 0)
                {
                    Vector3 target = AreaData.Areas[Area].MountainIdle.Target;
                    Vector3 vector3_4 = axis * 0.05f;
                    Vector3 vector3_5 = Vector3.Up * 0.05f;
                    Model.DebugPoints.Clear();
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector3_4 + vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector3_4 + vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector3_4 - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector3_4 + vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + vector3_4 - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - vector3_4 - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - (vector3_4 * 0.25f) - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + (vector3_4 * 0.25f) - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + (vector3_4 * 0.25f) + (Vector3.Down * 100f), Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - (vector3_4 * 0.25f) - vector3_5, Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target + (vector3_4 * 0.25f) + (Vector3.Down * 100f), Color.Red));
                    Model.DebugPoints.Add(new VertexPositionColor(target - (vector3_4 * 0.25f) + (Vector3.Down * 100f), Color.Red));
                }
            }
            door = Calc.Approach(door, Area != 9 || rotateAroundCenter ? 0.0f : 1f, Engine.DeltaTime * 1f);
            Model.CoreWallPosition = Vector3.Lerp(Vector3.Zero, -new Vector3(-1.5f, 1.5f, 1f), Ease.CubeInOut(door));
            Model.NearFogAlpha = Calc.Approach(Model.NearFogAlpha, ForceNearFog || rotateAroundCenter ? 1f : 0.0f, (rotateAroundCenter ? 1f : 4f) * Engine.DeltaTime);
            if (Celeste.PlayMode != Celeste.PlayModes.Debug)
            {
                return;
            }

            if (MInput.Keyboard.Pressed(Keys.P))
            {
                Console.WriteLine(GetCameraString());
            }

            if (MInput.Keyboard.Pressed(Keys.F2))
            {
                Engine.Scene = new OverworldLoader(Overworld.StartMode.ReturnFromOptions);
            }

            if (MInput.Keyboard.Pressed(Keys.Space))
            {
                inFreeCameraDebugMode = !inFreeCameraDebugMode;
            }

            Model.DrawDebugPoints = inFreeCameraDebugMode;
            if (!MInput.Keyboard.Pressed(Keys.F1))
            {
                return;
            }

            AreaData.ReloadMountainViews();
        }

        private Vector3 GetBetween(Vector3 from, Vector3 to, float ease)
        {
            Vector2 from1 = new(from.X, from.Z);
            Vector2 from2 = new(to.X, to.Z);
            double angleRadians = (double)Calc.AngleLerp(Calc.Angle(from1, Vector2.Zero), Calc.Angle(from2, Vector2.Zero), ease);
            float num1 = from1.Length();
            float num2 = from2.Length();
            float num3 = num1 + ((num2 - num1) * ease);
            float y = from.Y + ((to.Y - from.Y) * ease);
            double length = (double)num3;
            Vector2 vector2 = -Calc.AngleToVector((float)angleRadians, (float)length);
            return new Vector3(vector2.X, y, vector2.Y);
        }

        public override void BeforeRender(Scene scene)
        {
            Model.BeforeRender(scene);
        }

        public override void Render(Scene scene)
        {
            Model.Render();
            Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            OVR.Atlas["vignette"].Draw(Vector2.Zero, Vector2.Zero, Color.White * 0.2f);
            Draw.SpriteBatch.End();
            if (!inFreeCameraDebugMode)
            {
                return;
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, null, null, null, Engine.ScreenMatrix);
            ActiveFont.DrawOutline(GetCameraString(), new Vector2(8f, 8f), Vector2.Zero, Vector2.One * 0.75f, Color.White, 2f, Color.Black);
            Draw.SpriteBatch.End();
        }

        public void SnapCamera(int area, MountainCamera transform, bool targetRotate = false)
        {
            Area = area;
            Animating = false;
            rotateAroundCenter = false;
            rotateAroundTarget = targetRotate;
            Model.Camera = transform;
            percent = 1f;
        }

        public void SnapState(int state)
        {
            Model.SnapState(state);
        }

        public float EaseCamera(
            int area,
            MountainCamera transform,
            float? duration = null,
            bool nearTarget = true,
            bool targetRotate = false)
        {
            if (Area != area && area >= 0)
            {
                PlayWhoosh(Area, area);
            }

            Area = area;
            percent = 0.0f;
            Animating = true;
            rotateAroundCenter = false;
            rotateAroundTarget = targetRotate;
            userOffset = Vector2.Zero;
            easeCameraFrom = Model.Camera;
            if (nearTarget)
            {
                easeCameraFrom.Target = easeCameraFrom.Position + ((easeCameraFrom.Target - easeCameraFrom.Position).SafeNormalize() * 0.5f);
            }

            easeCameraTo = transform;
            float radiansA = easeCameraFrom.Rotation.Forward().XZ().Angle();
            float radiansB = easeCameraTo.Rotation.Forward().XZ().Angle();
            float num1 = Calc.AngleDiff(radiansA, radiansB);
            float num2 = -Math.Sign(num1) * (6.28318548f - Math.Abs(num1));
            Vector3 between = GetBetween(easeCameraFrom.Position, easeCameraTo.Position, 0.5f);
            Vector2 vector1 = Calc.AngleToVector(MathHelper.Lerp(radiansA, radiansA + num1, 0.5f), 1f);
            Vector2 vector2 = Calc.AngleToVector(MathHelper.Lerp(radiansA, radiansA + num2, 0.5f), 1f);
            easeCameraRotationAngleTo = (double)(between + new Vector3(vector1.X, 0.0f, vector1.Y)).Length() >= (double)(between + new Vector3(vector2.X, 0.0f, vector2.Y)).Length() ? radiansA + num2 : radiansA + num1;
            this.duration = duration.HasValue ? duration.Value : GetDuration(easeCameraFrom, easeCameraTo);
            return this.duration;
        }

        public void EaseState(int state)
        {
            Model.EaseState(state);
        }

        public void GotoRotationMode()
        {
            if (rotateAroundCenter)
            {
                return;
            }

            rotateAroundCenter = true;
            rotateTimer = new Vector2(Model.Camera.Position.X, Model.Camera.Position.Z).Angle();
            Model.EaseState(0);
        }

        private float GetDuration(MountainCamera from, MountainCamera to)
        {
            double num = (double)Calc.AngleDiff(Calc.Angle(new Vector2(from.Position.X, from.Position.Z), new Vector2(from.Target.X, from.Target.Z)), Calc.Angle(new Vector2(to.Position.X, to.Position.Z), new Vector2(to.Target.X, to.Target.Z)));
            double val2 = Math.Sqrt((double)(from.Position - to.Position).Length()) / 3.0;
            return Calc.Clamp((float)(Math.Max((double)Math.Abs((float)num) * 0.5, val2) * 0.699999988079071), 0.3f, 1.1f);
        }

        private void PlayWhoosh(int from, int to)
        {
            string path = "";
            if (from == 0 && to == 1)
            {
                path = "event:/ui/world_map/whoosh/400ms_forward";
            }
            else if (from == 1 && to == 0)
            {
                path = "event:/ui/world_map/whoosh/400ms_back";
            }
            else if (from == 1 && to == 2)
            {
                path = "event:/ui/world_map/whoosh/600ms_forward";
            }
            else if (from == 2 && to == 1)
            {
                path = "event:/ui/world_map/whoosh/600ms_back";
            }
            else if (from < to && from > 1 && from < 7)
            {
                path = "event:/ui/world_map/whoosh/700ms_forward";
            }
            else if (from > to && from > 2 && from < 8)
            {
                path = "event:/ui/world_map/whoosh/700ms_back";
            }
            else if (from == 7 && to == 8)
            {
                path = "event:/ui/world_map/whoosh/1000ms_forward";
            }
            else if (from == 8 && to == 7)
            {
                path = "event:/ui/world_map/whoosh/1000ms_back";
            }
            else if (from == 8 && to == 9)
            {
                path = "event:/ui/world_map/whoosh/600ms_forward";
            }
            else if (from == 9 && to == 8)
            {
                path = "event:/ui/world_map/whoosh/600ms_back";
            }
            else if (from == 9 && to == 10)
            {
                path = "event:/ui/world_map/whoosh/1000ms_forward";
            }
            else if (from == 10 && to == 9)
            {
                path = "event:/ui/world_map/whoosh/1000ms_back";
            }

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _ = Audio.Play(path);
        }

        private string GetCameraString()
        {
            Vector3 position = Model.Camera.Position;
            Vector3 vector3 = position + (Vector3.Transform(Vector3.Forward, Model.Camera.Rotation.Conjugated()) * 2f);
            return "position=\"" + position.X.ToString("0.000") + ", " + position.Y.ToString("0.000") + ", " + position.Z.ToString("0.000") + "\" \ntarget=\"" + vector3.X.ToString("0.000") + ", " + vector3.Y.ToString("0.000") + ", " + vector3.Z.ToString("0.000") + "\"";
        }
    }
}
