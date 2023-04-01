// Decompiled with JetBrains decompiler
// Type: Celeste.Assists
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    [Serializable]
    public struct Assists
    {
        public int GameSpeed;
        public bool Invincible;
        public DashModes DashMode;
        public bool DashAssist;
        public bool InfiniteStamina;
        public bool MirrorMode;
        public bool ThreeSixtyDashing;
        public bool InvisibleMotion;
        public bool NoGrabbing;
        public bool LowFriction;
        public bool SuperDashing;
        public bool Hiccups;
        public bool PlayAsBadeline;

        public static Assists Default => new()
        {
            GameSpeed = 10
        };

        public void EnfornceAssistMode()
        {
            GameSpeed = Calc.Clamp(GameSpeed, 5, 10);
            MirrorMode = false;
            ThreeSixtyDashing = false;
            InvisibleMotion = false;
            NoGrabbing = false;
            LowFriction = false;
            SuperDashing = false;
            Hiccups = false;
            PlayAsBadeline = false;
        }

        public enum DashModes
        {
            Normal,
            Two,
            Infinite,
        }
    }
}
