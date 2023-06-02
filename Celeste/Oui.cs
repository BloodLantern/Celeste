using Monocle;
using System.Collections;

namespace Celeste
{
    public abstract class Oui : Entity
    {
        public bool Focused;

        public Overworld Overworld => this.SceneAs<Overworld>();

        public bool Selected => this.Overworld != null && this.Overworld.Current == this;

        public Oui() => this.AddTag((int) Tags.HUD);

        public virtual bool IsStart(Overworld overworld, Overworld.StartMode start) => false;

        public abstract IEnumerator Enter(Oui from);

        public abstract IEnumerator Leave(Oui next);
    }
}
