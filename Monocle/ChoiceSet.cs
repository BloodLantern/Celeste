// Decompiled with JetBrains decompiler
// Type: Monocle.ChoiceSet`1
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class ChoiceSet<T>
    {
        private readonly Dictionary<T, int> choices;

        public int TotalWeight { get; private set; }

        public ChoiceSet()
        {
            choices = new Dictionary<T, int>();
            TotalWeight = 0;
        }

        public void Set(T choice, int weight)
        {
            _ = choices.TryGetValue(choice, out int num);
            TotalWeight -= num;
            if (weight <= 0)
            {
                if (!choices.ContainsKey(choice))
                {
                    return;
                }

                _ = choices.Remove(choice);
            }
            else
            {
                TotalWeight += weight;
                choices[choice] = weight;
            }
        }

        public int this[T choice]
        {
            get
            {
                _ = choices.TryGetValue(choice, out int num);
                return num;
            }
            set => Set(choice, value);
        }

        public void Set(T choice, float chance)
        {
            _ = choices.TryGetValue(choice, out int num1);
            TotalWeight -= num1;
            int num2 = (int)Math.Round(TotalWeight / (1.0 - (double)chance));
            if (num2 <= 0 && (double)chance > 0.0)
            {
                num2 = 1;
            }

            if (num2 <= 0)
            {
                if (!choices.ContainsKey(choice))
                {
                    return;
                }

                _ = choices.Remove(choice);
            }
            else
            {
                TotalWeight += num2;
                choices[choice] = num2;
            }
        }

        public void SetMany(float totalChance, params T[] choices)
        {
            if (choices.Length == 0)
            {
                return;
            }

            _ = (double)totalChance / choices.Length;
            int num2 = 0;
            foreach (T choice in choices)
            {
                _ = this.choices.TryGetValue(choice, out int num3);
                num2 += num3;
            }
            TotalWeight -= num2;
            int num4 = (int)Math.Round(TotalWeight / (1.0 - (double)totalChance) / choices.Length);
            if (num4 <= 0 && (double)totalChance > 0.0)
            {
                num4 = 1;
            }

            if (num4 <= 0)
            {
                foreach (T choice in choices)
                {
                    if (this.choices.ContainsKey(choice))
                    {
                        _ = this.choices.Remove(choice);
                    }
                }
            }
            else
            {
                TotalWeight += num4 * choices.Length;
                foreach (T choice in choices)
                {
                    this.choices[choice] = num4;
                }
            }
        }

        public T Get(Random random)
        {
            int num = random.Next(TotalWeight);
            foreach (KeyValuePair<T, int> choice in choices)
            {
                if (num < choice.Value)
                {
                    return choice.Key;
                }

                num -= choice.Value;
            }
            throw new Exception("Random choice error!");
        }

        public T Get()
        {
            return Get(Calc.Random);
        }

        private struct Choice
        {
            public T Data;
            public int Weight;

            public Choice(T data, int weight)
            {
                Data = data;
                Weight = weight;
            }
        }
    }
}
