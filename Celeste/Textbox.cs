using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    [Tracked]
    public class Textbox : Entity
    {
        private MTexture textbox = GFX.Portraits["textbox/default"];
        private MTexture textboxOverlay;
        private const int textboxInnerWidth = 1688;
        private const int textboxInnerHeight = 272;
        private const float portraitPadding = 16f;
        private const float tweenDuration = 0.4f;
        private const float switchToIdleAnimationDelay = 0.5f;
        private readonly float innerTextPadding;
        private readonly float maxLineWidthNoPortrait;
        private readonly float maxLineWidth;
        private readonly int linesPerPage;
        private const int stopVoiceCharactersEarly = 4;
        private float ease;
        private FancyText.Text text;
        private Func<IEnumerator>[] events;
        private Coroutine runRoutine;
        private Coroutine skipRoutine;
        private PixelFont font;
        private float lineHeight;
        private FancyText.Anchors anchor;
        private FancyText.Portrait portrait;
        private int index;
        private bool waitingForInput;
        private bool disableInput;
        private int shakeSeed;
        private float timer;
        private float gradientFade;
        private bool isInTrigger;
        private bool canSkip = true;
        private bool easingClose;
        private bool easingOpen;
        public Vector2 RenderOffset;
        private bool autoPressContinue;
        private char lastChar;
        private Sprite portraitSprite = new Sprite(null, null);
        private bool portraitExists;
        private bool portraitIdling;
        private float portraitScale = 1.5f;
        private Wiggler portraitWiggle;
        private Sprite portraitGlitchy;
        private bool isPortraitGlitchy;
        private Dictionary<string, SoundSource> talkers = new Dictionary<string, SoundSource>();
        private SoundSource activeTalker;
        private SoundSource phonestatic;

        public bool Opened { get; private set; }

        public int Page { get; private set; }

        public List<FancyText.Node> Nodes => text.Nodes;

        public bool UseRawDeltaTime
        {
            set => runRoutine.UseRawDeltaTime = value;
        }

        public int Start { get; private set; }

        public string PortraitName => portrait == null || portrait.Sprite == null ? "" : portrait.Sprite;

        public string PortraitAnimation => portrait == null || portrait.Sprite == null ? "" : portrait.Animation;

        public Textbox(string dialog, params Func<IEnumerator>[] events)
            : this(dialog, null, events)
        {
        }

        public Textbox(string dialog, Language language, params Func<IEnumerator>[] events)
        {
            Tag = (int) Tags.PauseUpdate | (int) Tags.HUD;
            Opened = true;
            font = Dialog.Language.Font;
            lineHeight = Dialog.Language.FontSize.LineHeight - 1;
            portraitSprite.UseRawDeltaTime = true;
            Add(portraitWiggle = Wiggler.Create(0.4f, 4f));
            this.events = events;
            linesPerPage = (int) (240.0 / lineHeight);
            innerTextPadding = (float) ((272.0 - lineHeight * (double) linesPerPage) / 2.0);
            maxLineWidthNoPortrait = (float) (1688.0 - innerTextPadding * 2.0);
            maxLineWidth = (float) (maxLineWidthNoPortrait - 240.0 - 32.0);
            text = FancyText.Parse(Dialog.Get(dialog, language), (int) maxLineWidth, linesPerPage, 0.0f, language: language);
            index = 0;
            Start = 0;
            skipRoutine = new Coroutine(SkipDialog());
            runRoutine = new Coroutine(RunRoutine());
            runRoutine.UseRawDeltaTime = true;
            if (Level.DialogSnapshot == null)
                Level.DialogSnapshot = Audio.CreateSnapshot("snapshot:/dialogue_in_progress", false);
            Audio.ResumeSnapshot(Level.DialogSnapshot);
            Add(phonestatic = new SoundSource());
        }

        public void SetStart(int value) => index = Start = value;

        private IEnumerator RunRoutine()
        {
            Textbox textbox = this;
            FancyText.Node last = null;
            float delayBuildup = 0.0f;
            while (textbox.index < textbox.Nodes.Count)
            {
                FancyText.Node current = textbox.Nodes[textbox.index];
                float delay = 0.0f;
                switch (current)
                {
                    case FancyText.Anchor _:
                        if (textbox.RenderOffset == Vector2.Zero)
                        {
                            FancyText.Anchors next = (current as FancyText.Anchor).Position;
                            if (textbox.ease >= 1.0 && next != textbox.anchor)
                                yield return textbox.EaseClose(false);
                            textbox.anchor = next;
                        }
                        break;
                    case FancyText.Portrait _:
                        FancyText.Portrait next1 = current as FancyText.Portrait;
                        textbox.phonestatic.Stop();
                        if (textbox.ease >= 1.0 && (textbox.portrait == null || next1.Sprite != textbox.portrait.Sprite || next1.Side != textbox.portrait.Side))
                            yield return textbox.EaseClose(false);
                        textbox.textbox = GFX.Portraits["textbox/default"];
                        textbox.textboxOverlay = null;
                        textbox.portraitExists = false;
                        textbox.activeTalker = null;
                        textbox.isPortraitGlitchy = false;
                        XmlElement xml = null;
                        if (!string.IsNullOrEmpty(next1.Sprite))
                        {
                            if (GFX.PortraitsSpriteBank.Has(next1.SpriteId))
                                xml = GFX.PortraitsSpriteBank.SpriteData[next1.SpriteId].Sources[0].XML;
                            textbox.portraitExists = xml != null;
                            textbox.isPortraitGlitchy = next1.Glitchy;
                            if (textbox.isPortraitGlitchy && textbox.portraitGlitchy == null)
                            {
                                textbox.portraitGlitchy = new Sprite(GFX.Portraits, "noise/");
                                textbox.portraitGlitchy.AddLoop("noise", "", 0.1f);
                                textbox.portraitGlitchy.Play("noise");
                            }
                        }
                        if (textbox.portraitExists)
                        {
                            if (textbox.portrait == null || next1.Sprite != textbox.portrait.Sprite)
                            {
                                GFX.PortraitsSpriteBank.CreateOn(textbox.portraitSprite, next1.SpriteId);
                                textbox.portraitScale = 240f / xml.AttrInt("size", 160);
                                if (!textbox.talkers.ContainsKey(next1.SfxEvent))
                                {
                                    SoundSource soundSource = new SoundSource().Play(next1.SfxEvent);
                                    textbox.talkers.Add(next1.SfxEvent, soundSource);
                                    textbox.Add(soundSource);
                                }
                            }
                            if (textbox.talkers.ContainsKey(next1.SfxEvent))
                                textbox.activeTalker = textbox.talkers[next1.SfxEvent];
                            string id = "textbox/" + xml.Attr("textbox", "default");
                            textbox.textbox = GFX.Portraits[id];
                            if (GFX.Portraits.Has(id + "_overlay"))
                                textbox.textboxOverlay = GFX.Portraits[id + "_overlay"];
                            string str = xml.Attr("phonestatic", "");
                            if (!string.IsNullOrEmpty(str))
                            {
                                if (str == "ex")
                                    textbox.phonestatic.Play("event:/char/dialogue/sfx_support/phone_static_ex");
                                else if (str == "mom")
                                    textbox.phonestatic.Play("event:/char/dialogue/sfx_support/phone_static_mom");
                            }
                            textbox.canSkip = false;
                            FancyText.Portrait portrait = textbox.portrait;
                            textbox.portrait = next1;
                            if (next1.Pop)
                                textbox.portraitWiggle.Start();
                            if (portrait == null || portrait.Sprite != next1.Sprite || portrait.Animation != next1.Animation)
                            {
                                if (textbox.portraitSprite.Has(next1.BeginAnimation))
                                {
                                    textbox.portraitSprite.Play(next1.BeginAnimation, true);
                                    yield return textbox.EaseOpen();
                                    while (textbox.portraitSprite.CurrentAnimationID == next1.BeginAnimation && textbox.portraitSprite.Animating)
                                        yield return null;
                                }
                                if (textbox.portraitSprite.Has(next1.IdleAnimation))
                                {
                                    textbox.portraitIdling = true;
                                    textbox.portraitSprite.Play(next1.IdleAnimation, true);
                                }
                            }
                            yield return textbox.EaseOpen();
                            textbox.canSkip = true;
                        }
                        else
                        {
                            textbox.portrait = null;
                            yield return textbox.EaseOpen();
                        }
                        next1 = null;
                        break;
                    case FancyText.NewPage _:
                        textbox.PlayIdleAnimation();
                        if (textbox.ease >= 1.0)
                        {
                            textbox.waitingForInput = true;
                            yield return 0.1f;
                            while (!textbox.ContinuePressed())
                                yield return null;
                            textbox.waitingForInput = false;
                        }
                        textbox.Start = textbox.index + 1;
                        textbox.Page++;
                        break;
                    case FancyText.Wait _:
                        textbox.PlayIdleAnimation();
                        delay = (current as FancyText.Wait).Duration;
                        break;
                    case FancyText.Trigger _:
                        textbox.isInTrigger = true;
                        textbox.PlayIdleAnimation();
                        FancyText.Trigger trigger = current as FancyText.Trigger;
                        if (!trigger.Silent)
                            yield return textbox.EaseClose(false);
                        int index1 = trigger.Index;
                        if (textbox.events != null && index1 >= 0 && index1 < textbox.events.Length)
                            yield return textbox.events[index1]();
                        textbox.isInTrigger = false;
                        trigger = null;
                        break;
                    case FancyText.Char _:
                        FancyText.Char ch = current as FancyText.Char;
                        textbox.lastChar = (char) ch.Character;
                        if (textbox.ease < 1.0)
                            yield return textbox.EaseOpen();
                        bool flag = false;
                        if (textbox.index - 5 > textbox.Start)
                        {
                            for (int index2 = textbox.index; index2 < Math.Min(textbox.index + 4, textbox.Nodes.Count); ++index2)
                            {
                                if (textbox.Nodes[index2] is FancyText.NewPage)
                                {
                                    flag = true;
                                    textbox.PlayIdleAnimation();
                                }
                            }
                        }
                        if (!flag && !ch.IsPunctuation)
                            textbox.PlayTalkAnimation();
                        if (last != null && last is FancyText.NewPage)
                        {
                            --textbox.index;
                            yield return 0.2f;
                            ++textbox.index;
                        }
                        delay = ch.Delay + delayBuildup;
                        ch = null;
                        break;
                }
                last = current;
                ++textbox.index;
                if (delay < 0.016000000759959221)
                {
                    delayBuildup += delay;
                }
                else
                {
                    delayBuildup = 0.0f;
                    if (delay > 0.5)
                        textbox.PlayIdleAnimation();
                    yield return delay;
                }
                current = null;
            }
            textbox.PlayIdleAnimation();
            if (textbox.ease > 0.0)
            {
                textbox.waitingForInput = true;
                while (!textbox.ContinuePressed())
                    yield return null;
                textbox.waitingForInput = false;
                textbox.Start = textbox.Nodes.Count;
                yield return textbox.EaseClose(true);
            }
            textbox.Close();
        }

        private void PlayIdleAnimation()
        {
            StopTalker();
            if (portraitIdling || portraitSprite == null || portrait == null || !portraitSprite.Has(portrait.IdleAnimation))
                return;
            portraitSprite.Play(portrait.IdleAnimation);
            portraitIdling = true;
        }

        private void StopTalker()
        {
            if (activeTalker == null)
                return;
            activeTalker.Param("dialogue_portrait", 0.0f);
            activeTalker.Param("dialogue_end", 1f);
        }

        private void PlayTalkAnimation()
        {
            StartTalker();
            if (!portraitIdling || portraitSprite == null || portrait == null || !portraitSprite.Has(portrait.TalkAnimation))
                return;
            portraitSprite.Play(portrait.TalkAnimation);
            portraitIdling = false;
        }

        private void StartTalker()
        {
            if (activeTalker == null)
                return;
            activeTalker.Param("dialogue_portrait", portrait != null ? portrait.SfxExpression : 1f);
            activeTalker.Param("dialogue_end", 0.0f);
        }

        private IEnumerator EaseOpen()
        {
            Textbox textbox1 = this;
            if (textbox1.ease < 1.0)
            {
                textbox1.easingOpen = true;
                if (textbox1.portrait != null && textbox1.portrait.Sprite.IndexOf("madeline", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    Audio.Play("event:/ui/game/textbox_madeline_in");
                else
                    Audio.Play("event:/ui/game/textbox_other_in");
                while (true)
                {
                    Textbox textbox2 = textbox1;
                    double ease = textbox1.ease;
                    double num1 = (textbox1.runRoutine.UseRawDeltaTime ? Engine.RawDeltaTime : (double) Engine.DeltaTime) / 0.40000000596046448;
                    double num2;
                    float num3 = (float) (num2 = ease + num1);
                    textbox2.ease = (float) num2;
                    if (num3 < 1.0)
                    {
                        textbox1.gradientFade = Math.Max(textbox1.gradientFade, textbox1.ease);
                        yield return null;
                    }
                    else
                        break;
                }
                textbox1.ease = textbox1.gradientFade = 1f;
                textbox1.easingOpen = false;
            }
        }

        private IEnumerator EaseClose(bool final)
        {
            Textbox textbox1 = this;
            textbox1.easingClose = true;
            if (textbox1.portrait != null && textbox1.portrait.Sprite.IndexOf("madeline", StringComparison.InvariantCultureIgnoreCase) >= 0)
                Audio.Play("event:/ui/game/textbox_madeline_out");
            else
                Audio.Play("event:/ui/game/textbox_other_out");
            while (true)
            {
                Textbox textbox2 = textbox1;
                double ease = textbox1.ease;
                double num1 = (textbox1.runRoutine.UseRawDeltaTime ? Engine.RawDeltaTime : (double) Engine.DeltaTime) / 0.40000000596046448;
                double num2;
                float num3 = (float) (num2 = ease - num1);
                textbox2.ease = (float) num2;
                if (num3 > 0.0)
                {
                    if (final)
                        textbox1.gradientFade = textbox1.ease;
                    yield return null;
                }
                else
                    break;
            }
            textbox1.ease = 0.0f;
            textbox1.easingClose = false;
        }

        private IEnumerator SkipDialog()
        {
            while (true)
            {
                if (!waitingForInput && canSkip && !easingOpen && !easingClose && ContinuePressed())
                {
                    StopTalker();
                    disableInput = true;
                    while (!waitingForInput && canSkip && !easingOpen && !easingClose && !isInTrigger && !runRoutine.Finished)
                        runRoutine.Update();
                }
                yield return null;
                disableInput = false;
            }
        }

        public bool SkipToPage(int page)
        {
            autoPressContinue = true;
            while (Page != page && !runRoutine.Finished)
                Update();
            autoPressContinue = false;
            Update();
            while (Opened && ease < 1.0)
                Update();
            return Page == page && Opened;
        }

        public void Close()
        {
            Opened = false;
            if (Scene == null)
                return;
            Scene.Remove(this);
        }

        private bool ContinuePressed()
        {
            if (autoPressContinue)
                return true;
            return (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed) && !disableInput;
        }

        public override void Update()
        {
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null))
                return;
            if (!autoPressContinue)
                skipRoutine.Update();
            runRoutine.Update();
            if (Scene != null && Scene.OnInterval(0.05f))
                shakeSeed = Calc.Random.Next();
            if (portraitSprite != null && ease >= 1.0)
                portraitSprite.Update();
            if (portraitGlitchy != null && ease >= 1.0)
                portraitGlitchy.Update();
            timer += Engine.DeltaTime;
            portraitWiggle.Update();
            int num = Math.Min(index, Nodes.Count);
            for (int start = Start; start < num; ++start)
            {
                if (Nodes[start] is FancyText.Char)
                {
                    FancyText.Char node = Nodes[start] as FancyText.Char;
                    if (node.Fade < 1.0)
                        node.Fade = Calc.Clamp(node.Fade + 8f * Engine.DeltaTime, 0.0f, 1f);
                }
            }
        }

        public override void Render()
        {
            if (Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
                return;
            float num1 = Ease.CubeInOut(ease);
            if (num1 < 0.05000000074505806)
                return;
            float x1 = 116f;
            Vector2 vector2_1 = new Vector2(x1, x1 / 2f) + RenderOffset;
            if (RenderOffset == Vector2.Zero)
            {
                if (anchor == FancyText.Anchors.Bottom)
                    vector2_1 = new Vector2(x1, (float) (1080.0 - x1 / 2.0 - 272.0));
                else if (anchor == FancyText.Anchors.Middle)
                    vector2_1 = new Vector2(x1, 404f);
                vector2_1.Y += (int) (136.0 * (1.0 - num1));
            }
            textbox.DrawCentered(vector2_1 + new Vector2(1688f, 272f * num1) / 2f, Color.White, new Vector2(1f, num1));
            if (waitingForInput)
            {
                float num2 = portrait == null || PortraitSide(portrait) < 0 ? 1688f : 1432f;
                Vector2 position = new Vector2(vector2_1.X + num2, vector2_1.Y + 272f) + new Vector2(-48f, (timer % 1.0 < 0.25 ? 6 : 0) - 40);
                GFX.Gui["textboxbutton"].DrawCentered(position);
            }
            if (portraitExists)
            {
                if (PortraitSide(portrait) > 0)
                {
                    portraitSprite.Position = new Vector2((float) (vector2_1.X + 1688.0 - 240.0 - 16.0), vector2_1.Y);
                    portraitSprite.Scale.X = -portraitScale;
                }
                else
                {
                    portraitSprite.Position = new Vector2(vector2_1.X + 16f, vector2_1.Y);
                    portraitSprite.Scale.X = portraitScale;
                }
                portraitSprite.Scale.X *= portrait.Flipped ? -1f : 1f;
                portraitSprite.Scale.Y = (float) (portraitScale * ((272.0 * num1 - 32.0) / 240.0) * (portrait.UpsideDown ? -1.0 : 1.0));
                Sprite portraitSprite1 = portraitSprite;
                portraitSprite1.Scale *= (float) (0.89999997615814209 + portraitWiggle.Value * 0.10000000149011612);
                Sprite portraitSprite2 = portraitSprite;
                portraitSprite2.Position += new Vector2(120f, (float) (272.0 * num1 * 0.5));
                portraitSprite.Color = Color.White * num1;
                if (Math.Abs(portraitSprite.Scale.Y) > 0.05000000074505806)
                {
                    portraitSprite.Render();
                    if (isPortraitGlitchy && portraitGlitchy != null)
                    {
                        portraitGlitchy.Position = portraitSprite.Position;
                        portraitGlitchy.Origin = portraitSprite.Origin;
                        portraitGlitchy.Scale = portraitSprite.Scale;
                        portraitGlitchy.Color = Color.White * 0.2f * num1;
                        portraitGlitchy.Render();
                    }
                }
            }
            if (textboxOverlay != null)
            {
                int x2 = 1;
                if (portrait != null && PortraitSide(portrait) > 0)
                    x2 = -1;
                textboxOverlay.DrawCentered(vector2_1 + new Vector2(1688f, 272f * num1) / 2f, Color.White, new Vector2(x2, num1));
            }
            Calc.PushRandom(shakeSeed);
            int num3 = 1;
            for (int start = Start; start < text.Nodes.Count; ++start)
            {
                if (text.Nodes[start] is FancyText.NewLine)
                    ++num3;
                else if (text.Nodes[start] is FancyText.NewPage)
                    break;
            }
            Vector2 vector2_2 = new Vector2(innerTextPadding + (portrait == null || PortraitSide(portrait) >= 0 ? 0.0f : 256f), innerTextPadding);
            Vector2 vector2_3 = new Vector2(portrait == null ? maxLineWidthNoPortrait : maxLineWidth, linesPerPage * lineHeight * num1) / 2f;
            float num4 = num3 >= 4 ? 0.75f : 1f;
            text.Draw(vector2_1 + vector2_2 + vector2_3, new Vector2(0.5f, 0.5f), new Vector2(1f, num1) * num4, num1, Start);
            Calc.PopRandom();
        }

        public int PortraitSide(FancyText.Portrait portrait) => SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode ? -portrait.Side : portrait.Side;

        public override void Removed(Scene scene)
        {
            Audio.EndSnapshot(Level.DialogSnapshot);
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Audio.EndSnapshot(Level.DialogSnapshot);
            base.SceneEnd(scene);
        }

        public static IEnumerator Say(string dialog, params Func<IEnumerator>[] events)
        {
            Textbox textbox = new Textbox(dialog, events);
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
                yield return null;
        }

        public static IEnumerator SayWhileFrozen(
            string dialog,
            params Func<IEnumerator>[] events)
        {
            Textbox textbox = new Textbox(dialog, events);
            textbox.Tag |= (int) Tags.FrozenUpdate;
            Engine.Scene.Add(textbox);
            while (textbox.Opened)
                yield return null;
        }
    }
}
