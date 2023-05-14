// Decompiled with JetBrains decompiler
// Type: Celeste.HeartGemDisplay
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    public class HeartGemDisplay : Component
    {
        public Vector2 Position;
        public Sprite[] Sprites;
        public Vector2 TargetPosition;
        private readonly Monocle.Image bg;
        private readonly Wiggler rotateWiggler;
        private Coroutine routine;
        private Vector2 bounce;
        private readonly Tween tween;

        public HeartGemDisplay(int heartgem, bool hasGem)
            : base(true, true)
        {
            Sprites = new Sprite[3];
            for (int index = 0; index < Sprites.Length; ++index)
            {
                Sprites[index] = GFX.GuiSpriteBank.Create(nameof(heartgem) + index);
                Sprites[index].Visible = heartgem == index & hasGem;
                Sprites[index].Play("spin");
            }
            bg = new Monocle.Image(GFX.Gui["collectables/heartgem/0/spin00"])
            {
                Color = Color.Black
            };
            _ = bg.CenterOrigin();
            rotateWiggler = Wiggler.Create(0.4f, 6f);
            rotateWiggler.UseRawDeltaTime = true;
            SimpleCurve curve = new(Vector2.UnitY * 80f, Vector2.Zero, Vector2.UnitY * -160f);
            tween = Tween.Create(Tween.TweenMode.Oneshot, duration: 0.4f);
            tween.OnStart = t => SpriteColor = Color.Transparent;
            tween.OnUpdate = t =>
            {
                bounce = curve.GetPoint(t.Eased);
                SpriteColor = Color.White * Calc.LerpClamp(0.0f, 1f, t.Percent * 1.5f);
            };
        }

        private Color SpriteColor
        {
            get => Sprites[0].Color;
            set
            {
                for (int index = 0; index < Sprites.Length; ++index)
                {
                    Sprites[index].Color = value;
                }
            }
        }

        public void Wiggle()
        {
            rotateWiggler.Start();
            for (int index = 0; index < Sprites.Length; ++index)
            {
                if (Sprites[index].Visible)
                {
                    Sprites[index].Play("spin", true);
                    Sprites[index].SetAnimationFrame(19);
                }
            }
        }

        public void Appear(AreaMode mode)
        {
            tween.Start();
            routine = new Coroutine(AppearSequence(Sprites[(int)mode]))
            {
                UseRawDeltaTime = true
            };
        }

        public void SetCurrentMode(AreaMode mode, bool has)
        {
            for (int index = 0; index < Sprites.Length; ++index)
            {
                Sprites[index].Visible = (AreaMode)index == mode & has;
            }

            if (has)
            {
                return;
            }

            routine = null;
        }

        public override void Update()
        {
            base.Update();
            if (routine != null && routine.Active)
            {
                routine.Update();
            }

            if (rotateWiggler.Active)
            {
                rotateWiggler.Update();
            }

            for (int index = 0; index < Sprites.Length; ++index)
            {
                if (Sprites[index].Active)
                {
                    Sprites[index].Update();
                }
            }
            if (tween != null && tween.Active)
            {
                tween.Update();
            }

            Position = Calc.Approach(Position, TargetPosition, 200f * Engine.DeltaTime);
            for (int index = 0; index < Sprites.Length; ++index)
            {
                Sprites[index].Scale.X = Calc.Approach(Sprites[index].Scale.X, 1f, 2f * Engine.DeltaTime);
                Sprites[index].Scale.Y = Calc.Approach(Sprites[index].Scale.Y, 1f, 2f * Engine.DeltaTime);
            }
        }

        public override void Render()
        {
            base.Render();
            bg.Position = Entity.Position + Position;
            for (int index = 0; index < Sprites.Length; ++index)
            {
                if (Sprites[index].Visible)
                {
                    Sprites[index].Rotation = (float)((double)rotateWiggler.Value * 30.0 * (Math.PI / 180.0));
                    Sprites[index].Position = Entity.Position + Position + bounce;
                    Sprites[index].Render();
                }
            }
        }

        private IEnumerator AppearSequence(Sprite sprite)
        {
            sprite.Play("idle");
            sprite.Visible = true;
            sprite.Scale = new Vector2(0.8f, 1.4f);
            yield return tween.Wait();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            sprite.Scale = new Vector2(1.4f, 0.8f);
            yield return 0.4f;
            _ = sprite.CenterOrigin();
            rotateWiggler.Start();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Medium);
            sprite.Play("spin");
            routine = null;
        }
    }
}
