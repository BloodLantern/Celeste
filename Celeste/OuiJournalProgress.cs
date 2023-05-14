// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalProgress
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiJournalProgress : OuiJournalPage
    {
        private readonly OuiJournalPage.Table table;

        public OuiJournalProgress(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            table = new OuiJournalPage.Table().AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_progress"), new Vector2(0.0f, 0.5f), 1f, Color.Black * 0.7f)).AddColumn(new OuiJournalPage.EmptyCell(20f)).AddColumn(new OuiJournalPage.EmptyCell(64f)).AddColumn(new OuiJournalPage.EmptyCell(64f)).AddColumn(new OuiJournalPage.EmptyCell(100f)).AddColumn(new OuiJournalPage.IconCell("strawberry", 150f)).AddColumn(new OuiJournalPage.IconCell("skullblue", 100f));
            if (SaveData.Instance.UnlockedModes >= 2)
            {
                _ = table.AddColumn(new OuiJournalPage.IconCell("skullred", 100f));
            }

            if (SaveData.Instance.UnlockedModes >= 3)
            {
                _ = table.AddColumn(new OuiJournalPage.IconCell("skullgold", 100f));
            }

            _ = table.AddColumn(new OuiJournalPage.IconCell("time", 220f));
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (!areaData.Interlude)
                {
                    if (areaData.ID <= SaveData.Instance.UnlockedAreas)
                    {
                        string text;
                        if (areaData.Mode[0].TotalStrawberries > 0 || area.TotalStrawberries > 0)
                        {
                            text = area.TotalStrawberries.ToString();
                            if (area.Modes[0].Completed)
                            {
                                text = text + "/" + areaData.Mode[0].TotalStrawberries;
                            }
                        }
                        else
                        {
                            text = "-";
                        }

                        List<string> stringList = new();
                        for (int index = 0; index < area.Modes.Length; ++index)
                        {
                            if (area.Modes[index].HeartGem)
                            {
                                stringList.Add("heartgem" + index);
                            }
                        }
                        if (stringList.Count <= 0)
                        {
                            stringList.Add("dot");
                        }

                        OuiJournalPage.Row row1 = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor)).Add(null);
                        string[] strArray = new string[1]
                        {
                            CompletionIcon(area)
                        };
                        OuiJournalPage.IconsCell entry1;
                        OuiJournalPage.IconsCell iconsCell = entry1 = new OuiJournalPage.IconsCell(strArray);
                        OuiJournalPage.Row row2 = row1.Add(entry1);
                        if (areaData.CanFullClear)
                        {
                            _ = row2.Add(new OuiJournalPage.IconsCell(new string[1]
                            {
                                area.Cassette ? "cassette" : "dot"
                            }));
                            _ = row2.Add(new OuiJournalPage.IconsCell(-32f, stringList.ToArray()));
                        }
                        else
                        {
                            iconsCell.SpreadOverColumns = 3;
                            _ = row2.Add(null).Add(null);
                        }
                        _ = row2.Add(new OuiJournalPage.TextCell(text, TextJustify, 0.5f, TextColor));
                        if (areaData.IsFinal)
                        {
                            OuiJournalPage.TextCell entry2 = new(Dialog.Deaths(area.Modes[0].Deaths), TextJustify, 0.5f, TextColor)
                            {
                                SpreadOverColumns = SaveData.Instance.UnlockedModes
                            };
                            _ = row2.Add(entry2);
                            for (int index = 0; index < SaveData.Instance.UnlockedModes - 1; ++index)
                            {
                                _ = row2.Add(null);
                            }
                        }
                        else
                        {
                            for (int mode = 0; mode < SaveData.Instance.UnlockedModes; ++mode)
                            {
                                _ = areaData.HasMode((AreaMode)mode)
                                    ? row2.Add(new OuiJournalPage.TextCell(Dialog.Deaths(area.Modes[mode].Deaths), TextJustify, 0.5f, TextColor))
                                    : row2.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));
                            }
                        }
                        _ = area.TotalTimePlayed > 0L
                            ? row2.Add(new OuiJournalPage.TextCell(Dialog.Time(area.TotalTimePlayed), TextJustify, 0.5f, TextColor))
                            : row2.Add(new OuiJournalPage.IconCell("dot"));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (table.Rows <= 1)
            {
                return;
            }

            _ = table.AddRow();
            OuiJournalPage.Row row = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(null).Add(null).Add(null).Add(null).Add(new OuiJournalPage.TextCell(SaveData.Instance.TotalStrawberries.ToString(), TextJustify, 0.6f, TextColor));
            OuiJournalPage.TextCell entry = new(Dialog.Deaths(SaveData.Instance.TotalDeaths), TextJustify, 0.6f, TextColor)
            {
                SpreadOverColumns = SaveData.Instance.UnlockedModes
            };
            _ = row.Add(entry);
            for (int index = 1; index < SaveData.Instance.UnlockedModes; ++index)
            {
                _ = row.Add(null);
            }

            _ = row.Add(new OuiJournalPage.TextCell(Dialog.Time(SaveData.Instance.Time), TextJustify, 0.6f, TextColor));
            _ = table.AddRow();
        }

        private string CompletionIcon(AreaStats data)
        {
            return !AreaData.Get(data.ID).CanFullClear && data.Modes[0].Completed
                ? "beat"
                : data.Modes[0].FullClear ? "fullclear" : data.Modes[0].Completed ? "clear" : "dot";
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            table.Render(new Vector2(60f, 20f));
            Draw.SpriteBatch.End();
        }

        private void DrawIcon(Vector2 pos, bool obtained, string icon)
        {
            if (obtained)
            {
                MTN.Journal[icon].DrawCentered(pos);
            }
            else
            {
                MTN.Journal["dot"].DrawCentered(pos);
            }
        }
    }
}
