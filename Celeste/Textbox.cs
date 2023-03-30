// Decompiled with JetBrains decompiler
// Type: Celeste.Textbox
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    [Tracked(false)]
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
        private Sprite portraitSprite = new Sprite((Atlas) null, (string) null);
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

        public List<FancyText.Node> Nodes => this.text.Nodes;

        public bool UseRawDeltaTime
        {
            set => this.runRoutine.UseRawDeltaTime = value;
        }

        public int Start { get; private set; }

        public string PortraitName => this.portrait == null || this.portrait.Sprite == null ? "" : this.portrait.Sprite;

        public string PortraitAnimation => this.portrait == null || this.portrait.Sprite == null ? "" : this.portrait.Animation;

        public Textbox(string dialog, params Func<IEnumerator>[] events)
            : this(dialog, (Language) null, events)
        {
        }

        public Textbox(string dialog, Language language, params Func<IEnumerator>[] events)
        {
            this.Tag = (int) Tags.PauseUpdate | (int) Tags.HUD;
            this.Opened = true;
            this.font = Dialog.Language.Font;
            this.lineHeight = (float) (Dialog.Language.FontSize.LineHeight - 1);
            this.portraitSprite.UseRawDeltaTime = true;
            this.Add((Component) (this.portraitWiggle = Wiggler.Create(0.4f, 4f)));
            this.events = events;
            this.linesPerPage = (int) (240.0 / (double) this.lineHeight);
            this.innerTextPadding = (float) ((272.0 - (double) this.lineHeight * (double) this.linesPerPage) / 2.0);
            this.maxLineWidthNoPortrait = (float) (1688.0 - (double) this.innerTextPadding * 2.0);
            this.maxLineWidth = (float) ((double) this.maxLineWidthNoPortrait - 240.0 - 32.0);
            this.text = FancyText.Parse(Dialog.Get(dialog, language), (int) this.maxLineWidth, this.linesPerPage, 0.0f, language: language);
            this.index = 0;
            this.Start = 0;
            this.skipRoutine = new Coroutine(this.SkipDialog());
            this.runRoutine = new Coroutine(this.RunRoutine());
            this.runRoutine.UseRawDeltaTime = true;
            if ((HandleBase) Level.DialogSnapshot == (HandleBase) null)
                Level.DialogSnapshot = Audio.CreateSnapshot("snapshot:/dialogue_in_progress", false);
            Audio.ResumeSnapshot(Level.DialogSnapshot);
            this.Add((Component) (this.phonestatic = new SoundSource()));
        }

        public void SetStart(int value) => this.index = this.Start = value;

        private IEnumerator RunRoutine()
        {
            Textbox textbox = this;
            FancyText.Node last = (FancyText.Node) null;
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
                            if ((double) textbox.ease >= 1.0 && next != textbox.anchor)
                                yield return (object) textbox.EaseClose(false);
                            textbox.anchor = next;
                            break;
                        }
                        break;
                    case FancyText.Portrait _:
                        FancyText.Portrait next1 = current as FancyText.Portrait;
                        textbox.phonestatic.Stop();
                        if ((double) textbox.ease >= 1.0 && (textbox.portrait == null || next1.Sprite != textbox.portrait.Sprite || next1.Side != textbox.portrait.Side))
                            yield return (object) textbox.EaseClose(false);
                        textbox.textbox = GFX.Portraits["textbox/default"];
                        textbox.textboxOverlay = (MTexture) null;
                        textbox.portraitExists = false;
                        textbox.activeTalker = (SoundSource) null;
                        textbox.isPortraitGlitchy = false;
                        XmlElement xml = (XmlElement) null;
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
                                textbox.portraitScale = 240f / (float) xml.AttrInt("size", 160);
                                if (!textbox.talkers.ContainsKey(next1.SfxEvent))
                                {
                                    SoundSource soundSource = new SoundSource().Play(next1.SfxEvent);
                                    textbox.talkers.Add(next1.SfxEvent, soundSource);
                                    textbox.Add((Component) soundSource);
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
                                    yield return (object) textbox.EaseOpen();
                                    while (textbox.portraitSprite.CurrentAnimationID == next1.BeginAnimation && textbox.portraitSprite.Animating)
                                        yield return (object) null;
                                }
                                if (textbox.portraitSprite.Has(next1.IdleAnimation))
                                {
                                    textbox.portraitIdling = true;
                                    textbox.portraitSprite.Play(next1.IdleAnimation, true);
                                }
                            }
                            yield return (object) textbox.EaseOpen();
                            textbox.canSkip = true;
                        }
                        else
                        {
                            textbox.portrait = (FancyText.Portrait) null;
                            yield return (object) textbox.EaseOpen();
                        }
                        next1 = (FancyText.Portrait) null;
                        break;
                    case FancyText.NewPage _:
                        textbox.PlayIdleAnimation();
                        if ((double) textbox.ease >= 1.0)
                        {
                            textbox.waitingForInput = true;
                            yield return (object) 0.1f;
                            while (!textbox.ContinuePressed())
                                yield return (object) null;
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
                            yield return (object) textbox.EaseClose(false);
                        int index1 = trigger.Index;
                        if (textbox.events != null && index1 >= 0 && index1 < textbox.events.Length)
                            yield return (object) textbox.events[index1]();
                        textbox.isInTrigger = false;
                        trigger = (FancyText.Trigger) null;
                        break;
                    case FancyText.Char _:
                        FancyText.Char ch = current as FancyText.Char;
                        textbox.lastChar = (char) ch.Character;
                        if ((double) textbox.ease < 1.0)
                            yield return (object) textbox.EaseOpen();
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
                            yield return (object) 0.2f;
                            ++textbox.index;
                        }
                        delay = ch.Delay + delayBuildup;
                        ch = (FancyText.Char) null;
                        break;
                }
                last = current;
                ++textbox.index;
                if ((double) delay < 0.016000000759959221)
                {
                    delayBuildup += delay;
                }
                else
                {
                    delayBuildup = 0.0f;
                    if ((double) delay > 0.5)
                        textbox.PlayIdleAnimation();
                    yield return (object) delay;
                }
                current = (FancyText.Node) null;
            }
            textbox.PlayIdleAnimation();
            if ((double) textbox.ease > 0.0)
            {
                textbox.waitingForInput = true;
                while (!textbox.ContinuePressed())
                    yield return (object) null;
                textbox.waitingForInput = false;
                textbox.Start = textbox.Nodes.Count;
                yield return (object) textbox.EaseClose(true);
            }
            textbox.Close();
        }

        private void PlayIdleAnimation()
        {
            this.StopTalker();
            if (this.portraitIdling || this.portraitSprite == null || this.portrait == null || !this.portraitSprite.Has(this.portrait.IdleAnimation))
                return;
            this.portraitSprite.Play(this.portrait.IdleAnimation);
            this.portraitIdling = true;
        }

        private void StopTalker()
        {
            if (this.activeTalker == null)
                return;
            this.activeTalker.Param("dialogue_portrait", 0.0f);
            this.activeTalker.Param("dialogue_end", 1f);
        }

        private void PlayTalkAnimation()
        {
            this.StartTalker();
            if (!this.portraitIdling || this.portraitSprite == null || this.portrait == null || !this.portraitSprite.Has(this.portrait.TalkAnimation))
                return;
            this.portraitSprite.Play(this.portrait.TalkAnimation);
            this.portraitIdling = false;
        }

        private void StartTalker()
        {
            if (this.activeTalker == null)
                return;
            this.activeTalker.Param("dialogue_portrait", this.portrait != null ? (float) this.portrait.SfxExpression : 1f);
            this.activeTalker.Param("dialogue_end", 0.0f);
        }

        private IEnumerator EaseOpen()
        {
            Textbox textbox1 = this;
            if ((double) textbox1.ease < 1.0)
            {
                textbox1.easingOpen = true;
                if (textbox1.portrait != null && textbox1.portrait.Sprite.IndexOf("madeline", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    Audio.Play("event:/ui/game/textbox_madeline_in");
                else
                    Audio.Play("event:/ui/game/textbox_other_in");
                while (true)
                {
                    Textbox textbox2 = textbox1;
                    double ease = (double) textbox1.ease;
                    double num1 = (textbox1.runRoutine.UseRawDeltaTime ? (double) Engine.RawDeltaTime : (double) Engine.DeltaTime) / 0.40000000596046448;
                    double num2;
                    float num3 = (float) (num2 = ease + num1);
                    textbox2.ease = (float) num2;
                    if ((double) num3 < 1.0)
                    {
                        textbox1.gradientFade = Math.Max(textbox1.gradientFade, textbox1.ease);
                        yield return (object) null;
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
                double ease = (double) textbox1.ease;
                double num1 = (textbox1.runRoutine.UseRawDeltaTime ? (double) Engine.RawDeltaTime : (double) Engine.DeltaTime) / 0.40000000596046448;
                double num2;
                float num3 = (float) (num2 = ease - num1);
                textbox2.ease = (float) num2;
                if ((double) num3 > 0.0)
                {
                    if (final)
                        textbox1.gradientFade = textbox1.ease;
                    yield return (object) null;
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
                if (!this.waitingForInput && this.canSkip && !this.easingOpen && !this.easingClose && this.ContinuePressed())
                {
                    this.StopTalker();
                    this.disableInput = true;
                    while (!this.waitingForInput && this.canSkip && !this.easingOpen && !this.easingClose && !this.isInTrigger && !this.runRoutine.Finished)
                        this.runRoutine.Update();
                }
                yield return (object) null;
                this.disableInput = false;
            }
        }

        public bool SkipToPage(int page)
        {
            this.autoPressContinue = true;
            while (this.Page != page && !this.runRoutine.Finished)
                this.Update();
            this.autoPressContinue = false;
            this.Update();
            while (this.Opened && (double) this.ease < 1.0)
                this.Update();
            return this.Page == page && this.Opened;
        }

        public void Close()
        {
            this.Opened = false;
            if (this.Scene == null)
                return;
            this.Scene.Remove((Entity) this);
        }

        private bool ContinuePressed()
        {
            if (this.autoPressContinue)
                return true;
            return (Input.MenuConfirm.Pressed || Input.MenuCancel.Pressed) && !this.disableInput;
        }

        public override void Update()
        {
            if (this.Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null))
                return;
            if (!this.autoPressContinue)
                this.skipRoutine.Update();
            this.runRoutine.Update();
            if (this.Scene != null && this.Scene.OnInterval(0.05f))
                this.shakeSeed = Calc.Random.Next();
            if (this.portraitSprite != null && (double) this.ease >= 1.0)
                this.portraitSprite.Update();
            if (this.portraitGlitchy != null && (double) this.ease >= 1.0)
                this.portraitGlitchy.Update();
            this.timer += Engine.DeltaTime;
            this.portraitWiggle.Update();
            int num = Math.Min(this.index, this.Nodes.Count);
            for (int start = this.Start; start < num; ++start)
            {
                if (this.Nodes[start] is FancyText.Char)
                {
                    FancyText.Char node = this.Nodes[start] as FancyText.Char;
                    if ((double) node.Fade < 1.0)
                        node.Fade = Calc.Clamp(node.Fade + 8f * Engine.DeltaTime, 0.0f, 1f);
                }
            }
        }

        public override void Render()
        {
            if (this.Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene))
                return;
            float num1 = Ease.CubeInOut(this.ease);
            if ((double) num1 < 0.05000000074505806)
                return;
            float x1 = 116f;
            Vector2 vector2_1 = new Vector2(x1, x1 / 2f) + this.RenderOffset;
            if (this.RenderOffset == Vector2.Zero)
            {
                if (this.anchor == FancyText.Anchors.Bottom)
                    vector2_1 = new Vector2(x1, (float) (1080.0 - (double) x1 / 2.0 - 272.0));
                else if (this.anchor == FancyText.Anchors.Middle)
                    vector2_1 = new Vector2(x1, 404f);
                vector2_1.Y += (float) (int) (136.0 * (1.0 - (double) num1));
            }
            this.textbox.DrawCentered(vector2_1 + new Vector2(1688f, 272f * num1) / 2f, Color.White, new Vector2(1f, num1));
            if (this.waitingForInput)
            {
                float num2 = this.portrait == null || this.PortraitSide(this.portrait) < 0 ? 1688f : 1432f;
                Vector2 position = new Vector2(vector2_1.X + num2, vector2_1.Y + 272f) + new Vector2(-48f, (float) (((double) this.timer % 1.0 < 0.25 ? 6 : 0) - 40));
                GFX.Gui["textboxbutton"].DrawCentered(position);
            }
            if (this.portraitExists)
            {
                if (this.PortraitSide(this.portrait) > 0)
                {
                    this.portraitSprite.Position = new Vector2((float) ((double) vector2_1.X + 1688.0 - 240.0 - 16.0), vector2_1.Y);
                    this.portraitSprite.Scale.X = -this.portraitScale;
                }
                else
                {
                    this.portraitSprite.Position = new Vector2(vector2_1.X + 16f, vector2_1.Y);
                    this.portraitSprite.Scale.X = this.portraitScale;
                }
                this.portraitSprite.Scale.X *= this.portrait.Flipped ? -1f : 1f;
                this.portraitSprite.Scale.Y = (float) ((double) this.portraitScale * ((272.0 * (double) num1 - 32.0) / 240.0) * (this.portrait.UpsideDown ? -1.0 : 1.0));
                Sprite portraitSprite1 = this.portraitSprite;
                portraitSprite1.Scale = portraitSprite1.Scale * (float) (0.89999997615814209 + (double) this.portraitWiggle.Value * 0.10000000149011612);
                Sprite portraitSprite2 = this.portraitSprite;
                portraitSprite2.Position = portraitSprite2.Position + new Vector2(120f, (float) (272.0 * (double) num1 * 0.5));
                this.portraitSprite.Color = Color.White * num1;
                if ((double) Math.Abs(this.portraitSprite.Scale.Y) > 0.05000000074505806)
                {
                    this.portraitSprite.Render();
                    if (this.isPortraitGlitchy && this.portraitGlitchy != null)
                    {
                        this.portraitGlitchy.Position = this.portraitSprite.Position;
                        this.portraitGlitchy.Origin = this.portraitSprite.Origin;
                        this.portraitGlitchy.Scale = this.portraitSprite.Scale;
                        this.portraitGlitchy.Color = Color.White * 0.2f * num1;
                        this.portraitGlitchy.Render();
                    }
                }
            }
            if (this.textboxOverlay != null)
            {
                int x2 = 1;
                if (this.portrait != null && this.PortraitSide(this.portrait) > 0)
                    x2 = -1;
                this.textboxOverlay.DrawCentered(vector2_1 + new Vector2(1688f, 272f * num1) / 2f, Color.White, new Vector2((float) x2, num1));
            }
            Calc.PushRandom(this.shakeSeed);
            int num3 = 1;
            for (int start = this.Start; start < this.text.Nodes.Count; ++start)
            {
                if (this.text.Nodes[start] is FancyText.NewLine)
                    ++num3;
                else if (this.text.Nodes[start] is FancyText.NewPage)
                    break;
            }
            Vector2 vector2_2 = new Vector2(this.innerTextPadding + (this.portrait == null || this.PortraitSide(this.portrait) >= 0 ? 0.0f : 256f), this.innerTextPadding);
            Vector2 vector2_3 = new Vector2(this.portrait == null ? this.maxLineWidthNoPortrait : this.maxLineWidth, (float) this.linesPerPage * this.lineHeight * num1) / 2f;
            float num4 = num3 >= 4 ? 0.75f : 1f;
            this.text.Draw(vector2_1 + vector2_2 + vector2_3, new Vector2(0.5f, 0.5f), new Vector2(1f, num1) * num4, num1, this.Start);
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
            Engine.Scene.Add((Entity) textbox);
            while (textbox.Opened)
                yield return (object) null;
        }

        public static IEnumerator SayWhileFrozen(
            string dialog,
            params Func<IEnumerator>[] events)
        {
            Textbox textbox = new Textbox(dialog, events);
            textbox.Tag |= (int) Tags.FrozenUpdate;
            Engine.Scene.Add((Entity) textbox);
            while (textbox.Opened)
                yield return (object) null;
        }
    }
}
