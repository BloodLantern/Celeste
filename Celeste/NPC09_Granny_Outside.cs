// Decompiled with JetBrains decompiler
// Type: Celeste.NPC09_Granny_Outside
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class NPC09_Granny_Outside : NPC
    {
        public const string Flag = "granny_outside";
        public Hahaha Hahaha;
        public GrannyLaughSfx LaughSfx;
        private bool talking;
        private Player player;
        private bool leaving;

        public NPC09_Granny_Outside(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            this.Add((Component) (this.Sprite = GFX.SpriteBank.Create("granny")));
            this.Sprite.Play("idle");
            this.Add((Component) (this.LaughSfx = new GrannyLaughSfx(this.Sprite)));
            this.MoveAnim = "walk";
            this.IdleAnim = "idle";
            this.Maxspeed = 40f;
            this.SetupGrannySpriteSounds();
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if ((scene as Level).Session.GetFlag("granny_outside"))
                this.RemoveSelf();
            scene.Add((Entity) (this.Hahaha = new Hahaha(this.Position + new Vector2(8f, -4f))));
            this.Hahaha.Enabled = false;
        }

        public override void Update()
        {
            if (!this.talking)
            {
                this.player = this.Level.Tracker.GetEntity<Player>();
                if (this.player != null && (double) this.player.X > (double) this.X - 48.0)
                {
                    (this.Scene as Level).StartCutscene(new Action<Level>(this.EndTalking));
                    this.Add((Component) new Coroutine(this.TalkRoutine(this.player)));
                    this.talking = true;
                }
            }
            this.Hahaha.Enabled = this.Sprite.CurrentAnimationID == "laugh";
            base.Update();
        }

        private IEnumerator TalkRoutine(Player player)
        {
            NPC09_Granny_Outside c09GrannyOutside = this;
            player.StateMachine.State = 11;
            while (!player.OnGround())
                yield return (object) null;
            c09GrannyOutside.Sprite.Scale.X = -1f;
            yield return (object) player.DummyWalkToExact((int) c09GrannyOutside.X - 16);
            yield return (object) 0.5f;
            yield return (object) c09GrannyOutside.Level.ZoomTo(new Vector2(200f, 110f), 2f, 0.5f);
            yield return (object) Textbox.Say("APP_OLDLADY_A", new Func<IEnumerator>(c09GrannyOutside.MoveRight), new Func<IEnumerator>(c09GrannyOutside.ExitRight));
            yield return (object) c09GrannyOutside.Level.ZoomBack(0.5f);
            c09GrannyOutside.Sprite.Scale.X = 1f;
            if (!c09GrannyOutside.leaving)
                yield return (object) c09GrannyOutside.ExitRight();
            while ((double) c09GrannyOutside.X < (double) (c09GrannyOutside.Level.Bounds.Right + 8))
                yield return (object) null;
            c09GrannyOutside.Level.EndCutscene();
            c09GrannyOutside.EndTalking(c09GrannyOutside.Level);
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator MoveRight()
        {
                yield return base.MoveTo(new Vector2(base.X + 8f, base.Y), false, null, false);
                yield break;
        }

        // ISSUE: reference to a compiler-generated field
        private IEnumerator ExitRight()
        {
                this.leaving = true;
                base.Add(new Coroutine(base.MoveTo(new Vector2((float)(this.Level.Bounds.Right + 16), base.Y), false, null, false), true));
                yield return null;
                yield break;
        }

        private void EndTalking(Level level)
        {
            if (this.player != null)
                this.player.StateMachine.State = 0;
            this.Level.Session.SetFlag("granny_outside");
            this.RemoveSelf();
        }
    }
}
