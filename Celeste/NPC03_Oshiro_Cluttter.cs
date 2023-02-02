// Decompiled with JetBrains decompiler
// Type: Celeste.NPC03_Oshiro_Cluttter
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
  public class NPC03_Oshiro_Cluttter : NPC
  {
    public const string TalkFlagsA = "oshiro_clutter_";
    public const string TalkFlagsB = "oshiro_clutter_optional_";
    public const string ClearedFlags = "oshiro_clutter_cleared_";
    public const string FinishedFlag = "oshiro_clutter_finished";
    public const string DoorOpenFlag = "oshiro_clutter_door_open";
    public Vector2 HomePosition;
    private int sectionsComplete;
    private bool talked;
    private bool inRoutine;
    private List<Vector2> nodes = new List<Vector2>();
    private Coroutine paceRoutine;
    private Coroutine talkRoutine;
    private SoundSource paceSfx;
    private float paceTimer;

    public NPC03_Oshiro_Cluttter(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Add((Component) (this.Sprite = (Sprite) new OshiroSprite(-1)));
      this.Add((Component) (this.Talker = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(0.0f, -24f), new Action<Player>(this.OnTalk))));
      this.Add((Component) (this.Light = new VertexLight(-Vector2.UnitY * 16f, Color.White, 1f, 32, 64)));
      this.MoveAnim = "move";
      this.IdleAnim = "idle";
      foreach (Vector2 node in data.Nodes)
        this.nodes.Add(node + offset);
      this.Add((Component) (this.paceSfx = new SoundSource()));
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      if (this.Session.GetFlag("oshiro_clutter_finished"))
      {
        this.RemoveSelf();
      }
      else
      {
        if (this.Session.GetFlag("oshiro_clutter_cleared_0"))
          ++this.sectionsComplete;
        if (this.Session.GetFlag("oshiro_clutter_cleared_1"))
          ++this.sectionsComplete;
        if (this.Session.GetFlag("oshiro_clutter_cleared_2"))
          ++this.sectionsComplete;
        if (this.sectionsComplete == 0 || this.sectionsComplete == 3)
          this.Sprite.Scale.X = 1f;
        if (this.sectionsComplete > 0)
          this.Position = this.nodes[this.sectionsComplete - 1];
        else if (!this.Session.GetFlag("oshiro_clutter_0"))
          this.Add((Component) (this.paceRoutine = new Coroutine(this.Pace())));
        if (this.sectionsComplete == 0 && this.Session.GetFlag("oshiro_clutter_0") && !this.Session.GetFlag("oshiro_clutter_optional_0"))
          this.Sprite.Play("idle_ground");
        if (this.sectionsComplete == 3 || this.Session.GetFlag("oshiro_clutter_optional_" + (object) this.sectionsComplete))
          this.Remove((Component) this.Talker);
      }
      this.HomePosition = this.Position;
    }

    public Vector2 ZoomPoint => this.sectionsComplete < 2 ? this.Position + new Vector2(0.0f, -30f) - this.Level.Camera.Position : this.Position + new Vector2(0.0f, -15f) - this.Level.Camera.Position;

    private void OnTalk(Player player)
    {
      this.talked = true;
      if (this.paceRoutine != null)
        this.paceRoutine.RemoveSelf();
      this.paceRoutine = (Coroutine) null;
      if (!this.Session.GetFlag("oshiro_clutter_" + (object) this.sectionsComplete))
      {
        this.Scene.Add((Entity) new CS03_OshiroClutter(player, this, this.sectionsComplete));
      }
      else
      {
        this.Level.StartCutscene(new Action<Level>(this.EndTalkRoutine));
        this.Session.SetFlag("oshiro_clutter_optional_" + (object) this.sectionsComplete);
        this.Add((Component) (this.talkRoutine = new Coroutine(this.TalkRoutine(player))));
        if (this.Talker == null)
          return;
        this.Talker.Enabled = false;
      }
    }

    private IEnumerator TalkRoutine(Player player)
    {
      NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
      yield return (object) c03OshiroCluttter.PlayerApproach(player, spacing: new float?(24f), side: new int?(c03OshiroCluttter.sectionsComplete == 1 || c03OshiroCluttter.sectionsComplete == 2 ? -1 : 1));
      yield return (object) c03OshiroCluttter.Level.ZoomTo(c03OshiroCluttter.ZoomPoint, 2f, 0.5f);
      yield return (object) Textbox.Say("CH3_OSHIRO_CLUTTER" + (object) c03OshiroCluttter.sectionsComplete + "_B", new Func<IEnumerator>(c03OshiroCluttter.StandUp));
      yield return (object) c03OshiroCluttter.Level.ZoomBack(0.5f);
      c03OshiroCluttter.Level.EndCutscene();
      c03OshiroCluttter.EndTalkRoutine(c03OshiroCluttter.Level);
    }

    private void EndTalkRoutine(Level level)
    {
      if (this.talkRoutine != null)
        this.talkRoutine.RemoveSelf();
      this.talkRoutine = (Coroutine) null;
      (this.Sprite as OshiroSprite).Pop("idle", false);
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (entity == null)
        return;
      entity.StateMachine.Locked = false;
      entity.StateMachine.State = 0;
    }

    // ISSUE: reference to a compiler-generated field
    private IEnumerator StandUp()
    {
        Audio.Play("event:/char/oshiro/chat_get_up", this.Position);
        (this.Sprite as OshiroSprite).Pop("idle", false);
        yield return 0.25f;
        yield break;
    }

    private IEnumerator Pace()
    {
      NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
      while (true)
      {
        (c03OshiroCluttter.Sprite as OshiroSprite).Wiggle();
        yield return (object) c03OshiroCluttter.PaceLeft();
        while ((double) c03OshiroCluttter.paceTimer < 2.2660000324249268)
          yield return (object) null;
        c03OshiroCluttter.paceTimer = 0.0f;
        (c03OshiroCluttter.Sprite as OshiroSprite).Wiggle();
        yield return (object) c03OshiroCluttter.PaceRight();
        while ((double) c03OshiroCluttter.paceTimer < 2.2660000324249268)
          yield return (object) null;
        c03OshiroCluttter.paceTimer = 0.0f;
      }
    }

    public IEnumerator PaceRight()
    {
      NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
      Vector2 homePosition = c03OshiroCluttter.HomePosition;
      if ((double) (c03OshiroCluttter.Position - homePosition).Length() > 8.0)
        c03OshiroCluttter.paceSfx.Play("event:/char/oshiro/move_04_pace_right");
      yield return (object) c03OshiroCluttter.MoveTo(homePosition);
    }

    public IEnumerator PaceLeft()
    {
      NPC03_Oshiro_Cluttter c03OshiroCluttter = this;
      Vector2 target = c03OshiroCluttter.HomePosition + new Vector2(-20f, 0.0f);
      if ((double) (c03OshiroCluttter.Position - target).Length() > 8.0)
        c03OshiroCluttter.paceSfx.Play("event:/char/oshiro/move_04_pace_left");
      yield return (object) c03OshiroCluttter.MoveTo(target);
    }

    public override void Update()
    {
      base.Update();
      this.paceTimer += Engine.DeltaTime;
      Player entity = this.Scene.Tracker.GetEntity<Player>();
      if (this.sectionsComplete == 3 && !this.inRoutine && entity != null && (double) entity.X < (double) this.X + 32.0 && (double) entity.Y <= (double) this.Y)
      {
        this.OnTalk(entity);
        this.inRoutine = true;
      }
      if (this.sectionsComplete != 0 || this.talked)
        return;
      Level scene = this.Scene as Level;
      if (entity != null && !entity.Dead)
      {
        float num = Calc.ClampedMap(Vector2.Distance(this.Center, entity.Center), 40f, 128f);
        scene.Session.Audio.Music.Layer(1, num);
        scene.Session.Audio.Music.Layer(2, 1f - num);
        scene.Session.Audio.Apply();
      }
      else
      {
        scene.Session.Audio.Music.Layer(1, true);
        scene.Session.Audio.Music.Layer(2, false);
        scene.Session.Audio.Apply();
      }
    }
  }
}
