// Decompiled with JetBrains decompiler
// Type: Celeste.MainMenuSmallButton
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class MainMenuSmallButton : MenuButton
    {
        private const float IconWidth = 64f;
        private const float IconSpacing = 20f;
        private const float MaxLabelWidth = 400f;
        private readonly MTexture icon;
        private readonly string label;
        private readonly float labelScale;
        private readonly Wiggler wiggler;
        private float ease;

        public MainMenuSmallButton(
            string labelName,
            string iconName,
            Oui oui,
            Vector2 targetPosition,
            Vector2 tweenFrom,
            Action onConfirm)
            : base(oui, targetPosition, tweenFrom, onConfirm)
        {
            label = Dialog.Clean(labelName);
            icon = GFX.Gui[iconName];
            labelScale = 1f;
            float x = ActiveFont.Measure(label).X;
            if ((double)x > 400.0)
            {
                labelScale = 400f / x;
            }

            Add(wiggler = Wiggler.Create(0.25f, 4f));
        }

        public override void Update()
        {
            base.Update();
            ease = Calc.Approach(ease, Selected ? 1f : 0.0f, 6f * Engine.DeltaTime);
        }

        public override void Render()
        {
            base.Render();
            float scale = 64f / icon.Width;
            Vector2 vector2 = new(Ease.CubeInOut(ease) * 32f, (float)(((double)ActiveFont.LineHeight / 2.0) + ((double)wiggler.Value * 8.0)));
            icon.DrawOutlineJustified(Position + vector2, new Vector2(0.0f, 0.5f), Color.White, scale);
            ActiveFont.DrawOutline(label, Position + vector2 + new Vector2(84f, 0.0f), new Vector2(0.0f, 0.5f), Vector2.One * labelScale, SelectionColor, 2f, Color.Black);
        }

        public override void OnSelect()
        {
            wiggler.Start();
        }

        public override float ButtonHeight => ActiveFont.LineHeight * 1.25f;
    }
}
