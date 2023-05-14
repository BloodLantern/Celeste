// Decompiled with JetBrains decompiler
// Type: FMOD.Studio.System
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FMOD.Studio
{
    public class System : HandleBase
    {
        public static RESULT create(out FMOD.Studio.System studiosystem)
        {
            studiosystem = null;
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_Create(out IntPtr studiosystem1, 69652U);
            if (result != RESULT.OK)
            {
                return result;
            }

            studiosystem = new FMOD.Studio.System(studiosystem1);
            return result;
        }

        public RESULT setAdvancedSettings(ADVANCEDSETTINGS settings)
        {
            settings.cbsize = Marshal.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD.Studio.System.FMOD_Studio_System_SetAdvancedSettings(rawPtr, ref settings);
        }

        public RESULT getAdvancedSettings(out ADVANCEDSETTINGS settings)
        {
            settings.cbsize = Marshal.SizeOf(typeof(ADVANCEDSETTINGS));
            return FMOD.Studio.System.FMOD_Studio_System_GetAdvancedSettings(rawPtr, out settings);
        }

        public RESULT initialize(
            int maxchannels,
            INITFLAGS studioFlags,
            FMOD.INITFLAGS flags,
            IntPtr extradriverdata)
        {
            return FMOD.Studio.System.FMOD_Studio_System_Initialize(rawPtr, maxchannels, studioFlags, flags, extradriverdata);
        }

        public RESULT release()
        {
            return FMOD.Studio.System.FMOD_Studio_System_Release(rawPtr);
        }

        public RESULT update()
        {
            return FMOD.Studio.System.FMOD_Studio_System_Update(rawPtr);
        }

        public RESULT getLowLevelSystem(out FMOD.System system)
        {
            system = null;
            _ = new IntPtr();
            RESULT lowLevelSystem = FMOD.Studio.System.FMOD_Studio_System_GetLowLevelSystem(rawPtr, out IntPtr system1);
            if (lowLevelSystem != RESULT.OK)
            {
                return lowLevelSystem;
            }

            system = new FMOD.System(system1);
            return lowLevelSystem;
        }

        public RESULT getEvent(string path, out EventDescription _event)
        {
            _event = null;
            _ = new IntPtr();
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_GetEvent(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), out IntPtr description);
            if (result != RESULT.OK)
            {
                return result;
            }

            _event = new EventDescription(description);
            return result;
        }

        public RESULT getBus(string path, out Bus bus)
        {
            bus = null;
            _ = new IntPtr();
            RESULT bus2 = FMOD.Studio.System.FMOD_Studio_System_GetBus(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), out IntPtr bus1);
            if (bus2 != RESULT.OK)
            {
                return bus2;
            }

            bus = new Bus(bus1);
            return bus2;
        }

        public RESULT getVCA(string path, out VCA vca)
        {
            vca = null;
            _ = new IntPtr();
            RESULT vca2 = FMOD.Studio.System.FMOD_Studio_System_GetVCA(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), out IntPtr vca1);
            if (vca2 != RESULT.OK)
            {
                return vca2;
            }

            vca = new VCA(vca1);
            return vca2;
        }

        public RESULT getBank(string path, out Bank bank)
        {
            bank = null;
            _ = new IntPtr();
            RESULT bank2 = FMOD.Studio.System.FMOD_Studio_System_GetBank(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), out IntPtr bank1);
            if (bank2 != RESULT.OK)
            {
                return bank2;
            }

            bank = new Bank(bank1);
            return bank2;
        }

        public RESULT getEventByID(Guid guid, out EventDescription _event)
        {
            _event = null;
            _ = new IntPtr();
            RESULT eventById = FMOD.Studio.System.FMOD_Studio_System_GetEventByID(rawPtr, ref guid, out IntPtr description);
            if (eventById != RESULT.OK)
            {
                return eventById;
            }

            _event = new EventDescription(description);
            return eventById;
        }

        public RESULT getBusByID(Guid guid, out Bus bus)
        {
            bus = null;
            _ = new IntPtr();
            RESULT busById = FMOD.Studio.System.FMOD_Studio_System_GetBusByID(rawPtr, ref guid, out IntPtr bus1);
            if (busById != RESULT.OK)
            {
                return busById;
            }

            bus = new Bus(bus1);
            return busById;
        }

        public RESULT getVCAByID(Guid guid, out VCA vca)
        {
            vca = null;
            _ = new IntPtr();
            RESULT vcaById = FMOD.Studio.System.FMOD_Studio_System_GetVCAByID(rawPtr, ref guid, out IntPtr vca1);
            if (vcaById != RESULT.OK)
            {
                return vcaById;
            }

            vca = new VCA(vca1);
            return vcaById;
        }

        public RESULT getBankByID(Guid guid, out Bank bank)
        {
            bank = null;
            _ = new IntPtr();
            RESULT bankById = FMOD.Studio.System.FMOD_Studio_System_GetBankByID(rawPtr, ref guid, out IntPtr bank1);
            if (bankById != RESULT.OK)
            {
                return bankById;
            }

            bank = new Bank(bank1);
            return bankById;
        }

        public RESULT getSoundInfo(string key, out SOUND_INFO info)
        {
            RESULT soundInfo = FMOD.Studio.System.FMOD_Studio_System_GetSoundInfo(rawPtr, Encoding.UTF8.GetBytes(key + "\0"), out SOUND_INFO_INTERNAL info1);
            if (soundInfo != RESULT.OK)
            {
                info = new SOUND_INFO();
                return soundInfo;
            }
            info1.assign(out info);
            return soundInfo;
        }

        public RESULT lookupID(string path, out Guid guid)
        {
            return FMOD.Studio.System.FMOD_Studio_System_LookupID(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), out guid);
        }

        public RESULT lookupPath(Guid guid, out string path)
        {
            path = null;
            byte[] numArray = new byte[256];
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_LookupPath(rawPtr, ref guid, numArray, numArray.Length, out int retrieved);
            if (result == RESULT.ERR_TRUNCATED)
            {
                numArray = new byte[retrieved];
                result = FMOD.Studio.System.FMOD_Studio_System_LookupPath(rawPtr, ref guid, numArray, numArray.Length, out retrieved);
            }
            if (result == RESULT.OK)
            {
                path = Encoding.UTF8.GetString(numArray, 0, retrieved - 1);
            }

            return result;
        }

        public RESULT getNumListeners(out int numlisteners)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetNumListeners(rawPtr, out numlisteners);
        }

        public RESULT setNumListeners(int numlisteners)
        {
            return FMOD.Studio.System.FMOD_Studio_System_SetNumListeners(rawPtr, numlisteners);
        }

        public RESULT getListenerAttributes(int listener, out _3D_ATTRIBUTES attributes)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetListenerAttributes(rawPtr, listener, out attributes);
        }

        public RESULT setListenerAttributes(int listener, _3D_ATTRIBUTES attributes)
        {
            return FMOD.Studio.System.FMOD_Studio_System_SetListenerAttributes(rawPtr, listener, ref attributes);
        }

        public RESULT getListenerWeight(int listener, out float weight)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetListenerWeight(rawPtr, listener, out weight);
        }

        public RESULT setListenerWeight(int listener, float weight)
        {
            return FMOD.Studio.System.FMOD_Studio_System_SetListenerWeight(rawPtr, listener, weight);
        }

        public RESULT loadBankFile(string name, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;
            _ = new IntPtr();
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_LoadBankFile(rawPtr, Encoding.UTF8.GetBytes(name + "\0"), flags, out IntPtr bank1);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(bank1);
            return result;
        }

        public RESULT loadBankMemory(byte[] buffer, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;
            _ = new IntPtr();
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_LoadBankMemory(rawPtr, buffer, buffer.Length, LOAD_MEMORY_MODE.LOAD_MEMORY, flags, out IntPtr bank1);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(bank1);
            return result;
        }

        public RESULT loadBankCustom(BANK_INFO info, LOAD_BANK_FLAGS flags, out Bank bank)
        {
            bank = null;
            info.size = Marshal.SizeOf((object)info);
            _ = new IntPtr();
            RESULT result = FMOD.Studio.System.FMOD_Studio_System_LoadBankCustom(rawPtr, ref info, flags, out IntPtr bank1);
            if (result != RESULT.OK)
            {
                return result;
            }

            bank = new Bank(bank1);
            return result;
        }

        public RESULT unloadAll()
        {
            return FMOD.Studio.System.FMOD_Studio_System_UnloadAll(rawPtr);
        }

        public RESULT flushCommands()
        {
            return FMOD.Studio.System.FMOD_Studio_System_FlushCommands(rawPtr);
        }

        public RESULT flushSampleLoading()
        {
            return FMOD.Studio.System.FMOD_Studio_System_FlushSampleLoading(rawPtr);
        }

        public RESULT startCommandCapture(string path, COMMANDCAPTURE_FLAGS flags)
        {
            return FMOD.Studio.System.FMOD_Studio_System_StartCommandCapture(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), flags);
        }

        public RESULT stopCommandCapture()
        {
            return FMOD.Studio.System.FMOD_Studio_System_StopCommandCapture(rawPtr);
        }

        public RESULT loadCommandReplay(
            string path,
            COMMANDREPLAY_FLAGS flags,
            out CommandReplay replay)
        {
            replay = null;
            _ = new IntPtr();
            int num = (int)FMOD.Studio.System.FMOD_Studio_System_LoadCommandReplay(rawPtr, Encoding.UTF8.GetBytes(path + "\0"), flags, out IntPtr commandReplay);
            if (num != 0)
            {
                return (RESULT)num;
            }

            replay = new CommandReplay(commandReplay);
            return (RESULT)num;
        }

        public RESULT getBankCount(out int count)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetBankCount(rawPtr, out count);
        }

        public RESULT getBankList(out Bank[] array)
        {
            array = null;
            RESULT bankCount = FMOD.Studio.System.FMOD_Studio_System_GetBankCount(rawPtr, out int count1);
            if (bankCount != RESULT.OK)
            {
                return bankCount;
            }

            if (count1 == 0)
            {
                array = new Bank[0];
                return bankCount;
            }
            IntPtr[] array1 = new IntPtr[count1];
            RESULT bankList = FMOD.Studio.System.FMOD_Studio_System_GetBankList(rawPtr, array1, count1, out int count2);
            if (bankList != RESULT.OK)
            {
                return bankList;
            }

            if (count2 > count1)
            {
                count2 = count1;
            }

            array = new Bank[count2];
            for (int index = 0; index < count2; ++index)
            {
                array[index] = new Bank(array1[index]);
            }

            return RESULT.OK;
        }

        public RESULT getCPUUsage(out CPU_USAGE usage)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetCPUUsage(rawPtr, out usage);
        }

        public RESULT getBufferUsage(out BUFFER_USAGE usage)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetBufferUsage(rawPtr, out usage);
        }

        public RESULT resetBufferUsage()
        {
            return FMOD.Studio.System.FMOD_Studio_System_ResetBufferUsage(rawPtr);
        }

        public RESULT setCallback(SYSTEM_CALLBACK callback, SYSTEM_CALLBACK_TYPE callbackmask = SYSTEM_CALLBACK_TYPE.ALL)
        {
            return FMOD.Studio.System.FMOD_Studio_System_SetCallback(rawPtr, callback, callbackmask);
        }

        public RESULT getUserData(out IntPtr userdata)
        {
            return FMOD.Studio.System.FMOD_Studio_System_GetUserData(rawPtr, out userdata);
        }

        public RESULT setUserData(IntPtr userdata)
        {
            return FMOD.Studio.System.FMOD_Studio_System_SetUserData(rawPtr, userdata);
        }

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_Create(
            out IntPtr studiosystem,
            uint headerversion);

        [DllImport("fmodstudio")]
        private static extern bool FMOD_Studio_System_IsValid(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetAdvancedSettings(
            IntPtr studiosystem,
            ref ADVANCEDSETTINGS settings);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetAdvancedSettings(
            IntPtr studiosystem,
            out ADVANCEDSETTINGS settings);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_Initialize(
            IntPtr studiosystem,
            int maxchannels,
            INITFLAGS studioFlags,
            FMOD.INITFLAGS flags,
            IntPtr extradriverdata);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_Release(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_Update(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetLowLevelSystem(
            IntPtr studiosystem,
            out IntPtr system);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetEvent(
            IntPtr studiosystem,
            byte[] path,
            out IntPtr description);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBus(
            IntPtr studiosystem,
            byte[] path,
            out IntPtr bus);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetVCA(
            IntPtr studiosystem,
            byte[] path,
            out IntPtr vca);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBank(
            IntPtr studiosystem,
            byte[] path,
            out IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetEventByID(
            IntPtr studiosystem,
            ref Guid guid,
            out IntPtr description);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBusByID(
            IntPtr studiosystem,
            ref Guid guid,
            out IntPtr bus);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetVCAByID(
            IntPtr studiosystem,
            ref Guid guid,
            out IntPtr vca);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBankByID(
            IntPtr studiosystem,
            ref Guid guid,
            out IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetSoundInfo(
            IntPtr studiosystem,
            byte[] key,
            out SOUND_INFO_INTERNAL info);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LookupID(
            IntPtr studiosystem,
            byte[] path,
            out Guid guid);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LookupPath(
            IntPtr studiosystem,
            ref Guid guid,
            [Out] byte[] path,
            int size,
            out int retrieved);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetNumListeners(
            IntPtr studiosystem,
            out int numlisteners);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetNumListeners(
            IntPtr studiosystem,
            int numlisteners);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetListenerAttributes(
            IntPtr studiosystem,
            int listener,
            out _3D_ATTRIBUTES attributes);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetListenerAttributes(
            IntPtr studiosystem,
            int listener,
            ref _3D_ATTRIBUTES attributes);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetListenerWeight(
            IntPtr studiosystem,
            int listener,
            out float weight);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetListenerWeight(
            IntPtr studiosystem,
            int listener,
            float weight);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LoadBankFile(
            IntPtr studiosystem,
            byte[] filename,
            LOAD_BANK_FLAGS flags,
            out IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LoadBankMemory(
            IntPtr studiosystem,
            byte[] buffer,
            int length,
            LOAD_MEMORY_MODE mode,
            LOAD_BANK_FLAGS flags,
            out IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LoadBankCustom(
            IntPtr studiosystem,
            ref BANK_INFO info,
            LOAD_BANK_FLAGS flags,
            out IntPtr bank);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_UnloadAll(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_FlushCommands(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_FlushSampleLoading(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_StartCommandCapture(
            IntPtr studiosystem,
            byte[] path,
            COMMANDCAPTURE_FLAGS flags);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_StopCommandCapture(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_LoadCommandReplay(
            IntPtr studiosystem,
            byte[] path,
            COMMANDREPLAY_FLAGS flags,
            out IntPtr commandReplay);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBankCount(
            IntPtr studiosystem,
            out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBankList(
            IntPtr studiosystem,
            IntPtr[] array,
            int capacity,
            out int count);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetCPUUsage(
            IntPtr studiosystem,
            out CPU_USAGE usage);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetBufferUsage(
            IntPtr studiosystem,
            out BUFFER_USAGE usage);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_ResetBufferUsage(IntPtr studiosystem);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetCallback(
            IntPtr studiosystem,
            SYSTEM_CALLBACK callback,
            SYSTEM_CALLBACK_TYPE callbackmask);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_GetUserData(
            IntPtr studiosystem,
            out IntPtr userdata);

        [DllImport("fmodstudio")]
        private static extern RESULT FMOD_Studio_System_SetUserData(
            IntPtr studiosystem,
            IntPtr userdata);

        public System(IntPtr raw)
            : base(raw)
        {
        }

        protected override bool isValidInternal()
        {
            return FMOD.Studio.System.FMOD_Studio_System_IsValid(rawPtr);
        }
    }
}
