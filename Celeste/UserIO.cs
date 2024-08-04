using Monocle;
using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

namespace Celeste
{
    public static class UserIO
    {
        public const string SaveDataTitle = "Celeste Save Data";
        private const string SavePath = "Saves";
        private const string BackupPath = "Backups";
        private const string Extension = ".celeste";
        private static bool savingInternal;
        private static bool savingFile;
        private static bool savingSettings;
        private static byte[] savingFileData;
        private static byte[] savingSettingsData;

        private static string GetHandle(string name) => Path.Combine(SavePath, name + Extension);

        private static string GetBackupHandle(string name) => Path.Combine(BackupPath, name + Extension);

        /// <summary>
        /// Opens the IO stream... I suppose. For some reason this function always
        /// returns <see langword="true"/> and <see cref="Close"/> is empty.
        /// </summary>
        /// <param name="mode">The mode to open the stream in.</param>
        /// <returns>Whether the stream could be opened... I suppose. This always returns <see langword="true"/>.</returns>
        public static bool Open(Mode mode) => true;

        public static bool Save<T>(string path, byte[] data) where T : class
        {
            string handle = GetHandle(path);
            bool flag = false;
            try
            {
                string backupHandle = GetBackupHandle(path);
                DirectoryInfo directory1 = new FileInfo(handle).Directory;
                if (!directory1.Exists)
                    directory1.Create();
                DirectoryInfo directory2 = new FileInfo(backupHandle).Directory;
                if (!directory2.Exists)
                    directory2.Create();
                using (FileStream fileStream = File.Open(backupHandle, FileMode.Create, FileAccess.Write))
                    fileStream.Write(data, 0, data.Length);
                if (Load<T>(path, true) is not null)
                {
                    File.Copy(backupHandle, handle, true);
                    flag = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                ErrorLog.Write(ex);
            }
            if (!flag)
                Console.WriteLine("Save Failed");
            return flag;
        }

        /// <summary>
        /// Loads a file and deserializes it as a T.
        /// </summary>
        /// <typeparam name="T">The type of file to deserialize.</typeparam>
        /// <param name="path">The file name without extension.</param>
        /// <param name="backup">Whether the file is a normal save file or a backup one.</param>
        /// <returns>The deserialized file.</returns>
        public static T Load<T>(string path, bool backup = false) where T : class
        {
            string filepath = !backup ? GetHandle(path) : GetBackupHandle(path);
            T obj = default;
            try
            {
                if (File.Exists(filepath))
                {
                    using FileStream fileStream = File.OpenRead(filepath);
                    obj = Deserialize<T>(fileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex);
                ErrorLog.Write(ex);
            }
            return obj;
        }

        /// <summary>
        /// Deserializes a file as a T.
        /// </summary>
        /// <typeparam name="T">The type of file to deserialize.</typeparam>
        /// <param name="stream">The file stream.</param>
        /// <returns>The deserialized file.</returns>
        private static T Deserialize<T>(Stream stream) where T : class => (T) new XmlSerializer(typeof (T)).Deserialize(stream);

        public static bool Exists(string path) => File.Exists(GetHandle(path));

        public static bool Delete(string path)
        {
            string handle = GetHandle(path);
            if (!File.Exists(handle))
                return false;
            File.Delete(handle);
            return true;
        }

        /// <summary>
        /// Closes the IO stream... I suppose. For some reason this function is empty
        /// and <see cref="Open(Mode)"/> always returns <see langword="true"/>.
        /// </summary>
        public static void Close()
        {
        }

        public static byte[] Serialize<T>(T instance)
        {
            using (MemoryStream memoryStream = new())
            {
                new XmlSerializer(typeof (T)).Serialize(memoryStream, instance);
                return memoryStream.ToArray();
            }
        }

        public static bool Saving { get; private set; }

        public static bool SavingResult { get; private set; }

        public static void SaveHandler(bool file, bool settings)
        {
            if (Saving)
                return;
            Saving = true;
            Celeste.SaveRoutine = new Coroutine(SaveRoutine(file, settings));
        }

        private static IEnumerator SaveRoutine(bool file, bool settings)
        {
            savingFile = file;
            savingSettings = settings;
            FileErrorOverlay menu;
            do
            {
                if (savingFile)
                {
                    SaveData.Instance.BeforeSave();
                    savingFileData = Serialize(SaveData.Instance);
                }
                if (savingSettings)
                    savingSettingsData = Serialize(Settings.Instance);
                savingInternal = true;
                SavingResult = false;
                RunThread.Start(UserIO.SaveThread, "USER_IO");
                SaveLoadIcon.Show(Engine.Scene);
                while (savingInternal)
                    yield return null;
                SaveLoadIcon.Hide();
                if (!SavingResult)
                {
                    menu = new FileErrorOverlay(FileErrorOverlay.Error.Save);
                    while (menu.Open)
                        yield return null;
                }
                else
                    break;
            }
            while (menu.TryAgain);
            
            Saving = false;
            Celeste.SaveRoutine = null;
        }

        private static void SaveThread()
        {
            SavingResult = false;
            if (Open(Mode.Write))
            {
                SavingResult = true;
                if (savingFile)
                    SavingResult &= Save<SaveData>(SaveData.GetFilename(), savingFileData);
                if (savingSettings)
                    SavingResult &= Save<Settings>("settings", savingSettingsData);
                Close();
            }
            savingInternal = false;
        }

        /// <summary>
        /// Stream modes. Either Read or Write.
        /// </summary>
        public enum Mode
        {
            /// <summary>
            /// Readonly stream.
            /// </summary>
            Read,
            /// <summary>
            /// Write-readable stream.
            /// </summary>
            Write
        }
    }
}
