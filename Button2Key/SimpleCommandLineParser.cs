using System;
using System.Collections.Generic;
using System.Text;

namespace Button2Key
{

    public class SimpleCommandLineParser
    {

        #region private objects

        private string _commandLine = String.Empty;
        private List<string> _commandBlocks;
        private string _command = String.Empty;
        private Dictionary<string, string> _parameters;
        private string _defaultParameter = String.Empty;

        #endregion

        #region public properties

        public string CommandLine
        {
            get
            {
                return (_commandLine);
            }
        }

        public string Command
        {
            get
            {
                return (_command);
            }
        }

        public string DefaultParameter
        {
            get
            {
                return (_defaultParameter);
            }
        }

        public Dictionary<string, string> Parameters
        {
            get
            {
                return (_parameters);
            }
        }

        #endregion

        #region constructor / destructor

        private SimpleCommandLineParser(string commandLine)
        {
            _parameters = new Dictionary<string, string>();
            _commandLine = commandLine;
        }

        #endregion

        #region private methods

        private void ParseCommandLine()
        {
            _commandBlocks = new List<string>();
            Boolean quoted = false;
            StringBuilder curCommandBlock = new StringBuilder(String.Empty);
            foreach (char curChar in CommandLine)
            {
                switch (curChar)
                {
                    case ' ':
                        {
                            if (!quoted & curCommandBlock.Length > 0)
                            {
                                _commandBlocks.Add(curCommandBlock.ToString());
                                curCommandBlock.Clear();
                            }
                            else if(quoted)
                            {
                                curCommandBlock.Append(curChar);
                            }
                            break;
                        }
                    case '\"':
                        {
                            quoted = !quoted;
                            if (!quoted & curCommandBlock.Length > 0)
                            {
                                _commandBlocks.Add(curCommandBlock.ToString());
                                curCommandBlock.Clear();
                            }
                            break;
                        }
                    default:
                        {
                            curCommandBlock.Append(curChar);
                            break;
                        }
                }
            }
            if (curCommandBlock.Length > 0)
            {
                _commandBlocks.Add(curCommandBlock.ToString());
                curCommandBlock.Clear();
            }
            int i = 0;
            foreach (string curBlock in _commandBlocks)
            {
                if (i == 0)
                {
                    _command = curBlock;
                }
                else
                {
                    if (curBlock.Contains("="))
                    {
                        int equalsPos = curBlock.IndexOf("=");
                        String key = curBlock.Substring(0, equalsPos);
                        key = key.TrimStart('-');
                        key = key.TrimStart('/');
                        String value = curBlock.Substring(equalsPos);
                        value = value.TrimStart('=');
                        _parameters.Add(key, value);
                    }
                    else
                    {
                        String key = curBlock.ToString();
                        if (String.IsNullOrEmpty(_defaultParameter) && !(key.StartsWith("-") || key.StartsWith("/")))
                        {
                            _defaultParameter = key;
                        }
                        key = key.TrimStart('-');
                        key = key.TrimStart('/');
                        if (!_parameters.ContainsKey(key))
                        {
                            _parameters.Add(curBlock.ToString(), String.Empty);
                        }
                    }
                }
                i++;
            }
        }

        #endregion

        #region public methods

        public string GetParameterValueOrDefault(string key)
        {
            if (Parameters.ContainsKey(key))
            {
                return (Parameters[key]);
            }
            else
            {
                return (_defaultParameter);
            }
        }

        public string GetParameterValueOrDefault(
            string key,
            string defaultValue)
        {
            if (Parameters.ContainsKey(key))
            {
                return (Parameters[key]);
            }
            else
            {
                return (defaultValue);
            }
        }

        public static SimpleCommandLineParser Parse(string commandLine)
        {
            SimpleCommandLineParser parser = new SimpleCommandLineParser(commandLine);
            parser.ParseCommandLine();
            return (parser);
        }

        #endregion

    }
}
