using Monocle;
using System.IO;
using System.Xml;

namespace Celeste
{
    public class SummitVignette : Scene
    {
        private CompleteRenderer complete;
        private bool slideFinished;
        private Session session;
        private bool ending;
        private bool ready;
        private bool addedRenderer;

        public SummitVignette(Session session)
        {
            this.session = session;
            session.Audio.Apply();
            RunThread.Start(LoadCompleteThread, "SUMMIT_VIGNETTE");
        }

        private void LoadCompleteThread()
        {
            Atlas atlas = null;
            XmlElement xml = GFX.CompleteScreensXml["Screens"]["SummitIntro"];
            if (xml != null)
                atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", xml.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
            complete = new CompleteRenderer(xml, atlas, 0.0f, () => slideFinished = true);
            complete.SlideDuration = 7.5f;
            ready = true;
        }

        public override void Update()
        {
            if (ready && !addedRenderer)
            {
                Add(complete);
                addedRenderer = true;
            }
            base.Update();
            if (!Input.MenuConfirm.Pressed && !slideFinished || ending || !ready)
                return;
            ending = true;
            MountainWipe mountainWipe = new MountainWipe(this, false, () => Engine.Scene = new LevelLoader(session));
        }

        public override void End()
        {
            base.End();
            if (complete == null)
                return;
            complete.Dispose();
        }
    }
}
