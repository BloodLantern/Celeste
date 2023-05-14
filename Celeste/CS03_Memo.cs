// Decompiled with JetBrains decompiler
// Type: Celeste.CS03_Memo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.IO;

namespace Celeste
{
    public class CS03_Memo : CutsceneEntity
    {
        private const string ReadOnceFlag = "memo_read";
        private readonly Player player;
        private CS03_Memo.MemoPage memo;

        public CS03_Memo(Player player)
            : base()
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Routine()));
        }

        private IEnumerator Routine()
        {
            CS03_Memo cs03Memo = this;
            cs03Memo.player.StateMachine.State = 11;
            cs03Memo.player.StateMachine.Locked = true;
            if (!cs03Memo.Level.Session.GetFlag("memo_read"))
            {
                yield return Textbox.Say("ch3_memo_opening");
                yield return 0.1f;
            }
            cs03Memo.memo = new CS03_Memo.MemoPage();
            cs03Memo.Scene.Add(cs03Memo.memo);
            yield return cs03Memo.memo.EaseIn();
            yield return cs03Memo.memo.Wait();
            yield return cs03Memo.memo.EaseOut();
            cs03Memo.memo = null;
            cs03Memo.EndCutscene(cs03Memo.Level);
        }

        public override void OnEnd(Level level)
        {
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag("memo_read");
            if (memo == null)
            {
                return;
            }

            memo.RemoveSelf();
        }

        private class MemoPage : Entity
        {
            private const float TextScale = 0.75f;
            private const float PaperScale = 1.5f;
            private readonly Atlas atlas;
            private readonly MTexture paper;
            private readonly MTexture title;
            private VirtualRenderTarget target;
            private readonly FancyText.Text text;
            private readonly float textDownscale = 1f;
            private float alpha = 1f;
            private readonly float scale = 1f;
            private float rotation;
            private float timer;
            private bool easingOut;

            public MemoPage()
            {
                Tag = (int)Tags.HUD;
                atlas = Atlas.FromAtlas(Path.Combine("Graphics", "Atlases", "Memo"), Atlas.AtlasDataFormat.Packer);
                paper = atlas["memo"];
                title = !atlas.Has("title_" + Settings.Instance.Language) ? atlas["title_english"] : atlas["title_" + Settings.Instance.Language];
                float num1 = (float)((paper.Width * 1.5) - 120.0);
                text = FancyText.Parse(Dialog.Get("CH3_MEMO"), (int)((double)num1 / 0.75), -1, defaultColor: new Color?(Color.Black * 0.6f));
                float num2 = text.WidestLine() * 0.75f;
                if ((double)num2 > (double)num1)
                {
                    textDownscale = num1 / num2;
                }

                Add(new BeforeRenderHook(new Action(BeforeRender)));
            }

            public IEnumerator EaseIn()
            {
                CS03_Memo.MemoPage memoPage = this;
                _ = Audio.Play("event:/game/03_resort/memo_in");
                Vector2 from = new(Engine.Width / 2, Engine.Height + 100);
                Vector2 to = new(Engine.Width / 2, (Engine.Height / 2) - 150);
                float rFrom = -0.1f;
                float rTo = 0.05f;
                for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime)
                {
                    memoPage.Position = from + ((to - from) * Ease.CubeOut(p));
                    memoPage.alpha = Ease.CubeOut(p);
                    memoPage.rotation = rFrom + ((rTo - rFrom) * Ease.CubeOut(p));
                    yield return null;
                }
            }

            public IEnumerator Wait()
            {
                CS03_Memo.MemoPage memoPage = this;
                float start = memoPage.Position.Y;
                int index = 0;
                while (!Input.MenuCancel.Pressed)
                {
                    float num = start - (index * 400);
                    memoPage.Position.Y += (float)(((double)num - memoPage.Position.Y) * (1.0 - Math.Pow(0.0099999997764825821, (double)Engine.DeltaTime)));
                    if (Input.MenuUp.Pressed && index > 0)
                    {
                        --index;
                    }
                    else if (index < 2)
                    {
                        if ((Input.MenuDown.Pressed && !Input.MenuDown.Repeating) || Input.MenuConfirm.Pressed)
                        {
                            ++index;
                        }
                    }
                    else if (Input.MenuConfirm.Pressed)
                    {
                        break;
                    }

                    yield return null;
                }
                _ = Audio.Play("event:/ui/main/button_lowkey");
            }

            public IEnumerator EaseOut()
            {
                CS03_Memo.MemoPage memoPage = this;
                _ = Audio.Play("event:/game/03_resort/memo_out");
                memoPage.easingOut = true;
                Vector2 from = memoPage.Position;
                Vector2 to = new(Engine.Width / 2, -memoPage.target.Height);
                float rFrom = memoPage.rotation;
                float rTo = memoPage.rotation + 0.1f;
                for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime * 1.5f)
                {
                    memoPage.Position = from + ((to - from) * Ease.CubeIn(p));
                    memoPage.alpha = 1f - Ease.CubeIn(p);
                    memoPage.rotation = rFrom + ((rTo - rFrom) * Ease.CubeIn(p));
                    yield return null;
                }
                memoPage.RemoveSelf();
            }

            public void BeforeRender()
            {
                target ??= VirtualContent.CreateRenderTarget("oshiro-memo", (int)(paper.Width * 1.5), (int)(paper.Height * 1.5));
                Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)target);
                Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                paper.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
                title.Draw(Vector2.Zero, Vector2.Zero, Color.White, 1.5f);
                text.Draw(new Vector2((float)(paper.Width * 1.5 / 2.0), 210f), new Vector2(0.5f, 0.0f), Vector2.One * 0.75f * textDownscale, 1f);
                Draw.SpriteBatch.End();
            }

            public override void Removed(Scene scene)
            {
                target?.Dispose();
                target = null;
                atlas.Dispose();
                base.Removed(scene);
            }

            public override void SceneEnd(Scene scene)
            {
                target?.Dispose();
                target = null;
                atlas.Dispose();
                base.SceneEnd(scene);
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                base.Update();
            }

            public override void Render()
            {
                if ((Scene is Level scene && (scene.FrozenOrPaused || scene.RetryPlayerCorpse != null || scene.SkippingCutscene)) || target == null)
                {
                    return;
                }

                Draw.SpriteBatch.Draw((RenderTarget2D)target, Position, new Rectangle?(target.Bounds), Color.White * alpha, rotation, new Vector2(target.Width, 0.0f) / 2f, scale, SpriteEffects.None, 0.0f);
                if (easingOut)
                {
                    return;
                }

                GFX.Gui["textboxbutton"].DrawCentered(Position + new Vector2((target.Width / 2) + 40, target.Height + (timer % 1.0 < 0.25 ? 6 : 0)));
            }
        }
    }
}
