﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS05_Badeline : CutsceneEntity
    {
        private Player player;
        private NPC05_Badeline npc;
        private BadelineDummy badeline;
        private int index;
        private bool moved;

        public static string GetFlag(int index) => "badeline_" + index;

        public CS05_Badeline(Player player, NPC05_Badeline npc, BadelineDummy badeline, int index)
        {
            this.player = player;
            this.npc = npc;
            this.badeline = badeline;
            this.index = index;
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS05_Badeline cs05Badeline = this;
            cs05Badeline.player.StateMachine.State = 11;
            cs05Badeline.player.StateMachine.Locked = true;
            yield return 0.25f;
            if (cs05Badeline.index == 3)
            {
                cs05Badeline.player.DummyAutoAnimate = false;
                cs05Badeline.player.Sprite.Play("tired");
                yield return 0.2f;
            }
            while (cs05Badeline.player.Scene != null && !cs05Badeline.player.OnGround())
                yield return null;
            Vector2 screenSpaceFocusPoint = (cs05Badeline.badeline.Center + cs05Badeline.player.Center) * 0.5f - cs05Badeline.Level.Camera.Position + new Vector2(0.0f, -12f);
            yield return cs05Badeline.Level.ZoomTo(screenSpaceFocusPoint, 2f, 0.5f);
            yield return Textbox.Say("ch5_shadow_maddy_" + cs05Badeline.index, cs05Badeline.BadelineLeaves);
            if (!cs05Badeline.moved)
                cs05Badeline.npc.MoveToNode(cs05Badeline.index);
            yield return cs05Badeline.Level.ZoomBack(0.5f);
            cs05Badeline.EndCutscene(level);
        }

        public override void OnEnd(Level level)
        {
            npc.SnapToNode(index);
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
            level.Session.SetFlag(CS05_Badeline.GetFlag(index));
        }

        private IEnumerator BadelineLeaves()
        {
            yield return 0.1f;
            moved = true;
            npc.MoveToNode(index);
            yield return 0.5f;
            player.Sprite.Play("tiredStill");
            yield return 0.5f;
            player.Sprite.Play("idle");
            yield return 0.6f;
        }
    }
}
