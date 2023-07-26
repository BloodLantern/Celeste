using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class JumpthruPlatform : JumpThru
    {
        private readonly int columns;
        private readonly string overrideTexture;
        private readonly int overrideSoundIndex = -1;

        public JumpthruPlatform(
            Vector2 position,
            int width,
            string overrideTexture,
            int overrideSoundIndex = -1)
            : base(position, width, true)
        {
            columns = width / 8;
            Depth = -60;
            this.overrideTexture = overrideTexture;
            this.overrideSoundIndex = overrideSoundIndex;
        }

        public JumpthruPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Attr("texture", "default"), data.Int("surfaceIndex", -1))
        {
        }

        public override void Awake(Scene scene)
        {
            string type = AreaData.Get(scene).Jumpthru;
            if (!string.IsNullOrEmpty(overrideTexture) && !overrideTexture.Equals("default"))
                type = overrideTexture;
            if (overrideSoundIndex > 0)
            {
                SurfaceSoundIndex = overrideSoundIndex;
            }
            else
            {
                string lowerType = type.ToLower();
                if (!(lowerType == "dream"))
                {
                    if (lowerType is not "temple" and not "templeb")
                    {
                        if (!(lowerType == "core"))
                        {
                            if (lowerType is "wood" or "cliffside")
                                SurfaceSoundIndex = 5;
                        }
                        else
                            SurfaceSoundIndex = 3;
                    }
                    else
                        SurfaceSoundIndex = 8;
                }
                else
                    SurfaceSoundIndex = 32;
            }

            MTexture texture = GFX.Game["objects/jumpthru/" + type];
            int partWidth = texture.Width / 8;
            for (int x = 0; x < columns; x++)
            {
                int offsetX;
                int offsetY;

                // Left
                if (x == 0)
                {
                    offsetX = 0;
                    offsetY = CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(-1f, 0f)) ? 0 : 1;
                }
                // Right
                else if (x == columns - 1)
                {
                    offsetX = partWidth - 1;
                    offsetY = CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(1f, 0f)) ? 0 : 1;
                }
                // Middle
                else
                {
                    offsetX = 1 + Calc.Random.Next(partWidth - 2);
                    offsetY = Calc.Random.Choose(0, 1);
                }

                Add(
                    new Image(texture.GetSubtexture(offsetX * 8, offsetY * 8, 8, 8))
                    {
                        X = x * 8
                    }
                );
            }
        }
    }
}
