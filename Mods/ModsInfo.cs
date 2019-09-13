using System;
using System.Collections.Generic;
using System.Text;

namespace OsuRTDataProvider.Mods
{
    public struct ModsInfo
    {
        [Flags]
        public enum Mods : uint
        {
            NoMod = 0u,
            None = 0u,
            NoFail = 1u << 0,
            Easy = 1u << 1,
            TouchDevice = 1u << 2,
            Hidden = 1u << 3,
            HardRock = 1u << 4,
            SuddenDeath = 1u << 5,
            DoubleTime = 1u << 6,
            Relax = 1u << 7,
            HalfTime = 1u << 8,
            Nightcore = 1u << 9,
            Flashlight = 1u << 10,
            Autoplay = 1u << 11,
            SpunOut = 1u << 12,
            AutoPilot = 1u << 13,
            Perfect = 1u << 14,
            Key4 = 1u << 15,
            Key5 = 1u << 16,
            Key6 = 1u << 17,
            Key7 = 1u << 18,
            Key8 = 1u << 19,
            FadeIn = 1u << 20,
            Random = 1u << 21,
            Cinema = 1u << 22,
            Target = 1u << 23,
            Key9 = 1u << 24,
            KeyCoop = 1u << 25,
            Key1 = 1u << 26,
            Key3 = 1u << 27,
            Key2 = 1u << 28,
            ScoreV2 = 1u << 29,
            Mirror = 1u << 30,
            Unknown = 0xFFFFFFFFu
        }

        private static Dictionary<string, string> s_name_to_sname = new Dictionary<string, string>
        {
            ["NoMod"] = "",
            ["None"] = "",
            ["NoFail"] = "NF",
            ["Easy"] = "EZ",
            ["TouchDevice"] = "TD",
            ["Hidden"] = "HD",
            ["HardRock"] = "HR",
            ["SuddenDeath"] = "SD",
            ["DoubleTime"] = "DT",
            ["Relax"] = "RL",
            ["HalfTime"] = "HT",
            ["Nightcore"] = "NC",
            ["Flashlight"] = "FL",
            ["Autoplay"] = "Auto",
            ["SpunOut"] = "SO",
            ["AutoPilot"] = "AP",
            ["Perfect"] = "PF",
            ["Key1"] = "1K",
            ["Key2"] = "2K",
            ["Key3"] = "3K",
            ["Key4"] = "4K",
            ["Key5"] = "5K",
            ["Key6"] = "6K",
            ["Key7"] = "7K",
            ["Key8"] = "8K",
            ["Key9"] = "9K",
            ["KeyCoop"] = "Co-op",
            ["FadeIn"] = "FI",
            ["Random"] = "RD",
            ["Cinema"] = "CN",
            ["Target"] = "TP",
            ["ScoreV2"] = "V2",
            ["Mirror"] = "MR",
            ["Unknown"] = "Unknown"
        };

        static public ModsInfo Empty
        {
            get
            {
                ModsInfo m;
                m.m_mod = Mods.Unknown;
                m.m_time_rate = 1.0;
                return m;
            }
        }

        private Mods m_mod;

        /// <summary>
        /// Get Mods
        /// </summary>
        public Mods Mod
        {
            set
            {
                if ((value & Mods.Nightcore) == Mods.Nightcore)
                    value &= ~Mods.DoubleTime;
                else if ((value & Mods.Cinema) == Mods.Cinema)
                    value &= ~Mods.Autoplay;
                else if ((value & Mods.Perfect) == Mods.Perfect)
                    value &= ~Mods.SuddenDeath;

                if ((value & Mods.Nightcore) > 0 || (value & Mods.DoubleTime) > 0)
                    m_time_rate = 1.5;
                else if ((value & Mods.HalfTime) > 0)
                    m_time_rate = 0.75;
                else
                    m_time_rate = 1.0;

                m_mod = value;
            }
            get => m_mod;
        }

        private double m_time_rate;
        public double TimeRate => m_time_rate;

        private static readonly List<Mods> s_invaild_mods_mask = new List<Mods> {
            Mods.Easy|Mods.HardRock,
            Mods.HalfTime|Mods.DoubleTime,
            Mods.SuddenDeath|Mods.AutoPilot,
            Mods.AutoPilot|Mods.Relax,
            Mods.NoFail|Mods.SuddenDeath,
            Mods.NoFail|Mods.Perfect,
            Mods.NoFail|Mods.Relax,
            Mods.NoFail|Mods.AutoPilot,
            Mods.Relax|Mods.SuddenDeath,
            Mods.SpunOut|Mods.AutoPilot,
            Mods.Key1 | Mods.Key3,
            Mods.Key1 | Mods.Key4,
            Mods.Key1 | Mods.Key5,
            Mods.Key1 | Mods.Key6,
            Mods.Key1 | Mods.Key7,
            Mods.Key1 | Mods.Key8,
            Mods.Key1 | Mods.Key9,

            Mods.Key2 | Mods.Key3,
            Mods.Key2 | Mods.Key4,
            Mods.Key2 | Mods.Key5,
            Mods.Key2 | Mods.Key6,
            Mods.Key2 | Mods.Key7,
            Mods.Key2 | Mods.Key8,
            Mods.Key2 | Mods.Key9,

            Mods.Key3 | Mods.Key4,
            Mods.Key3 | Mods.Key5,
            Mods.Key3 | Mods.Key6,
            Mods.Key3 | Mods.Key7,
            Mods.Key3 | Mods.Key8,
            Mods.Key3 | Mods.Key9,

            Mods.Key4 | Mods.Key5,
            Mods.Key4 | Mods.Key6,
            Mods.Key4 | Mods.Key7,
            Mods.Key4 | Mods.Key8,
            Mods.Key4 | Mods.Key9,

            Mods.Key5 | Mods.Key6,
            Mods.Key5 | Mods.Key7,
            Mods.Key5 | Mods.Key8,
            Mods.Key5 | Mods.Key9,

            Mods.Key6 | Mods.Key7,
            Mods.Key6 | Mods.Key8,
            Mods.Key6 | Mods.Key9,

            Mods.Key7 | Mods.Key8,
            Mods.Key7 | Mods.Key9,

            Mods.Key8 | Mods.Key9,
        };

        public static bool VaildMods(ModsInfo mods)
        {
            foreach(var mask in s_invaild_mods_mask)
            {
                if ((mask & mods.Mod) == mask)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get Mods Name
        /// </summary>
        public string Name
        {
            get
            {
                var mods_str = Mod.ToString().Replace(" ", string.Empty);
                mods_str = mods_str.Replace("None", "NoMod");
                if (mods_str.Contains("NoMod,"))
                {
                    mods_str = mods_str.Replace("NoMod,", "");
                }
                return mods_str;
            }
        }

        /// <summary>
        /// Get Short Mods Name
        /// </summary>
        public string ShortName
        {
            get
            {
                string mods_str = Name;
                string[] mods_arr = mods_str.Replace(" ", string.Empty).Split(',');
                StringBuilder b = new StringBuilder(128);

                foreach (var str in mods_arr)
                {
                    if (s_name_to_sname.ContainsKey(str))
                        b.Append(s_name_to_sname[str]);
                    else return "Error";
                    b.Append(',');
                }
                return b.Remove(b.Length - 1, 1).ToString().Trim(',');
            }
        }

        public bool HasMod(Mods mods)
        {
            return (m_mod & mods) > 0;
        }

        public override string ToString()
        {
            return ShortName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ModsInfo))
            {
                return false;
            }

            var info = (ModsInfo)obj;
            return m_mod == info.m_mod;
        }

        public override int GetHashCode()
        {
            var hashCode = -801518429;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + m_mod.GetHashCode();
            return hashCode;
        }

        public static bool operator !=(ModsInfo a, ModsInfo b)
        {
            return a.Mod != b.Mod;
        }

        public static bool operator ==(ModsInfo a, ModsInfo b)
        {
            return a.Mod == b.Mod;
        }

        public static bool operator !=(ModsInfo a, Mods b)
        {
            return a.Mod != b;
        }

        public static bool operator ==(ModsInfo a, Mods b)
        {
            return a.Mod == b;
        }
    }
}