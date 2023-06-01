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

        public static void Write(Exception e) => ErrorLog.Write(e.ToString());

        public static void Write(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string str1 = "";
            if (Path.IsPathRooted("error_log.txt"))
            {
                string directoryName = Path.GetDirectoryName("error_log.txt");
                if (!Directory.Exists(directoryName))
                    Directory.CreateDirectory(directoryName);
            }
            if (File.Exists("error_log.txt"))
            {
                StreamReader streamReader = new StreamReader("error_log.txt");
                str1 = streamReader.ReadToEnd();
                streamReader.Close();
                if (!str1.Contains("=========================================="))
                    str1 = "";
            }
            if (Engine.Instance != null)
                stringBuilder.Append(Engine.Instance.Title);
            else
                stringBuilder.Append("Monocle Engine");
            stringBuilder.AppendLine(" Error Log");
            stringBuilder.AppendLine("==========================================");
            stringBuilder.AppendLine();
            if (Engine.Instance != null && Engine.Instance.Version != (Version) null)
            {
                stringBuilder.Append("Ver ");
                stringBuilder.AppendLine(Engine.Instance.Version.ToString());
            }
            stringBuilder.AppendLine(DateTime.Now.ToString());
            stringBuilder.AppendLine(str);
            if (str1 != "")
            {
                int startIndex = str1.IndexOf("==========================================") + "==========================================".Length;
                string str2 = str1.Substring(startIndex);
                stringBuilder.AppendLine(str2);
            }
            StreamWriter streamWriter = new StreamWriter("error_log.txt", false);
            streamWriter.Write(stringBuilder.ToString());
            streamWriter.Close();
        }

        public static void Open()
        {
            if (!File.Exists("error_log.txt"))
                return;
            Process.Start("error_log.txt");
        }
    }
}
