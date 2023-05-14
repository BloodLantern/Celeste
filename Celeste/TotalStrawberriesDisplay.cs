// Decompiled with JetBrains decompiler
// Type: Celeste.TotalStrawberriesDisplay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class TotalStrawberriesDisplay : Entity
    {
        private const float NumberUpdateDelay = 0.4f;
        private const float ComboUpdateDelay = 0.3f;
        private const float AfterUpdateDelay = 2f;
        private const float LerpInSpeed = 1.2f;
        private const float LerpOutSpeed = 2f;
        public static readonly Color FlashColor = Calc.HexToColor("FF5E76");
        private readonly MTexture bg;
        public float DrawLerp;
        private float strawberriesUpdateTimer;
        private float strawberriesWaitTimer;
        private readonly StrawberriesCounter strawberries;

        public TotalStrawberriesDisplay()
        {
            Y = 96f;
            Depth = -101;
            Tag = (int)Tags.HUD | (int)Tags.Global | (int)Tags.PauseUpdate | (int)Tags.TransitionUpdate;
            bg = GFX.Gui["strawberryCountBG"];
            Add(strawberries = new StrawberriesCounter(false, SaveData.Instance.TotalStrawberries));
        }

        public override void Update()
        {
            base.Update();
            Level scene = Scene as Level;
            if (SaveData.Instance.TotalStrawberries > strawberries.Amount && strawberriesUpdateTimer <= 0.0)
            {
                strawberriesUpdateTimer = 0.4f;
            }

            DrawLerp = SaveData.Instance.TotalStrawberries > strawberries.Amount || strawberriesUpdateTimer > 0.0 || strawberriesWaitTimer > 0.0 || (scene.Paused && scene.PauseMainMenuOpen) ? Calc.Approach(DrawLerp, 1f, 1.2f * Engine.RawDeltaTime) : Calc.Approach(DrawLerp, 0.0f, 2f * Engine.RawDeltaTime);
            if (strawberriesWaitTimer > 0.0)
            {
                strawberriesWaitTimer -= Engine.RawDeltaTime;
            }

            if (strawberriesUpdateTimer > 0.0 && DrawLerp == 1.0)
            {
                strawberriesUpdateTimer -= Engine.RawDeltaTime;
                if (strawberriesUpdateTimer <= 0.0)
                {
                    if (strawberries.Amount < SaveData.Instance.TotalStrawberries)
                    {
                        ++strawberries.Amount;
                    }

                    strawberriesWaitTimer = 2f;
                    if (strawberries.Amount < SaveData.Instance.TotalStrawberries)
                    {
                        strawberriesUpdateTimer = 0.3f;
                    }
                }
            }
            if (Visible)
            {
                float target = 96f;
                if (!scene.TimerHidden)
                {
                    if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
                    {
                        target += 58f;
                    }
                    else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                    {
                        target += 78f;
                    }
                }
                Y = Calc.Approach(Y, target, Engine.DeltaTime * 800f);
            }
            Visible = DrawLerp > 0.0;
        }

        public override void Render()
        {
            Vector2 vector2 = Vector2.Lerp(new Vector2(-bg.Width, Y), new Vector2(32f, Y), Ease.CubeOut(DrawLerp)).Round();
            bg.DrawJustified(vector2 + new Vector2(-96f, 12f), new Vector2(0.0f, 0.5f));
            strawberries.Position = vector2 + new Vector2(0.0f, -Y);
            strawberries.Render();
        }
    }
}
