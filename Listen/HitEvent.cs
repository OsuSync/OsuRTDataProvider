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

    public class HitEvent
    {
        public int timeStamp;
        public float x, y, z;

        public HitEvent(float x, float y, float z, int timeStamp)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.timeStamp = timeStamp;
        }
    }
}
