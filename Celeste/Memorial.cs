using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Memorial : Entity
    {
        private Image sprite;
        private MemorialText text;
        private Sprite dreamyText;
        private bool wasShowing;
        private SoundSource loopingSfx;

        public Memorial(Vector2 position)
            : base(position)
        {
            Tag = (int) Tags.PauseUpdate;
            Add(sprite = new Image(GFX.Game["scenery/memorial/memorial"]));
            sprite.Origin = new Vector2(sprite.Width / 2f, sprite.Height);
            Depth = 100;
            Collider = new Hitbox(60f, 80f, -30f, -60f);
            Add(loopingSfx = new SoundSource());
        }

        public Memorial(EntityData data, Vector2 offset)
            : this(data.Position + offset)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Level level = scene as Level;
            level.Add(text = new MemorialText(this, level.Session.Dreaming));
            if (level.Session.Dreaming)
            {
                Add(dreamyText = new Sprite(GFX.Game, "scenery/memorial/floatytext"));
                dreamyText.AddLoop("dreamy", "", 0.1f);
                dreamyText.Play("dreamy");
                dreamyText.Position = new Vector2((float) (-(double) dreamyText.Width / 2.0), -33f);
            }
            if (level.Session.Area.ID != 1 || level.Session.Area.Mode != AreaMode.Normal)
                return;
            Audio.SetMusicParam("end", 1f);
        }

        public override void Update()
        {
            base.Update();
            Level scene = Scene as Level;
            if (scene.Paused)
            {
                loopingSfx.Pause();
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                bool dreaming = scene.Session.Dreaming;
                wasShowing = text.Show;
                text.Show = entity != null && CollideCheck(entity);
                if (text.Show && !wasShowing)
                {
                    Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_in" : "event:/ui/game/memorial_text_in");
                    if (dreaming)
                    {
                        loopingSfx.Play("event:/ui/game/memorial_dream_loop");
                        loopingSfx.Param("end", 0.0f);
                    }
                }
                else if (!text.Show && wasShowing)
                {
                    Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_out" : "event:/ui/game/memorial_text_out");
                    loopingSfx.Param("end", 1f);
                    loopingSfx.Stop();
                }
                loopingSfx.Resume();
            }
        }
    }
}
