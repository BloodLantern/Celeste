// Decompiled with JetBrains decompiler
// Type: Celeste.HeartGemDoor
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
  public class HeartGemDoor : Entity
  {
    private const string OpenedFlag = "opened_heartgem_door_";
    public static ParticleType P_Shimmer;
    public static ParticleType P_Slice;
    public readonly int Requires;
    public int Size;
    private readonly float openDistance;
    private float openPercent;
    private Solid TopSolid;
    private Solid BotSolid;
    private float offset;
    private Vector2 mist;
    private MTexture temp = new MTexture();
    private List<MTexture> icon;
    private HeartGemDoor.Particle[] particles = new HeartGemDoor.Particle[50];
    private bool startHidden;
    private float heartAlpha = 1f;

    public int HeartGems => SaveData.Instance.CheatMode ? this.Requires : SaveData.Instance.TotalHeartGems;

    public float Counter { get; private set; }

    public bool Opened { get; private set; }

    private float openAmount => this.openPercent * this.openDistance;

    public HeartGemDoor(EntityData data, Vector2 offset)
      : base(data.Position + offset)
    {
      this.Requires = data.Int("requires");
      this.Add((Component) new CustomBloom(new Action(this.RenderBloom)));
      this.Size = data.Width;
      this.openDistance = 32f;
      Vector2? nullable = data.FirstNodeNullable(new Vector2?(offset));
      if (nullable.HasValue)
        this.openDistance = Math.Abs(nullable.Value.Y - this.Y);
      this.icon = GFX.Game.GetAtlasSubtextures("objects/heartdoor/icon");
      this.startHidden = data.Bool(nameof (startHidden));
    }

    public override void Added(Scene scene)
    {
      base.Added(scene);
      Level level1 = scene as Level;
      for (int index = 0; index < this.particles.Length; ++index)
      {
        this.particles[index].Position = new Vector2(Calc.Random.NextFloat((float) this.Size), Calc.Random.NextFloat((float) level1.Bounds.Height));
        this.particles[index].Speed = (float) Calc.Random.Range(4, 12);
        this.particles[index].Color = Color.White * Calc.Random.Range(0.2f, 0.6f);
      }
      Level level2 = level1;
      double x = (double) this.X;
      Rectangle bounds = level1.Bounds;
      double y1 = (double) (bounds.Top - 32);
      Vector2 position1 = new Vector2((float) x, (float) y1);
      double size1 = (double) this.Size;
      double y2 = (double) this.Y;
      bounds = level1.Bounds;
      double top = (double) bounds.Top;
      double height1 = y2 - top + 32.0;
      Solid solid1 = this.TopSolid = new Solid(position1, (float) size1, (float) height1, true);
      level2.Add((Entity) solid1);
      this.TopSolid.SurfaceSoundIndex = 32;
      this.TopSolid.SquishEvenInAssistMode = true;
      this.TopSolid.EnableAssistModeChecks = false;
      Level level3 = level1;
      Vector2 position2 = new Vector2(this.X, this.Y);
      double size2 = (double) this.Size;
      bounds = level1.Bounds;
      double height2 = (double) bounds.Bottom - (double) this.Y + 32.0;
      Solid solid2 = this.BotSolid = new Solid(position2, (float) size2, (float) height2, true);
      level3.Add((Entity) solid2);
      this.BotSolid.SurfaceSoundIndex = 32;
      this.BotSolid.SquishEvenInAssistMode = true;
      this.BotSolid.EnableAssistModeChecks = false;
      if ((this.Scene as Level).Session.GetFlag("opened_heartgem_door_" + (object) this.Requires))
      {
        this.Opened = true;
        this.Visible = true;
        this.openPercent = 1f;
        this.Counter = (float) this.Requires;
        this.TopSolid.Y -= this.openDistance;
        this.BotSolid.Y += this.openDistance;
      }
      else
        this.Add((Component) new Coroutine(this.Routine()));
    }

    public override void Awake(Scene scene)
    {
      base.Awake(scene);
      if (this.Opened)
      {
        this.Scene.CollideFirst<DashBlock>(this.BotSolid.Collider.Bounds)?.RemoveSelf();
      }
      else
      {
        if (!this.startHidden)
          return;
        Player entity = this.Scene.Tracker.GetEntity<Player>();
        if (entity != null && (double) entity.X > (double) this.X)
        {
          this.startHidden = false;
          this.Scene.CollideFirst<DashBlock>(this.BotSolid.Collider.Bounds)?.RemoveSelf();
        }
        else
          this.Visible = false;
      }
    }

    private IEnumerator Routine()
    {
      HeartGemDoor heartGemDoor = this;
      Level level = heartGemDoor.Scene as Level;
      float topTo;
      float botTo;
      float topFrom;
      float botFrom;
      float p;
      if (heartGemDoor.startHidden)
      {
        Player entity1;
        do
        {
          yield return (object) null;
          entity1 = heartGemDoor.Scene.Tracker.GetEntity<Player>();
        }
        while (entity1 == null || (double) Math.Abs(entity1.X - heartGemDoor.Center.X) >= 100.0);
        Audio.Play("event:/new_content/game/10_farewell/heart_door", heartGemDoor.Position);
        heartGemDoor.Visible = true;
        heartGemDoor.heartAlpha = 0.0f;
        topTo = heartGemDoor.TopSolid.Y;
        botTo = heartGemDoor.BotSolid.Y;
        topFrom = (heartGemDoor.TopSolid.Y -= 240f);
        botFrom = (heartGemDoor.BotSolid.Y -= 240f);
        for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime * 1.2f)
        {
          float num = Ease.CubeIn(p);
          heartGemDoor.TopSolid.MoveToY(topFrom + (topTo - topFrom) * num);
          heartGemDoor.BotSolid.MoveToY(botFrom + (botTo - botFrom) * num);
          DashBlock dashBlock = heartGemDoor.Scene.CollideFirst<DashBlock>(heartGemDoor.BotSolid.Collider.Bounds);
          if (dashBlock != null)
          {
            level.Shake(0.5f);
            Celeste.Celeste.Freeze(0.1f);
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            dashBlock.Break(heartGemDoor.BotSolid.BottomCenter, new Vector2(0.0f, 1f), playDebrisSound: false);
            Player entity2 = heartGemDoor.Scene.Tracker.GetEntity<Player>();
            if (entity2 != null && (double) Math.Abs(entity2.X - heartGemDoor.Center.X) < 40.0)
              entity2.PointBounce(entity2.Position + Vector2.UnitX * 8f);
          }
          yield return (object) null;
        }
        level.Shake(0.5f);
        Celeste.Celeste.Freeze(0.1f);
        heartGemDoor.TopSolid.Y = topTo;
        heartGemDoor.BotSolid.Y = botTo;
        while ((double) heartGemDoor.heartAlpha < 1.0)
        {
          heartGemDoor.heartAlpha = Calc.Approach(heartGemDoor.heartAlpha, 1f, Engine.DeltaTime * 2f);
          yield return (object) null;
        }
        yield return (object) 0.6f;
      }
      while (!heartGemDoor.Opened && (double) heartGemDoor.Counter < (double) heartGemDoor.Requires)
      {
        Player entity = heartGemDoor.Scene.Tracker.GetEntity<Player>();
        if (entity != null && (double) Math.Abs(entity.X - heartGemDoor.Center.X) < 80.0 && (double) entity.X < (double) heartGemDoor.X)
        {
          if ((double) heartGemDoor.Counter == 0.0 && heartGemDoor.HeartGems > 0)
            Audio.Play("event:/game/09_core/frontdoor_heartfill", heartGemDoor.Position);
          if (heartGemDoor.HeartGems < heartGemDoor.Requires)
            level.Session.SetFlag("granny_door");
          int counter1 = (int) heartGemDoor.Counter;
          int target = Math.Min(heartGemDoor.HeartGems, heartGemDoor.Requires);
          heartGemDoor.Counter = Calc.Approach(heartGemDoor.Counter, (float) target, (float) ((double) Engine.DeltaTime * (double) heartGemDoor.Requires * 0.800000011920929));
          int counter2 = (int) heartGemDoor.Counter;
          if (counter1 != counter2)
          {
            yield return (object) 0.1f;
            if ((double) heartGemDoor.Counter < (double) target)
              Audio.Play("event:/game/09_core/frontdoor_heartfill", heartGemDoor.Position);
          }
        }
        else
          heartGemDoor.Counter = Calc.Approach(heartGemDoor.Counter, 0.0f, (float) ((double) Engine.DeltaTime * (double) heartGemDoor.Requires * 4.0));
        yield return (object) null;
      }
      yield return (object) 0.5f;
      heartGemDoor.Scene.Add((Entity) new HeartGemDoor.WhiteLine(heartGemDoor.Position, heartGemDoor.Size));
      level.Shake();
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Long);
      level.Flash(Color.White * 0.5f);
      Audio.Play("event:/game/09_core/frontdoor_unlock", heartGemDoor.Position);
      heartGemDoor.Opened = true;
      level.Session.SetFlag("opened_heartgem_door_" + (object) heartGemDoor.Requires);
      heartGemDoor.offset = 0.0f;
      yield return (object) 0.6f;
      botFrom = heartGemDoor.TopSolid.Y;
      topFrom = heartGemDoor.TopSolid.Y - heartGemDoor.openDistance;
      botTo = heartGemDoor.BotSolid.Y;
      topTo = heartGemDoor.BotSolid.Y + heartGemDoor.openDistance;
      for (p = 0.0f; (double) p < 1.0; p += Engine.DeltaTime)
      {
        level.Shake();
        heartGemDoor.openPercent = Ease.CubeIn(p);
        heartGemDoor.TopSolid.MoveToY(MathHelper.Lerp(botFrom, topFrom, heartGemDoor.openPercent));
        heartGemDoor.BotSolid.MoveToY(MathHelper.Lerp(botTo, topTo, heartGemDoor.openPercent));
        if ((double) p >= 0.40000000596046448 && level.OnInterval(0.1f))
        {
          for (int index = 4; index < heartGemDoor.Size; index += 4)
          {
            level.ParticlesBG.Emit(HeartGemDoor.P_Shimmer, 1, new Vector2((float) ((double) heartGemDoor.TopSolid.Left + (double) index + 1.0), heartGemDoor.TopSolid.Bottom - 2f), new Vector2(2f, 2f), -1.57079637f);
            level.ParticlesBG.Emit(HeartGemDoor.P_Shimmer, 1, new Vector2((float) ((double) heartGemDoor.BotSolid.Left + (double) index + 1.0), heartGemDoor.BotSolid.Top + 2f), new Vector2(2f, 2f), 1.57079637f);
          }
        }
        yield return (object) null;
      }
      heartGemDoor.TopSolid.MoveToY(topFrom);
      heartGemDoor.BotSolid.MoveToY(topTo);
      heartGemDoor.openPercent = 1f;
    }

    public override void Update()
    {
      base.Update();
      if (this.Opened)
        return;
      this.offset += 12f * Engine.DeltaTime;
      this.mist.X -= 4f * Engine.DeltaTime;
      this.mist.Y -= 24f * Engine.DeltaTime;
      for (int index = 0; index < this.particles.Length; ++index)
        this.particles[index].Position.Y += this.particles[index].Speed * Engine.DeltaTime;
    }

    public void RenderBloom()
    {
      if (this.Opened || !this.Visible)
        return;
      this.DrawBloom(new Rectangle((int) this.TopSolid.X, (int) this.TopSolid.Y, this.Size, (int) ((double) this.TopSolid.Height + (double) this.BotSolid.Height)));
    }

    private void DrawBloom(Rectangle bounds)
    {
      Draw.Rect((float) (bounds.Left - 4), (float) bounds.Top, 2f, (float) bounds.Height, Color.White * 0.25f);
      Draw.Rect((float) (bounds.Left - 2), (float) bounds.Top, 2f, (float) bounds.Height, Color.White * 0.5f);
      Draw.Rect(bounds, Color.White * 0.75f);
      Draw.Rect((float) bounds.Right, (float) bounds.Top, 2f, (float) bounds.Height, Color.White * 0.5f);
      Draw.Rect((float) (bounds.Right + 2), (float) bounds.Top, 2f, (float) bounds.Height, Color.White * 0.25f);
    }

    private void DrawMist(Rectangle bounds, Vector2 mist)
    {
      Color color = Color.White * 0.6f;
      MTexture mtexture = GFX.Game["objects/heartdoor/mist"];
      int num1 = mtexture.Width / 2;
      int num2 = mtexture.Height / 2;
      for (int index1 = 0; index1 < bounds.Width; index1 += num1)
      {
        for (int index2 = 0; index2 < bounds.Height; index2 += num2)
        {
          mtexture.GetSubtexture((int) this.Mod(mist.X, (float) num1), (int) this.Mod(mist.Y, (float) num2), Math.Min(num1, bounds.Width - index1), Math.Min(num2, bounds.Height - index2), this.temp);
          this.temp.Draw(new Vector2((float) (bounds.X + index1), (float) (bounds.Y + index2)), Vector2.Zero, color);
        }
      }
    }

    private void DrawInterior(Rectangle bounds)
    {
      Draw.Rect(bounds, Calc.HexToColor("18668f"));
      this.DrawMist(bounds, this.mist);
      this.DrawMist(bounds, new Vector2(this.mist.Y, this.mist.X) * 1.5f);
      Vector2 vector2_1 = (this.Scene as Level).Camera.Position;
      if (this.Opened)
        vector2_1 = Vector2.Zero;
      for (int index = 0; index < this.particles.Length; ++index)
      {
        Vector2 vector2_2 = this.particles[index].Position + vector2_1 * 0.2f;
        vector2_2.X = this.Mod(vector2_2.X, (float) bounds.Width);
        vector2_2.Y = this.Mod(vector2_2.Y, (float) bounds.Height);
        Draw.Pixel.Draw(new Vector2((float) bounds.X, (float) bounds.Y) + vector2_2, Vector2.Zero, this.particles[index].Color);
      }
    }

    private void DrawEdges(Rectangle bounds, Color color)
    {
      MTexture mtexture1 = GFX.Game["objects/heartdoor/edge"];
      MTexture mtexture2 = GFX.Game["objects/heartdoor/top"];
      int height = (int) ((double) this.offset % 8.0);
      if (height > 0)
      {
        mtexture1.GetSubtexture(0, 8 - height, 7, height, this.temp);
        this.temp.DrawJustified(new Vector2((float) (bounds.Left + 4), (float) bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
        this.temp.DrawJustified(new Vector2((float) (bounds.Right - 4), (float) bounds.Top), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
      }
      for (int index = height; index < bounds.Height; index += 8)
      {
        mtexture1.GetSubtexture(0, 0, 8, Math.Min(8, bounds.Height - index), this.temp);
        this.temp.DrawJustified(new Vector2((float) (bounds.Left + 4), (float) (bounds.Top + index)), new Vector2(0.5f, 0.0f), color, new Vector2(-1f, 1f));
        this.temp.DrawJustified(new Vector2((float) (bounds.Right - 4), (float) (bounds.Top + index)), new Vector2(0.5f, 0.0f), color, new Vector2(1f, 1f));
      }
      for (int index = 0; index < bounds.Width; index += 8)
      {
        mtexture2.DrawCentered(new Vector2((float) (bounds.Left + 4 + index), (float) (bounds.Top + 4)), color);
        mtexture2.DrawCentered(new Vector2((float) (bounds.Left + 4 + index), (float) (bounds.Bottom - 4)), color, new Vector2(1f, -1f));
      }
    }

    public override void Render()
    {
      Color color1 = this.Opened ? Color.White * 0.25f : Color.White;
      if (!this.Opened && this.TopSolid.Visible && this.BotSolid.Visible)
      {
        Rectangle bounds = new Rectangle((int) this.TopSolid.X, (int) this.TopSolid.Y, this.Size, (int) ((double) this.TopSolid.Height + (double) this.BotSolid.Height));
        this.DrawInterior(bounds);
        this.DrawEdges(bounds, color1);
      }
      else
      {
        if (this.TopSolid.Visible)
        {
          Rectangle bounds = new Rectangle((int) this.TopSolid.X, (int) this.TopSolid.Y, this.Size, (int) this.TopSolid.Height);
          this.DrawInterior(bounds);
          this.DrawEdges(bounds, color1);
        }
        if (this.BotSolid.Visible)
        {
          Rectangle bounds = new Rectangle((int) this.BotSolid.X, (int) this.BotSolid.Y, this.Size, (int) this.BotSolid.Height);
          this.DrawInterior(bounds);
          this.DrawEdges(bounds, color1);
        }
      }
      if ((double) this.heartAlpha <= 0.0)
        return;
      float num1 = 12f;
      int num2 = (int) ((double) (this.Size - 8) / (double) num1);
      int num3 = (int) Math.Ceiling((double) this.Requires / (double) num2);
      Color color2 = color1 * this.heartAlpha;
      for (int index1 = 0; index1 < num3; ++index1)
      {
        int num4 = (index1 + 1) * num2 < this.Requires ? num2 : this.Requires - index1 * num2;
        Vector2 vector2 = new Vector2(this.X + (float) this.Size * 0.5f, this.Y) + new Vector2((float) ((double) -num4 / 2.0 + 0.5), (float) ((double) -num3 / 2.0 + (double) index1 + 0.5)) * num1;
        if (this.Opened)
        {
          if (index1 < num3 / 2)
            vector2.Y -= this.openAmount + 8f;
          else
            vector2.Y += this.openAmount + 8f;
        }
        for (int index2 = 0; index2 < num4; ++index2)
        {
          int min = index1 * num2 + index2;
          this.icon[(int) ((double) Ease.CubeIn(Calc.ClampedMap(this.Counter, (float) min, (float) min + 1f)) * (double) (this.icon.Count - 1))].DrawCentered(vector2 + new Vector2((float) index2 * num1, 0.0f), color2);
        }
      }
    }

    private float Mod(float x, float m) => (x % m + m) % m;

    private struct Particle
    {
      public Vector2 Position;
      public float Speed;
      public Color Color;
    }

    private class WhiteLine : Entity
    {
      private float fade = 1f;
      private int blockSize;

      public WhiteLine(Vector2 origin, int blockSize)
        : base(origin)
      {
        this.Depth = -1000000;
        this.blockSize = blockSize;
      }

      public override void Update()
      {
        base.Update();
        this.fade = Calc.Approach(this.fade, 0.0f, Engine.DeltaTime);
        if ((double) this.fade > 0.0)
          return;
        this.RemoveSelf();
        Level level = this.SceneAs<Level>();
        for (float left = (float) (int) level.Camera.Left; (double) left < (double) level.Camera.Right; ++left)
        {
          if ((double) left < (double) this.X || (double) left >= (double) this.X + (double) this.blockSize)
            level.Particles.Emit(HeartGemDoor.P_Slice, new Vector2(left, this.Y));
        }
      }

      public override void Render()
      {
        Vector2 position = (this.Scene as Level).Camera.Position;
        float height = Math.Max(1f, 4f * this.fade);
        Draw.Rect(position.X - 10f, this.Y - height / 2f, 340f, height, Color.White);
      }
    }
  }
}
