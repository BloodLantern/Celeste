using System;
using System.Collections.Generic;

namespace Monocle
{
    public class ChoiceSet<T>
    {
        private Dictionary<T, int> choices;

        public int TotalWeight { get; private set; }

        public ChoiceSet()
        {
            choices = new Dictionary<T, int>();
            TotalWeight = 0;
        }

        public void Set(T choice, int weight)
        {
            int num = 0;
            choices.TryGetValue(choice, out num);
            TotalWeight -= num;
            if (weight <= 0)
            {
                if (!choices.ContainsKey(choice))
                    return;
                choices.Remove(choice);
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
                int num = 0;
                choices.TryGetValue(choice, out num);
                return num;
            }
            set => Set(choice, value);
        }

        public void Set(T choice, float chance)
        {
            int num1 = 0;
            choices.TryGetValue(choice, out num1);
            TotalWeight -= num1;
            int num2 = (int) Math.Round(TotalWeight / (1.0 - chance));
            if (num2 <= 0 && chance > 0.0)
                num2 = 1;
            if (num2 <= 0)
            {
                if (!choices.ContainsKey(choice))
                    return;
                choices.Remove(choice);
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
                return;
            double num1 = totalChance / (double) choices.Length;
            int num2 = 0;
            foreach (T choice in choices)
            {
                int num3 = 0;
                this.choices.TryGetValue(choice, out num3);
                num2 += num3;
            }
            TotalWeight -= num2;
            int num4 = (int) Math.Round(TotalWeight / (1.0 - totalChance) / choices.Length);
            if (num4 <= 0 && totalChance > 0.0)
                num4 = 1;
            if (num4 <= 0)
            {
                foreach (T choice in choices)
                {
                    if (this.choices.ContainsKey(choice))
                        this.choices.Remove(choice);
                }
            }
            else
            {
                TotalWeight += num4 * choices.Length;
                foreach (T choice in choices)
                    this.choices[choice] = num4;
            }
        }

        public T Get(Random random)
        {
            int num = random.Next(TotalWeight);
            foreach (KeyValuePair<T, int> choice in choices)
            {
                if (num < choice.Value)
                    return choice.Key;
                num -= choice.Value;
            }
            throw new Exception("Random choice error!");
        }

        public T Get() => Get(Calc.Random);

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
