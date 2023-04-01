// Decompiled with JetBrains decompiler
// Type: Celeste.BestTimeDisplay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class BestTimeDisplay : Component
    {
        private static readonly Color IconColor = Color.Lerp(Calc.HexToColor("7CFF70"), Color.Black, 0.25f);
        private static readonly Color FullClearColor = Color.Lerp(Calc.HexToColor("FF3D57"), Color.Black, 0.25f);
        public Vector2 Position;
        private TimeSpan time;
        private string sTime;
        private readonly Wiggler wiggler;
        private readonly MTexture icon;
        private float flashTimer;
        private Color iconColor;

        public BestTimeDisplay(Modes mode, TimeSpan time)
            : base(true, true)
        {
            this.time = time;
            UpdateString();
            wiggler = Wiggler.Create(0.5f, 3f);
            wiggler.UseRawDeltaTime = true;
            switch (mode)
            {
                case Modes.BestFullClear:
                    icon = GFX.Game["gui/bestFullClearTime"];
                    iconColor = FullClearColor;
                    break;
                case Modes.Current:
                    icon = null;
                    break;
                default:
                    icon = GFX.Game["gui/bestTime"];
                    iconColor = IconColor;
                    break;
            }
        }

        private void UpdateString()
        {
            sTime = time.ShortGameplayFormat();
        }

        public void Wiggle()
        {
            wiggler.Start();
            flashTimer = 0.5f;
        }

        public TimeSpan Time
        {
            get => time;
            set
            {
                if (!(time != value))
                {
                    return;
                }

                time = value;
                UpdateString();
                wiggler.Start();
                flashTimer = 0.5f;
            }
        }

        public override void Update()
        {
            base.Update();
            if (wiggler.Active)
            {
                wiggler.Update();
            }

            if (flashTimer <= 0.0)
            {
                return;
            }

            flashTimer -= Engine.RawDeltaTime;
        }

        public override void Render()
        {
            if (!WillRender)
            {
                return;
            }

            Vector2 vector2 = RenderPosition - (Vector2.UnitY * wiggler.Value * 3f);
            Color color = Color.White;
            if (flashTimer > 0.0 && Scene.BetweenRawInterval(0.05f))
            {
                color = StrawberriesCounter.FlashColor;
            }

            icon?.DrawOutlineCentered(vector2 + new Vector2(-4f, -3f), iconColor);
            ActiveFont.DrawOutline(sTime, vector2 + new Vector2(0.0f, 4f), new Vector2(0.5f, 0.0f), Vector2.One, color, 2f, Color.Black);
        }

        public Vector2 RenderPosition => (Entity.Position + Position).Round();

        public bool WillRender => time > TimeSpan.Zero;

        public enum Modes
        {
            Best,
            BestFullClear,
            Current,
        }
    }
}
