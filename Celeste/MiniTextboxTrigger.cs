using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MiniTextboxTrigger : Trigger
    {
        private EntityID id;
        private string[] dialogOptions;
        private Modes mode;
        private bool triggered;
        private bool onlyOnce;
        private int deathCount;

        public MiniTextboxTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            mode = data.Enum<Modes>(nameof (mode));
            dialogOptions = data.Attr("dialog_id").Split(',');
            onlyOnce = data.Bool("only_once");
            deathCount = data.Int("death_count", -1);
            if (mode != Modes.OnTheoEnter)
                return;
            Add(new HoldableCollider(c => Trigger()));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (mode != Modes.OnLevelStart)
                return;
            Trigger();
        }

        public override void OnEnter(Player player)
        {
            if (mode != Modes.OnPlayerEnter)
                return;
            Trigger();
        }

        private void Trigger()
        {
            if (triggered || deathCount >= 0 && (Scene as Level).Session.DeathsInCurrentLevel != deathCount)
                return;
            triggered = true;
            Scene.Add(new MiniTextbox(Calc.Random.Choose(dialogOptions)));
            if (!onlyOnce)
                return;
            (Scene as Level).Session.DoNotLoad.Add(id);
        }

        private enum Modes
        {
            OnPlayerEnter,
            OnLevelStart,
            OnTheoEnter,
        }
    }
}
