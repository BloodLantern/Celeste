using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class WaveDashPage01 : WaveDashPage
    {
        private AreaCompleteTitle title;
        private float subtitleEase;

        public WaveDashPage01()
        {
            Transition = Transitions.ScaleIn;
            ClearColor = Calc.HexToColor("9fc5e8");
        }

        public override void Added(WaveDashPresentation presentation) => base.Added(presentation);

        public override IEnumerator Routine()
        {
            WaveDashPage01 waveDashPage01 = this;
            Audio.SetAltMusic("event:/new_content/music/lvl10/intermission_powerpoint");
            yield return 1f;
            waveDashPage01.title = new AreaCompleteTitle(new Vector2(waveDashPage01.Width / 2f, (float) (waveDashPage01.Height / 2.0 - 100.0)), Dialog.Clean("WAVEDASH_PAGE1_TITLE"), 2f, true);
            yield return 1f;
            while (waveDashPage01.subtitleEase < 1.0)
            {
                waveDashPage01.subtitleEase = Calc.Approach(waveDashPage01.subtitleEase, 1f, Engine.DeltaTime);
                yield return null;
            }
            yield return 0.1f;
        }

        public override void Update()
        {
            if (title == null)
                return;
            title.Update();
        }

        public override void Render()
        {
            if (title != null)
                title.Render();
            if (subtitleEase <= 0.0)
                return;
            Vector2 position = new Vector2(Width / 2f, (float) (Height / 2.0 + 80.0));
            float x = (float) (1.0 + Ease.BigBackIn(1f - subtitleEase) * 2.0);
            float y = (float) (0.25 + Ease.BigBackIn(subtitleEase) * 0.75);
            ActiveFont.Draw(Dialog.Clean("WAVEDASH_PAGE1_SUBTITLE"), position, new Vector2(0.5f, 0.5f), new Vector2(x, y), Color.Black * 0.8f);
        }
    }
}
