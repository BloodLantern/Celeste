﻿using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class DeathsCounter : Component
    {
        private const int IconWidth = 60;
        public Vector2 Position;
        public bool CenteredX;
        public bool CanWiggle = true;
        public float Alpha = 1f;
        public float Scale = 1f;
        public float Stroke = 2f;
        public Color Color = Color.White;
        private int amount;
        private int minDigits;
        private Wiggler wiggler;
        private Wiggler iconWiggler;
        private float flashTimer;
        private string sAmount;
        private MTexture icon;
        private MTexture x;

        public DeathsCounter(AreaMode mode, bool centeredX, int amount, int minDigits = 0)
            : base(true, true)
        {
            CenteredX = centeredX;
            this.amount = amount;
            this.minDigits = minDigits;
            UpdateString();
            wiggler = Wiggler.Create(0.5f, 3f);
            wiggler.StartZero = true;
            wiggler.UseRawDeltaTime = true;
            iconWiggler = Wiggler.Create(0.5f, 3f);
            iconWiggler.UseRawDeltaTime = true;
            SetMode(mode);
            x = GFX.Gui[nameof (x)];
        }

        private void UpdateString()
        {
            if (minDigits > 0)
                sAmount = amount.ToString("D" + minDigits);
            else
                sAmount = amount.ToString();
        }

        public int Amount
        {
            get => amount;
            set
            {
                if (amount == value)
                    return;
                amount = value;
                UpdateString();
                if (!CanWiggle)
                    return;
                wiggler.Start();
                flashTimer = 0.5f;
            }
        }

        public int MinDigits
        {
            get => minDigits;
            set
            {
                if (minDigits == value)
                    return;
                minDigits = value;
                UpdateString();
            }
        }

        public void SetMode(AreaMode mode)
        {
            switch (mode)
            {
                case AreaMode.Normal:
                    icon = GFX.Gui["collectables/skullBlue"];
                    break;
                case AreaMode.BSide:
                    icon = GFX.Gui["collectables/skullRed"];
                    break;
                default:
                    icon = GFX.Gui["collectables/skullGold"];
                    break;
            }
            iconWiggler.Start();
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
                wiggler.Update();
            if (iconWiggler.Active)
                iconWiggler.Update();
            if (flashTimer <= 0.0)
                return;
            flashTimer -= Engine.RawDeltaTime;
        }

        public override void Render()
        {
            Vector2 renderPosition = RenderPosition;
            float x = ActiveFont.Measure(sAmount).X;
            float num = (float) (62.0 + this.x.Width + 2.0) + x;
            Color color = Color;
            Color black = Color.Black;
            if (flashTimer > 0.0 && Scene != null && Scene.BetweenRawInterval(0.05f))
                color = StrawberriesCounter.FlashColor;
            if (Alpha < 1.0)
            {
                color *= Alpha;
                black *= Alpha;
            }
            if (CenteredX)
                renderPosition -= Vector2.UnitX * (num / 2f) * Scale;
            icon.DrawCentered(renderPosition + new Vector2(30f, 0.0f) * Scale, Color.White * Alpha, Scale * (float) (1.0 + iconWiggler.Value * 0.20000000298023224));
            this.x.DrawCentered(renderPosition + new Vector2(62f + this.x.Width / 2, 2f) * Scale, color, Scale);
            ActiveFont.DrawOutline(sAmount, renderPosition + new Vector2(num - x / 2f, (float) (-(double) wiggler.Value * 18.0)) * Scale, new Vector2(0.5f, 0.5f), Vector2.One * Scale, color, Stroke, black);
        }

        public Vector2 RenderPosition => ((Entity != null ? Entity.Position : Vector2.Zero) + Position).Round();
    }
}
