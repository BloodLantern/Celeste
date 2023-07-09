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
                        AltMusic = attribute.Value as string;
                        continue;
                    case "ambience":
                        Ambience = attribute.Value as string;
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
                        EditorColorIndex = (int) attribute.Value;
                        continue;
                    case "cameraOffsetX":
                        CameraOffset.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        continue;
                    case "cameraOffsetY":
                        CameraOffset.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                        continue;
                    case "dark":
                        Dark = (bool) attribute.Value;
                        continue;
                    case "delayAltMusicFade":
                        DelayAltMusic = (bool) attribute.Value;
                        continue;
                    case "disableDownTransition":
                        DisableDownTransition = (bool) attribute.Value;
                        continue;
                    case "enforceDashNumber":
                        EnforceDashNumber = (int) attribute.Value;
                        continue;
                    case "height":
                        Bounds.Height = (int) attribute.Value;
                        if (Bounds.Height == 184)
                        {
                            Bounds.Height = 180;
                            continue;
                        }
                        continue;
                    case "music":
                        Music = (string) attribute.Value;
                        continue;
                    case "musicLayer1":
                        MusicLayers[0] = (bool) attribute.Value ? 1f : 0f;
                        continue;
                    case "musicLayer2":
                        MusicLayers[1] = (bool) attribute.Value ? 1f : 0f;
                        continue;
                    case "musicLayer3":
                        MusicLayers[2] = (bool) attribute.Value ? 1f : 0f;
                        continue;
                    case "musicLayer4":
                        MusicLayers[3] = (bool) attribute.Value ? 1f : 0f;
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
                        Space = (bool) attribute.Value;
                        continue;
                    case "underwater":
                        Underwater = (bool) attribute.Value;
                        continue;
                    case "whisper":
                        MusicWhispers = (bool) attribute.Value;
                        continue;
                    case "width":
                        Bounds.Width = (int) attribute.Value;
                        continue;
                    case "windPattern":
                        WindPattern = (WindController.Patterns) Enum.Parse(typeof(WindController.Patterns), (string) attribute.Value);
                        continue;
                    case "x":
                        Bounds.X = (int) attribute.Value;
                        continue;
                    case "y":
                        Bounds.Y = (int) attribute.Value;
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
            foreach (BinaryPacker.Element child in data.Children)
            {
                if (child.Name == "entities")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element entity in child.Children)
                        {
                            if (entity.Name == "player")
                                Spawns.Add(new Vector2(Bounds.X + Convert.ToSingle(entity.Attributes["x"], CultureInfo.InvariantCulture), Bounds.Y + Convert.ToSingle(entity.Attributes["y"], CultureInfo.InvariantCulture)));
                            else if (entity.Name is "strawberry" or "snowberry")
                                Strawberries++;
                            else if (entity.Name == "shard")
                                HasGem = true;
                            else if (entity.Name == "blackGem")
                                HasHeartGem = true;
                            else if (entity.Name == "checkpoint")
                                HasCheckpoint = true;

                            if (entity.Name != "player")
                                Entities.Add(CreateEntityData(entity));
                        }
                }
                else if (child.Name == "triggers")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element trigger in child.Children)
                            Triggers.Add(CreateEntityData(trigger));
                }
                else if (child.Name == "bgdecals")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element decal in child.Children)
                            BgDecals.Add(
                                new DecalData()
                                {
                                    Position = new Vector2(Convert.ToSingle(decal.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["y"], CultureInfo.InvariantCulture)),
                                    Scale = new Vector2(Convert.ToSingle(decal.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                    Texture = (string) decal.Attributes["texture"]
                                }
                            );
                }
                else if (child.Name == "fgdecals")
                {
                    if (child.Children != null)
                        foreach (BinaryPacker.Element decal in child.Children)
                            FgDecals.Add(
                                new DecalData()
                                {
                                    Position = new Vector2(Convert.ToSingle(decal.Attributes["x"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["y"], CultureInfo.InvariantCulture)),
                                    Scale = new Vector2(Convert.ToSingle(decal.Attributes["scaleX"], CultureInfo.InvariantCulture), Convert.ToSingle(decal.Attributes["scaleY"], CultureInfo.InvariantCulture)),
                                    Texture = (string) decal.Attributes["texture"]
                                }
                            );
                }
                else if (child.Name == "solids")
                    Solids = child.Attr("innerText");
                else if (child.Name == "bg")
                    Bg = child.Attr("innerText");
                else if (child.Name == "fgtiles")
                    FgTiles = child.Attr("innerText");
                else if (child.Name == "bgtiles")
                    BgTiles = child.Attr("innerText");
                else if (child.Name == "objtiles")
                    ObjTiles = child.Attr("innerText");
            }
            Dummy = Spawns.Count == 0;
        }

        private EntityData CreateEntityData(BinaryPacker.Element entity)
        {
            EntityData result = new()
            {
                Name = entity.Name,
                Level = this,
                Nodes = new Vector2[entity.Children == null ? 0 : entity.Children.Count]
            };

            if (entity.Attributes != null)
                foreach (KeyValuePair<string, object> attribute in entity.Attributes)
                {
                    if (attribute.Key == "id")
                        result.ID = (int) attribute.Value;
                    else if (attribute.Key == "x")
                        result.Position.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "y")
                        result.Position.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "width")
                        result.Width = (int) attribute.Value;
                    else if (attribute.Key == "height")
                        result.Height = (int) attribute.Value;
                    else if (attribute.Key == "originX")
                        result.Origin.X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "originY")
                        result.Origin.Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else
                    {
                        result.Values ??= new Dictionary<string, object>();
                        result.Values.Add(attribute.Key, attribute.Value);
                    }
                }

            for (int i = 0; i < result.Nodes.Length; i++)
                foreach (KeyValuePair<string, object> attribute in entity.Children[i].Attributes)
                {
                    if (attribute.Key == "x")
                        result.Nodes[i].X = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                    else if (attribute.Key == "y")
                        result.Nodes[i].Y = Convert.ToSingle(attribute.Value, CultureInfo.InvariantCulture);
                }

            return result;
        }

        public bool Check(Vector2 at) => at.X >= Bounds.Left && at.Y >= Bounds.Top && at.X < Bounds.Right && at.Y < Bounds.Bottom;

        public Rectangle TileBounds => new(Bounds.X / 8, Bounds.Y / 8, (int) Math.Ceiling(Bounds.Width / 8.0), (int) Math.Ceiling(Bounds.Height / 8.0));

        public Vector2 Position
        {
            get => new(Bounds.X, Bounds.Y);
            set
            {
                for (int index = 0; index < Spawns.Count; ++index)
                    Spawns[index] -= Position;
                Bounds.X = (int) value.X;
                Bounds.Y = (int) value.Y;
                for (int index = 0; index < Spawns.Count; ++index)
                    Spawns[index] += Position;
            }
        }

        public int LoadSeed
        {
            get
            {
                int loadSeed = 0;
                foreach (char ch in Name)
                    loadSeed += ch;
                return loadSeed;
            }
        }
    }
}
