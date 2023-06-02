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
            this.Transition = WaveDashPage.Transitions.Rotate3D;
            this.ClearColor = Calc.HexToColor("d9d2e9");
        }

        public override IEnumerator Routine()
        {
            WaveDashPage06 waveDashPage06 = this;
            yield return (object) 1f;
            Audio.Play("event:/new_content/game/10_farewell/ppt_happy_wavedashing");
            waveDashPage06.title = new AreaCompleteTitle(new Vector2((float) waveDashPage06.Width / 2f, 150f), Dialog.Clean("WAVEDASH_PAGE6_TITLE"), 2f, true);
            yield return (object) 1.5f;
        }

        public override void Update()
        {
            if (this.title == null)
                return;
            this.title.Update();
        }

        public override void Render()
        {
            this.Presentation.Gfx["Bird Clip Art"].DrawCentered(new Vector2((float) this.Width, (float) this.Height) / 2f, Color.White, 1.5f);
            if (this.title == null)
                return;
            this.title.Render();
        }
    }
}
