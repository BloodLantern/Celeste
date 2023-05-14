// Decompiled with JetBrains decompiler
// Type: Celeste.LevelData
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Celeste
{
    public class LevelData
    {
        public string Name;
        public bool Dummy;
        public int Strawberries;
        public bool HasGem;
        public bool HasHeartGem;
        public bool HasCheckpoint;
        public bool DisableDownTransition;
        public Rectangle Bounds;
        public List<EntityData> Entities;
        public List<EntityData> Triggers;
        public List<Vector2> Spawns;
        public List<DecalData> FgDecals;
        public List<DecalData> BgDecals;
        public string Solids = "";
        public string Bg = "";
        public string FgTiles = "";
        public string BgTiles = "";
        public string ObjTiles = "";
        public WindController.Patterns WindPattern;
        public Vector2 CameraOffset;
        public bool Dark;
        public bool Underwater;
        public bool Space;
        public string Music = "";
        public string AltMusic = "";
        public string Ambience = "";
        public float[] MusicLayers = new float[4];
        public int MusicProgress = -1;
        public int AmbienceProgress = -1;
        public bool MusicWhispers;
        public bool DelayAltMusic;
        public int EnforceDashNumber;
        public int EditorColorIndex;

        public LevelData(BinaryPacker.Element data)
        {
            Bounds = new Rectangle();
            foreach (KeyValuePair<string, object> attribute in data.Attributes)
            {
                switch (attribute.Key)
                {
                    case "alt_music":
                        AltMusic = (string)attribute.Value;
                        continue;
                    case "ambience":
                        Ambience = (string)attribute.Value;
                        continue;
                    case "ambienceProgress":
                        string s1 = attribute.Value.ToString();
                        if (string.IsNullOrEmpty(s1) || !int.TryParse(s1, out AmbienceProgress))
                        {
                            AmbienceProgress = -1;
                            continue;
                        }
                        continue;
                    case "c":
                        EditorColorIndex = (int)attribute.Value;
                        continue;
                    case "cameraOffsetX":
                        CameraOffset.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        continue;
                    case "cameraOffsetY":
                        CameraOffset.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        continue;
                    case "dark":
                        Dark = (bool)attribute.Value;
                        continue;
                    case "delayAltMusicFade":
                        DelayAltMusic = (bool)attribute.Value;
                        continue;
                    case "disableDownTransition":
                        DisableDownTransition = (bool)attribute.Value;
                        continue;
                    case "enforceDashNumber":
                        EnforceDashNumber = (int)attribute.Value;
                        continue;
                    case "height":
                        Bounds.Height = (int)attribute.Value;
                        if (Bounds.Height == 184)
                        {
                            Bounds.Height = 180;
                            continue;
                        }
                        continue;
                    case "music":
                        Music = (string)attribute.Value;
                        continue;
                    case "musicLayer1":
                        MusicLayers[0] = (bool)attribute.Value ? 1f : 0.0f;
                        continue;
                    case "musicLayer2":
                        MusicLayers[1] = (bool)attribute.Value ? 1f : 0.0f;
                        continue;
                    case "musicLayer3":
                        MusicLayers[2] = (bool)attribute.Value ? 1f : 0.0f;
                        continue;
                    case "musicLayer4":
                        MusicLayers[3] = (bool)attribute.Value ? 1f : 0.0f;
                        continue;
                    case "musicProgress":
                        string s2 = attribute.Value.ToString();
                        if (string.IsNullOrEmpty(s2) || !int.TryParse(s2, out MusicProgress))
                        {
                            MusicProgress = -1;
                            continue;
                        }
                        continue;
                    case "name":
                        Name = attribute.Value.ToString().Substring(4);
                        continue;
                    case "space":
                        Space = (bool)attribute.Value;
                        continue;
                    case "underwater":
                        Underwater = (bool)attribute.Value;
                        continue;
                    case "whisper":
                        MusicWhispers = (bool)attribute.Value;
                        continue;
                    case "width":
                        Bounds.Width = (int)attribute.Value;
                        continue;
                    case "windPattern":
                        WindPattern = (WindController.Patterns)Enum.Parse(typeof(WindController.Patterns), (string)attribute.Value);
                        continue;
                    case "x":
                        Bounds.X = (int)attribute.Value;
                        continue;
                    case "y":
                        Bounds.Y = (int)attribute.Value;
                        continue;
                    default:
                        continue;
                }
            }
            Spawns = new List<Vector2>();
            Entities = new List<EntityData>();
            Triggers = new List<EntityData>();
            BgDecals = new List<DecalData>();
            FgDecals = new List<DecalData>();
            foreach (BinaryPacker.Element child1 in data.Children)
            {
                if (child1.Name == "entities")
                {
                    if (child1.Children != null)
                    {
                        foreach (BinaryPacker.Element child2 in child1.Children)
                        {
                            if (child2.Name == "player")
                            {
                                Spawns.Add(new Vector2(Bounds.X + Convert.ToSingle(child2.Attributes["x"], CultureInfo.InvariantCulture), Bounds.Y + Convert.ToSingle(child2.Attributes["y"], CultureInfo.InvariantCulture)));
                            }
                            else if (child2.Name is "strawberry" or "snowberry")
                            {
                                ++Strawberries;
                            }
                            else if (child2.Name == "shard")
                            {
                                HasGem = true;
                            }
                            else if (child2.Name == "blackGem")
                            {
                                HasHeartGem = true;
                            }
                            else if (child2.Name == "checkpoint")
                            {
                                HasCheckpoint = true;
                            }

                            if (!child2.Name.Equals("player"))
                            {
                                Entities.Add(CreateEntityData(child2));
                            }
                        }
                    }
                }
                else if (child1.Name == "triggers")
                {
                    if (child1.Children != null)
                    {
                        foreach (BinaryPacker.Element child3 in child1.Children)
                        {
                            Triggers.Add(CreateEntityData(child3));
                        }
                    }
                }
                else if (child1.Name == "bgdecals")
                {
                    if (child1.Children != null)
                    {
                        foreach (BinaryPacker.Element child4 in child1.Children)
                        {
                            BgDecals.Add(new DecalData()
                            {
                                Position = new Vector2(Convert.ToSingle(child4.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(child4.Attributes["y"], CultureInfo.InvariantCulture)),
                                Scale = new Vector2(Convert.ToSingle(child4.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(child4.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                Texture = (string)child4.Attributes["texture"]
                            });
                        }
                    }
                }
                else if (child1.Name == "fgdecals")
                {
                    if (child1.Children != null)
                    {
                        foreach (BinaryPacker.Element child5 in child1.Children)
                        {
                            FgDecals.Add(new DecalData()
                            {
                                Position = new Vector2(Convert.ToSingle(child5.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(child5.Attributes["y"], CultureInfo.InvariantCulture)),
                                Scale = new Vector2(Convert.ToSingle(child5.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(child5.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                Texture = (string)child5.Attributes["texture"]
                            });
                        }
                    }
                }
                else if (child1.Name == "solids")
                {
                    Solids = child1.Attr("innerText");
                }
                else if (child1.Name == "bg")
                {
                    Bg = child1.Attr("innerText");
                }
                else if (child1.Name == "fgtiles")
                {
                    FgTiles = child1.Attr("innerText");
                }
                else if (child1.Name == "bgtiles")
                {
                    BgTiles = child1.Attr("innerText");
                }
                else if (child1.Name == "objtiles")
                {
                    ObjTiles = child1.Attr("innerText");
                }
            }
            Dummy = Spawns.Count <= 0;
        }

        private EntityData CreateEntityData(BinaryPacker.Element entity)
        {
            EntityData entityData = new()
            {
                Name = entity.Name,
                Level = this
            };
            if (entity.Attributes != null)
            {
                foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                {
                    if (attribute.Key == "id")
                    {
                        entityData.ID = (int)attribute.Value;
                    }
                    else if (attribute.Key == "x")
                    {
                        entityData.Position.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attribute.Key == "y")
                    {
                        entityData.Position.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attribute.Key == "width")
                    {
                        entityData.Width = (int)attribute.Value;
                    }
                    else if (attribute.Key == "height")
                    {
                        entityData.Height = (int)attribute.Value;
                    }
                    else if (attribute.Key == "originX")
                    {
                        entityData.Origin.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attribute.Key == "originY")
                    {
                        entityData.Origin.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        entityData.Values ??= new Dictionary<string, object>();
                        entityData.Values.Add(attribute.Key, attribute.Value);
                    }
                }
            }
            entityData.Nodes = new Vector2[entity.Children == null ? 0 : entity.Children.Count];
            for (int index = 0; index < entityData.Nodes.Length; ++index)
            {
                foreach (KeyValuePair<string, object> attribute in entity.Children[index].Attributes)
                {
                    if (attribute.Key == "x")
                    {
                        entityData.Nodes[index].X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                    else if (attribute.Key == "y")
                    {
                        entityData.Nodes[index].Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    }
                }
            }
            return entityData;
        }

        public bool Check(Vector2 at)
        {
            return at.X >= (double)Bounds.Left && at.Y >= (double)Bounds.Top && at.X < (double)Bounds.Right && at.Y < (double)Bounds.Bottom;
        }

        public Rectangle TileBounds => new(Bounds.X / 8, Bounds.Y / 8, (int)Math.Ceiling(Bounds.Width / 8.0), (int)Math.Ceiling(Bounds.Height / 8.0));

        public Vector2 Position
        {
            get => new(Bounds.X, Bounds.Y);
            set
            {
                for (int index = 0; index < Spawns.Count; ++index)
                {
                    Spawns[index] -= Position;
                }

                Bounds.X = (int)value.X;
                Bounds.Y = (int)value.Y;
                for (int index = 0; index < Spawns.Count; ++index)
                {
                    Spawns[index] += Position;
                }
            }
        }

        public int LoadSeed
        {
            get
            {
                int loadSeed = 0;
                foreach (char ch in Name)
                {
                    loadSeed += ch;
                }

                return loadSeed;
            }
        }
    }
}
