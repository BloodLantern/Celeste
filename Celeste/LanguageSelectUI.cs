// Decompiled with JetBrains decompiler
// Type: Celeste.LanguageSelectUI
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class LanguageSelectUI : TextMenu
    {
        private bool open = true;

        public LanguageSelectUI()
        {
            this.Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
            this.Alpha = 0.0f;
            foreach (Language orderedLanguage in Dialog.OrderedLanguages)
            {
                Language language = orderedLanguage;
                this.Add(new LanguageSelectUI.LanguageOption(language).Pressed((Action) (() =>
                {
                    this.open = false;
                    this.SetNextLanguage(language);
                })));
            }
            this.OnESC = this.OnPause = this.OnCancel = (Action) (() =>
            {
                this.open = false;
                this.Focused = false;
            });
        }

        private void SetNextLanguage(Language next)
        {
            if (!(Settings.Instance.Language != next.Id))
                return;
            Language language1 = Dialog.Languages[Settings.Instance.Language];
            Language language2 = Dialog.Languages["english"];
            if (language1.FontFace != language2.FontFace)
                Fonts.Unload(language1.FontFace);
            Fonts.Load(next.FontFace);
            Settings.Instance.Language = next.Id;
            Settings.Instance.ApplyLanguage();
        }

        public override void Update()
        {
            if ((double) this.Alpha > 0.0)
                base.Update();
            if (!this.open && (double) this.Alpha <= 0.0)
                this.Close();
            this.Alpha = Calc.Approach(this.Alpha, this.open ? 1f : 0.0f, Engine.DeltaTime * 8f);
        }

        public override void Render()
        {
            if ((double) this.Alpha <= 0.0)
                return;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(this.Alpha));
            base.Render();
        }

        private class LanguageOption : TextMenu.Item
        {
            public Language Language;
            private float selectedEase;

            public LanguageOption(Language language)
            {
                this.Selectable = true;
                this.Language = language;
            }

            public bool Selected => this.Container.Current == this;

            public override void Added()
            {
                this.Container.InnerContent = TextMenu.InnerContentMode.OneColumn;
                if (Dialog.Language != this.Language)
                    return;
                this.Container.Current = (TextMenu.Item) this;
            }

            public override float LeftWidth() => (float) this.Language.Icon.Width;

            public override float Height() => (float) this.Language.Icon.Height;

            public override void Update() => this.selectedEase = Calc.Approach(this.selectedEase, this.Selected ? 1f : 0.0f, Engine.DeltaTime * 5f);

            public override void Render(Vector2 position, bool highlighted)
            {
                Color color = this.Disabled ? Color.DarkSlateGray : (highlighted ? this.Container.HighlightColor : Color.White) * this.Container.Alpha;
                position += (1f - Ease.CubeOut(this.Container.Alpha)) * Vector2.UnitY * 32f;
                if (this.Selected)
                    GFX.Gui["dotarrow_outline"].DrawCentered(position, color);
                position += Vector2.UnitX * Ease.CubeInOut(this.selectedEase) * 32f;
                this.Language.Icon.DrawJustified(position, new Vector2(0.0f, 0.5f), Color.White * this.Container.Alpha, 1f);
            }
        }
    }
}
