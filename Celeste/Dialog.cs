// Decompiled with JetBrains decompiler
// Type: Celeste.Dialog
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Monocle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Celeste
{
    public static class Dialog
    {
        public static Language Language = null;
        public static Dictionary<string, Language> Languages;
        public static List<Language> OrderedLanguages;
        private static readonly string[] LanguageDataVariables = new string[7]
        {
            "language",
            "icon",
            "order",
            "split_regex",
            "commas",
            "periods",
            "font"
        };
        private const string path = "Dialog";

        public static void Load()
        {
            Dialog.Language = null;
            Dialog.Languages = new Dictionary<string, Language>();
            foreach (string file in Directory.GetFiles(Path.Combine(Engine.ContentDirectory, nameof(Dialog)), "*.txt", SearchOption.AllDirectories))
            {
                _ = Dialog.LoadLanguage(file);
            }

            Dialog.Language = Settings.Instance != null && Settings.Instance.Language != null && Dialog.Languages.ContainsKey(Settings.Instance.Language)
                ? Dialog.Languages[Settings.Instance.Language]
                : Dialog.Languages.ContainsKey("english")
                ? Dialog.Languages["english"]
                : Dialog.Languages.Count > 0 ? Dialog.Languages.ElementAt<KeyValuePair<string, Language>>(0).Value : throw new Exception("Missing Language Files");

            Settings.Instance.Language = Dialog.Language.Id;
            Dialog.OrderedLanguages = new List<Language>();
            foreach (KeyValuePair<string, Language> language in Dialog.Languages)
            {
                Dialog.OrderedLanguages.Add(language.Value);
            }

            Dialog.OrderedLanguages.Sort((a, b) => a.Order != b.Order ? a.Order - b.Order : a.Id.CompareTo(b.Id));
        }

        public static Language LoadLanguage(string filename)
        {
            Language language = !File.Exists(filename + ".export") ? Language.FromTxt(filename) : Language.FromExport(filename + ".export");
            if (language != null)
            {
                Dialog.Languages[language.Id] = language;
            }

            return language;
        }

        public static void Unload()
        {
            foreach (KeyValuePair<string, Language> language in Dialog.Languages)
            {
                language.Value.Dispose();
            }

            Dialog.Languages.Clear();
            Dialog.Language = null;
            Dialog.OrderedLanguages.Clear();
            Dialog.OrderedLanguages = null;
        }

        public static bool Has(string name, Language language = null)
        {
            language ??= Dialog.Language;
            return language.Dialog.ContainsKey(name);
        }

        public static string Get(string name, Language language = null)
        {
            language ??= Dialog.Language;
            return language.Dialog.TryGetValue(name, out string str) ? str : "XXX";
        }

        public static string Clean(string name, Language language = null)
        {
            language ??= Dialog.Language;
            return language.Cleaned.TryGetValue(name, out string str) ? str : "XXX";
        }

        public static string Time(long ticks)
        {
            TimeSpan timeSpan = TimeSpan.FromTicks(ticks);
            return (int)timeSpan.TotalHours > 0 ? ((int)timeSpan.TotalHours).ToString() + timeSpan.ToString("\\:mm\\:ss\\.fff") : timeSpan.Minutes.ToString() + timeSpan.ToString("\\:ss\\.fff");
        }

        public static string FileTime(long ticks)
        {
            TimeSpan timeSpan = TimeSpan.FromTicks(ticks);
            return timeSpan.TotalHours >= 1.0 ? ((int)timeSpan.TotalHours).ToString() + timeSpan.ToString("\\:mm\\:ss\\.fff") : timeSpan.ToString("mm\\:ss\\.fff");
        }

        public static string Deaths(int deaths)
        {
            return deaths > 999999
                ? (deaths / 1000000f).ToString("0.00") + "m"
                : deaths > 9999 ? (deaths / 1000f).ToString("0.0") + "k" : deaths.ToString();
        }

        public static void CheckCharacters()
        {
            foreach (string file in Directory.GetFiles(Path.Combine(Engine.ContentDirectory, nameof(Dialog)), "*.txt", SearchOption.AllDirectories))
            {
                HashSet<int> intSet = new();
                foreach (string readLine in File.ReadLines(file, Encoding.UTF8))
                {
                    for (int index = 0; index < readLine.Length; ++index)
                    {
                        if (!intSet.Contains(readLine[index]))
                        {
                            _ = intSet.Add(readLine[index]);
                        }
                    }
                }
                List<int> intList = new();
                foreach (int num in intSet)
                {
                    intList.Add(num);
                }

                intList.Sort();
                StringBuilder stringBuilder = new();
                _ = stringBuilder.Append("chars=");
                int num1 = 0;
                Console.WriteLine("Characters of : " + file);
                int num2;
                for (int index1 = 0; index1 < intList.Count; index1 = num2 + 1)
                {
                    bool flag = false;
                    int index2;
                    for (index2 = index1 + 1; index2 < intList.Count && intList[index2] == intList[index2 - 1] + 1; ++index2)
                    {
                        flag = true;
                    }

                    _ = flag
                        ? stringBuilder.Append(intList[index1].ToString() + "-" + intList[index2 - 1] + ",")
                        : stringBuilder.Append(intList[index1].ToString() + ",");

                    num2 = index2 - 1;
                    ++num1;
                    if (num1 >= 10)
                    {
                        num1 = 0;
                        _ = stringBuilder.Remove(stringBuilder.Length - 1, 1);
                        Console.WriteLine(stringBuilder.ToString());
                        _ = stringBuilder.Clear();
                        _ = stringBuilder.Append("chars=");
                    }
                }
                _ = stringBuilder.Remove(stringBuilder.Length - 1, 1);
                Console.WriteLine(stringBuilder.ToString());
                Console.WriteLine();
            }
        }

        public static bool CheckLanguageFontCharacters(string a)
        {
            Language language = Dialog.Languages[a];
            bool flag = true;
            HashSet<int> values = new();
            foreach (KeyValuePair<string, string> keyValuePair in language.Dialog)
            {
                for (int index = 0; index < keyValuePair.Value.Length; ++index)
                {
                    int key = keyValuePair.Value[index];
                    if (!values.Contains(key) && !language.FontSize.Characters.ContainsKey(key))
                    {
                        _ = values.Add(key);
                        flag = false;
                    }
                }
            }
            Console.WriteLine("FONT: " + a);
            if (values.Count > 0)
            {
                Console.WriteLine(" - Missing Characters: " + string.Join<int>(",", values));
            }

            Console.WriteLine(" - OK: " + flag.ToString());
            Console.WriteLine();
            if (values.Count > 0)
            {
                string contents = "";
                foreach (int num in values)
                {
                    contents += ((char)num).ToString();
                }

                File.WriteAllText(a + "-missing-debug.txt", contents);
            }
            return flag;
        }

        public static bool CompareLanguages(string a, string b, bool compareContent)
        {
            Console.WriteLine("COMPARE: " + a + " -> " + b);
            Language language1 = Dialog.Languages[a];
            Language language2 = Dialog.Languages[b];
            bool flag = true;
            List<string> values1 = new();
            List<string> values2 = new();
            List<string> values3 = new();
            foreach (KeyValuePair<string, string> keyValuePair in language1.Dialog)
            {
                if (!language2.Dialog.ContainsKey(keyValuePair.Key))
                {
                    values2.Add(keyValuePair.Key);
                    flag = false;
                }
                else if (compareContent && language2.Dialog[keyValuePair.Key] != language1.Dialog[keyValuePair.Key])
                {
                    values3.Add(keyValuePair.Key);
                    flag = false;
                }
            }
            foreach (KeyValuePair<string, string> keyValuePair in language2.Dialog)
            {
                if (!language1.Dialog.ContainsKey(keyValuePair.Key))
                {
                    values1.Add(keyValuePair.Key);
                    flag = false;
                }
            }
            if (values1.Count > 0)
            {
                Console.WriteLine(" - Missing from " + a + ": " + string.Join(", ", values1));
            }

            if (values2.Count > 0)
            {
                Console.WriteLine(" - Missing from " + b + ": " + string.Join(", ", values2));
            }

            if (values3.Count > 0)
            {
                Console.WriteLine(" - Diff. Content: " + string.Join(", ", values3));
            }

            static List<List<string>> func(string text)
            {
                List<List<string>> stringListList = new();
                foreach (Capture match in Regex.Matches(text, "\\{([^}]*)\\}"))
                {
                    string[] strArray = Regex.Split(match.Value, "(\\{|\\}|\\s)");
                    List<string> stringList = new();
                    foreach (string str in strArray)
                    {
                        if (!string.IsNullOrWhiteSpace(str) && str.Length > 0 && str != "{" && str != "}")
                        {
                            stringList.Add(str);
                        }
                    }
                    stringListList.Add(stringList);
                }
                return stringListList;
            }
            foreach (KeyValuePair<string, string> keyValuePair in language1.Dialog)
            {
                if (language2.Dialog.ContainsKey(keyValuePair.Key))
                {
                    List<List<string>> stringListList1 = func(keyValuePair.Value);
                    List<List<string>> stringListList2 = func(language2.Dialog[keyValuePair.Key]);
                    int index1 = 0;
                    int index2 = 0;
                    for (; index1 < stringListList1.Count; ++index1)
                    {
                        string str = stringListList1[index1][0];
                        if (str is "portrait" or "trigger")
                        {
                            while (index2 < stringListList2.Count && stringListList2[index2][0] != str)
                            {
                                ++index2;
                            }

                            if (index2 >= stringListList2.Count)
                            {
                                Console.WriteLine(" - Command number mismatch in " + keyValuePair.Key + " in " + b);
                                flag = false;
                                index1 = stringListList1.Count;
                            }
                            else
                            {
                                if (str == "portrait")
                                {
                                    for (int index3 = 0; index3 < stringListList1[index1].Count; ++index3)
                                    {
                                        if (stringListList1[index1][index3] != stringListList2[index2][index3])
                                        {
                                            Console.WriteLine(" - Portrait in " + keyValuePair.Key + " is incorrect in " + b + " ({" + string.Join(" ", stringListList1[index1]) + "} vs {" + string.Join(" ", stringListList2[index2]) + "})");
                                            flag = false;
                                        }
                                    }
                                }
                                else if (str == "trigger" && stringListList1[index1][1] != stringListList2[index2][1])
                                {
                                    Console.WriteLine(" - Trigger in " + keyValuePair.Key + " is incorrect in " + b + " (" + stringListList1[index1][1] + " vs " + stringListList2[index2][1] + ")");
                                    flag = false;
                                }
                                ++index2;
                            }
                        }
                    }
                }
            }
            Console.WriteLine(" - OK: " + flag.ToString());
            Console.WriteLine();
            return flag;
        }
    }
}
