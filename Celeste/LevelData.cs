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
      this.Bounds = new Rectangle();
      foreach (KeyValuePair<string, object> attribute in data.Attributes)
      {
        switch (attribute.Key)
        {
          case "alt_music":
            this.AltMusic = (string) attribute.Value;
            continue;
          case "ambience":
            this.Ambience = (string) attribute.Value;
            continue;
          case "ambienceProgress":
            string s1 = attribute.Value.ToString();
            if (string.IsNullOrEmpty(s1) || !int.TryParse(s1, out this.AmbienceProgress))
            {
              this.AmbienceProgress = -1;
              continue;
            }
            continue;
          case "c":
            this.EditorColorIndex = (int) attribute.Value;
            continue;
          case "cameraOffsetX":
            this.CameraOffset.X = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
            continue;
          case "cameraOffsetY":
            this.CameraOffset.Y = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
            continue;
          case "dark":
            this.Dark = (bool) attribute.Value;
            continue;
          case "delayAltMusicFade":
            this.DelayAltMusic = (bool) attribute.Value;
            continue;
          case "disableDownTransition":
            this.DisableDownTransition = (bool) attribute.Value;
            continue;
          case "enforceDashNumber":
            this.EnforceDashNumber = (int) attribute.Value;
            continue;
          case "height":
            this.Bounds.Height = (int) attribute.Value;
            if (this.Bounds.Height == 184)
            {
              this.Bounds.Height = 180;
              continue;
            }
            continue;
          case "music":
            this.Music = (string) attribute.Value;
            continue;
          case "musicLayer1":
            this.MusicLayers[0] = (bool) attribute.Value ? 1f : 0.0f;
            continue;
          case "musicLayer2":
            this.MusicLayers[1] = (bool) attribute.Value ? 1f : 0.0f;
            continue;
          case "musicLayer3":
            this.MusicLayers[2] = (bool) attribute.Value ? 1f : 0.0f;
            continue;
          case "musicLayer4":
            this.MusicLayers[3] = (bool) attribute.Value ? 1f : 0.0f;
            continue;
          case "musicProgress":
            string s2 = attribute.Value.ToString();
            if (string.IsNullOrEmpty(s2) || !int.TryParse(s2, out this.MusicProgress))
            {
              this.MusicProgress = -1;
              continue;
            }
            continue;
          case "name":
            this.Name = attribute.Value.ToString().Substring(4);
            continue;
          case "space":
            this.Space = (bool) attribute.Value;
            continue;
          case "underwater":
            this.Underwater = (bool) attribute.Value;
            continue;
          case "whisper":
            this.MusicWhispers = (bool) attribute.Value;
            continue;
          case "width":
            this.Bounds.Width = (int) attribute.Value;
            continue;
          case "windPattern":
            this.WindPattern = (WindController.Patterns) Enum.Parse(typeof (WindController.Patterns), (string) attribute.Value);
            continue;
          case "x":
            this.Bounds.X = (int) attribute.Value;
            continue;
          case "y":
            this.Bounds.Y = (int) attribute.Value;
            continue;
          default:
            continue;
        }
      }
      this.Spawns = new List<Vector2>();
      this.Entities = new List<EntityData>();
      this.Triggers = new List<EntityData>();
      this.BgDecals = new List<DecalData>();
      this.FgDecals = new List<DecalData>();
      foreach (BinaryPacker.Element child1 in data.Children)
      {
        if (child1.Name == "entities")
        {
          if (child1.Children != null)
          {
            foreach (BinaryPacker.Element child2 in child1.Children)
            {
              if (child2.Name == "player")
                this.Spawns.Add(new Vector2((float) this.Bounds.X + Convert.ToSingle(child2.Attributes["x"], (IFormatProvider) CultureInfo.InvariantCulture), (float) this.Bounds.Y + Convert.ToSingle(child2.Attributes["y"], (IFormatProvider) CultureInfo.InvariantCulture)));
              else if (child2.Name == "strawberry" || child2.Name == "snowberry")
                ++this.Strawberries;
              else if (child2.Name == "shard")
                this.HasGem = true;
              else if (child2.Name == "blackGem")
                this.HasHeartGem = true;
              else if (child2.Name == "checkpoint")
                this.HasCheckpoint = true;
              if (!child2.Name.Equals("player"))
                this.Entities.Add(this.CreateEntityData(child2));
            }
          }
        }
        else if (child1.Name == "triggers")
        {
          if (child1.Children != null)
          {
            foreach (BinaryPacker.Element child3 in child1.Children)
              this.Triggers.Add(this.CreateEntityData(child3));
          }
        }
        else if (child1.Name == "bgdecals")
        {
          if (child1.Children != null)
          {
            foreach (BinaryPacker.Element child4 in child1.Children)
              this.BgDecals.Add(new DecalData()
              {
                Position = new Vector2(Convert.ToSingle(child4.Attributes["x"], (IFormatProvider) CultureInfo.InvariantCulture), Convert.ToSingle(child4.Attributes["y"], (IFormatProvider) CultureInfo.InvariantCulture)),
                Scale = new Vector2(Convert.ToSingle(child4.Attributes["scaleX"], (IFormatProvider) CultureInfo.InvariantCulture), Convert.ToSingle(child4.Attributes["scaleY"], (IFormatProvider) CultureInfo.InvariantCulture)),
                Texture = (string) child4.Attributes["texture"]
              });
          }
        }
        else if (child1.Name == "fgdecals")
        {
          if (child1.Children != null)
          {
            foreach (BinaryPacker.Element child5 in child1.Children)
              this.FgDecals.Add(new DecalData()
              {
                Position = new Vector2(Convert.ToSingle(child5.Attributes["x"], (IFormatProvider) CultureInfo.InvariantCulture), Convert.ToSingle(child5.Attributes["y"], (IFormatProvider) CultureInfo.InvariantCulture)),
                Scale = new Vector2(Convert.ToSingle(child5.Attributes["scaleX"], (IFormatProvider) CultureInfo.InvariantCulture), Convert.ToSingle(child5.Attributes["scaleY"], (IFormatProvider) CultureInfo.InvariantCulture)),
                Texture = (string) child5.Attributes["texture"]
              });
          }
        }
        else if (child1.Name == "solids")
          this.Solids = child1.Attr("innerText");
        else if (child1.Name == "bg")
          this.Bg = child1.Attr("innerText");
        else if (child1.Name == "fgtiles")
          this.FgTiles = child1.Attr("innerText");
        else if (child1.Name == "bgtiles")
          this.BgTiles = child1.Attr("innerText");
        else if (child1.Name == "objtiles")
          this.ObjTiles = child1.Attr("innerText");
      }
      this.Dummy = this.Spawns.Count <= 0;
    }

    private EntityData CreateEntityData(BinaryPacker.Element entity)
    {
      EntityData entityData = new EntityData();
      entityData.Name = entity.Name;
      entityData.Level = this;
      if (entity.Attributes != null)
      {
        foreach (KeyValuePair<string, object> attribute in entity.Attributes)
        {
          if (attribute.Key == "id")
            entityData.ID = (int) attribute.Value;
          else if (attribute.Key == "x")
            entityData.Position.X = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
          else if (attribute.Key == "y")
            entityData.Position.Y = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
          else if (attribute.Key == "width")
            entityData.Width = (int) attribute.Value;
          else if (attribute.Key == "height")
            entityData.Height = (int) attribute.Value;
          else if (attribute.Key == "originX")
            entityData.Origin.X = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
          else if (attribute.Key == "originY")
          {
            entityData.Origin.Y = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
          }
          else
          {
            if (entityData.Values == null)
              entityData.Values = new Dictionary<string, object>();
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
            entityData.Nodes[index].X = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
          else if (attribute.Key == "y")
            entityData.Nodes[index].Y = Convert.ToSingle(attribute.Value, (IFormatProvider) CultureInfo.InvariantCulture);
        }
      }
      return entityData;
    }

    public bool Check(Vector2 at) => (double) at.X >= (double) this.Bounds.Left && (double) at.Y >= (double) this.Bounds.Top && (double) at.X < (double) this.Bounds.Right && (double) at.Y < (double) this.Bounds.Bottom;

    public Rectangle TileBounds => new Rectangle(this.Bounds.X / 8, this.Bounds.Y / 8, (int) Math.Ceiling((double) this.Bounds.Width / 8.0), (int) Math.Ceiling((double) this.Bounds.Height / 8.0));

    public Vector2 Position
    {
      get => new Vector2((float) this.Bounds.X, (float) this.Bounds.Y);
      set
      {
        for (int index = 0; index < this.Spawns.Count; ++index)
          this.Spawns[index] -= this.Position;
        this.Bounds.X = (int) value.X;
        this.Bounds.Y = (int) value.Y;
        for (int index = 0; index < this.Spawns.Count; ++index)
          this.Spawns[index] += this.Position;
      }
    }

    public int LoadSeed
    {
      get
      {
        int loadSeed = 0;
        foreach (char ch in this.Name)
          loadSeed += (int) ch;
        return loadSeed;
      }
    }
  }
}
