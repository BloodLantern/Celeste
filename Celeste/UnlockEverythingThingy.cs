// Decompiled with JetBrains decompiler
// Type: Celeste.UnlockEverythingThingy
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste
{
    public class UnlockEverythingThingy : CheatListener
    {
        public UnlockEverythingThingy()
        {
            AddInput('u', () => Input.MenuUp.Pressed && !Input.MenuUp.Repeating);
            AddInput('d', () => Input.MenuDown.Pressed && !Input.MenuDown.Repeating);
            AddInput('r', () => Input.MenuRight.Pressed && !Input.MenuRight.Repeating);
            AddInput('l', () => Input.MenuLeft.Pressed && !Input.MenuLeft.Repeating);
            AddInput('A', () => Input.MenuConfirm.Pressed);
            AddInput('L', () => Input.MenuJournal.Pressed);
            AddInput('R', () => Input.Grab.Pressed && !Input.MenuJournal.Pressed);
            AddCheat("lrLRuudlRA", new Action(EnteredCheat));
            Logging = true;
        }

        public void EnteredCheat()
        {
            Level level = SceneAs<Level>();
            level.PauseLock = true;
            level.Frozen = true;
            level.Flash(Color.White);
            _ = Audio.Play("event:/game/06_reflection/feather_bubble_get", (Scene as Level).Camera.Position + new Vector2(160f, 90f));
            new FadeWipe(Scene, false, () => UnlockEverything(level)).Duration = 2f;
            RemoveSelf();
        }

        public void UnlockEverything(Level level)
        {
            SaveData.Instance.RevealedChapter9 = true;
            SaveData.Instance.UnlockedAreas = SaveData.Instance.MaxArea;
            SaveData.Instance.CheatMode = true;
            Settings.Instance.Pico8OnMainMenu = true;
            Settings.Instance.VariantsUnlocked = true;
            level.Session.InArea = false;
            Engine.Scene = new LevelExit(LevelExit.Mode.GiveUp, level.Session);
        }
    }
}
