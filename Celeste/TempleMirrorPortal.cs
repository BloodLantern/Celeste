using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class TempleMirrorPortal : Entity
    {
        public static ParticleType P_CurtainDrop;
        public float DistortionFade = 1f;
        private bool canTrigger;
        private int switchCounter;
        private VirtualRenderTarget buffer;
        private float bufferAlpha;
        private float bufferTimer;
        private Debris[] debris = new Debris[50];
        private Color debrisColorFrom = Calc.HexToColor("f442d4");
        private Color debrisColorTo = Calc.HexToColor("000000");
        private MTexture debrisTexture = GFX.Game["particles/blob"];
        private Curtain curtain;
        private TemplePortalTorch leftTorch;
        private TemplePortalTorch rightTorch;

        public TempleMirrorPortal(Vector2 position)
            : base(position)
        {
            Depth = 2000;
            Collider = new Hitbox(120f, 64f, -60f, -32f);
            Add(new PlayerCollider(OnPlayer));
        }

        public TempleMirrorPortal(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            scene.Add(curtain = new Curtain(Position));
            scene.Add(new Bg(Position));
            scene.Add(leftTorch = new TemplePortalTorch(Position + new Vector2(-90f, 0.0f)));
            scene.Add(rightTorch = new TemplePortalTorch(Position + new Vector2(90f, 0.0f)));
        }

        public void OnSwitchHit(int side) => Add(new Coroutine(OnSwitchRoutine(side)));

        private IEnumerator OnSwitchRoutine(int side)
        {
            TempleMirrorPortal templeMirrorPortal = this;
            yield return 0.4f;
            if (side < 0)
                templeMirrorPortal.leftTorch.Light(templeMirrorPortal.switchCounter);
            else
                templeMirrorPortal.rightTorch.Light(templeMirrorPortal.switchCounter);
            ++templeMirrorPortal.switchCounter;
            if ((templeMirrorPortal.Scene as Level).Session.Area.Mode == AreaMode.Normal)
            {
                LightingRenderer lighting = (templeMirrorPortal.Scene as Level).Lighting;
                float lightTarget = Math.Max(0.0f, lighting.Alpha - 0.2f);
                while ((lighting.Alpha -= Engine.DeltaTime) > (double) lightTarget)
                    yield return null;
                lighting = null;
            }
            yield return 0.15f;
            if (templeMirrorPortal.switchCounter >= 2)
            {
                yield return 0.1f;
                Audio.Play("event:/game/05_mirror_temple/mainmirror_reveal", templeMirrorPortal.Position);
                templeMirrorPortal.curtain.Drop();
                templeMirrorPortal.canTrigger = true;
                yield return 0.1f;
                Level level = templeMirrorPortal.SceneAs<Level>();
                for (int index1 = 0; index1 < 120; index1 += 12)
                {
                    for (int index2 = 0; index2 < 60; index2 += 6)
                        level.Particles.Emit(TempleMirrorPortal.P_CurtainDrop, 1, templeMirrorPortal.curtain.Position + new Vector2(index1 - 57, index2 - 27), new Vector2(6f, 3f));
                }
            }
        }

        public void Activate() => Add(new Coroutine(ActivateRoutine()));

        private IEnumerator ActivateRoutine()
        {
            TempleMirrorPortal templeMirrorPortal = this;
            LightingRenderer light = (templeMirrorPortal.Scene as Level).Lighting;
            float debrisStart = 0.0f;
            templeMirrorPortal.Add(new BeforeRenderHook(templeMirrorPortal.BeforeRender));
            templeMirrorPortal.Add(new DisplacementRenderHook(templeMirrorPortal.RenderDisplacement));
            while (true)
            {
                templeMirrorPortal.bufferAlpha = Calc.Approach(templeMirrorPortal.bufferAlpha, 1f, Engine.DeltaTime);
                templeMirrorPortal.bufferTimer += 4f * Engine.DeltaTime;
                light.Alpha = Calc.Approach(light.Alpha, 0.2f, Engine.DeltaTime * 0.25f);
                if (debrisStart < (double) templeMirrorPortal.debris.Length)
                {
                    int index = (int) debrisStart;
                    templeMirrorPortal.debris[index].Direction = Calc.AngleToVector(Calc.Random.NextFloat(6.28318548f), 1f);
                    templeMirrorPortal.debris[index].Enabled = true;
                    templeMirrorPortal.debris[index].Duration = 0.5f + Calc.Random.NextFloat(0.7f);
                }
                debrisStart += Engine.DeltaTime * 10f;
                for (int index = 0; index < templeMirrorPortal.debris.Length; ++index)
                {
                    if (templeMirrorPortal.debris[index].Enabled)
                    {
                        templeMirrorPortal.debris[index].Percent %= 1f;
                        templeMirrorPortal.debris[index].Percent += Engine.DeltaTime / templeMirrorPortal.debris[index].Duration;
                    }
                }
                yield return null;
            }
        }

        private void BeforeRender()
        {
            if (buffer == null)
                buffer = VirtualContent.CreateRenderTarget("temple-portal", 120, 64);
            Vector2 position = new Vector2(buffer.Width, buffer.Height) / 2f;
            MTexture mtexture = GFX.Game["objects/temple/portal/portal"];
            Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
            Engine.Graphics.GraphicsDevice.Clear(Color.Black);
            Draw.SpriteBatch.Begin();
            for (int index = 0; index < 10.0; ++index)
            {
                float amount = (float) (bufferTimer % 1.0 * 0.10000000149011612 + index / 10.0);
                Color color = Color.Lerp(Color.Black, Color.Purple, amount);
                float scale = amount;
                float rotation = 6.28318548f * amount;
                mtexture.DrawCentered(position, color, scale, rotation);
            }
            Draw.SpriteBatch.End();
        }

        private void RenderDisplacement() => Draw.Rect(X - 60f, Y - 32f, 120f, 64f, new Color(0.5f, 0.5f, 0.25f * DistortionFade * bufferAlpha, 1f));

        public override void Render()
        {
            base.Render();
            if (buffer != null)
                Draw.SpriteBatch.Draw((RenderTarget2D) buffer, Position + new Vector2((float) (-(double) Collider.Width / 2.0), (float) (-(double) Collider.Height / 2.0)), Color.White * bufferAlpha);
            GFX.Game["objects/temple/portal/portalframe"].DrawCentered(Position);
            Level scene = Scene as Level;
            for (int index = 0; index < debris.Length; ++index)
            {
                Debris debri = debris[index];
                if (debri.Enabled)
                {
                    float num = Ease.SineOut(debri.Percent);
                    debrisTexture.DrawCentered(Position + debri.Direction * (1f - num) * (float) (190.0 - scene.Zoom * 30.0), Color.Lerp(debrisColorFrom, debrisColorTo, num), Calc.LerpClamp(1f, 0.2f, num), index * 0.05f);
                }
            }
        }

        private void OnPlayer(Player player)
        {
            if (!canTrigger)
                return;
            canTrigger = false;
            Scene.Add(new CS04_MirrorPortal(player, this));
        }

        public override void Removed(Scene scene)
        {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (buffer != null)
                buffer.Dispose();
            buffer = null;
        }

        private struct Debris
        {
            public Vector2 Direction;
            public float Percent;
            public float Duration;
            public bool Enabled;
        }

        private class Bg : Entity
        {
            private MirrorSurface surface;
            private Vector2[] offsets;
            private List<MTexture> textures;

            public Bg(Vector2 position)
                : base(position)
            {
                Depth = 9500;
                textures = GFX.Game.GetAtlasSubtextures("objects/temple/portal/reflection");
                Vector2 vector2 = new Vector2(10f, 4f);
                offsets = new Vector2[textures.Count];
                for (int index = 0; index < offsets.Length; ++index)
                    offsets[index] = vector2 + new Vector2(Calc.Random.Range(-4, 4), Calc.Random.Range(-4, 4));
                Add(surface = new MirrorSurface());
                surface.OnRender = () =>
                {
                    for (int index = 0; index < textures.Count; ++index)
                    {
                        surface.ReflectionOffset = offsets[index];
                        textures[index].DrawCentered(Position, surface.ReflectionColor);
                    }
                };
            }

            public override void Render() => GFX.Game["objects/temple/portal/surface"].DrawCentered(Position);
        }

        private class Curtain : Solid
        {
            public Sprite Sprite;

            public Curtain(Vector2 position)
                : base(position, 140f, 12f, true)
            {
                Add(Sprite = GFX.SpriteBank.Create("temple_portal_curtain"));
                Depth = 1999;
                Collider.Position.X = -70f;
                Collider.Position.Y = 33f;
                Collidable = false;
                SurfaceSoundIndex = 17;
            }

            public override void Update()
            {
                base.Update();
                if (!Collidable)
                    return;
                Player player1;
                if ((player1 = CollideFirst<Player>(Position + new Vector2(-1f, 0.0f))) != null && player1.OnGround() && Input.Aim.Value.X > 0.0)
                {
                    player1.MoveV(Top - player1.Bottom);
                    player1.MoveH(1f);
                }
                else
                {
                    Player player2;
                    if ((player2 = CollideFirst<Player>(Position + new Vector2(1f, 0.0f))) == null || !player2.OnGround() || Input.Aim.Value.X >= 0.0)
                        return;
                    player2.MoveV(Top - player2.Bottom);
                    player2.MoveH(-1f);
                }
            }

            public void Drop()
            {
                Sprite.Play("fall");
                Depth = -8999;
                Collidable = true;
                bool flag = false;
                Player player;
                while ((player = CollideFirst<Player>(Position)) != null && !flag)
                {
                    Collidable = false;
                    flag = player.MoveV(-1f);
                    Collidable = true;
                }
            }
        }
    }
}
