// Decompiled with JetBrains decompiler
// Type: Celeste.PreviewPostcard
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
    public class PreviewPostcard : Scene
    {
        private readonly Postcard postcard;

        public PreviewPostcard(Postcard postcard)
        {
            _ = Audio.SetMusic(null);
            _ = Audio.SetAmbience(null);
            this.postcard = postcard;
            Add(new Entity()
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
            {
                return;
            }

            postcard.BeforeRender();
        }
    }
}
