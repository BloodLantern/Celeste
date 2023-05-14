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
        private readonly MTexture distortionTexture;
        private float distortionAlpha;
        private bool hasHitPlayer;

        public TempleBigEyeballShockwave()
        {
            Depth = -1000000;
            Collider = new Hitbox(48f, 200f, -30f, -100f);
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            MTexture mtexture = GFX.Game["util/displacementcirclehollow"];
            distortionTexture = mtexture.GetSubtexture(0, 0, mtexture.Width / 2, mtexture.Height);
            Add(new DisplacementRenderHook(new Action(RenderDisplacement)));
        }

        public TempleBigEyeballShockwave Init(Vector2 position)
        {
            Position = position;
            Collidable = true;
            distortionAlpha = 0.0f;
            hasHitPlayer = false;
            return this;
        }

        public override void Update()
        {
            base.Update();
            X -= 300f * Engine.DeltaTime;
            distortionAlpha = Calc.Approach(distortionAlpha, 1f, Engine.DeltaTime * 4f);
            if ((double)X >= SceneAs<Level>().Bounds.Left - 20)
            {
                return;
            }

            RemoveSelf();
        }

        private void RenderDisplacement()
        {
            distortionTexture.DrawCentered(Position, Color.White * 0.8f * distortionAlpha, new Vector2(0.9f, 1.5f));
        }

        private void OnPlayer(Player player)
        {
            if (player.StateMachine.State == 2)
            {
                return;
            }

            player.Speed.X = -100f;
            if (player.Speed.Y > 30.0)
            {
                player.Speed.Y = 30f;
            }

            if (hasHitPlayer)
            {
                return;
            }

            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            _ = Audio.Play("event:/game/05_mirror_temple/eye_pulse", player.Position);
            hasHitPlayer = true;
        }
    }
}
