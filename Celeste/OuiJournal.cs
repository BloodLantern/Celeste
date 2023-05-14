// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiJournal : Oui
    {
        private const float onScreenX = 0.0f;
        private const float offScreenX = -1920f;
        public bool PageTurningLocked;
        public List<OuiJournalPage> Pages = new();
        public int PageIndex;
        public VirtualRenderTarget CurrentPageBuffer;
        public VirtualRenderTarget NextPageBuffer;
        private bool turningPage;
        private float turningScale;
        private Color backColor = Color.Lerp(Color.White, Color.Black, 0.2f);
        private bool fromAreaInspect;
        private float rotation;
        private MountainCamera cameraStart;
        private MountainCamera cameraEnd;
        private readonly MTexture arrow = MTN.Journal["pageArrow"];
        private float dot;
        private float dotTarget;
        private float dotEase;
        private float leftArrowEase;
        private float rightArrowEase;

        public OuiJournalPage Page => Pages[PageIndex];

        public OuiJournalPage NextPage => Pages[PageIndex + 1];

        public OuiJournalPage PrevPage => Pages[PageIndex - 1];

        public override IEnumerator Enter(Oui from)
        {
            OuiJournal journal = this;
            //Stats.MakeRequest();
            journal.Overworld.ShowConfirmUI = false;
            journal.fromAreaInspect = from is OuiChapterPanel;
            journal.PageIndex = 0;
            journal.Visible = true;
            journal.X = -1920f;
            journal.turningPage = false;
            journal.turningScale = 1f;
            journal.rotation = 0.0f;
            journal.dot = 0.0f;
            journal.dotTarget = 0.0f;
            journal.dotEase = 0.0f;
            journal.leftArrowEase = 0.0f;
            journal.rightArrowEase = 0.0f;
            journal.NextPageBuffer = VirtualContent.CreateRenderTarget("journal-a", 1610, 1000);
            journal.CurrentPageBuffer = VirtualContent.CreateRenderTarget("journal-b", 1610, 1000);
            journal.Pages.Add(new OuiJournalCover(journal));
            journal.Pages.Add(new OuiJournalProgress(journal));
            journal.Pages.Add(new OuiJournalSpeedrun(journal));
            journal.Pages.Add(new OuiJournalDeaths(journal));
            journal.Pages.Add(new OuiJournalPoem(journal));
            if (Stats.Has())
            {
                journal.Pages.Add(new OuiJournalGlobal(journal));
            }

            int num1 = 0;
            foreach (OuiJournalPage page in journal.Pages)
            {
                page.PageIndex = num1++;
            }

            journal.Pages[0].Redraw(journal.CurrentPageBuffer);
            journal.cameraStart = journal.Overworld.Mountain.UntiltedCamera;
            journal.cameraEnd = journal.cameraStart;
            journal.cameraEnd.Position += -journal.cameraStart.Rotation.Forward() * 1f;
            double num2 = (double)journal.Overworld.Mountain.EaseCamera(journal.Overworld.Mountain.Area, journal.cameraEnd, new float?(2f));
            journal.Overworld.Mountain.AllowUserRotation = false;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / 0.4f)
            {
                journal.rotation = -0.025f * Ease.BackOut(p);
                journal.X = (float)((1920.0 * (double)Ease.CubeInOut(p)) - 1920.0);
                journal.dotEase = p;
                yield return null;
            }
            journal.dotEase = 1f;
        }

        public override void HandleGraphicsReset()
        {
            base.HandleGraphicsReset();
            if (Pages.Count <= 0)
            {
                return;
            }

            Page.Redraw(CurrentPageBuffer);
        }

        public IEnumerator TurnPage(int direction)
        {
            turningPage = true;
            if (direction < 0)
            {
                --PageIndex;
                turningScale = -1f;
                --dotTarget;
                Page.Redraw(CurrentPageBuffer);
                NextPage.Redraw(NextPageBuffer);
                while ((double)(turningScale = Calc.Approach(turningScale, 1f, Engine.DeltaTime * 8f)) < 1.0)
                {
                    yield return null;
                }
            }
            else
            {
                NextPage.Redraw(NextPageBuffer);
                turningScale = 1f;
                ++dotTarget;
                while ((double)(turningScale = Calc.Approach(turningScale, -1f, Engine.DeltaTime * 8f)) > -1.0)
                {
                    yield return null;
                }

                ++PageIndex;
                Page.Redraw(CurrentPageBuffer);
            }
            turningScale = 1f;
            turningPage = false;
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiJournal ouiJournal = this;
            _ = Audio.Play("event:/ui/world_map/journal/back");
            _ = (double)ouiJournal.Overworld.Mountain.EaseCamera(ouiJournal.Overworld.Mountain.Area, ouiJournal.cameraStart, new float?(0.4f));
            UserIO.SaveHandler(false, true);
            yield return ouiJournal.EaseOut(0.4f);
            while (UserIO.Saving)
            {
                yield return null;
            }

            ouiJournal.CurrentPageBuffer.Dispose();
            ouiJournal.NextPageBuffer.Dispose();
            ouiJournal.Overworld.ShowConfirmUI = true;
            ouiJournal.Pages.Clear();
            ouiJournal.Visible = false;
            ouiJournal.Overworld.Mountain.AllowUserRotation = true;
        }

        private IEnumerator EaseOut(float duration)
        {
            OuiJournal ouiJournal = this;
            float rotFrom = ouiJournal.rotation;
            for (float p = 0.0f; (double)p < 1.0; p += Engine.DeltaTime / duration)
            {
                ouiJournal.rotation = rotFrom * (1f - Ease.BackOut(p));
                ouiJournal.X = (float)(0.0 + (-1920.0 * (double)Ease.CubeInOut(p)));
                ouiJournal.dotEase = 1f - p;
                yield return null;
            }
            ouiJournal.dotEase = 0.0f;
        }

        public override void Update()
        {
            base.Update();
            dot = Calc.Approach(dot, dotTarget, Engine.DeltaTime * 8f);
            leftArrowEase = Calc.Approach(leftArrowEase, dotTarget > 0.0 ? 1f : 0.0f, Engine.DeltaTime * 5f) * dotEase;
            rightArrowEase = Calc.Approach(rightArrowEase, dotTarget < (double)(Pages.Count - 1) ? 1f : 0.0f, Engine.DeltaTime * 5f) * dotEase;
            if (!Focused || turningPage)
            {
                return;
            }

            Page.Update();
            if (!PageTurningLocked)
            {
                if (Input.MenuLeft.Pressed && PageIndex > 0)
                {
                    _ = PageIndex == 1
                        ? Audio.Play("event:/ui/world_map/journal/page_cover_back")
                        : Audio.Play("event:/ui/world_map/journal/page_main_back");

                    Add(new Coroutine(TurnPage(-1)));
                }
                else if (Input.MenuRight.Pressed && PageIndex < Pages.Count - 1)
                {
                    _ = PageIndex == 0
                        ? Audio.Play("event:/ui/world_map/journal/page_cover_forward")
                        : Audio.Play("event:/ui/world_map/journal/page_main_forward");

                    Add(new Coroutine(TurnPage(1)));
                }
            }
            if (PageTurningLocked || (!Input.MenuJournal.Pressed && !Input.MenuCancel.Pressed))
            {
                return;
            }

            Close();
        }

        private void Close()
        {
            _ = fromAreaInspect ? Overworld.Goto<OuiChapterPanel>() : Overworld.Goto<OuiChapterSelect>();
        }

        public override void Render()
        {
            Vector2 position = Position + new Vector2(128f, 120f);
            float x1 = Ease.CubeInOut(Math.Max(0.0f, turningScale));
            float num1 = Ease.CubeInOut(Math.Abs(Math.Min(0.0f, turningScale)));
            if (SaveData.Instance.CheatMode)
            {
                MTN.FileSelect["cheatmode"].DrawCentered(position + new Vector2(80f, 360f), Color.White, 1f, 1.57079637f);
            }

            if (SaveData.Instance.AssistMode)
            {
                MTN.FileSelect["assist"].DrawCentered(position + new Vector2(100f, 370f), Color.White, 1f, 1.57079637f);
            }

            MTexture mtexture1 = MTN.Journal["edge"];
            mtexture1.Draw(position + new Vector2(-mtexture1.Width, 0.0f), Vector2.Zero, Color.White, 1f, rotation);
            if (PageIndex > 0)
            {
                MTN.Journal[PrevPage.PageTexture].Draw(position, Vector2.Zero, backColor, new Vector2(-1f, 1f), rotation);
            }

            if (turningPage)
            {
                MTN.Journal[NextPage.PageTexture].Draw(position, Vector2.Zero, Color.White, 1f, rotation);
                Draw.SpriteBatch.Draw((RenderTarget2D)NextPageBuffer, position, new Rectangle?(NextPageBuffer.Bounds), Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0.0f);
            }
            if (turningPage && (double)num1 > 0.0)
            {
                MTN.Journal[Page.PageTexture].Draw(position, Vector2.Zero, backColor, new Vector2(-1f * num1, 1f), rotation);
            }

            if ((double)x1 > 0.0)
            {
                MTN.Journal[Page.PageTexture].Draw(position, Vector2.Zero, Color.White, new Vector2(x1, 1f), rotation);
                Draw.SpriteBatch.Draw((RenderTarget2D)CurrentPageBuffer, position, new Rectangle?(CurrentPageBuffer.Bounds), Color.White, rotation, Vector2.Zero, new Vector2(x1, 1f), SpriteEffects.None, 0.0f);
            }
            if (Pages.Count <= 0)
            {
                return;
            }

            int count = Pages.Count;
            MTexture mtexture2 = GFX.Gui["dot_outline"];
            int num2 = mtexture2.Width * count;
            Vector2 vector2 = new(960f, (float)(1040.0 - (40.0 * (double)Ease.CubeOut(dotEase))));
            for (int index = 0; index < count; ++index)
            {
                mtexture2.DrawCentered(vector2 + new Vector2((-num2 / 2) + (mtexture2.Width * (index + 0.5f)), 0.0f), Color.White * 0.25f);
            }

            float x2 = (float)(1.0 + ((double)Calc.YoYo(dot % 1f) * 4.0));
            mtexture2.DrawCentered(vector2 + new Vector2((-num2 / 2) + (mtexture2.Width * (dot + 0.5f)), 0.0f), Color.White, new Vector2(x2, 1f));
            GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2((-num2 / 2) - 50, (float)(32.0 * (1.0 - (double)Ease.CubeOut(leftArrowEase)))), Color.White * leftArrowEase, new Vector2(-1f, 1f));
            GFX.Gui["dotarrow_outline"].DrawCentered(vector2 + new Vector2((num2 / 2) + 50, (float)(32.0 * (1.0 - (double)Ease.CubeOut(rightArrowEase)))), Color.White * rightArrowEase);
        }
    }
}
