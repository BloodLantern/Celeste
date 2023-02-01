// Decompiled with JetBrains decompiler
// Type: Celeste.OuiFileNaming
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
  public class OuiFileNaming : Oui
  {
    public string StartingName;
    public OuiFileSelectSlot FileSlot;
    public const int MinNameLength = 1;
    public const int MaxNameLengthNormal = 12;
    public const int MaxNameLengthJP = 8;
    private string[] letters;
    private int index;
    private int line;
    private float widestLetter;
    private float widestLine;
    private int widestLineCount;
    private bool selectingOptions = true;
    private int optionsIndex;
    private bool hiragana = true;
    private float lineHeight;
    private float lineSpacing;
    private float boxPadding;
    private float optionsScale;
    private string cancel;
    private string space;
    private string backspace;
    private string accept;
    private float cancelWidth;
    private float spaceWidth;
    private float backspaceWidth;
    private float beginWidth;
    private float optionsWidth;
    private float boxWidth;
    private float boxHeight;
    private float pressedTimer;
    private float timer;
    private float ease;
    private Wiggler wiggler;
    private static int[] dakuten_able = new int[40]
    {
      12363,
      12365,
      12367,
      12369,
      12371,
      12373,
      12375,
      12377,
      12379,
      12381,
      12383,
      12385,
      12388,
      12390,
      12392,
      12399,
      12402,
      12405,
      12408,
      12411,
      12459,
      12461,
      12463,
      12465,
      12467,
      12469,
      12471,
      12473,
      12475,
      12477,
      12479,
      12481,
      12484,
      12486,
      12488,
      12495,
      12498,
      12501,
      12504,
      12507
    };
    private static int[] handakuten_able = new int[10]
    {
      12400,
      12403,
      12406,
      12409,
      12412,
      12496,
      12499,
      12502,
      12505,
      12508
    };
    private Color unselectColor = Color.LightGray;
    private Color selectColorA = Calc.HexToColor("84FF54");
    private Color selectColorB = Calc.HexToColor("FCFF59");
    private Color disableColor = Color.DarkSlateBlue;

    public string Name
    {
      get => this.FileSlot.Name;
      set => this.FileSlot.Name = value;
    }

    public int MaxNameLength => !this.Japanese ? 12 : 8;

    public bool Japanese => Settings.Instance.Language == "japanese";

    private Vector2 boxtopleft => this.Position + new Vector2((float) ((1920.0 - (double) this.boxWidth) / 2.0), (float) (360.0 + (680.0 - (double) this.boxHeight) / 2.0));

    public OuiFileNaming()
    {
      this.wiggler = Wiggler.Create(0.25f, 4f);
      this.Position = new Vector2(0.0f, 1080f);
      this.Visible = false;
    }

    public override IEnumerator Enter(Oui from)
    {
      OuiFileNaming ouiFileNaming = this;
      if (ouiFileNaming.Name == Dialog.Clean("FILE_DEFAULT") || Settings.Instance != null && ouiFileNaming.Name == Settings.Instance.DefaultFileName)
        ouiFileNaming.Name = "";
      ouiFileNaming.Overworld.ShowInputUI = false;
      ouiFileNaming.selectingOptions = false;
      ouiFileNaming.optionsIndex = 0;
      ouiFileNaming.index = 0;
      ouiFileNaming.line = 0;
      ouiFileNaming.ReloadLetters(Dialog.Clean("name_letters"));
      ouiFileNaming.optionsScale = 0.75f;
      ouiFileNaming.cancel = Dialog.Clean("name_back");
      ouiFileNaming.space = Dialog.Clean("name_space");
      ouiFileNaming.backspace = Dialog.Clean("name_backspace");
      ouiFileNaming.accept = Dialog.Clean("name_accept");
      ouiFileNaming.cancelWidth = ActiveFont.Measure(ouiFileNaming.cancel).X * ouiFileNaming.optionsScale;
      ouiFileNaming.spaceWidth = ActiveFont.Measure(ouiFileNaming.space).X * ouiFileNaming.optionsScale;
      ouiFileNaming.backspaceWidth = ActiveFont.Measure(ouiFileNaming.backspace).X * ouiFileNaming.optionsScale;
      ouiFileNaming.beginWidth = (float) ((double) ActiveFont.Measure(ouiFileNaming.accept).X * (double) ouiFileNaming.optionsScale * 1.25);
      ouiFileNaming.optionsWidth = (float) ((double) ouiFileNaming.cancelWidth + (double) ouiFileNaming.spaceWidth + (double) ouiFileNaming.backspaceWidth + (double) ouiFileNaming.beginWidth + (double) ouiFileNaming.widestLetter * 3.0);
      ouiFileNaming.Visible = true;
      Vector2 posFrom = ouiFileNaming.Position;
      Vector2 posTo = Vector2.Zero;
      for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime * 3f)
      {
        ouiFileNaming.ease = Ease.CubeIn(t);
        ouiFileNaming.Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
        yield return (object) null;
      }
      ouiFileNaming.ease = 1f;
      posFrom = new Vector2();
      posTo = new Vector2();
      yield return (object) 0.05f;
      ouiFileNaming.Focused = true;
      yield return (object) 0.05f;
      ouiFileNaming.wiggler.Start();
    }

    private void ReloadLetters(string chars)
    {
      this.letters = chars.Split('\n');
      this.widestLetter = 0.0f;
      foreach (char text in chars)
      {
        float x = ActiveFont.Measure(text).X;
        if ((double) x > (double) this.widestLetter)
          this.widestLetter = x;
      }
      if (this.Japanese)
        this.widestLetter *= 1.5f;
      this.widestLineCount = 0;
      foreach (string letter in this.letters)
      {
        if (letter.Length > this.widestLineCount)
          this.widestLineCount = letter.Length;
      }
      this.widestLine = (float) this.widestLineCount * this.widestLetter;
      this.lineHeight = ActiveFont.LineHeight;
      this.lineSpacing = ActiveFont.LineHeight * 0.1f;
      this.boxPadding = this.widestLetter;
      this.boxWidth = Math.Max(this.widestLine, this.optionsWidth) + this.boxPadding * 2f;
      this.boxHeight = (float) ((double) (this.letters.Length + 1) * (double) this.lineHeight + (double) this.letters.Length * (double) this.lineSpacing + (double) this.boxPadding * 3.0);
    }

    public override IEnumerator Leave(Oui next)
    {
      OuiFileNaming ouiFileNaming = this;
      ouiFileNaming.Overworld.ShowInputUI = true;
      ouiFileNaming.Focused = false;
      Vector2 posFrom = ouiFileNaming.Position;
      Vector2 posTo = new Vector2(0.0f, 1080f);
      for (float t = 0.0f; (double) t < 1.0; t += Engine.DeltaTime * 2f)
      {
        ouiFileNaming.ease = 1f - Ease.CubeIn(t);
        ouiFileNaming.Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
        yield return (object) null;
      }
      ouiFileNaming.Visible = false;
    }

    public override void Update()
    {
      base.Update();
      if (this.Selected && this.Focused)
      {
        if (!string.IsNullOrWhiteSpace(this.Name) && MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S))
          this.ResetDefaultName();
        if (Celeste.Input.MenuJournal.Pressed && this.Japanese)
          this.SwapType();
        if (Celeste.Input.MenuRight.Pressed && (this.optionsIndex < 3 || !this.selectingOptions) && (this.Name.Length > 0 || !this.selectingOptions))
        {
          if (this.selectingOptions)
          {
            this.optionsIndex = Math.Min(this.optionsIndex + 1, 3);
          }
          else
          {
            do
            {
              this.index = (this.index + 1) % this.letters[this.line].Length;
            }
            while (this.letters[this.line][this.index] == ' ');
          }
          this.wiggler.Start();
          Audio.Play("event:/ui/main/rename_entry_rollover");
        }
        else if (Celeste.Input.MenuLeft.Pressed && (this.optionsIndex > 0 || !this.selectingOptions))
        {
          if (this.selectingOptions)
          {
            this.optionsIndex = Math.Max(this.optionsIndex - 1, 0);
          }
          else
          {
            do
            {
              this.index = (this.index + this.letters[this.line].Length - 1) % this.letters[this.line].Length;
            }
            while (this.letters[this.line][this.index] == ' ');
          }
          this.wiggler.Start();
          Audio.Play("event:/ui/main/rename_entry_rollover");
        }
        else if (Celeste.Input.MenuDown.Pressed && !this.selectingOptions)
        {
          for (int index = this.line + 1; index < this.letters.Length; ++index)
          {
            if (this.index < this.letters[index].Length && this.letters[index][this.index] != ' ')
            {
              this.line = index;
              goto label_22;
            }
          }
          this.selectingOptions = true;
label_22:
          if (this.selectingOptions)
          {
            float num1 = (float) this.index * this.widestLetter;
            float num2 = this.boxWidth - this.boxPadding * 2f;
            this.optionsIndex = this.Name.Length == 0 || (double) num1 < (double) this.cancelWidth + ((double) num2 - (double) this.cancelWidth - (double) this.beginWidth - (double) this.backspaceWidth - (double) this.spaceWidth - (double) this.widestLetter * 3.0) / 2.0 ? 0 : ((double) num1 >= (double) num2 - (double) this.beginWidth - (double) this.backspaceWidth - (double) this.widestLetter * 2.0 ? ((double) num1 >= (double) num2 - (double) this.beginWidth - (double) this.widestLetter ? 3 : 2) : 1);
          }
          this.wiggler.Start();
          Audio.Play("event:/ui/main/rename_entry_rollover");
        }
        else if ((Celeste.Input.MenuUp.Pressed || this.selectingOptions && this.Name.Length <= 0 && this.optionsIndex > 0) && (this.line > 0 || this.selectingOptions))
        {
          if (this.selectingOptions)
          {
            this.line = this.letters.Length;
            this.selectingOptions = false;
            float num = this.boxWidth - this.boxPadding * 2f;
            if (this.optionsIndex == 0)
              this.index = (int) ((double) this.cancelWidth / 2.0 / (double) this.widestLetter);
            else if (this.optionsIndex == 1)
              this.index = (int) (((double) num - (double) this.beginWidth - (double) this.backspaceWidth - (double) this.spaceWidth / 2.0 - (double) this.widestLetter * 2.0) / (double) this.widestLetter);
            else if (this.optionsIndex == 2)
              this.index = (int) (((double) num - (double) this.beginWidth - (double) this.backspaceWidth / 2.0 - (double) this.widestLetter) / (double) this.widestLetter);
            else if (this.optionsIndex == 3)
              this.index = (int) (((double) num - (double) this.beginWidth / 2.0) / (double) this.widestLetter);
          }
          --this.line;
          while (this.line > 0 && (this.index >= this.letters[this.line].Length || this.letters[this.line][this.index] == ' '))
            --this.line;
          while (this.index >= this.letters[this.line].Length || this.letters[this.line][this.index] == ' ')
            --this.index;
          this.wiggler.Start();
          Audio.Play("event:/ui/main/rename_entry_rollover");
        }
        else if (Celeste.Input.MenuConfirm.Pressed)
        {
          if (this.selectingOptions)
          {
            if (this.optionsIndex == 0)
              this.Cancel();
            else if (this.optionsIndex == 1 && this.Name.Length > 0)
              this.Space();
            else if (this.optionsIndex == 2)
              this.Backspace();
            else if (this.optionsIndex == 3)
              this.Finish();
          }
          else if (this.Japanese && this.letters[this.line][this.index] == '゛' && this.Name.Length > 0 && ((IEnumerable<int>) OuiFileNaming.dakuten_able).Contains<int>((int) this.Name.Last<char>()))
          {
            int num = (int) this.Name[this.Name.Length - 1] + 1;
            this.Name = this.Name.Substring(0, this.Name.Length - 1);
            this.Name += ((char) num).ToString();
            this.wiggler.Start();
            Audio.Play("event:/ui/main/rename_entry_char");
          }
          else if (this.Japanese && this.letters[this.line][this.index] == '゜' && this.Name.Length > 0 && (((IEnumerable<int>) OuiFileNaming.handakuten_able).Contains<int>((int) this.Name.Last<char>()) || ((IEnumerable<int>) OuiFileNaming.handakuten_able).Contains<int>((int) this.Name.Last<char>() + 1)))
          {
            int num3 = (int) this.Name[this.Name.Length - 1];
            int num4 = !((IEnumerable<int>) OuiFileNaming.handakuten_able).Contains<int>(num3) ? num3 + 2 : num3 + 1;
            this.Name = this.Name.Substring(0, this.Name.Length - 1);
            this.Name += ((char) num4).ToString();
            this.wiggler.Start();
            Audio.Play("event:/ui/main/rename_entry_char");
          }
          else if (this.Name.Length < this.MaxNameLength)
          {
            this.Name += this.letters[this.line][this.index].ToString();
            this.wiggler.Start();
            Audio.Play("event:/ui/main/rename_entry_char");
          }
          else
            Audio.Play("event:/ui/main/button_invalid");
        }
        else if (Celeste.Input.MenuCancel.Pressed)
        {
          if (this.Name.Length > 0)
            this.Backspace();
          else
            this.Cancel();
        }
        else if (Celeste.Input.Pause.Pressed)
          this.Finish();
      }
      this.pressedTimer -= Engine.DeltaTime;
      this.timer += Engine.DeltaTime;
      this.wiggler.Update();
    }

    private void ResetDefaultName()
    {
      if (this.StartingName == Settings.Instance.DefaultFileName || this.StartingName == Dialog.Clean("FILE_DEFAULT"))
        this.StartingName = this.Name;
      Settings.Instance.DefaultFileName = this.Name;
      Audio.Play("event:/new_content/ui/rename_entry_accept_locked");
    }

    private void Space()
    {
      if (this.Name.Length < this.MaxNameLength && this.Name.Length > 0)
      {
        this.Name += " ";
        this.wiggler.Start();
        Audio.Play("event:/ui/main/rename_entry_char");
      }
      else
        Audio.Play("event:/ui/main/button_invalid");
    }

    private void Backspace()
    {
      if (this.Name.Length > 0)
      {
        this.Name = this.Name.Substring(0, this.Name.Length - 1);
        Audio.Play("event:/ui/main/rename_entry_backspace");
      }
      else
        Audio.Play("event:/ui/main/button_invalid");
    }

    private void Finish()
    {
      if (this.Name.Length >= 1)
      {
        if (MInput.GamePads.Length != 0 && MInput.GamePads[0] != null && (MInput.GamePads[0].Check(Buttons.LeftTrigger) || MInput.GamePads[0].Check(Buttons.LeftShoulder)) && (MInput.GamePads[0].Check(Buttons.RightTrigger) || MInput.GamePads[0].Check(Buttons.RightShoulder)))
          this.ResetDefaultName();
        this.Focused = false;
        this.Overworld.Goto<OuiFileSelect>();
        Audio.Play("event:/ui/main/rename_entry_accept");
      }
      else
        Audio.Play("event:/ui/main/button_invalid");
    }

    private void SwapType()
    {
      this.hiragana = !this.hiragana;
      if (this.hiragana)
        this.ReloadLetters(Dialog.Clean("name_letters"));
      else
        this.ReloadLetters(Dialog.Clean("name_letters_katakana"));
    }

    private void Cancel()
    {
      this.FileSlot.Name = this.StartingName;
      this.Focused = false;
      this.Overworld.Goto<OuiFileSelect>();
      Audio.Play("event:/ui/main/button_back");
    }

    public override void Render()
    {
      Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.8f * this.ease);
      Vector2 vector2 = this.boxtopleft + new Vector2(this.boxPadding, this.boxPadding);
      int num1 = 0;
      foreach (string letter in this.letters)
      {
        for (int index = 0; index < letter.Length; ++index)
        {
          bool selected = num1 == this.line && index == this.index && !this.selectingOptions;
          Vector2 scale = Vector2.One * (selected ? 1.2f : 1f);
          Vector2 at = vector2 + new Vector2(this.widestLetter, this.lineHeight) / 2f;
          if (selected)
            at += new Vector2(0.0f, this.wiggler.Value) * 8f;
          this.DrawOptionText(letter[index].ToString(), at, new Vector2(0.5f, 0.5f), scale, selected);
          vector2.X += this.widestLetter;
        }
        vector2.X = this.boxtopleft.X + this.boxPadding;
        vector2.Y += this.lineHeight + this.lineSpacing;
        ++num1;
      }
      float num2 = this.wiggler.Value * 8f;
      vector2.Y = this.boxtopleft.Y + this.boxHeight - this.lineHeight - this.boxPadding;
      Draw.Rect(vector2.X, vector2.Y - this.boxPadding * 0.5f, this.boxWidth - this.boxPadding * 2f, 4f, Color.White);
      this.DrawOptionText(this.cancel, vector2 + new Vector2(0.0f, this.lineHeight + (!this.selectingOptions || this.optionsIndex != 0 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * this.optionsScale, this.selectingOptions && this.optionsIndex == 0);
      vector2.X = this.boxtopleft.X + this.boxWidth - this.backspaceWidth - this.widestLetter - this.spaceWidth - this.widestLetter - this.beginWidth - this.boxPadding;
      this.DrawOptionText(this.space, vector2 + new Vector2(0.0f, this.lineHeight + (!this.selectingOptions || this.optionsIndex != 1 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * this.optionsScale, this.selectingOptions && this.optionsIndex == 1, this.Name.Length == 0 || !this.Focused);
      vector2.X += this.spaceWidth + this.widestLetter;
      this.DrawOptionText(this.backspace, vector2 + new Vector2(0.0f, this.lineHeight + (!this.selectingOptions || this.optionsIndex != 2 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * this.optionsScale, this.selectingOptions && this.optionsIndex == 2, this.Name.Length <= 0 || !this.Focused);
      vector2.X += this.backspaceWidth + this.widestLetter;
      this.DrawOptionText(this.accept, vector2 + new Vector2(0.0f, this.lineHeight + (!this.selectingOptions || this.optionsIndex != 3 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * this.optionsScale * 1.25f, this.selectingOptions && this.optionsIndex == 3, this.Name.Length < 1 || !this.Focused);
      if (!this.Japanese)
        return;
      float scale1 = 1f;
      string text = Dialog.Clean(this.hiragana ? "NAME_LETTERS_SWAP_KATAKANA" : "NAME_LETTERS_SWAP_HIRAGANA");
      MTexture mtexture = Celeste.Input.GuiButton(Celeste.Input.MenuJournal);
      ActiveFont.Measure(text);
      float num3 = (float) mtexture.Width * scale1;
      Vector2 position = new Vector2(70f, (float) (1144.0 - 154.0 * (double) this.ease));
      mtexture.DrawJustified(position, new Vector2(0.0f, 0.5f), Color.White, scale1, 0.0f);
      ActiveFont.DrawOutline(text, position + new Vector2(16f + num3, 0.0f), new Vector2(0.0f, 0.5f), Vector2.One * scale1, Color.White, 2f, Color.Black);
    }

    private void DrawOptionText(
      string text,
      Vector2 at,
      Vector2 justify,
      Vector2 scale,
      bool selected,
      bool disabled = false)
    {
      int num = !selected ? 0 : ((double) this.pressedTimer > 0.0 ? 1 : 0);
      Color color = disabled ? this.disableColor : this.GetTextColor(selected);
      Color edgeColor = disabled ? Color.Lerp(this.disableColor, Color.Black, 0.7f) : Color.Gray;
      if (num != 0)
        ActiveFont.Draw(text, at + Vector2.UnitY, justify, scale, color);
      else
        ActiveFont.DrawEdgeOutline(text, at, justify, scale, color, 4f, edgeColor);
    }

    private Color GetTextColor(bool selected)
    {
      if (!selected)
        return this.unselectColor;
      return Settings.Instance.DisableFlashes || Calc.BetweenInterval(this.timer, 0.1f) ? this.selectColorA : this.selectColorB;
    }
  }
}
