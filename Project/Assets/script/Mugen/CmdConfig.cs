﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mugen
{

	public enum AI_Type
	{
		none,
		VarSet,
		ChangeState
	}

	public class AI_Command
	{
		public string name {
			get;
			set;
		}

		public AI_Type type
		{
			get;
			set;
		}

		public string value {
			get;
			set;
		}

		public string command
		{
			get;
			set;
		}
	}

    // 用户按键映射
    public class Cmd_Remap: IConfigPropertys
    {

        public string ConfigName
        {
            get
            {
                return string.Empty;
            }
        }

        // name: old key, value: new key
        public string x
        {
            get;
            protected set;
        }

        public string y
        {
            get;
            protected set;
        }

        public string z
        {
            get;
            protected set;
        }

        public string a
        {
            get;
            protected set;
        }

        public string b
        {
            get;
            protected set;
        }

        public string c
        {
            get;
            protected set;
        }

        public string s
        {
            get;
            protected set;
        }
    }

    public class Cmd_Command
    {
        public string name
        {
            get;
            set;
        }

        // 按键组
        public string[] keyCommands
        {
            get;
            set;
        }

        // 多久时间按下按键为有效（单位：帧）
        public int time
        {
            get;
            set;
        }

        public int buffer__time
        {
            get;
            set;
        }

		public string aiName {
			get;
			set;
		}
    }


	public class CmdConfig
	{
        private Cmd_Remap m_Remap = null;
        // 默认接受指令时间(>=1)
        private int m_Command__Time = 1;
        // 默认指令蓄力的时间(>=1 and <= 30)
        private int m_Command__Buffer__Time = 1;
        private Dictionary<string, Cmd_Command> m_CommandMap = null;
		private Dictionary<string, AI_Command> m_AICmdMap = null;

        public Cmd_Command GetCommand(string name)
        {
            if (string.IsNullOrEmpty(name) || m_CommandMap == null)
                return null;
            Cmd_Command ret;
            if (!m_CommandMap.TryGetValue(name, out ret))
                ret = null;
            return ret;
        }

		public AI_Command GetAICommand(string name)
		{
			if (string.IsNullOrEmpty (name) || m_AICmdMap == null)
				return null;
			AI_Command cmd;
			if (!m_AICmdMap.TryGetValue (name, out cmd))
				cmd = null;
			return cmd;
		}

        public Cmd_Command[] GetCommandArray()
        {
            if (m_CommandMap == null)
                return null;
            Cmd_Command[] ret = m_CommandMap.Values.ToArray();
            return ret;
        }

        protected void AddCommand(Cmd_Command command)
        {
            if (command == null)
                return;
            if (m_CommandMap == null)
                m_CommandMap = new Dictionary<string, Cmd_Command>();
            m_CommandMap[command.name] = command;
        }

        public int Command__Time
        {
            get
            {
                return m_Command__Time;
            }
        }

        public int Command__Buffer__Time
        {
            get
            {
                return m_Command__Buffer__Time;
            }
        }

        public Cmd_Remap reMap
        {
            get
            {
                return m_Remap;
            }
        }

		public bool LoadFromFile(string fileName)
		{
            Clear();
			if (string.IsNullOrEmpty (fileName))
				return false;
			string str = AppConfig.GetInstance ().Loader.LoadText (fileName);
			return LoadFromStr (str);
		}

        public void Clear()
        {
            m_Remap = null;
            m_Command__Buffer__Time = 1;
            m_Command__Time = 1;
            if (m_CommandMap != null)
                m_CommandMap.Clear();
        }

		public bool LoadFromStr(string str)
		{
            Clear();
			if (string.IsNullOrEmpty(str))
				return false;
			ConfigReader reader = new ConfigReader ();
			reader.LoadString (str);
			return LoadFromReader (reader);
		}

		public bool LoadFromReader(ConfigReader reader)
		{
            Clear();
			if (reader == null)
				return false;

            ConfigSection section = reader.GetSection("Remap");
            if (section != null)
            {
                m_Remap = new Cmd_Remap();
                if (!section.GetPropertysValues(m_Remap))
                    m_Remap = null;
            }

            section = reader.GetSection("Defaults");
            if (section != null)
            {
                for (int i = 0; i < section.ContentListCount; ++i)
                {
                    string key, value;
                    if (section.GetKeyValue(i, out key, out value))
                    {
                        if (string.Compare(key, "command.time", true) == 0)
                            m_Command__Time = int.Parse(value);
                        else if (string.Compare(key, "command.buffer.time", true) == 0)
                            m_Command__Buffer__Time = int.Parse(value);
                    }
                }
            }

            // 创建Command
            for (int i = 0; i < reader.SectionCount; ++i)
            {
                section = reader.GetSections(i);
                if (section == null)
                    continue;
				if (string.Compare (section.Tile, "Command", true) == 0) {
					Cmd_Command cmd = null;
					for (int j = 0; j < section.ContentListCount; ++j) {
						string key, value;
						if (section.GetKeyValue (j, out key, out value)) {
							if (string.Compare (key, "name", true) == 0) {
								if (cmd == null)
									cmd = new Cmd_Command ();
								cmd.name = value;
							} else if (string.Compare (key, "time", true) == 0) {
								if (cmd == null)
									cmd = new Cmd_Command ();
								cmd.time = int.Parse (value);
							} else if (string.Compare (key, "buffer.time", true) == 0) {
								if (cmd == null)
									cmd = new Cmd_Command ();
								cmd.buffer__time = int.Parse (value);
							} else if (string.Compare (key, "command", true) == 0) {
								if (cmd == null)
									cmd = new Cmd_Command ();
								cmd.keyCommands = ConfigSection.Split (value);
							}
						}
					}
					if (cmd != null && !string.IsNullOrEmpty (cmd.name))
						AddCommand (cmd);
				} else if (section.Tile.StartsWith ("State -1", StringComparison.CurrentCultureIgnoreCase)) {
					
					string[] names = ConfigSection.Split (section.Tile);
					if (names == null || names.Length < 2)
						continue;
					
					string aiName = names [1];
					AI_Type aiType = AI_Type.none;
					AI_Command aiCmd = null;

					for (int j = 0; j < section.ContentListCount; ++j) {
						string key, value;
						if (section.GetKeyValue (j, out key, out value)) {
							if (string.Compare (key, "type", true) == 0) {
								if (string.Compare (value, "ChangeState", true) == 0) {
									aiType = AI_Type.ChangeState;
									if (aiCmd == null) {
										aiCmd = new AI_Command ();
										aiCmd.type = aiType;
										aiCmd.name = aiName;
									}
								}
							} else {
								if (aiCmd == null)
									continue;
								if (string.Compare (key, "value", true) == 0) {
									aiCmd.value = value;
								} else if (string.Compare (key, "triggerall", true) == 0) {
									if (value.StartsWith ("command", StringComparison.CurrentCultureIgnoreCase)) {
										int idx = value.IndexOf ("=");
										if (idx >= 0) {
											aiCmd.command = value.Substring (idx + 1, value.Length - idx - 1).Trim ();
											if (!string.IsNullOrEmpty (aiCmd.command)) {
												Cmd_Command cmdCmd = GetCommand (aiCmd.command);
												if (cmdCmd != null) {
													cmdCmd.aiName = aiCmd.name;
												}
											}
										}
									}
								}
							}
						}
					}


					if (aiCmd == null || aiCmd.type == AI_Type.none)
						continue;
					if (m_AICmdMap == null)
						m_AICmdMap = new Dictionary<string, AI_Command> ();
					m_AICmdMap [aiCmd.name] = aiCmd;

				}
            }

            return true;
		}
	}
}
