// Decompiled with JetBrains decompiler
// Type: Monocle.CheatListener
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using System;
using System.Collections.Generic;

namespace Monocle
{
    public class CheatListener : Entity
    {
        public string CurrentInput;
        public bool Logging;
        private readonly List<Tuple<char, Func<bool>>> inputs;
        private readonly List<Tuple<string, Action>> cheats;
        private int maxInput;

        public CheatListener()
        {
            Visible = false;
            CurrentInput = "";
            inputs = new List<Tuple<char, Func<bool>>>();
            cheats = new List<Tuple<string, Action>>();
        }

        public override void Update()
        {
            bool flag = false;
            foreach (Tuple<char, Func<bool>> input in inputs)
            {
                if (input.Item2())
                {
                    CurrentInput += input.Item1.ToString();
                    flag = true;
                }
            }
            if (!flag)
                return;
            if (CurrentInput.Length > maxInput)
                CurrentInput = CurrentInput.Substring(CurrentInput.Length - maxInput);
            if (Logging)
                Calc.Log(CurrentInput);
            foreach (Tuple<string, Action> cheat in cheats)
            {
                if (CurrentInput.Contains(cheat.Item1))
                {
                    CurrentInput = "";
                    cheat.Item2?.Invoke();
                    cheats.Remove(cheat);
                    if (!Logging)
                        break;
                    Calc.Log(("Cheat Activated: " + cheat.Item1));
                    break;
                }
            }
        }

        public void AddCheat(string code, Action onEntered = null)
        {
            cheats.Add(new Tuple<string, Action>(code, onEntered));
            maxInput = Math.Max(code.Length, maxInput);
        }

        public void AddInput(char id, Func<bool> checker) => inputs.Add(new Tuple<char, Func<bool>>(id, checker));
    }
}
