using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class InputMappingInfo : TextMenu.Item
    {
        private List<object> info = new List<object>();
        private bool controllerMode;
        private float borderEase;
        private bool fixedPosition;

        public InputMappingInfo(bool controllerMode)
        {
            string[] strArray = Dialog.Clean("BTN_CONFIG_INFO").Split('|');
            if (strArray.Length == 3)
            {
                info.Add(strArray[0]);
                info.Add(Input.MenuConfirm);
                info.Add(strArray[1]);
                info.Add(Input.MenuJournal);
                info.Add(strArray[2]);
            }
            this.controllerMode = controllerMode;
            AboveAll = true;
        }

        public override float LeftWidth() => 100f;

        public override float Height() => ActiveFont.LineHeight * 2f;

        public override void Update()
        {
            borderEase = Calc.Approach(borderEase, fixedPosition ? 1f : 0.0f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render(Vector2 position, bool highlighted)
        {
            fixedPosition = false;
            if (position.Y < 100.0)
            {
                fixedPosition = true;
                position.Y = 100f;
            }
            Color color1 = Color.Gray * Ease.CubeOut(Container.Alpha);
            Color strokeColor = Color.Black * Ease.CubeOut(Container.Alpha);
            Color color2 = Color.White * Ease.CubeOut(Container.Alpha);
            float num = 0.0f;
            for (int index = 0; index < info.Count; ++index)
            {
                if (info[index] is string)
                {
                    string text = info[index] as string;
                    num += ActiveFont.Measure(text).X * 0.6f;
                }
                else if (info[index] is VirtualButton)
                {
                    VirtualButton button = info[index] as VirtualButton;
                    if (controllerMode)
                    {
                        MTexture mtexture = Input.GuiButton(button, Input.PrefixMode.Attached);
                        num += mtexture.Width * 0.6f;
                    }
                    else if (button.Binding.Keyboard.Count > 0)
                    {
                        MTexture mtexture = Input.GuiKey(button.Binding.Keyboard[0]);
                        num += mtexture.Width * 0.6f;
                    }
                    else
                    {
                        MTexture mtexture = Input.GuiKey(Keys.None);
                        num += mtexture.Width * 0.6f;
                    }
                }
            }
            Vector2 position1 = position + new Vector2(Container.Width - num, 0.0f) / 2f;
            if (borderEase > 0.0)
            {
                Draw.HollowRect(position1.X - 22f, position1.Y - 42f, num + 44f, 84f, Color.White * Ease.CubeOut(Container.Alpha) * borderEase);
                Draw.HollowRect(position1.X - 21f, position1.Y - 41f, num + 42f, 82f, Color.White * Ease.CubeOut(Container.Alpha) * borderEase);
                Draw.Rect(position1.X - 20f, position1.Y - 40f, num + 40f, 80f, Color.Black * Ease.CubeOut(Container.Alpha));
            }
            for (int index = 0; index < info.Count; ++index)
            {
                if (info[index] is string)
                {
                    string text = info[index] as string;
                    ActiveFont.DrawOutline(text, position1, new Vector2(0.0f, 0.5f), Vector2.One * 0.6f, color1, 2f, strokeColor);
                    position1.X += ActiveFont.Measure(text).X * 0.6f;
                }
                else if (info[index] is VirtualButton)
                {
                    VirtualButton button = info[index] as VirtualButton;
                    if (controllerMode)
                    {
                        MTexture mtexture = Input.GuiButton(button, Input.PrefixMode.Attached);
                        mtexture.DrawJustified(position1, new Vector2(0.0f, 0.5f), color2, 0.6f);
                        position1.X += mtexture.Width * 0.6f;
                    }
                    else if (button.Binding.Keyboard.Count > 0)
                    {
                        MTexture mtexture = Input.GuiKey(button.Binding.Keyboard[0]);
                        mtexture.DrawJustified(position1, new Vector2(0.0f, 0.5f), color2, 0.6f);
                        position1.X += mtexture.Width * 0.6f;
                    }
                    else
                    {
                        MTexture mtexture = Input.GuiKey(Keys.None);
                        mtexture.DrawJustified(position1, new Vector2(0.0f, 0.5f), color2, 0.6f);
                        position1.X += mtexture.Width * 0.6f;
                    }
                }
            }
        }
    }
}
