// Decompiled with JetBrains decompiler
// Type: Monocle.VirtualInput
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

namespace Monocle
{
    public abstract class VirtualInput
    {
        public VirtualInput() => MInput.VirtualInputs.Add(this);

        public void Deregister() => MInput.VirtualInputs.Remove(this);

        public abstract void Update();

        public enum OverlapBehaviors
        {
            CancelOut,
            TakeOlder,
            TakeNewer,
        }

        public enum ThresholdModes
        {
            LargerThan,
            LessThan,
            EqualTo,
        }
    }
}
