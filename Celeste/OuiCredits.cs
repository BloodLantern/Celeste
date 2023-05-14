﻿// Decompiled with JetBrains decompiler
// Type: Celeste.OuiCredits
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class OuiCredits : Oui
    {
        private readonly Vector2 onScreen = new(960f, 0.0f);
        private readonly Vector2 offScreen = new(3840f, 0.0f);
        private Credits credits;
        private float vignetteAlpha;

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Position = offScreen;
            Visible = false;
        }

        public override IEnumerator Enter(Oui from)
        {
            OuiCredits ouiCredits = this;
            _ = Audio.SetMusic("event:/music/menu/credits");
            ouiCredits.Overworld.ShowConfirmUI = false;
            Credits.BorderColor = Color.Black;
            ouiCredits.credits = new Credits
            {
                Enabled = false
            };
            ouiCredits.Visible = true;
            ouiCredits.vignetteAlpha = 0.0f;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 4f)
            {
                ouiCredits.Position = ouiCredits.offScreen + ((ouiCredits.onScreen - ouiCredits.offScreen) * Ease.CubeOut(p));
                yield return null;
            }
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiCredits ouiCredits = this;
            _ = Audio.Play("event:/ui/main/whoosh_large_out");
            ouiCredits.Overworld.SetNormalMusic();
            ouiCredits.Overworld.ShowConfirmUI = true;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 4f)
            {
                ouiCredits.Position = ouiCredits.onScreen + ((ouiCredits.offScreen - ouiCredits.onScreen) * Ease.CubeIn(p));
                yield return null;
            }
            ouiCredits.Visible = false;
        }

        public override void Update()
        {
            if (Focused && (Input.MenuCancel.Pressed || credits.BottomTimer > 3.0))
            {
                _ = Overworld.Goto<OuiMainMenu>();
            }

            if (credits != null)
            {
                credits.Update();
                credits.Enabled = Focused && Selected;
            }
            vignetteAlpha = Calc.Approach(vignetteAlpha, Selected ? 1f : 0.0f, Engine.DeltaTime * (Selected ? 1f : 4f));
            base.Update();
        }

        public override void Render()
        {
            if (vignetteAlpha > 0.0)
            {
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * vignetteAlpha * 0.4f);
                OVR.Atlas["vignette"].Draw(Vector2.Zero, Vector2.Zero, Color.White * Ease.CubeInOut(vignetteAlpha), 1f);
            }
            if (credits == null)
            {
                return;
            }

            credits.Render(Position);
        }
    }
}
