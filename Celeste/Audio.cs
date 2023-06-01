// Decompiled with JetBrains decompiler
// Type: Celeste.Audio
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using FMOD;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste
{
    public static class Audio
    {
        private static FMOD.Studio.System system;
        private static FMOD.Studio._3D_ATTRIBUTES attributes3d = new();
        public static Dictionary<string, EventDescription> cachedEventDescriptions = new();
        private static Camera currentCamera;
        private static bool ready;
        private static EventInstance currentMusicEvent = null;
        private static EventInstance currentAltMusicEvent = null;
        private static EventInstance currentAmbientEvent = null;
        private static EventInstance mainDownSnapshot = null;
        public static string CurrentMusic = "";
        private static bool musicUnderwater;
        private static EventInstance musicUnderwaterSnapshot;

        public static void Init()
        {
            FMOD.Studio.INITFLAGS studioFlags = FMOD.Studio.INITFLAGS.NORMAL;
            if (Settings.Instance.LaunchWithFMODLiveUpdate)
                studioFlags = FMOD.Studio.INITFLAGS.LIVEUPDATE;

            CheckFmod(FMOD.Studio.System.create(out system));
            CheckFmod(system.initialize(1024, studioFlags, FMOD.INITFLAGS.NORMAL, IntPtr.Zero));

            attributes3d.forward = new() { x = 0f, y = 0f, z = 1f };
            attributes3d.up = new() { x = 0f, y = 1f, z = 0f };
            SetListenerPosition(new Vector3(0, 0, 1), new Vector3(0, 1, 0), new Vector3(0, 0, -345));
            ready = true;
        }

        public static void Update()
        {
            if (system == null || !ready)
                return;

            CheckFmod(system.update());
        }

        public static void Unload()
        {
            if (system == null)
                return;

            CheckFmod(system.unloadAll());
            CheckFmod(system.release());
            system = null;
        }

        public static void SetListenerPosition(Vector3 forward, Vector3 up, Vector3 position)
        {
            system.setListenerAttributes(
                0,
                new FMOD.Studio._3D_ATTRIBUTES()
                {
                    forward = {
                        x = forward.X,
                        y = forward.Y,
                        z = forward.Z
                    },
                    up =
                    {
                        x = up.X,
                        y = up.Y,
                        z = up.Z
                    },
                    position =
                    {
                        x = position.X,
                        y = position.Y,
                        z = position.Z
                    }
                }
            );
        }

        public static void SetCamera(Camera camera)
        {
            currentCamera = camera;
        }

        internal static void CheckFmod(RESULT result)
        {
            if (result != RESULT.OK)
                throw new Exception("FMOD Failed: " + result);
        }

        public static EventInstance Play(string path)
        {
            EventInstance instance = CreateInstance(path);
            if (instance != null)
            {
                instance.start();
                instance.release();
            }
            return instance;
        }

        public static EventInstance Play(string path, string param, float value)
        {
            EventInstance instance = CreateInstance(path);
            if (instance != null)
            {
                SetParameter(instance, param, value);
                instance.start();
                instance.release();
            }
            return instance;
        }

        public static EventInstance Play(string path, Vector2 position)
        {
            EventInstance instance = CreateInstance(path, new Vector2?(position));
            if (instance != null)
            {
                instance.start();
                instance.release();
            }
            return instance;
        }

        public static EventInstance Play(
            string path,
            Vector2 position,
            string param,
            float value)
        {
            EventInstance instance = CreateInstance(path, new Vector2?(position));
            if (instance != null)
            {
                if (param != null)
                    instance.setParameterValue(param, value);

                instance.start();
                instance.release();
            }
            return instance;
        }

        public static EventInstance Play(
            string path,
            Vector2 position,
            string param,
            float value,
            string param2,
            float value2)
        {
            EventInstance instance = CreateInstance(path, new Vector2?(position));
            if (instance != null)
            {
                if (param != null)
                    instance.setParameterValue(param, value);

                if (param2 != null)
                    instance.setParameterValue(param2, value2);

                instance.start();
                instance.release();
            }
            return instance;
        }

        public static EventInstance Loop(string path)
        {
            EventInstance instance = CreateInstance(path);
            if (instance != null)
                instance.start();

            return instance;
        }

        public static EventInstance Loop(string path, string param, float value)
        {
            EventInstance instance = CreateInstance(path);
            if (instance != null)
            {
                instance.setParameterValue(param, value);
                instance.start();
            }
            return instance;
        }

        public static EventInstance Loop(string path, Vector2 position)
        {
            EventInstance instance = CreateInstance(path, new Vector2?(position));
            if (instance != null)
                instance.start();

            return instance;
        }

        public static EventInstance Loop(
            string path,
            Vector2 position,
            string param,
            float value)
        {
            EventInstance instance = CreateInstance(path, new Vector2?(position));
            if (instance != null)
            {
                instance.setParameterValue(param, value);
                instance.start();
            }
            return instance;
        }

        public static void Pause(EventInstance instance)
        {
            if (instance == null)
                return;

            instance.setPaused(true);
        }

        public static void Resume(EventInstance instance)
        {
            if (instance == null)
                return;

            instance.setPaused(false);
        }

        public static void Position(EventInstance instance, Vector2 position)
        {
            if (instance == null)
                return;

            Vector2 cameraCenter = Vector2.Zero;
            if (currentCamera != null)
                cameraCenter = currentCamera.Position + (new Vector2(320f, 180f) / 2f);

            float num1 = position.X - cameraCenter.X;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                num1 = -num1;

            attributes3d.position.x = num1;
            attributes3d.position.y = position.Y - cameraCenter.Y;
            attributes3d.position.z = 0.0f;
            instance.set3DAttributes(attributes3d);
        }

        public static void SetParameter(EventInstance instance, string param, float value)
        {
            if (instance == null)
                return;

            instance.setParameterValue(param, value);
        }

        public static void Stop(EventInstance instance, bool allowFadeOut = true)
        {
            if (instance == null)
                return;

            instance.stop(allowFadeOut ? STOP_MODE.ALLOWFADEOUT : STOP_MODE.IMMEDIATE);
            instance.release();
        }

        public static EventInstance CreateInstance(string path, Vector2? position = null)
        {
            EventDescription eventDescription = GetEventDescription(path);
            if (eventDescription == null)
                return null;

            eventDescription.createInstance(out EventInstance instance1);
            eventDescription.is3D(out bool is3D);
            if (is3D && position.HasValue)
                Position(instance1, position.Value);

            return instance1;
        }

        public static EventDescription GetEventDescription(string path)
        {
            EventDescription _event = null;
            if (path != null && !cachedEventDescriptions.TryGetValue(path, out _event))
            {
                RESULT result = system.getEvent(path, out _event);
                switch (result)
                {
                    case RESULT.OK:
                        _event.loadSampleData();
                        cachedEventDescriptions.Add(path, _event);
                        break;
                    case RESULT.ERR_EVENT_NOTFOUND:
                        break;
                    default:
                        throw new Exception("FMOD getEvent failed: " + result);
                }
            }
            return _event;
        }

        public static void ReleaseUnusedDescriptions()
        {
            List<string> stringList = new();
            foreach (KeyValuePair<string, EventDescription> eventDescription in cachedEventDescriptions)
            {
                eventDescription.Value.getInstanceCount(out int count);
                if (count <= 0)
                {
                    eventDescription.Value.unloadSampleData();
                    stringList.Add(eventDescription.Key);
                }
            }
            foreach (string key in stringList)
                cachedEventDescriptions.Remove(key);
        }

        public static string GetEventName(EventInstance instance)
        {
            if (instance != null)
            {
                instance.getDescription(out EventDescription description1);
                if (description1 != null)
                {
                    description1.getPath(out string path1);
                    return path1;
                }
            }
            return "";
        }

        public static bool IsPlaying(EventInstance instance)
        {
            if (instance != null)
            {
                instance.getPlaybackState(out PLAYBACK_STATE state);
                if (state is PLAYBACK_STATE.PLAYING or PLAYBACK_STATE.STARTING)
                    return true;
            }
            return false;
        }

        public static bool BusPaused(string path, bool? pause = null)
        {
            bool paused1 = false;
            if (system != null && system.getBus(path, out Bus bus) == RESULT.OK)
            {
                if (pause.HasValue)
                    bus.setPaused(pause.Value);

                bus.getPaused(out paused1);
            }
            return paused1;
        }

        public static bool BusMuted(string path, bool? mute)
        {
            bool paused1 = false;
            if (system.getBus(path, out Bus bus) == RESULT.OK)
            {
                if (mute.HasValue)
                    bus.setMute(mute.Value);

                bus.getPaused(out paused1);
            }
            return paused1;
        }

        public static void BusStopAll(string path, bool immediate = false)
        {
            if (system == null || system.getBus(path, out Bus bus) != RESULT.OK)
                return;

            bus.stopAllEvents(immediate ? STOP_MODE.IMMEDIATE : STOP_MODE.ALLOWFADEOUT);
        }

        public static float VCAVolume(string path, float? volume = null)
        {
            RESULT vcaStatus = system.getVCA(path, out VCA vca1);
            float volume1 = 1f;
            if (vcaStatus == RESULT.OK)
            {
                if (volume.HasValue)
                    vca1.setVolume(volume.Value);

                vca1.getVolume(out volume1, out _);
            }
            return volume1;
        }

        public static EventInstance CreateSnapshot(string name, bool start = true)
        {
            system.getEvent(name, out EventDescription _event);
            EventInstance instance;
            if (_event != null)
                _event.createInstance(out instance);
            else
                throw new Exception("Snapshot " + name + " doesn't exist");

            if (start)
                instance.start();

            return instance;
        }

        public static void ResumeSnapshot(EventInstance snapshot)
        {
            if (snapshot != null)
                snapshot.start();
        }

        public static bool IsSnapshotRunning(EventInstance snapshot)
        {
            if (snapshot == null)
                return false;

            snapshot.getPlaybackState(out PLAYBACK_STATE state);
            return state is PLAYBACK_STATE.PLAYING or PLAYBACK_STATE.STARTING or PLAYBACK_STATE.SUSTAINING;
        }

        public static void EndSnapshot(EventInstance snapshot)
        {
            if (snapshot == null)
                return;

            snapshot.stop(STOP_MODE.ALLOWFADEOUT);
        }

        public static void ReleaseSnapshot(EventInstance snapshot)
        {
            if (snapshot == null)
                return;

            snapshot.stop(STOP_MODE.ALLOWFADEOUT);
            snapshot.release();
        }

        public static EventInstance CurrentMusicEventInstance => currentMusicEvent;
        public static EventInstance CurrentAmbienceEventInstance => currentAmbientEvent;

        public static bool SetMusic(string path, bool startPlaying = true, bool allowFadeOut = true)
        {
            if (string.IsNullOrEmpty(path) || path == "null")
            {
                Stop(currentMusicEvent, allowFadeOut);
                currentMusicEvent = null;
                CurrentMusic = "";
            }
            else if (!CurrentMusic.Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                Stop(currentMusicEvent, allowFadeOut);
                EventInstance instance = CreateInstance(path);
                if (instance != null & startPlaying)
                    instance.start();

                currentMusicEvent = instance;
                CurrentMusic = GetEventName(instance);
                return true;
            }
            return false;
        }

        public static bool SetAmbience(string path, bool startPlaying = true)
        {
            if (string.IsNullOrEmpty(path) || path == "null")
            {
                Stop(currentAmbientEvent);
                currentAmbientEvent = null;
            }
            else if (!GetEventName(currentAmbientEvent).Equals(path, StringComparison.OrdinalIgnoreCase))
            {
                Stop(currentAmbientEvent);
                EventInstance instance = CreateInstance(path);
                if (instance != null & startPlaying)
                    instance.start();
                currentAmbientEvent = instance;
                return true;
            }
            return false;
        }

        public static void SetMusicParam(string path, float value)
        {
            if (currentMusicEvent == null)
                return;

            currentMusicEvent.setParameterValue(path, value);
        }

        public static void SetAltMusic(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                EndSnapshot(mainDownSnapshot);
                Stop(currentAltMusicEvent);
                currentAltMusicEvent = null;
            }
            else
            {
                if (GetEventName(currentAltMusicEvent).Equals(path, StringComparison.OrdinalIgnoreCase))
                    return;

                StartMainDownSnapshot();
                Stop(currentAltMusicEvent);
                currentAltMusicEvent = Loop(path);
            }
        }

        private static void StartMainDownSnapshot()
        {
            if (mainDownSnapshot == null)
                mainDownSnapshot = CreateSnapshot("snapshot:/music_mains_mute");
            else
                ResumeSnapshot(mainDownSnapshot);
        }

        //private static void EndMainDownSnapshot() => EndSnapshot(mainDownSnapshot);

        public static float MusicVolume
        {
            get => VCAVolume("vca:/music");
            set => VCAVolume("vca:/music", new float?(value));
        }

        public static float SfxVolume
        {
            get => VCAVolume("vca:/gameplay_sfx");
            set
            {
                VCAVolume("vca:/gameplay_sfx", new float?(value));
                VCAVolume("vca:/ui_sfx", new float?(value));
            }
        }

        public static bool PauseMusic
        {
            get => BusPaused("bus:/music");
            set => BusPaused("bus:/music", new bool?(value));
        }

        public static bool PauseGameplaySfx
        {
            get => BusPaused("bus:/gameplay_sfx");
            set
            {
                BusPaused("bus:/gameplay_sfx", new bool?(value));
                BusPaused("bus:/music/stings", new bool?(value));
            }
        }

        public static bool PauseUISfx
        {
            get => BusPaused("bus:/ui_sfx");
            set => BusPaused("bus:/ui_sfx", new bool?(value));
        }

        public static bool MusicUnderwater
        {
            get => musicUnderwater;
            set
            {
                if (musicUnderwater == value)
                    return;

                musicUnderwater = value;
                if (musicUnderwater)
                {
                    if (musicUnderwaterSnapshot == null)
                        musicUnderwaterSnapshot = CreateSnapshot("snapshot:/underwater");
                    else
                        ResumeSnapshot(musicUnderwaterSnapshot);
                }
                else
                    EndSnapshot(musicUnderwaterSnapshot);
            }
        }

        public static class Banks
        {
            public static Bank Master;
            public static Bank Music;
            public static Bank Sfxs;
            public static Bank UI;
            public static Bank DlcMusic;
            public static Bank DlcSfxs;

            public static Bank Load(string name, bool loadStrings)
            {
                string str = Path.Combine(Engine.ContentDirectory, "FMOD", "Desktop", name);
                CheckFmod(system.loadBankFile(str + ".bank", LOAD_BANK_FLAGS.NORMAL, out Bank bank));
                bank.loadSampleData();
                if (loadStrings)
                    CheckFmod(system.loadBankFile(str + ".strings.bank", LOAD_BANK_FLAGS.NORMAL, out Bank _));

                return bank;
            }
        }
    }
}
