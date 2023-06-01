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
        private MTexture bg;
        public float DrawLerp;
        private float strawberriesUpdateTimer;
        private float strawberriesWaitTimer;
        private StrawberriesCounter strawberries;

        public TotalStrawberriesDisplay()
        {
            this.Y = 96f;
            this.Depth = -101;
            this.Tag = (int) Tags.HUD | (int) Tags.Global | (int) Tags.PauseUpdate | (int) Tags.TransitionUpdate;
            this.bg = GFX.Gui["strawberryCountBG"];
            this.Add((Component) (this.strawberries = new StrawberriesCounter(false, SaveData.Instance.TotalStrawberries)));
        }

        public override void Update()
        {
            base.Update();
            Level scene = this.Scene as Level;
            if (SaveData.Instance.TotalStrawberries > this.strawberries.Amount && (double) this.strawberriesUpdateTimer <= 0.0)
                this.strawberriesUpdateTimer = 0.4f;
            this.DrawLerp = SaveData.Instance.TotalStrawberries > this.strawberries.Amount || (double) this.strawberriesUpdateTimer > 0.0 || (double) this.strawberriesWaitTimer > 0.0 || scene.Paused && scene.PauseMainMenuOpen ? Calc.Approach(this.DrawLerp, 1f, 1.2f * Engine.RawDeltaTime) : Calc.Approach(this.DrawLerp, 0.0f, 2f * Engine.RawDeltaTime);
            if ((double) this.strawberriesWaitTimer > 0.0)
                this.strawberriesWaitTimer -= Engine.RawDeltaTime;
            if ((double) this.strawberriesUpdateTimer > 0.0 && (double) this.DrawLerp == 1.0)
            {
                this.strawberriesUpdateTimer -= Engine.RawDeltaTime;
                if ((double) this.strawberriesUpdateTimer <= 0.0)
                {
                    if (this.strawberries.Amount < SaveData.Instance.TotalStrawberries)
                        ++this.strawberries.Amount;
                    this.strawberriesWaitTimer = 2f;
                    if (this.strawberries.Amount < SaveData.Instance.TotalStrawberries)
                        this.strawberriesUpdateTimer = 0.3f;
                }
            }
            if (this.Visible)
            {
                float target = 96f;
                if (!scene.TimerHidden)
                {
                    if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
                        target += 58f;
                    else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                        target += 78f;
                }
                this.Y = Calc.Approach(this.Y, target, Engine.DeltaTime * 800f);
            }
            this.Visible = (double) this.DrawLerp > 0.0;
        }

        public override void Render()
        {
            Vector2 vector2 = Vector2.Lerp(new Vector2((float) -this.bg.Width, this.Y), new Vector2(32f, this.Y), Ease.CubeOut(this.DrawLerp)).Round();
            this.bg.DrawJustified(vector2 + new Vector2(-96f, 12f), new Vector2(0.0f, 0.5f));
            this.strawberries.Position = vector2 + new Vector2(0.0f, -this.Y);
            this.strawberries.Render();
        }
    }
}
