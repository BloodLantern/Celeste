// Decompiled with JetBrains decompiler
// Type: Celeste.OuiAssistMode
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
  public class OuiAssistMode : Oui
  {
    public OuiFileSelectSlot FileSlot;
    private float fade;
    private List<OuiAssistMode.Page> pages = new List<OuiAssistMode.Page>();
    private int pageIndex;
    private int questionIndex = 1;
    private float questionEase;
    private Wiggler wiggler;
    private float dot;
    private FancyText.Text questionText;
    private Color iconColor = Calc.HexToColor("44adf7");
    private float leftArrowEase;
    private float rightArrowEase;
    private EventInstance mainSfx;
    private const float textScale = 0.8f;

    public OuiAssistMode()
    {
      this.Visible = false;
      this.Add((Component) (this.wiggler = Wiggler.Create(0.4f, 4f)));
    }

    public override IEnumerator Enter(Oui from)
    {
      OuiAssistMode ouiAssistMode = this;
      ouiAssistMode.Focused = false;
      ouiAssistMode.Visible = true;
      ouiAssistMode.pageIndex = 0;
      ouiAssistMode.questionIndex = 1;
      ouiAssistMode.questionEase = 0.0f;
      ouiAssistMode.dot = 0.0f;
      ouiAssistMode.questionText = FancyText.Parse(Dialog.Get("ASSIST_ASK"), 1600, -1, defaultColor: new Color?(Color.White));
      if (!ouiAssistMode.FileSlot.AssistModeEnabled)
      {
        for (int index = 0; Dialog.Has("ASSIST_MODE_" + (object) index); ++index)
          ouiAssistMode.pages.Add(new OuiAssistMode.Page()
          {
            Text = FancyText.Parse(Dialog.Get("ASSIST_MODE_" + (object) index), 2000, -1, defaultColor: new Color?(Color.White * 0.9f)),
            Ease = 0.0f
          });
        ouiAssistMode.pages[0].Ease = 1f;
        ouiAssistMode.mainSfx = Audio.Play("event:/ui/main/assist_info_whistle");
      }
      else
        ouiAssistMode.questionEase = 1f;
      while ((double) ouiAssistMode.fade < 1.0)
      {
        ouiAssistMode.fade += Engine.DeltaTime * 4f;
        yield return (object) null;
      }
      ouiAssistMode.Focused = true;
      ouiAssistMode.Add((Component) new Coroutine(ouiAssistMode.InputRoutine()));
    }

    public override IEnumerator Leave(Oui next)
    {
      OuiAssistMode ouiAssistMode = this;
      ouiAssistMode.Focused = false;
      while ((double) ouiAssistMode.fade > 0.0)
      {
        ouiAssistMode.fade -= Engine.DeltaTime * 4f;
        yield return (object) null;
      }
      if ((HandleBase) ouiAssistMode.mainSfx != (HandleBase) null)
      {
        int num = (int) ouiAssistMode.mainSfx.release();
      }
      ouiAssistMode.pages.Clear();
      ouiAssistMode.Visible = false;
    }

    private IEnumerator InputRoutine()
    {
      OuiAssistMode ouiAssistMode = this;
      while (!Input.MenuCancel.Pressed)
      {
        int was = ouiAssistMode.pageIndex;
        if ((Input.MenuConfirm.Pressed || Input.MenuRight.Pressed) && ouiAssistMode.pageIndex < ouiAssistMode.pages.Count)
        {
          ++ouiAssistMode.pageIndex;
          Audio.Play("event:/ui/main/rollover_down");
          Audio.SetParameter(ouiAssistMode.mainSfx, "assist_progress", (float) ouiAssistMode.pageIndex);
        }
        else if (Input.MenuLeft.Pressed && ouiAssistMode.pageIndex > 0)
        {
          Audio.Play("event:/ui/main/rollover_up");
          --ouiAssistMode.pageIndex;
        }
        if (was != ouiAssistMode.pageIndex)
        {
          if (was < ouiAssistMode.pages.Count)
          {
            ouiAssistMode.pages[was].Direction = (float) Math.Sign(was - ouiAssistMode.pageIndex);
            while ((double) (ouiAssistMode.pages[was].Ease = Calc.Approach(ouiAssistMode.pages[was].Ease, 0.0f, Engine.DeltaTime * 8f)) != 0.0)
              yield return (object) null;
          }
          else
          {
            while ((double) (ouiAssistMode.questionEase = Calc.Approach(ouiAssistMode.questionEase, 0.0f, Engine.DeltaTime * 8f)) != 0.0)
              yield return (object) null;
          }
          if (ouiAssistMode.pageIndex < ouiAssistMode.pages.Count)
          {
            ouiAssistMode.pages[ouiAssistMode.pageIndex].Direction = (float) Math.Sign(ouiAssistMode.pageIndex - was);
            while ((double) (ouiAssistMode.pages[ouiAssistMode.pageIndex].Ease = Calc.Approach(ouiAssistMode.pages[ouiAssistMode.pageIndex].Ease, 1f, Engine.DeltaTime * 8f)) != 1.0)
              yield return (object) null;
          }
          else
          {
            while ((double) (ouiAssistMode.questionEase = Calc.Approach(ouiAssistMode.questionEase, 1f, Engine.DeltaTime * 8f)) != 1.0)
              yield return (object) null;
          }
        }
        if (ouiAssistMode.pageIndex >= ouiAssistMode.pages.Count)
        {
          if (Input.MenuConfirm.Pressed)
          {
            ouiAssistMode.FileSlot.AssistModeEnabled = ouiAssistMode.questionIndex == 0;
            if (ouiAssistMode.FileSlot.AssistModeEnabled)
              ouiAssistMode.FileSlot.VariantModeEnabled = false;
            ouiAssistMode.FileSlot.CreateButtons();
            ouiAssistMode.Focused = false;
            ouiAssistMode.Overworld.Goto<OuiFileSelect>();
            Audio.Play(ouiAssistMode.questionIndex == 0 ? "event:/ui/main/assist_button_yes" : "event:/ui/main/assist_button_no");
            Audio.SetParameter(ouiAssistMode.mainSfx, "assist_progress", ouiAssistMode.questionIndex == 0 ? 4f : 5f);
            yield break;
          }
          else if (Input.MenuUp.Pressed && ouiAssistMode.questionIndex > 0)
          {
            Audio.Play("event:/ui/main/rollover_up");
            --ouiAssistMode.questionIndex;
            ouiAssistMode.wiggler.Start();
          }
          else if (Input.MenuDown.Pressed && ouiAssistMode.questionIndex < 1)
          {
            Audio.Play("event:/ui/main/rollover_down");
            ++ouiAssistMode.questionIndex;
            ouiAssistMode.wiggler.Start();
          }
        }
        yield return (object) null;
      }
      ouiAssistMode.Focused = false;
      ouiAssistMode.Overworld.Goto<OuiFileSelect>();
      Audio.Play("event:/ui/main/button_back");
      Audio.SetParameter(ouiAssistMode.mainSfx, "assist_progress", 6f);
    }

    public override void Update()
    {
      this.dot = Calc.Approach(this.dot, (float) this.pageIndex, Engine.DeltaTime * 8f);
      this.leftArrowEase = Calc.Approach(this.leftArrowEase, this.pageIndex > 0 ? 1f : 0.0f, Engine.DeltaTime * 4f);
      this.rightArrowEase = Calc.Approach(this.rightArrowEase, this.pageIndex < this.pages.Count ? 1f : 0.0f, Engine.DeltaTime * 4f);
      base.Update();
    }

    public override void Render()
    {
      if (!this.Visible)
        return;
      Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * this.fade * 0.9f);
      for (int index = 0; index < this.pages.Count; ++index)
      {
        OuiAssistMode.Page page = this.pages[index];
        float num = Ease.CubeOut(page.Ease);
        if ((double) num > 0.0)
        {
          Vector2 position = new Vector2(960f, 620f);
          position.X += (float) ((double) page.Direction * (1.0 - (double) num) * 256.0);
          page.Text.DrawJustifyPerLine(position, new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, num * this.fade);
        }
      }
      if ((double) this.questionEase > 0.0)
      {
        float num1 = Ease.CubeOut(this.questionEase);
        float num2 = this.wiggler.Value * 8f;
        Vector2 position = new Vector2((float) (960.0 + (1.0 - (double) num1) * 256.0), 620f);
        float lineHeight = ActiveFont.LineHeight;
        this.questionText.DrawJustifyPerLine(position, new Vector2(0.5f, 0.0f), Vector2.One, num1 * this.fade);
        ActiveFont.DrawOutline(Dialog.Clean("ASSIST_YES"), position + new Vector2((float) ((this.questionIndex == 0 ? (double) num2 : 0.0) * 1.2000000476837158) * num1, (float) ((double) lineHeight * 1.3999999761581421 + 10.0)), new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, this.SelectionColor(this.questionIndex == 0), 2f, Color.Black * num1 * this.fade);
        ActiveFont.DrawOutline(Dialog.Clean("ASSIST_NO"), position + new Vector2((float) ((this.questionIndex == 1 ? (double) num2 : 0.0) * 1.2000000476837158) * num1, (float) ((double) lineHeight * 2.2000000476837158 + 20.0)), new Vector2(0.5f, 0.0f), Vector2.One * 0.8f, this.SelectionColor(this.questionIndex == 1), 2f, Color.Black * num1 * this.fade);
      }
      if (this.pages.Count > 0)
      {
        int num3 = this.pages.Count + 1;
        MTexture mtexture = GFX.Gui["dot"];
        int num4 = mtexture.Width * num3;
        Vector2 vector2 = new Vector2(960f, (float) (960.0 - 40.0 * (double) Ease.CubeOut(this.fade)));
        for (int index = 0; index < num3; ++index)
          mtexture.DrawCentered(vector2 + new Vector2((float) (-num4 / 2) + (float) mtexture.Width * ((float) index + 0.5f), 0.0f), Color.White * 0.25f);
        float x = (float) (1.0 + (double) Calc.YoYo(this.dot % 1f) * 4.0);
        mtexture.DrawCentered(vector2 + new Vector2((float) (-num4 / 2) + (float) mtexture.Width * (this.dot + 0.5f), 0.0f), this.iconColor, new Vector2(x, 1f));
        GFX.Gui["dotarrow"].DrawCentered(vector2 + new Vector2((float) (-num4 / 2 - 50), (float) (32.0 * (1.0 - (double) Ease.CubeOut(this.leftArrowEase)))), this.iconColor * this.leftArrowEase, new Vector2(-1f, 1f));
        GFX.Gui["dotarrow"].DrawCentered(vector2 + new Vector2((float) (num4 / 2 + 50), (float) (32.0 * (1.0 - (double) Ease.CubeOut(this.rightArrowEase)))), this.iconColor * this.rightArrowEase);
      }
      GFX.Gui["assistmode"].DrawJustified(new Vector2(960f, (float) (540.0 + 64.0 * (double) Ease.CubeOut(this.fade))), new Vector2(0.5f, 1f), this.iconColor * this.fade);
    }

    private Color SelectionColor(bool selected) => selected ? (Settings.Instance.DisableFlashes || this.Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorA : TextMenu.HighlightColorB) * this.fade : Color.White * this.fade;

    private class Page
    {
      public FancyText.Text Text;
      public float Ease;
      public float Direction;
    }
  }
}
