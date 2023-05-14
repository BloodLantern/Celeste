// Decompiled with JetBrains decompiler
// Type: Celeste.WaveDashPage03
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class WaveDashPage03 : WaveDashPage
    {
        private readonly string title;
        private string titleDisplayed;
        private MTexture clipArt;
        private float clipArtEase;
        private FancyText.Text infoText;
        private AreaCompleteTitle easyText;

        public WaveDashPage03()
        {
            Transition = WaveDashPage.Transitions.Blocky;
            ClearColor = Calc.HexToColor("d9ead3");
            title = Dialog.Clean("WAVEDASH_PAGE3_TITLE");
            titleDisplayed = "";
        }

        public override void Added(WaveDashPresentation presentation)
        {
            base.Added(presentation);
            clipArt = presentation.Gfx["moveset"];
        }

        public override IEnumerator Routine()
        {
            WaveDashPage03 waveDashPage03 = this;
            while (waveDashPage03.titleDisplayed.Length < waveDashPage03.title.Length)
            {
                waveDashPage03.titleDisplayed += waveDashPage03.title[waveDashPage03.titleDisplayed.Length].ToString();
                yield return 0.05f;
            }
            yield return waveDashPage03.PressButton();
            _ = Audio.Play("event:/new_content/game/10_farewell/ppt_wavedash_whoosh");
            while (waveDashPage03.clipArtEase < 1.0)
            {
                waveDashPage03.clipArtEase = Calc.Approach(waveDashPage03.clipArtEase, 1f, Engine.DeltaTime);
                yield return null;
            }
            yield return 0.25f;
            waveDashPage03.infoText = FancyText.Parse(Dialog.Get("WAVEDASH_PAGE3_INFO"), waveDashPage03.Width - 240, 32, defaultColor: new Color?(Color.Black * 0.7f));
            yield return waveDashPage03.PressButton();
            _ = Audio.Play("event:/new_content/game/10_farewell/ppt_its_easy");
            waveDashPage03.easyText = new AreaCompleteTitle(new Vector2(waveDashPage03.Width / 2f, waveDashPage03.Height - 150), Dialog.Clean("WAVEDASH_PAGE3_EASY"), 2f, true);
            yield return 1f;
        }

        public override void Update()
        {
            if (easyText == null)
            {
                return;
            }

            easyText.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(titleDisplayed, new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            if (clipArtEase > 0.0)
            {
                clipArt.DrawCentered(new Vector2(Width / 2f, (float)((Height / 2.0) - 90.0)), Color.White * clipArtEase, Vector2.One * (float)(1.0 + ((1.0 - clipArtEase) * 3.0)) * 0.8f, (float)((1.0 - clipArtEase) * 8.0));
            }

            infoText?.Draw(new Vector2(Width / 2f, Height - 350), new Vector2(0.5f, 0.0f), Vector2.One, 1f);
            if (easyText == null)
            {
                return;
            }

            easyText.Render();
        }
    }
}
