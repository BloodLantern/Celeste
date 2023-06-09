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
        public Color CassseteNoteColor = Color.White; // Yes, this is Casssete and not Cassette
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

        public static ModeProperties GetMode(AreaKey area) => GetMode(area.ID, area.Mode);

        public static ModeProperties GetMode(int id, AreaMode mode = AreaMode.Normal) => Areas[id].Mode[(int) mode];

        public static void Load()
        {
            Areas = new List<AreaData>
            {
                new() // Prologue (Chapter 0)
                {
                    Name = "area_0",
                    Icon = "areas/intro",
                    Interlude = true,
                    CompleteScreenName = null,
                    Mode = new ModeProperties[3]
                    {
                        new()
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
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new CurtainWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "wood"
                },

                new() // Forsaken City (Chapter 1)
                {
                    Name = "area_1",
                    Icon = "areas/city",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "ForsakenCity",
                    CassetteCheckpointIndex = 2,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "fc",
                            Path = "1-ForsakenCity",
                            Checkpoints = new CheckpointData[2]
                            {
                                new("6", "checkpoint_1_0"),
                                new("9b", "checkpoint_1_1")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/lvl1/main", "event:/env/amb/01_main")
                        },
                        new()
                        {
                            PoemID = "fcr",
                            Path = "1H-ForsakenCity",
                            Checkpoints = new CheckpointData[2]
                            {
                                new("04", "checkpoint_1h_0"),
                                new("08", "checkpoint_1h_1")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/01_forsaken_city", "event:/env/amb/01_main")
                        },
                        new()
                        {
                            Path = "1X-ForsakenCity",
                            Checkpoints =  null,
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/01_forsaken_city", "event:/env/amb/01_main")
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("6c7c81"),
                    TitleAccentColor = Calc.HexToColor("2f344b"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.Jump,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new AngledWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "wood",
                    CassseteNoteColor = Calc.HexToColor("33a9ee"),
                    CassetteSong = "event:/music/cassette/01_forsaken_city"
                },

                new() // Old Site (Chapter 2)
                {
                    Name = "area_2",
                    Icon = "areas/oldsite",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "OldSite",
                    CassetteCheckpointIndex = 0,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "os",
                            Path = "2-OldSite",
                            Checkpoints = new CheckpointData[2]
                            {
                                new("3", "checkpoint_2_0", new PlayerInventory?(PlayerInventory.Default), true),
                                new("end_3", "checkpoint_2_1")
                            },
                            Inventory = PlayerInventory.OldSite,
                            AudioState = new AudioState("event:/music/lvl2/beginning", "event:/env/amb/02_dream")
                        },
                        new()
                        {
                            PoemID = "osr",
                            Path = "2H-OldSite",
                            Checkpoints = new CheckpointData[2]
                            {
                                new("03", "checkpoint_2h_0", dreaming: true),
                                new("08b", "checkpoint_2h_1", dreaming: true)
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/02_old_site", "event:/env/amb/02_dream")
                        },
                        new()
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
                    ColorGrade = "oldsite",
                    Wipe = (scene, wipeIn, onComplete) => new DreamWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.15f,
                    BloomBase = 0.5f,
                    BloomStrength = 1f,
                    OnLevelBegin = (level =>
                    {
                        if (level.Session.Area.Mode != AreaMode.Normal)
                            return;
                        level.Add(new OldSiteChaseMusicHandler());
                    }),
                    Jumpthru = "wood",
                    CassseteNoteColor = Calc.HexToColor("33eea2"),
                    CassetteSong = "event:/music/cassette/02_old_site"
                },

                new() // Celestial Resort (Chapter 3)
                {
                    Name = "area_3",
                    Icon = "areas/resort",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "CelestialResort",
                    CassetteCheckpointIndex = 2,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "cr",
                            Path = "3-CelestialResort",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("08-a", "checkpoint_3_0")
                                {
                                    AudioState = new AudioState(new AudioTrackState("event:/music/lvl3/explore").SetProgress(3), new AudioTrackState("event:/env/amb/03_interior"))
                                },
                                new("09-d", "checkpoint_3_1")
                                {
                                    AudioState = new AudioState(new AudioTrackState("event:/music/lvl3/clean").SetProgress(3), new AudioTrackState("event:/env/amb/03_interior"))
                                },
                                new("00-d", "checkpoint_3_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/lvl3/intro", "event:/env/amb/03_exterior")
                        },
                        new()
                        {
                            PoemID = "crr",
                            Path = "3H-CelestialResort",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("06", "checkpoint_3h_0"),
                                new("11", "checkpoint_3h_1"),
                                new("16", "checkpoint_3h_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/03_resort", "event:/env/amb/03_exterior")
                        },
                        new()
                        {
                            Path = "3X-CelestialResort",
                            Checkpoints = null,
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/03_resort", "event:/env/amb/03_exterior")
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("b93c27"),
                    TitleAccentColor = Calc.HexToColor("ffdd42"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.WalkInRight,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new KeyDoorWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.15f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "wood",
                    CassseteNoteColor = Calc.HexToColor("eed933"),
                    CassetteSong = "event:/music/cassette/03_resort"
                },

                new() // Golden Ridge (Chapter 4)
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
                        new()
                        {
                            PoemID = "cs",
                            Path = "4-GoldenRidge",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("b-00", "checkpoint_4_0"),
                                new("c-00", "checkpoint_4_1"),
                                new("d-00", "checkpoint_4_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/lvl4/main", "event:/env/amb/04_main")
                        },
                        new()
                        {
                            PoemID = "csr",
                            Path = "4H-GoldenRidge",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("b-00", "checkpoint_4h_0"),
                                new("c-00", "checkpoint_4h_1"),
                                new("d-00", "checkpoint_4h_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/04_cliffside", "event:/env/amb/04_main")
                        },
                        new()
                        {
                            Path = "4X-GoldenRidge",
                            Checkpoints = null,
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/04_cliffside", "event:/env/amb/04_main")
                        }
                    },
                    IntroType = Player.IntroTypes.WalkInRight,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new WindWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.1f,
                    BloomBase = 0.25f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "cliffside",
                    Spike = "cliffside",
                    CrumbleBlock = "cliffside",
                    WoodPlatform = "cliffside",
                    CassseteNoteColor = Calc.HexToColor("eb4bd9"),
                    CassetteSong = "event:/music/cassette/04_cliffside"
                },

                new() // Mirror Temple (Chapter 5)
                {
                    Name = "area_5",
                    Icon = "areas/temple",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "MirrorTemple",
                    CassetteCheckpointIndex = 1,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "t",
                            Path = "5-MirrorTemple",
                            Checkpoints = new CheckpointData[4]
                            {
                                new("b-00", "checkpoint_5_0"),
                                new("c-00", "checkpoint_5_1", dreaming: true, audioState: new AudioState("event:/music/lvl5/middle_temple", "event:/env/amb/05_interior_dark")),
                                new("d-00", "checkpoint_5_2", dreaming: true, audioState: new AudioState("event:/music/lvl5/middle_temple", "event:/env/amb/05_interior_dark")),
                                new("e-00", "checkpoint_5_3", dreaming: true, audioState: new AudioState("event:/music/lvl5/mirror", "event:/env/amb/05_interior_dark"))
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/lvl5/normal", "event:/env/amb/05_interior_main")
                        },
                        new()
                        {
                            PoemID = "tr",
                            Path = "5H-MirrorTemple",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("b-00", "checkpoint_5h_0"),
                                new("c-00", "checkpoint_5h_1"),
                                new("d-00", "checkpoint_5h_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/05_mirror_temple", "event:/env/amb/05_interior_main")
                        },
                        new()
                        {
                            Path = "5X-MirrorTemple",
                            Checkpoints =  null,
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/05_mirror_temple", "event:/env/amb/05_interior_main")
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("8314bc"),
                    TitleAccentColor = Calc.HexToColor("df72f9"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.WakeUp,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new DropWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.15f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = (level =>
                    {
                        level.Add(new SeekerEffectsController());
                        if (level.Session.Area.Mode != AreaMode.Normal)
                            return;
                        level.Add(new TempleEndingMusicHandler());
                    }),
                    Jumpthru = "temple",
                    CassseteNoteColor = Calc.HexToColor("5a56e6"),
                    CobwebColor = new Color[1]
                    {
                        Calc.HexToColor("9f2166")
                    },
                    CassetteSong = "event:/music/cassette/05_mirror_temple"
                },

                new() // Reflection (Chapter 6)
                {
                    Name = "area_6",
                    Icon = "areas/reflection",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "Fall",
                    CassetteCheckpointIndex = 2,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "tf",
                            Path = "6-Reflection",
                            Checkpoints = new CheckpointData[5]
                            {
                                new("00", "checkpoint_6_0"),
                                new("04", "checkpoint_6_1"),
                                new("b-00", "checkpoint_6_2"),
                                new("boss-00", "checkpoint_6_3"),
                                new("after-00", "checkpoint_6_4", new PlayerInventory?(PlayerInventory.CH6End))
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
                        },
                        new()
                        {
                            PoemID = "tfr",
                            Path = "6H-Reflection",
                            Checkpoints = new CheckpointData[3]
                            {
                                new("b-00", "checkpoint_6h_0"),
                                new("c-00", "checkpoint_6h_1"),
                                new("d-00", "checkpoint_6h_2")
                            },
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/06_reflection", "event:/env/amb/06_main")
                        },
                        new()
                        {
                            Path = "6X-Reflection",
                            Checkpoints =  null,
                            Inventory = PlayerInventory.Default,
                            AudioState = new AudioState("event:/music/remix/06_reflection", "event:/env/amb/06_main")
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("359FE0"),
                    TitleAccentColor = Calc.HexToColor("3C5CBC"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.None,
                    Dreaming = false,
                    ColorGrade = "reflection",
                    Wipe = (scene, wipeIn, onComplete) => new FallWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.2f,
                    BloomBase = 0.2f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "reflection",
                    Spike = "reflection",
                    CassseteNoteColor = Calc.HexToColor("56e6dd"),
                    CassetteSong = "event:/music/cassette/06_reflection"
                },

                new() // The Summit (Chapter 7)
                {
                    Name = "area_7",
                    Icon = "areas/summit",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "Summit",
                    CassetteCheckpointIndex = 3,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID = "ts",
                            Path = "7-Summit",
                            Checkpoints = new CheckpointData[6]
                            {
                                new("b-00", "checkpoint_7_0", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(1),  null)),
                                new("c-00", "checkpoint_7_1", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(2),  null)),
                                new("d-00", "checkpoint_7_2", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(3),  null)),
                                new("e-00b", "checkpoint_7_3", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(4),  null)),
                                new("f-00", "checkpoint_7_4", audioState: new AudioState(new AudioTrackState("event:/music/lvl7/main").SetProgress(5),  null)),
                                new("g-00", "checkpoint_7_5", audioState: new AudioState("event:/music/lvl7/final_ascent", null))
                            },
                            Inventory = PlayerInventory.TheSummit,
                            AudioState = new AudioState("event:/music/lvl7/main", null)
                        },
                        new()
                        {
                            PoemID = "tsr",
                            Path = "7H-Summit",
                            Checkpoints = new CheckpointData[6]
                            {
                                new("b-00", "checkpoint_7H_0"),
                                new("c-01", "checkpoint_7H_1"),
                                new("d-00", "checkpoint_7H_2"),
                                new("e-00", "checkpoint_7H_3"),
                                new("f-00", "checkpoint_7H_4"),
                                new("g-00", "checkpoint_7H_5")
                            },
                            Inventory = PlayerInventory.TheSummit,
                            AudioState = new AudioState("event:/music/remix/07_summit", null)
                        },
                        new()
                        {
                            Path = "7X-Summit",
                            Checkpoints = null,
                            Inventory = PlayerInventory.TheSummit,
                            AudioState = new AudioState("event:/music/remix/07_summit",  null)
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("FFD819"),
                    TitleAccentColor = Calc.HexToColor("197DB7"),
                    TitleTextColor = Color.Black,
                    IntroType = Player.IntroTypes.None,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new MountainWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.2f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "temple",
                    Spike = "outline",
                    CassseteNoteColor = Calc.HexToColor("e69156"),
                    CassetteSong = "event:/music/cassette/07_summit"
                },

                new() // Epilogue (Chapter 8)
                {
                    Name = "area_8",
                    Icon = "areas/intro",
                    Interlude = true,
                    CompleteScreenName = null,
                    CassetteCheckpointIndex = 1,
                    Mode = new ModeProperties[3]
                    {
                        new()
                        {
                            PoemID =  null,
                            Path = "8-Epilogue",
                            Checkpoints =  null,
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
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new CurtainWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "wood"
                },

                new() // Core (Chapter 9)
                {
                    Name = "area_9",
                    Icon = "areas/core",
                    Interlude = false,
                    CanFullClear = true,
                    CompleteScreenName = "Core",
                    CassetteCheckpointIndex = 3,
                    Mode = new ModeProperties[3]
                    {
                        new()
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
                        },
                        new()
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
                        },
                        new()
                        {
                            Path = "9X-Core",
                            Checkpoints = null,
                            Inventory = PlayerInventory.Core,
                            AudioState = new AudioState("event:/music/remix/09_core", "event:/env/amb/09_main")
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("761008"),
                    TitleAccentColor = Calc.HexToColor("E0201D"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.WalkInRight,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new HeartWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.0f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "core",
                    CassseteNoteColor = Calc.HexToColor("e6566a"),
                    CassetteSong = "event:/music/cassette/09_core",
                    CoreMode = Session.CoreModes.Hot
                },

                new() // Farewell (Chapter 10)
                {
                    Name = "area_10",
                    Icon = "areas/farewell",
                    Interlude = false,
                    CanFullClear = false,
                    IsFinal = true,
                    CompleteScreenName = "Core",
                    CassetteCheckpointIndex = -1,
                    Mode = new ModeProperties[1]
                    {
                        new()
                        {
                            PoemID = "fw",
                            Path = "LostLevels",
                            Checkpoints = new CheckpointData[8]
                            {
                                new("a-00", "checkpoint_9_0", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1),  null)),
                                new("c-00", "checkpoint_9_1", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1),  null)),
                                new("e-00z", "checkpoint_9_2", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part02"),  null)),
                                new("f-door", "checkpoint_9_3", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/intermission_heartgroove"),  null)),
                                new("h-00b", "checkpoint_9_4", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part03"),  null)),
                                new("i-00", "checkpoint_9_5", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/cassette_rooms").Param("sixteenth_note", 7f),  null))
                                {
                                    ColorGrade = "feelingdown"
                                },
                                new("j-00", "checkpoint_9_6", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/cassette_rooms").Param("sixteenth_note", 7f).SetProgress(3),  null))
                                {
                                    ColorGrade = "feelingdown"
                                },
                                new("j-16", "checkpoint_9_7", audioState: new AudioState(new AudioTrackState("event:/new_content/music/lvl10/final_run").SetProgress(3),  null))
                            },
                            Inventory = PlayerInventory.Farewell,
                            AudioState = new AudioState(new AudioTrackState("event:/new_content/music/lvl10/part01").SetProgress(1), new AudioTrackState("event:/env/amb/00_prologue"))
                        }
                    },
                    TitleBaseColor = Calc.HexToColor("240d7c"),
                    TitleAccentColor = Calc.HexToColor("FF6AA9"),
                    TitleTextColor = Color.White,
                    IntroType = Player.IntroTypes.ThinkForABit,
                    Dreaming = false,
                    ColorGrade = null,
                    Wipe = (scene, wipeIn, onComplete) => new StarfieldWipe(scene, wipeIn, onComplete),
                    DarknessAlpha = 0.05f,
                    BloomBase = 0.5f,
                    BloomStrength = 1f,
                    OnLevelBegin = null,
                    Jumpthru = "wood",
                    CassseteNoteColor = Calc.HexToColor("e6566a"),
                    CassetteSong = null,
                    CobwebColor = new Color[3]
                    {
                        Calc.HexToColor("42c192"),
                        Calc.HexToColor("af36a8"),
                        Calc.HexToColor("3474a6")
                    }
                }
            };

            int length = Enum.GetNames(typeof(AreaMode)).Length;
            for (int i = 0; i < Areas.Count; ++i)
            {
                Areas[i].ID = i;
                Areas[i].Mode[0].MapData = new MapData(new AreaKey(i));
                if (!Areas[i].Interlude)
                    for (int mode = 1; mode < length; ++mode)
                    {
                        AreaMode areaMode = (AreaMode) mode;
                        if (Areas[i].HasMode(areaMode))
                            Areas[i].Mode[mode].MapData = new MapData(new AreaKey(i, areaMode));
                    }
            }
            ReloadMountainViews();
        }

        public static void ReloadMountainViews()
        {
            foreach (XmlElement xml in (XmlNode) Calc.LoadXML(Path.Combine(Engine.ContentDirectory, "Overworld", "AreaViews.xml"))["Views"])
            {
                int i = xml.AttrInt("id");
                if (i >= 0 && i < Areas.Count)
                {
                    Vector3 idlePosition = xml["Idle"].AttrVector3("position");
                    Vector3 idleTarget = xml["Idle"].AttrVector3("target");
                    Areas[i].MountainIdle = new MountainCamera(idlePosition, idleTarget);
                    Vector3 selectPosition = xml["Select"].AttrVector3("position");
                    Vector3 selectTarget = xml["Select"].AttrVector3("target");
                    Areas[i].MountainSelect = new MountainCamera(selectPosition, selectTarget);
                    Vector3 zoomPosition = xml["Zoom"].AttrVector3("position");
                    Vector3 zoomTarget = xml["Zoom"].AttrVector3("target");
                    Areas[i].MountainZoom = new MountainCamera(zoomPosition, zoomTarget);
                    if (xml["Cursor"] != null)
                        Areas[i].MountainCursor = xml["Cursor"].AttrVector3("position");
                    Areas[i].MountainState = xml.AttrInt("state", 0);
                }
            }
        }

        public static bool IsPoemRemix(string id)
        {
            foreach (AreaData area in Areas)
            {
                if (area.Mode.Length > 1 && area.Mode[1] != null && !string.IsNullOrEmpty(area.Mode[1].PoemID) && area.Mode[1].PoemID.Equals(id, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static int GetCheckpointID(AreaKey area, string level)
        {
            CheckpointData[] checkpoints = Areas[area.ID].Mode[(int) area.Mode].Checkpoints;
            if (checkpoints != null)
            {
                for (int checkpointId = 0; checkpointId < checkpoints.Length; ++checkpointId)
                {
                    if (checkpoints[checkpointId].Level.Equals(level))
                        return checkpointId;
                }
            }
            return -1;
        }

        public static CheckpointData GetCheckpoint(AreaKey area, string level)
        {
            CheckpointData[] checkpoints = Areas[area.ID].Mode[(int) area.Mode].Checkpoints;
            if (level != null && checkpoints != null)
            {
                foreach (CheckpointData checkpoint in checkpoints)
                {
                    if (checkpoint.Level.Equals(level))
                        return checkpoint;
                }
            }
            return null;
        }

        public static string GetCheckpointName(AreaKey area, string level)
        {
            if (string.IsNullOrEmpty(level))
                return "START";
            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null ? Dialog.Clean(checkpoint.Name) : null;
        }

        public static PlayerInventory GetCheckpointInventory(AreaKey area, string level)
        {
            CheckpointData checkpoint = GetCheckpoint(area, level);
            return checkpoint != null && checkpoint.Inventory.HasValue ? checkpoint.Inventory.Value : Areas[area.ID].Mode[(int) area.Mode].Inventory;
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

        public static AudioState GetCheckpointAudioState(AreaKey area, string level) => GetCheckpoint(area, level)?.AudioState;

        public static string GetCheckpointColorGrading(AreaKey area, string level) => GetCheckpoint(area, level)?.ColorGrade;

        public static void Unload() => Areas = null;

        public static AreaData Get(Scene scene) => scene is not null and Level ? Areas[(scene as Level).Session.Area.ID] : null;

        public static AreaData Get(Session session) => session != null ? Areas[session.Area.ID] : null;

        public static AreaData Get(AreaKey area) => Areas[area.ID];

        public static AreaData Get(int id) => Areas[id];

        public XmlElement CompleteScreenXml => GFX.CompleteScreensXml["Screens"][CompleteScreenName];

        public void DoScreenWipe(Scene scene, bool wipeIn, Action onComplete = null)
        {
            if (Wipe == null)
                (Wipe = new Action<Scene, bool, Action>((s, w, oc) => new CurtainWipe(s, w, oc)))(scene, wipeIn, onComplete);
            else
                Wipe(scene, wipeIn, onComplete);
        }

        public bool HasMode(AreaMode mode) => (AreaMode) Mode.Length > mode && Mode[(int) mode] != null && Mode[(int) mode].Path != null;
    }
}
