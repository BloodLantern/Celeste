// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalDeaths
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OuiJournalDeaths : OuiJournalPage
    {
        private readonly OuiJournalPage.Table table;

        public OuiJournalDeaths(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            table = new OuiJournalPage.Table().AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_deaths"), new Vector2(1f, 0.5f), 0.7f, TextColor, 300f));
            for (int index = 0; index < SaveData.Instance.UnlockedModes; ++index)
            {
                _ = table.AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_mode_" + (AreaMode)index), TextJustify, 0.6f, TextColor, 240f));
            }

            bool[] flagArray = new bool[3]
            {
                true,
                SaveData.Instance.UnlockedModes >= 2,
                SaveData.Instance.UnlockedModes >= 3
            };
            int[] numArray = new int[3];
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (!areaData.Interlude && !areaData.IsFinal)
                {
                    if (areaData.ID > SaveData.Instance.UnlockedAreas)
                    {
                        flagArray[0] = flagArray[1] = flagArray[2] = false;
                        break;
                    }
                    OuiJournalPage.Row row = table.AddRow();
                    _ = row.Add(new OuiJournalPage.TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
                    for (int mode = 0; mode < SaveData.Instance.UnlockedModes; ++mode)
                    {
                        if (areaData.HasMode((AreaMode)mode))
                        {
                            if (area.Modes[mode].SingleRunCompleted)
                            {
                                int deaths = area.Modes[mode].BestDeaths;
                                if (deaths > 0)
                                {
                                    foreach (EntityData goldenberry in AreaData.Areas[area.ID].Mode[mode].MapData.Goldenberries)
                                    {
                                        EntityID entityId = new(goldenberry.Level.Name, goldenberry.ID);
                                        if (area.Modes[mode].Strawberries.Contains(entityId))
                                        {
                                            deaths = 0;
                                        }
                                    }
                                }
                                _ = row.Add(new OuiJournalPage.TextCell(Dialog.Deaths(deaths), TextJustify, 0.5f, TextColor));
                                numArray[mode] += deaths;
                            }
                            else
                            {
                                _ = row.Add(new OuiJournalPage.IconCell("dot"));
                                flagArray[mode] = false;
                            }
                        }
                        else
                        {
                            _ = row.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));
                        }
                    }
                }
            }
            if (flagArray[0] || flagArray[1] || flagArray[2])
            {
                _ = table.AddRow();
                OuiJournalPage.Row row = table.AddRow();
                _ = row.Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor));
                for (int index = 0; index < SaveData.Instance.UnlockedModes; ++index)
                {
                    _ = row.Add(new OuiJournalPage.TextCell(Dialog.Deaths(numArray[index]), TextJustify, 0.6f, TextColor));
                }

                _ = table.AddRow();
            }
            int num = 0;
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (areaData.IsFinal)
                {
                    if (areaData.ID <= SaveData.Instance.UnlockedAreas)
                    {
                        OuiJournalPage.Row row = table.AddRow();
                        _ = row.Add(new OuiJournalPage.TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
                        if (area.Modes[0].SingleRunCompleted)
                        {
                            int deaths = area.Modes[0].BestDeaths;
                            if (deaths > 0)
                            {
                                foreach (EntityData goldenberry in AreaData.Areas[area.ID].Mode[0].MapData.Goldenberries)
                                {
                                    EntityID entityId = new(goldenberry.Level.Name, goldenberry.ID);
                                    if (area.Modes[0].Strawberries.Contains(entityId))
                                    {
                                        deaths = 0;
                                    }
                                }
                            }
                            OuiJournalPage.TextCell entry = new(Dialog.Deaths(deaths), TextJustify, 0.5f, TextColor);
                            _ = row.Add(entry);
                            num += deaths;
                        }
                        else
                        {
                            _ = row.Add(new OuiJournalPage.IconCell("dot"));
                        }

                        _ = table.AddRow();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (!flagArray[0] || !flagArray[1] || !flagArray[2])
            {
                return;
            }

            OuiJournalPage.TextCell entry1 = new(Dialog.Deaths(numArray[0] + numArray[1] + numArray[2] + num), TextJustify, 0.6f, TextColor)
            {
                SpreadOverColumns = 3
            };
            _ = table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_grandtotal"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(entry1);
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
