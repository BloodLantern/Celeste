// Decompiled with JetBrains decompiler
// Type: Celeste.StrawberriesCounter
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class StrawberriesCounter : Component
    {
        public static readonly Color FlashColor = Calc.HexToColor("FF5E76");
        private const int IconWidth = 60;
        public bool Golden;
        public Vector2 Position;
        public bool CenteredX;
        public bool CanWiggle = true;
        public float Scale = 1f;
        public float Stroke = 2f;
        public float Rotation;
        public Color Color = Color.White;
        public Color OutOfColor = Color.LightGray;
        public bool OverworldSfx;
        private int amount;
        private int outOf = -1;
        private readonly Wiggler wiggler;
        private float flashTimer;
        private string sAmount;
        private string sOutOf;
        private readonly MTexture x;
        private bool showOutOf;

        public StrawberriesCounter(bool centeredX, int amount, int outOf = 0, bool showOutOf = false)
            : base(true, true)
        {
            CenteredX = centeredX;
            this.amount = amount;
            this.outOf = outOf;
            this.showOutOf = showOutOf;
            UpdateStrings();
            wiggler = Wiggler.Create(0.5f, 3f);
            wiggler.StartZero = true;
            wiggler.UseRawDeltaTime = true;
            x = GFX.Gui[nameof(x)];
        }

        public int Amount
        {
            get => amount;
            set
            {
                if (amount == value)
                {
                    return;
                }

                amount = value;
                UpdateStrings();
                if (!CanWiggle)
                {
                    return;
                }

                _ = OverworldSfx
                    ? Audio.Play(Golden ? "event:/ui/postgame/goldberry_count" : "event:/ui/postgame/strawberry_count")
                    : Audio.Play("event:/ui/game/increment_strawberry");

                wiggler.Start();
                flashTimer = 0.5f;
            }
        }

        public int OutOf
        {
            get => outOf;
            set
            {
                outOf = value;
                UpdateStrings();
            }
        }

        public bool ShowOutOf
        {
            get => showOutOf;
            set
            {
                if (showOutOf == value)
                {
                    return;
                }

                showOutOf = value;
                UpdateStrings();
            }
        }

        public float FullHeight => Math.Max(ActiveFont.LineHeight, GFX.Gui["collectables/strawberry"].Height);

        private void UpdateStrings()
        {
            sAmount = amount.ToString();
            sOutOf = outOf > -1 ? "/" + outOf.ToString() : "";
        }

        public void Wiggle()
        {
            wiggler.Start();
            flashTimer = 0.5f;
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
            Vector2 renderPosition = RenderPosition;
            Vector2 vector = Calc.AngleToVector(Rotation, 1f);
            Vector2 vector2 = new(-vector.Y, vector.X);
            string text = showOutOf ? sOutOf : "";
            float x1 = ActiveFont.Measure(sAmount).X;
            float x2 = ActiveFont.Measure(text).X;
            float num = (float)(62.0 + x.Width + 2.0) + x1 + x2;
            Color color = Color;
            if (flashTimer > 0.0 && Scene != null && Scene.BetweenRawInterval(0.05f))
            {
                color = StrawberriesCounter.FlashColor;
            }

            if (CenteredX)
            {
                renderPosition -= vector * (num / 2f) * Scale;
            }

            string id = Golden ? "collectables/goldberry" : "collectables/strawberry";
            GFX.Gui[id].DrawCentered(renderPosition + (vector * 60f * 0.5f * Scale), Color.White, Scale);
            x.DrawCentered(renderPosition + (vector * (float)(62.0 + (x.Width * 0.5)) * Scale) + (vector2 * 2f * Scale), color, Scale);
            ActiveFont.DrawOutline(sAmount, renderPosition + (vector * (float)((double)num - (double)x2 - ((double)x1 * 0.5)) * Scale) + (vector2 * (wiggler.Value * 18f) * Scale), new Vector2(0.5f, 0.5f), Vector2.One * Scale, color, Stroke, Color.Black);
            if (!(text != ""))
            {
                return;
            }

            ActiveFont.DrawOutline(text, renderPosition + (vector * (num - (x2 / 2f)) * Scale), new Vector2(0.5f, 0.5f), Vector2.One * Scale, OutOfColor, Stroke, Color.Black);
        }

        public Vector2 RenderPosition => ((Entity != null ? Entity.Position : Vector2.Zero) + Position).Round();
    }
}
