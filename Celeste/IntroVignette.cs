using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class IntroVignette : Scene
    {
        private const float SFXDuration = 18.683f;
        private Session session;
        private bool started;
        private float timer;
        private string areaMusic;
        private float fade;
        private EventInstance sfx;
        private TextMenu menu;
        private float pauseFade;
        private HudRenderer hud;
        private bool exiting;
        private Coroutine textCoroutine;
        private FancyText.Text text;
        private int textStart;
        private float textAlpha;
        private HiresSnow snow;

        public bool CanPause => menu == null;

        public IntroVignette(Session session, HiresSnow snow = null)
        {
            this.session = session;
            areaMusic = session.Audio.Music.Event;
            session.Audio.Music.Event = null;
            session.Audio.Apply();
            sfx = Audio.Play("event:/game/00_prologue/intro_vignette");
            if (snow == null)
            {
                fade = 1f;
                snow = new HiresSnow();
            }
            Add(hud = new HudRenderer());
            Add(this.snow = snow);
            RendererList.UpdateLists();
            text = FancyText.Parse(Dialog.Get("CH0_INTRO"), 960, 8, 0.0f);
            textCoroutine = new Coroutine(TextSequence());
        }

        private IEnumerator TextSequence()
        {
            yield return 3f;
            while (textStart < text.Count)
            {
                textAlpha = 1f;
                float fadeTimePerCharacter = 1f / text.GetCharactersOnPage(textStart);
                for (int i = textStart; i < text.Count && !(text[i] is FancyText.NewPage); ++i)
                {
                    if (text[i] is FancyText.Char c)
                    {
                        while ((c.Fade += Engine.DeltaTime / fadeTimePerCharacter) < 1.0)
                            yield return null;
                        c.Fade = 1f;
                        c = null;
                    }
                }
                yield return 2.5f;
                while (textAlpha > 0.0)
                {
                    textAlpha -= 1f * Engine.DeltaTime;
                    yield return null;
                }
                textAlpha = 0.0f;
                textStart = text.GetNextPageStart(textStart);
                yield return 0.5f;
            }
            textStart = int.MaxValue;
        }

        public override void Update()
        {
            if (menu == null)
            {
                base.Update();
                if (!exiting)
                {
                    if (textCoroutine != null && textCoroutine.Active)
                        textCoroutine.Update();
                    timer += Engine.DeltaTime;
                    if (timer >= 18.683000564575195 && !started)
                        StartGame();
                    if (timer < 16.683000564575195 && menu == null && (Input.Pause.Pressed || Input.ESC.Pressed))
                    {
                        Input.Pause.ConsumeBuffer();
                        Input.ESC.ConsumeBuffer();
                        OpenMenu();
                    }
                }
            }
            else if (!exiting)
                menu.Update();
            pauseFade = Calc.Approach(pauseFade, menu != null ? 1f : 0.0f, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0.0f, Engine.DeltaTime * 3f);
            fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime);
        }

        public void OpenMenu()
        {
            Audio.Play("event:/ui/game/pause");
            Audio.Pause(sfx);
            Add(menu = new TextMenu());
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(CloseMenu));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(StartGame));
            menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_quit")).Pressed(ReturnToMap));
            menu.OnCancel = menu.OnESC = menu.OnPause = CloseMenu;
        }

        private void CloseMenu()
        {
            Audio.Play("event:/ui/game/unpause");
            Audio.Resume(sfx);
            if (menu != null)
                menu.RemoveSelf();
            menu = null;
        }

        private void StartGame()
        {
            textCoroutine = null;
            StopSfx();
            session.Audio.Music.Event = areaMusic;
            if (menu != null)
            {
                menu.RemoveSelf();
                menu = null;
            }
            new FadeWipe(this, false, () => Engine.Scene = new LevelLoader(session)).OnUpdate = f => textAlpha = Math.Min(textAlpha, 1f - f);
            started = true;
            exiting = true;
        }

        private void ReturnToMap()
        {
            StopSfx();
            menu.RemoveSelf();
            menu = null;
            exiting = true;
            bool toAreaQuit = SaveData.Instance.Areas[0].Modes[0].Completed && Celeste.PlayMode != Celeste.PlayModes.Event;
            new FadeWipe(this, false, () =>
            {
                if (toAreaQuit)
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.AreaQuit, snow);
                else
                    Engine.Scene = new OverworldLoader(Overworld.StartMode.Titlescreen, snow);
            }).OnUpdate = f => textAlpha = Math.Min(textAlpha, 1f - f);
            RendererList.UpdateLists();
            RendererList.MoveToFront(snow);
        }

        private void StopSfx() => Audio.Stop(sfx, false);

        public override void End()
        {
            StopSfx();
            base.End();
        }

        public override void Render()
        {
            base.Render();
            if (fade <= 0.0 && textAlpha <= 0.0)
                return;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            if (fade > 0.0)
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            if (textStart < text.Nodes.Count && textAlpha > 0.0)
                text.Draw(new Vector2(1920f, 1080f) * 0.5f, new Vector2(0.5f, 0.5f), Vector2.One, textAlpha * (1f - pauseFade), textStart);
            Draw.SpriteBatch.End();
        }
    }
}
