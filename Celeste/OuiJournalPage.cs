// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalPage
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public abstract class OuiJournalPage
    {
        public const int PageWidth = 1610;
        public const int PageHeight = 1000;
        public readonly Vector2 TextJustify = new(0.5f, 0.5f);
        public const float TextScale = 0.5f;
        public readonly Color TextColor = Color.Black * 0.6f;
        public int PageIndex;
        public string PageTexture;
        public OuiJournal Journal;

        public OuiJournalPage(OuiJournal journal)
        {
            Journal = journal;
        }

        public virtual void Redraw(VirtualRenderTarget buffer)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Transparent);
        }

        public virtual void Update()
        {
        }

        public class Table
        {
            private const float headerHeight = 80f;
            private const float headerBottomMargin = 20f;
            private const float rowHeight = 60f;
            private readonly List<OuiJournalPage.Row> rows = new();

            public int Rows => rows.Count;

            public OuiJournalPage.Row Header => rows.Count <= 0 ? null : rows[0];

            public OuiJournalPage.Table AddColumn(OuiJournalPage.Cell label)
            {
                if (rows.Count == 0)
                {
                    _ = AddRow();
                }

                _ = rows[0].Add(label);
                return this;
            }

            public OuiJournalPage.Row AddRow()
            {
                OuiJournalPage.Row row = new();
                rows.Add(row);
                return row;
            }

            public float Height()
            {
                return (float)(100.0 + (60.0 * (rows.Count - 1)));
            }

            public void Render(Vector2 position)
            {
                if (Header == null)
                {
                    return;
                }

                float num1 = 0.0f;
                float num2 = 0.0f;
                for (int index = 0; index < Header.Count; ++index)
                {
                    num2 += Header[index].Width() + 20f;
                }

                for (int index1 = 0; index1 < Header.Count; ++index1)
                {
                    float columnWidth = Header[index1].Width();
                    Header[index1].Render(position + new Vector2(num1 + (columnWidth * 0.5f), 40f), columnWidth);
                    int num3 = 1;
                    float y = 130f;
                    for (int index2 = 1; index2 < rows.Count; ++index2)
                    {
                        Vector2 center = position + new Vector2(num1 + (columnWidth * 0.5f), y);
                        if (rows[index2].Count > 0)
                        {
                            if (num3 % 2 == 0)
                            {
                                Draw.Rect(center.X - (columnWidth * 0.5f), center.Y - 27f, columnWidth + 20f, 54f, Color.Black * 0.08f);
                            }

                            if (index1 < rows[index2].Count && rows[index2][index1] != null)
                            {
                                OuiJournalPage.Cell cell = rows[index2][index1];
                                if (cell.SpreadOverColumns > 1)
                                {
                                    for (int index3 = index1 + 1; index3 < index1 + cell.SpreadOverColumns; ++index3)
                                    {
                                        center.X += Header[index3].Width() * 0.5f;
                                    }

                                    center.X += (float)((cell.SpreadOverColumns - 1) * 20.0 * 0.5);
                                }
                                rows[index2][index1].Render(center, columnWidth);
                            }
                            ++num3;
                            y += 60f;
                        }
                        else
                        {
                            Draw.Rect(center.X - (columnWidth * 0.5f), center.Y - 25.5f, columnWidth + 20f, 6f, Color.Black * 0.3f);
                            y += 15f;
                        }
                    }
                    num1 += columnWidth + 20f;
                }
            }
        }

        public class Row
        {
            public List<OuiJournalPage.Cell> Entries = new();

            public OuiJournalPage.Row Add(OuiJournalPage.Cell entry)
            {
                Entries.Add(entry);
                return this;
            }

            public int Count => Entries.Count;

            public OuiJournalPage.Cell this[int index] => Entries[index];
        }

        public abstract class Cell
        {
            public int SpreadOverColumns = 1;

            public virtual float Width()
            {
                return 0.0f;
            }

            public virtual void Render(Vector2 center, float columnWidth)
            {
            }
        }

        public class EmptyCell : OuiJournalPage.Cell
        {
            private readonly float width;

            public EmptyCell(float width)
            {
                this.width = width;
            }

            public override float Width()
            {
                return width;
            }
        }

        public class TextCell : OuiJournalPage.Cell
        {
            private readonly string text;
            private Vector2 justify;
            private readonly float scale;
            private Color color;
            private readonly float width;
            private readonly bool forceWidth;

            public TextCell(
                string text,
                Vector2 justify,
                float scale,
                Color color,
                float width = 0.0f,
                bool forceWidth = false)
            {
                this.text = text;
                this.justify = justify;
                this.scale = scale;
                this.color = color;
                this.width = width;
                this.forceWidth = forceWidth;
            }

            public override float Width()
            {
                return forceWidth ? width : Math.Max(width, ActiveFont.Measure(text).X * scale);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num1 = ActiveFont.Measure(text).X * scale;
                float num2 = 1f;
                if (!forceWidth && (double)num1 > (double)columnWidth)
                {
                    num2 = columnWidth / num1;
                }

                ActiveFont.Draw(text, center + new Vector2((float)((-(double)columnWidth / 2.0) + ((double)columnWidth * justify.X)), 0.0f), justify, Vector2.One * scale * num2, color);
            }
        }

        public class IconCell : OuiJournalPage.Cell
        {
            private readonly string icon;
            private readonly float width;

            public IconCell(string icon, float width = 0.0f)
            {
                this.icon = icon;
                this.width = width;
            }

            public override float Width()
            {
                return Math.Max(MTN.Journal[icon].Width, width);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                MTN.Journal[icon].DrawCentered(center);
            }
        }

        public class IconsCell : OuiJournalPage.Cell
        {
            private readonly float iconSpacing = 4f;
            private readonly string[] icons;

            public IconsCell(float iconSpacing, params string[] icons)
            {
                this.iconSpacing = iconSpacing;
                this.icons = icons;
            }

            public IconsCell(params string[] icons)
            {
                this.icons = icons;
            }

            public override float Width()
            {
                float num = 0.0f;
                for (int index = 0; index < icons.Length; ++index)
                {
                    num += MTN.Journal[icons[index]].Width;
                }

                return num + ((icons.Length - 1) * iconSpacing);
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = Width();
                Vector2 position = center + new Vector2((float)(-(double)num * 0.5), 0.0f);
                for (int index = 0; index < icons.Length; ++index)
                {
                    MTexture mtexture = MTN.Journal[icons[index]];
                    mtexture.DrawJustified(position, new Vector2(0.0f, 0.5f));
                    position.X += mtexture.Width + iconSpacing;
                }
            }
        }
    }
}
