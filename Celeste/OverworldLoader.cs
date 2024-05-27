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
            StartMode = startMode;
            Snow = snow == null ? new HiresSnow() : snow;
            fadeIn = snow == null;
        }

        public override void Begin()
        {
            Add(new HudRenderer());
            Add(Snow);
            if (fadeIn)
            {
                ScreenWipe.WipeColor = Color.Black;
                FadeWipe fadeWipe = new FadeWipe(this, true);
            }
            RendererList.UpdateLists();
            Session session = null;
            if (SaveData.Instance != null)
                session = SaveData.Instance.CurrentSession;
            Add(new Entity
            {
                new Coroutine(Routine(session))
            });
            activeThread = Thread.CurrentThread;
            activeThread.Priority = ThreadPriority.Lowest;
            RunThread.Start(LoadThread, "OVERWORLD_LOADER", true);
        }

        private void LoadThread()
        {
            if (!MTN.Loaded)
                MTN.Load();
            if (!MTN.DataLoaded)
                MTN.LoadData();
            CheckVariantsPostcardAtLaunch();
            overworld = new Overworld(this);
            overworld.Entities.UpdateLists();
            loaded = true;
            activeThread.Priority = ThreadPriority.Normal;
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
                yield return 3f;
                overworldLoader.Add(overworldLoader.postcard = new Postcard(Dialog.Get("POSTCARD_CSIDES"), "event:/ui/main/postcard_csides_in", "event:/ui/main/postcard_csides_out"));
                yield return overworldLoader.postcard.DisplayRoutine();
            }
            while (!overworldLoader.loaded)
                yield return null;
            if (overworldLoader.showVariantPostcard)
            {
                yield return 3f;
                Settings.Instance.VariantsUnlocked = true;
                overworldLoader.Add(overworldLoader.postcard = new Postcard(Dialog.Get("POSTCARD_VARIANTS"), "event:/new_content/ui/postcard_variants_in", "event:/new_content/ui/postcard_variants_out"));
                yield return overworldLoader.postcard.DisplayRoutine();
                UserIO.SaveHandler(false, true);
                while (UserIO.Saving)
                    yield return null;
                while (SaveLoadIcon.Instance != null)
                    yield return null;
            }
            Engine.Scene = overworldLoader.overworld;
        }

        public override void BeforeRender()
        {
            base.BeforeRender();
            if (postcard == null)
                return;
            postcard.BeforeRender();
        }

        private void CheckVariantsPostcardAtLaunch()
        {
            if (StartMode != Overworld.StartMode.Titlescreen || Settings.Instance.VariantsUnlocked || Settings.LastVersion != null && !(new Version(Settings.LastVersion) <= new Version(1, 2, 4, 2)) || !UserIO.Open(UserIO.Mode.Read))
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
                            showVariantPostcard = true;
                            break;
                        }
                    }
                }
            }
            UserIO.Close();
            SaveData.Instance = null;
        }
    }
}
