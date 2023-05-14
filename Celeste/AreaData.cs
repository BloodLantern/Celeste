// Decompiled with JetBrains decompiler
// Type: Celeste.AreaData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Celeste
{
    public class AreaData
    {
        public static List<AreaData> Areas;
        public string Name;
        public string Icon;
        public int ID;
        public bool Interlude;
        public bool CanFullClear;
        public bool IsFinal;
        public string CompleteScreenName;
        public ModeProperties[] Mode;
        public int CassetteCheckpointIndex = -1;
        public Color TitleBaseColor = Color.White;
        public Color TitleAccentColor = Color.Gray;
        public Color TitleTextColor = Color.White;
        public Player.IntroTypes IntroType;
        public bool Dreaming;
        public string ColorGrade;
        public Action<Scene, bool, Action> Wipe;
        public float DarknessAlpha = 0.05f;
        public float BloomBase;
        public float BloomStrength = 1f;
        public Action<Level> OnLevelBegin;
        public string Jumpthru = "wood";
        public string Spike = "default";
        public string CrumbleBlock = "default";
        public string WoodPlatform = "default";
        public Color CassseteNoteColor = Color.White;
        public Color[] CobwebColor = new Color[1]
        {
            Calc.HexToColor("696a6a")
        };
        public string CassetteSong = "event:/music/cassette/01_forsaken_city";
        public Session.CoreModes CoreMode;
        public int MountainState;
        public MountainCamera MountainIdle;
        public MountainCamera MountainSelect;
        public MountainCamera MountainZoom;
        public Vector3 MountainCursor;
        public float MountainCursorScale;

        public static ModeProperties GetMode(AreaKey area)
        {
            return GetMode(area.ID, area.Mode);
        }

        public static ModeProperties GetMode(int id, AreaMode mode = AreaMode.Normal)
        {
            return Areas[id].Mode[(int)mode];
        }

        public static void Load()
        {
            Areas = new List<AreaData>();
            AreaData prologue = new()
            {
                Name = "area_0",
                Icon = "areas/intro",
                Interlude = true,
                CompleteScreenName = null,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = null,
                    Path = "0-Intro",
                    Checkpoints = null,
                    Inventory = PlayerInventory.Prologue,
                    AudioState = new AudioState("event:/music/lvl0/intro", "event:/env/amb/00_prologue")
                },
                null,
                null
            },
                TitleBaseColor = Calc.HexToColor("383838"),
                TitleAccentColor = Calc.HexToColor("50AFAE"),
                TitleTextColor = Color.White,
                IntroType = Player.IntroTypes.WalkInRight,
                Dreaming = false,
                ColorGrade = null
            };
            CurtainWipe curtainWipe1;
            prologue.Wipe = (scene, wipeIn, onComplete) => curtainWipe1 = new CurtainWipe(scene, wipeIn, onComplete);
            prologue.DarknessAlpha = 0.05f;
            prologue.BloomBase = 0.0f;
            prologue.BloomStrength = 1f;
            prologue.OnLevelBegin = null;
            prologue.Jumpthru = "wood";
            Areas.Add(prologue);

            AreaData forsakenCity = new()
            {
                Name = "area_1",
                Icon = "areas/city",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "ForsakenCity",
                CassetteCheckpointIndex = 2,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = "fc",
                    Path = "1-ForsakenCity",
                    Checkpoints = new CheckpointData[2]
                    {
                        new CheckpointData("6", "checkpoint_1_0"),
                        new CheckpointData("9b", "checkpoint_1_1")
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/lvl1/main", "event:/env/amb/01_main")
                },
                new ModeProperties()
                {
                    PoemID = "fcr",
                    Path = "1H-ForsakenCity",
                    Checkpoints = new CheckpointData[2]
                    {
                        new CheckpointData("04", "checkpoint_1h_0"),
                        new CheckpointData("08", "checkpoint_1h_1")
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/01_forsaken_city", "event:/env/amb/01_main")
                },
                new ModeProperties()
                {
                    Path = "1X-ForsakenCity",
                    Checkpoints = null,
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/01_forsaken_city", "event:/env/amb/01_main")
                }
            },
                TitleBaseColor = Calc.HexToColor("6c7c81"),
                TitleAccentColor = Calc.HexToColor("2f344b"),
                TitleTextColor = Color.White,
                IntroType = Player.IntroTypes.Jump,
                Dreaming = false,
                ColorGrade = null
            };
            AngledWipe angledWipe;
            forsakenCity.Wipe = (scene, wipeIn, onComplete) => angledWipe = new AngledWipe(scene, wipeIn, onComplete);
            forsakenCity.DarknessAlpha = 0.05f;
            forsakenCity.BloomBase = 0f;
            forsakenCity.BloomStrength = 1f;
            forsakenCity.OnLevelBegin = null;
            forsakenCity.Jumpthru = "wood";
            forsakenCity.CassseteNoteColor = Calc.HexToColor("33a9ee");
            forsakenCity.CassetteSong = "event:/music/cassette/01_forsaken_city";
            Areas.Add(forsakenCity);

            AreaData oldSite = new()
            {
                Name = "area_2",
                Icon = "areas/oldsite",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "OldSite",
                CassetteCheckpointIndex = 0,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = "os",
                    Path = "2-OldSite",
                    Checkpoints = new CheckpointData[2]
                    {
                        new CheckpointData("3", "checkpoint_2_0", new PlayerInventory?(PlayerInventory.Default), true),
                        new CheckpointData("end_3", "checkpoint_2_1")
                    },
                    Inventory = PlayerInventory.OldSite,
                    AudioState = new AudioState("event:/music/lvl2/beginning", "event:/env/amb/02_dream")
                },
                new ModeProperties()
                {
                    PoemID = "osr",
                    Path = "2H-OldSite",
                    Checkpoints = new CheckpointData[2]
                    {
                        new CheckpointData("03", "checkpoint_2h_0", dreaming: true),
                        new CheckpointData("08b", "checkpoint_2h_1", dreaming: true)
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/02_old_site", "event:/env/amb/02_dream")
                },
                new ModeProperties()
                {
                    Path = "2X-OldSite",
                    Checkpoints = null,
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/02_old_site", "event:/env/amb/02_dream")
                }
            },
                TitleBaseColor = Calc.HexToColor("247F35"),
                TitleAccentColor = Calc.HexToColor("E4EF69"),
                TitleTextColor = Color.White,
                IntroType = Player.IntroTypes.WakeUp,
                Dreaming = true,
                ColorGrade = "oldsite"
            };
            DreamWipe dreamWipe;
            oldSite.Wipe = (scene, wipeIn, onComplete) => dreamWipe = new DreamWipe(scene, wipeIn, onComplete);
            oldSite.DarknessAlpha = 0.15f;
            oldSite.BloomBase = 0.5f;
            oldSite.BloomStrength = 1f;
            oldSite.OnLevelBegin = level =>
            {
                if (level.Session.Area.Mode != AreaMode.Normal)
                {
                    return;
                }

                level.Add(new OldSiteChaseMusicHandler());
            };
            oldSite.Jumpthru = "wood";
            oldSite.CassseteNoteColor = Calc.HexToColor("33eea2");
            oldSite.CassetteSong = "event:/music/cassette/02_old_site";
            Areas.Add(oldSite);

            AreaData celestialResort = new()
            {
                Name = "area_3",
                Icon = "areas/resort",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "CelestialResort",
                CassetteCheckpointIndex = 2
            };
            AreaData areaData8 = celestialResort;
            ModeProperties[] modePropertiesArray1 = new ModeProperties[3];
            ModeProperties modeProperties1 = new()
            {
                PoemID = "cr",
                Path = "3-CelestialResort",
                Checkpoints = new CheckpointData[3]
            {
                new CheckpointData("08-a", "checkpoint_3_0")
                {
                    AudioState = new AudioState(new AudioTrackState("event:/music/lvl3/explore").SetProgress(3), new AudioTrackState("event:/env/amb/03_interior"))
                },
                new CheckpointData("09-d", "checkpoint_3_1")
                {
                    AudioState = new AudioState(new AudioTrackState("event:/music/lvl3/clean").SetProgress(3), new AudioTrackState("event:/env/amb/03_interior"))
                },
                new CheckpointData("00-d", "checkpoint_3_2")
            },
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/lvl3/intro", "event:/env/amb/03_exterior")
            };
            modePropertiesArray1[0] = modeProperties1;
            modePropertiesArray1[1] = new ModeProperties()
            {
                PoemID = "crr",
                Path = "3H-CelestialResort",
                Checkpoints = new CheckpointData[3]
                {
                    new CheckpointData("06", "checkpoint_3h_0"),
                    new CheckpointData("11", "checkpoint_3h_1"),
                    new CheckpointData("16", "checkpoint_3h_2")
                },
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/remix/03_resort", "event:/env/amb/03_exterior")
            };
            modePropertiesArray1[2] = new ModeProperties()
            {
                Path = "3X-CelestialResort",
                Checkpoints = null,
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/remix/03_resort", "event:/env/amb/03_exterior")
            };
            areaData8.Mode = modePropertiesArray1;
            celestialResort.TitleBaseColor = Calc.HexToColor("b93c27");
            celestialResort.TitleAccentColor = Calc.HexToColor("ffdd42");
            celestialResort.TitleTextColor = Color.White;
            celestialResort.IntroType = Player.IntroTypes.WalkInRight;
            celestialResort.Dreaming = false;
            celestialResort.ColorGrade = null;
            KeyDoorWipe keyDoorWipe;
            celestialResort.Wipe = (scene, wipeIn, onComplete) => keyDoorWipe = new KeyDoorWipe(scene, wipeIn, onComplete);
            celestialResort.DarknessAlpha = 0.15f;
            celestialResort.BloomBase = 0.0f;
            celestialResort.BloomStrength = 1f;
            celestialResort.OnLevelBegin = null;
            celestialResort.Jumpthru = "wood";
            celestialResort.CassseteNoteColor = Calc.HexToColor("eed933");
            celestialResort.CassetteSong = "event:/music/cassette/03_resort";
            Areas.Add(celestialResort);

            AreaData cliffSide = new()
            {
                Name = "area_4",
                Icon = "areas/cliffside",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "Cliffside",
                CassetteCheckpointIndex = 0,
                TitleBaseColor = Calc.HexToColor("FF7F83"),
                TitleAccentColor = Calc.HexToColor("6D54B7"),
                TitleTextColor = Color.White,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = "cs",
                    Path = "4-GoldenRidge",
                    Checkpoints = new CheckpointData[3]
                    {
                        new CheckpointData("b-00", "checkpoint_4_0"),
                        new CheckpointData("c-00", "checkpoint_4_1"),
                        new CheckpointData("d-00", "checkpoint_4_2")
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/lvl4/main", "event:/env/amb/04_main")
                },
                new ModeProperties()
                {
                    PoemID = "csr",
                    Path = "4H-GoldenRidge",
                    Checkpoints = new CheckpointData[3]
                    {
                        new CheckpointData("b-00", "checkpoint_4h_0"),
                        new CheckpointData("c-00", "checkpoint_4h_1"),
                        new CheckpointData("d-00", "checkpoint_4h_2")
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/04_cliffside", "event:/env/amb/04_main")
                },
                new ModeProperties()
                {
                    Path = "4X-GoldenRidge",
                    Checkpoints = null,
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/04_cliffside", "event:/env/amb/04_main")
                }
            },
                IntroType = Player.IntroTypes.WalkInRight,
                Dreaming = false,
                ColorGrade = null
            };
            WindWipe windWipe;
            cliffSide.Wipe = (scene, wipeIn, onComplete) => windWipe = new WindWipe(scene, wipeIn, onComplete);
            cliffSide.DarknessAlpha = 0.1f;
            cliffSide.BloomBase = 0.25f;
            cliffSide.BloomStrength = 1f;
            cliffSide.OnLevelBegin = null;
            cliffSide.Jumpthru = "cliffside";
            cliffSide.Spike = "cliffside";
            cliffSide.CrumbleBlock = "cliffside";
            cliffSide.WoodPlatform = "cliffside";
            cliffSide.CassseteNoteColor = Calc.HexToColor("eb4bd9");
            cliffSide.CassetteSong = "event:/music/cassette/04_cliffside";
            Areas.Add(cliffSide);

            AreaData mirrorTemple = new()
            {
                Name = "area_5",
                Icon = "areas/temple",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "MirrorTemple",
                CassetteCheckpointIndex = 1,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = "t",
                    Path = "5-MirrorTemple",
                    Checkpoints = new CheckpointData[4]
                    {
                        new CheckpointData("b-00", "checkpoint_5_0"),
                        new CheckpointData("c-00", "checkpoint_5_1", dreaming: true, audioState: new AudioState("event:/music/lvl5/middle_temple", "event:/env/amb/05_interior_dark")),
                        new CheckpointData("d-00", "checkpoint_5_2", dreaming: true, audioState: new AudioState("event:/music/lvl5/middle_temple", "event:/env/amb/05_interior_dark")),
                        new CheckpointData("e-00", "checkpoint_5_3", dreaming: true, audioState: new AudioState("event:/music/lvl5/mirror", "event:/env/amb/05_interior_dark"))
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/lvl5/normal", "event:/env/amb/05_interior_main")
                },
                new ModeProperties()
                {
                    PoemID = "tr",
                    Path = "5H-MirrorTemple",
                    Checkpoints = new CheckpointData[3]
                    {
                        new CheckpointData("b-00", "checkpoint_5h_0"),
                        new CheckpointData("c-00", "checkpoint_5h_1"),
                        new CheckpointData("d-00", "checkpoint_5h_2")
                    },
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/05_mirror_temple", "event:/env/amb/05_interior_main")
                },
                new ModeProperties()
                {
                    Path = "5X-MirrorTemple",
                    Checkpoints = null,
                    Inventory = PlayerInventory.Default,
                    AudioState = new AudioState("event:/music/remix/05_mirror_temple", "event:/env/amb/05_interior_main")
                }
            },
                TitleBaseColor = Calc.HexToColor("8314bc"),
                TitleAccentColor = Calc.HexToColor("df72f9"),
                TitleTextColor = Color.White,
                IntroType = Player.IntroTypes.WakeUp,
                Dreaming = false,
                ColorGrade = null
            };
            DropWipe dropWipe;
            mirrorTemple.Wipe = (scene, wipeIn, onComplete) => dropWipe = new DropWipe(scene, wipeIn, onComplete);
            mirrorTemple.DarknessAlpha = 0.15f;
            mirrorTemple.BloomBase = 0.0f;
            mirrorTemple.BloomStrength = 1f;
            mirrorTemple.OnLevelBegin = level =>
            {
                level.Add(new SeekerEffectsController());
                if (level.Session.Area.Mode != AreaMode.Normal)
                {
                    return;
                }

                level.Add(new TempleEndingMusicHandler());
            };
            mirrorTemple.Jumpthru = "temple";
            mirrorTemple.CassseteNoteColor = Calc.HexToColor("5a56e6");
            mirrorTemple.CobwebColor = new Color[1]
            {
                Calc.HexToColor("9f2166")
            };
            mirrorTemple.CassetteSong = "event:/music/cassette/05_mirror_temple";
            Areas.Add(mirrorTemple);

            AreaData reflection = new()
            {
                Name = "area_6",
                Icon = "areas/reflection",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "Fall",
                CassetteCheckpointIndex = 2
            };
            AreaData areaData15 = reflection;
            ModeProperties[] modePropertiesArray2 = new ModeProperties[3];
            ModeProperties modeProperties2 = new()
            {
                PoemID = "tf",
                Path = "6-Reflection",
                Checkpoints = new CheckpointData[5]
            {
                new CheckpointData("00", "checkpoint_6_0"),
                new CheckpointData("04", "checkpoint_6_1"),
                new CheckpointData("b-00", "checkpoint_6_2"),
                new CheckpointData("boss-00", "checkpoint_6_3"),
                new CheckpointData("after-00", "checkpoint_6_4", new PlayerInventory?(PlayerInventory.CH6End))
                {
                    Flags = new HashSet<string>()
                    {
                        "badeline_connection"
                    },
                    AudioState = new AudioState(new AudioTrackState("event:/music/lvl6/badeline_acoustic").Param("levelup", 2f), new AudioTrackState("event:/env/amb/06_main"))
                }
            },
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/lvl6/main", "event:/env/amb/06_main")
            };
            modePropertiesArray2[0] = modeProperties2;
            modePropertiesArray2[1] = new ModeProperties()
            {
                PoemID = "tfr",
                Path = "6H-Reflection",
                Checkpoints = new CheckpointData[3]
                {
                    new CheckpointData("b-00", "checkpoint_6h_0"),
                    new CheckpointData("c-00", "checkpoint_6h_1"),
                    new CheckpointData("d-00", "checkpoint_6h_2")
                },
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/remix/06_reflection", "event:/env/amb/06_main")
            };
            modePropertiesArray2[2] = new ModeProperties()
            {
                Path = "6X-Reflection",
                Checkpoints = null,
                Inventory = PlayerInventory.Default,
                AudioState = new AudioState("event:/music/remix/06_reflection", "event:/env/amb/06_main")
            };
            areaData15.Mode = modePropertiesArray2;
            reflection.TitleBaseColor = Calc.HexToColor("359FE0");
            reflection.TitleAccentColor = Calc.HexToColor("3C5CBC");
            reflection.TitleTextColor = Color.White;
            reflection.IntroType = Player.IntroTypes.None;
            reflection.Dreaming = false;
            reflection.ColorGrade = "reflection";
            FallWipe fallWipe;
            reflection.Wipe = (scene, wipeIn, onComplete) => fallWipe = new FallWipe(scene, wipeIn, onComplete);
            reflection.DarknessAlpha = 0.2f;
            reflection.BloomBase = 0.2f;
            reflection.BloomStrength = 1f;
            reflection.OnLevelBegin = null;
            reflection.Jumpthru = "reflection";
            reflection.Spike = "reflection";
            reflection.CassseteNoteColor = Calc.HexToColor("56e6dd");
            reflection.CassetteSong = "event:/music/cassette/06_reflection";
            Areas.Add(reflection);

            AreaData summit = new()
            {
                Name = "area_7",
                Icon = "areas/summit",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "Summit",
                CassetteCheckpointIndex = 3,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = "ts",
                    Path = "7-Summit",
                    Checkpoints = new CheckpointData[6]
                    {
                        new CheckpointData("b-00", "checkpoint_7_0", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(1),  null)),
                        new CheckpointData("c-00", "checkpoint_7_1", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(2),  null)),
                        new CheckpointData("d-00", "checkpoint_7_2", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(3),  null)),
                        new CheckpointData("e-00b", "checkpoint_7_3", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(4),  null)),
                        new CheckpointData("f-00", "checkpoint_7_4", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(5),  null)),
                        new CheckpointData("g-00", "checkpoint_7_5", audioState: new AudioState("event:/music/lvl7/final_ascent",  null))
                    },
                    Inventory = PlayerInventory.TheSummit,
                    AudioState = new AudioState("event:/music/lvl7/main", null)
                },
                new ModeProperties()
                {
                    PoemID = "tsr",
                    Path = "7H-Summit",
                    Checkpoints = new CheckpointData[6]
                    {
                        new CheckpointData("b-00", "checkpoint_7H_0"),
                        new CheckpointData("c-01", "checkpoint_7H_1"),
                        new CheckpointData("d-00", "checkpoint_7H_2"),
                        new CheckpointData("e-00", "checkpoint_7H_3"),
                        new CheckpointData("f-00", "checkpoint_7H_4"),
                        new CheckpointData("g-00", "checkpoint_7H_5")
                    },
                    Inventory = PlayerInventory.TheSummit,
                    AudioState = new AudioState("event:/music/remix/07_summit", null)
                },
                new ModeProperties()
                {
                    Path = "7X-Summit",
                    Checkpoints = null,
                    Inventory = PlayerInventory.TheSummit,
                    AudioState = new AudioState("event:/music/remix/07_summit", null)
                }
            },
                TitleBaseColor = Calc.HexToColor("FFD819"),
                TitleAccentColor = Calc.HexToColor("197DB7"),
                TitleTextColor = Color.Black,
                IntroType = Player.IntroTypes.None,
                Dreaming = false,
                ColorGrade = null
            };
            MountainWipe mountainWipe;
            summit.Wipe = (scene, wipeIn, onComplete) => mountainWipe = new MountainWipe(scene, wipeIn, onComplete);
            summit.DarknessAlpha = 0.05f;
            summit.BloomBase = 0.2f;
            summit.BloomStrength = 1f;
            summit.OnLevelBegin = null;
            summit.Jumpthru = "temple";
            summit.Spike = "outline";
            summit.CassseteNoteColor = Calc.HexToColor("e69156");
            summit.CassetteSong = "event:/music/cassette/07_summit";
            Areas.Add(summit);

            AreaData epilogue = new()
            {
                Name = "area_8",
                Icon = "areas/intro",
                Interlude = true,
                CompleteScreenName = null,
                CassetteCheckpointIndex = 1,
                Mode = new ModeProperties[3]
            {
                new ModeProperties()
                {
                    PoemID = null,
                    Path = "8-Epilogue",
                    Checkpoints = null,
                    Inventory = PlayerInventory.TheSummit,
                    AudioState = new AudioState("event:/music/lvl8/main", "event:/env/amb/00_prologue")
                },
                null,
                null
            },
                TitleBaseColor = Calc.HexToColor("383838"),
                TitleAccentColor = Calc.HexToColor("50AFAE"),
                TitleTextColor = Color.White,
                IntroType = Player.IntroTypes.WalkInLeft,
                Dreaming = false,
                ColorGrade = null
            };
            CurtainWipe curtainWipe2;
            epilogue.Wipe = (scene, wipeIn, onComplete) => curtainWipe2 = new CurtainWipe(scene, wipeIn, onComplete);
            epilogue.DarknessAlpha = 0.05f;
            epilogue.BloomBase = 0.0f;
            epilogue.BloomStrength = 1f;
            epilogue.OnLevelBegin = null;
            epilogue.Jumpthru = "wood";
            Areas.Add(epilogue);

            AreaData core = new()
            {
                Name = "area_9",
                Icon = "areas/core",
                Interlude = false,
                CanFullClear = true,
                CompleteScreenName = "Core",
                CassetteCheckpointIndex = 3
            };
            AreaData areaData22 = core;
            ModeProperties[] modePropertiesArray3 = new ModeProperties[3];
            ModeProperties modeProperties3 = new()
            {
                PoemID = "mc",
                Path = "9-Core",
                Checkpoints = new CheckpointData[3]
            {
                new CheckpointData("a-00", "checkpoint_8_0"),
                new CheckpointData("c-00", "checkpoint_8_1")
                {
                    CoreMode = new Session.CoreModes?(Session.CoreModes.Cold)
                },
                new CheckpointData("d-00", "checkpoint_8_2")
            },
                Inventory = PlayerInventory.Core,
                AudioState = new AudioState("event:/music/lvl9/main", "event:/env/amb/09_main"),
                IgnoreLevelAudioLayerData = true
            };
            modePropertiesArray3[0] = modeProperties3;
            modePropertiesArray3[1] = new ModeProperties()
            {
                PoemID = "mcr",
                Path = "9H-Core",
                Checkpoints = new CheckpointData[3]
                {
                    new CheckpointData("a-00", "checkpoint_8H_0"),
                    new CheckpointData("b-00", "checkpoint_8H_1"),
                    new CheckpointData("c-01", "checkpoint_8H_2")
                },
                Inventory = PlayerInventory.Core,
                AudioState = new AudioState("event:/music/remix/09_core", "event:/env/amb/09_main")
            };
            modePropertiesArray3[2] = new ModeProperties()
            {
                Path = "9X-Core",
                Checkpoints = null,
                Inventory = PlayerInventory.Core,
                AudioState = new AudioState("event:/music/remix/09_core", "event:/env/amb/09_main")
            };
            areaData22.Mode = modePropertiesArray3;
            core.TitleBaseColor = Calc.HexToColor("761008");
            core.TitleAccentColor = Calc.HexToColor("E0201D");
            core.TitleTextColor = Color.White;
            core.IntroType = Player.IntroTypes.WalkInRight;
            core.Dreaming = false;
            core.ColorGrade = null;
            HeartWipe heartWipe;
            core.Wipe = (scene, wipeIn, onComplete) => heartWipe = new HeartWipe(scene, wipeIn, onComplete);
            core.DarknessAlpha = 0.05f;
            core.BloomBase = 0.0f;
            core.BloomStrength = 1f;
            core.OnLevelBegin = null;
            core.Jumpthru = "core";
            core.CassseteNoteColor = Calc.HexToColor("e6566a");
            core.CassetteSong = "event:/music/cassette/09_core";
            core.CoreMode = Session.CoreModes.Hot;
            Areas.Add(core);

            AreaData farewell = new()
            {
                Name = "area_10",
                Icon = "areas/farewell",
                Interlude = false,
                CanFullClear = false,
                IsFinal = true,
                CompleteScreenName = "Core",
                CassetteCheckpointIndex = -1
            };
            AreaData areaData25 = farewell;
            ModeProperties[] modePropertiesArray4 = new ModeProperties[1];
            ModeProperties modeProperties4 = new()
            {
                PoemID = "fw",
                Path = "LostLevels",
                Checkpoints = new CheckpointData[8]
            {
                new CheckpointData("a-00", "checkpoint_9_0", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1),  null)),
                new CheckpointData("c-00", "checkpoint_9_1", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1),  null)),
                new CheckpointData("e-00z", "checkpoint_9_2", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part02"),  null)),
                new CheckpointData("f-door", "checkpoint_9_3", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/intermission_heartgroove"),  null)),
                new CheckpointData("h-00b", "checkpoint_9_4", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part03"),  null)),
                new CheckpointData("i-00", "checkpoint_9_5", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/cassette_rooms").Param("sixteenth_note", 7f),  null))
                {
                    ColorGrade = "feelingdown"
                },
                new CheckpointData("j-00", "checkpoint_9_6", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/cassette_rooms").Param("sixteenth_note", 7f).SetProgress(3),  null))
                {
                    ColorGrade = "feelingdown"
                },
                new CheckpointData("j-16", "checkpoint_9_7", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/final_run").SetProgress(3),  null))
            },
                Inventory = PlayerInventory.Farewell,
                AudioState = new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1), new AudioTrackState("event:/env/amb/00_prologue"))
            };
            modePropertiesArray4[0] = modeProperties4;
            areaData25.Mode = modePropertiesArray4;
            farewell.TitleBaseColor = Calc.HexToColor("240d7c");
            farewell.TitleAccentColor = Calc.HexToColor("FF6AA9");
            farewell.TitleTextColor = Color.White;
            farewell.IntroType = Player.IntroTypes.ThinkForABit;
            farewell.Dreaming = false;
            farewell.ColorGrade = null;
            StarfieldWipe starfieldWipe;
            farewell.Wipe = (scene, wipeIn, onComplete) => starfieldWipe = new StarfieldWipe(scene, wipeIn, onComplete);
            farewell.DarknessAlpha = 0.05f;
            farewell.BloomBase = 0.5f;
            farewell.BloomStrength = 1f;
            farewell.OnLevelBegin = null;
            farewell.Jumpthru = "wood";
            farewell.CassseteNoteColor = Calc.HexToColor("e6566a");
            farewell.CassetteSong = null;
            farewell.CobwebColor = new Color[3]
            {
                Calc.HexToColor("42c192"),
                Calc.HexToColor("af36a8"),
                Calc.HexToColor("3474a6")
            };
            Areas.Add(farewell);

            int length = Enum.GetNames(typeof(AreaMode)).Length;
            for (int index = 0; index < Areas.Count; ++index)
            {
                Areas[index].ID = index;
                Areas[index].Mode[0].MapData = new MapData(new AreaKey(index));
                if (!Areas[index].Interlude)
                {
                    for (AreaMode mode = AreaMode.Normal; (int)mode < length; ++mode)
                    {
                        if (Areas[index].HasMode(mode))
                        {
                            Areas[index].Mode[(int)mode].MapData = new MapData(new AreaKey(index, mode));
                        }
                    }
                }
            }
            ReloadMountainViews();
        }

        public static void ReloadMountainViews()
        {
            foreach (XmlElement xml in (XmlNode)Calc.LoadXML(Path.Combine(Engine.ContentDirectory, "Overworld", "AreaViews.xml"))["Views"])
            {
                int index = xml.AttrInt("id");
                if (index >= 0 && index < Areas.Count)
                {
                    Vector3 pos1 = xml["Idle"].AttrVector3("position");
                    Vector3 target1 = xml["Idle"].AttrVector3("target");
                    Areas[index].MountainIdle = new MountainCamera(pos1, target1);
                    Vector3 pos2 = xml["Select"].AttrVector3("position");
                    Vector3 target2 = xml["Select"].AttrVector3("target");
                    Areas[index].MountainSelect = new MountainCamera(pos2, target2);
                    Vector3 pos3 = xml["Zoom"].AttrVector3("position");
                    Vector3 target3 = xml["Zoom"].AttrVector3("target");
                    Areas[index].MountainZoom = new MountainCamera(pos3, target3);
                    if (xml["Cursor"] != null)
                    {
                        Areas[index].MountainCursor = xml["Cursor"].AttrVector3("position");
                    }

                    Areas[index].MountainState = xml.AttrInt("state", 0);
                }
            }
        }

        public static bool IsPoemRemix(string id)
        {
            foreach (AreaData area in Areas)
            {
                if (area.Mode.Length > 1
                    && area.Mode[(int)AreaMode.Normal] != null
                    && !string.IsNullOrEmpty(area.Mode[(int)AreaMode.Normal].PoemID)
                    && area.Mode[(int)AreaMode.Normal].PoemID.Equals(id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static int GetCheckpointID(AreaKey area, string level)
        {
            CheckpointData[] checkpoints = Areas[area.ID].Mode[(int)area.Mode].Checkpoints;
            if (checkpoints != null)
            {
                for (int checkpointId = 0; checkpointId < checkpoints.Length; ++checkpointId)
                {
                    if (checkpoints[checkpointId].Level.Equals(level))
                    {
                        return checkpointId;
                    }
                }
            }

            return -1;
        }

        public static CheckpointData GetCheckpoint(AreaKey area, string level)
        {
            CheckpointData[] checkpoints = Areas[area.ID].Mode[(int)area.Mode].Checkpoints;
            if (level != null && checkpoints != null)
            {
                foreach (CheckpointData checkpoint in checkpoints)
                {
                    if (checkpoint.Level.Equals(level))
                    {
                        return checkpoint;
                    }
                }
            }

            return null;
        }

        public static string GetCheckpointName(AreaKey area, string level)
        {
            if (string.IsNullOrEmpty(level))
            {
                return "START";
            }

            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null ? Dialog.Clean(checkpoint.Name) : null;
        }

        public static PlayerInventory GetCheckpointInventory(AreaKey area, string level)
        {
            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null && checkpoint.Inventory.HasValue ? checkpoint.Inventory.Value : Areas[area.ID].Mode[(int)area.Mode].Inventory;
        }

        public static bool GetCheckpointDreaming(AreaKey area, string level)
        {
            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null ? checkpoint.Dreaming : Areas[area.ID].Dreaming;
        }

        public static Session.CoreModes GetCheckpointCoreMode(AreaKey area, string level)
        {
            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null && checkpoint.CoreMode.HasValue ? checkpoint.CoreMode.Value : Areas[area.ID].CoreMode;
        }

        public static AudioState GetCheckpointAudioState(AreaKey area, string level)
        {
            return GetCheckpoint(area, level)?.AudioState;
        }

        public static string GetCheckpointColorGrading(AreaKey area, string level)
        {
            return GetCheckpoint(area, level)?.ColorGrade;
        }

        public static void Unload()
        {
            Areas = null;
        }

        public static AreaData Get(Scene scene)
        {
            return scene is not null and Level ? Areas[(scene as Level).Session.Area.ID] : null;
        }

        public static AreaData Get(Session session)
        {
            return session != null ? Areas[session.Area.ID] : null;
        }

        public static AreaData Get(AreaKey area)
        {
            return Areas[area.ID];
        }

        public static AreaData Get(int id)
        {
            return Areas[id];
        }

        public XmlElement CompleteScreenXml => GFX.CompleteScreensXml["Screens"][CompleteScreenName];

        public void DoScreenWipe(Scene scene, bool wipeIn, Action onComplete = null)
        {
            if (Wipe == null)
            {
                _ = new WindWipe(scene, wipeIn, onComplete);
            }
            else
            {
                Wipe(scene, wipeIn, onComplete);
            }
        }

        public bool HasMode(AreaMode mode)
        {
            return (AreaMode)Mode.Length > mode && Mode[(int)mode] != null && Mode[(int)mode].Path != null;
        }
    }
}
