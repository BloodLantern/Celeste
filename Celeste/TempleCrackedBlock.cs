// Decompiled with JetBrains decompiler
// Type: Celeste.TempleCrackedBlock
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class TempleCrackedBlock : Solid
    {
        private EntityID eid;
        private readonly bool persistent;
        private readonly MTexture[,,] tiles;
        private float frame;
        private bool broken;
        private readonly int frames;

        public TempleCrackedBlock(
            EntityID eid,
            Vector2 position,
            float width,
            float height,
            bool persistent)
            : base(position, width, height, true)
        {
            this.eid = eid;
            this.persistent = persistent;
            Collidable = Visible = false;
            int length1 = (int)((double)width / 8.0);
            int length2 = (int)((double)height / 8.0);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures("objects/temple/breakBlock");
            tiles = new MTexture[length1, length2, atlasSubtextures.Count];
            frames = atlasSubtextures.Count;
            for (int index1 = 0; index1 < length1; ++index1)
            {
                for (int index2 = 0; index2 < length2; ++index2)
                {
                    int num1 = index1 >= length1 / 2 || index1 >= 2 ? (index1 < length1 / 2 || index1 < length1 - 2 ? 2 + (index1 % 2) : 5 - (length1 - index1 - 1)) : index1;
                    int num2 = index2 >= length2 / 2 || index2 >= 2 ? (index2 < length2 / 2 || index2 < length2 - 2 ? 2 + (index2 % 2) : 5 - (length2 - index2 - 1)) : index2;
                    for (int index3 = 0; index3 < atlasSubtextures.Count; ++index3)
                    {
                        tiles[index1, index2, index3] = atlasSubtextures[index3].GetSubtexture(num1 * 8, num2 * 8, 8, 8);
                    }
                }
            }
            Add(new LightOcclude(0.5f));
        }

        public TempleCrackedBlock(EntityID eid, EntityData data, Vector2 offset)
            : this(eid, data.Position + offset, data.Width, data.Height, data.Bool(nameof(persistent)))
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (CollideCheck<Player>())
            {
                if (persistent)
                {
                    _ = SceneAs<Level>().Session.DoNotLoad.Add(eid);
                }

                RemoveSelf();
            }
            else
            {
                Collidable = Visible = true;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!broken)
            {
                return;
            }

            frame += Engine.DeltaTime * 15f;
            if (frame < (double)frames)
            {
                return;
            }

            RemoveSelf();
        }

        public override void Render()
        {
            int frame = (int)this.frame;
            if (frame >= frames)
            {
                return;
            }

            for (int x = 0; x < (double)Width / 8.0; ++x)
            {
                for (int y = 0; y < (double)Height / 8.0; ++y)
                {
                    tiles[x, y, frame].Draw(Position + (new Vector2(x, y) * 8f));
                }
            }
        }

        public void Break(Vector2 from)
        {
            if (persistent)
            {
                _ = SceneAs<Level>().Session.DoNotLoad.Add(eid);
            }

            _ = Audio.Play("event:/game/05_mirror_temple/crackedwall_vanish", Center);
            broken = true;
            Collidable = false;
            for (int index1 = 0; index1 < (double)Width / 8.0; ++index1)
            {
                for (int index2 = 0; index2 < (double)Height / 8.0; ++index2)
                {
                    Scene.Add(Engine.Pooler.Create<Debris>().Init(Position + new Vector2((index1 * 8) + 4, (index2 * 8) + 4), '1').BlastFrom(from));
                }
            }
        }
    }
}
