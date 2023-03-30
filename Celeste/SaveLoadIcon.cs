// Decompiled with JetBrains decompiler
// Type: Celeste.SaveLoadIcon
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
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
            scene.Add((Entity) (SaveLoadIcon.Instance = new SaveLoadIcon()));
        }

        public static void Hide()
        {
            if (SaveLoadIcon.Instance == null)
                return;
            SaveLoadIcon.Instance.display = false;
        }

        public SaveLoadIcon()
        {
            this.Tag = (int) Tags.HUD | (int) Tags.FrozenUpdate | (int) Tags.PauseUpdate | (int) Tags.Global;
            this.Depth = -1000000;
            this.Add((Component) (this.icon = GFX.GuiSpriteBank.Create("save")));
            this.icon.UseRawDeltaTime = true;
            this.Add((Component) (this.wiggler = Wiggler.Create(0.4f, 4f, (Action<float>) (f => this.icon.Rotation = f * 0.1f))));
            this.wiggler.UseRawDeltaTime = true;
            this.Add((Component) new Coroutine(this.Routine())
            {
                UseRawDeltaTime = true
            });
            this.icon.Visible = false;
        }

        private IEnumerator Routine()
        {
            SaveLoadIcon saveLoadIcon = this;
            saveLoadIcon.icon.Play("start", true);
            saveLoadIcon.icon.Visible = true;
            yield return (object) 0.25f;
            float timer = 1f;
            while (saveLoadIcon.display)
            {
                timer -= Engine.DeltaTime;
                if ((double) timer <= 0.0)
                {
                    saveLoadIcon.wiggler.Start();
                    timer = 1f;
                }
                yield return (object) null;
            }
            saveLoadIcon.icon.Play("end");
            yield return (object) 0.5f;
            saveLoadIcon.icon.Visible = false;
            yield return (object) null;
            SaveLoadIcon.Instance = (SaveLoadIcon) null;
            saveLoadIcon.RemoveSelf();
        }

        public override void Render()
        {
            this.Position = new Vector2(1760f, 920f);
            base.Render();
        }
    }
}
