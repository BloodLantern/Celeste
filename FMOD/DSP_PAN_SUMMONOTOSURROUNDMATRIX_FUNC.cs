﻿// Decompiled with JetBrains decompiler
// Type: FMOD.DSP_PAN_SUMMONOTOSURROUNDMATRIX_FUNC
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD
{
    public delegate RESULT DSP_PAN_SUMMONOTOSURROUNDMATRIX_FUNC(
        ref DSP_STATE dsp_state,
        int targetSpeakerMode,
        float direction,
        float extent,
        float lowFrequencyGain,
        float overallGain,
        int matrixHop,
        IntPtr matrix);
}
