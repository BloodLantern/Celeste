// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalGlobal
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class OuiJournalGlobal : OuiJournalPage
    {
        private readonly OuiJournalPage.Table table;

        public OuiJournalGlobal(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "page";
            table = new OuiJournalPage.Table().AddColumn(new OuiJournalPage.TextCell("", new Vector2(1f, 0.5f), 1f, TextColor, 700f)).AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("STATS_TITLE"), new Vector2(0.5f, 0.5f), 1f, TextColor, 48f, true)).AddColumn(new OuiJournalPage.TextCell("", new Vector2(1f, 0.5f), 0.7f, TextColor, 700f));
            foreach (Stat stat in Enum.GetValues(typeof(Stat)))
            {
                if (SaveData.Instance.CheatMode || SaveData.Instance.DebugMode || ((stat != Stat.GOLDBERRIES || SaveData.Instance.TotalHeartGems >= 16) && ((stat != Stat.PICO_BERRIES && stat != Stat.PICO_COMPLETES && stat != Stat.PICO_DEATHS) || Settings.Instance.Pico8OnMainMenu)))
                {
                    string str = Stats.Global(stat).ToString();
                    string text1 = Stats.Name(stat);
                    string text2 = "";
                    int index = str.Length - 1;
                    int num = 0;
                    while (index >= 0)
                    {
                        text2 = str[index].ToString() + (num <= 0 || num % 3 != 0 ? "" : ",") + text2;
                        --index;
                        ++num;
                    }
                    OuiJournalPage.Row row = table.AddRow();
                    _ = row.Add(new OuiJournalPage.TextCell(text1, new Vector2(1f, 0.5f), 0.7f, TextColor));
                    _ = row.Add(null);
                    _ = row.Add(new OuiJournalPage.TextCell(text2, new Vector2(0.0f, 0.5f), 0.8f, TextColor));
                }
            }
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
