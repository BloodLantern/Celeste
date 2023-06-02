using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class OuiJournalCover : OuiJournalPage
    {
        public OuiJournalCover(OuiJournal journal)
            : base(journal)
        {
            this.PageTexture = "cover";
        }

        public override void Redraw(VirtualRenderTarget buffer)
        {
            base.Redraw(buffer);
            Draw.SpriteBatch.Begin();
            string str = Dialog.Clean("journal_of");
            if (str.Length > 0)
                str += "\n";
            ActiveFont.Draw(SaveData.Instance == null || !Dialog.Language.CanDisplay(SaveData.Instance.Name) ? str + Dialog.Clean("FILE_DEFAULT") : str + SaveData.Instance.Name, new Vector2(805f, 400f), new Vector2(0.5f, 0.5f), Vector2.One * 2f, Color.Black * 0.5f);
            Draw.SpriteBatch.End();
        }
    }
}
