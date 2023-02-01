// Decompiled with JetBrains decompiler
// Type: Celeste.AltMusicTrigger
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;

namespace Celeste
{
  public class AltMusicTrigger : Trigger
  {
    public string Track;
    public bool ResetOnLeave;

    public AltMusicTrigger(EntityData data, Vector2 offset)
      : base(data, offset)
    {
      this.Track = data.Attr("track");
      this.ResetOnLeave = data.Bool("resetOnLeave", true);
    }

    public override void OnEnter(Player player) => Audio.SetAltMusic(SFX.EventnameByHandle(this.Track));

    public override void OnLeave(Player player)
    {
      if (!this.ResetOnLeave)
        return;
      Audio.SetAltMusic((string) null);
    }
  }
}
