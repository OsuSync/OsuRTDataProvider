using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuRTDataProvider.Listen;

using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace OsuRTDataProvider.Helper
{
    public static class OsuStatusHelper
    {
        public static bool IsListening(OsuStatus status)
        {
            const OsuStatus listen = OsuStatus.SelectSong | OsuStatus.MatchSetup | OsuStatus.Lobby | OsuStatus.Idle;
            return (listen & status) == status;
        }
    }
}
