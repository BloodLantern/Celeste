using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Celeste
{
    [Tracked(false)]
    public class CrystalStaticSpinner : Entity
    {
        public static ParticleType P_Move;
        public const float ParticleInterval = 0.02f;
        private static readonly Dictionary<CrystalColor, string> fgTextureLookup = new()
        {
            {
                CrystalColor.Blue,
                "danger/crystal/fg_blue"
            },
            {
                CrystalColor.Red,
                "danger/crystal/fg_red"
            },
            {
                CrystalColor.Purple,
                "danger/crystal/fg_purple"
            },
            {
                CrystalColor.Rainbow,
                "danger/crystal/fg_white"
            }
        };
        private static readonly Dictionary<CrystalColor, string> bgTextureLookup = new()
        {
            {
                CrystalColor.Blue,
                "danger/crystal/bg_blue"
            },
            {
                CrystalColor.Red,
                "danger/crystal/bg_red"
            },
            {
                CrystalColor.Purple,
                "danger/crystal/bg_purple"
            },
            {
                CrystalColor.Rainbow,
                "danger/crystal/bg_white"
            }
        };
        public bool AttachToSolid;
        private Entity filler;
        private Border border;
        private readonly float offset = Calc.Random.NextFloat();
        private bool expanded;
        private readonly int randomSeed;
        private CrystalColor color;

        public CrystalStaticSpinner(Vector2 position, bool attachToSolid, CrystalColor color)
            : base(position)
        {
            this.color = color;
            Tag = (int) Tags.TransitionUpdate;
            Collider = new ColliderList(new Collider[2]
            {
                 new Circle(6f),
                 new Hitbox(16f, 4f, -8f, -3f)
            });
            Visible = false;
            Add(new PlayerCollider(new Action<Player>(OnPlayer)));
            Add(new HoldableCollider(new Action<Holdable>(OnHoldable)));
            Add(new LedgeBlocker());
            Depth = -8500;
            AttachToSolid = attachToSolid;
            if (attachToSolid)
                Add(new StaticMover()
                {
                    OnShake = new Action<Vector2>(OnShake),
                    SolidChecker = new Func<Solid, bool>(IsRiding),
                    OnDestroy = new Action(RemoveSelf)
                });
            randomSeed = Calc.Random.Next();
        }

        public CrystalStaticSpinner(EntityData data, Vector2 offset, CrystalColor color)
            : this(data.Position + offset, data.Bool("attachToSolid"), color)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if ((scene as Level).Session.Area.ID == 9)
            {
                Add(new CoreModeListener(this));
                color = (scene as Level).CoreMode != Session.CoreModes.Cold ? CrystalColor.Red : CrystalColor.Blue;
            }
            if (!InView())
                return;
            CreateSprites();
        }

        public void ForceInstantiate()
        {
            CreateSprites();
            Visible = true;
        }

        public override void Update()
        {
            if (!Visible)
            {
                Collidable = false;
                if (InView())
                {
                    Visible = true;
                    if (!expanded)
                        CreateSprites();
                    if (color == CrystalColor.Rainbow)
                        UpdateHue();
                }
            }
            else
            {
                base.Update();
                if (color == CrystalColor.Rainbow && Scene.OnInterval(0.08f, offset))
                    UpdateHue();
                if (Scene.OnInterval(0.25f, offset) && !InView())
                    Visible = false;
                if (Scene.OnInterval(0.05f, offset))
                {
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity != null)
                        Collidable = (double) Math.Abs(entity.X - X) < 128.0 && (double) Math.Abs(entity.Y - Y) < 128.0;
                }
            }
            if (filler == null)
                return;
            filler.Position = Position;
        }

        private void UpdateHue()
        {
            foreach (Component component in Components)
            {
                if (component is Monocle.Image image)
                    image.Color = GetHue(Position + image.Position);
            }
            if (filler == null)
                return;
            foreach (Component component in filler.Components)
            {
                if (component is Monocle.Image image)
                    image.Color = GetHue(Position + image.Position);
            }
        }

        private bool InView()
        {
            Camera camera = (Scene as Level).Camera;
            return (double) X > (double) camera.X - 16.0 && (double) Y > (double) camera.Y - 16.0 && (double) X < (double) camera.X + 320.0 + 16.0 && (double) Y < (double) camera.Y + 180.0 + 16.0;
        }

        private void CreateSprites()
        {
            if (expanded)
                return;
            Calc.PushRandom(randomSeed);
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(fgTextureLookup[this.color]);
            MTexture mtexture = Calc.Random.Choose(atlasSubtextures);
            Color color = Color.White;
            if (this.color == CrystalColor.Rainbow)
                color = GetHue(Position);
            if (!SolidCheck(new Vector2(X - 4f, Y - 4f)))
                Add(new Image(mtexture.GetSubtexture(0, 0, 14, 14)).SetOrigin(12f, 12f).SetColor(color));
            if (!SolidCheck(new Vector2(X + 4f, Y - 4f)))
                Add(new Image(mtexture.GetSubtexture(10, 0, 14, 14)).SetOrigin(2f, 12f).SetColor(color));
            if (!SolidCheck(new Vector2(X + 4f, Y + 4f)))
                Add(new Image(mtexture.GetSubtexture(10, 10, 14, 14)).SetOrigin(2f, 2f).SetColor(color));
            if (!SolidCheck(new Vector2(X - 4f, Y + 4f)))
                Add(new Image(mtexture.GetSubtexture(0, 10, 14, 14)).SetOrigin(12f, 2f).SetColor(color));
            foreach (CrystalStaticSpinner entity in Scene.Tracker.GetEntities<CrystalStaticSpinner>().Cast<CrystalStaticSpinner>())
            {
                if (entity != this && entity.AttachToSolid == AttachToSolid && (double) entity.X >= (double) X && (double) (entity.Position - Position).Length() < 24.0)
                    AddSprite((Position + entity.Position) / 2f - Position);
            }
            Scene.Add(border = new Border(this, filler));
            expanded = true;
            Calc.PopRandom();
        }

        private void AddSprite(Vector2 offset)
        {
            if (filler == null)
            {
                Scene.Add(filler = new Entity(Position));
                filler.Depth = Depth + 1;
            }
            List<MTexture> atlasSubtextures = GFX.Game.GetAtlasSubtextures(CrystalStaticSpinner.bgTextureLookup[color]);
            Monocle.Image image = new(Calc.Random.Choose<MTexture>(atlasSubtextures));
            image.Position = offset;
            image.Rotation = Calc.Random.Choose<int>(0, 1, 2, 3) * 1.57079637f;
            image.CenterOrigin();
            if (color == CrystalColor.Rainbow)
                image.Color = GetHue(Position + offset);
            filler.Add(image);
        }

        private bool SolidCheck(Vector2 position)
        {
            if (AttachToSolid)
                return false;
            foreach (Solid solid in Scene.CollideAll<Solid>(position))
            {
                if (solid is SolidTiles)
                    return true;
            }
            return false;
        }

        private void ClearSprites()
        {
            filler?.RemoveSelf();
            filler = null;
            border?.RemoveSelf();
            border = null;
            foreach (Component component in Components.GetAll<Monocle.Image>())
                component.RemoveSelf();
            expanded = false;
        }

        private void OnShake(Vector2 pos)
        {
            foreach (Component component in Components)
            {
                if (component is Monocle.Image)
                    (component as Monocle.Image).Position = pos;
            }
        }

        private bool IsRiding(Solid solid) => CollideCheck(solid);

        private void OnPlayer(Player player) => player.Die((player.Position - Position).SafeNormalize());

        private void OnHoldable(Holdable h) => h.HitSpinner(this);

        public override void Removed(Scene scene)
        {
            if (filler != null && filler.Scene == scene)
                filler.RemoveSelf();
            if (border != null && border.Scene == scene)
                border.RemoveSelf();
            base.Removed(scene);
        }

        public void Destroy(bool boss = false)
        {
            if (InView())
            {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                Color color = Color.White;
                if (this.color == CrystalColor.Red)
                    color = Calc.HexToColor("ff4f4f");
                else if (this.color == CrystalColor.Blue)
                    color = Calc.HexToColor("639bff");
                else if (this.color == CrystalColor.Purple)
                    color = Calc.HexToColor("ff4fef");
                CrystalDebris.Burst(Position, color, boss, 8);
            }
            RemoveSelf();
        }

        private Color GetHue(Vector2 position)
        {
            float num = 280f;
            return Calc.HsvToColor((float) (0.40000000596046448 + (double) Calc.YoYo((position.Length() + Scene.TimeActive * 50f) % num / num) * 0.40000000596046448), 0.4f, 0.9f);
        }

        private class CoreModeListener : Component
        {
            public CrystalStaticSpinner Parent;

            public CoreModeListener(CrystalStaticSpinner parent)
                : base(true, false)
            {
                Parent = parent;
            }

            public override void Update()
            {
                Level scene = Scene as Level;
                if ((Parent.color != CrystalColor.Blue || scene.CoreMode != Session.CoreModes.Hot) && (Parent.color != CrystalColor.Red || scene.CoreMode != Session.CoreModes.Cold))
                    return;
                Parent.color = Parent.color != CrystalColor.Blue ? CrystalColor.Blue : CrystalColor.Red;
                Parent.ClearSprites();
                Parent.CreateSprites();
            }
        }

        private class Border : Entity
        {
            private readonly Entity[] drawing = new Entity[2];

            public Border(Entity parent, Entity filler)
            {
                drawing[0] = parent;
                drawing[1] = filler;
                Depth = parent.Depth + 2;
            }

            public override void Render()
            {
                if (!drawing[0].Visible)
                    return;
                DrawBorder(drawing[0]);
                DrawBorder(drawing[1]);
            }

            private void DrawBorder(Entity entity)
            {
                if (entity == null)
                    return;
                foreach (Component component in entity.Components)
                {
                    if (component is Monocle.Image image)
                    {
                        Color color = image.Color;
                        Vector2 position = image.Position;
                        image.Color = Color.Black;
                        image.Position = position + new Vector2(0.0f, -1f);
                        image.Render();
                        image.Position = position + new Vector2(0.0f, 1f);
                        image.Render();
                        image.Position = position + new Vector2(-1f, 0.0f);
                        image.Render();
                        image.Position = position + new Vector2(1f, 0.0f);
                        image.Render();
                        image.Color = color;
                        image.Position = position;
                    }
                }
            }
        }
    }
}
