using Monocle;
using System.Collections;

namespace Celeste
{
    public abstract class Oui : Entity
    {
        public bool Focused;

        public Overworld Overworld => SceneAs<Overworld>();

        public bool Selected => Overworld != null && Overworld.Current == this;

        public Oui() => AddTag((int) Tags.HUD);

        public virtual bool IsStart(Overworld overworld, Overworld.StartMode start) => false;

        public abstract IEnumerator Enter(Oui from);

        public abstract IEnumerator Leave(Oui next);
    }
}
