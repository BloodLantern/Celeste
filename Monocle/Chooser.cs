// Decompiled with JetBrains decompiler
// Type: Monocle.Chooser`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Monocle
{
    public class Chooser<T>
    {
        private readonly List<Chooser<T>.Choice> choices;

        public Chooser()
        {
            choices = new List<Chooser<T>.Choice>();
        }

        public Chooser(T firstChoice, float weight)
            : this()
        {
            _ = Add(firstChoice, weight);
        }

        public Chooser(params T[] choices)
            : this()
        {
            foreach (T choice in choices)
            {
                _ = Add(choice, 1f);
            }
        }

        public int Count => choices.Count;

        public T this[int index]
        {
            get => index < 0 || index >= Count ? throw new IndexOutOfRangeException() : choices[index].Value;
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }

                choices[index].Value = value;
            }
        }

        public Chooser<T> Add(T choice, float weight)
        {
            weight = Math.Max(weight, 0.0f);
            choices.Add(new Chooser<T>.Choice(choice, weight));
            TotalWeight += weight;
            return this;
        }

        public T Choose()
        {
            if ((double)TotalWeight <= 0.0)
            {
                return default;
            }

            if (choices.Count == 1)
            {
                return choices[0].Value;
            }

            double num1 = Calc.Random.NextDouble() * (double)TotalWeight;
            float num2 = 0.0f;
            for (int index = 0; index < choices.Count - 1; ++index)
            {
                num2 += choices[index].Weight;
                if (num1 < (double)num2)
                {
                    return choices[index].Value;
                }
            }
            return choices[choices.Count - 1].Value;
        }

        public float TotalWeight { get; private set; }

        public bool CanChoose => (double)TotalWeight > 0.0;

        public static Chooser<TT> FromString<TT>(string data) where TT : IConvertible
        {
            Chooser<TT> chooser = new();
            string[] strArray1 = data.Split(',');
            if (strArray1.Length == 1 && strArray1[0].IndexOf(':') == -1)
            {
                _ = chooser.Add((TT)Convert.ChangeType(strArray1[0], typeof(TT)), 1f);
                return chooser;
            }
            foreach (string str1 in strArray1)
            {
                if (str1.IndexOf(':') == -1)
                {
                    _ = chooser.Add((TT)Convert.ChangeType(str1, typeof(TT)), 1f);
                }
                else
                {
                    string[] strArray2 = str1.Split(':');
                    string str2 = strArray2[0].Trim();
                    string str3 = strArray2[1].Trim();
                    _ = chooser.Add((TT)Convert.ChangeType(str2, typeof(TT)), Convert.ToSingle(str3, CultureInfo.InvariantCulture));
                }
            }
            return chooser;
        }

        private class Choice
        {
            public T Value;
            public float Weight;

            public Choice(T value, float weight)
            {
                Value = value;
                Weight = weight;
            }
        }
    }
}
