// Decompiled with JetBrains decompiler
// Type: Celeste.TempleBigEyeballShockwave
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
  [Pooled]
  public class TempleBigEyeballShockwave : Entity
  {
    private MTexture distortionTexture;
    private float distortionAlpha;
    private bool hasHitPlayer;

    public TempleBigEyeballShockwave()
    {
      this.Depth = -1000000;
      this.Collider = (Collider) new Hitbox(48f, 200f, -30f, -100f);
      this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
      MTexture mtexture = GFX.Game["util/displacementcirclehollow"];
      this.distortionTexture = mtexture.GetSubtexture(0, 0, mtexture.Width / 2, mtexture.Height);
      this.Add((Component) new DisplacementRenderHook(new Action(this.RenderDisplacement)));
    }

    public TempleBigEyeballShockwave Init(Vector2 position)
    {
      this.Position = position;
      this.Collidable = true;
      this.distortionAlpha = 0.0f;
      this.hasHitPlayer = false;
      return this;
    }

    public override void Update()
    {
      base.Update();
      this.X -= 300f * Engine.DeltaTime;
      this.distortionAlpha = Calc.Approach(this.distortionAlpha, 1f, Engine.DeltaTime * 4f);
      if ((double) this.X >= (double) (this.SceneAs<Level>().Bounds.Left - 20))
        return;
      this.RemoveSelf();
    }

    private void RenderDisplacement() => this.distortionTexture.DrawCentered(this.Position, Color.White * 0.8f * this.distortionAlpha, new Vector2(0.9f, 1.5f));

    private void OnPlayer(Player player)
    {
      if (player.StateMachine.State == 2)
        return;
      player.Speed.X = -100f;
      if ((double) player.Speed.Y > 30.0)
        player.Speed.Y = 30f;
      if (this.hasHitPlayer)
        return;
      Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
      Audio.Play("event:/game/05_mirror_temple/eye_pulse", player.Position);
      this.hasHitPlayer = true;
    }
  }
}
