// Decompiled with JetBrains decompiler
// Type: Celeste.QuickResetHint
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class QuickResetHint : Entity
    {
        private readonly string textStart;
        private readonly string textHold;
        private readonly string textPress;
        private readonly List<object> controllerList;
        private readonly List<object> keyboardList;

        public QuickResetHint()
        {
            Tag = (int)Tags.HUD;
            Buttons buttons1 = Buttons.LeftShoulder;
            Buttons buttons2 = Buttons.RightShoulder;
            textStart = Dialog.Clean("UI_QUICK_RESTART_TITLE") + " ";
            textHold = Dialog.Clean("UI_QUICK_RESTART_HOLD");
            textPress = Dialog.Clean("UI_QUICK_RESTART_PRESS");
            if (Settings.Instance.Language == "japanese")
            {
                controllerList = new List<object>()
                {
                     textStart,
                     buttons1,
                     buttons2,
                     textHold,
                     "、",
                     Input.FirstButton(Input.Pause),
                     textPress
                };
                keyboardList = new List<object>()
                {
                     textStart,
                     Input.FirstKey(Input.QuickRestart),
                     textPress
                };
            }
            else
            {
                controllerList = new List<object>()
                {
                     textStart,
                     textHold,
                     buttons1,
                     buttons2,
                     ",  ",
                     textPress,
                     Input.FirstButton(Input.Pause)
                };
                keyboardList = new List<object>()
                {
                     textStart,
                     textPress,
                     Input.FirstKey(Input.QuickRestart)
                };
            }
        }

        public override void Render()
        {
            List<object> objectList = Input.GuiInputController() ? controllerList : keyboardList;
            float num = 0.0f;
            foreach (object text in objectList)
            {
                switch (text)
                {
                    case string _:
                        num += ActiveFont.Measure(text as string).X;
                        continue;
                    case Buttons button:
                        num += Input.GuiSingleButton(button).Width + 16f;
                        continue;
                    case Keys key:
                        num += Input.GuiKey(key).Width + 16f;
                        continue;
                    default:
                        continue;
                }
            }
            Vector2 position = new((float)((1920.0 - (double)(num * 0.75f)) / 2.0), 980f);
            foreach (object text in objectList)
            {
                switch (text)
                {
                    case string _:
                        ActiveFont.DrawOutline(text as string, position, new Vector2(0.0f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
                        position.X += ActiveFont.Measure(text as string).X * 0.75f;
                        continue;
                    case Buttons button:
                        MTexture mtexture1 = Input.GuiSingleButton(button);
                        mtexture1.DrawJustified(position + new Vector2((float)((mtexture1.Width + 16.0) * 0.75 * 0.5), 0.0f), new Vector2(0.5f, 0.5f), Color.White, 0.75f);
                        position.X += (float)((mtexture1.Width + 16.0) * 0.75);
                        continue;
                    case Keys key:
                        MTexture mtexture2 = Input.GuiKey(key);
                        mtexture2.DrawJustified(position + new Vector2((float)((mtexture2.Width + 16.0) * 0.75 * 0.5), 0.0f), new Vector2(0.5f, 0.5f), Color.White, 0.75f);
                        position.X += (float)((mtexture2.Width + 16.0) * 0.75);
                        continue;
                    default:
                        continue;
                }
            }
        }
    }
}
