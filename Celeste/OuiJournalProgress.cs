using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class OuiJournalProgress : OuiJournalPage
    {
        private Table table;

        public OuiJournalProgress(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            table = new Table().AddColumn(new TextCell(Dialog.Clean("journal_progress"), new Vector2(0.0f, 0.5f), 1f, Color.Black * 0.7f)).AddColumn(new EmptyCell(20f)).AddColumn(new EmptyCell(64f)).AddColumn(new EmptyCell(64f)).AddColumn(new EmptyCell(100f)).AddColumn(new IconCell("strawberry", 150f)).AddColumn(new IconCell("skullblue", 100f));
            if (SaveData.Instance.UnlockedModes >= 2)
                table.AddColumn(new IconCell("skullred", 100f));
            if (SaveData.Instance.UnlockedModes >= 3)
                table.AddColumn(new IconCell("skullgold", 100f));
            table.AddColumn(new IconCell("time", 220f));
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
                                text = text + "/" + areaData.Mode[0].TotalStrawberries;
                        }
                        else
                            text = "-";
                        List<string> stringList = new List<string>();
                        for (int index = 0; index < area.Modes.Length; ++index)
                        {
                            if (area.Modes[index].HeartGem)
                                stringList.Add("heartgem" + index);
                        }
                        if (stringList.Count <= 0)
                            stringList.Add("dot");
                        Row row1 = table.AddRow().Add(new TextCell(Dialog.Clean(areaData.Name), new Vector2(1f, 0.5f), 0.6f, TextColor)).Add(null);
                        string[] strArray = new string[1]
                        {
                            CompletionIcon(area)
                        };
                        IconsCell entry1;
                        IconsCell iconsCell = entry1 = new IconsCell(strArray);
                        Row row2 = row1.Add(entry1);
                        if (areaData.CanFullClear)
                        {
                            row2.Add(new IconsCell(area.Cassette ? "cassette" : "dot"));
                            row2.Add(new IconsCell(-32f, stringList.ToArray()));
                        }
                        else
                        {
                            iconsCell.SpreadOverColumns = 3;
                            row2.Add(null).Add(null);
                        }
                        row2.Add(new TextCell(text, TextJustify, 0.5f, TextColor));
                        if (areaData.IsFinal)
                        {
                            TextCell entry2 = new TextCell(Dialog.Deaths(area.Modes[0].Deaths), TextJustify, 0.5f, TextColor);
                            entry2.SpreadOverColumns = SaveData.Instance.UnlockedModes;
                            row2.Add(entry2);
                            for (int index = 0; index < SaveData.Instance.UnlockedModes - 1; ++index)
                                row2.Add(null);
                        }
                        else
                        {
                            for (int mode = 0; mode < SaveData.Instance.UnlockedModes; ++mode)
                            {
                                if (areaData.HasMode((AreaMode) mode))
                                    row2.Add(new TextCell(Dialog.Deaths(area.Modes[mode].Deaths), TextJustify, 0.5f, TextColor));
                                else
                                    row2.Add(new TextCell("-", TextJustify, 0.5f, TextColor));
                            }
                        }
                        if (area.TotalTimePlayed > 0L)
                            row2.Add(new TextCell(Dialog.Time(area.TotalTimePlayed), TextJustify, 0.5f, TextColor));
                        else
                            row2.Add(new IconCell("dot"));
                    }
                    else
                        break;
                }
            }
            if (table.Rows <= 1)
                return;
            table.AddRow();
            Row row = table.AddRow().Add(new TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(null).Add(null).Add(null).Add(null).Add(new TextCell(SaveData.Instance.TotalStrawberries.ToString(), TextJustify, 0.6f, TextColor));
            TextCell entry = new TextCell(Dialog.Deaths(SaveData.Instance.TotalDeaths), TextJustify, 0.6f, TextColor);
            entry.SpreadOverColumns = SaveData.Instance.UnlockedModes;
            row.Add(entry);
            for (int index = 1; index < SaveData.Instance.UnlockedModes; ++index)
                row.Add(null);
            row.Add(new TextCell(Dialog.Time(SaveData.Instance.Time), TextJustify, 0.6f, TextColor));
            table.AddRow();
        }

        private string CompletionIcon(AreaStats data)
        {
            if (!AreaData.Get(data.ID).CanFullClear && data.Modes[0].Completed)
                return "beat";
            if (data.Modes[0].FullClear)
                return "fullclear";
            return data.Modes[0].Completed ? "clear" : "dot";
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
                MTN.Journal[icon].DrawCentered(pos);
            else
                MTN.Journal["dot"].DrawCentered(pos);
        }
    }
}
