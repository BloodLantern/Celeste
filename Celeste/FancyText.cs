// Decompiled with JetBrains decompiler
// Type: Celeste.FancyText
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace Celeste
{
    public class FancyText
    {
        public static Color DefaultColor = Color.LightGray;
        public const float CharacterDelay = 0.01f;
        public const float PeriodDelay = 0.3f;
        public const float CommaDelay = 0.15f;
        public const float ShakeDistance = 2f;
        private readonly Language language;
        private readonly string text;
        private readonly FancyText.Text group = new();
        private readonly int maxLineWidth;
        private readonly int linesPerPage;
        private readonly PixelFont font;
        private readonly PixelFontSize size;
        private Color defaultColor;
        private readonly float startFade;
        private int currentLine;
        private int currentPage;
        private float currentPosition;
        private Color currentColor;
        private float currentScale = 1f;
        private float currentDelay = 0.01f;
        private bool currentShake;
        private bool currentWave;
        private bool currentImpact;
        private bool currentMessedUp;
        private int currentCharIndex;

        public static FancyText.Text Parse(
            string text,
            int maxLineWidth,
            int linesPerPage,
            float startFade = 1f,
            Color? defaultColor = null,
            Language language = null)
        {
            return new FancyText(text, maxLineWidth, linesPerPage, startFade, defaultColor.HasValue ? defaultColor.Value : FancyText.DefaultColor, language).Parse();
        }

        private FancyText(
            string text,
            int maxLineWidth,
            int linesPerPage,
            float startFade,
            Color defaultColor,
            Language language)
        {
            this.text = text;
            this.maxLineWidth = maxLineWidth;
            this.linesPerPage = linesPerPage < 0 ? int.MaxValue : linesPerPage;
            this.startFade = startFade;
            this.defaultColor = currentColor = defaultColor;
            language ??= Dialog.Language;
            this.language = language;
            group.Nodes = new List<FancyText.Node>();
            group.Font = font = Fonts.Get(language.FontFace);
            group.BaseSize = language.FontFaceSize;
            size = font.Get(group.BaseSize);
        }

        private FancyText.Text Parse()
        {
            string[] strArray1 = Regex.Split(text, language.SplitRegex);
            string[] strArray2 = new string[strArray1.Length];
            int num1 = 0;
            for (int index = 0; index < strArray1.Length; ++index)
            {
                if (!string.IsNullOrEmpty(strArray1[index]))
                {
                    strArray2[num1++] = strArray1[index];
                }
            }
            Stack<Color> colorStack = new();
            FancyText.Portrait[] portraitArray = new FancyText.Portrait[2];
            for (int index1 = 0; index1 < num1; ++index1)
            {
                if (strArray2[index1] == "{")
                {
                    int num2 = index1 + 1;
                    string[] strArray3 = strArray2;
                    int index2 = num2;
                    index1 = index2 + 1;
                    string s = strArray3[index2];
                    List<string> stringList = new();
                    for (; index1 < strArray2.Length && strArray2[index1] != "}"; ++index1)
                    {
                        if (!string.IsNullOrWhiteSpace(strArray2[index1]))
                        {
                            stringList.Add(strArray2[index1]);
                        }
                    }
                    if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float result1))
                    {
                        group.Nodes.Add(new FancyText.Wait()
                        {
                            Duration = result1
                        });
                    }
                    else if (s[0] == '#')
                    {
                        string hex = "";
                        if (s.Length > 1)
                        {
                            hex = s.Substring(1);
                        }
                        else if (stringList.Count > 0)
                        {
                            hex = stringList[0];
                        }

                        if (string.IsNullOrEmpty(hex))
                        {
                            currentColor = colorStack.Count <= 0 ? defaultColor : colorStack.Pop();
                        }
                        else
                        {
                            colorStack.Push(currentColor);
                            currentColor = !(hex == "red") ? (!(hex == "green") ? (!(hex == "blue") ? Calc.HexToColor(hex) : Color.Blue) : Color.Green) : Color.Red;
                        }
                    }
                    else if (s == "break")
                    {
                        CalcLineWidth();
                        ++currentPage;
                        ++group.Pages;
                        currentLine = 0;
                        currentPosition = 0.0f;
                        group.Nodes.Add(new FancyText.NewPage());
                    }
                    else if (s == "n")
                    {
                        AddNewLine();
                    }
                    else if (s == ">>")
                    {
                        currentDelay = stringList.Count <= 0 || !float.TryParse(stringList[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float result2) ? 0.01f : 0.01f / result2;
                    }
                    else if (s.Equals("/>>"))
                    {
                        currentDelay = 0.01f;
                    }
                    else if (s.Equals("anchor"))
                    {
                        if (Enum.TryParse<FancyText.Anchors>(stringList[0], true, out Anchors result3))
                        {
                            group.Nodes.Add(new FancyText.Anchor()
                            {
                                Position = result3
                            });
                        }
                    }
                    else if (s.Equals("portrait") || s.Equals("left") || s.Equals("right"))
                    {
                        if (s.Equals("portrait") && stringList.Count > 0 && stringList[0].Equals("none"))
                        {
                            group.Nodes.Add(new FancyText.Portrait());
                        }
                        else
                        {
                            FancyText.Portrait portrait;
                            if (s.Equals("left"))
                            {
                                portrait = portraitArray[0];
                            }
                            else if (s.Equals("right"))
                            {
                                portrait = portraitArray[1];
                            }
                            else
                            {
                                portrait = new FancyText.Portrait();
                                foreach (string str in stringList)
                                {
                                    if (str.Equals("upsidedown"))
                                    {
                                        portrait.UpsideDown = true;
                                    }
                                    else if (str.Equals("flip"))
                                    {
                                        portrait.Flipped = true;
                                    }
                                    else if (str.Equals("left"))
                                    {
                                        portrait.Side = -1;
                                    }
                                    else if (str.Equals("right"))
                                    {
                                        portrait.Side = 1;
                                    }
                                    else if (str.Equals("pop"))
                                    {
                                        portrait.Pop = true;
                                    }
                                    else if (portrait.Sprite == null)
                                    {
                                        portrait.Sprite = str;
                                    }
                                    else
                                    {
                                        portrait.Animation = str;
                                    }
                                }
                            }
                            if (GFX.PortraitsSpriteBank.Has(portrait.SpriteId))
                            {
                                List<SpriteDataSource> sources = GFX.PortraitsSpriteBank.SpriteData[portrait.SpriteId].Sources;
                                for (int index3 = sources.Count - 1; index3 >= 0; --index3)
                                {
                                    XmlElement xml1 = sources[index3].XML;
                                    if (xml1 != null)
                                    {
                                        portrait.SfxEvent ??= "event:/char/dialogue/" + xml1.Attr("sfx", "");
                                        if (xml1.HasAttr("glitchy"))
                                        {
                                            portrait.Glitchy = xml1.AttrBool("glitchy", false);
                                        }

                                        if (xml1.HasChild("sfxs") && portrait.SfxExpression == 1)
                                        {
                                            foreach (object obj in (XmlNode)xml1["sfxs"])
                                            {
                                                if (obj is XmlElement xml2 && xml2.Name.Equals(portrait.Animation, StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    portrait.SfxExpression = xml2.AttrInt("index");
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            group.Nodes.Add(portrait);
                            portraitArray[portrait.Side > 0 ? 1 : 0] = portrait;
                        }
                    }
                    else if (s.Equals("trigger") || s.Equals("silent_trigger"))
                    {
                        string str = "";
                        for (int index4 = 1; index4 < stringList.Count; ++index4)
                        {
                            str = str + stringList[index4] + " ";
                        }

                        if (int.TryParse(stringList[0], out int result4) && result4 >= 0)
                        {
                            group.Nodes.Add(new FancyText.Trigger()
                            {
                                Index = result4,
                                Silent = s.StartsWith("silent"),
                                Label = str
                            });
                        }
                    }
                    else if (s.Equals("*"))
                    {
                        currentShake = true;
                    }
                    else if (s.Equals("/*"))
                    {
                        currentShake = false;
                    }
                    else if (s.Equals("~"))
                    {
                        currentWave = true;
                    }
                    else if (s.Equals("/~"))
                    {
                        currentWave = false;
                    }
                    else if (s.Equals("!"))
                    {
                        currentImpact = true;
                    }
                    else if (s.Equals("/!"))
                    {
                        currentImpact = false;
                    }
                    else if (s.Equals("%"))
                    {
                        currentMessedUp = true;
                    }
                    else if (s.Equals("/%"))
                    {
                        currentMessedUp = false;
                    }
                    else if (s.Equals("big"))
                    {
                        currentScale = 1.5f;
                    }
                    else if (s.Equals("/big"))
                    {
                        currentScale = 1f;
                    }
                    else if (s.Equals("s"))
                    {
                        int result5 = 1;
                        if (stringList.Count > 0)
                        {
                            _ = int.TryParse(stringList[0], out result5);
                        }

                        currentPosition += 5 * result5;
                    }
                    else if (s.Equals("savedata"))
                    {
                        if (SaveData.Instance == null)
                        {
                            if (stringList[0].Equals("name", StringComparison.OrdinalIgnoreCase))
                            {
                                AddWord("Madeline");
                            }
                            else
                            {
                                AddWord("[SD:" + stringList[0] + "]");
                            }
                        }
                        else if (stringList[0].Equals("name", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!language.CanDisplay(SaveData.Instance.Name))
                            {
                                AddWord(Dialog.Clean("FILE_DEFAULT", language));
                            }
                            else
                            {
                                AddWord(SaveData.Instance.Name);
                            }
                        }
                        else
                        {
                            AddWord(typeof(SaveData).GetField(stringList[0]).GetValue(SaveData.Instance).ToString());
                        }
                    }
                }
                else
                {
                    AddWord(strArray2[index1]);
                }
            }
            CalcLineWidth();
            return group;
        }

        private void CalcLineWidth()
        {
            FancyText.Char @char = null;
            int index;
            for (index = group.Nodes.Count - 1; index >= 0 && @char == null; --index)
            {
                if (group.Nodes[index] is FancyText.Char)
                {
                    @char = group.Nodes[index] as FancyText.Char;
                }
                else if (group.Nodes[index] is FancyText.NewLine or FancyText.NewPage)
                {
                    return;
                }
            }
            if (@char == null)
            {
                return;
            }

            float num = @char.Position + (size.Get(@char.Character).XAdvance * @char.Scale);
            @char.LineWidth = num;
            for (; index >= 0 && group.Nodes[index] is not FancyText.NewLine && group.Nodes[index] is not FancyText.NewPage; --index)
            {
                if (group.Nodes[index] is FancyText.Char)
                {
                    (group.Nodes[index] as FancyText.Char).LineWidth = num;
                }
            }
        }

        private void AddNewLine()
        {
            CalcLineWidth();
            ++currentLine;
            currentPosition = 0.0f;
            ++group.Lines;
            if (currentLine > linesPerPage)
            {
                ++group.Pages;
                ++currentPage;
                currentLine = 0;
                group.Nodes.Add(new FancyText.NewPage());
            }
            else
            {
                group.Nodes.Add(new FancyText.NewLine());
            }
        }

        private void AddWord(string word)
        {
            if (currentPosition + (double)(size.Measure(word).X * currentScale) > maxLineWidth)
            {
                AddNewLine();
            }

            for (int index = 0; index < word.Length; ++index)
            {
                if ((currentPosition != 0.0 || word[index] != ' ') && word[index] != '\\')
                {
                    PixelFontCharacter pixelFontCharacter = size.Get(word[index]);
                    if (pixelFontCharacter != null)
                    {
                        float num1 = 0.0f;
                        if (index == word.Length - 1 && (index == 0 || word[index - 1] != '\\'))
                        {
                            if (Contains(language.CommaCharacters, word[index]))
                            {
                                num1 = 0.15f;
                            }
                            else if (Contains(language.PeriodCharacters, word[index]))
                            {
                                num1 = 0.3f;
                            }
                        }
                        group.Nodes.Add(new FancyText.Char()
                        {
                            Index = currentCharIndex++,
                            Character = word[index],
                            Position = currentPosition,
                            Line = currentLine,
                            Page = currentPage,
                            Delay = currentImpact ? 0.00349999988f : currentDelay + num1,
                            Color = currentColor,
                            Scale = currentScale,
                            Rotation = currentMessedUp ? Calc.Random.Choose<int>(-1, 1) * Calc.Random.Choose<float>(0.17453292f, 0.34906584f) : 0.0f,
                            YOffset = currentMessedUp ? Calc.Random.Choose<int>(-3, -6, 3, 6) : 0.0f,
                            Fade = startFade,
                            Shake = currentShake,
                            Impact = currentImpact,
                            Wave = currentWave,
                            IsPunctuation = Contains(language.CommaCharacters, word[index]) || Contains(language.PeriodCharacters, word[index])
                        });
                        currentPosition += pixelFontCharacter.XAdvance * currentScale;
                        if (index < word.Length - 1 && pixelFontCharacter.Kerning.TryGetValue(word[index], out int num2))
                        {
                            currentPosition += num2 * currentScale;
                        }
                    }
                }
            }
        }

        private bool Contains(string str, char character)
        {
            for (int index = 0; index < str.Length; ++index)
            {
                if (str[index] == character)
                {
                    return true;
                }
            }
            return false;
        }

        public class Node
        {
        }

        public class Char : FancyText.Node
        {
            public int Index;
            public int Character;
            public float Position;
            public int Line;
            public int Page;
            public float Delay;
            public float LineWidth;
            public Color Color;
            public float Scale;
            public float Rotation;
            public float YOffset;
            public float Fade;
            public bool Shake;
            public bool Wave;
            public bool Impact;
            public bool IsPunctuation;

            public void Draw(
                PixelFont font,
                float baseSize,
                Vector2 position,
                Vector2 scale,
                float alpha)
            {
                float num = (Impact ? 2f - Fade : 1f) * Scale;
                Vector2 zero = Vector2.Zero;
                Vector2 vector2_1 = scale * num;
                PixelFontSize pixelFontSize = font.Get(baseSize * Math.Max(vector2_1.X, vector2_1.Y));
                PixelFontCharacter pixelFontCharacter = pixelFontSize.Get(Character);
                Vector2 scale1 = vector2_1 * (baseSize / pixelFontSize.Size);
                position.X += Position * scale.X;
                Vector2 vector2_2 = zero + (Shake ? new Vector2(Calc.Random.Next(3) - 1, Calc.Random.Next(3) - 1) * 2f : Vector2.Zero) + (Wave ? new Vector2(0.0f, (float)Math.Sin((Index * 0.25) + (Engine.Scene.RawTimeActive * 8.0)) * 4f) : Vector2.Zero);
                vector2_2.X += pixelFontCharacter.XOffset;
                vector2_2.Y += pixelFontCharacter.YOffset + (float)((-8.0 * (1.0 - Fade)) + (YOffset * (double)Fade));
                pixelFontCharacter.Texture.Draw(position + (vector2_2 * scale1), Vector2.Zero, Color * Fade * alpha, scale1, Rotation);
            }
        }

        public class Portrait : FancyText.Node
        {
            public int Side;
            public string Sprite;
            public string Animation;
            public bool UpsideDown;
            public bool Flipped;
            public bool Pop;
            public bool Glitchy;
            public string SfxEvent;
            public int SfxExpression = 1;

            public string SpriteId => "portrait_" + Sprite;

            public string BeginAnimation => "begin_" + Animation;

            public string IdleAnimation => "idle_" + Animation;

            public string TalkAnimation => "talk_" + Animation;
        }

        public class Wait : FancyText.Node
        {
            public float Duration;
        }

        public class Trigger : FancyText.Node
        {
            public int Index;
            public bool Silent;
            public string Label;
        }

        public class NewLine : FancyText.Node
        {
        }

        public class NewPage : FancyText.Node
        {
        }

        public enum Anchors
        {
            Top,
            Middle,
            Bottom,
        }

        public class Anchor : FancyText.Node
        {
            public FancyText.Anchors Position;
        }

        public class Text
        {
            public List<FancyText.Node> Nodes;
            public int Lines;
            public int Pages;
            public PixelFont Font;
            public float BaseSize;

            public int Count => Nodes.Count;

            public FancyText.Node this[int index] => Nodes[index];

            public int GetCharactersOnPage(int start)
            {
                int charactersOnPage = 0;
                for (int index = start; index < Count; ++index)
                {
                    if (Nodes[index] is FancyText.Char)
                    {
                        ++charactersOnPage;
                    }
                    else if (Nodes[index] is FancyText.NewPage)
                    {
                        break;
                    }
                }
                return charactersOnPage;
            }

            public int GetNextPageStart(int start)
            {
                for (int index = start; index < Count; ++index)
                {
                    if (Nodes[index] is FancyText.NewPage)
                    {
                        return index + 1;
                    }
                }
                return Nodes.Count;
            }

            public float WidestLine()
            {
                int val1 = 0;
                for (int index = 0; index < Nodes.Count; ++index)
                {
                    if (Nodes[index] is FancyText.Char)
                    {
                        val1 = Math.Max(val1, (int)(Nodes[index] as FancyText.Char).LineWidth);
                    }
                }
                return val1;
            }

            public void Draw(
                Vector2 position,
                Vector2 justify,
                Vector2 scale,
                float alpha,
                int start = 0,
                int end = 2147483647)
            {
                int num1 = Math.Min(Nodes.Count, end);
                int num2 = 0;
                float val1_1 = 0.0f;
                float num3 = 0.0f;
                PixelFontSize pixelFontSize = Font.Get(BaseSize);
                for (int index = start; index < num1; ++index)
                {
                    if (Nodes[index] is FancyText.NewLine)
                    {
                        if ((double)val1_1 == 0.0)
                        {
                            val1_1 = 1f;
                        }

                        num3 += val1_1;
                        val1_1 = 0.0f;
                    }
                    else if (Nodes[index] is FancyText.Char)
                    {
                        num2 = Math.Max(num2, (int)(Nodes[index] as FancyText.Char).LineWidth);
                        val1_1 = Math.Max(val1_1, (Nodes[index] as FancyText.Char).Scale);
                    }
                    else if (Nodes[index] is FancyText.NewPage)
                    {
                        break;
                    }
                }
                float num4 = num3 + val1_1;
                position -= justify * new Vector2(num2, num4 * pixelFontSize.LineHeight) * scale;
                float val1_2 = 0.0f;
                for (int index = start; index < num1 && Nodes[index] is not FancyText.NewPage; ++index)
                {
                    if (Nodes[index] is FancyText.NewLine)
                    {
                        if ((double)val1_2 == 0.0)
                        {
                            val1_2 = 1f;
                        }

                        position.Y += pixelFontSize.LineHeight * val1_2 * scale.Y;
                        val1_2 = 0.0f;
                    }
                    if (Nodes[index] is FancyText.Char)
                    {
                        FancyText.Char node = Nodes[index] as FancyText.Char;
                        node.Draw(Font, BaseSize, position, scale, alpha);
                        val1_2 = Math.Max(val1_2, node.Scale);
                    }
                }
            }

            public void DrawJustifyPerLine(
                Vector2 position,
                Vector2 justify,
                Vector2 scale,
                float alpha,
                int start = 0,
                int end = 2147483647)
            {
                int num1 = Math.Min(Nodes.Count, end);
                float val1_1 = 0.0f;
                float num2 = 0.0f;
                PixelFontSize pixelFontSize = Font.Get(BaseSize);
                for (int index = start; index < num1; ++index)
                {
                    if (Nodes[index] is FancyText.NewLine)
                    {
                        if ((double)val1_1 == 0.0)
                        {
                            val1_1 = 1f;
                        }

                        num2 += val1_1;
                        val1_1 = 0.0f;
                    }
                    else if (Nodes[index] is FancyText.Char)
                    {
                        val1_1 = Math.Max(val1_1, (Nodes[index] as FancyText.Char).Scale);
                    }
                    else if (Nodes[index] is FancyText.NewPage)
                    {
                        break;
                    }
                }
                float num3 = num2 + val1_1;
                float val1_2 = 0.0f;
                for (int index = start; index < num1 && Nodes[index] is not FancyText.NewPage; ++index)
                {
                    if (Nodes[index] is FancyText.NewLine)
                    {
                        if ((double)val1_2 == 0.0)
                        {
                            val1_2 = 1f;
                        }

                        position.Y += val1_2 * pixelFontSize.LineHeight * scale.Y;
                        val1_2 = 0.0f;
                    }
                    if (Nodes[index] is FancyText.Char)
                    {
                        FancyText.Char node = Nodes[index] as FancyText.Char;
                        Vector2 vector2 = -justify * new Vector2(node.LineWidth, num3 * pixelFontSize.LineHeight) * scale;
                        node.Draw(Font, BaseSize, position + vector2, scale, alpha);
                        val1_2 = Math.Max(val1_2, node.Scale);
                    }
                }
            }
        }
    }
}
