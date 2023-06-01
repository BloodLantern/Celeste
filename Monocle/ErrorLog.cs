// Decompiled with JetBrains decompiler
// Type: Monocle.ErrorLog
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Monocle
{
    public static class ErrorLog
    {
        public const string Filename = "error_log.txt";
        public const string Marker = "==========================================";

        public static void Write(Exception e)
        {
            Write(e.ToString());
        }

        public static void Write(string str)
        {
            StringBuilder stringBuilder = new();
            string str1 = "";
            if (Path.IsPathRooted(Filename))
            {
                string directoryName = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
            }
            if (File.Exists(Filename))
            {
                StreamReader streamReader = new(Filename);
                str1 = streamReader.ReadToEnd();
                streamReader.Close();
                if (!str1.Contains(Marker))
                    str1 = "";
            }
            _ = Engine.Instance != null ? stringBuilder.Append(Engine.Instance.Title) : stringBuilder.Append("Monocle Engine");

            stringBuilder.AppendLine(" Error Log");
            stringBuilder.AppendLine(Marker);
            stringBuilder.AppendLine();
            if (Engine.Instance != null && Engine.Instance.Version != null)
            {
                stringBuilder.Append("Ver ");
                stringBuilder.AppendLine(Engine.Instance.Version.ToString());
            }
            stringBuilder.AppendLine(DateTime.Now.ToString());
            stringBuilder.AppendLine(str);
            if (str1 != "")
            {
                int startIndex = str1.IndexOf(Marker) + Marker.Length;
                string str2 = str1.Substring(startIndex);
                stringBuilder.AppendLine(str2);
            }
            StreamWriter streamWriter = new(Filename, false);
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
        }

        public static void Open()
        {
            if (!File.Exists(Filename))
                return;

            Process.Start(Filename);
        }
    }
}
