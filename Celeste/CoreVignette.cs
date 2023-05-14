// Decompiled with JetBrains decompiler
// Type: Celeste.CoreVignette
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class CoreVignette : Scene
    {
        private readonly Session session;
        private Coroutine textCoroutine;
        private readonly FancyText.Text text;
        private int textStart;
        private float textAlpha;
        private readonly HiresSnow snow;
        private readonly HudRenderer hud;
        private TextMenu menu;
        private float fade;
        private float pauseFade;
        private bool started;
        private bool exiting;

        public bool CanPause => menu == null;

        public CoreVignette(Session session, HiresSnow snow = null)
        {
            this.session = session;
            snow ??= new HiresSnow();
            Add(hud = new HudRenderer());
            Add(this.snow = snow);
            RendererList.UpdateLists();
            text = FancyText.Parse(Dialog.Get("APP_INTRO"), 960, 8, 0.0f);
            textCoroutine = new Coroutine(TextSequence());
        }

        private IEnumerator TextSequence()
        {
            yield return 1f;
            while (textStart < text.Count)
            {
                textAlpha = 1f;
                float fadeTimePerCharacter = 1f / text.GetCharactersOnPage(textStart);
                for (int i = textStart; i < text.Count && text[i] is not FancyText.NewPage; ++i)
                {
                    if (text[i] is FancyText.Char c)
                    {
                        while ((double)(c.Fade += Engine.DeltaTime / fadeTimePerCharacter) < 1.0)
                        {
                            yield return null;
                        }

                        c.Fade = 1f;
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
            if (!started)
            {
                StartGame();
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
                    {
                        textCoroutine.Update();
                    }

                    if (menu == null && (Input.Pause.Pressed || Input.ESC.Pressed))
                    {
                        Input.Pause.ConsumeBuffer();
                        Input.ESC.ConsumeBuffer();
                        OpenMenu();
                    }
                }
            }
            else if (!exiting)
            {
                menu.Update();
            }

            pauseFade = Calc.Approach(pauseFade, menu != null ? 1f : 0.0f, Engine.DeltaTime * 8f);
            hud.BackgroundFade = Calc.Approach(hud.BackgroundFade, menu != null ? 0.6f : 0.0f, Engine.DeltaTime * 3f);
            fade = Calc.Approach(fade, 0.0f, Engine.DeltaTime);
        }

        public void OpenMenu()
        {
            _ = Audio.Play("event:/ui/game/pause");
            Add(menu = new TextMenu());
            _ = menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_resume")).Pressed(new Action(CloseMenu)));
            _ = menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_skip")).Pressed(new Action(StartGame)));
            _ = menu.Add(new TextMenu.Button(Dialog.Clean("intro_vignette_quit")).Pressed(new Action(ReturnToMap)));
            menu.OnCancel = menu.OnESC = menu.OnPause = new Action(CloseMenu);
        }

        private void CloseMenu()
        {
            _ = Audio.Play("event:/ui/game/unpause");
            menu?.RemoveSelf();
            menu = null;
        }

        private void StartGame()
        {
            textCoroutine = null;
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
            menu.RemoveSelf();
            menu = null;
            exiting = true;
            bool toAreaQuit = SaveData.Instance.Areas[0].Modes[0].Completed && Celeste.PlayMode != Celeste.PlayModes.Event;
            new FadeWipe(this, false, () =>
            {
                Engine.Scene = toAreaQuit
                    ? new OverworldLoader(Overworld.StartMode.AreaQuit, snow)
                    : (Scene)new OverworldLoader(Overworld.StartMode.Titlescreen, snow);
            }).OnUpdate = f => textAlpha = Math.Min(textAlpha, 1f - f);
            RendererList.UpdateLists();
            RendererList.MoveToFront(snow);
        }

        public override void Render()
        {
            base.Render();
            if (fade <= 0.0 && textAlpha <= 0.0)
            {
                return;
            }

            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, RasterizerState.CullNone, null, Engine.ScreenMatrix);
            if (fade > 0.0)
            {
                Draw.Rect(-1f, -1f, 1922f, 1082f, Color.Black * fade);
            }

            if (textStart < text.Nodes.Count && textAlpha > 0.0)
            {
                text.Draw(new Vector2(1920f, 1080f) * 0.5f, new Vector2(0.5f, 0.5f), Vector2.One, textAlpha * (1f - pauseFade), textStart);
            }

            Draw.SpriteBatch.End();
        }
    }
}
