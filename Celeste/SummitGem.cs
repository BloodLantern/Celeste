using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;

namespace Celeste
{
    public class SummitGem : Entity
    {
        public static ParticleType P_Shatter;
        public static readonly Color[] GemColors = new Color[6]
        {
            Calc.HexToColor("9ee9ff"),
            Calc.HexToColor("54baff"),
            Calc.HexToColor("90ff2d"),
            Calc.HexToColor("ffd300"),
            Calc.HexToColor("ff609d"),
            Calc.HexToColor("c5e1ba")
        };
        public int GemID;
        public EntityID GID;
        private Sprite sprite;
        private Wiggler scaleWiggler;
        private Vector2 moveWiggleDir;
        private Wiggler moveWiggler;
        private float bounceSfxDelay;

        public SummitGem(EntityData data, Vector2 position, EntityID gid)
            : base(data.Position + position)
        {
            GID = gid;
            GemID = data.Int("gem");
            Collider = new Hitbox(12f, 12f, -6f, -6f);
            Add(sprite = new Sprite(GFX.Game, "collectables/summitgems/" + GemID + "/gem"));
            sprite.AddLoop("idle", "", 0.08f);
            sprite.Play("idle");
            sprite.CenterOrigin();
            if (SaveData.Instance.SummitGems != null && SaveData.Instance.SummitGems[GemID])
                sprite.Color = Color.White * 0.5f;
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (float) (1.0 + f * 0.30000001192092896)));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            Level scene = Scene as Level;
            if (player.DashAttacking)
            {
                Add(new Coroutine(SmashRoutine(player, scene)));
            }
            else
            {
                player.PointBounce(Center);
                moveWiggler.Start();
                scaleWiggler.Start();
                moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if (bounceSfxDelay > 0.0)
                    return;
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounceSfxDelay = 0.1f;
            }
        }

        private IEnumerator SmashRoutine(Player player, Level level)
        {
            SummitGem follow = this;
            follow.Visible = false;
            follow.Collidable = false;
            player.Stamina = 110f;
            SoundEmitter.Play("event:/game/07_summit/gem_get", follow);
            Session session = (follow.Scene as Level).Session;
            session.DoNotLoad.Add(follow.GID);
            session.SummitGems[follow.GemID] = true;
            SaveData.Instance.RegisterSummitGem(follow.GemID);
            level.Shake();
            Celeste.Freeze(0.1f);
            SummitGem.P_Shatter.Color = SummitGem.GemColors[follow.GemID];
            float direction = player.Speed.Angle();
            level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, follow.Position, Vector2.One * 4f, direction - 1.57079637f);
            level.ParticlesFG.Emit(SummitGem.P_Shatter, 5, follow.Position, Vector2.One * 4f, direction + 1.57079637f);
            SlashFx.Burst(follow.Position, direction);
            for (int index = 0; index < 10; ++index)
                follow.Scene.Add(new AbsorbOrb(follow.Position, player));
            level.Flash(Color.White, true);
            follow.Scene.Add(new BgFlash());
            Engine.TimeRate = 0.5f;
            while (Engine.TimeRate < 1.0)
            {
                Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                yield return null;
            }
            follow.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            bounceSfxDelay -= Engine.DeltaTime;
            sprite.Position = moveWiggleDir * moveWiggler.Value * -8f;
        }

        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                Depth = 10100;
                Tag = (int) Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                alpha = Calc.Approach(alpha, 0.0f, Engine.DeltaTime * 0.5f);
                if (alpha > 0.0)
                    return;
                RemoveSelf();
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * alpha);
            }
        }
    }
}
