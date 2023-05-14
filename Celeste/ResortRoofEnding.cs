// Decompiled with JetBrains decompiler
// Type: Celeste.ResortRoofEnding
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
    public class ResortRoofEnding : Solid
    {
        private readonly MTexture[] roofCenters = new MTexture[4]
        {
            GFX.Game["decals/3-resort/roofCenter"],
            GFX.Game["decals/3-resort/roofCenter_b"],
            GFX.Game["decals/3-resort/roofCenter_c"],
            GFX.Game["decals/3-resort/roofCenter_d"]
        };
        private readonly List<Monocle.Image> images = new();
        private readonly List<Coroutine> wobbleRoutines = new();
        public bool BeginFalling;

        public ResortRoofEnding(EntityData data, Vector2 offset)
            : base(data.Position + offset, data.Width, 2f, true)
        {
            EnableAssistModeChecks = false;
            Monocle.Image image1 = new(GFX.Game["decals/3-resort/roofEdge_d"]);
            _ = image1.CenterOrigin();
            image1.X = 8f;
            image1.Y = 4f;
            Add(image1);
            int num;
            for (num = 0; num < (double)Width; num += 16)
            {
                Monocle.Image image2 = new(Calc.Random.Choose<MTexture>(roofCenters));
                _ = image2.CenterOrigin();
                image2.X = num + 8;
                image2.Y = 4f;
                Add(image2);
                images.Add(image2);
            }
            Monocle.Image image3 = new(GFX.Game["decals/3-resort/roofEdge"]);
            _ = image3.CenterOrigin();
            image3.X = num + 8;
            image3.Y = 4f;
            Add(image3);
            images.Add(image3);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((Scene as Level).Session.GetFlag("oshiroEnding"))
            {
                return;
            }

            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }

            Scene.Add(new CS03_Ending(this, entity));
        }

        public override void Render()
        {
            Position += Shake;
            base.Render();
            Position -= Shake;
        }

        public void Wobble(AngryOshiro ghost, bool fall = false)
        {
            foreach (Component wobbleRoutine in wobbleRoutines)
            {
                wobbleRoutine.RemoveSelf();
            }

            wobbleRoutines.Clear();
            Player entity = Scene.Tracker.GetEntity<Player>();
            foreach (Monocle.Image image in images)
            {
                Coroutine coroutine = new(WobbleImage(image, Math.Abs(X + image.X - ghost.X) * (1f / 1000f), entity, fall));
                Add(coroutine);
                wobbleRoutines.Add(coroutine);
            }
        }

        private IEnumerator WobbleImage(Monocle.Image img, float delay, Player player, bool fall)
        {
            ResortRoofEnding resortRoofEnding = this;
            float orig = img.Y;
            yield return delay;
            for (int index = 0; index < 2; ++index)
            {
                resortRoofEnding.Scene.Add(Engine.Pooler.Create<Debris>().Init(resortRoofEnding.Position + img.Position + new Vector2((index * 8) - 4, Calc.Random.Range(0, 8)), '9'));
            }

            float p1;
            float amount;
            if (!fall)
            {
                p1 = 0.0f;
                amount = 5f;
                while (true)
                {
                    p1 += Engine.DeltaTime * 16f;
                    amount = Calc.Approach(amount, 1f, Engine.DeltaTime * 5f);
                    float num = (float)Math.Sin((double)p1) * amount;
                    img.Y = orig + num;
                    if (player != null && (double)Math.Abs(resortRoofEnding.X + img.X - player.X) < 16.0)
                    {
                        player.Sprite.Y = num;
                    }

                    yield return null;
                }
            }
            else
            {
                if (fall)
                {
                    while (!resortRoofEnding.BeginFalling)
                    {
                        int num = Calc.Random.Range(0, 2);
                        img.Y = orig + num;
                        if (player != null && (double)Math.Abs(resortRoofEnding.X + img.X - player.X) < 16.0)
                        {
                            player.Sprite.Y = num;
                        }

                        yield return 0.01f;
                    }
                    img.Texture = GFX.Game["decals/3-resort/roofCenter_snapped_" + Calc.Random.Choose<string>("a", "b", "c")];
                    resortRoofEnding.Collidable = false;
                    amount = Calc.Random.NextFloat();
                    p1 = Calc.Random.NextFloat(48f) - 24f;
                    float speedY = (float)-(80.0 + (double)Calc.Random.NextFloat(80f));
                    float up = new Vector2(0.0f, -1f).Angle();
                    float off = Calc.Random.NextFloat();
                    for (float p2 = 0.0f; (double)p2 < 4.0; p2 += Engine.DeltaTime)
                    {
                        Monocle.Image image = img;
                        image.Position += new Vector2(p1, speedY) * Engine.DeltaTime;
                        img.Rotation += amount * Ease.CubeIn(p2);
                        p1 = Calc.Approach(p1, 0.0f, Engine.DeltaTime * 200f);
                        speedY += 600f * Engine.DeltaTime;
                        if (resortRoofEnding.Scene.OnInterval(0.1f, off))
                        {
                            Dust.Burst(resortRoofEnding.Position + img.Position, up);
                        }

                        yield return null;
                    }
                }
                player.Sprite.Y = 0.0f;
            }
        }
    }
}
