// Decompiled with JetBrains decompiler
// Type: FMOD.HandleBase
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
    public class HandleBase
    {
        protected IntPtr rawPtr;

        public HandleBase(IntPtr newPtr)
        {
            rawPtr = newPtr;
        }

        public bool isValid()
        {
            return rawPtr != IntPtr.Zero;
        }

        public IntPtr getRaw()
        {
            return rawPtr;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HandleBase);
        }

        public bool Equals(HandleBase p)
        {
            return p is not null && rawPtr == p.rawPtr;
        }

        public override int GetHashCode()
        {
            return rawPtr.ToInt32();
        }

        public static bool operator ==(HandleBase a, HandleBase b)
        {
            return a == (object)b || (a is not null && b is not null && a.rawPtr == b.rawPtr);
        }

        public static bool operator !=(HandleBase a, HandleBase b)
        {
            return !(a == b);
        }
    }
}
