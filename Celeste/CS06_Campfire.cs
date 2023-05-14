// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_Campfire
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    public class CS06_Campfire : CutsceneEntity
    {
        public const string Flag = "campfire_chat";
        public const string DuskBackgroundFlag = "duskbg";
        public const string StarsBackgrundFlag = "starsbg";
        private readonly NPC theo;
        private readonly Player player;
        private Bonfire bonfire;
        private Plateau plateau;
        private Vector2 cameraStart;
        private Vector2 playerCampfirePosition;
        private Vector2 theoCampfirePosition;
        private Selfie selfie;
        private float optionEase;
        private readonly Dictionary<string, CS06_Campfire.Option[]> nodes = new();
        private readonly HashSet<CS06_Campfire.Question> asked = new();
        private List<CS06_Campfire.Option> currentOptions = new();
        private int currentOptionIndex;

        public CS06_Campfire(NPC theo, Player player)
            : base()
        {
            Tag = (int)Tags.HUD;
            this.theo = theo;
            this.player = player;
            CS06_Campfire.Question question1 = new("outfor");
            CS06_Campfire.Question question2 = new("temple");
            CS06_Campfire.Question question3 = new("explain");
            CS06_Campfire.Question question4 = new("thankyou");
            CS06_Campfire.Question question5 = new("why");
            CS06_Campfire.Question question6 = new("depression");
            CS06_Campfire.Question question7 = new("defense");
            CS06_Campfire.Question question8 = new("vacation");
            CS06_Campfire.Question question9 = new("trust");
            CS06_Campfire.Question question10 = new("family");
            CS06_Campfire.Question question11 = new("grandpa");
            CS06_Campfire.Question question12 = new("tips");
            CS06_Campfire.Question question13 = new(nameof(selfie));
            CS06_Campfire.Question question14 = new("sleep");
            CS06_Campfire.Question question15 = new("sleep_confirm");
            CS06_Campfire.Question question16 = new("sleep_cancel");
            nodes.Add("start", new CS06_Campfire.Option[14]
            {
                new CS06_Campfire.Option(question1, "start").ExcludedBy(question5),
                new CS06_Campfire.Option(question2, "start").Require(question9),
                new CS06_Campfire.Option(question9, "start").Require(question3),
                new CS06_Campfire.Option(question10, "start").Require(question9, question5),
                new CS06_Campfire.Option(question11, "start").Require(question10, question7),
                new CS06_Campfire.Option(question12, "start").Require(question11),
                new CS06_Campfire.Option(question3, "start"),
                new CS06_Campfire.Option(question4, "start").Require(question3),
                new CS06_Campfire.Option(question5, "start").Require(question3, question9),
                new CS06_Campfire.Option(question6, "start").Require(question5),
                new CS06_Campfire.Option(question7, "start").Require(question6),
                new CS06_Campfire.Option(question8, "start").Require(question6),
                new CS06_Campfire.Option(question13, "").Require(question7, question11),
                new CS06_Campfire.Option(question14, "sleep").Require(question5).ExcludedBy(question7, question11).Repeatable()
            });
            nodes.Add("sleep", new CS06_Campfire.Option[2]
            {
                new CS06_Campfire.Option(question16, "start").Repeatable(),
                new CS06_Campfire.Option(question15, "")
            });
        }

        public override void OnBegin(Level level)
        {
            _ = Audio.SetMusic(null, false, false);
            level.SnapColorGrade(null);
            level.Bloom.Base = 0.0f;
            level.Session.SetFlag("duskbg");
            plateau = Scene.Entities.FindFirst<Plateau>();
            bonfire = Scene.Tracker.GetEntity<Bonfire>();
            level.Camera.Position = new Vector2(level.Bounds.Left, bonfire.Y - 144f);
            level.ZoomSnap(new Vector2(80f, 120f), 2f);
            cameraStart = level.Camera.Position;
            theo.X = level.Camera.X - 48f;
            theoCampfirePosition = new Vector2(bonfire.X - 16f, bonfire.Y);
            player.Light.Alpha = 0.0f;
            player.X = level.Bounds.Left - 40;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            playerCampfirePosition = new Vector2(bonfire.X + 20f, bonfire.Y);
            if (level.Session.GetFlag("campfire_chat"))
            {
                WasSkipped = true;
                level.ResetZoom();
                level.EndCutscene();
                EndCutscene(level);
            }
            else
            {
                Add(new Coroutine(Cutscene(level)));
            }
        }

        private IEnumerator PlayerLightApproach()
        {
            while (player.Light.Alpha < 1.0)
            {
                player.Light.Alpha = Calc.Approach(player.Light.Alpha, 1f, Engine.DeltaTime * 2f);
                yield return null;
            }
        }

        private IEnumerator Cutscene(Level level)
        {
            CS06_Campfire cs06Campfire = this;
            yield return 0.1f;
            cs06Campfire.Add(new Coroutine(cs06Campfire.PlayerLightApproach()));
            Coroutine camTo;
            cs06Campfire.Add(camTo = new Coroutine(CutsceneEntity.CameraTo(new Vector2(level.Camera.X + 90f, level.Camera.Y), 6f, Ease.CubeIn)));
            cs06Campfire.player.DummyAutoAnimate = false;
            cs06Campfire.player.Sprite.Play("carryTheoWalk");
            for (float p = 0.0f; (double)p < 3.5; p += Engine.DeltaTime)
            {
                SpotlightWipe.FocusPoint = new Vector2(40f, 120f);
                cs06Campfire.player.NaiveMove(new Vector2(32f * Engine.DeltaTime, 0.0f));
                yield return null;
            }
            cs06Campfire.player.Sprite.Play("carryTheoCollapse");
            _ = Audio.Play("event:/char/madeline/theo_collapse", cs06Campfire.player.Position);
            yield return 0.3f;
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            Vector2 position = cs06Campfire.player.Position + new Vector2(16f, 1f);
            cs06Campfire.Level.ParticlesFG.Emit(Payphone.P_Snow, 2, position, Vector2.UnitX * 4f);
            cs06Campfire.Level.ParticlesFG.Emit(Payphone.P_SnowB, 12, position, Vector2.UnitX * 10f);
            yield return 0.7f;
            FadeWipe fade = new(level, false)
            {
                Duration = 1.5f,
                EndTimer = 2.5f
            };
            yield return fade.Wait();
            cs06Campfire.bonfire.SetMode(Bonfire.Mode.Lit);
            yield return 2.45f;
            camTo.Cancel();
            cs06Campfire.theo.Position = cs06Campfire.theoCampfirePosition;
            cs06Campfire.theo.Sprite.Play("sleep");
            cs06Campfire.theo.Sprite.SetAnimationFrame(cs06Campfire.theo.Sprite.CurrentAnimationTotalFrames - 1);
            cs06Campfire.player.Position = cs06Campfire.playerCampfirePosition;
            cs06Campfire.player.Facing = Facings.Left;
            cs06Campfire.player.Sprite.Play("asleep");
            level.Session.SetFlag("starsbg");
            level.Session.SetFlag("duskbg", false);
            fade.EndTimer = 0.0f;
            FadeWipe fadeWipe1 = new(level, true);
            yield return null;
            level.ResetZoom();
            level.Camera.Position = new Vector2(cs06Campfire.bonfire.X - 160f, cs06Campfire.bonfire.Y - 140f);
            yield return 3f;
            _ = Audio.SetMusic("event:/music/lvl6/madeline_and_theo");
            yield return 1.5f;
            // ISSUE: reference to a compiler-generated method
            cs06Campfire.Add(Wiggler.Create(0.6f, 3f, delegate (float v)
            {
                theo.Sprite.Scale = Vector2.One * (1f + (0.1f * v));
            }, true, true));
            cs06Campfire.Level.Particles.Emit(NPC01_Theo.P_YOLO, 4, cs06Campfire.theo.Position + new Vector2(-4f, -14f), Vector2.One * 3f);
            yield return 0.5f;
            cs06Campfire.theo.Sprite.Play("wakeup");
            yield return 1f;
            cs06Campfire.player.Sprite.Play("halfWakeUp");
            yield return 0.25f;
            yield return Textbox.Say("ch6_theo_intro");
            string key = "start";
            while (!string.IsNullOrEmpty(key) && cs06Campfire.nodes.ContainsKey(key))
            {
                cs06Campfire.currentOptionIndex = 0;
                cs06Campfire.currentOptions = new List<CS06_Campfire.Option>();
                foreach (CS06_Campfire.Option option in cs06Campfire.nodes[key])
                {
                    if (option.CanAsk(cs06Campfire.asked))
                    {
                        cs06Campfire.currentOptions.Add(option);
                    }
                }
                if (cs06Campfire.currentOptions.Count > 0)
                {
                    _ = Audio.Play("event:/ui/game/chatoptions_appear");
                    while ((double)(cs06Campfire.optionEase += Engine.DeltaTime * 4f) < 1.0)
                    {
                        yield return null;
                    }

                    cs06Campfire.optionEase = 1f;
                    yield return 0.25f;
                    while (!Input.MenuConfirm.Pressed)
                    {
                        if (Input.MenuUp.Pressed && cs06Campfire.currentOptionIndex > 0)
                        {
                            _ = Audio.Play("event:/ui/game/chatoptions_roll_up");
                            --cs06Campfire.currentOptionIndex;
                        }
                        else if (Input.MenuDown.Pressed && cs06Campfire.currentOptionIndex < cs06Campfire.currentOptions.Count - 1)
                        {
                            _ = Audio.Play("event:/ui/game/chatoptions_roll_down");
                            ++cs06Campfire.currentOptionIndex;
                        }
                        yield return null;
                    }
                    _ = Audio.Play("event:/ui/game/chatoptions_select");
                    while ((double)(cs06Campfire.optionEase -= Engine.DeltaTime * 4f) > 0.0)
                    {
                        yield return null;
                    }

                    CS06_Campfire.Option selected = cs06Campfire.currentOptions[cs06Campfire.currentOptionIndex];
                    _ = cs06Campfire.asked.Add(selected.Question);
                    cs06Campfire.currentOptions = null;
                    yield return Textbox.Say(selected.Question.Answer, new Func<IEnumerator>(cs06Campfire.WaitABit), new Func<IEnumerator>(cs06Campfire.SelfieSequence), new Func<IEnumerator>(cs06Campfire.BeerSequence));
                    key = selected.Goto;
                    if (!string.IsNullOrEmpty(key))
                    {
                        selected = null;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            FadeWipe fadeWipe2 = new(level, false)
            {
                Duration = 3f
            };
            yield return fadeWipe2.Wait();
            cs06Campfire.EndCutscene(level);
        }

        private IEnumerator WaitABit()
        {
            yield return 0.8f;
        }

        private IEnumerator SelfieSequence()
        {
            CS06_Campfire cs06Campfire = this;
            cs06Campfire.Add(new Coroutine(cs06Campfire.Level.ZoomTo(new Vector2(160f, 105f), 2f, 0.5f)));
            yield return 0.1f;
            cs06Campfire.theo.Sprite.Play("idle");
            // ISSUE: reference to a compiler-generated method
            cs06Campfire.Add(Alarm.Create(Alarm.AlarmMode.Oneshot, delegate
            {
                theo.Sprite.Scale.X = -1f;
            }, 0.25f, true));
            cs06Campfire.player.DummyAutoAnimate = true;
            yield return cs06Campfire.player.DummyWalkToExact((int)((double)cs06Campfire.theo.X + 5.0), speedMultiplier: 0.7f);
            yield return 0.2f;
            _ = Audio.Play("event:/game/02_old_site/theoselfie_foley", cs06Campfire.theo.Position);
            cs06Campfire.theo.Sprite.Play("takeSelfie");
            yield return 1f;
            cs06Campfire.selfie = new Selfie(cs06Campfire.SceneAs<Level>());
            cs06Campfire.Scene.Add(cs06Campfire.selfie);
            yield return cs06Campfire.selfie.PictureRoutine("selfieCampfire");
            cs06Campfire.selfie = null;
            yield return 0.5f;
            yield return cs06Campfire.Level.ZoomBack(0.5f);
            yield return 0.2f;
            cs06Campfire.theo.Sprite.Scale.X = 1f;
            yield return cs06Campfire.player.DummyWalkToExact((int)cs06Campfire.playerCampfirePosition.X, speedMultiplier: 0.7f);
            cs06Campfire.theo.Sprite.Play("wakeup");
            yield return 0.1;
            cs06Campfire.player.DummyAutoAnimate = false;
            cs06Campfire.player.Facing = Facings.Left;
            cs06Campfire.player.Sprite.Play("sleep");
            yield return 2f;
            cs06Campfire.player.Sprite.Play("halfWakeUp");
        }

        private IEnumerator BeerSequence()
        {
            yield return 0.5f;
        }

        public override void OnEnd(Level level)
        {
            if (!WasSkipped)
            {
                level.ZoomSnap(new Vector2(160f, 120f), 2f);
                FadeWipe fadeWipe = new(level, true)
                {
                    Duration = 3f
                };
                Coroutine zoom = new(level.ZoomBack(fadeWipe.Duration));
                fadeWipe.OnUpdate = f => zoom.Update();
            }
            selfie?.RemoveSelf();
            level.Session.SetFlag("campfire_chat");
            level.Session.SetFlag("starsbg", false);
            level.Session.SetFlag("duskbg", false);
            level.Session.Dreaming = true;
            level.Add(new StarJumpController());
            level.Add(new CS06_StarJumpEnd(theo, player, playerCampfirePosition, cameraStart));
            level.Add(new FlyFeather(level.LevelOffset + new Vector2(272f, 2616f), false, false));
            SetBloom(1f);
            bonfire.Activated = false;
            bonfire.SetMode(Bonfire.Mode.Lit);
            theo.Sprite.Play("sleep");
            theo.Sprite.SetAnimationFrame(theo.Sprite.CurrentAnimationTotalFrames - 1);
            theo.Sprite.Scale.X = 1f;
            theo.Position = theoCampfirePosition;
            player.Sprite.Play("asleep");
            player.Position = playerCampfirePosition;
            player.StateMachine.Locked = false;
            player.StateMachine.State = 15;
            player.Speed = Vector2.Zero;
            player.Facing = Facings.Left;
            level.Camera.Position = player.CameraTarget;
            if (WasSkipped)
            {
                player.StateMachine.State = 0;
            }

            RemoveSelf();
        }

        private void SetBloom(float add)
        {
            Level.Session.BloomBaseAdd = add;
            Level.Bloom.Base = AreaData.Get(Level).BloomBase + add;
        }

        public override void Update()
        {
            if (currentOptions != null)
            {
                for (int index = 0; index < currentOptions.Count; ++index)
                {
                    currentOptions[index].Update();
                    currentOptions[index].Highlight = Calc.Approach(currentOptions[index].Highlight, currentOptionIndex == index ? 1f : 0.0f, Engine.DeltaTime * 4f);
                }
            }
            base.Update();
        }

        public override void Render()
        {
            if (Level.Paused || currentOptions == null)
            {
                return;
            }

            int num = 0;
            foreach (CS06_Campfire.Option currentOption in currentOptions)
            {
                currentOption.Render(new Vector2(260f, (float)(120.0 + (160.0 * num))), optionEase);
                ++num;
            }
        }

        private class Option
        {
            public CS06_Campfire.Question Question;
            public string Goto;
            public List<CS06_Campfire.Question> OnlyAppearIfAsked;
            public List<CS06_Campfire.Question> DoNotAppearIfAsked;
            public bool CanRepeat;
            public float Highlight;
            public const float Width = 1400f;
            public const float Height = 140f;
            public const float Padding = 20f;
            public const float TextScale = 0.7f;

            public Option(CS06_Campfire.Question question, string go)
            {
                Question = question;
                Goto = go;
            }

            public CS06_Campfire.Option Require(
                params CS06_Campfire.Question[] onlyAppearIfAsked)
            {
                OnlyAppearIfAsked = new List<CS06_Campfire.Question>(onlyAppearIfAsked);
                return this;
            }

            public CS06_Campfire.Option ExcludedBy(
                params CS06_Campfire.Question[] doNotAppearIfAsked)
            {
                DoNotAppearIfAsked = new List<CS06_Campfire.Question>(doNotAppearIfAsked);
                return this;
            }

            public CS06_Campfire.Option Repeatable()
            {
                CanRepeat = true;
                return this;
            }

            public bool CanAsk(HashSet<CS06_Campfire.Question> asked)
            {
                if (!CanRepeat && asked.Contains(Question))
                {
                    return false;
                }

                if (OnlyAppearIfAsked != null)
                {
                    foreach (CS06_Campfire.Question question in OnlyAppearIfAsked)
                    {
                        if (!asked.Contains(question))
                        {
                            return false;
                        }
                    }
                }
                if (DoNotAppearIfAsked != null)
                {
                    bool flag = true;
                    foreach (CS06_Campfire.Question question in DoNotAppearIfAsked)
                    {
                        if (!asked.Contains(question))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        return false;
                    }
                }
                return true;
            }

            public void Update()
            {
                Question.Portrait.Update();
            }

            public void Render(Vector2 position, float ease)
            {
                float num1 = Ease.CubeOut(ease);
                float amount = Ease.CubeInOut(Highlight);
                position.Y += (float)(-32.0 * (1.0 - (double)num1));
                position.X += amount * 32f;
                Color color1 = Color.Lerp(Color.Gray, Color.White, amount) * num1;
                float alpha = MathHelper.Lerp(0.6f, 1f, amount) * num1;
                Color color2 = Color.White * (float)(0.5 + ((double)amount * 0.5));
                GFX.Portraits[Question.Textbox].Draw(position, Vector2.Zero, color1);
                Facings facings = Question.PortraitSide;
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                {
                    facings = (Facings)(-(int)facings);
                }

                float num2 = 100f;
                Question.Portrait.Scale = Vector2.One * (num2 / Question.PortraitSize);
                if (facings == Facings.Right)
                {
                    Question.Portrait.Position = position + new Vector2((float)(1380.0 - ((double)num2 * 0.5)), 70f);
                    Question.Portrait.Scale.X *= -1f;
                }
                else
                {
                    Question.Portrait.Position = position + new Vector2((float)(20.0 + ((double)num2 * 0.5)), 70f);
                }

                Question.Portrait.Color = color2 * num1;
                Question.Portrait.Render();
                float num3 = (float)((140.0 - ((double)ActiveFont.LineHeight * 0.699999988079071)) / 2.0);
                Vector2 position1 = new(0.0f, position.Y + 70f);
                Vector2 justify = new(0.0f, 0.5f);
                if (facings == Facings.Right)
                {
                    justify.X = 1f;
                    position1.X = (float)(position.X + 1400.0 - 20.0) - num3 - num2;
                }
                else
                {
                    position1.X = position.X + 20f + num3 + num2;
                }

                Question.AskText.Draw(position1, justify, Vector2.One * 0.7f, alpha);
            }
        }

        private class Question
        {
            public string Ask;
            public string Answer;
            public string Textbox;
            public FancyText.Text AskText;
            public Sprite Portrait;
            public Facings PortraitSide;
            public float PortraitSize;

            public Question(string id)
            {
                int maxLineWidth = 1828;
                Ask = "ch6_theo_ask_" + id;
                Answer = "ch6_theo_say_" + id;
                AskText = FancyText.Parse(Dialog.Get(Ask), maxLineWidth, -1);
                foreach (FancyText.Node node in AskText.Nodes)
                {
                    if (node is FancyText.Portrait)
                    {
                        FancyText.Portrait portrait = node as FancyText.Portrait;
                        Portrait = GFX.PortraitsSpriteBank.Create(portrait.SpriteId);
                        Portrait.Play(portrait.IdleAnimation);
                        PortraitSide = (Facings)portrait.Side;
                        Textbox = "textbox/" + portrait.Sprite + "_ask";
                        XmlElement xml = GFX.PortraitsSpriteBank.SpriteData[portrait.SpriteId].Sources[0].XML;
                        if (xml == null)
                        {
                            break;
                        }

                        PortraitSize = xml.AttrInt("size", 160);
                        break;
                    }
                }
            }
        }
    }
}
