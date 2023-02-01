// Decompiled with JetBrains decompiler
// Type: Celeste.NPC07X_Granny_Ending
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class NPC07X_Granny_Ending : NPC
  {
    public Hahaha Hahaha;
    public GrannyLaughSfx LaughSfx;
    private Player player;
    private TalkComponent talker;
    private Coroutine talkRoutine;
    private int conversation;
    private bool ch9EasterEgg;

    public NPC07X_Granny_Ending(EntityData data, Vector2 offset, bool ch9EasterEgg = false)
      : base(data.Position + offset)
    {
      this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("granny")));
      this.Sprite.Play("idle");
      this.Sprite.Scale.X = -1f;
      this.Add((Component) (this.LaughSfx = new GrannyLaughSfx(this.Sprite)));
      this.Add((Component) (this.talker = new TalkComponent(new Rectangle(-20, -8, 40, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
      this.MoveAnim = "walk";
      this.Maxspeed = 40f;
      this.ch9EasterEgg = ch9EasterEgg;
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      scene.Add((Entity) (this.Hahaha = new Hahaha(this.Position + new Vector2(8f, -4f))));
      this.Hahaha.Enabled = false;
    }

    public override void Update()
    {
      this.Hahaha.Enabled = this.Sprite.CurrentAnimationID == "laugh";
      base.Update();
    }

    private void OnTalk(Player player)
    {
      this.player = player;
      (this.Scene as Level).StartCutscene(new Action<Level>(this.EndTalking));
      this.Add((Component) (this.talkRoutine = new Coroutine(this.TalkRoutine(player))));
    }

    private IEnumerator TalkRoutine(Player player)
    {
      NPC07X_Granny_Ending c07XGrannyEnding = this;
      player.StateMachine.State = 11;
      player.ForceCameraUpdate = true;
      while (!player.OnGround())
        yield return (object) null;
      yield return (object) player.DummyWalkToExact((int) c07XGrannyEnding.X - 16);
      player.Facing = Facings.Right;
      if (c07XGrannyEnding.ch9EasterEgg)
      {
        yield return (object) 0.5f;
        yield return (object) c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
        Dialog.Language.Dialog["CH10_GRANNY_EASTEREGG"] = "{portrait GRANNY right mock} I see you have discovered Debug Mode.";
        yield return (object) Textbox.Say("CH10_GRANNY_EASTEREGG");
        c07XGrannyEnding.talker.Enabled = false;
      }
      else if (c07XGrannyEnding.conversation == 0)
      {
        yield return (object) 0.5f;
        yield return (object) c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
        yield return (object) Textbox.Say("CH7_CSIDE_OLDLADY", new Func<IEnumerator>(c07XGrannyEnding.StartLaughing), new Func<IEnumerator>(c07XGrannyEnding.StopLaughing));
      }
      else if (c07XGrannyEnding.conversation == 1)
      {
        yield return (object) 0.5f;
        yield return (object) c07XGrannyEnding.Level.ZoomTo(c07XGrannyEnding.Position - c07XGrannyEnding.Level.Camera.Position + new Vector2(0.0f, -32f), 2f, 0.5f);
        yield return (object) Textbox.Say("CH7_CSIDE_OLDLADY_B", new Func<IEnumerator>(c07XGrannyEnding.StartLaughing), new Func<IEnumerator>(c07XGrannyEnding.StopLaughing));
        c07XGrannyEnding.talker.Enabled = false;
      }
      yield return (object) c07XGrannyEnding.Level.ZoomBack(0.5f);
      c07XGrannyEnding.Level.EndCutscene();
      c07XGrannyEnding.EndTalking(c07XGrannyEnding.Level);
    }

    private IEnumerator StartLaughing()
    {
      // ISSUE: reference to a compiler-generated field
      int num = this.\u003C\u003E1__state;
      NPC07X_Granny_Ending c07XGrannyEnding = this;
      if (num != 0)
      {
        if (num != 1)
          return false;
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E1__state = -1;
        return false;
      }
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      c07XGrannyEnding.Sprite.Play("laugh");
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E2__current = (object) null;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = 1;
      return true;
    }

    private IEnumerator StopLaughing()
    {
      // ISSUE: reference to a compiler-generated field
      int num = this.\u003C\u003E1__state;
      NPC07X_Granny_Ending c07XGrannyEnding = this;
      if (num != 0)
      {
        if (num != 1)
          return false;
        // ISSUE: reference to a compiler-generated field
        this.\u003C\u003E1__state = -1;
        return false;
      }
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      c07XGrannyEnding.Sprite.Play("idle");
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E2__current = (object) null;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = 1;
      return true;
    }

    private void EndTalking(Level level)
    {
      if (this.player != null)
      {
        this.player.StateMachine.State = 0;
        this.player.ForceCameraUpdate = false;
      }
      ++this.conversation;
      if (this.talkRoutine != null)
      {
        this.talkRoutine.RemoveSelf();
        this.talkRoutine = (Coroutine) null;
      }
      this.Sprite.Play("idle");
    }
  }
}
