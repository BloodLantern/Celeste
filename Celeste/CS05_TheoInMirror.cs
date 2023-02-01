// Decompiled with JetBrains decompiler
// Type: Celeste.CS05_TheoInMirror
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
  public class CS05_TheoInMirror : CutsceneEntity
  {
    public const string Flag = "theoInMirror";
    private NPC theo;
    private Player player;
    private int playerFinalX;

    public CS05_TheoInMirror(NPC theo, Player player)
      : base()
    {
      this.theo = theo;
      this.player = player;
      this.playerFinalX = (int) theo.Position.X + 24;
    }

    public override void OnBegin(Level level) => this.Add((Component) new Coroutine(this.Cutscene(level)));

    private IEnumerator Cutscene(Level level)
    {
      CS05_TheoInMirror cs05TheoInMirror = this;
      cs05TheoInMirror.player.StateMachine.State = 11;
      cs05TheoInMirror.player.StateMachine.Locked = true;
      yield return (object) cs05TheoInMirror.player.DummyWalkTo(cs05TheoInMirror.theo.X - 16f);
      yield return (object) 0.5f;
      cs05TheoInMirror.theo.Sprite.Scale.X = -1f;
      yield return (object) 0.25f;
      yield return (object) Textbox.Say("ch5_theo_mirror");
      cs05TheoInMirror.Add((Component) new Coroutine(cs05TheoInMirror.theo.MoveTo(cs05TheoInMirror.theo.Position + new Vector2(64f, 0.0f))));
      yield return (object) 0.4f;
      yield return (object) cs05TheoInMirror.player.DummyWalkToExact(cs05TheoInMirror.playerFinalX);
      cs05TheoInMirror.EndCutscene(level);
    }

    public override void OnEnd(Level level)
    {
      this.player.StateMachine.Locked = false;
      this.player.StateMachine.State = 0;
      this.player.X = (float) this.playerFinalX;
      this.player.MoveV(200f);
      this.player.Speed = Vector2.Zero;
      this.Scene.Remove((Entity) this.theo);
      level.Session.SetFlag("theoInMirror");
    }
  }
}
