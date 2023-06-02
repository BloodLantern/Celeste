using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Threading;

namespace Celeste
{
    public class OverworldLoader : Scene
    {
        public Overworld.StartMode StartMode;
        public HiresSnow Snow;
        private bool loaded;
        private bool fadeIn;
        private Overworld overworld;
        private Postcard postcard;
        private bool showVariantPostcard;
        private bool showUnlockCSidePostcard;
        private Thread activeThread;

        public OverworldLoader(Overworld.StartMode startMode, HiresSnow snow = null)
        {
            this.StartMode = startMode;
            this.Snow = snow == null ? new HiresSnow() : snow;
            this.fadeIn = snow == null;
        }

        public override void Begin()
        {
            this.Add((Monocle.Renderer) new HudRenderer());
            this.Add((Monocle.Renderer) this.Snow);
            if (this.fadeIn)
            {
                ScreenWipe.WipeColor = Color.Black;
                FadeWipe fadeWipe = new FadeWipe((Scene) this, true);
            }
            this.RendererList.UpdateLists();
            Session session = (Session) null;
            if (SaveData.Instance != null)
                session = SaveData.Instance.CurrentSession;
            this.Add(new Entity()
            {
                (Component) new Coroutine(this.Routine(session))
            });
            this.activeThread = Thread.CurrentThread;
            this.activeThread.Priority = ThreadPriority.Lowest;
            RunThread.Start(new Action(this.LoadThread), "OVERWORLD_LOADER", true);
        }

        private void LoadThread()
        {
            if (!MTN.Loaded)
                MTN.Load();
            if (!MTN.DataLoaded)
                MTN.LoadData();
            this.CheckVariantsPostcardAtLaunch();
            this.overworld = new Overworld(this);
            this.overworld.Entities.UpdateLists();
            this.loaded = true;
            this.activeThread.Priority = ThreadPriority.Normal;
        }

        private IEnumerator Routine(Session session)
        {
            OverworldLoader overworldLoader = this;
            if ((overworldLoader.StartMode == Overworld.StartMode.AreaComplete || overworldLoader.StartMode == Overworld.StartMode.AreaQuit) && session != null)
            {
                if (session.UnlockedCSide)
                    overworldLoader.showUnlockCSidePostcard = true;
                if (!Settings.Instance.VariantsUnlocked && SaveData.Instance != null && SaveData.Instance.TotalHeartGems >= 24)
                    overworldLoader.showVariantPostcard = true;
            }
            if (overworldLoader.showUnlockCSidePostcard)
            {
                yield return (object) 3f;
                overworldLoader.Add((Entity) (overworldLoader.postcard = new Postcard(Dialog.Get("POSTCARD_CSIDES"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out")));
                yield return (object) overworldLoader.postcard.DisplayRoutine();
            }
            while (!overworldLoader.loaded)
                yield return (object) null;
            if (overworldLoader.showVariantPostcard)
            {
                yield return (object) 3f;
                Settings.Instance.VariantsUnlocked = true;
                overworldLoader.Add((Entity) (overworldLoader.postcard = new Postcard(Dialog.Get("POSTCARD_VARIANTS"), "event:/new_content/ui/postcard_variants_in", "event:/new_content/ui/postcard_variants_out")));
                yield return (object) overworldLoader.postcard.DisplayRoutine();
                UserIO.SaveHandler(false, true);
                while (UserIO.Saving)
                    yield return (object) null;
                while (SaveLoadIcon.Instance != null)
                    yield return (object) null;
            }
            Engine.Scene = (Scene) overworldLoader.overworld;
        }

        public override void BeforeRender()
        {
            base.BeforeRender();
            if (this.postcard == null)
                return;
            this.postcard.BeforeRender();
        }

        private void CheckVariantsPostcardAtLaunch()
        {
            if (this.StartMode != Overworld.StartMode.Titlescreen || Settings.Instance.VariantsUnlocked || Settings.LastVersion != null && !(new Version(Settings.LastVersion) <= new Version(1, 2, 4, 2)) || !UserIO.Open(UserIO.Mode.Read))
                return;
            for (int slot = 0; slot < 3; ++slot)
            {
                if (UserIO.Exists(SaveData.GetFilename(slot)))
                {
                    SaveData saveData = UserIO.Load<SaveData>(SaveData.GetFilename(slot));
                    if (saveData != null)
                    {
                        saveData.AfterInitialize();
                        if (saveData.TotalHeartGems >= 24)
                        {
                            this.showVariantPostcard = true;
                            break;
                        }
                    }
                }
            }
            UserIO.Close();
            SaveData.Instance = (SaveData) null;
        }
    }
}
