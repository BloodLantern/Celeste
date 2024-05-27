using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class WaveDashPage06 : WaveDashPage
    {
        private AreaCompleteTitle title;

        public WaveDashPage06()
        {
            Transition = Transitions.Rotate3D;
            ClearColor = Calc.HexToColor("d9d2e9");
        }

        public override IEnumerator Routine()
        {
            WaveDashPage06 waveDashPage06 = this;
            yield return 1f;
            Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
            waveDashPage06.title = new AreaCompleteTitle(new Vector2(waveDashPage06.Width / 2f, 150f), Dialog.Clean("WAVEDASH_PAGE6_TITLE"), 2f, true);
            yield return 1.5f;
        }

        public override void Update()
        {
            if (title == null)
                return;
            title.Update();
        }

        public override void Render()
        {
            Presentation.Gfx["Bird Clip Art"].DrawCentered(new Vector2(Width, Height) / 2f, Color.White, 1.5f);
            if (title == null)
                return;
            title.Render();
        }
    }
}
