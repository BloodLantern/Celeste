using Monocle;
using System;

namespace Celeste
{
    [Serializable]
    public struct Assists
    {
        public int GameSpeed;
        public bool Invincible;
        public Assists.DashModes DashMode;
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

        public static Assists Default => new Assists()
        {
            GameSpeed = 10
        };

        public void EnfornceAssistMode()
        {
            this.GameSpeed = Calc.Clamp(this.GameSpeed, 5, 10);
            this.MirrorMode = false;
            this.ThreeSixtyDashing = false;
            this.InvisibleMotion = false;
            this.NoGrabbing = false;
            this.LowFriction = false;
            this.SuperDashing = false;
            this.Hiccups = false;
            this.PlayAsBadeline = false;
        }

        public enum DashModes
        {
            Normal,
            Two,
            Infinite,
        }
    }
}
