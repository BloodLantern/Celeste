// Decompiled with JetBrains decompiler
// Type: Monocle.SaveLoad
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;

namespace Monocle
{
  public static class SaveLoad
  {
    public static void SerializeToFile<T>(T obj, string filepath, SaveLoad.SerializeModes mode)
    {
      using (FileStream serializationStream = new FileStream(filepath, FileMode.Create))
      {
        if (mode == SaveLoad.SerializeModes.Binary)
        {
          new BinaryFormatter().Serialize((Stream) serializationStream, (object) obj);
        }
        else
        {
          if (mode != SaveLoad.SerializeModes.XML)
            return;
          new XmlSerializer(typeof (T)).Serialize((Stream) serializationStream, (object) obj);
        }
      }
    }

    public static bool SafeSerializeToFile<T>(T obj, string filepath, SaveLoad.SerializeModes mode)
    {
      try
      {
        SaveLoad.SerializeToFile<T>(obj, filepath, mode);
        return true;
      }
      catch
      {
        return false;
      }
    }

    public static T DeserializeFromFile<T>(string filepath, SaveLoad.SerializeModes mode)
    {
      using (FileStream serializationStream = File.OpenRead(filepath))
        return mode == SaveLoad.SerializeModes.Binary ? (T) new BinaryFormatter().Deserialize((Stream) serializationStream) : (T) new XmlSerializer(typeof (T)).Deserialize((Stream) serializationStream);
    }

    public static T SafeDeserializeFromFile<T>(
      string filepath,
      SaveLoad.SerializeModes mode,
      bool debugUnsafe = false)
    {
      if (!File.Exists(filepath))
        return default (T);
      if (debugUnsafe)
        return SaveLoad.DeserializeFromFile<T>(filepath, mode);
      try
      {
        return SaveLoad.DeserializeFromFile<T>(filepath, mode);
      }
      catch
      {
        return default (T);
      }
    }

    public static T SafeDeserializeFromFile<T>(
      string filepath,
      SaveLoad.SerializeModes mode,
      out bool loadError,
      bool debugUnsafe = false)
    {
      if (File.Exists(filepath))
      {
        if (debugUnsafe)
        {
          loadError = false;
          return SaveLoad.DeserializeFromFile<T>(filepath, mode);
        }
        try
        {
          loadError = false;
          return SaveLoad.DeserializeFromFile<T>(filepath, mode);
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
