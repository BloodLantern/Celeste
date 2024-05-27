using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OuiJournalDeaths : OuiJournalPage
    {
        private Table table;

        public OuiJournalDeaths(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_deaths"), new Vector2(1f, 0.5f), 0.7f, TextColor, 300f));
            for (int index = 0; index < SaveData.Instance.UnlockedModes; ++index)
                table.AddColumn(new TextCell(Dialog.Clean("journal_mode_" + (AreaMode) index), TextJustify, 0.6f, TextColor, 240f));
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
                    Row row = table.AddRow();
                    row.Add(new TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
                    for (int mode = 0; mode < SaveData.Instance.UnlockedModes; ++mode)
                    {
                        if (areaData.HasMode((AreaMode) mode))
                        {
                            if (area.Modes[mode].SingleRunCompleted)
                            {
                                int deaths = area.Modes[mode].BestDeaths;
                                if (deaths > 0)
                                {
                                    foreach (EntityData goldenberry in AreaData.Areas[area.ID].Mode[mode].MapData.Goldenberries)
                                    {
                                        EntityID entityId = new EntityID(goldenberry.Level.Name, goldenberry.ID);
                                        if (area.Modes[mode].Strawberries.Contains(entityId))
                                            deaths = 0;
                                    }
                                }
                                row.Add(new TextCell(Dialog.Deaths(deaths), TextJustify, 0.5f, TextColor));
                                numArray[mode] += deaths;
                            }
                            else
                            {
                                row.Add(new IconCell("dot"));
                                flagArray[mode] = false;
                            }
                        }
                        else
                            row.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
                    }
                }
            }
            if (flagArray[0] || flagArray[1] || flagArray[2])
            {
                table.AddRow();
                Row row = table.AddRow();
                row.Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor));
                for (int index = 0; index < SaveData.Instance.UnlockedModes; ++index)
                    row.Add(new TextCell(Dialog.Deaths(numArray[index]), TextJustify, 0.6f, TextColor));
                table.AddRow();
            }
            int num = 0;
            foreach (AreaStats area in SaveData.Instance.Areas)
            {
                AreaData areaData = AreaData.Get(area.ID);
                if (areaData.IsFinal)
                {
                    if (areaData.ID <= SaveData.Instance.UnlockedAreas)
                    {
                        Row row = table.AddRow();
                        row.Add(new TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor));
                        if (area.Modes[0].SingleRunCompleted)
                        {
                            int deaths = area.Modes[0].BestDeaths;
                            if (deaths > 0)
                            {
                                foreach (EntityData goldenberry in AreaData.Areas[area.ID].Mode[0].MapData.Goldenberries)
                                {
                                    EntityID entityId = new EntityID(goldenberry.Level.Name, goldenberry.ID);
                                    if (area.Modes[0].Strawberries.Contains(entityId))
                                        deaths = 0;
                                }
                            }
                            TextCell entry = new TextCell(Dialog.Deaths(deaths), TextJustify, 0.5f, TextColor);
                            row.Add(entry);
                            num += deaths;
                        }
                        else
                            row.Add(new IconCell("dot"));
                        table.AddRow();
                    }
                    else
                        break;
                }
            }
            if (!flagArray[0] || !flagArray[1] || !flagArray[2])
                return;
            TextCell entry1 = new TextCell(Dialog.Deaths(numArray[0] + numArray[1] + numArray[2] + num), TextJustify, 0.6f, TextColor);
            entry1.SpreadOverColumns = 3;
            table.AddRow().Add(new TextCell(Dialog.Clean("journal_grandtotal"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(entry1);
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
