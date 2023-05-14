// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalSpeedrun
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OuiJournalSpeedrun : OuiJournalPage
    {
        private readonly OuiJournalPage.Table table;

        public OuiJournalSpeedrun(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            Vector2 justify = new(0.5f, 0.5f);
            float scale = 0.5f;
            Color color = Color.Black * 0.6f;
            table = new OuiJournalPage.Table().AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_speedruns"), new Vector2(1f, 0.5f), 0.7f, Color.Black * 0.7f)).AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_mode_normal"), justify, scale + 0.1f, color, 240f)).AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_mode_normal_fullclear"), justify, scale + 0.1f, color, 240f));
            if (SaveData.Instance.UnlockedModes >= 2)
            {
                _ = table.AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_mode_bside"), justify, scale + 0.1f, color, 240f));
            }

            if (SaveData.Instance.UnlockedModes >= 3)
            {
                _ = table.AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_mode_cside"), justify, scale + 0.1f, color, 240f));
            }

            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (!areaData.Interlude && !areaData.IsFinal)
                {
                    if (areaData.ID <= SaveData.Instance.UnlockedAreas)
                    {
                        OuiJournalPage.Row row = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), scale + 0.1f, color));
                        _ = area.Modes[0].BestTime > 0L
                            ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(area.Modes[0].BestTime), justify, scale, color))
                            : row.Add(new OuiJournalPage.IconCell("dot"));

                        _ = areaData.CanFullClear
                            ? area.Modes[0].BestFullClearTime > 0L
                                ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(area.Modes[0].BestFullClearTime), justify, scale, color))
                                : row.Add(new OuiJournalPage.IconCell("dot"))
                            : row.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));

                        if (SaveData.Instance.UnlockedModes >= 2)
                        {
                            _ = areaData.HasMode(AreaMode.BSide)
                                ? area.Modes[1].BestTime > 0L
                                    ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(area.Modes[1].BestTime), justify, scale, color))
                                    : row.Add(new OuiJournalPage.IconCell("dot"))
                                : row.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));
                        }
                        if (SaveData.Instance.UnlockedModes >= 3)
                        {
                            _ = areaData.HasMode(AreaMode.CSide)
                                ? area.Modes[2].BestTime > 0L
                                    ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(area.Modes[2].BestTime), justify, scale, color))
                                    : row.Add(new OuiJournalPage.IconCell("dot"))
                                : row.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            bool flag1 = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            long ticks1 = 0;
            long ticks2 = 0;
            long ticks3 = 0;
            long ticks4 = 0;
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (!areaData.Interlude && !areaData.IsFinal)
                {
                    if (area.ID > SaveData.Instance.UnlockedAreas)
                    {
                        int num;
                        flag4 = (num = 0) != 0;
                        flag3 = num != 0;
                        flag2 = num != 0;
                        flag1 = num != 0;
                        break;
                    }
                    ticks1 += area.Modes[0].BestTime;
                    ticks2 += area.Modes[0].BestFullClearTime;
                    ticks3 += area.Modes[1].BestTime;
                    ticks4 += area.Modes[2].BestTime;
                    if (area.Modes[0].BestTime <= 0L)
                    {
                        flag1 = false;
                    }

                    if (areaData.CanFullClear && area.Modes[0].BestFullClearTime <= 0L)
                    {
                        flag2 = false;
                    }

                    if (areaData.HasMode(AreaMode.BSide) && area.Modes[1].BestTime <= 0L)
                    {
                        flag3 = false;
                    }

                    if (areaData.HasMode(AreaMode.CSide) && area.Modes[2].BestTime <= 0L)
                    {
                        flag4 = false;
                    }
                }
            }
            if (flag1 | flag2 | flag3 | flag4)
            {
                _ = table.AddRow();
                OuiJournalPage.Row row = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), scale + 0.2f, color));
                _ = flag1
                    ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(ticks1), justify, scale + 0.1f, color))
                    : row.Add(new OuiJournalPage.IconCell("dot"));

                _ = flag2
                    ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(ticks2), justify, scale + 0.1f, color))
                    : row.Add(new OuiJournalPage.IconCell("dot"));

                if (SaveData.Instance.UnlockedModes >= 2)
                {
                    _ = flag3
                        ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(ticks3), justify, scale + 0.1f, color))
                        : row.Add(new OuiJournalPage.IconCell("dot"));
                }
                if (SaveData.Instance.UnlockedModes >= 3)
                {
                    _ = flag4
                        ? row.Add(new OuiJournalPage.TextCell(Dialog.Time(ticks4), justify, scale + 0.1f, color))
                        : row.Add(new OuiJournalPage.IconCell("dot"));
                }
                _ = table.AddRow();
            }
            long num1 = 0;
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (areaData.IsFinal)
                {
                    if (areaData.ID <= SaveData.Instance.UnlockedAreas)
                    {
                        num1 += area.Modes[0].BestTime;
                        OuiJournalPage.Row row = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), scale + 0.1f, color));
                        _ = row.Add(null);
                        OuiJournalPage.Cell cell;
                        _ = area.Modes[0].BestTime > 0L
                            ? row.Add(cell = new OuiJournalPage.TextCell(Dialog.Time(area.Modes[0].BestTime), justify, scale, color))
                            : row.Add(cell = new OuiJournalPage.IconCell("dot"));

                        _ = table.AddRow();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (!(flag1 & flag2 & flag3 & flag4))
            {
                return;
            }

            OuiJournalPage.TextCell entry = new(Dialog.Time(ticks1 + ticks2 + ticks3 + ticks4 + num1), justify, scale + 0.2f, color)
            {
                SpreadOverColumns = 1 + SaveData.Instance.UnlockedModes
            };
            _ = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_grandtotal"), new Vector2(1f, 0.5f), scale + 0.3f, color)).Add(entry);
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            table.Render(new Vector2(60f, 20f));
            Draw.SpriteBatch.End();
        }
    }
}
