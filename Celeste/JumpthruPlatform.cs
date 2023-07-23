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
            MTexture mtexture = GFX.Game["objects/jumpthru/" + type];
            int num1 = mtexture.Width / 8;
            for (int index = 0; index < columns; ++index)
            {
                int num2;
                int num3;
                if (index == 0)
                {
                    num2 = 0;
                    num3 = CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(-1f, 0.0f)) ? 0 : 1;
                }
                else if (index == columns - 1)
                {
                    num2 = num1 - 1;
                    num3 = CollideCheck<Solid, SwapBlock, ExitBlock>(Position + new Vector2(1f, 0.0f)) ? 0 : 1;
                }
                else
                {
                    num2 = 1 + Calc.Random.Next(num1 - 2);
                    num3 = Calc.Random.Choose<int>(0, 1);
                }
                Image image = new(mtexture.GetSubtexture(num2 * 8, num3 * 8, 8, 8))
                {
                    X = index * 8
                };
                Add(image);
            }
        }
    }
}
