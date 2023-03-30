// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_PARAMETER_DESC_UNION
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.Runtime.InteropServices;

namespace FMOD
{
    [StructLayout(LayoutKind.Explicit)]
    public struct DSP_PARAMETER_DESC_UNION
    {
        [FieldOffset(0)]
        public DSP_PARAMETER_DESC_FLOAT floatdesc;
        [FieldOffset(0)]
        public DSP_PARAMETER_DESC_INT intdesc;
        [FieldOffset(0)]
        public DSP_PARAMETER_DESC_BOOL booldesc;
        [FieldOffset(0)]
        public DSP_PARAMETER_DESC_DATA datadesc;
    }
}
