// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.PARAMETER_DESCRIPTION_INTERNAL
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD.Studio
{
  internal struct PARAMETER_DESCRIPTION_INTERNAL
  {
    public IntPtr name;
    public int index;
    public float minimum;
    public float maximum;
    public float defaultvalue;
    public PARAMETER_TYPE type;

    public void assign(out PARAMETER_DESCRIPTION publicDesc)
    {
      publicDesc.name = MarshallingHelper.stringFromNativeUtf8(this.name);
      publicDesc.index = this.index;
      publicDesc.minimum = this.minimum;
      publicDesc.maximum = this.maximum;
      publicDesc.defaultvalue = this.defaultvalue;
      publicDesc.type = this.type;
    }
  }
}
