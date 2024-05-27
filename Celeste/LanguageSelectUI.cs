using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class LanguageSelectUI : TextMenu
    {
        private bool open = true;

        public LanguageSelectUI()
        {
            Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
            Alpha = 0.0f;
            foreach (Language orderedLanguage in Dialog.OrderedLanguages)
            {
                Language language = orderedLanguage;
                Add(new LanguageOption(language).Pressed(() =>
                {
                    open = false;
                    SetNextLanguage(language);
                }));
            }
            OnESC = OnPause = OnCancel = () =>
            {
                open = false;
                Focused = false;
            };
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
            if (Alpha > 0.0)
                base.Update();
            if (!open && Alpha <= 0.0)
                Close();
            Alpha = Calc.Approach(Alpha, open ? 1f : 0.0f, Engine.DeltaTime * 8f);
        }

        public override void Render()
        {
            if (Alpha <= 0.0)
                return;
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * Ease.CubeOut(Alpha));
            base.Render();
        }

        private class LanguageOption : Item
        {
            public Language Language;
            private float selectedEase;

            public LanguageOption(Language language)
            {
                Selectable = true;
                Language = language;
            }

            public bool Selected => Container.Current == this;

            public override void Added()
            {
                Container.InnerContent = InnerContentMode.OneColumn;
                if (Dialog.Language != Language)
                    return;
                Container.Current = this;
            }

            public override float LeftWidth() => Language.Icon.Width;

            public override float Height() => Language.Icon.Height;

            public override void Update() => selectedEase = Calc.Approach(selectedEase, Selected ? 1f : 0.0f, Engine.DeltaTime * 5f);

            public override void Render(Vector2 position, bool highlighted)
            {
                Color color = Disabled ? Color.DarkSlateGray : (highlighted ? Container.HighlightColor : Color.White) * Container.Alpha;
                position += (1f - Ease.CubeOut(Container.Alpha)) * Vector2.UnitY * 32f;
                if (Selected)
                    GFX.Gui["dotarrow_outline"].DrawCentered(position, color);
                position += Vector2.UnitX * Ease.CubeInOut(selectedEase) * 32f;
                Language.Icon.DrawJustified(position, new Vector2(0.0f, 0.5f), Color.White * Container.Alpha, 1f);
            }
        }
    }
}
