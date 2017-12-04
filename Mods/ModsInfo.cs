using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MemoryReader.Mods
{
    public class ModsInfo
    {
        [Flags]
        public enum Mods:uint
        {
            None = 0u,
            NoFail = 1u << 0,
            Easy = 1u << 1,
            //NoVideo = 1 << 2,
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
            Relax2 = 1u << 13,
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
        }

        static private List<string> mod_short_str = new List<string>()
        {"","NF","EZ","HD","HR","SD","DT","RX","HT","NC","FL","AP","SO","RX2","PF","1K","2K","3K","4K","5K","6K","7K","8K","9K","KC",
         "FI","RD","CE","TG","V2"};

        static private Dictionary<string, string> mod_map = new Dictionary<string, string>();

        private Mods m_mod;
        public Mods Mod {
            set
            {
                if ((value & Mods.Nightcore) == Mods.Nightcore)
                    value &= ~Mods.DoubleTime;
                else if ((value & Mods.Cinema) == Mods.Cinema)
                    value &= ~Mods.Autoplay;
                else if ((value & Mods.Perfect) == Mods.Perfect)
                    value &= ~Mods.SuddenDeath;
                m_mod = value;
            }
            get=>m_mod;
        }

        static ModsInfo()
        {
            int i = 0;
            var fields = typeof(Mods).GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach(var fi in fields)
            {
                mod_map.Add(fi.Name, mod_short_str[i++]);
            }
        }

        public ModsInfo()
        {
            Mod = Mods.None;
        }

        public string Name
        {
            get
            {
                return Mod.ToString().Replace(" ",string.Empty);
            }
        }

        public string ShortName
        {
            get
            {
                string ret = string.Empty;
                string mods_str = Name;
                string[] mods_arr = mods_str.Replace(" ", string.Empty).Split(',');
                foreach (var str in mods_arr)
                {
                    if (mod_map.ContainsKey(str))
                        ret += mod_map[str];
                    else return "Error";
                    ret += ",";
                }
                return ret.Remove(ret.Length - 1);
            }
        }

        public override string ToString()
        {
            return ShortName;
        }
    }
}
