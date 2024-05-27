using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class SaveLoadIcon : Entity
    {
        public static SaveLoadIcon Instance;
        private bool display = true;
        private Sprite icon;
        private Wiggler wiggler;

        public static bool OnScreen => SaveLoadIcon.Instance != null;

        public static void Show(Scene scene)
        {
            if (SaveLoadIcon.Instance != null)
                SaveLoadIcon.Instance.RemoveSelf();
            scene.Add(SaveLoadIcon.Instance = new SaveLoadIcon());
        }

        public static void Hide()
        {
            if (SaveLoadIcon.Instance == null)
                return;
            SaveLoadIcon.Instance.display = false;
        }

        public SaveLoadIcon()
        {
            Tag = (int) Tags.HUD | (int) Tags.FrozenUpdate | (int) Tags.PauseUpdate | (int) Tags.Global;
            Depth = -1000000;
            Add(icon = GFX.GuiSpriteBank.Create("save"));
            icon.UseRawDeltaTime = true;
            Add(wiggler = Wiggler.Create(0.4f, 4f, f => icon.Rotation = f * 0.1f));
            wiggler.UseRawDeltaTime = true;
            Add(new Coroutine(Routine())
            {
                UseRawDeltaTime = true
            });
            icon.Visible = false;
        }

        private IEnumerator Routine()
        {
            SaveLoadIcon saveLoadIcon = this;
            saveLoadIcon.icon.Play("start", true);
            saveLoadIcon.icon.Visible = true;
            yield return 0.25f;
            float timer = 1f;
            while (saveLoadIcon.display)
            {
                timer -= Engine.DeltaTime;
                if (timer <= 0.0)
                {
                    saveLoadIcon.wiggler.Start();
                    timer = 1f;
                }
                yield return null;
            }
            saveLoadIcon.icon.Play("end");
            yield return 0.5f;
            saveLoadIcon.icon.Visible = false;
            yield return null;
            SaveLoadIcon.Instance = null;
            saveLoadIcon.RemoveSelf();
        }

        public override void Render()
        {
            Position = new Vector2(1760f, 920f);
            base.Render();
        }
    }
}
