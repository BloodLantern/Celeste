// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.Bank
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    public class Bank : HandleBase
    {
        public RESULT getID(out Guid id)
        {
            return Bank.FMOD_Studio_Bank_GetID(rawPtr, out id);
        }

        public RESULT getPath(out string path)
        {
            path = null;
            byte[] numArray = new byte[256];
            RESULT path1 = Bank.FMOD_Studio_Bank_GetPath(rawPtr, numArray, numArray.Length, out int retrieved);
            if (path1 == RESULT.ERR_TRUNCATED)
            {
                numArray = new byte[retrieved];
                path1 = Bank.FMOD_Studio_Bank_GetPath(rawPtr, numArray, numArray.Length, out retrieved);
            }
            if (path1 == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(numArray, 0, retrieved - 1);
            }

            return path1;
        }

        public RESULT unload()
        {
            RESULT result = Bank.FMOD_Studio_Bank_Unload(rawPtr);
            if (result != RESULT.OK)
            {
                return result;
            }

            rawPtr = IntPtr.Zero;
            return RESULT.OK;
        }

        public RESULT loadSampleData()
        {
            return Bank.FMOD_Studio_Bank_LoadSampleData(rawPtr);
        }

        public RESULT unloadSampleData()
        {
            return Bank.FMOD_Studio_Bank_UnloadSampleData(rawPtr);
        }

        public RESULT getLoadingState(out LOADING_STATE state)
        {
            return Bank.FMOD_Studio_Bank_GetLoadingState(rawPtr, out state);
        }

        public RESULT getSampleLoadingState(out LOADING_STATE state)
        {
            return Bank.FMOD_Studio_Bank_GetSampleLoadingState(rawPtr, out state);
        }

        public RESULT getStringCount(out int count)
        {
            return Bank.FMOD_Studio_Bank_GetStringCount(rawPtr, out count);
        }

        public RESULT getStringInfo(int index, out Guid id, out string path)
        {
            path = null;
            byte[] numArray = new byte[256];
            RESULT stringInfo = Bank.FMOD_Studio_Bank_GetStringInfo(rawPtr, index, out id, numArray, numArray.Length, out int retrieved);
            if (stringInfo == RESULT.ERR_TRUNCATED)
            {
                numArray = new byte[retrieved];
                stringInfo = Bank.FMOD_Studio_Bank_GetStringInfo(rawPtr, index, out id, numArray, numArray.Length, out retrieved);
            }
            if (stringInfo == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(numArray, 0, retrieved - 1);
            }

            return RESULT.OK;
        }

        public RESULT getEventCount(out int count)
        {
            return Bank.FMOD_Studio_Bank_GetEventCount(rawPtr, out count);
        }

        public RESULT getEventList(out EventDescription[] array)
        {
            array = null;
            RESULT eventCount = Bank.FMOD_Studio_Bank_GetEventCount(rawPtr, out int count1);
            if (eventCount != RESULT.OK)
            {
                return eventCount;
            }

            if (count1 == 0)
            {
                array = new EventDescription[0];
                return eventCount;
            }
            IntPtr[] array1 = new IntPtr[count1];
            RESULT eventList = Bank.FMOD_Studio_Bank_GetEventList(rawPtr, array1, count1, out int count2);
            if (eventList != RESULT.OK)
            {
                return eventList;
            }

            if (count2 > count1)
            {
                count2 = count1;
            }

            array = new EventDescription[count2];
            for (int index = 0; index < count2; ++index)
            {
                array[index] = new EventDescription(array1[index]);
            }

            return RESULT.OK;
        }

        public RESULT getBusCount(out int count)
        {
            return Bank.FMOD_Studio_Bank_GetBusCount(rawPtr, out count);
        }

        public RESULT getBusList(out Bus[] array)
        {
            array = null;
            RESULT busCount = Bank.FMOD_Studio_Bank_GetBusCount(rawPtr, out int count1);
            if (busCount != RESULT.OK)
            {
                return busCount;
            }

            if (count1 == 0)
            {
                array = new Bus[0];
                return busCount;
            }
            IntPtr[] array1 = new IntPtr[count1];
            RESULT busList = Bank.FMOD_Studio_Bank_GetBusList(rawPtr, array1, count1, out int count2);
            if (busList != RESULT.OK)
            {
                return busList;
            }

            if (count2 > count1)
            {
                count2 = count1;
            }

            array = new Bus[count2];
            for (int index = 0; index < count2; ++index)
            {
                array[index] = new Bus(array1[index]);
            }

            return RESULT.OK;
        }

        public RESULT getVCACount(out int count)
        {
            return Bank.FMOD_Studio_Bank_GetVCACount(rawPtr, out count);
        }

        public RESULT getVCAList(out VCA[] array)
        {
            array = null;
            RESULT vcaCount = Bank.FMOD_Studio_Bank_GetVCACount(rawPtr, out int count1);
            if (vcaCount != RESULT.OK)
            {
                return vcaCount;
            }

            if (count1 == 0)
            {
                array = new VCA[0];
                return vcaCount;
            }
            IntPtr[] array1 = new IntPtr[count1];
            RESULT vcaList = Bank.FMOD_Studio_Bank_GetVCAList(rawPtr, array1, count1, out int count2);
            if (vcaList != RESULT.OK)
            {
                return vcaList;
            }

            if (count2 > count1)
            {
                count2 = count1;
            }

            array = new VCA[count2];
            for (int index = 0; index < count2; ++index)
            {
                array[index] = new VCA(array1[index]);
            }

            return RESULT.OK;
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return Bank.FMOD_Studio_Bank_GetUserData(rawPtr, out userdata);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return Bank.FMOD_Studio_Bank_SetUserData(rawPtr, userdata);
        }

        [DllImport("fmodstudio")]
        private static extern bool FMOD_Studio_Bank_IsValid(IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetID(IntPtr bank, out Guid id);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetPath(
            IntPtr bank,
            [Out] byte[] path,
            int size,
            out int retrieved);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_Unload(IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_LoadSampleData(IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_UnloadSampleData(IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetLoadingState(
            IntPtr bank,
            out LOADING_STATE state);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetSampleLoadingState(
            IntPtr bank,
            out LOADING_STATE state);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetStringCount(IntPtr bank, out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetStringInfo(
            IntPtr bank,
            int index,
            out Guid id,
            [Out] byte[] path,
            int size,
            out int retrieved);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetEventCount(IntPtr bank, out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetEventList(
            IntPtr bank,
            IntPtr[] array,
            int capacity,
            out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetBusCount(IntPtr bank, out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetBusList(
            IntPtr bank,
            IntPtr[] array,
            int capacity,
            out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetVCACount(IntPtr bank, out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetVCAList(
            IntPtr bank,
            IntPtr[] array,
            int capacity,
            out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_GetUserData(
            IntPtr studiosystem,
            out IntPtr userdata);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_Bank_SetUserData(
            IntPtr studiosystem,
            IntPtr userdata);

        public Bank(IntPtr raw)
            : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return Bank.FMOD_Studio_Bank_IsValid(rawPtr);
        }
    }
}
