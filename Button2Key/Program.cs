using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;

namespace Button2Key
{

    //add type = keyboardinput name=assettocorsa_resetvrcam description = "Reset VR Camera" command="keydown:control;keydown:space;sleep:100;keyup:space;keyup:control"
    //add type = mapping button=Buttons7 value="equals:128" input=assettocorsa_resetvrcam
    //listen productguid=c24f046d-0000-0000-0000-504944564944

    class Program
    {

        #region public enums

        public enum InputCommandOperation
        {
            none = 0,
            keypress = 1,
            sleep = 2
        }

        public enum KeyboardKeyPressDirection
        {
            none = 0,
            down = 1,
            up = 2
        }

        public enum ValueComparisonOperator
        {
            none = 0,
            equals = 1,
            greaterthan = 2,
            lessthan = 3
        }

        #endregion

        #region private structs

        private struct Input
        {
            public string Name;
            public string Description;
            public List<InputCommand> Commands;

            public override string ToString()
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine(String.Format("Name : {0}", Name));
                output.AppendLine(String.Format("Description : {0}", Description));
                output.AppendLine("Commands -");
                for(int iCommand = 0; iCommand < Commands.Count; iCommand++)
                {
                    InputCommand curCommand = Commands[iCommand];
                    bool isLast = iCommand == (Commands.Count - 1);
                    output.AppendLine(curCommand.ToString());
                    if (!isLast) output.AppendLine("---");
                }

                return (output.ToString());
            }
        }

        private struct InputCommand
        {
            public InputCommandOperation Operation;
            public KeyboardKeyPressDirection KeyPressDirection;
            public WindowsInput.Native.VirtualKeyCode VirtualKeyCode;
            public int SleepTime;

            public override string ToString()
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine(String.Format("Operation : {0}", Operation));
                if (Operation == InputCommandOperation.keypress)
                {
                    output.AppendLine(String.Format("KeyPressDirection : {0}", KeyPressDirection));
                    output.AppendLine(String.Format("KeyPressDirection : {0}", VirtualKeyCode));
                }
                if (Operation == InputCommandOperation.sleep)
                {
                    output.AppendLine(String.Format("KeyPressDirection : {0}", SleepTime));
                }
                return (output.ToString());
            }
        }

        private struct ValueCompare
        {
            public ValueComparisonOperator Operator;
            public int Value;

            public bool Match(int value)
            {
                switch(Operator)
                {
                    case ValueComparisonOperator.equals:
                        {
                            return (value == Value);
                        }
                    case ValueComparisonOperator.greaterthan:
                        {
                            return (Value > value);
                        }
                    case ValueComparisonOperator.lessthan:
                        {
                            return (Value < value);
                        }
                    default:
                        {
                            return (false);
                        }
                }
            }

            public override string ToString()
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine(String.Format("Operator : {0}", Operator));
                output.AppendLine(String.Format("Value : {0}", Value));
                return (output.ToString());
            }
        }

        private struct Mapping
        {
            public string Button;
            public ValueCompare Value;
            public Input Input;

            public override string ToString()
            {
                StringBuilder output = new StringBuilder();
                output.AppendLine(String.Format("Button : {0}", Button));
                string value = Value.ToString();
                output.AppendLine(String.Format("Value : {0}", value.Trim()));
                output.AppendLine(String.Format("Input : {0}", Input.Name));
                return (output.ToString());
            }
        }

        #endregion

        #region private objects

        private static DirectInput _directInput;
        private static IList<DeviceInstance> _allDevices;
        private static bool _listen;
        private static bool _debugInput;
        private static InputSimulator _inputSimulator;

        private static Dictionary<string, Input> _inputs = new Dictionary<string, Input>();
        private static Dictionary<string, Mapping> _mappings = new Dictionary<string, Mapping>();

        #endregion

        public static async Task<int> Main(string[] args)
        {
            await Task.Yield();

            Console.WriteLine("Button2Key Mapping Utility v{0}", typeof(Program).Assembly.GetName().Version);

            Console.WriteLine("Initialising DirectInput.");
            _directInput = new DirectInput();
            _allDevices = _directInput.GetDevices();
            Console.WriteLine("Found '{0}' input devices.", _allDevices.Count);

            Console.WriteLine("Initialising input simulator.");
            _inputSimulator = new InputSimulator();

            bool exit = false;

            while (!exit)
            {
                string inputLine = RequestInput();
                ProcessLine(inputLine, out exit);
            }

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();

            return (0);
        }

        private static void ProcessLine(
            string line,
            out bool exit)
        {
            exit = false;
            SimpleCommandLineParser parser = SimpleCommandLineParser.Parse(line);
            switch (parser.Command.ToLower())
            {
                case "run":
                    {
                        string input = parser.GetParameterValueOrDefault("input");

                        if(File.Exists(input))
                        {
                            string[] inputLines = File.ReadAllLines(input);
                            foreach (string curLine in inputLines)
                            {
                                string trimmedLine = curLine.Trim();
                                if (!String.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("//"))
                                {
                                    ProcessLine(curLine, out exit);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Input file does not exist.");
                        }

                        break;
                    }
                case "list":
                    {
                        string type = parser.GetParameterValueOrDefault("type");

                        switch (type.ToLower())
                        {
                            case "devices":
                                {
                                    _allDevices = _directInput.GetDevices();
                                    for (int iInstance = 0; iInstance < _allDevices.Count; iInstance++)
                                    {
                                        DeviceInstance curInstance = _allDevices[iInstance];
                                        bool isLast = iInstance == (_allDevices.Count - 1);
                                        Console.WriteLine(FormatDeviceInstanceString(curInstance));
                                        if (!isLast) Console.WriteLine("---");
                                    }

                                    break;
                                }
                            case "inputs":
                                {
                                    string[] keys = _inputs.Keys.ToArray();
                                    for (int iInput = 0; iInput < keys.Length; iInput++)
                                    {
                                        Input curInput = _inputs[keys[iInput]];
                                        bool isLast = iInput == (keys.Length - 1);
                                        Console.WriteLine(curInput);
                                        if (!isLast) Console.WriteLine("---");
                                    }
                                    break;
                                }
                            case "mappings":
                                {
                                    string[] keys = _mappings.Keys.ToArray();
                                    for (int iMapping = 0; iMapping < keys.Length; iMapping++)
                                    {
                                        Mapping curMapping = _mappings[keys[iMapping]];
                                        bool isLast = iMapping == (keys.Length - 1);
                                        Console.WriteLine(curMapping);
                                        if (!isLast) Console.WriteLine("---");
                                    }
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("Unknown type.");
                                    break;
                                }
                        }

                        break;
                    }
                case "listen":
                    {
                        DeviceInstance instance = null;

                        _debugInput = parser.GetParameterValueOrDefault("debug", "on") == "on";
                        string instanceguid = parser.GetParameterValueOrDefault("instanceguid", String.Empty);
                        string productguid = parser.GetParameterValueOrDefault("productguid", String.Empty);
                        if (!String.IsNullOrEmpty(instanceguid))
                        {
                            instance = _allDevices.FirstOrDefault(di => di.InstanceGuid.ToString().ToLower() == instanceguid.ToLower());
                        }
                        else if (!String.IsNullOrEmpty(productguid))
                        {
                            instance = _allDevices.FirstOrDefault(di => di.ProductGuid.ToString().ToLower() == productguid.ToLower());
                        }

                        if (instance != null)
                        {
                            Console.WriteLine("Start listening to input from '{0}'.", instance.ProductName);
                            _listen = true;
                            Console.WriteLine("Press Escape key to stop listening...");
                            Task listening = ListenDevice(instance);
                            ConsoleKeyInfo key = Console.ReadKey();
                            while (key.Key != ConsoleKey.Escape)
                            {
                                key = Console.ReadKey();
                            }
                            Console.WriteLine("Stopping...");
                            _listen = false;
                            listening.Wait();
                            Console.WriteLine("Stopped.");
                        }
                        else
                        {
                            Console.WriteLine("Device not found.", instanceguid);
                        }

                        break;
                    }
                case "add":
                    {
                        string type = parser.GetParameterValueOrDefault("type");

                        switch (type.ToLower())
                        {
                            case "keyboardinput":
                                {
                                    string name = parser.GetParameterValueOrDefault("name", String.Empty);
                                    string desription = parser.GetParameterValueOrDefault("description", String.Empty);

                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        string command = parser.GetParameterValueOrDefault("command", String.Empty);

                                        if (!String.IsNullOrEmpty(command))
                                        {
                                            List<InputCommand> commands = new List<InputCommand>();
                                            string[] commandParts = command.Split(';');
                                            foreach (string curPart in commandParts)
                                            {
                                                string[] partParts = curPart.Split(':');
                                                switch (partParts[0])
                                                {
                                                    case "keydown":
                                                        {
                                                            WindowsInput.Native.VirtualKeyCode key = (WindowsInput.Native.VirtualKeyCode)Enum.Parse(typeof(WindowsInput.Native.VirtualKeyCode), partParts[1], true);
                                                            commands.Add(new InputCommand()
                                                            {
                                                                Operation = InputCommandOperation.keypress,
                                                                KeyPressDirection = KeyboardKeyPressDirection.down,
                                                                VirtualKeyCode = key
                                                            });
                                                            break;
                                                        }
                                                    case "keyup":
                                                        {
                                                            WindowsInput.Native.VirtualKeyCode key = (WindowsInput.Native.VirtualKeyCode)Enum.Parse(typeof(WindowsInput.Native.VirtualKeyCode), partParts[1], true);
                                                            commands.Add(new InputCommand()
                                                            {
                                                                Operation = InputCommandOperation.keypress,
                                                                KeyPressDirection = KeyboardKeyPressDirection.up,
                                                                VirtualKeyCode = key
                                                            });
                                                            break;
                                                        }
                                                    case "sleep":
                                                        {
                                                            int time = int.Parse(partParts[1]);
                                                            commands.Add(new InputCommand()
                                                            {
                                                                Operation = InputCommandOperation.sleep,
                                                                SleepTime = time
                                                            });
                                                            break;
                                                        }
                                                }
                                            }

                                            Input input = new Input()
                                            {
                                                Name = name,
                                                Description = desription,
                                                Commands = commands
                                            };

                                            Console.WriteLine("Adding command '{0}'.", name);
                                            _inputs.Add(name, input);
                                        }
                                        else
                                        {
                                            Console.WriteLine("Missing required parameter 'command'.");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Missing required parameter 'name'.");
                                    }

                                    break;
                                }
                            case "mapping":
                                {
                                    string button = parser.GetParameterValueOrDefault("button", String.Empty);

                                    if (!String.IsNullOrEmpty(button))
                                    {
                                        string value = parser.GetParameterValueOrDefault("value", String.Empty);
                                        string input = parser.GetParameterValueOrDefault("input", String.Empty);

                                        if (!String.IsNullOrEmpty(input))
                                        {
                                            if (_inputs.ContainsKey(input))
                                            {
                                                string[] valueParts = value.Split(':');
                                                ValueCompare compare = new ValueCompare()
                                                {
                                                    Operator = (ValueComparisonOperator)Enum.Parse(typeof(ValueComparisonOperator), valueParts[0], true),
                                                    Value = int.Parse(valueParts[1])
                                                };

                                                Mapping mapping = new Mapping()
                                                {
                                                    Input = _inputs[input],
                                                    Button = button.ToLower(),
                                                    Value = compare
                                                };

                                                Console.WriteLine("Adding command '{0}' -> '{1}'.", button.ToLower(), mapping.Input.Name);
                                                _mappings.Add(button.ToLower(), mapping);
                                            }
                                            else
                                            {
                                                Console.WriteLine("No input exists with the name '{0}', you must add one before you can add a mapping for it.", input);
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Missing required parameter 'input'.");
                                        }

                                    }
                                    else
                                    {
                                        Console.WriteLine("Missing required parameter 'button'.");
                                    }

                                    break;
                                }
                        }

                        break;
                    }
                case "exit":
                    {
                        Console.WriteLine("Exiting...");
                        exit = true;
                        break;
                    }
                default:
                    {
                        Console.WriteLine("Unknown command.");
                        break;
                    }
            }
        }

        private static string RequestInput()
        {
            Console.Write("> ");
            return(Console.ReadLine());
        }

        private static string FormatDeviceInstanceString(DeviceInstance instance)
        {
            StringBuilder output = new StringBuilder();
            output.AppendLine(String.Format("{0} : {1}", "Product Guid", instance.ProductGuid));
            output.AppendLine(String.Format("{0} : {1}", "Product Name", instance.ProductName));
            output.AppendLine(String.Format("{0} : {1}", "Instance Guid", instance.InstanceGuid));
            output.AppendLine(String.Format("{0} : {1}/{2}", "Type", instance.Type, instance.Subtype));
            return (output.ToString());
        }

        private static Task ListenDevice(DeviceInstance deviceInstance)
        {
            return Task.Run(() =>
            {
                var joystick = new Joystick(_directInput, deviceInstance.InstanceGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
                while (_listen)
                {
                    joystick.Poll();
                    JoystickUpdate[] datas = joystick.GetBufferedData();
                    if (datas.Length > 0)
                    {
                        foreach(JoystickUpdate curUpdate in datas)
                        {
                            if(_mappings.ContainsKey(curUpdate.Offset.ToString().ToLower()))
                            {
                                Mapping mapping = _mappings[curUpdate.Offset.ToString().ToLower()];
                                if(mapping.Value.Match(curUpdate.Value))
                                {
                                    Console.WriteLine("Activating mapping '{0}' / '{1}'.", mapping.Input.Name, mapping.Input.Description);
                                    ProcessInputCommands(mapping.Input.Commands);
                                }
                            }

                            if(_debugInput) Console.WriteLine(curUpdate);
                        }
                    }
                }
            });
        }

        private static void ProcessInputCommands(List<InputCommand> commands)
        {
            foreach(InputCommand curCommand in commands)
            {
                switch(curCommand.Operation)
                {
                    case InputCommandOperation.keypress:
                        {
                            switch(curCommand.KeyPressDirection)
                            {
                                case KeyboardKeyPressDirection.down:
                                    {
                                        Console.WriteLine("Simulating key down '{0}'.", curCommand.VirtualKeyCode);
                                        _inputSimulator.Keyboard.KeyDown(curCommand.VirtualKeyCode);
                                        break;
                                    }
                                case KeyboardKeyPressDirection.up:
                                    {
                                        Console.WriteLine("Simulating key up '{0}'.", curCommand.VirtualKeyCode);
                                        _inputSimulator.Keyboard.KeyUp(curCommand.VirtualKeyCode);
                                        break;
                                    }
                            }
                            break;
                        }
                    case InputCommandOperation.sleep:
                        {
                            Console.WriteLine("Sleeping for {0}ms.", curCommand.SleepTime);
                            Thread.Sleep(curCommand.SleepTime);
                            break;
                        }
                }
            }
        }

    }
}
