// Decompiled with JetBrains decompiler
// Type: Celeste.SummitGemManager
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;

namespace Celeste
{
    public class SummitGemManager : Entity
    {
        private readonly List<SummitGemManager.Gem> gems = new();

        public SummitGemManager(EntityData data, Vector2 offset)
            : base(data.Position + offset)
        {
            Depth = -10010;
            int index = 0;
            foreach (Vector2 position in data.NodesOffset(offset))
            {
                gems.Add(new SummitGemManager.Gem(index, position));
                ++index;
            }
            Add(new Coroutine(Routine()));
        }

        public override void Awake(Scene scene)
        {
            foreach (SummitGemManager.Gem gem in gems)
            {
                scene.Add(gem);
            }

            base.Awake(scene);
        }

        private IEnumerator Routine()
        {
            SummitGemManager summitGemManager = this;
            Level level = summitGemManager.Scene as Level;
            if (level.Session.HeartGem)
            {
                foreach (SummitGemManager.Gem gem in summitGemManager.gems)
                {
                    gem.Sprite.RemoveSelf();
                }

                summitGemManager.gems.Clear();
            }
            else
            {
                while (true)
                {
                    Player entity = summitGemManager.Scene.Tracker.GetEntity<Player>();
                    if (entity == null || (double)(entity.Position - summitGemManager.Position).Length() >= 64.0)
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
                yield return 0.5f;
                bool alreadyHasHeart = level.Session.OldStats.Modes[0].HeartGem;
                int broken = 0;
                int index = 0;
                foreach (SummitGemManager.Gem gem in summitGemManager.gems)
                {
                    bool flag = level.Session.SummitGems[index];
                    if (!alreadyHasHeart)
                    {
                        flag = ((flag ? 1 : 0) | (SaveData.Instance.SummitGems == null ? 0 : (SaveData.Instance.SummitGems[index] ? 1 : 0))) != 0;
                    }

                    if (flag)
                    {
                        switch (index)
                        {
                            case 0:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_1", gem.Position);
                                break;
                            case 1:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_2", gem.Position);
                                break;
                            case 2:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_3", gem.Position);
                                break;
                            case 3:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_4", gem.Position);
                                break;
                            case 4:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_5", gem.Position);
                                break;
                            case 5:
                                _ = Audio.Play("event:/game/07_summit/gem_unlock_6", gem.Position);
                                break;
                        }
                        gem.Sprite.Play("spin");
                        while (gem.Sprite.CurrentAnimationID == "spin")
                        {
                            gem.Bloom.Alpha = Calc.Approach(gem.Bloom.Alpha, 1f, Engine.DeltaTime * 3f);
                            if (gem.Bloom.Alpha > 0.5)
                            {
                                gem.Shake = Calc.Random.ShakeVector();
                            }

                            gem.Sprite.Y -= Engine.DeltaTime * 8f;
                            gem.Sprite.Scale = Vector2.One * (float)(1.0 + (gem.Bloom.Alpha * 0.10000000149011612));
                            yield return null;
                        }
                        yield return 0.2f;
                        level.Shake();
                        Input.Rumble(RumbleStrength.Light, RumbleLength.Short);
                        for (int index1 = 0; index1 < 20; ++index1)
                        {
                            level.ParticlesFG.Emit(SummitGem.P_Shatter, gem.Position + new Vector2(Calc.Random.Range(-8, 8), Calc.Random.Range(-8, 8)), SummitGem.GemColors[index], Calc.Random.NextFloat(6.28318548f));
                        }

                        ++broken;
                        gem.Bloom.RemoveSelf();
                        gem.Sprite.RemoveSelf();
                        yield return 0.25f;
                    }
                    ++index;
                }
                if (broken >= 6)
                {
                    HeartGem heart = summitGemManager.Scene.Entities.FindFirst<HeartGem>();
                    if (heart != null)
                    {
                        _ = Audio.Play("event:/game/07_summit/gem_unlock_complete", heart.Position);
                        yield return 0.1f;
                        Vector2 from = heart.Position;
                        for (float p = 0.0f; (double)p < 1.0 && heart.Scene != null; p += Engine.DeltaTime)
                        {
                            heart.Position = Vector2.Lerp(from, summitGemManager.Position + new Vector2(0.0f, -16f), Ease.CubeOut(p));
                            yield return null;
                        }
                        _ = new Vector2();
                    }
                }
            }
        }

        private class Gem : Entity
        {
            public Vector2 Shake;
            public Sprite Sprite;
            public Monocle.Image Bg;
            public BloomPoint Bloom;

            public Gem(int index, Vector2 position)
                : base(position)
            {
                Depth = -10010;
                Add(Bg = new Monocle.Image(GFX.Game["collectables/summitgems/" + index + "/bg"]));
                Add(Sprite = new Sprite(GFX.Game, "collectables/summitgems/" + index + "/gem"));
                Add(Bloom = new BloomPoint(0.0f, 20f));
                Sprite.AddLoop("idle", "", 0.05f, new int[1]);
                Sprite.Add("spin", "", 0.05f, "idle");
                Sprite.Play("idle");
                _ = Sprite.CenterOrigin();
                _ = Bg.CenterOrigin();
            }

            public override void Update()
            {
                Bloom.Position = Sprite.Position;
                base.Update();
            }

            public override void Render()
            {
                Vector2 position = Sprite.Position;
                Sprite sprite = Sprite;
                sprite.Position += Shake;
                base.Render();
                Sprite.Position = position;
            }
        }
    }
}
