// Decompiled with JetBrains decompiler
// Type: Celeste.Memorial
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste
{
    public class Memorial : Entity
    {
        private readonly Monocle.Image sprite;
        private MemorialText text;
        private Sprite dreamyText;
        private bool wasShowing;
        private readonly SoundSource loopingSfx;

        public Memorial(Vector2 position)
            : base(position)
        {
            Tag = (int)Tags.PauseUpdate;
            Add(sprite = new Monocle.Image(GFX.Game["scenery/memorial/memorial"]));
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
                dreamyText.Position = new Vector2((float)(-(double)dreamyText.Width / 2.0), -33f);
            }
            if (level.Session.Area.ID != 1 || level.Session.Area.Mode != AreaMode.Normal)
            {
                return;
            }

            Audio.SetMusicParam("end", 1f);
        }

        public override void Update()
        {
            base.Update();
            Level scene = Scene as Level;
            if (scene.Paused)
            {
                _ = loopingSfx.Pause();
            }
            else
            {
                Player entity = Scene.Tracker.GetEntity<Player>();
                bool dreaming = scene.Session.Dreaming;
                wasShowing = text.Show;
                text.Show = entity != null && CollideCheck(entity);
                if (text.Show && !wasShowing)
                {
                    _ = Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_in" : "event:/ui/game/memorial_text_in");
                    if (dreaming)
                    {
                        _ = loopingSfx.Play("event:/ui/game/memorial_dream_loop");
                        _ = loopingSfx.Param("end", 0.0f);
                    }
                }
                else if (!text.Show && wasShowing)
                {
                    _ = Audio.Play(dreaming ? "event:/ui/game/memorial_dream_text_out" : "event:/ui/game/memorial_text_out");
                    _ = loopingSfx.Param("end", 1f);
                    _ = loopingSfx.Stop();
                }
                _ = loopingSfx.Resume();
            }
        }
    }
}
