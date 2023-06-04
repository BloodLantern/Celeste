using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Monocle
{
    [Serializable]
    public class Binding
    {
        public List<Keys> Keyboard = new();
        public List<Buttons> Controller = new();
        [XmlIgnore]
        public List<Binding> ExclusiveFrom = new();

        public bool HasInput => Keyboard.Count > 0 || Controller.Count > 0;

        public bool Add(params Keys[] keys)
        {
            bool flag = false;
            Keys[] keysArray = keys;
label_9:
            for (int index = 0; index < keysArray.Length; ++index)
            {
                Keys key = keysArray[index];
                if (!Keyboard.Contains(key))
                {
                    foreach (Binding binding in ExclusiveFrom)
                    {
                        if (binding.Needs(key))
                            goto label_9;
                    }
                    Keyboard.Add(key);
                    flag = true;
                }
            }
            return flag;
        }

        public bool Add(params Buttons[] buttons)
        {
            bool flag = false;
            Buttons[] buttonsArray = buttons;
label_9:
            for (int index = 0; index < buttonsArray.Length; ++index)
            {
                Buttons button = buttonsArray[index];
                if (!Controller.Contains(button))
                {
                    foreach (Binding binding in ExclusiveFrom)
                    {
                        if (binding.Needs(button))
                            goto label_9;
                    }
                    Controller.Add(button);
                    flag = true;
                }
            }
            return flag;
        }

        public bool Needs(Buttons button)
        {
            if (!Controller.Contains(button))
                return false;
            if (Controller.Count <= 1)
                return true;
            if (!IsExclusive(button))
                return false;
            foreach (Buttons button1 in Controller)
            {
                if (button1 != button && IsExclusive(button1))
                    return false;
            }
            return true;
        }

        public bool Needs(Keys key)
        {
            if (!Keyboard.Contains(key))
                return false;
            if (Keyboard.Count <= 1)
                return true;
            if (!IsExclusive(key))
                return false;
            foreach (Keys key1 in Keyboard)
            {
                if (key1 != key && IsExclusive(key1))
                    return false;
            }
            return true;
        }

        public bool IsExclusive(Buttons button)
        {
            foreach (Binding binding in ExclusiveFrom)
            {
                if (binding.Controller.Contains(button))
                    return false;
            }
            return true;
        }

        public bool IsExclusive(Keys key)
        {
            foreach (Binding binding in ExclusiveFrom)
            {
                if (binding.Keyboard.Contains(key))
                    return false;
            }
            return true;
        }

        public bool ClearKeyboard()
        {
            if (ExclusiveFrom.Count > 0)
            {
                if (Keyboard.Count <= 1)
                    return false;
                int index1 = 0;
                for (int index2 = 1; index2 < Keyboard.Count; ++index2)
                {
                    if (IsExclusive(Keyboard[index2]))
                        index1 = index2;
                }
                Keys keys = Keyboard[index1];
                Keyboard.Clear();
                Keyboard.Add(keys);
            }
            else
                Keyboard.Clear();
            return true;
        }

        public bool ClearGamepad()
        {
            if (ExclusiveFrom.Count > 0)
            {
                if (Controller.Count <= 1)
                    return false;
                int index1 = 0;
                for (int index2 = 1; index2 < Controller.Count; ++index2)
                {
                    if (IsExclusive(Controller[index2]))
                        index1 = index2;
                }
                Buttons buttons = Controller[index1];
                Controller.Clear();
                Controller.Add(buttons);
            }
            else
                Controller.Clear();
            return true;
        }

        public float Axis(int gamepadIndex, float threshold)
        {
            foreach (Keys key in Keyboard)
            {
                if (MInput.Keyboard.Check(key))
                    return 1f;
            }
            foreach (Buttons button in Controller)
            {
                float num = MInput.GamePads[gamepadIndex].Axis(button, threshold);
                if ((double) num != 0.0)
                    return num;
            }
            return 0.0f;
        }

        public bool Check(int gamepadIndex, float threshold)
        {
            for (int index = 0; index < Keyboard.Count; ++index)
            {
                if (MInput.Keyboard.Check(Keyboard[index]))
                    return true;
            }
            for (int index = 0; index < Controller.Count; ++index)
            {
                if (MInput.GamePads[gamepadIndex].Check(Controller[index], threshold))
                    return true;
            }
            return false;
        }

        public bool Pressed(int gamepadIndex, float threshold)
        {
            for (int index = 0; index < Keyboard.Count; ++index)
            {
                if (MInput.Keyboard.Pressed(Keyboard[index]))
                    return true;
            }
            for (int index = 0; index < Controller.Count; ++index)
            {
                if (MInput.GamePads[gamepadIndex].Pressed(Controller[index], threshold))
                    return true;
            }
            return false;
        }

        public bool Released(int gamepadIndex, float threshold)
        {
            for (int index = 0; index < Keyboard.Count; ++index)
            {
                if (MInput.Keyboard.Released(Keyboard[index]))
                    return true;
            }
            for (int index = 0; index < Controller.Count; ++index)
            {
                if (MInput.GamePads[gamepadIndex].Released(Controller[index], threshold))
                    return true;
            }
            return false;
        }

        public static void SetExclusive(params Binding[] list)
        {
            foreach (Binding binding in list)
                binding.ExclusiveFrom.Clear();
            foreach (Binding binding1 in list)
            {
                foreach (Binding binding2 in list)
                {
                    if (binding1 != binding2)
                    {
                        binding1.ExclusiveFrom.Add(binding2);
                        binding2.ExclusiveFrom.Add(binding1);
                    }
                }
            }
        }
    }
}
