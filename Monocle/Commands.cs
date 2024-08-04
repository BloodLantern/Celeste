using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Monocle
{
    public class Commands
    {
        private const float UNDERSCORE_TIME = 0.5f;
        private const float REPEAT_DELAY = 0.5f;
        private const float REPEAT_EVERY = 1f / 30f;
        private const float OPACITY = 0.8f;
        public bool Enabled = true;
        public bool Open;
        private readonly Dictionary<string, CommandInfo> commands;
        private readonly List<string> sorted;
        private KeyboardState oldState;
        private KeyboardState currentState;
        private string currentText = "";
        private readonly List<Line> drawCommands;
        private bool underscore;
        private float underscoreCounter;
        private readonly List<string> commandHistory;
        private int seekIndex = -1;
        private int tabIndex = -1;
        private string tabSearch;
        private float repeatCounter;
        private Keys? repeatKey;
        private bool canOpen;

        public Action[] FunctionKeyActions { get; private set; }

        public Commands()
        {
            commandHistory = new List<string>();
            drawCommands = new List<Line>();
            commands = new Dictionary<string, CommandInfo>();
            sorted = new List<string>();
            FunctionKeyActions = new Action[12];
            BuildCommandsList();
        }

        public void Log(object obj, Color color)
        {
            string text = obj.ToString();
            if (text.Contains("\n"))
            {
                string str = text;
                char[] chArray = new char[1]{ '\n' };
                foreach (object obj1 in str.Split(chArray))
                    Log(obj1, color);
            }
            else
            {
                int length;
                for (int index1 = Engine.Instance.Window.ClientBounds.Width - 40; Draw.DefaultFont.MeasureString(text).X > (double) index1; text = text.Substring(length + 1))
                {
                    length = -1;
                    for (int index2 = 0; index2 < text.Length; ++index2)
                    {
                        if (text[index2] == ' ')
                        {
                            if (Draw.DefaultFont.MeasureString(text.Substring(0, index2)).X <= (double) index1)
                                length = index2;
                            else
                                break;
                        }
                    }
                    if (length != -1)
                        drawCommands.Insert(0, new Line(text.Substring(0, length), color));
                    else
                        break;
                }
                drawCommands.Insert(0, new Line(text, color));
                int num = (Engine.Instance.Window.ClientBounds.Height - 100) / 30;
                while (drawCommands.Count > num)
                    drawCommands.RemoveAt(drawCommands.Count - 1);
            }
        }

        public void Log(object obj) => Log(obj, Color.White);

        internal void UpdateClosed()
        {
            if (!canOpen)
                canOpen = true;
            else if (MInput.Keyboard.Pressed(Keys.OemTilde, Keys.Oem8))
            {
                Open = true;
                currentState = Keyboard.GetState();
            }
            for (int num = 0; num < FunctionKeyActions.Length; ++num)
            {
                if (MInput.Keyboard.Pressed((Keys) ('p' + num)))
                    ExecuteFunctionKeyAction(num);
            }
        }

        internal void UpdateOpen()
        {
            oldState = currentState;
            currentState = Keyboard.GetState();
            underscoreCounter += Engine.DeltaTime;
            while (underscoreCounter >= UNDERSCORE_TIME)
            {
                underscoreCounter -= UNDERSCORE_TIME;
                underscore = !underscore;
            }
            if (repeatKey.HasValue)
            {
                if (currentState[repeatKey.Value] == KeyState.Down)
                {
                    for (repeatCounter += Engine.DeltaTime; repeatCounter >= REPEAT_DELAY; repeatCounter -= REPEAT_EVERY)
                        HandleKey(repeatKey.Value);
                }
                else
                    repeatKey = new Keys?();
            }
            foreach (Keys pressedKey in currentState.GetPressedKeys())
            {
                if (oldState[pressedKey] == KeyState.Up)
                {
                    HandleKey(pressedKey);
                    break;
                }
            }
        }

        private void HandleKey(Keys key)
        {
            if (key is not Keys.Tab and not Keys.LeftShift and not Keys.RightShift and not Keys.RightAlt and not Keys.LeftAlt and not Keys.RightControl and not Keys.LeftControl)
                tabIndex = -1;
            if (key is not Keys.OemTilde and not Keys.Oem8 and not Keys.Enter)
            {
                Keys? repeatKey = this.repeatKey;
                Keys keys = key;
                if (!(repeatKey.GetValueOrDefault() == keys & repeatKey.HasValue))
                {
                    this.repeatKey = key;
                    repeatCounter = 0.0f;
                }
            }
            if (key <= Keys.Enter)
            {
                if (key != Keys.Back)
                {
                    if (key != Keys.Tab)
                    {
                        if (key == Keys.Enter)
                        {
                            if (currentText.Length <= 0)
                                return;
                            EnterCommand();
                            return;
                        }
                    }
                    else
                    {
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            if (tabIndex == -1)
                            {
                                tabSearch = currentText;
                                FindLastTab();
                            }
                            else
                            {
                                --tabIndex;
                                if (tabIndex < 0 || tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0)
                                    FindLastTab();
                            }
                        }
                        else if (tabIndex == -1)
                        {
                            tabSearch = currentText;
                            FindFirstTab();
                        }
                        else
                        {
                            ++tabIndex;
                            if (tabIndex >= sorted.Count || tabSearch != "" && sorted[tabIndex].IndexOf(tabSearch) != 0)
                                FindFirstTab();
                        }
                        if (tabIndex == -1)
                            return;
                        currentText = sorted[tabIndex];
                        return;
                    }
                }
                else
                {
                    if (currentText.Length <= 0)
                        return;
                    currentText = currentText.Substring(0, currentText.Length - 1);
                    return;
                }
            }
            else if (key <= Keys.F12)
            {
                switch (key - 32)
                {
                    case Keys.None:
                        currentText += " ";
                        return;
                    case (Keys) 1:
                    case (Keys) 2:
                    case (Keys) 3:
                    case (Keys) 4:
                    case (Keys) 5:
                    case (Keys) 7:
                    case Keys.Tab:
                    case (Keys) 10:
                    case (Keys) 11:
                    case (Keys) 12:
                    case Keys.Enter:
                    case (Keys) 15:
                        break;
                    case (Keys) 6:
                        if (seekIndex >= commandHistory.Count - 1)
                            return;
                        ++seekIndex;
                        currentText = string.Join(" ", commandHistory[seekIndex]);
                        return;
                    case Keys.Back:
                        if (seekIndex <= -1)
                            return;
                        --seekIndex;
                        if (seekIndex == -1)
                        {
                            currentText = "";
                            return;
                        }
                        currentText = string.Join(" ", commandHistory[seekIndex]);
                        return;
                    case (Keys) 14:
                        currentText = "";
                        return;
                    case (Keys) 16:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += ")";
                            return;
                        }
                        currentText += "0";
                        return;
                    case (Keys) 17:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "!";
                            return;
                        }
                        currentText += "1";
                        return;
                    case (Keys) 18:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "@";
                            return;
                        }
                        currentText += "2";
                        return;
                    case Keys.Pause:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "#";
                            return;
                        }
                        currentText += "3";
                        return;
                    case Keys.CapsLock:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "$";
                            return;
                        }
                        currentText += "4";
                        return;
                    case Keys.Kana:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "%";
                            return;
                        }
                        currentText += "5";
                        return;
                    case (Keys) 22:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "^";
                            return;
                        }
                        currentText += "6";
                        return;
                    case (Keys) 23:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "&";
                            return;
                        }
                        currentText += "7";
                        return;
                    case (Keys) 24:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "*";
                            return;
                        }
                        currentText += "8";
                        return;
                    case Keys.Kanji:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "(";
                            return;
                        }
                        currentText += "9";
                        return;
                    default:
                        if ((uint) (key - 112) <= 11U)
                        {
                            ExecuteFunctionKeyAction((int) (key - 'p'));
                            return;
                        }
                        break;
                }
            }
            else
            {
                switch (key - 186)
                {
                    case Keys.None:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += ":";
                            return;
                        }
                        currentText += ";";
                        return;
                    case (Keys) 1:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "+";
                            return;
                        }
                        currentText += "=";
                        return;
                    case (Keys) 2:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "<";
                            return;
                        }
                        currentText += ",";
                        return;
                    case (Keys) 3:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "_";
                            return;
                        }
                        currentText += "-";
                        return;
                    case (Keys) 4:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += ">";
                            return;
                        }
                        currentText += ".";
                        return;
                    case (Keys) 5:
                        if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                        {
                            currentText += "?";
                            return;
                        }
                        currentText += "/";
                        return;
                    case (Keys) 6:
label_104:
                        Open = canOpen = false;
                        return;
                    default:
                        switch (key - 219)
                        {
                            case Keys.None:
                                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                                {
                                    currentText += "{";
                                    return;
                                }
                                currentText += "[";
                                return;
                            case (Keys) 2:
                                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                                {
                                    currentText += "}";
                                    return;
                                }
                                currentText += "]";
                                return;
                            case (Keys) 3:
                                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                                {
                                    currentText += "\"";
                                    return;
                                }
                                currentText += "'";
                                return;
                            case (Keys) 4:
                                goto label_104;
                            case (Keys) 7:
                                if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                                {
                                    currentText += "|";
                                    return;
                                }
                                currentText += "\\";
                                return;
                        }
                        break;
                }
            }
            if (key.ToString().Length != 1)
                return;
            if (currentState[Keys.LeftShift] == KeyState.Down || currentState[Keys.RightShift] == KeyState.Down)
                currentText += key.ToString();
            else
                currentText += key.ToString().ToLower();
        }

        private void EnterCommand()
        {
            string[] strArray = currentText.Split(new char[2]
            {
                ' ',
                ','
            }, StringSplitOptions.RemoveEmptyEntries);
            if (commandHistory.Count == 0 || commandHistory[0] != currentText)
                commandHistory.Insert(0, currentText);
            drawCommands.Insert(0, new Line(currentText, Color.Aqua));
            currentText = "";
            seekIndex = -1;
            string[] args = new string[strArray.Length - 1];
            for (int i = 1; i < strArray.Length; ++i)
                args[i - 1] = strArray[i];
            ExecuteCommand(strArray[0].ToLower(), args);
        }

        private void FindFirstTab()
        {
            for (int i = 0; i < sorted.Count; ++i)
                if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
                {
                    tabIndex = i;
                    break;
                }
        }

        private void FindLastTab()
        {
            for (int i = 0; i < sorted.Count; ++i)
                if (tabSearch == "" || sorted[i].IndexOf(tabSearch) == 0)
                    tabIndex = i;
        }

        internal void Render()
        {
            int viewWidth = Engine.ViewWidth;
            int viewHeight = Engine.ViewHeight;

            Draw.SpriteBatch.Begin();
            Draw.Rect(10f, viewHeight - 50, viewWidth - 20, 40f, Color.Black * OPACITY);

            string text = '>' + currentText;
            if (underscore)
                text += '_';
            Draw.SpriteBatch.DrawString(Draw.DefaultFont, text, new Vector2(20f, viewHeight - 42), Color.White);

            if (drawCommands.Count > 0)
            {
                int height = 10 + 30 * drawCommands.Count;
                Draw.Rect(10f, viewHeight - height - 60, viewWidth - 20, height, Color.Black * OPACITY);
                for (int i = 0; i < drawCommands.Count; ++i)
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, drawCommands[i].Text, new Vector2(20f, viewHeight - 92 - 30 * i), drawCommands[i].Color);
            }

#if ENABLE_DEBUG
            if (Engine.Scene is Level)
            {
                Player player = Engine.Scene.Tracker.GetEntity<Player>();
                if (player != null)
                {
                    Draw.Rect(10, 40, 200, 200, Color.Black * OPACITY);
                    uint i = 0;
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, "Player:", new Vector2(20, 50), Color.White);
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, "| Position: " + player.Position, new Vector2(20, 50 + 15 * ++i), Color.White);
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, "| Dashes: " + player.Dashes, new Vector2(20, 50 + 15 * ++i), Color.White);
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, "| Max dashes: " + player.MaxDashes, new Vector2(20, 50 + 15 * ++i), Color.White);
                    Draw.SpriteBatch.DrawString(Draw.DefaultFont, "| Can dash: " + player.CanDash, new Vector2(20, 50 + 15 * ++i), Color.White);
                }
            }
#endif
            Draw.SpriteBatch.End();
        }

        public void ExecuteCommand(string command, string[] args)
        {
            if (commands.ContainsKey(command))
                commands[command].Action(args);
            else
                Log("Command '" + command + "' not found! Type 'help' for list of commands", Color.Yellow);
        }

        public void ExecuteFunctionKeyAction(int num)
        {
            if (FunctionKeyActions[num] == null)
                return;
            FunctionKeyActions[num]();
        }

        private void BuildCommandsList()
        {
            foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    ProcessMethod(method);

            foreach (Type type in Assembly.GetEntryAssembly().GetTypes())
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    ProcessMethod(method);

            foreach (KeyValuePair<string, CommandInfo> command in commands)
                sorted.Add(command.Key);
            sorted.Sort();
        }

        private void ProcessMethod(MethodInfo method)
        {
            Command command = null;

            object[] customAttributes = method.GetCustomAttributes(typeof(Command), false);
            if (customAttributes.Length != 0)
                command = customAttributes[0] as Command;

            if (command == null)
                return;

            if (!method.IsStatic)
                throw new Exception(method.DeclaringType.Name + "." + method.Name + " is marked as a command, but is not static");

            CommandInfo commandInfo = new()
            {
                Help = command.Help
            };
            ParameterInfo[] parameters = method.GetParameters();

            object[] defaultParameterValues = new object[parameters.Length];
            string[] parameterInfos = new string[parameters.Length];

            for (int index = 0; index < parameters.Length; ++index)
            {
                ParameterInfo parameter = parameters[index];
                parameterInfos[index] = parameter.Name + ':';
                if (parameter.ParameterType == typeof(string))
                    parameterInfos[index] += typeof(string).Name;
                else if (parameter.ParameterType == typeof(int))
                    parameterInfos[index] += typeof(int).Name;
                else if (parameter.ParameterType == typeof(float))
                    parameterInfos[index] += typeof(float).Name;
                else
                {
                    if (parameter.ParameterType != typeof(bool))
                        throw new Exception(method.DeclaringType.Name + '.' + method.Name + " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool");
                    parameterInfos[index] += typeof(bool).Name;
                }

                if (parameter.DefaultValue == DBNull.Value)
                    defaultParameterValues[index] = null;
                else if (parameter.DefaultValue != null)
                {
                    defaultParameterValues[index] = parameter.DefaultValue;
                    if (parameter.ParameterType == typeof(string))
                        parameterInfos[index] += "=\"" + parameter.DefaultValue + "\"";
                    else
                        parameterInfos[index] += "=" + parameter.DefaultValue;
                }
                else
                    defaultParameterValues[index] = null;
            }

            commandInfo.Usage = parameterInfos.Length != 0 ? "[" + string.Join(" ", parameterInfos) + "]" : "";
            commandInfo.Action = args =>
            {
                if (parameters.Length == 0)
                    InvokeMethod(method);
                else
                {
                    object[] objArray = (object[])defaultParameterValues.Clone();
                    for (int index = 0; index < objArray.Length && index < args.Length; ++index)
                    {
                        if (parameters[index].ParameterType == typeof(string))
                            objArray[index] = ArgString(args[index]);
                        else if (parameters[index].ParameterType == typeof(int))
                            objArray[index] = ArgInt(args[index]);
                        else if (parameters[index].ParameterType == typeof(float))
                            objArray[index] = ArgFloat(args[index]);
                        else if (parameters[index].ParameterType == typeof(bool))
                            objArray[index] = ArgBool(args[index]);
                    }
                    InvokeMethod(method, objArray);
                }
            };
            commands[command.Name] = commandInfo;
        }

        private void InvokeMethod(MethodInfo method, object[] param = null)
        {
            try
            {
                method.Invoke(null, param);
            }
            catch (Exception ex)
            {
                Engine.Commands.Log(ex.InnerException.Message, Color.Yellow);
                LogStackTrace(ex.InnerException.StackTrace);
            }
        }

        private void LogStackTrace(string stackTrace)
        {
            string str1 = stackTrace;
            char[] chArray = new char[1]{ '\n' };
            foreach (string str2 in str1.Split(chArray))
            {
                string strOut = str2;
                int length1 = strOut.LastIndexOf(" in ") + 4;
                int startIndex1 = strOut.LastIndexOf('\\') + 1;
                if (length1 != -1 && startIndex1 != -1)
                    strOut = strOut.Substring(0, length1) + strOut.Substring(startIndex1);
                int length2 = strOut.IndexOf('(') + 1;
                int startIndex2 = strOut.IndexOf(')');
                if (length2 != -1 && startIndex2 != -1)
                    strOut = strOut.Substring(0, length2) + strOut.Substring(startIndex2);
                int startIndex3 = strOut.LastIndexOf(':');
                if (startIndex3 != -1)
                    strOut = strOut.Insert(startIndex3 + 1, " ").Insert(startIndex3, " ");
                Engine.Commands.Log("-> " + strOut.TrimStart(), Color.White);
            }
        }

        private static string ArgString(string arg) => arg == null ? "" : arg;

        private static bool ArgBool(string arg)
        {
            switch (arg)
            {
                case "0":
                    return false;
                case null:
                    return false;
                default:
                    if (!(arg.ToLower() == "false"))
                        return !(arg.ToLower() == "f");
                    goto case "0";
            }
        }

        private static int ArgInt(string arg)
        {
            try
            {
                return Convert.ToInt32(arg);
            }
            catch
            {
                return 0;
            }
        }

        private static float ArgFloat(string arg)
        {
            try
            {
                return Convert.ToSingle(arg, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0.0f;
            }
        }

        [Command("clear", "Clears the terminal")]
        public static void Clear() => Engine.Commands.drawCommands.Clear();

        [Command("exit", "Exits the game")]
        private static void Exit() => Engine.Instance.Exit();

        [Command("vsync", "Enables or disables vertical sync")]
        private static void Vsync(bool enabled = true)
        {
            Engine.Graphics.SynchronizeWithVerticalRetrace = enabled;
            Engine.Graphics.ApplyChanges();
            Engine.Commands.Log("Vertical Sync " + (enabled ? "Enabled" : "Disabled"));
        }

        [Command("count", "Logs amount of Entities in the Scene. Pass a tagIndex to count only Entities with that tag")]
        private static void Count(int tagIndex = -1)
        {
            if (Engine.Scene == null)
                Engine.Commands.Log("Current Scene is null!");
            else if (tagIndex < 0)
                Engine.Commands.Log(Engine.Scene.Entities.Count.ToString());
            else
                Engine.Commands.Log(Engine.Scene.TagLists[tagIndex].Count.ToString());
        }

        [Command("tracker", "Logs all tracked objects in the scene. Set mode to 'e' for just entities, or 'c' for just components")]
        private static void Tracker(string mode)
        {
            if (Engine.Scene == null)
                Engine.Commands.Log("Current Scene is null!");
            else if (!(mode == "e"))
            {
                if (!(mode == "c"))
                {
                    Engine.Commands.Log("-- Entities --");
                    Engine.Scene.Tracker.LogEntities();
                    Engine.Commands.Log("-- Components --");
                    Engine.Scene.Tracker.LogComponents();
                }
                else
                    Engine.Scene.Tracker.LogComponents();
            }
            else
                Engine.Scene.Tracker.LogEntities();
        }

        [Command("pooler", "Logs the pooled Entity counts")]
        private static void Pooler() => Engine.Pooler.Log();

        [Command("fullscreen", "Switches to fullscreen mode")]
        private static void Fullscreen() => Engine.SetFullscreen();

        [Command("window", "Switches to window mode")]
        private static void Window(int scale = 1) => Engine.SetWindowed(Engine.Width * scale, Engine.Height * scale);

        [Command("help", "Shows usage help for a given command")]
        private static void Help(string command)
        {
            if (Engine.Commands.sorted.Contains(command))
            {
                CommandInfo command1 = Engine.Commands.commands[command];
                StringBuilder stringBuilder = new();
                stringBuilder.Append(":: ");
                stringBuilder.Append(command);
                if (!string.IsNullOrEmpty(command1.Usage))
                {
                    stringBuilder.Append(" ");
                    stringBuilder.Append(command1.Usage);
                }
                Engine.Commands.Log(stringBuilder.ToString());
                if (string.IsNullOrEmpty(command1.Help))
                    Engine.Commands.Log("No help info set");
                else
                    Engine.Commands.Log(command1.Help);
            }
            else
            {
                StringBuilder stringBuilder = new();
                stringBuilder.Append("Commands list: ");
                stringBuilder.Append(string.Join(", ", Engine.Commands.sorted));
                Engine.Commands.Log(stringBuilder.ToString());
                Engine.Commands.Log("Type 'help command' for more info on that command!");
            }
        }

        private struct CommandInfo
        {
            public Action<string[]> Action;
            public string Help;
            public string Usage;
        }

        private struct Line
        {
            public string Text;
            public Color Color;

            public Line(string text)
            {
                Text = text;
                Color = Color.White;
            }

            public Line(string text, Color color)
            {
                Text = text;
                Color = color;
            }
        }
    }
}
