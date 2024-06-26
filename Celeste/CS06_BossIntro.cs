﻿using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CS06_BossIntro : CutsceneEntity
    {
        public const string Flag = "boss_intro";
        private Player player;
        private FinalBoss boss;
        private Vector2 bossEndPosition;
        private BadelineAutoAnimator animator;
        private float playerTargetX;

        public CS06_BossIntro(float playerTargetX, Player player, FinalBoss boss)
        {
            this.player = player;
            this.boss = boss;
            this.playerTargetX = playerTargetX;
            bossEndPosition = boss.Position + new Vector2(0.0f, -16f);
        }

        public override void OnBegin(Level level) => Add(new Coroutine(Cutscene(level)));

        private IEnumerator Cutscene(Level level)
        {
            CS06_BossIntro cs06BossIntro = this;
            cs06BossIntro.player.StateMachine.State = 11;
            cs06BossIntro.player.StateMachine.Locked = true;
            while (!cs06BossIntro.player.Dead && !cs06BossIntro.player.OnGround())
                yield return null;
            while (cs06BossIntro.player.Dead)
                yield return null;
            cs06BossIntro.player.Facing = Facings.Right;
            cs06BossIntro.Add(new Coroutine(CutsceneEntity.CameraTo(new Vector2((float) ((cs06BossIntro.player.X + (double) cs06BossIntro.boss.X) / 2.0 - 160.0), level.Bounds.Bottom - 180), 1f)));
            yield return 0.5f;
            if (!cs06BossIntro.player.Dead)
                yield return cs06BossIntro.player.DummyWalkToExact((int) (cs06BossIntro.playerTargetX - 8.0));
            cs06BossIntro.player.Facing = Facings.Right;
            yield return Textbox.Say("ch6_boss_start", cs06BossIntro.BadelineFloat, cs06BossIntro.PlayerStepForward);
            yield return level.ZoomBack(0.5f);
            cs06BossIntro.EndCutscene(level);
        }

        private IEnumerator BadelineFloat()
        {
            CS06_BossIntro cs06BossIntro = this;
            cs06BossIntro.Add(new Coroutine(cs06BossIntro.Level.ZoomTo(new Vector2(170f, 110f), 2f, 1f)));
            Audio.Play("event:/char/badeline/boss_prefight_getup", cs06BossIntro.boss.Position);
            cs06BossIntro.boss.Sitting = false;
            cs06BossIntro.boss.NormalSprite.Play("fallSlow");
            cs06BossIntro.boss.NormalSprite.Scale.X = -1f;
            cs06BossIntro.boss.Add(cs06BossIntro.animator = new BadelineAutoAnimator());
            float fromY = cs06BossIntro.boss.Y;
            for (float p = 0.0f; p < 1.0; p += Engine.DeltaTime * 4f)
            {
                cs06BossIntro.boss.Position.Y = MathHelper.Lerp(fromY, cs06BossIntro.bossEndPosition.Y, Ease.CubeInOut(p));
                yield return null;
            }
        }

        private IEnumerator PlayerStepForward()
        {
            yield return player.DummyWalkToExact((int) player.X + 8);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped && this.player != null)
            {
                this.player.X = playerTargetX;
                Player player;
                for (; !this.player.OnGround() && this.player.Y < (double) level.Bounds.Bottom; ++player.Y)
                    player = this.player;
            }
            this.player.StateMachine.Locked = false;
            this.player.StateMachine.State = 0;
            boss.Position = bossEndPosition;
            if (boss.NormalSprite != null)
            {
                boss.NormalSprite.Scale.X = -1f;
                boss.NormalSprite.Play("laugh");
            }
            boss.Sitting = false;
            if (animator != null)
                boss.Remove(animator);
            level.Session.SetFlag("boss_intro");
        }
    }
}
