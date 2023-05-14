// Decompiled with JetBrains decompiler
// Type: Celeste.OuiJournalCover
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OuiJournalCover : OuiJournalPage
    {
        public OuiJournalCover(OuiJournal journal)
            : base(journal)
        {
            PageTexture = "cover";
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            string str = Dialog.Clean("journal_of");
            if (str.Length > 0)
            {
                str += "\n";
            }

            ActiveFont.Draw(SaveData.Instance == null || !Dialog.Language.CanDisplay(SaveData.Instance.Name) ? str + Dialog.Clean("FILE_DEFAULT") : str + SaveData.Instance.Name, new Vector2(805f, 400f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Black * 0.5f);
            Draw.SpriteBatch.End();
        }
    }
}
