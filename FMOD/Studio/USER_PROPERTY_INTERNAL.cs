﻿// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.USER_PROPERTY_INTERNAL
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;

namespace FMOD.Studio
{
    internal struct USER_PROPERTY_INTERNAL
    {
        private readonly IntPtr name;
        private readonly USER_PROPERTY_TYPE type;
        private Union_IntBoolFloatString value;

        public USER_PROPERTY createPublic()
        {
            USER_PROPERTY userProperty = new()
            {
                name = MarshallingHelper.stringFromNativeUtf8(name),
                type = type
            };
            switch (type)
            {
                case USER_PROPERTY_TYPE.INTEGER:
                    userProperty.intvalue = value.intvalue;
                    break;
                case USER_PROPERTY_TYPE.BOOLEAN:
                    userProperty.boolvalue = value.boolvalue;
                    break;
                case USER_PROPERTY_TYPE.FLOAT:
                    userProperty.floatvalue = value.floatvalue;
                    break;
                case USER_PROPERTY_TYPE.STRING:
                    userProperty.stringvalue = MarshallingHelper.stringFromNativeUtf8(value.stringvalue);
                    break;
            }
            return userProperty;
        }
    }
}
