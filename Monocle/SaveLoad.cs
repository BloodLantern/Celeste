using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Monocle
{
    public static class SaveLoad
    {
        public static void SerializeToFile<T>(T obj, string filepath, SerializeModes mode)
        {
            using (FileStream serializationStream = new(filepath, FileMode.Create))
            {
                if (mode == SerializeModes.Binary)
                {
                    new BinaryFormatter().Serialize(serializationStream, obj);
                }
                else
                {
                    if (mode != SerializeModes.XML)
                        return;
                    new XmlSerializer(typeof (T)).Serialize(serializationStream, obj);
                }
            }
        }

        public static bool SafeSerializeToFile<T>(T obj, string filepath, SerializeModes mode)
        {
            try
            {
                SerializeToFile<T>(obj, filepath, mode);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static T DeserializeFromFile<T>(string filepath, SerializeModes mode)
        {
            using (FileStream serializationStream = File.OpenRead(filepath))
                return mode == SerializeModes.Binary ? (T) new BinaryFormatter().Deserialize(serializationStream) : (T) new XmlSerializer(typeof (T)).Deserialize(serializationStream);
        }

        public static T SafeDeserializeFromFile<T>(
            string filepath,
            SerializeModes mode,
            bool debugUnsafe = false)
        {
            if (!File.Exists(filepath))
                return default (T);
            if (debugUnsafe)
                return DeserializeFromFile<T>(filepath, mode);
            try
            {
                return DeserializeFromFile<T>(filepath, mode);
            }
            catch
            {
                return default (T);
            }
        }

        public static T SafeDeserializeFromFile<T>(
            string filepath,
            SerializeModes mode,
            out bool loadError,
            bool debugUnsafe = false)
        {
            if (File.Exists(filepath))
            {
                if (debugUnsafe)
                {
                    loadError = false;
                    return DeserializeFromFile<T>(filepath, mode);
                }
                try
                {
                    loadError = false;
                    return DeserializeFromFile<T>(filepath, mode);
                }
                catch
                {
                    loadError = true;
                    return default (T);
                }
            }
            else
            {
                loadError = false;
                return default (T);
            }
        }

        public enum SerializeModes
        {
            Binary,
            XML,
        }
    }
}
