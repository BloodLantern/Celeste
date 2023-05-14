// Decompiled with JetBrains decompiler
// Type: Celeste.SpeedrunTimerDisplay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class SpeedrunTimerDisplay : Entity
    {
        public float CompleteTimer;
        public const int GuiChapterHeight = 58;
        public const int GuiFileHeight = 78;
        private static float numberWidth;
        private static float spacerWidth;
        private readonly MTexture bg = GFX.Gui["strawberryCountBG"];
        public float DrawLerp;
        private readonly Wiggler wiggler;

        public SpeedrunTimerDisplay()
        {
            Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
            Depth = -100;
            Y = 60f;
            SpeedrunTimerDisplay.CalculateBaseSizes();
            Add(wiggler = Wiggler.Create(0.5f, 4f));
        }

        public static void CalculateBaseSizes()
        {
            PixelFontSize pixelFontSize = Dialog.Languages["english"].Font.Get(Dialog.Languages["english"].FontFaceSize);
            for (int index = 0; index < 10; ++index)
            {
                float x = pixelFontSize.Measure(index.ToString()).X;
                if ((double)x > numberWidth)
                {
                    SpeedrunTimerDisplay.numberWidth = x;
                }
            }
            SpeedrunTimerDisplay.spacerWidth = pixelFontSize.Measure('.').X;
        }

        public override void Update()
        {
            Level scene = Scene as Level;
            if (scene.Completed)
            {
                if (CompleteTimer == 0.0)
                {
                    wiggler.Start();
                }

                CompleteTimer += Engine.DeltaTime;
            }
            bool flag = false;
            if (scene.Session.Area.ID != 8 && !scene.TimerHidden)
            {
                if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
                {
                    if (CompleteTimer < 3.0)
                    {
                        flag = true;
                    }
                }
                else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                {
                    flag = true;
                }
            }
            DrawLerp = Calc.Approach(DrawLerp, flag ? 1f : 0.0f, Engine.DeltaTime * 4f);
            base.Update();
        }

        public override void Render()
        {
            if (DrawLerp <= 0.0)
            {
                return;
            }

            float x = -300f * Ease.CubeIn(1f - DrawLerp);
            Level scene = Scene as Level;
            Session session = scene.Session;
            if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
            {
                string timeString = TimeSpan.FromTicks(session.Time).ShortGameplayFormat();
                bg.Draw(new Vector2(x, Y));
                SpeedrunTimerDisplay.DrawTime(new Vector2(x + 32f, Y + 44f), timeString, (float)(1.0 + ((double)wiggler.Value * 0.15000000596046448)), session.StartedFromBeginning, scene.Completed, session.BeatBestTime);
            }
            else
            {
                if (Settings.Instance.SpeedrunClock != SpeedrunType.File)
                {
                    return;
                }

                TimeSpan timeSpan1 = TimeSpan.FromTicks(session.Time);
                string timeString1 = timeSpan1.TotalHours < 1.0 ? timeSpan1.ToString("mm\\:ss") : ((int)timeSpan1.TotalHours).ToString() + ":" + timeSpan1.ToString("mm\\:ss");
                TimeSpan timeSpan2 = TimeSpan.FromTicks(SaveData.Instance.Time);
                int totalHours = (int)timeSpan2.TotalHours;
                string timeString2 = totalHours.ToString() + timeSpan2.ToString("\\:mm\\:ss\\.fff");
                int num = totalHours < 10 ? 64 : (totalHours < 100 ? 96 : 128);
                Draw.Rect(x, Y, num + 2, 38f, Color.Black);
                bg.Draw(new Vector2(x + num, Y));
                SpeedrunTimerDisplay.DrawTime(new Vector2(x + 32f, Y + 44f), timeString2);
                bg.Draw(new Vector2(x, Y + 38f), Vector2.Zero, Color.White, 0.6f);
                SpeedrunTimerDisplay.DrawTime(new Vector2(x + 32f, (float)((double)Y + 40.0 + 26.400001525878906)), timeString1, (float)((1.0 + ((double)wiggler.Value * 0.15000000596046448)) * 0.60000002384185791), session.StartedFromBeginning, scene.Completed, session.BeatBestTime, 0.6f);
            }
        }

        public static void DrawTime(
            Vector2 position,
            string timeString,
            float scale = 1f,
            bool valid = true,
            bool finished = false,
            bool bestTime = false,
            float alpha = 1f)
        {
            PixelFont font = Dialog.Languages["english"].Font;
            float fontFaceSize = Dialog.Languages["english"].FontFaceSize;
            float num1 = scale;
            float x = position.X;
            float y = position.Y;
            Color color1 = Color.White * alpha;
            Color color2 = Color.LightGray * alpha;
            if (!valid)
            {
                color1 = Calc.HexToColor("918988") * alpha;
                color2 = Calc.HexToColor("7a6f6d") * alpha;
            }
            else if (bestTime)
            {
                color1 = Calc.HexToColor("fad768") * alpha;
                color2 = Calc.HexToColor("cfa727") * alpha;
            }
            else if (finished)
            {
                color1 = Calc.HexToColor("6ded87") * alpha;
                color2 = Calc.HexToColor("43d14c") * alpha;
            }
            for (int index = 0; index < timeString.Length; ++index)
            {
                char ch = timeString[index];
                if (ch == '.')
                {
                    num1 = scale * 0.7f;
                    y -= 5f * scale;
                }
                Color color3 = ch == ':' || ch == '.' || (double)num1 < (double)scale ? color2 : color1;
                float num2 = (float)((ch is ':' or '.' ? spacerWidth : numberWidth) + 4.0) * num1;
                font.DrawOutline(fontFaceSize, ch.ToString(), new Vector2(x + (num2 / 2f), y), new Vector2(0.5f, 1f), Vector2.One * num1, color3, 2f, Color.Black);
                x += num2;
            }
        }

        public static float GetTimeWidth(string timeString, float scale = 1f)
        {
            float num1 = scale;
            float timeWidth = 0.0f;
            for (int index = 0; index < timeString.Length; ++index)
            {
                char ch = timeString[index];
                if (ch == '.')
                {
                    num1 = scale * 0.7f;
                }

                float num2 = (float)((ch is ':' or '.' ? spacerWidth : numberWidth) + 4.0) * num1;
                timeWidth += num2;
            }
            return timeWidth;
        }
    }
}
