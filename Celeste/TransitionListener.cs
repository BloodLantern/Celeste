// Decompiled with JetBrains decompiler
// Type: Celeste.TransitionListener
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;

namespace Celeste
{
    [Tracked(false)]
    public class TransitionListener : Component
    {
        public Action OnInBegin;
        public Action OnInEnd;
        public Action<float> OnIn;
        public Action OnOutBegin;
        public Action<float> OnOut;

        public TransitionListener()
            : base(false, false)
        {
        }
    }
}
