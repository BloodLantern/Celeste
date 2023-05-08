// Decompiled with JetBrains decompiler
// Type: Celeste.CassetteBlockManager
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD.Studio;
using Monocle;

namespace Celeste
{
    [Tracked(false)]
    public class CassetteBlockManager : Entity
    {
        private int currentIndex;
        private float beatTimer;
        private int beatIndex;
        private float tempoMult;
        private int leadBeats;
        private int maxBeat;
        private bool isLevelMusic;
        private int beatIndexOffset;
        private EventInstance sfx;
        private EventInstance snapshot;

        public CassetteBlockManager()
        {
            Tag = (int)Tags.Global;
            Add(new TransitionListener()
            {
                OnOutBegin = () =>
                {
                    if (!SceneAs<Level>().HasCassetteBlocks)
                        RemoveSelf();
                    else
                    {
                        maxBeat = SceneAs<Level>().CassetteBlockBeats;
                        tempoMult = SceneAs<Level>().CassetteBlockTempo;
                    }
                }
            });
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            isLevelMusic = AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong == null;
            if (isLevelMusic)
            {
                leadBeats = 0;
                beatIndexOffset = 5;
            }
            else
            {
                beatIndexOffset = 0;
                leadBeats = 16;
                snapshot = Audio.CreateSnapshot("snapshot:/music_mains_mute");
            }
            maxBeat = SceneAs<Level>().CassetteBlockBeats;
            tempoMult = SceneAs<Level>().CassetteBlockTempo;
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (isLevelMusic)
            {
                return;
            }

            Audio.Stop(snapshot);
            Audio.Stop(sfx);
        }

        public override void SceneEnd(Scene scene)
        {
            base.SceneEnd(scene);
            if (isLevelMusic)
                return;

            Audio.Stop(snapshot);
            Audio.Stop(sfx);
        }

        public override void Update()
        {
            base.Update();
            if (isLevelMusic)
                sfx = Audio.CurrentMusicEventInstance;

            if (sfx == null && !isLevelMusic)
            {
                sfx = Audio.CreateInstance(AreaData.Areas[SceneAs<Level>().Session.Area.ID].CassetteSong);
                _ = Audio.Play("event:/game/general/cassette_block_switch_2");
            }
            else
                AdvanceMusic(Engine.DeltaTime * tempoMult);
        }

        public void AdvanceMusic(float time)
        {
            beatTimer += time;
            if (beatTimer < 1f / 6f)
                return;

            beatTimer -= 1f / 6f;
            ++beatIndex;
            beatIndex %= 256;
            if (beatIndex % 8 == 0)
            {
                ++currentIndex;
                currentIndex %= maxBeat;
                SetActiveIndex(currentIndex);
                if (!isLevelMusic)
                    _ = Audio.Play("event:/game/general/cassette_block_switch_2");

                Input.Rumble(RumbleStrength.Medium, RumbleLength.Short);
            }
            else if ((beatIndex + 1) % 8 == 0)
                SetWillActivate((currentIndex + 1) % maxBeat);
            else if ((beatIndex + 4) % 8 == 0 && !isLevelMusic)
                _ = Audio.Play("event:/game/general/cassette_block_switch_1");

            if (leadBeats > 0)
            {
                --leadBeats;
                if (leadBeats == 0)
                {
                    beatIndex = 0;
                    if (!isLevelMusic)
                        _ = (int) sfx.start();
                }
            }
            if (leadBeats > 0)
                return;

            _ = (int) sfx.setParameterValue("sixteenth_note", GetSixteenthNote());
        }

        public int GetSixteenthNote()
        {
            return (beatIndex + beatIndexOffset) % 256 + 1;
        }

        public void StopBlocks()
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
                entity.Finish();

            if (isLevelMusic)
                return;

            Audio.Stop(sfx);
        }

        public void Finish()
        {
            if (!isLevelMusic)
                Audio.Stop(snapshot);

            RemoveSelf();
        }

        public void OnLevelStart()
        {
            maxBeat = SceneAs<Level>().CassetteBlockBeats;
            tempoMult = SceneAs<Level>().CassetteBlockTempo;
            currentIndex = beatIndex % 8 < 5 ? maxBeat - 1 : maxBeat - 2;
            SilentUpdateBlocks();
        }

        private void SilentUpdateBlocks()
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
                if (entity.ID.Level == SceneAs<Level>().Session.Level)
                    entity.SetActivatedSilently(entity.Index == currentIndex);
        }

        public void SetActiveIndex(int index)
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
                entity.Activated = entity.Index == index;
        }

        public void SetWillActivate(int index)
        {
            foreach (CassetteBlock entity in Scene.Tracker.GetEntities<CassetteBlock>())
                if (entity.Index == index || entity.Activated)
                    entity.WillToggle();
        }
    }
}
