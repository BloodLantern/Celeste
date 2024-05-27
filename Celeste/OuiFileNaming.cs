using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections;
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
            get => FileSlot.Name;
            set => FileSlot.Name = value;
        }

        public int MaxNameLength => !Japanese ? 12 : 8;

        public bool Japanese => Settings.Instance.Language == "japanese";

        private Vector2 boxtopleft => Position + new Vector2((float) ((1920.0 - boxWidth) / 2.0), (float) (360.0 + (680.0 - boxHeight) / 2.0));

        public OuiFileNaming()
        {
            wiggler = Wiggler.Create(0.25f, 4f);
            Position = new Vector2(0.0f, 1080f);
            Visible = false;
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
            ouiFileNaming.beginWidth = (float) (ActiveFont.Measure(ouiFileNaming.accept).X * (double) ouiFileNaming.optionsScale * 1.25);
            ouiFileNaming.optionsWidth = (float) (ouiFileNaming.cancelWidth + (double) ouiFileNaming.spaceWidth + ouiFileNaming.backspaceWidth + ouiFileNaming.beginWidth + ouiFileNaming.widestLetter * 3.0);
            ouiFileNaming.Visible = true;
            Vector2 posFrom = ouiFileNaming.Position;
            Vector2 posTo = Vector2.Zero;
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime * 3f)
            {
                ouiFileNaming.ease = Ease.CubeIn(t);
                ouiFileNaming.Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
                yield return null;
            }
            ouiFileNaming.ease = 1f;
            posFrom = new Vector2();
            posTo = new Vector2();
            yield return 0.05f;
            ouiFileNaming.Focused = true;
            yield return 0.05f;
            ouiFileNaming.wiggler.Start();
        }

        private void ReloadLetters(string chars)
        {
            letters = chars.Split('\n');
            widestLetter = 0.0f;
            foreach (char text in chars)
            {
                float x = ActiveFont.Measure(text).X;
                if (x > (double) widestLetter)
                    widestLetter = x;
            }
            if (Japanese)
                widestLetter *= 1.5f;
            widestLineCount = 0;
            foreach (string letter in letters)
            {
                if (letter.Length > widestLineCount)
                    widestLineCount = letter.Length;
            }
            widestLine = widestLineCount * widestLetter;
            lineHeight = ActiveFont.LineHeight;
            lineSpacing = ActiveFont.LineHeight * 0.1f;
            boxPadding = widestLetter;
            boxWidth = Math.Max(widestLine, optionsWidth) + boxPadding * 2f;
            boxHeight = (float) ((letters.Length + 1) * (double) lineHeight + letters.Length * (double) lineSpacing + boxPadding * 3.0);
        }

        public override IEnumerator Leave(Oui next)
        {
            OuiFileNaming ouiFileNaming = this;
            ouiFileNaming.Overworld.ShowInputUI = true;
            ouiFileNaming.Focused = false;
            Vector2 posFrom = ouiFileNaming.Position;
            Vector2 posTo = new Vector2(0.0f, 1080f);
            for (float t = 0.0f; t < 1.0; t += Engine.DeltaTime * 2f)
            {
                ouiFileNaming.ease = 1f - Ease.CubeIn(t);
                ouiFileNaming.Position = posFrom + (posTo - posFrom) * Ease.CubeInOut(t);
                yield return null;
            }
            ouiFileNaming.Visible = false;
        }

        public override void Update()
        {
            base.Update();
            if (Selected && Focused)
            {
                if (!string.IsNullOrWhiteSpace(Name) && MInput.Keyboard.Check(Keys.LeftControl) && MInput.Keyboard.Pressed(Keys.S))
                    ResetDefaultName();
                if (Input.MenuJournal.Pressed && Japanese)
                    SwapType();
                if (Input.MenuRight.Pressed && (optionsIndex < 3 || !selectingOptions) && (Name.Length > 0 || !selectingOptions))
                {
                    if (selectingOptions)
                    {
                        optionsIndex = Math.Min(optionsIndex + 1, 3);
                    }
                    else
                    {
                        do
                        {
                            index = (index + 1) % letters[line].Length;
                        }
                        while (letters[line][index] == ' ');
                    }
                    wiggler.Start();
                    Audio.Play("event:/ui/main/rename_entry_rollover");
                }
                else if (Input.MenuLeft.Pressed && (optionsIndex > 0 || !selectingOptions))
                {
                    if (selectingOptions)
                    {
                        optionsIndex = Math.Max(optionsIndex - 1, 0);
                    }
                    else
                    {
                        do
                        {
                            index = (index + letters[line].Length - 1) % letters[line].Length;
                        }
                        while (letters[line][index] == ' ');
                    }
                    wiggler.Start();
                    Audio.Play("event:/ui/main/rename_entry_rollover");
                }
                else if (Input.MenuDown.Pressed && !selectingOptions)
                {
                    for (int index = line + 1; index < letters.Length; ++index)
                    {
                        if (this.index < letters[index].Length && letters[index][this.index] != ' ')
                        {
                            line = index;
                            goto label_22;
                        }
                    }
                    selectingOptions = true;
label_22:
                    if (selectingOptions)
                    {
                        float num1 = index * widestLetter;
                        float num2 = boxWidth - boxPadding * 2f;
                        optionsIndex = Name.Length == 0 || num1 < cancelWidth + (num2 - (double) cancelWidth - beginWidth - backspaceWidth - spaceWidth - widestLetter * 3.0) / 2.0 ? 0 : (num1 >= num2 - (double) beginWidth - backspaceWidth - widestLetter * 2.0 ? (num1 >= num2 - (double) beginWidth - widestLetter ? 3 : 2) : 1);
                    }
                    wiggler.Start();
                    Audio.Play("event:/ui/main/rename_entry_rollover");
                }
                else if ((Input.MenuUp.Pressed || selectingOptions && Name.Length <= 0 && optionsIndex > 0) && (line > 0 || selectingOptions))
                {
                    if (selectingOptions)
                    {
                        line = letters.Length;
                        selectingOptions = false;
                        float num = boxWidth - boxPadding * 2f;
                        if (optionsIndex == 0)
                            index = (int) (cancelWidth / 2.0 / widestLetter);
                        else if (optionsIndex == 1)
                            index = (int) ((num - (double) beginWidth - backspaceWidth - spaceWidth / 2.0 - widestLetter * 2.0) / widestLetter);
                        else if (optionsIndex == 2)
                            index = (int) ((num - (double) beginWidth - backspaceWidth / 2.0 - widestLetter) / widestLetter);
                        else if (optionsIndex == 3)
                            index = (int) ((num - beginWidth / 2.0) / widestLetter);
                    }
                    --line;
                    while (line > 0 && (index >= letters[line].Length || letters[line][index] == ' '))
                        --line;
                    while (index >= letters[line].Length || letters[line][index] == ' ')
                        --index;
                    wiggler.Start();
                    Audio.Play("event:/ui/main/rename_entry_rollover");
                }
                else if (Input.MenuConfirm.Pressed)
                {
                    if (selectingOptions)
                    {
                        if (optionsIndex == 0)
                            Cancel();
                        else if (optionsIndex == 1 && Name.Length > 0)
                            Space();
                        else if (optionsIndex == 2)
                            Backspace();
                        else if (optionsIndex == 3)
                            Finish();
                    }
                    else if (Japanese && letters[line][index] == '゛' && Name.Length > 0 && OuiFileNaming.dakuten_able.Contains(Name.Last()))
                    {
                        int num = Name[Name.Length - 1] + 1;
                        Name = Name.Substring(0, Name.Length - 1);
                        Name += ((char) num).ToString();
                        wiggler.Start();
                        Audio.Play("event:/ui/main/rename_entry_char");
                    }
                    else if (Japanese && letters[line][index] == '゜' && Name.Length > 0 && (OuiFileNaming.handakuten_able.Contains(Name.Last()) || OuiFileNaming.handakuten_able.Contains(Name.Last() + 1)))
                    {
                        int num3 = Name[Name.Length - 1];
                        int num4 = !OuiFileNaming.handakuten_able.Contains(num3) ? num3 + 2 : num3 + 1;
                        Name = Name.Substring(0, Name.Length - 1);
                        Name += ((char) num4).ToString();
                        wiggler.Start();
                        Audio.Play("event:/ui/main/rename_entry_char");
                    }
                    else if (Name.Length < MaxNameLength)
                    {
                        Name += letters[line][index].ToString();
                        wiggler.Start();
                        Audio.Play("event:/ui/main/rename_entry_char");
                    }
                    else
                        Audio.Play("event:/ui/main/button_invalid");
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (Name.Length > 0)
                        Backspace();
                    else
                        Cancel();
                }
                else if (Input.Pause.Pressed)
                    Finish();
            }
            pressedTimer -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            wiggler.Update();
        }

        private void ResetDefaultName()
        {
            if (StartingName == Settings.Instance.DefaultFileName || StartingName == Dialog.Clean("FILE_DEFAULT"))
                StartingName = Name;
            Settings.Instance.DefaultFileName = Name;
            Audio.Play("event:/new_content/ui/rename_entry_accept_locked");
        }

        private void Space()
        {
            if (Name.Length < MaxNameLength && Name.Length > 0)
            {
                Name += " ";
                wiggler.Start();
                Audio.Play("event:/ui/main/rename_entry_char");
            }
            else
                Audio.Play("event:/ui/main/button_invalid");
        }

        private void Backspace()
        {
            if (Name.Length > 0)
            {
                Name = Name.Substring(0, Name.Length - 1);
                Audio.Play("event:/ui/main/rename_entry_backspace");
            }
            else
                Audio.Play("event:/ui/main/button_invalid");
        }

        private void Finish()
        {
            if (Name.Length >= 1)
            {
                if (MInput.GamePads.Length != 0 && MInput.GamePads[0] != null && (MInput.GamePads[0].Check(Buttons.LeftTrigger) || MInput.GamePads[0].Check(Buttons.LeftShoulder)) && (MInput.GamePads[0].Check(Buttons.RightTrigger) || MInput.GamePads[0].Check(Buttons.RightShoulder)))
                    ResetDefaultName();
                Focused = false;
                Overworld.Goto<OuiFileSelect>();
                Audio.Play("event:/ui/main/rename_entry_accept");
            }
            else
                Audio.Play("event:/ui/main/button_invalid");
        }

        private void SwapType()
        {
            hiragana = !hiragana;
            if (hiragana)
                ReloadLetters(Dialog.Clean("name_letters"));
            else
                ReloadLetters(Dialog.Clean("name_letters_katakana"));
        }

        private void Cancel()
        {
            FileSlot.Name = StartingName;
            Focused = false;
            Overworld.Goto<OuiFileSelect>();
            Audio.Play("event:/ui/main/button_back");
        }

        public override void Render()
        {
            Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * 0.8f * ease);
            Vector2 vector2 = boxtopleft + new Vector2(boxPadding, boxPadding);
            int num1 = 0;
            foreach (string letter in letters)
            {
                for (int index = 0; index < letter.Length; ++index)
                {
                    bool selected = num1 == line && index == this.index && !selectingOptions;
                    Vector2 scale = Vector2.One * (selected ? 1.2f : 1f);
                    Vector2 at = vector2 + new Vector2(widestLetter, lineHeight) / 2f;
                    if (selected)
                        at += new Vector2(0.0f, wiggler.Value) * 8f;
                    DrawOptionText(letter[index].ToString(), at, new Vector2(0.5f, 0.5f), scale, selected);
                    vector2.X += widestLetter;
                }
                vector2.X = boxtopleft.X + boxPadding;
                vector2.Y += lineHeight + lineSpacing;
                ++num1;
            }
            float num2 = wiggler.Value * 8f;
            vector2.Y = boxtopleft.Y + boxHeight - lineHeight - boxPadding;
            Draw.Rect(vector2.X, vector2.Y - boxPadding * 0.5f, boxWidth - boxPadding * 2f, 4f, Color.White);
            DrawOptionText(cancel, vector2 + new Vector2(0.0f, lineHeight + (!selectingOptions || optionsIndex != 0 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 0);
            vector2.X = boxtopleft.X + boxWidth - backspaceWidth - widestLetter - spaceWidth - widestLetter - beginWidth - boxPadding;
            DrawOptionText(space, vector2 + new Vector2(0.0f, lineHeight + (!selectingOptions || optionsIndex != 1 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 1, Name.Length == 0 || !Focused);
            vector2.X += spaceWidth + widestLetter;
            DrawOptionText(backspace, vector2 + new Vector2(0.0f, lineHeight + (!selectingOptions || optionsIndex != 2 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * optionsScale, selectingOptions && optionsIndex == 2, Name.Length <= 0 || !Focused);
            vector2.X += backspaceWidth + widestLetter;
            DrawOptionText(accept, vector2 + new Vector2(0.0f, lineHeight + (!selectingOptions || optionsIndex != 3 ? 0.0f : num2)), new Vector2(0.0f, 1f), Vector2.One * optionsScale * 1.25f, selectingOptions && optionsIndex == 3, Name.Length < 1 || !Focused);
            if (!Japanese)
                return;
            float scale1 = 1f;
            string text = Dialog.Clean(hiragana ? "NAME_LETTERS_SWAP_KATAKANA" : "NAME_LETTERS_SWAP_HIRAGANA");
            MTexture mtexture = Input.GuiButton(Input.MenuJournal);
            ActiveFont.Measure(text);
            float num3 = mtexture.Width * scale1;
            Vector2 position = new Vector2(70f, (float) (1144.0 - 154.0 * ease));
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
            int num = !selected ? 0 : (pressedTimer > 0.0 ? 1 : 0);
            Color color = disabled ? disableColor : GetTextColor(selected);
            Color edgeColor = disabled ? Color.Lerp(disableColor, Color.Black, 0.7f) : Color.Gray;
            if (num != 0)
                ActiveFont.Draw(text, at + Vector2.UnitY, justify, scale, color);
            else
                ActiveFont.DrawEdgeOutline(text, at, justify, scale, color, 4f, edgeColor);
        }

        private Color GetTextColor(bool selected)
        {
            if (!selected)
                return unselectColor;
            return Settings.Instance.DisableFlashes || Calc.BetweenInterval(timer, 0.1f) ? selectColorA : selectColorB;
        }
    }
}
