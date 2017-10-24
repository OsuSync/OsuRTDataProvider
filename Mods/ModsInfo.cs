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
            None = 0,
            NoFail = 1 << 0,
            Easy = 1 << 1,
            //NoVideo = 1 << 2,
            Hidden = 1 << 3,
            HardRock = 1 << 4,
            SuddenDeath = 1 << 5,
            DoubleTime = 1 << 6,
            Relax = 1 << 7,
            HalfTime = 1 << 8,
            Nightcore = 1 << 9,
            Flashlight = 1 << 10,
            Autoplay = 1 << 11,
            SpunOut = 1 << 12,
            Relax2 = 1 << 13,
            Perfect = 1 << 14,
            Key4 = 1 << 15,
            Key5 = 1 << 16,
            Key6 = 1 << 17,
            Key7 = 1 << 18,
            Key8 = 1 << 19,
            FadeIn = 1 << 20,
            Random = 1 << 21,
            Cinema = 1 << 22,
            Target = 1 << 23,
            Key9 = 1 << 24,
            KeyCoop = 1 << 25,
            Key1 = 1 << 26,
            Key3 = 1 << 27,
            Key2 = 1 << 28,
            ScoreV2 = 1 << 29,
        }

        static private List<string> mod_short_str = new List<string>()
        {"","NF","EZ","HD","HR","SD","DT","RL","HT","NC","FL","AP","SO","RL2","PF","1K","2K","3K","4K","5K","6K","7K","8K","9K","KC",
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

        public void Reset()
        {
            Mod = Mods.None;
        }

        public string Name
        {
            get
            {
                return Mod.ToString().Replace(" ","");
            }
        }

        public string ShortName
        {
            get
            {
                string ret = "";
                string mods_str = Name;
                string[] mods_arr = mods_str.Replace(" ", "").Split(',');
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
