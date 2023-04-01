// Decompiled with JetBrains decompiler
// Type: Celeste.AutoSplitterInfo
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;

namespace Celeste
{
    public class AutoSplitterInfo
    {
        public int Chapter;
        public int Mode;
        public string Level;
        public bool TimerActive;
        public bool ChapterStarted;
        public bool ChapterComplete;
        public long ChapterTime;
        public int ChapterStrawberries;
        public bool ChapterCassette;
        public bool ChapterHeart;
        public long FileTime;
        public int FileStrawberries;
        public int FileCassettes;
        public int FileHearts;

        public void Update()
        {
            Level scene = Engine.Scene as Level;
            ChapterStarted = scene != null;
            ChapterComplete = ChapterStarted && scene.Completed;
            TimerActive = ChapterStarted && !scene.Completed;
            Chapter = ChapterStarted ? scene.Session.Area.ID : -1;
            Mode = ChapterStarted ? (int)scene.Session.Area.Mode : -1;
            Level = ChapterStarted ? scene.Session.Level : "";
            ChapterTime = ChapterStarted ? scene.Session.Time : 0L;
            FileTime = SaveData.Instance != null ? SaveData.Instance.Time : 0L;
            ChapterStrawberries = ChapterStarted ? scene.Session.Strawberries.Count : 0;
            FileStrawberries = SaveData.Instance != null ? SaveData.Instance.TotalStrawberries : 0;
            ChapterHeart = ChapterStarted && scene.Session.HeartGem;
            FileHearts = SaveData.Instance != null ? SaveData.Instance.TotalHeartGems : 0;
            ChapterCassette = ChapterStarted && scene.Session.Cassette;
            FileCassettes = SaveData.Instance != null ? SaveData.Instance.TotalCassettes : 0;
        }
    }
}
