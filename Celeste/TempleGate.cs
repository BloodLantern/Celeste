using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
    [Tracked]
    public class TempleGate : Solid
    {
        private const int OpenHeight = 0;
        private const float HoldingWaitTime = 0.2f;
        private const float HoldingOpenDistSq = 4096f;
        private const float HoldingCloseDistSq = 6400f;
        private const int MinDrawHeight = 4;
        public string LevelID;
        public Types Type;
        public bool ClaimedByASwitch;
        private bool theoGate;
        private int closedHeight;
        private Sprite sprite;
        private Shaker shaker;
        private float drawHeight;
        private float drawHeightMoveSpeed;
        private bool open;
        private float holdingWaitTimer = 0.2f;
        private Vector2 holdingCheckFrom;
        private bool lockState;

        public TempleGate(
            Vector2 position,
            int height,
            Types type,
            string spriteName,
            string levelID)
            : base(position, 8f, height, true)
        {
            Type = type;
            closedHeight = height;
            LevelID = levelID;
            Add(sprite = GFX.SpriteBank.Create("templegate_" + spriteName));
            sprite.X = Collider.Width / 2f;
            sprite.Play("idle");
            Add(shaker = new Shaker(false));
            Depth = -9000;
            theoGate = spriteName.Equals("theo", StringComparison.InvariantCultureIgnoreCase);
            holdingCheckFrom = Position + new Vector2(Width / 2f, height / 2);
        }

        public TempleGate(EntityData data, Vector2 offset, string levelID)
            : this(data.Position + offset, data.Height, data.Enum<Types>("type"), data.Attr(nameof (sprite), "default"), levelID)
        {
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (Type == Types.CloseBehindPlayer)
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                if (entity != null && entity.Left < (double) Right && entity.Bottom >= (double) Top && entity.Top <= (double) Bottom)
                {
                    StartOpen();
                    Add(new Coroutine(CloseBehindPlayer()));
                }
            }
            else if (Type == Types.CloseBehindPlayerAlways)
            {
                StartOpen();
                Add(new Coroutine(CloseBehindPlayer()));
            }
            else if (Type == Types.CloseBehindPlayerAndTheo)
            {
                StartOpen();
                Add(new Coroutine(CloseBehindPlayerAndTheo()));
            }
            else if (Type == Types.HoldingTheo)
            {
                if (TheoIsNearby())
                    StartOpen();
                Hitbox.Width = 16f;
            }
            else if (Type == Types.TouchSwitches)
                Add(new Coroutine(CheckTouchSwitches()));
            drawHeight = Math.Max(4f, Height);
        }

        public bool CloseBehindPlayerCheck()
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            return entity != null && entity.X < (double) X;
        }

        public void SwitchOpen()
        {
            sprite.Play("open");
            Alarm.Set(this, 0.2f, () =>
            {
                shaker.ShakeFor(0.2f, false);
                Alarm.Set(this, 0.2f, Open);
            });
        }

        public void Open()
        {
            Audio.Play(theoGate ? "event:/game/05_mirror_temple/gate_theo_open" : "event:/game/05_mirror_temple/gate_main_open", Position);
            holdingWaitTimer = 0.2f;
            drawHeightMoveSpeed = 200f;
            drawHeight = Height;
            shaker.ShakeFor(0.2f, false);
            SetHeight(0);
            sprite.Play("open");
            open = true;
        }

        public void StartOpen()
        {
            SetHeight(0);
            drawHeight = 4f;
            open = true;
        }

        public void Close()
        {
            Audio.Play(theoGate ? "event:/game/05_mirror_temple/gate_theo_close" : "event:/game/05_mirror_temple/gate_main_close", Position);
            holdingWaitTimer = 0.2f;
            drawHeightMoveSpeed = 300f;
            drawHeight = Math.Max(4f, Height);
            shaker.ShakeFor(0.2f, false);
            SetHeight(closedHeight);
            sprite.Play("hit");
            open = false;
        }

        private IEnumerator CloseBehindPlayer()
        {
            TempleGate templeGate = this;
            while (true)
            {
                Player entity = templeGate.Scene.Tracker.GetEntity<Player>();
                if (templeGate.lockState || entity == null || entity.Left <= templeGate.Right + 4.0)
                    yield return null;
                else
                    break;
            }
            templeGate.Close();
        }

        private IEnumerator CloseBehindPlayerAndTheo()
        {
            TempleGate templeGate = this;
            while (true)
            {
                Player entity1 = templeGate.Scene.Tracker.GetEntity<Player>();
                if (entity1 != null && entity1.Left > templeGate.Right + 4.0)
                {
                    TheoCrystal entity2 = templeGate.Scene.Tracker.GetEntity<TheoCrystal>();
                    if (!templeGate.lockState && entity2 != null && entity2.Left > templeGate.Right + 4.0)
                        break;
                }
                yield return null;
            }
            templeGate.Close();
        }

        private IEnumerator CheckTouchSwitches()
        {
            TempleGate templeGate = this;
            while (!Switch.Check(templeGate.Scene))
                yield return null;
            templeGate.sprite.Play("open");
            yield return 0.5f;
            templeGate.shaker.ShakeFor(0.2f, false);
            yield return 0.2f;
            while (templeGate.lockState)
                yield return null;
            templeGate.Open();
        }

        public bool TheoIsNearby()
        {
            TheoCrystal entity = Scene.Tracker.GetEntity<TheoCrystal>();
            return entity == null || entity.X > X + 10.0 || Vector2.DistanceSquared(holdingCheckFrom, entity.Center) < (open ? 6400.0 : 4096.0);
        }

        private void SetHeight(int height)
        {
            if (height < (double) Collider.Height)
            {
                Collider.Height = height;
            }
            else
            {
                float y = Y;
                int height1 = (int) Collider.Height;
                if (Collider.Height < 64.0)
                {
                    Y -= 64f - Collider.Height;
                    Collider.Height = 64f;
                }
                MoveVExact(height - height1);
                Y = y;
                Collider.Height = height;
            }
        }

        public override void Update()
        {
            base.Update();
            if (Type == Types.HoldingTheo)
            {
                if (holdingWaitTimer > 0.0)
                    holdingWaitTimer -= Engine.DeltaTime;
                else if (!lockState)
                {
                    if (open && !TheoIsNearby())
                    {
                        Close();
                        CollideFirst<Player>(Position + new Vector2(8f, 0.0f))?.Die(Vector2.Zero);
                    }
                    else if (!open && TheoIsNearby())
                        Open();
                }
            }
            float target = Math.Max(4f, Height);
            if (drawHeight != (double) target)
            {
                lockState = true;
                drawHeight = Calc.Approach(drawHeight, target, drawHeightMoveSpeed * Engine.DeltaTime);
            }
            else
                lockState = false;
        }

        public override void Render()
        {
            Vector2 vector2 = new Vector2(Math.Sign(shaker.Value.X), 0.0f);
            Draw.Rect(X - 2f, Y - 8f, 14f, 10f, Color.Black);
            sprite.DrawSubrect(Vector2.Zero + vector2, new Rectangle(0, (int) (sprite.Height - (double) drawHeight), (int) sprite.Width, (int) drawHeight));
        }

        public enum Types
        {
            NearestSwitch,
            CloseBehindPlayer,
            CloseBehindPlayerAlways,
            HoldingTheo,
            TouchSwitches,
            CloseBehindPlayerAndTheo,
        }
    }
}
