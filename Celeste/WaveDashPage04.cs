// Decompiled with JetBrains decompiler
// Type: Celeste.WaveDashPage04
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class WaveDashPage04 : WaveDashPage
    {
        private WaveDashPlaybackTutorial tutorial;
        private FancyText.Text list;
        private int listIndex;
        private float time;

        public WaveDashPage04()
        {
            Transition = WaveDashPage.Transitions.FadeIn;
            ClearColor = Calc.HexToColor("f4cccc");
        }

        public override void Added(WaveDashPresentation presentation)
        {
            base.Added(presentation);
            List<MTexture> textures = Presentation.Gfx.GetAtlasSubtextures("playback/platforms");
            tutorial = new WaveDashPlaybackTutorial("wavedashppt", new Vector2(-126f, 0.0f), new Vector2(1f, 1f), new Vector2(1f, -1f))
            {
                OnRender = () => textures[(int)(time % (double)textures.Count)].DrawCentered(Vector2.Zero)
            };
        }

        public override IEnumerator Routine()
        {
            WaveDashPage04 waveDashPage04 = this;
            yield return 0.5f;
            waveDashPage04.list = FancyText.Parse(Dialog.Get("WAVEDASH_PAGE4_LIST"), waveDashPage04.Width, 32, defaultColor: new Color?(Color.Black * 0.7f));
            float delay = 0.0f;
            for (; waveDashPage04.listIndex < waveDashPage04.list.Nodes.Count; ++waveDashPage04.listIndex)
            {
                if (waveDashPage04.list.Nodes[waveDashPage04.listIndex] is FancyText.NewLine)
                {
                    yield return waveDashPage04.PressButton();
                }
                else
                {
                    delay += 0.008f;
                    if ((double)delay >= 0.016000000759959221)
                    {
                        delay -= 0.016f;
                        yield return 0.016f;
                    }
                }
            }
        }

        public override void Update()
        {
            time += Engine.DeltaTime * 4f;
            tutorial.Update();
        }

        public override void Render()
        {
            ActiveFont.DrawOutline(Dialog.Clean("WAVEDASH_PAGE4_TITLE"), new Vector2(128f, 100f), Vector2.Zero, Vector2.One * 1.5f, Color.White, 2f, Color.Black);
            tutorial.Render(new Vector2(Width / 2f, (float)((Height / 2.0) - 100.0)), 4f);
            if (list == null)
            {
                return;
            }

            list.Draw(new Vector2(160f, Height - 400), new Vector2(0.0f, 0.0f), Vector2.One, 1f, end: listIndex);
        }
    }
}
