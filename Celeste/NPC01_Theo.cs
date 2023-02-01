// Decompiled with JetBrains decompiler
// Type: Celeste.NPC01_Theo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class NPC01_Theo : NPC
  {
    public static ParticleType P_YOLO;
    private const string DoneTalking = "theoDoneTalking";
    private int currentConversation;
    private Coroutine talkRoutine;

    public NPC01_Theo(Vector2 position)
      : base(position)
    {
      this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("theo")));
      this.Sprite.Play("idle");
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.currentConversation = this.Session.GetCounter("theo");
      if (this.Session.GetFlag("theoDoneTalking"))
        return;
      this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-8, -8, 88, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
    }

    private void OnTalk(Player player)
    {
      this.Level.StartCutscene(new Action<Level>(this.OnTalkEnd));
      this.Add((Component) (this.talkRoutine = new Coroutine(this.Talk(player))));
    }

    private IEnumerator Talk(Player player)
    {
      NPC01_Theo npC01Theo = this;
      if (npC01Theo.currentConversation == 0)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player);
        yield return (object) Textbox.Say("CH1_THEO_A", new Func<IEnumerator>(((NPC) npC01Theo).PlayerApproach48px));
      }
      else if (npC01Theo.currentConversation == 1)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player);
        yield return (object) 0.2f;
        yield return (object) npC01Theo.PlayerApproach(player, spacing: new float?(48f));
        yield return (object) Textbox.Say("CH1_THEO_B");
      }
      else if (npC01Theo.currentConversation == 2)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
        yield return (object) Textbox.Say("CH1_THEO_C");
      }
      else if (npC01Theo.currentConversation == 3)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
        yield return (object) Textbox.Say("CH1_THEO_D");
      }
      else if (npC01Theo.currentConversation == 4)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
        yield return (object) Textbox.Say("CH1_THEO_E");
      }
      else if (npC01Theo.currentConversation == 5)
      {
        yield return (object) npC01Theo.PlayerApproachRightSide(player, spacing: new float?(48f));
        yield return (object) Textbox.Say("CH1_THEO_F", new Func<IEnumerator>(npC01Theo.Yolo));
        npC01Theo.Sprite.Play("yoloEnd");
        npC01Theo.Remove((Component) npC01Theo.Talker);
        yield return (object) npC01Theo.Level.ZoomBack(0.5f);
      }
      npC01Theo.Level.EndCutscene();
      npC01Theo.OnTalkEnd(npC01Theo.Level);
    }

    private void OnTalkEnd(Level level)
    {
      if (this.currentConversation == 0)
        SaveData.Instance.SetFlag("MetTheo");
      else if (this.currentConversation == 1)
        SaveData.Instance.SetFlag("TheoKnowsName");
      else if (this.currentConversation == 5)
      {
        this.Session.SetFlag("theoDoneTalking");
        this.Remove((Component) this.Talker);
      }
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (entity != null)
      {
        entity.StateMachine.Locked = false;
        entity.StateMachine.State = 0;
      }
      this.Session.IncrementCounter("theo");
      ++this.currentConversation;
      this.talkRoutine.Cancel();
      this.talkRoutine.RemoveSelf();
      this.Sprite.Play("idle");
    }

    private IEnumerator Yolo()
    {
      NPC01_Theo npC01Theo = this;
      yield return (object) npC01Theo.Level.ZoomTo(new Vector2(128f, 128f), 2f, 0.5f);
      yield return (object) 0.2f;
      Audio.Play("event:/char/theo/yolo_fist", npC01Theo.Position);
      npC01Theo.Sprite.Play("yolo");
      yield return (object) 0.1f;
      npC01Theo.Level.DirectionalShake(-Vector2.UnitY);
      npC01Theo.Level.ParticlesFG.Emit(NPC01_Theo.P_YOLO, 6, npC01Theo.Position + new Vector2(-3f, -24f), Vector2.One * 4f);
      yield return (object) 0.5f;
    }
  }
}
