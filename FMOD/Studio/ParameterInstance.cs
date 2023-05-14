// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.ParameterInstance
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;

namespace FMOD.Studio
{
    public class ParameterInstance : HandleBase
    {
        public RESULT getDescription(out PARAMETER_DESCRIPTION description)
        {
            description = new PARAMETER_DESCRIPTION();
            RESULT description2 = ParameterInstance.FMOD_Studio_ParameterInstance_GetDescription(rawPtr, out PARAMETER_DESCRIPTION_INTERNAL description1);
            if (description2 != RESULT.OK)
            {
                return description2;
            }

            description1.assign(out description);
            return description2;
        }

        public RESULT getValue(out float value)
        {
            return ParameterInstance.FMOD_Studio_ParameterInstance_GetValue(rawPtr, out value);
        }

        public RESULT setValue(float value)
        {
            return ParameterInstance.FMOD_Studio_ParameterInstance_SetValue(rawPtr, value);
        }

        [DllImport("fmodstudio")]
        private static extern bool FMOD_Studio_ParameterInstance_IsValid(IntPtr parameter);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_ParameterInstance_GetDescription(
            IntPtr parameter,
            out PARAMETER_DESCRIPTION_INTERNAL description);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_ParameterInstance_GetValue(
            IntPtr parameter,
            out float value);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_ParameterInstance_SetValue(
            IntPtr parameter,
            float value);

        public ParameterInstance(IntPtr raw)
            : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return ParameterInstance.FMOD_Studio_ParameterInstance_IsValid(rawPtr);
        }
    }
}
