using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class StarJumpBlock : Solid
    {
        private Level level;
        private bool sinks;
        private float startY;
        private float yLerp;
        private float sinkTimer;

        public StarJumpBlock(Vector2 position, float width, float height, bool sinks)
            : base(position, width, height, false)
        {
            Depth = -10000;
            this.sinks = sinks;
            Add(new LightOcclude());
            startY = Y;
            SurfaceSoundIndex = 32;
        }

        public StarJumpBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Height, data.Bool(nameof (sinks)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            level = SceneAs<Level>();
            List<MTexture> atlasSubtextures1 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/leftrailing");
            List<MTexture> atlasSubtextures2 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/railing");
            List<MTexture> atlasSubtextures3 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/rightrailing");
            List<MTexture> atlasSubtextures4 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeH");
            for (int x = 8; x < Width - 8.0; x += 8)
            {
                if (Open(x, -8f))
                {
                    Image image1 = new Image(Calc.Random.Choose(atlasSubtextures4));
                    image1.CenterOrigin();
                    image1.Position = new Vector2(x + 4, 4f);
                    Add(image1);
                    Image image2 = new Image(atlasSubtextures2[mod((int) (X + (double) x) / 8, atlasSubtextures2.Count)]);
                    image2.Position = new Vector2(x, -8f);
                    Add(image2);
                }
                if (Open(x, Height))
                {
                    Image image = new Image(Calc.Random.Choose(atlasSubtextures4));
                    image.CenterOrigin();
                    image.Scale.Y = -1f;
                    image.Position = new Vector2(x + 4, Height - 4f);
                    Add(image);
                }
            }
            List<MTexture> atlasSubtextures5 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeV");
            for (int y = 8; y < Height - 8.0; y += 8)
            {
                if (Open(-8f, y))
                {
                    Image image = new Image(Calc.Random.Choose(atlasSubtextures5));
                    image.CenterOrigin();
                    image.Scale.X = -1f;
                    image.Position = new Vector2(4f, y + 4);
                    Add(image);
                }
                if (Open(Width, y))
                {
                    Image image = new Image(Calc.Random.Choose(atlasSubtextures5));
                    image.CenterOrigin();
                    image.Position = new Vector2(Width - 4f, y + 4);
                    Add(image);
                }
            }
            List<MTexture> atlasSubtextures6 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/corner");
            Image image3 = null;
            if (Open(-8f, 0.0f) && Open(0.0f, -8f))
            {
                image3 = new Image(Calc.Random.Choose(atlasSubtextures6));
                image3.Scale.X = -1f;
                Image image4 = new Image(atlasSubtextures1[mod((int) X / 8, atlasSubtextures1.Count)]);
                image4.Position = new Vector2(0.0f, -8f);
                Add(image4);
            }
            else if (Open(-8f, 0.0f))
            {
                image3 = new Image(Calc.Random.Choose(atlasSubtextures5));
                image3.Scale.X = -1f;
            }
            else if (Open(0.0f, -8f))
            {
                image3 = new Image(Calc.Random.Choose(atlasSubtextures4));
                Image image5 = new Image(atlasSubtextures2[mod((int) X / 8, atlasSubtextures2.Count)]);
                image5.Position = new Vector2(0.0f, -8f);
                Add(image5);
            }
            image3.CenterOrigin();
            image3.Position = new Vector2(4f, 4f);
            Add(image3);
            Image image6 = null;
            if (Open(Width, 0.0f) && Open(Width - 8f, -8f))
            {
                image6 = new Image(Calc.Random.Choose(atlasSubtextures6));
                Image image7 = new Image(atlasSubtextures3[mod((int) (X + (double) Width) / 8 - 1, atlasSubtextures3.Count)]);
                image7.Position = new Vector2(Width - 8f, -8f);
                Add(image7);
            }
            else if (Open(Width, 0.0f))
                image6 = new Image(Calc.Random.Choose(atlasSubtextures5));
            else if (Open(Width - 8f, -8f))
            {
                image6 = new Image(Calc.Random.Choose(atlasSubtextures4));
                Image image8 = new Image(atlasSubtextures2[mod((int) (X + (double) Width) / 8 - 1, atlasSubtextures2.Count)]);
                image8.Position = new Vector2(Width - 8f, -8f);
                Add(image8);
            }
            image6.CenterOrigin();
            image6.Position = new Vector2(Width - 4f, 4f);
            Add(image6);
            Image image9 = null;
            if (Open(-8f, Height - 8f) && Open(0.0f, Height))
            {
                image9 = new Image(Calc.Random.Choose(atlasSubtextures6));
                image9.Scale.X = -1f;
            }
            else if (Open(-8f, Height - 8f))
            {
                image9 = new Image(Calc.Random.Choose(atlasSubtextures5));
                image9.Scale.X = -1f;
            }
            else if (Open(0.0f, Height))
                image9 = new Image(Calc.Random.Choose(atlasSubtextures4));
            image9.Scale.Y = -1f;
            image9.CenterOrigin();
            image9.Position = new Vector2(4f, Height - 4f);
            Add(image9);
            Image image10 = null;
            if (Open(Width, Height - 8f) && Open(Width - 8f, Height))
                image10 = new Image(Calc.Random.Choose(atlasSubtextures6));
            else if (Open(Width, Height - 8f))
                image10 = new Image(Calc.Random.Choose(atlasSubtextures5));
            else if (Open(Width - 8f, Height))
                image10 = new Image(Calc.Random.Choose(atlasSubtextures4));
            image10.Scale.Y = -1f;
            image10.CenterOrigin();
            image10.Position = new Vector2(Width - 4f, Height - 4f);
            Add(image10);
        }

        private int mod(int x, int m) => (x % m + m) % m;

        private bool Open(float x, float y) => !Scene.CollideCheck<StarJumpBlock>(new Vector2((float) (X + (double) x + 4.0), (float) (Y + (double) y + 4.0)));

        public override void Update()
        {
            base.Update();
            if (!sinks)
                return;
            if (HasPlayerRider())
                sinkTimer = 0.1f;
            else if (sinkTimer > 0.0)
                sinkTimer -= Engine.DeltaTime;
            yLerp = sinkTimer <= 0.0 ? Calc.Approach(yLerp, 0.0f, 1f * Engine.DeltaTime) : Calc.Approach(yLerp, 1f, 1f * Engine.DeltaTime);
            MoveToY(MathHelper.Lerp(startY, startY + 12f, Ease.SineInOut(yLerp)));
        }

        public override void Render()
        {
            StarJumpController entity = Scene.Tracker.GetEntity<StarJumpController>();
            if (entity != null)
            {
                Vector2 vector2 = level.Camera.Position.Floor();
                Draw.SpriteBatch.Draw((RenderTarget2D) entity.BlockFill, Position, new Rectangle((int) (X - (double) vector2.X), (int) (Y - (double) vector2.Y), (int) Width, (int) Height), Color.White);
            }
            base.Render();
        }
    }
}
