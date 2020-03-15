using OsuRTDataProvider.Listen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static OsuRTDataProvider.Listen.OsuListenerManager;

namespace OsuRTDataProvider.Memory
{

    internal class OsuHitEventFinder : OsuFinderBase
    {
        private OsuReplayHitEventFinder replay;
        private OsuPlayingHitEventFinder playing;
        private OsuStatus preStatus = OsuStatus.Unkonwn;
        private long preTime = long.MaxValue;
        private PlayType currentPlayType = PlayType.Unknown;
        public static readonly List<HitEvent> EMPTY_EVENTS = new List<HitEvent>();

        public OsuHitEventFinder(Process process) : base(process)
        {
            replay = new OsuReplayHitEventFinder(process);
            playing = new OsuPlayingHitEventFinder(process);
        }

        public override bool TryInit()
        {
            bool success = true;
            success &= replay.TryInit();
            success &= playing.TryInit();
            return success;
        }

        public void GetHitEvents(OsuStatus osuStatus, long playTime, out PlayType playType, out List<HitEvent> hitEvents, out bool hasChanged)
        {
            hasChanged = false;
            playType = PlayType.Unknown;
            hitEvents = EMPTY_EVENTS;
            if (osuStatus != OsuStatus.Playing)
            {
                hasChanged = this.preStatus == OsuStatus.Playing;
                if (hasChanged) Logger.Debug($"Hit events changed due to osustatus: {OsuStatus.Playing} -> {osuStatus}");

                this.preStatus = osuStatus;
                this.currentPlayType = PlayType.Unknown;
                return;
            }

            if (this.preStatus != OsuStatus.Playing || preTime > playTime)
            {
                replay.Clear();
                playing.Clear();
                if (preStatus != OsuStatus.Playing)
                    Logger.Debug($"Hit events changed due to osustatus: {preStatus} -> {OsuStatus.Playing}");
                else
                    Logger.Debug($"Hit events changed due to playing time: {preTime} -> {playTime}");
                hasChanged = true;
            }
            this.preStatus = OsuStatus.Playing;
            this.preTime = playTime;

            if (currentPlayType == PlayType.Replay || currentPlayType == PlayType.Unknown)
            {
                hasChanged |= replay.GetEvents(out hitEvents);
                currentPlayType = hitEvents.Count == 0 ? PlayType.Unknown : PlayType.Replay;
            }

            if (currentPlayType == PlayType.Playing || currentPlayType == PlayType.Unknown)
            {
                hasChanged |= playing.GetEvents(out hitEvents);
                currentPlayType = hitEvents.Count == 0 ? PlayType.Unknown : PlayType.Playing;
            }

            playType = currentPlayType;
        }
    }

    internal class OsuReplayHitEventFinder : BaseOsuHitEventFinder
    {
        // D9 5D C0 EB 4E A1 ?? ?? ?? ?? 8B 48 34 4E
        // 74 4D A1 ?? ?? ?? ?? 8B 58 34 8D 46 FF
        // A1 ?? ?? ?? ?? 8B 40 34 8B 70 0C 
        // 75 0E 33 D2 89 15 ?? ?? ?? ?? 89 15
        internal override string[] pattern => new string[] {
            "\xD9\x5D\xC0\xEB\x4E\xA1\x00\x00\x00\x00\x8B\x48\x34\x4E",
            "\x74\x4D\xA1\x00\x00\x00\x00\x8B\x58\x34\x8D\x46\xFF",
            "\xA1\x00\x00\x00\x00\x8B\x40\x34\x8B\x70\x0C",
            "\x75\x0E\x33\xD2\x89\x15\x0\x0\x0\x0\x89\x15"
        };

        internal override string[] mask => new string[] { 
            "xxxxxx????xxxx", "xxx????xxxxxx", "x????xxxxxx", "xxxxxx????xx"
        };

        internal override int[] offset => new int[] { 6, 3, 1, 6 };

        internal override string name => "Replay";

        public OsuReplayHitEventFinder(Process osu) : base(osu)
        {
        }
    }

    internal class OsuPlayingHitEventFinder : BaseOsuHitEventFinder
    {
        // 83 7E 60 00 74 2C A1 ?? ?? ?? ?? 8B 50 1C 8B 4A 04
        // 5D C3 A1 ?? ?? ?? ?? 8B 50 1C 8B 4A 04
        internal override string[] pattern => new string[] {
            "\x83\x7E\x60\x00\x74\x2C\xA1\x00\x00\x00\x00\x8B\x50\x1C\x8B\x4A\x04",
            "\x5D\xC3\xA1\x00\x00\x00\x00\x8B\x50\x1C\x8B\x4A\x04"
        };

        internal override string[] mask => new string[] {
            "xxxxxxx????xxxxxx", "xxx????xxxxxx"
        };

        internal override int[] offset => new int[] { 7, 3 };

        internal override string name => "Playing";

        public OsuPlayingHitEventFinder(Process osu) : base(osu)
        {
        }
    }

    internal abstract class BaseOsuHitEventFinder : OsuFinderBase
    {

        IntPtr[] Addresses = new IntPtr[5] { IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero };
        int[] PreOffsets = new int[4] { -1, -1, -1, -1 };

        internal abstract string[] pattern { get; }
        internal abstract string[] mask { get; }
        internal abstract int[] offset { get; }
        internal abstract string name { get; }

        private List<HitEvent> CurrentEvents = new List<HitEvent>();

        public int GetOffset(int offsetDepth, int index)
        {
            switch (offsetDepth)
            {
                case 0: return 0x34;
                case 1: return 0x4;
                case 2: return 0x8 + index * 0x4;
                case 3: return 0;
                default: return -1; // this should not happen
            }
        }


        public BaseOsuHitEventFinder(Process osu) : base(osu)
        {
        }

        public override bool TryInit()
        {
            bool success = false;

            SigScan.Reload();
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    Addresses[0] = SigScan.FindPattern(StringToByte(pattern[i]), mask[i], offset[i]);
                    success = Addresses[0] != IntPtr.Zero;
                    
                    if (!success)
                    {
                        continue;
                    }

                    success = TryReadIntPtrFromMemory(Addresses[0], out Addresses[0]);
                    success &= Addresses[0] != IntPtr.Zero;
                    if (!success)
                    {
                        continue;
                    }

                    LogHelper.LogToFile($"Hit Event ({name}) Base Address: 0x{(int)Addresses[0]:X8} by pattern #{i}");
                    break;
                }
            }
            SigScan.ResetRegion();

            return success;
        }

        private HitEvent GetHitEvent(int index)
        {
            bool success = true, changed = false;
            for (int depth = 0; depth < 4; depth++)
            {
                int offset = GetOffset(depth, index);
                if (offset != PreOffsets[depth] || changed)
                {
                    PreOffsets[depth] = offset;
                    changed = true;
                    success &= TryReadIntPtrFromMemory(Addresses[depth], out Addresses[depth + 1]);
                    if (Addresses[depth + 1] == IntPtr.Zero || !success)
                    {
                        return null;
                    }
                    Addresses[depth + 1] = Addresses[depth + 1] + offset;

                    //LogHelper.LogToFile($"Hit Event Base Address({depth + 1}): 0x{(int)Addresses[depth + 1]:X8}");
                }
            }

            float x, y;
            int z;
            int timeStamp;
            success &= TryReadSingleFromMemory(Addresses[4] + 4, out x);
            success &= TryReadSingleFromMemory(Addresses[4] + 8, out y);
            success &= TryReadIntFromMemory(Addresses[4] + 12, out z);
            success &= TryReadIntFromMemory(Addresses[4] + 16, out timeStamp);

            if (success)
            {
                return new HitEvent(x, y, z, timeStamp);
            }
            return null;
        }

        public void Clear()
        {
            CurrentEvents.Clear();
        }

        // Return if the events are changed.
        public bool GetEvents(out List<HitEvent> hitEvents)
        {

            hitEvents = CurrentEvents;
            PreOffsets = new int[4] { -1, -1, -1, -1 };

            int increment = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            while (true)
            {
                HitEvent hitEvent = GetHitEvent(CurrentEvents.Count);
                if (hitEvent == null)
                {
                    break;
                }
                CurrentEvents.Add(hitEvent);
                increment = increment + 1;
                //LogHelper.LogToFile($"{increment}");
            }

            sw.Stop();
            long time = sw.ElapsedMilliseconds;
            if (increment != 0)
            {
                LogHelper.LogToFile($"Sync hit events: count = {increment}, time = {time}ms, speed = {(time != 0 ? (increment / (double)time) : -1)}/ms");
            }

            return increment != 0;
        }
    }
}
