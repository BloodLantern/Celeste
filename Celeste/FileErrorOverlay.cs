// Decompiled with JetBrains decompiler
// Type: Celeste.FileErrorOverlay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections;

namespace Celeste
{
    public class FileErrorOverlay : Overlay
    {
        private readonly FileErrorOverlay.Error mode;
        private TextMenu menu;

        public bool Open { get; private set; }

        public bool TryAgain { get; private set; }

        public bool Ignore { get; private set; }

        public FileErrorOverlay(FileErrorOverlay.Error mode)
        {
            Open = true;
            this.mode = mode;
            Add(new Coroutine(Routine()));
            Engine.Scene.Add(this);
        }

        private IEnumerator Routine()
        {
            FileErrorOverlay fileErrorOverlay = this;
            yield return fileErrorOverlay.FadeIn();
            bool waiting = true;
            int option = 0;
            _ = Audio.Play("event:/ui/main/message_confirm");
            fileErrorOverlay.menu = new TextMenu
            {
                new TextMenu.Header(Dialog.Clean("savefailed_title")),
                new TextMenu.Button(Dialog.Clean(fileErrorOverlay.mode == FileErrorOverlay.Error.Save ? "savefailed_retry" : "loadfailed_goback")).Pressed(() =>
                {
                    option = 0;
                    waiting = false;
                }),
                new TextMenu.Button(Dialog.Clean("savefailed_ignore")).Pressed(() =>
                {
                    option = 1;
                    waiting = false;
                })
            };
            while (waiting)
            {
                yield return null;
            }

            fileErrorOverlay.menu = null;
            fileErrorOverlay.Ignore = option == 1;
            fileErrorOverlay.TryAgain = option == 0;
            yield return fileErrorOverlay.FadeOut();
            fileErrorOverlay.Open = false;
            fileErrorOverlay.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            menu?.Update();
            if (SaveLoadIcon.Instance == null || SaveLoadIcon.Instance.Scene != Scene)
            {
                return;
            }

            SaveLoadIcon.Instance.Update();
        }

        public override void Render()
        {
            RenderFade();
            menu?.Render();
            base.Render();
        }

        public enum Error
        {
            Load,
            Save,
        }
    }
}
