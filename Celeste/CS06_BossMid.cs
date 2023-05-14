// Decompiled with JetBrains decompiler
// Type: Celeste.CS06_BossMid
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS06_BossMid : CutsceneEntity
    {
        public const string Flag = "boss_mid";
        private Player player;

        public CS06_BossMid()
            : base()
        {
        }

        public override void OnBegin(Level level)
        {
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CS06_BossMid cs06BossMid = this;
            while (cs06BossMid.player == null)
            {
                cs06BossMid.player = cs06BossMid.Scene.Tracker.GetEntity<Player>();
                yield return null;
            }
            cs06BossMid.player.StateMachine.State = 11;
            cs06BossMid.player.StateMachine.Locked = true;
            while (!cs06BossMid.player.OnGround())
            {
                yield return null;
            }

            yield return cs06BossMid.player.DummyWalkToExact((int)cs06BossMid.player.X + 20);
            yield return level.ZoomTo(new Vector2(80f, 110f), 2f, 0.5f);
            yield return Textbox.Say("ch6_boss_middle");
            yield return 0.1f;
            yield return level.ZoomBack(0.4f);
            cs06BossMid.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            Player player;
            if (WasSkipped && this.player != null)
            {
                for (; !this.player.OnGround() && (double)this.player.Y < level.Bounds.Bottom; ++player.Y)
                {
                    player = this.player;
                }
            }
            if (this.player != null)
            {
                this.player.StateMachine.Locked = false;
                this.player.StateMachine.State = 0;
            }
            level.Entities.FindFirst<FinalBoss>()?.OnPlayer(null);
            level.Session.SetFlag("boss_mid");
        }
    }
}
