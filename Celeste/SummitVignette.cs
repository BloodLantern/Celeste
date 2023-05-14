// Decompiled with JetBrains decompiler
// Type: Celeste.SummitVignette
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;
using System.IO;
using System.Xml;

namespace Celeste
{
    public class SummitVignette : Scene
    {
        private CompleteRenderer complete;
        private bool slideFinished;
        private readonly Session session;
        private bool ending;
        private bool ready;
        private bool addedRenderer;

        public SummitVignette(Session session)
        {
            this.session = session;
            session.Audio.Apply();
            RunThread.Start(new Action(LoadCompleteThread), "SUMMIT_VIGNETTE");
        }

        private void LoadCompleteThread()
        {
            Atlas atlas = null;
            XmlElement xml = GFX.CompleteScreensXml["Screens"]["SummitIntro"];
            if (xml != null)
            {
                atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", xml.Attr("atlas")), Atlas.AtlasDataFormat.PackerNoAtlas);
            }

            complete = new CompleteRenderer(xml, atlas, 0.0f, () => slideFinished = true)
            {
                SlideDuration = 7.5f
            };
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
            if ((!Input.MenuConfirm.Pressed && !slideFinished) || ending || !ready)
            {
                return;
            }

            ending = true;
            MountainWipe mountainWipe = new(this, false, () => Engine.Scene = new LevelLoader(session));
        }

        public override void End()
        {
            base.End();
            if (complete == null)
            {
                return;
            }

            complete.Dispose();
        }
    }
}
