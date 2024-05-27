using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class UnlockedRemixDisplay : Entity
    {
        private const float DisplayDuration = 4f;
        private const float LerpInSpeed = 1.2f;
        private const float LerpOutSpeed = 2f;
        private const float IconSize = 128f;
        private const float Spacing = 20f;
        private string text;
        private float drawLerp;
        private MTexture bg;
        private MTexture icon;
        private float rotation;
        private bool unlockedRemix;
        private TotalStrawberriesDisplay strawberries;
        private Wiggler wiggler;
        private bool hasCassetteAlready;

        public UnlockedRemixDisplay()
        {
            Tag = (int) Tags.HUD | (int) Tags.Global | (int) Tags.PauseUpdate | (int) Tags.TransitionUpdate;
            bg = GFX.Gui["strawberryCountBG"];
            icon = GFX.Gui["collectables/cassette"];
            text = Dialog.Clean("ui_remix_unlocked");
            Visible = false;
            Y = 96f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            hasCassetteAlready = SaveData.Instance.Areas[AreaData.Get(Scene).ID].Cassette;
            unlockedRemix = (scene as Level).Session.Cassette;
        }

        public override void Update()
        {
            base.Update();
            if (!unlockedRemix && (Scene as Level).Session.Cassette)
            {
                unlockedRemix = true;
                Add(new Coroutine(DisplayRoutine()));
            }
            if (!Visible)
                return;
            float target = 96f;
            if (Settings.Instance.SpeedrunClock == SpeedrunType.Chapter)
                target += 58f;
            else if (Settings.Instance.SpeedrunClock == SpeedrunType.File)
                target += 78f;
            if (strawberries.Visible)
                target += 96f;
            Y = Calc.Approach(Y, target, Engine.DeltaTime * 800f);
        }

        private IEnumerator DisplayRoutine()
        {
            UnlockedRemixDisplay unlockedRemixDisplay = this;
            unlockedRemixDisplay.strawberries = unlockedRemixDisplay.Scene.Entities.FindFirst<TotalStrawberriesDisplay>();
            unlockedRemixDisplay.Visible = true;
            while ((unlockedRemixDisplay.drawLerp += Engine.DeltaTime * 1.2f) < 1.0)
                yield return null;
            // ISSUE: reference to a compiler-generated method
            unlockedRemixDisplay.Add(wiggler = Wiggler.Create(0.8f, 4f, delegate (float f)
            {
                    rotation = f * 0.1f;
            }, true));
            unlockedRemixDisplay.drawLerp = 1f;
            yield return 4f;
            while ((unlockedRemixDisplay.drawLerp -= Engine.DeltaTime * 2f) > 0.0)
                yield return null;
            unlockedRemixDisplay.Visible = false;
            unlockedRemixDisplay.RemoveSelf();
        }

        public override void Render()
        {
            float x = !hasCassetteAlready ? (float) (ActiveFont.Measure(text).X + 128.0 + 80.0) : 188f;
            Vector2 vector2 = Vector2.Lerp(new Vector2(-x, Y), new Vector2(0.0f, Y), Ease.CubeOut(drawLerp));
            bg.DrawJustified(vector2 + new Vector2(x, 0.0f), new Vector2(1f, 0.5f));
            Draw.Rect(vector2.X, vector2.Y - bg.Height / 2, (float) (x - (double) bg.Width + 1.0), bg.Height, Color.Black);
            float scale = 128f / icon.Width;
            icon.DrawJustified(vector2 + new Vector2((float) (20.0 + icon.Width * (double) scale * 0.5), 0.0f), new Vector2(0.5f, 0.5f), Color.White, scale, rotation);
            if (hasCassetteAlready)
                return;
            ActiveFont.DrawOutline(text, vector2 + new Vector2(168f, 0.0f), new Vector2(0.0f, 0.6f), Vector2.One, Color.White, 2f, Color.Black);
        }
    }
}
