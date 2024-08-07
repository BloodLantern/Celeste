﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Monocle
{
    public static class MInput
    {
        internal static List<VirtualInput> VirtualInputs;
        public static bool Active = true;
        public static bool Disabled = false;
        public static bool ControllerHasFocus;
        public static bool IsControllerFocused;

        public static KeyboardData Keyboard { get; private set; }

        public static MouseData Mouse { get; private set; }

        public static GamePadData[] GamePads { get; private set; }

        internal static void Initialize()
        {
            Keyboard = new KeyboardData();
            Mouse = new MouseData();
            GamePads = new GamePadData[4];
            for (int playerIndex = 0; playerIndex < 4; ++playerIndex)
                GamePads[playerIndex] = new GamePadData(playerIndex);
            VirtualInputs = new List<VirtualInput>();
        }

        internal static void Shutdown()
        {
            foreach (GamePadData gamePad in GamePads)
                gamePad.StopRumble();
        }

        internal static void Update()
        {
            if (Engine.Instance.IsActive && Active)
            {
                if (Engine.Commands.Open)
                {
                    Keyboard.UpdateNull();
                    Mouse.UpdateNull();
                }
                else
                {
                    Keyboard.Update();
                    Mouse.Update();
                }
                bool flag1 = false;
                bool flag2 = false;
                for (int index = 0; index < 4; ++index)
                {
                    GamePads[index].Update();
                    if (GamePads[index].HasAnyInput())
                    {
                        ControllerHasFocus = true;
                        flag1 = true;
                    }
                    if (GamePads[index].Attached)
                        flag2 = true;
                }
                if (!flag2 || !flag1 && Keyboard.HasAnyInput())
                    ControllerHasFocus = false;
            }
            else
            {
                Keyboard.UpdateNull();
                Mouse.UpdateNull();
                for (int index = 0; index < 4; ++index)
                    GamePads[index].UpdateNull();
            }
            UpdateVirtualInputs();
        }

        public static void UpdateNull()
        {
            Keyboard.UpdateNull();
            Mouse.UpdateNull();
            for (int index = 0; index < 4; ++index)
                GamePads[index].UpdateNull();
            UpdateVirtualInputs();
        }

        private static void UpdateVirtualInputs()
        {
            foreach (VirtualInput virtualInput in VirtualInputs)
                virtualInput.Update();
        }

        public static void RumbleFirst(float strength, float time) => GamePads[0].Rumble(strength, time);

        public static int Axis(bool negative, bool positive, int bothValue) => negative ? (positive ? bothValue : -1) : (positive ? 1 : 0);

        public static int Axis(float axisValue, float deadzone) => Math.Abs(axisValue) >= (double) deadzone ? Math.Sign(axisValue) : 0;

        public static int Axis(
            bool negative,
            bool positive,
            int bothValue,
            float axisValue,
            float deadzone)
        {
            int num = Axis(axisValue, deadzone);
            if (num == 0)
                num = Axis(negative, positive, bothValue);
            return num;
        }

        public class KeyboardData
        {
            public KeyboardState PreviousState;
            public KeyboardState CurrentState;

            internal KeyboardData()
            {
            }

            internal void Update()
            {
                PreviousState = CurrentState;
                CurrentState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            }

            internal void UpdateNull()
            {
                PreviousState = CurrentState;
                CurrentState = new KeyboardState();
                CurrentState.GetPressedKeys();
            }

            public bool HasAnyInput() => CurrentState.GetPressedKeys().Length != 0;

            public bool Check(Keys key) => !Disabled && key != Keys.None && CurrentState.IsKeyDown(key);

            public bool Pressed(Keys key) => !Disabled && key != Keys.None && CurrentState.IsKeyDown(key) && !PreviousState.IsKeyDown(key);

            public bool Released(Keys key) => !Disabled && key != Keys.None && !CurrentState.IsKeyDown(key) && PreviousState.IsKeyDown(key);

            public bool Check(Keys keyA, Keys keyB) => Check(keyA) || Check(keyB);

            public bool Pressed(Keys keyA, Keys keyB) => Pressed(keyA) || Pressed(keyB);

            public bool Released(Keys keyA, Keys keyB) => Released(keyA) || Released(keyB);

            public bool Check(Keys keyA, Keys keyB, Keys keyC) => Check(keyA) || Check(keyB) || Check(keyC);

            public bool Pressed(Keys keyA, Keys keyB, Keys keyC) => Pressed(keyA) || Pressed(keyB) || Pressed(keyC);

            public bool Released(Keys keyA, Keys keyB, Keys keyC) => Released(keyA) || Released(keyB) || Released(keyC);

            public int AxisCheck(Keys negative, Keys positive) => Check(negative) ? (Check(positive) ? 0 : -1) : (Check(positive) ? 1 : 0);

            public int AxisCheck(Keys negative, Keys positive, int both) => Check(negative) ? (Check(positive) ? both : -1) : (Check(positive) ? 1 : 0);
        }

        public class MouseData
        {
            public MouseState PreviousState;
            public MouseState CurrentState;

            internal MouseData()
            {
                PreviousState = new MouseState();
                CurrentState = new MouseState();
            }

            internal void Update()
            {
                PreviousState = CurrentState;
                CurrentState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }

            internal void UpdateNull()
            {
                PreviousState = CurrentState;
                CurrentState = new MouseState();
            }

            public bool CheckLeftButton => CurrentState.LeftButton == ButtonState.Pressed;

            public bool CheckRightButton => CurrentState.RightButton == ButtonState.Pressed;

            public bool CheckMiddleButton => CurrentState.MiddleButton == ButtonState.Pressed;

            public bool PressedLeftButton => CurrentState.LeftButton == ButtonState.Pressed && PreviousState.LeftButton == ButtonState.Released;

            public bool PressedRightButton => CurrentState.RightButton == ButtonState.Pressed && PreviousState.RightButton == ButtonState.Released;

            public bool PressedMiddleButton => CurrentState.MiddleButton == ButtonState.Pressed && PreviousState.MiddleButton == ButtonState.Released;

            public bool ReleasedLeftButton => CurrentState.LeftButton == ButtonState.Released && PreviousState.LeftButton == ButtonState.Pressed;

            public bool ReleasedRightButton => CurrentState.RightButton == ButtonState.Released && PreviousState.RightButton == ButtonState.Pressed;

            public bool ReleasedMiddleButton => CurrentState.MiddleButton == ButtonState.Released && PreviousState.MiddleButton == ButtonState.Pressed;

            public int Wheel => CurrentState.ScrollWheelValue;

            public int WheelDelta => CurrentState.ScrollWheelValue - PreviousState.ScrollWheelValue;

            public bool WasMoved => CurrentState.X != PreviousState.X || CurrentState.Y != PreviousState.Y;

            public float X
            {
                get => Position.X;
                set => Position = new Vector2(value, Position.Y);
            }

            public float Y
            {
                get => Position.Y;
                set => Position = new Vector2(Position.X, value);
            }

            public Vector2 Position
            {
                get => Vector2.Transform(new Vector2(CurrentState.X, CurrentState.Y), Matrix.Invert(Engine.ScreenMatrix));
                set
                {
                    Vector2 vector2 = Vector2.Transform(value, Engine.ScreenMatrix);
                    Microsoft.Xna.Framework.Input.Mouse.SetPosition((int) Math.Round(vector2.X), (int) Math.Round(vector2.Y));
                }
            }
        }

        public class GamePadData
        {
            public readonly PlayerIndex PlayerIndex;
            public GamePadState PreviousState;
            public GamePadState CurrentState;
            public bool Attached;
            public bool HadInputThisFrame;
            private float rumbleStrength;
            private float rumbleTime;

            internal GamePadData(int playerIndex) => PlayerIndex = (PlayerIndex) Calc.Clamp(playerIndex, 0, 3);

            public bool HasAnyInput()
            {
                if (!PreviousState.IsConnected && CurrentState.IsConnected || PreviousState.Buttons != CurrentState.Buttons || PreviousState.DPad != CurrentState.DPad || CurrentState.Triggers.Left > 0.0099999997764825821 || CurrentState.Triggers.Right > 0.0099999997764825821)
                    return true;
                Vector2 vector2 = CurrentState.ThumbSticks.Left;
                if (vector2.Length() <= 0.0099999997764825821)
                {
                    vector2 = CurrentState.ThumbSticks.Right;
                    if (vector2.Length() <= 0.0099999997764825821)
                        return false;
                }
                return true;
            }

            public void Update()
            {
                PreviousState = CurrentState;
                CurrentState = GamePad.GetState(PlayerIndex);
                if (!Attached && CurrentState.IsConnected)
                    IsControllerFocused = true;
                Attached = CurrentState.IsConnected;
                if (rumbleTime <= 0.0)
                    return;
                rumbleTime -= Engine.DeltaTime;
                if (rumbleTime > 0.0)
                    return;
                GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
            }

            public void UpdateNull()
            {
                PreviousState = CurrentState;
                CurrentState = new GamePadState();
                Attached = GamePad.GetState(PlayerIndex).IsConnected;
                if (rumbleTime > 0.0)
                    rumbleTime -= Engine.DeltaTime;
                GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
            }

            public void Rumble(float strength, float time)
            {
                if (rumbleTime > 0.0 && (double) strength <= rumbleStrength && ((double) strength != rumbleStrength || (double) time <= rumbleTime))
                    return;
                GamePad.SetVibration(PlayerIndex, strength, strength);
                rumbleStrength = strength;
                rumbleTime = time;
            }

            public void StopRumble()
            {
                GamePad.SetVibration(PlayerIndex, 0.0f, 0.0f);
                rumbleTime = 0.0f;
            }

            public bool Check(Buttons button) => !Disabled && CurrentState.IsButtonDown(button);

            public bool Pressed(Buttons button) => !Disabled && CurrentState.IsButtonDown(button) && PreviousState.IsButtonUp(button);

            public bool Released(Buttons button) => !Disabled && CurrentState.IsButtonUp(button) && PreviousState.IsButtonDown(button);

            public bool Check(Buttons buttonA, Buttons buttonB) => Check(buttonA) || Check(buttonB);

            public bool Pressed(Buttons buttonA, Buttons buttonB) => Pressed(buttonA) || Pressed(buttonB);

            public bool Released(Buttons buttonA, Buttons buttonB) => Released(buttonA) || Released(buttonB);

            public bool Check(Buttons buttonA, Buttons buttonB, Buttons buttonC) => Check(buttonA) || Check(buttonB) || Check(buttonC);

            public bool Pressed(Buttons buttonA, Buttons buttonB, Buttons buttonC) => Pressed(buttonA) || Pressed(buttonB) || Check(buttonC);

            public bool Released(Buttons buttonA, Buttons buttonB, Buttons buttonC) => Released(buttonA) || Released(buttonB) || Check(buttonC);

            public Vector2 GetLeftStick()
            {
                Vector2 left = CurrentState.ThumbSticks.Left;
                left.Y = -left.Y;
                return left;
            }

            public Vector2 GetLeftStick(float deadzone)
            {
                Vector2 leftStick = CurrentState.ThumbSticks.Left;
                if (leftStick.LengthSquared() < deadzone * (double) deadzone)
                    leftStick = Vector2.Zero;
                else
                    leftStick.Y = -leftStick.Y;
                return leftStick;
            }

            public Vector2 GetRightStick()
            {
                Vector2 right = CurrentState.ThumbSticks.Right;
                right.Y = -right.Y;
                return right;
            }

            public Vector2 GetRightStick(float deadzone)
            {
                Vector2 rightStick = CurrentState.ThumbSticks.Right;
                if (rightStick.LengthSquared() < deadzone * (double) deadzone)
                    rightStick = Vector2.Zero;
                else
                    rightStick.Y = -rightStick.Y;
                return rightStick;
            }

            public bool LeftStickLeftCheck(float deadzone) => CurrentState.ThumbSticks.Left.X <= -(double) deadzone;

            public bool LeftStickLeftPressed(float deadzone) => CurrentState.ThumbSticks.Left.X <= -(double) deadzone && PreviousState.ThumbSticks.Left.X > -(double) deadzone;

            public bool LeftStickLeftReleased(float deadzone) => CurrentState.ThumbSticks.Left.X > -(double) deadzone && PreviousState.ThumbSticks.Left.X <= -(double) deadzone;

            public bool LeftStickRightCheck(float deadzone) => CurrentState.ThumbSticks.Left.X >= (double) deadzone;

            public bool LeftStickRightPressed(float deadzone) => CurrentState.ThumbSticks.Left.X >= (double) deadzone && PreviousState.ThumbSticks.Left.X < (double) deadzone;

            public bool LeftStickRightReleased(float deadzone) => CurrentState.ThumbSticks.Left.X < (double) deadzone && PreviousState.ThumbSticks.Left.X >= (double) deadzone;

            public bool LeftStickDownCheck(float deadzone) => CurrentState.ThumbSticks.Left.Y <= -(double) deadzone;

            public bool LeftStickDownPressed(float deadzone) => CurrentState.ThumbSticks.Left.Y <= -(double) deadzone && PreviousState.ThumbSticks.Left.Y > -(double) deadzone;

            public bool LeftStickDownReleased(float deadzone) => CurrentState.ThumbSticks.Left.Y > -(double) deadzone && PreviousState.ThumbSticks.Left.Y <= -(double) deadzone;

            public bool LeftStickUpCheck(float deadzone) => CurrentState.ThumbSticks.Left.Y >= (double) deadzone;

            public bool LeftStickUpPressed(float deadzone) => CurrentState.ThumbSticks.Left.Y >= (double) deadzone && PreviousState.ThumbSticks.Left.Y < (double) deadzone;

            public bool LeftStickUpReleased(float deadzone) => CurrentState.ThumbSticks.Left.Y < (double) deadzone && PreviousState.ThumbSticks.Left.Y >= (double) deadzone;

            public float LeftStickHorizontal(float deadzone)
            {
                float x = CurrentState.ThumbSticks.Left.X;
                return Math.Abs(x) < (double) deadzone ? 0.0f : x;
            }

            public float LeftStickVertical(float deadzone)
            {
                float y = CurrentState.ThumbSticks.Left.Y;
                return Math.Abs(y) < (double) deadzone ? 0.0f : -y;
            }

            public bool RightStickLeftCheck(float deadzone) => CurrentState.ThumbSticks.Right.X <= -(double) deadzone;

            public bool RightStickLeftPressed(float deadzone) => CurrentState.ThumbSticks.Right.X <= -(double) deadzone && PreviousState.ThumbSticks.Right.X > -(double) deadzone;

            public bool RightStickLeftReleased(float deadzone) => CurrentState.ThumbSticks.Right.X > -(double) deadzone && PreviousState.ThumbSticks.Right.X <= -(double) deadzone;

            public bool RightStickRightCheck(float deadzone) => CurrentState.ThumbSticks.Right.X >= (double) deadzone;

            public bool RightStickRightPressed(float deadzone) => CurrentState.ThumbSticks.Right.X >= (double) deadzone && PreviousState.ThumbSticks.Right.X < (double) deadzone;

            public bool RightStickRightReleased(float deadzone) => CurrentState.ThumbSticks.Right.X < (double) deadzone && PreviousState.ThumbSticks.Right.X >= (double) deadzone;

            public bool RightStickDownCheck(float deadzone) => CurrentState.ThumbSticks.Right.Y <= -(double) deadzone;

            public bool RightStickDownPressed(float deadzone) => CurrentState.ThumbSticks.Right.Y <= -(double) deadzone && PreviousState.ThumbSticks.Right.Y > -(double) deadzone;

            public bool RightStickDownReleased(float deadzone) => CurrentState.ThumbSticks.Right.Y > -(double) deadzone && PreviousState.ThumbSticks.Right.Y <= -(double) deadzone;

            public bool RightStickUpCheck(float deadzone) => CurrentState.ThumbSticks.Right.Y >= (double) deadzone;

            public bool RightStickUpPressed(float deadzone) => CurrentState.ThumbSticks.Right.Y >= (double) deadzone && PreviousState.ThumbSticks.Right.Y < (double) deadzone;

            public bool RightStickUpReleased(float deadzone) => CurrentState.ThumbSticks.Right.Y < (double) deadzone && PreviousState.ThumbSticks.Right.Y >= (double) deadzone;

            public float RightStickHorizontal(float deadzone)
            {
                float x = CurrentState.ThumbSticks.Right.X;
                return Math.Abs(x) < (double) deadzone ? 0.0f : x;
            }

            public float RightStickVertical(float deadzone)
            {
                float y = CurrentState.ThumbSticks.Right.Y;
                return Math.Abs(y) < (double) deadzone ? 0.0f : -y;
            }

            public int DPadHorizontal
            {
                get
                {
                    if (CurrentState.DPad.Right == ButtonState.Pressed)
                        return 1;
                    return CurrentState.DPad.Left != ButtonState.Pressed ? 0 : -1;
                }
            }

            public int DPadVertical
            {
                get
                {
                    if (CurrentState.DPad.Down == ButtonState.Pressed)
                        return 1;
                    return CurrentState.DPad.Up != ButtonState.Pressed ? 0 : -1;
                }
            }

            public Vector2 DPad => new(DPadHorizontal, DPadVertical);

            public bool DPadLeftCheck => CurrentState.DPad.Left == ButtonState.Pressed;

            public bool DPadLeftPressed => CurrentState.DPad.Left == ButtonState.Pressed && PreviousState.DPad.Left == ButtonState.Released;

            public bool DPadLeftReleased => CurrentState.DPad.Left == ButtonState.Released && PreviousState.DPad.Left == ButtonState.Pressed;

            public bool DPadRightCheck => CurrentState.DPad.Right == ButtonState.Pressed;

            public bool DPadRightPressed => CurrentState.DPad.Right == ButtonState.Pressed && PreviousState.DPad.Right == ButtonState.Released;

            public bool DPadRightReleased => CurrentState.DPad.Right == ButtonState.Released && PreviousState.DPad.Right == ButtonState.Pressed;

            public bool DPadUpCheck => CurrentState.DPad.Up == ButtonState.Pressed;

            public bool DPadUpPressed => CurrentState.DPad.Up == ButtonState.Pressed && PreviousState.DPad.Up == ButtonState.Released;

            public bool DPadUpReleased => CurrentState.DPad.Up == ButtonState.Released && PreviousState.DPad.Up == ButtonState.Pressed;

            public bool DPadDownCheck => CurrentState.DPad.Down == ButtonState.Pressed;

            public bool DPadDownPressed => CurrentState.DPad.Down == ButtonState.Pressed && PreviousState.DPad.Down == ButtonState.Released;

            public bool DPadDownReleased => CurrentState.DPad.Down == ButtonState.Released && PreviousState.DPad.Down == ButtonState.Pressed;

            public bool LeftTriggerCheck(float threshold) => !Disabled && CurrentState.Triggers.Left >= (double) threshold;

            public bool LeftTriggerPressed(float threshold) => !Disabled && CurrentState.Triggers.Left >= (double) threshold && PreviousState.Triggers.Left < (double) threshold;

            public bool LeftTriggerReleased(float threshold) => !Disabled && CurrentState.Triggers.Left < (double) threshold && PreviousState.Triggers.Left >= (double) threshold;

            public bool RightTriggerCheck(float threshold) => !Disabled && CurrentState.Triggers.Right >= (double) threshold;

            public bool RightTriggerPressed(float threshold) => !Disabled && CurrentState.Triggers.Right >= (double) threshold && PreviousState.Triggers.Right < (double) threshold;

            public bool RightTriggerReleased(float threshold) => !Disabled && CurrentState.Triggers.Right < (double) threshold && PreviousState.Triggers.Right >= (double) threshold;

            public float Axis(Buttons button, float threshold)
            {
                if (Disabled)
                    return 0.0f;
                switch (button)
                {
                    case Buttons.DPadUp:
                    case Buttons.DPadDown:
                    case Buttons.DPadLeft:
                    case Buttons.DPadRight:
                    case Buttons.Start:
                    case Buttons.Back:
                    case Buttons.LeftStick:
                    case Buttons.RightStick:
                    case Buttons.LeftShoulder:
                    case Buttons.RightShoulder:
                    case Buttons.A:
                    case Buttons.B:
                    case Buttons.X:
                    case Buttons.Y:
                        if (Check(button))
                            return 1f;
                        break;
                    case Buttons.LeftThumbstickLeft:
                        if (CurrentState.ThumbSticks.Left.X <= -(double) threshold)
                            return -CurrentState.ThumbSticks.Left.X;
                        break;
                    case Buttons.RightTrigger:
                        if (CurrentState.Triggers.Right >= (double) threshold)
                            return CurrentState.Triggers.Right;
                        break;
                    case Buttons.LeftTrigger:
                        if (CurrentState.Triggers.Left >= (double) threshold)
                            return CurrentState.Triggers.Left;
                        break;
                    case Buttons.RightThumbstickUp:
                        if (CurrentState.ThumbSticks.Right.Y >= (double) threshold)
                            return CurrentState.ThumbSticks.Right.Y;
                        break;
                    case Buttons.RightThumbstickDown:
                        if (CurrentState.ThumbSticks.Right.Y <= -(double) threshold)
                            return -CurrentState.ThumbSticks.Right.Y;
                        break;
                    case Buttons.RightThumbstickRight:
                        if (CurrentState.ThumbSticks.Right.X >= (double) threshold)
                            return CurrentState.ThumbSticks.Right.X;
                        break;
                    case Buttons.RightThumbstickLeft:
                        if (CurrentState.ThumbSticks.Right.X <= -(double) threshold)
                            return -CurrentState.ThumbSticks.Right.X;
                        break;
                    case Buttons.LeftThumbstickUp:
                        if (CurrentState.ThumbSticks.Left.Y >= (double) threshold)
                            return CurrentState.ThumbSticks.Left.Y;
                        break;
                    case Buttons.LeftThumbstickDown:
                        if (CurrentState.ThumbSticks.Left.Y <= -(double) threshold)
                            return -CurrentState.ThumbSticks.Left.Y;
                        break;
                    case Buttons.LeftThumbstickRight:
                        if (CurrentState.ThumbSticks.Left.X >= (double) threshold)
                            return CurrentState.ThumbSticks.Left.X;
                        break;
                }
                return 0.0f;
            }

            public bool Check(Buttons button, float threshold)
            {
                if (Disabled)
                    return false;
                switch (button)
                {
                    case Buttons.DPadUp:
                    case Buttons.DPadDown:
                    case Buttons.DPadLeft:
                    case Buttons.DPadRight:
                    case Buttons.Start:
                    case Buttons.Back:
                    case Buttons.LeftStick:
                    case Buttons.RightStick:
                    case Buttons.LeftShoulder:
                    case Buttons.RightShoulder:
                    case Buttons.A:
                    case Buttons.B:
                    case Buttons.X:
                    case Buttons.Y:
                        if (Check(button))
                            return true;
                        break;
                    case Buttons.LeftThumbstickLeft:
                        if (LeftStickLeftCheck(threshold))
                            return true;
                        break;
                    case Buttons.RightTrigger:
                        if (RightTriggerCheck(threshold))
                            return true;
                        break;
                    case Buttons.LeftTrigger:
                        if (LeftTriggerCheck(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickUp:
                        if (RightStickUpCheck(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickDown:
                        if (RightStickDownCheck(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickRight:
                        if (RightStickRightCheck(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickLeft:
                        if (RightStickLeftCheck(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickUp:
                        if (LeftStickUpCheck(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickDown:
                        if (LeftStickDownCheck(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickRight:
                        if (LeftStickRightCheck(threshold))
                            return true;
                        break;
                }
                return false;
            }

            public bool Pressed(Buttons button, float threshold)
            {
                if (Disabled)
                    return false;
                switch (button)
                {
                    case Buttons.DPadUp:
                    case Buttons.DPadDown:
                    case Buttons.DPadLeft:
                    case Buttons.DPadRight:
                    case Buttons.Start:
                    case Buttons.Back:
                    case Buttons.LeftStick:
                    case Buttons.RightStick:
                    case Buttons.LeftShoulder:
                    case Buttons.RightShoulder:
                    case Buttons.A:
                    case Buttons.B:
                    case Buttons.X:
                    case Buttons.Y:
                        if (Pressed(button))
                            return true;
                        break;
                    case Buttons.LeftThumbstickLeft:
                        if (LeftStickLeftPressed(threshold))
                            return true;
                        break;
                    case Buttons.RightTrigger:
                        if (RightTriggerPressed(threshold))
                            return true;
                        break;
                    case Buttons.LeftTrigger:
                        if (LeftTriggerPressed(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickUp:
                        if (RightStickUpPressed(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickDown:
                        if (RightStickDownPressed(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickRight:
                        if (RightStickRightPressed(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickLeft:
                        if (RightStickLeftPressed(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickUp:
                        if (LeftStickUpPressed(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickDown:
                        if (LeftStickDownPressed(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickRight:
                        if (LeftStickRightPressed(threshold))
                            return true;
                        break;
                }
                return false;
            }

            public bool Released(Buttons button, float threshold)
            {
                if (Disabled)
                    return false;
                switch (button)
                {
                    case Buttons.DPadUp:
                    case Buttons.DPadDown:
                    case Buttons.DPadLeft:
                    case Buttons.DPadRight:
                    case Buttons.Start:
                    case Buttons.Back:
                    case Buttons.LeftStick:
                    case Buttons.RightStick:
                    case Buttons.LeftShoulder:
                    case Buttons.RightShoulder:
                    case Buttons.A:
                    case Buttons.B:
                    case Buttons.X:
                    case Buttons.Y:
                        if (Released(button))
                            return true;
                        break;
                    case Buttons.LeftThumbstickLeft:
                        if (LeftStickLeftReleased(threshold))
                            return true;
                        break;
                    case Buttons.RightTrigger:
                        if (RightTriggerReleased(threshold))
                            return true;
                        break;
                    case Buttons.LeftTrigger:
                        if (LeftTriggerReleased(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickUp:
                        if (RightStickUpReleased(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickDown:
                        if (RightStickDownReleased(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickRight:
                        if (RightStickRightReleased(threshold))
                            return true;
                        break;
                    case Buttons.RightThumbstickLeft:
                        if (RightStickLeftReleased(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickUp:
                        if (LeftStickUpReleased(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickDown:
                        if (LeftStickDownReleased(threshold))
                            return true;
                        break;
                    case Buttons.LeftThumbstickRight:
                        if (LeftStickRightReleased(threshold))
                            return true;
                        break;
                }
                return false;
            }
        }
    }
}
