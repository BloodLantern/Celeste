// Decompiled with JetBrains decompiler
// Type: Celeste.SummitGem
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
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
            this.GID = gid;
            this.GemID = data.Int("gem");
            this.Collider = (Collider) new Hitbox(12f, 12f, -6f, -6f);
            this.Add((Component) (this.sprite = new Sprite(GFX.Game, "collectables/summitgems/" + (object) this.GemID + "/gem")));
            this.sprite.AddLoop("idle", "", 0.08f);
            this.sprite.Play("idle");
            this.sprite.CenterOrigin();
            if (SaveData.Instance.SummitGems != null && SaveData.Instance.SummitGems[this.GemID])
                this.sprite.Color = Color.White * 0.5f;
            this.Add((Component) (this.scaleWiggler = Wiggler.Create(0.5f, 4f, (Action<float>) (f => this.sprite.Scale = Vector2.One * (float) (1.0 + (double) f * 0.30000001192092896)))));
            this.moveWiggler = Wiggler.Create(0.8f, 2f);
            this.moveWiggler.StartZero = true;
            this.Add((Component) this.moveWiggler);
            this.Add((Component) new PlayerCollider(new Action<Player>(this.OnPlayer)));
        }

        private void OnPlayer(Player player)
        {
            Level scene = this.Scene as Level;
            if (player.DashAttacking)
            {
                this.Add((Component) new Coroutine(this.SmashRoutine(player, scene)));
            }
            else
            {
                player.PointBounce(this.Center);
                this.moveWiggler.Start();
                this.scaleWiggler.Start();
                this.moveWiggleDir = (this.Center - player.Center).SafeNormalize(Vector2.UnitY);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                if ((double) this.bounceSfxDelay > 0.0)
                    return;
                Audio.Play("event:/game/general/crystalheart_bounce", this.Position);
                this.bounceSfxDelay = 0.1f;
            }
        }

        private IEnumerator SmashRoutine(Player player, Level level)
        {
            SummitGem follow = this;
            follow.Visible = false;
            follow.Collidable = false;
            player.Stamina = 110f;
            SoundEmitter.Play("event:/game/07_summit/gem_get", (Entity) follow);
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
                follow.Scene.Add((Entity) new AbsorbOrb(follow.Position, (Entity) player));
            level.Flash(Color.White, true);
            follow.Scene.Add((Entity) new SummitGem.BgFlash());
            Engine.TimeRate = 0.5f;
            while ((double) Engine.TimeRate < 1.0)
            {
                Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                yield return (object) null;
            }
            follow.RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            this.bounceSfxDelay -= Engine.DeltaTime;
            this.sprite.Position = this.moveWiggleDir * this.moveWiggler.Value * -8f;
        }

        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                this.Depth = 10100;
                this.Tag = (int) Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                this.alpha = Calc.Approach(this.alpha, 0.0f, Engine.DeltaTime * 0.5f);
                if ((double) this.alpha > 0.0)
                    return;
                this.RemoveSelf();
            }

            public override void Render()
            {
                Vector2 position = (this.Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * this.alpha);
            }
        }
    }
}
