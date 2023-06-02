using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
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
            this.Depth = -10000;
            this.sinks = sinks;
            this.Add((Component) new LightOcclude());
            this.startY = this.Y;
            this.SurfaceSoundIndex = 32;
        }

        public StarJumpBlock(EntityData data, Vector2 offset)
            : this(data.Position + offset, (float) data.Width, (float) data.Height, data.Bool(nameof (sinks)))
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            this.level = this.SceneAs<Level>();
            List<MTexture> atlasSubtextures1 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/leftrailing");
            List<MTexture> atlasSubtextures2 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/railing");
            List<MTexture> atlasSubtextures3 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/rightrailing");
            List<MTexture> atlasSubtextures4 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeH");
            for (int x = 8; (double) x < (double) this.Width - 8.0; x += 8)
            {
                if (this.Open((float) x, -8f))
                {
                    Monocle.Image image1 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
                    image1.CenterOrigin();
                    image1.Position = new Vector2((float) (x + 4), 4f);
                    this.Add((Component) image1);
                    Monocle.Image image2 = new Monocle.Image(atlasSubtextures2[this.mod((int) ((double) this.X + (double) x) / 8, atlasSubtextures2.Count)]);
                    image2.Position = new Vector2((float) x, -8f);
                    this.Add((Component) image2);
                }
                if (this.Open((float) x, this.Height))
                {
                    Monocle.Image image = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
                    image.CenterOrigin();
                    image.Scale.Y = -1f;
                    image.Position = new Vector2((float) (x + 4), this.Height - 4f);
                    this.Add((Component) image);
                }
            }
            List<MTexture> atlasSubtextures5 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/edgeV");
            for (int y = 8; (double) y < (double) this.Height - 8.0; y += 8)
            {
                if (this.Open(-8f, (float) y))
                {
                    Monocle.Image image = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
                    image.CenterOrigin();
                    image.Scale.X = -1f;
                    image.Position = new Vector2(4f, (float) (y + 4));
                    this.Add((Component) image);
                }
                if (this.Open(this.Width, (float) y))
                {
                    Monocle.Image image = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
                    image.CenterOrigin();
                    image.Position = new Vector2(this.Width - 4f, (float) (y + 4));
                    this.Add((Component) image);
                }
            }
            List<MTexture> atlasSubtextures6 = GFX.Game.GetAtlasSubtextures("objects/starjumpBlock/corner");
            Monocle.Image image3 = (Monocle.Image) null;
            if (this.Open(-8f, 0.0f) && this.Open(0.0f, -8f))
            {
                image3 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures6));
                image3.Scale.X = -1f;
                Monocle.Image image4 = new Monocle.Image(atlasSubtextures1[this.mod((int) this.X / 8, atlasSubtextures1.Count)]);
                image4.Position = new Vector2(0.0f, -8f);
                this.Add((Component) image4);
            }
            else if (this.Open(-8f, 0.0f))
            {
                image3 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
                image3.Scale.X = -1f;
            }
            else if (this.Open(0.0f, -8f))
            {
                image3 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
                Monocle.Image image5 = new Monocle.Image(atlasSubtextures2[this.mod((int) this.X / 8, atlasSubtextures2.Count)]);
                image5.Position = new Vector2(0.0f, -8f);
                this.Add((Component) image5);
            }
            image3.CenterOrigin();
            image3.Position = new Vector2(4f, 4f);
            this.Add((Component) image3);
            Monocle.Image image6 = (Monocle.Image) null;
            if (this.Open(this.Width, 0.0f) && this.Open(this.Width - 8f, -8f))
            {
                image6 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures6));
                Monocle.Image image7 = new Monocle.Image(atlasSubtextures3[this.mod((int) ((double) this.X + (double) this.Width) / 8 - 1, atlasSubtextures3.Count)]);
                image7.Position = new Vector2(this.Width - 8f, -8f);
                this.Add((Component) image7);
            }
            else if (this.Open(this.Width, 0.0f))
                image6 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
            else if (this.Open(this.Width - 8f, -8f))
            {
                image6 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
                Monocle.Image image8 = new Monocle.Image(atlasSubtextures2[this.mod((int) ((double) this.X + (double) this.Width) / 8 - 1, atlasSubtextures2.Count)]);
                image8.Position = new Vector2(this.Width - 8f, -8f);
                this.Add((Component) image8);
            }
            image6.CenterOrigin();
            image6.Position = new Vector2(this.Width - 4f, 4f);
            this.Add((Component) image6);
            Monocle.Image image9 = (Monocle.Image) null;
            if (this.Open(-8f, this.Height - 8f) && this.Open(0.0f, this.Height))
            {
                image9 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures6));
                image9.Scale.X = -1f;
            }
            else if (this.Open(-8f, this.Height - 8f))
            {
                image9 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
                image9.Scale.X = -1f;
            }
            else if (this.Open(0.0f, this.Height))
                image9 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
            image9.Scale.Y = -1f;
            image9.CenterOrigin();
            image9.Position = new Vector2(4f, this.Height - 4f);
            this.Add((Component) image9);
            Monocle.Image image10 = (Monocle.Image) null;
            if (this.Open(this.Width, this.Height - 8f) && this.Open(this.Width - 8f, this.Height))
                image10 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures6));
            else if (this.Open(this.Width, this.Height - 8f))
                image10 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures5));
            else if (this.Open(this.Width - 8f, this.Height))
                image10 = new Monocle.Image(Calc.Random.Choose<MTexture>(atlasSubtextures4));
            image10.Scale.Y = -1f;
            image10.CenterOrigin();
            image10.Position = new Vector2(this.Width - 4f, this.Height - 4f);
            this.Add((Component) image10);
        }

        private int mod(int x, int m) => (x % m + m) % m;

        private bool Open(float x, float y) => !this.Scene.CollideCheck<StarJumpBlock>(new Vector2((float) ((double) this.X + (double) x + 4.0), (float) ((double) this.Y + (double) y + 4.0)));

        public override void Update()
        {
            base.Update();
            if (!this.sinks)
                return;
            if (this.HasPlayerRider())
                this.sinkTimer = 0.1f;
            else if ((double) this.sinkTimer > 0.0)
                this.sinkTimer -= Engine.DeltaTime;
            this.yLerp = (double) this.sinkTimer <= 0.0 ? Calc.Approach(this.yLerp, 0.0f, 1f * Engine.DeltaTime) : Calc.Approach(this.yLerp, 1f, 1f * Engine.DeltaTime);
            this.MoveToY(MathHelper.Lerp(this.startY, this.startY + 12f, Ease.SineInOut(this.yLerp)));
        }

        public override void Render()
        {
            StarJumpController entity = this.Scene.Tracker.GetEntity<StarJumpController>();
            if (entity != null)
            {
                Vector2 vector2 = this.level.Camera.Position.Floor();
                Draw.SpriteBatch.Draw((Texture2D) (RenderTarget2D) entity.BlockFill, this.Position, new Rectangle?(new Rectangle((int) ((double) this.X - (double) vector2.X), (int) ((double) this.Y - (double) vector2.Y), (int) this.Width, (int) this.Height)), Color.White);
            }
            base.Render();
        }
    }
}
