// Decompiled with JetBrains decompiler
// Type: Celeste.MainMenuClimb
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class MainMenuClimb : MenuButton
    {
        private const float MaxLabelWidth = 256f;
        private const float BaseLabelScale = 1.5f;
        private readonly string label;
        private readonly MTexture icon;
        private readonly float labelScale;
        private readonly Wiggler bounceWiggler;
        private readonly Wiggler rotateWiggler;
        private readonly Wiggler bigBounceWiggler;
        private bool confirmed;

        public MainMenuClimb(Oui oui, Vector2 targetPosition, Vector2 tweenFrom, Action onConfirm)
            : base(oui, targetPosition, tweenFrom, onConfirm)
        {
            label = Dialog.Clean("menu_begin");
            icon = GFX.Gui["menu/start"];
            labelScale = 1f;
            float num = ActiveFont.Measure(label).X * 1.5f;
            if ((double)num > 256.0)
            {
                labelScale = 256f / num;
            }

            Add(bounceWiggler = Wiggler.Create(0.25f, 4f));
            Add(rotateWiggler = Wiggler.Create(0.3f, 6f));
            Add(bigBounceWiggler = Wiggler.Create(0.4f, 2f));
        }

        public override void OnSelect()
        {
            confirmed = false;
            bounceWiggler.Start();
        }

        public override void Confirm()
        {
            base.Confirm();
            confirmed = true;
            bounceWiggler.Start();
            bigBounceWiggler.Start();
            rotateWiggler.Start();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }

        public override void Render()
        {
            Vector2 vector2_1 = new(0.0f, bounceWiggler.Value * 8f);
            Vector2 vector2_2 = (Vector2.UnitY * icon.Height) + new Vector2(0.0f, -Math.Abs(bigBounceWiggler.Value * 40f));
            if (!confirmed)
            {
                vector2_2 += vector2_1;
            }

            icon.DrawOutlineJustified(Position + vector2_2, new Vector2(0.5f, 1f), Color.White, 1f, (float)((double)rotateWiggler.Value * 10.0 * (Math.PI / 180.0)));
            ActiveFont.DrawOutline(label, Position + vector2_1 + new Vector2(0.0f, 48 + icon.Height), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * labelScale, SelectionColor, 2f, Color.Black);
        }

        public override float ButtonHeight => (float)(icon.Height + (double)ActiveFont.LineHeight + 48.0);
    }
}
