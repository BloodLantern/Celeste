using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class MovingPlatform : JumpThru
    {
        private Vector2 start;
        private Vector2 end;
        private float addY;
        private float sinkTimer;
        private MTexture[] textures;
        private string lastSfx;
        private SoundSource sfx;

        public MovingPlatform(Vector2 position, int width, Vector2 node)
            : base(position, width, false)
        {
            start = Position;
            end = node;
            Add(sfx = new SoundSource());
            SurfaceSoundIndex = 5;
            lastSfx = Math.Sign(start.X - end.X) > 0 || Math.Sign(start.Y - end.Y) > 0 ? "event:/game/03_resort/platform_horiz_left" : "event:/game/03_resort/platform_horiz_right";
            Tween tween = Tween.Create(Tween.TweenMode.YoyoLooping, Ease.SineInOut, 2f);
            tween.OnUpdate = t => MoveTo(Vector2.Lerp(start, end, t.Eased) + Vector2.UnitY * addY);
            tween.OnStart = t =>
            {
                if (lastSfx == "event:/game/03_resort/platform_horiz_left")
                    sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_right");
                else
                    sfx.Play(lastSfx = "event:/game/03_resort/platform_horiz_left");
            };
            Add(tween);
            tween.Start(false);
            Add(new LightOcclude(0.2f));
        }

        public MovingPlatform(EntityData data, Vector2 offset)
            : this(data.Position + offset, data.Width, data.Nodes[0] + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Session session = SceneAs<Level>().Session;
            MTexture mtexture = session.Area.ID != 7 || !session.Level.StartsWith("e-") ? GFX.Game["objects/woodPlatform/" + AreaData.Get(scene).WoodPlatform] : GFX.Game["objects/woodPlatform/" + AreaData.Get(4).WoodPlatform];
            textures = new MTexture[mtexture.Width / 8];
            for (int index = 0; index < textures.Length; ++index)
                textures[index] = mtexture.GetSubtexture(index * 8, 0, 8, 8);
            Vector2 vector2 = new Vector2(Width, Height + 4f) / 2f;
            scene.Add(new MovingPlatformLine(start + vector2, end + vector2));
        }

        public override void Render()
        {
            textures[0].Draw(Position);
            for (int x = 8; x < Width - 8.0; x += 8)
                textures[1].Draw(Position + new Vector2(x, 0.0f));
            textures[3].Draw(Position + new Vector2(Width - 8f, 0.0f));
            textures[2].Draw(Position + new Vector2((float) (Width / 2.0 - 4.0), 0.0f));
        }

        public override void OnStaticMoverTrigger(StaticMover sm) => sinkTimer = 0.4f;

        public override void Update()
        {
            base.Update();
            if (HasPlayerRider())
            {
                sinkTimer = 0.2f;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            }
            else if (sinkTimer > 0.0)
            {
                sinkTimer -= Engine.DeltaTime;
                addY = Calc.Approach(addY, 3f, 50f * Engine.DeltaTime);
            }
            else
                addY = Calc.Approach(addY, 0.0f, 20f * Engine.DeltaTime);
        }
    }
}
