using Monocle;
using System.Collections;

namespace Celeste
{
    public class PreviewPostcard : Scene
    {
        private Postcard postcard;

        public PreviewPostcard(Postcard postcard)
        {
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            this.postcard = postcard;
            Add(new Entity
            {
                new Coroutine(Routine(postcard))
            });
            Add(new HudRenderer());
        }

        private IEnumerator Routine(Postcard postcard)
        {
            PreviewPostcard previewPostcard = this;
            yield return 0.25f;
            previewPostcard.Add(postcard);
            yield return postcard.DisplayRoutine();
            Engine.Scene = new OverworldLoader(Overworld.StartMode.MainMenu);
        }

        public override void BeforeRender()
        {
            base.BeforeRender();
            if (postcard == null)
                return;
            postcard.BeforeRender();
        }
    }
}
