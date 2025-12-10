using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsuRTDataProvider.Listen
{
    public enum PlayType
    {
        Playing = 0,
        Replay = 1,

        Unknown = -1
    }

    [Flags]
    public enum KeysDownFlags
    {
        M1 = 1,
        M2 = 2,
        K1 = 4,
        K2 = 8,
        Smoke = 16,
    }

    public class HitEvent
    {
        public int TimeStamp { get; }
        public KeysDownFlags KeysDown => (KeysDownFlags)Z;
        public float X { get; }
        public float Y { get; }
        private int Z;


        public HitEvent(float x, float y, int z, int timeStamp)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.TimeStamp = timeStamp;
        }
    }
}
