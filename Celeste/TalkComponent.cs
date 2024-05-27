using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class TalkComponent : Component
    {
        public static TalkComponent PlayerOver;
        public bool Enabled = true;
        public Rectangle Bounds;
        public Vector2 DrawAt;
        public Action<Player> OnTalk;
        public bool PlayerMustBeFacing = true;
        public TalkComponentUI UI;
        public HoverDisplay HoverUI;
        private float cooldown;
        private float hoverTimer;
        private float disableDelay;

        public TalkComponent(
            Rectangle bounds,
            Vector2 drawAt,
            Action<Player> onTalk,
            HoverDisplay hoverDisplay = null)
            : base(true, true)
        {
            Bounds = bounds;
            DrawAt = drawAt;
            OnTalk = onTalk;
            if (hoverDisplay == null)
                HoverUI = new HoverDisplay
                {
                    Texture = GFX.Gui["hover/highlight"],
                    InputPosition = new Vector2(0.0f, -75f)
                };
            else
                HoverUI = hoverDisplay;
        }

        public override void Update()
        {
            if (UI == null)
                Entity.Scene.Add(UI = new TalkComponentUI(this));
            Player entity = Scene.Tracker.GetEntity<Player>();
            bool flag = disableDelay < 0.05000000074505806 && entity != null && entity.CollideRect(new Rectangle((int) (Entity.X + (double) Bounds.X), (int) (Entity.Y + (double) Bounds.Y), Bounds.Width, Bounds.Height)) && entity.OnGround() && entity.Bottom < Entity.Y + (double) Bounds.Bottom + 4.0 && entity.StateMachine.State == 0 && (!PlayerMustBeFacing || Math.Abs(entity.X - Entity.X) <= 16.0 || entity.Facing == (Facings) Math.Sign(Entity.X - entity.X)) && (TalkComponent.PlayerOver == null || TalkComponent.PlayerOver == this);
            if (flag)
                hoverTimer += Engine.DeltaTime;
            else if (UI.Display)
                hoverTimer = 0.0f;
            if (TalkComponent.PlayerOver == this && !flag)
                TalkComponent.PlayerOver = null;
            else if (flag)
                TalkComponent.PlayerOver = this;
            if (flag && cooldown <= 0.0 && entity != null && (int) entity.StateMachine == 0 && Input.Talk.Pressed && Enabled && !Scene.Paused)
            {
                cooldown = 0.1f;
                if (OnTalk != null)
                    OnTalk(entity);
            }
            if (flag && (int) entity.StateMachine == 0)
                cooldown -= Engine.DeltaTime;
            if (!Enabled)
                disableDelay += Engine.DeltaTime;
            else
                disableDelay = 0.0f;
            UI.Highlighted = flag && hoverTimer > 0.10000000149011612;
            base.Update();
        }

        public override void Removed(Entity entity)
        {
            Dispose();
            base.Removed(entity);
        }

        public override void EntityRemoved(Scene scene)
        {
            Dispose();
            base.EntityRemoved(scene);
        }

        public override void SceneEnd(Scene scene)
        {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose()
        {
            if (TalkComponent.PlayerOver == this)
                TalkComponent.PlayerOver = null;
            Scene.Remove(UI);
            UI = null;
        }

        public override void DebugRender(Camera camera)
        {
            base.DebugRender(camera);
            Draw.HollowRect(Entity.X + Bounds.X, Entity.Y + Bounds.Y, Bounds.Width, Bounds.Height, Color.Green);
        }

        public class HoverDisplay
        {
            public MTexture Texture;
            public Vector2 InputPosition;
            public string SfxIn = "event:/ui/game/hotspot_main_in";
            public string SfxOut = "event:/ui/game/hotspot_main_out";
        }

        public class TalkComponentUI : Entity
        {
            public TalkComponent Handler;
            private bool highlighted;
            private float slide;
            private float timer;
            private Wiggler wiggler;
            private float alpha = 1f;
            private Color lineColor = new Color(1f, 1f, 1f);

            public bool Highlighted
            {
                get => highlighted;
                set
                {
                    if (!(highlighted != value & Display))
                        return;
                    highlighted = value;
                    if (highlighted)
                        Audio.Play(Handler.HoverUI.SfxIn);
                    else
                        Audio.Play(Handler.HoverUI.SfxOut);
                    wiggler.Start();
                }
            }

            public bool Display
            {
                get
                {
                    if (!Handler.Enabled || Scene == null || Scene.Tracker.GetEntity<Textbox>() != null)
                        return false;
                    Player entity = Scene.Tracker.GetEntity<Player>();
                    if (entity == null || entity.StateMachine.State == 11)
                        return false;
                    Level scene = Scene as Level;
                    return !scene.FrozenOrPaused && scene.RetryPlayerCorpse == null;
                }
            }

            public TalkComponentUI(TalkComponent handler)
            {
                Handler = handler;
                AddTag((int) Tags.HUD | (int) Tags.Persistent);
                Add(wiggler = Wiggler.Create(0.25f, 4f));
            }

            public override void Awake(Scene scene)
            {
                base.Awake(scene);
                if (Handler.Entity != null && !Scene.CollideCheck<FakeWall>(Handler.Entity.Position))
                    return;
                alpha = 0.0f;
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                slide = Calc.Approach(slide, Display ? 1f : 0.0f, Engine.DeltaTime * 4f);
                if (alpha < 1.0 && Handler.Entity != null && !Scene.CollideCheck<FakeWall>(Handler.Entity.Position))
                    alpha = Calc.Approach(alpha, 1f, 2f * Engine.DeltaTime);
                base.Update();
            }

            public override void Render()
            {
                Level scene = Scene as Level;
                if (scene.FrozenOrPaused || slide <= 0.0 || Handler.Entity == null)
                    return;
                Vector2 position1 = Handler.Entity.Position + Handler.DrawAt - scene.Camera.Position.Floor();
                if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode)
                    position1.X = 320f - position1.X;
                position1.X *= 6f;
                position1.Y *= 6f;
                position1.Y += (float) (Math.Sin(timer * 4.0) * 12.0 + 64.0 * (1.0 - Ease.CubeOut(slide)));
                float scale = !Highlighted ? (float) (1.0 + wiggler.Value * 0.5) : (float) (1.0 - wiggler.Value * 0.5);
                float num = Ease.CubeInOut(slide) * alpha;
                Color color = lineColor * num;
                if (Highlighted)
                    Handler.HoverUI.Texture.DrawJustified(position1, new Vector2(0.5f, 1f), color * alpha, scale);
                else
                    GFX.Gui["hover/idle"].DrawJustified(position1, new Vector2(0.5f, 1f), color * alpha, scale);
                if (!Highlighted)
                    return;
                Vector2 position2 = position1 + Handler.HoverUI.InputPosition * scale;
                if (Input.GuiInputController())
                    Input.GuiButton(Input.Talk).DrawJustified(position2, new Vector2(0.5f), Color.White * num, scale);
                else
                    ActiveFont.DrawOutline(Input.FirstKey(Input.Talk).ToString().ToUpper(), position2, new Vector2(0.5f), new Vector2(scale), Color.White * num, 2f, Color.Black);
            }
        }
    }
}
