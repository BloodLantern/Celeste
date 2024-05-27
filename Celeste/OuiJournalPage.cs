using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public abstract class OuiJournalPage
    {
        public const int PageWidth = 1610;
        public const int PageHeight = 1000;
        public readonly Vector2 TextJustify = new Vector2(0.5f, 0.5f);
        public const float TextScale = 0.5f;
        public readonly Color TextColor = Color.Black * 0.6f;
        public int PageIndex;
        public string PageTexture;
        public OuiJournal Journal;

        public OuiJournalPage(OuiJournal journal) => Journal = journal;

        public virtual void Redraw(VirtualRenderTarget buffer)
        {
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
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
            private List<Row> rows = new List<Row>();

            public int Rows => rows.Count;

            public Row Header => rows.Count <= 0 ? null : rows[0];

            public Table AddColumn(Cell label)
            {
                if (rows.Count == 0)
                    AddRow();
                rows[0].Add(label);
                return this;
            }

            public Row AddRow()
            {
                Row row = new Row();
                rows.Add(row);
                return row;
            }

            public float Height() => (float) (100.0 + 60.0 * (rows.Count - 1));

            public void Render(Vector2 position)
            {
                if (Header == null)
                    return;
                float num1 = 0.0f;
                float num2 = 0.0f;
                for (int index = 0; index < Header.Count; ++index)
                    num2 += Header[index].Width() + 20f;
                for (int index1 = 0; index1 < Header.Count; ++index1)
                {
                    float columnWidth = Header[index1].Width();
                    Header[index1].Render(position + new Vector2(num1 + columnWidth * 0.5f, 40f), columnWidth);
                    int num3 = 1;
                    float y = 130f;
                    for (int index2 = 1; index2 < rows.Count; ++index2)
                    {
                        Vector2 center = position + new Vector2(num1 + columnWidth * 0.5f, y);
                        if (rows[index2].Count > 0)
                        {
                            if (num3 % 2 == 0)
                                Draw.Rect(center.X - columnWidth * 0.5f, center.Y - 27f, columnWidth + 20f, 54f, Color.Black * 0.08f);
                            if (index1 < rows[index2].Count && rows[index2][index1] != null)
                            {
                                Cell cell = rows[index2][index1];
                                if (cell.SpreadOverColumns > 1)
                                {
                                    for (int index3 = index1 + 1; index3 < index1 + cell.SpreadOverColumns; ++index3)
                                        center.X += Header[index3].Width() * 0.5f;
                                    center.X += (float) ((cell.SpreadOverColumns - 1) * 20.0 * 0.5);
                                }
                                rows[index2][index1].Render(center, columnWidth);
                            }
                            ++num3;
                            y += 60f;
                        }
                        else
                        {
                            Draw.Rect(center.X - columnWidth * 0.5f, center.Y - 25.5f, columnWidth + 20f, 6f, Color.Black * 0.3f);
                            y += 15f;
                        }
                    }
                    num1 += columnWidth + 20f;
                }
            }
        }

        public class Row
        {
            public List<Cell> Entries = new List<Cell>();

            public Row Add(Cell entry)
            {
                Entries.Add(entry);
                return this;
            }

            public int Count => Entries.Count;

            public Cell this[int index] => Entries[index];
        }

        public abstract class Cell
        {
            public int SpreadOverColumns = 1;

            public virtual float Width() => 0.0f;

            public virtual void Render(Vector2 center, float columnWidth)
            {
            }
        }

        public class EmptyCell : Cell
        {
            private float width;

            public EmptyCell(float width) => this.width = width;

            public override float Width() => width;
        }

        public class TextCell : Cell
        {
            private string text;
            private Vector2 justify;
            private float scale;
            private Color color;
            private float width;
            private bool forceWidth;

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

            public override float Width() => forceWidth ? width : Math.Max(width, ActiveFont.Measure(text).X * scale);

            public override void Render(Vector2 center, float columnWidth)
            {
                float num1 = ActiveFont.Measure(text).X * scale;
                float num2 = 1f;
                if (!forceWidth && num1 > (double) columnWidth)
                    num2 = columnWidth / num1;
                ActiveFont.Draw(text, center + new Vector2((float) (-(double) columnWidth / 2.0 + columnWidth * (double) justify.X), 0.0f), justify, Vector2.One * scale * num2, color);
            }
        }

        public class IconCell : Cell
        {
            private string icon;
            private float width;

            public IconCell(string icon, float width = 0.0f)
            {
                this.icon = icon;
                this.width = width;
            }

            public override float Width() => Math.Max(MTN.Journal[icon].Width, width);

            public override void Render(Vector2 center, float columnWidth) => MTN.Journal[icon].DrawCentered(center);
        }

        public class IconsCell : Cell
        {
            private float iconSpacing = 4f;
            private string[] icons;

            public IconsCell(float iconSpacing, params string[] icons)
            {
                this.iconSpacing = iconSpacing;
                this.icons = icons;
            }

            public IconsCell(params string[] icons) => this.icons = icons;

            public override float Width()
            {
                float num = 0.0f;
                for (int index = 0; index < icons.Length; ++index)
                    num += MTN.Journal[icons[index]].Width;
                return num + (icons.Length - 1) * iconSpacing;
            }

            public override void Render(Vector2 center, float columnWidth)
            {
                float num = Width();
                Vector2 position = center + new Vector2((float) (-(double) num * 0.5), 0.0f);
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
