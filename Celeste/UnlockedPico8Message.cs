// Decompiled with JetBrains decompiler
// Type: Celeste.UnlockedPico8Message
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste
{
  public class UnlockedPico8Message : Entity
  {
    private float alpha;
    private string text;
    private bool waitForKeyPress;
    private float timer;
    private Action callback;

    public UnlockedPico8Message(Action callback = null) => this.callback = callback;

    public override void Added(Scene scene)
    {
      base.Added(scene);
      this.Tag = (int) Tags.HUD | (int) Tags.PauseUpdate;
      this.text = ActiveFont.FontSize.AutoNewline(Dialog.Clean("PICO8_UNLOCKED"), 900);
      this.Depth = -10000;
      this.Add((Component) new Coroutine(this.Routine()));
    }

    private IEnumerator Routine()
    {
      UnlockedPico8Message unlockedPico8Message = this;
      Level level = unlockedPico8Message.Scene as Level;
      level.PauseLock = true;
      level.Paused = true;
      while ((double) (unlockedPico8Message.alpha += Engine.DeltaTime / 0.5f) < 1.0)
        yield return (object) null;
      unlockedPico8Message.alpha = 1f;
      unlockedPico8Message.waitForKeyPress = true;
      while (!Input.MenuConfirm.Pressed)
        yield return (object) null;
      unlockedPico8Message.waitForKeyPress = false;
      while ((double) (unlockedPico8Message.alpha -= Engine.DeltaTime / 0.5f) > 0.0)
        yield return (object) null;
      unlockedPico8Message.alpha = 0.0f;
      level.PauseLock = false;
      level.Paused = false;
      unlockedPico8Message.RemoveSelf();
      if (unlockedPico8Message.callback != null)
        unlockedPico8Message.callback();
    }

    public override void Update()
    {
      this.timer += Engine.DeltaTime;
      base.Update();
    }

    public override void Render()
    {
      float num = Ease.CubeOut(this.alpha);
      Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
      GFX.Gui["pico8"].DrawJustified(Celeste.Celeste.TargetCenter + new Vector2(0.0f, (float) (-64.0 * (1.0 - (double) num) - 16.0)), new Vector2(0.5f, 1f), Color.White * num);
      Vector2 position = Celeste.Celeste.TargetCenter + new Vector2(0.0f, (float) (64.0 * (1.0 - (double) num) + 16.0));
      Vector2 vector2 = ActiveFont.Measure(this.text);
      ActiveFont.Draw(this.text, position, new Vector2(0.5f, 0.0f), Vector2.One, Color.White * num);
      if (!this.waitForKeyPress)
        return;
      GFX.Gui["textboxbutton"].DrawCentered(Celeste.Celeste.TargetCenter + new Vector2((float) ((double) vector2.X / 2.0 + 32.0), (float) ((double) vector2.Y + 48.0 + ((double) this.timer % 1.0 < 0.25 ? 6.0 : 0.0))));
    }
  }
}
