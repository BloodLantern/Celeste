// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.HandleBase
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD.Studio
{
  public abstract class HandleBase
  {
    protected IntPtr rawPtr;

    public HandleBase(IntPtr newPtr) => this.rawPtr = newPtr;

    public bool isValid() => this.rawPtr != IntPtr.Zero && this.isValidInternal();

    protected abstract bool isValidInternal();

    public IntPtr getRaw() => this.rawPtr;

    public override bool Equals(object obj) => this.Equals(obj as HandleBase);

    public bool Equals(HandleBase p) => (object) p != null && this.rawPtr == p.rawPtr;

    public override int GetHashCode() => this.rawPtr.ToInt32();

    public static bool operator ==(HandleBase a, HandleBase b)
    {
      if ((object) a == (object) b)
        return true;
      return (object) a != null && (object) b != null && a.rawPtr == b.rawPtr;
    }

    public static bool operator !=(HandleBase a, HandleBase b) => !(a == b);
  }
}
