// Decompiled with JetBrains decompiler
// Type: Celeste.TempleEndingMusicHandler
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Celeste
{
    public class TempleEndingMusicHandler : Entity
    {
        public const string StartLevel = "e-01";
        public const string EndLevel = "e-09";
        public const string ApplyIn = "e-*";
        private readonly HashSet<string> levels = new();
        private float startX;
        private float endX;

        public TempleEndingMusicHandler()
        {
            Tag = (int)Tags.TransitionUpdate | (int)Tags.Global;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Regex regex = new(Regex.Escape("e-*").Replace("\\*", ".*") + "$");
            foreach (LevelData level in (scene as Level).Session.MapData.Levels)
            {
                if (level.Name.Equals("e-01"))
                {
                    startX = level.Bounds.Left;
                }
                else if (level.Name.Equals("e-09"))
                {
                    endX = level.Bounds.Right;
                }

                if (regex.IsMatch(level.Name))
                {
                    _ = levels.Add(level.Name);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            Level scene = Scene as Level;
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null || !levels.Contains(scene.Session.Level) || !(Audio.CurrentMusic == "event:/music/lvl5/mirror"))
            {
                return;
            }

            float num = Calc.Clamp((float)(((double)entity.X - startX) / (endX - (double)startX)), 0.0f, 1f);
            _ = scene.Session.Audio.Music.Layer(1, 1f - num);
            _ = scene.Session.Audio.Music.Layer(5, num);
            scene.Session.Audio.Apply();
        }
    }
}
