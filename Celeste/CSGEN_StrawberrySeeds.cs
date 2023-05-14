// Decompiled with JetBrains decompiler
// Type: Celeste.CSGEN_StrawberrySeeds
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class CSGEN_StrawberrySeeds : CutsceneEntity
    {
        private readonly Strawberry strawberry;
        private Vector2 cameraStart;
        private ParticleSystem system;
        private EventInstance snapshot;
        private EventInstance sfx;

        public CSGEN_StrawberrySeeds(Strawberry strawberry)
            : base()
        {
            this.strawberry = strawberry;
        }

        public override void OnBegin(Level level)
        {
            cameraStart = level.Camera.Position;
            Add(new Coroutine(Cutscene(level)));
        }

        private IEnumerator Cutscene(Level level)
        {
            CSGEN_StrawberrySeeds csgenStrawberrySeeds = this;
            csgenStrawberrySeeds.sfx = Audio.Play("event:/game/general/seed_complete_main", csgenStrawberrySeeds.Position);
            csgenStrawberrySeeds.snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
            Player entity = csgenStrawberrySeeds.Scene.Tracker.GetEntity<Player>();
            if (entity != null)
            {
                csgenStrawberrySeeds.cameraStart = entity.CameraTarget;
            }

            foreach (StrawberrySeed seed in csgenStrawberrySeeds.strawberry.Seeds)
            {
                seed.OnAllCollected();
            }

            csgenStrawberrySeeds.strawberry.Depth = -2000002;
            csgenStrawberrySeeds.strawberry.AddTag((int)Tags.FrozenUpdate);
            yield return 0.35f;
            csgenStrawberrySeeds.Tag = (int)Tags.FrozenUpdate | (int)Tags.HUD;
            level.Frozen = true;
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 0.5f;
            level.Displacement.Clear();
            level.Displacement.Enabled = false;
            _ = Audio.BusPaused("bus:/gameplay_sfx/ambience", new bool?(true));
            _ = Audio.BusPaused("bus:/gameplay_sfx/char", new bool?(true));
            _ = Audio.BusPaused("bus:/gameplay_sfx/game/general/yes_pause", new bool?(true));
            _ = Audio.BusPaused("bus:/gameplay_sfx/game/chapters", new bool?(true));
            yield return 0.1f;
            csgenStrawberrySeeds.system = new ParticleSystem(-2000002, 50)
            {
                Tag = (int)Tags.FrozenUpdate
            };
            level.Add(csgenStrawberrySeeds.system);
            float num = 6.28318548f / csgenStrawberrySeeds.strawberry.Seeds.Count;
            float angleOffset = 1.57079637f;
            Vector2 zero = Vector2.Zero;
            foreach (StrawberrySeed seed in csgenStrawberrySeeds.strawberry.Seeds)
            {
                zero += seed.Position;
            }

            Vector2 averagePos = zero / csgenStrawberrySeeds.strawberry.Seeds.Count;
            foreach (StrawberrySeed seed in csgenStrawberrySeeds.strawberry.Seeds)
            {
                seed.StartSpinAnimation(averagePos, csgenStrawberrySeeds.strawberry.Position, angleOffset, 4f);
                angleOffset -= num;
            }
            Vector2 target = (csgenStrawberrySeeds.strawberry.Position - new Vector2(160f, 90f)).Clamp(level.Bounds.Left, level.Bounds.Top, level.Bounds.Right - 320, level.Bounds.Bottom - 180);
            csgenStrawberrySeeds.Add(new Coroutine(CutsceneEntity.CameraTo(target, 3.5f, Ease.CubeInOut)));
            yield return 4f;
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            _ = Audio.Play("event:/game/general/seed_complete_berry", csgenStrawberrySeeds.strawberry.Position);
            foreach (StrawberrySeed seed in csgenStrawberrySeeds.strawberry.Seeds)
            {
                seed.StartCombineAnimation(csgenStrawberrySeeds.strawberry.Position, 0.6f, csgenStrawberrySeeds.system);
            }

            yield return 0.6f;
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            foreach (Entity seed in csgenStrawberrySeeds.strawberry.Seeds)
            {
                seed.RemoveSelf();
            }

            csgenStrawberrySeeds.strawberry.CollectedSeeds();
            yield return 0.5f;
            float dist = (level.Camera.Position - csgenStrawberrySeeds.cameraStart).Length();
            yield return CutsceneEntity.CameraTo(csgenStrawberrySeeds.cameraStart, dist / 180f);
            if ((double)dist > 80.0)
            {
                yield return 0.25f;
            }

            level.EndCutscene();
            csgenStrawberrySeeds.OnEnd(level);
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                Audio.Stop(sfx);
            }

            level.OnEndOfFrame += () =>
            {
                if (WasSkipped)
                {
                    foreach (Entity seed in strawberry.Seeds)
                    {
                        seed.RemoveSelf();
                    }

                    strawberry.CollectedSeeds();
                    level.Camera.Position = cameraStart;
                }
                strawberry.Depth = -100;
                strawberry.RemoveTag((int)Tags.FrozenUpdate);
                level.Frozen = false;
                level.FormationBackdrop.Display = false;
                level.Displacement.Enabled = true;
            };
            RemoveSelf();
        }

        private void EndSfx()
        {
            _ = Audio.BusPaused("bus:/gameplay_sfx/ambience", new bool?(false));
            _ = Audio.BusPaused("bus:/gameplay_sfx/char", new bool?(false));
            _ = Audio.BusPaused("bus:/gameplay_sfx/game/general/yes_pause", new bool?(false));
            _ = Audio.BusPaused("bus:/gameplay_sfx/game/chapters", new bool?(false));
            Audio.ReleaseSnapshot(snapshot);
        }

        public override void Removed(Scene scene)
        {
            EndSfx();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            EndSfx();
            base.SceneEnd(scene);
        }
    }
}
