// Decompiled with JetBrains decompiler
// Type: Celeste.MiniTextboxTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class MiniTextboxTrigger : Trigger
    {
        private EntityID id;
        private readonly string[] dialogOptions;
        private readonly MiniTextboxTrigger.Modes mode;
        private bool triggered;
        private readonly bool onlyOnce;
        private readonly int deathCount;

        public MiniTextboxTrigger(EntityData data, Vector2 offset, EntityID id)
            : base(data, offset)
        {
            this.id = id;
            mode = data.Enum<MiniTextboxTrigger.Modes>(nameof(mode));
            dialogOptions = data.Attr("dialog_id").Split(',');
            onlyOnce = data.Bool("only_once");
            deathCount = data.Int("death_count", -1);
            if (mode != MiniTextboxTrigger.Modes.OnTheoEnter)
            {
                return;
            }

            Add(new HoldableCollider(c => Trigger()));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (mode != MiniTextboxTrigger.Modes.OnLevelStart)
            {
                return;
            }

            Trigger();
        }

        public override void OnEnter(Player player)
        {
            if (mode != MiniTextboxTrigger.Modes.OnPlayerEnter)
            {
                return;
            }

            Trigger();
        }

        private void Trigger()
        {
            if (triggered || (deathCount >= 0 && (Scene as Level).Session.DeathsInCurrentLevel != deathCount))
            {
                return;
            }

            triggered = true;
            Scene.Add(new MiniTextbox(Calc.Random.Choose<string>(dialogOptions)));
            if (!onlyOnce)
            {
                return;
            }
            _ = (Scene as Level).Session.DoNotLoad.Add(id);
        }

        private enum Modes
        {
            OnPlayerEnter,
            OnLevelStart,
            OnTheoEnter,
        }
    }
}
