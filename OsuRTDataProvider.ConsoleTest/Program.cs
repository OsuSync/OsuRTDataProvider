using System;
using OsuRTDataProvider.Listen;

namespace OsuRTDataProvider.ConsoleTest;

class Program
{
    static void Main(string[] args)
    {
        Logger.OnDebug += s => Console.WriteLine("[ORTDP] DEBUG " + s);
        Logger.OnInfo += s => Console.WriteLine("[ORTDP] INFO " + s);
        Logger.OnWarn += s => Console.WriteLine("[ORTDP] WARN " + s);
        Logger.OnError += s => Console.WriteLine("[ORTDP] ERROR " + s);

        Console.WriteLine("Starting OsuRTDataProvider Console Test...");

        var manager = new OsuListenerManager();

        manager.OnStatusChanged += (last, current) =>
        {
            Console.WriteLine($"Status Changed: {last} -> {current}");
        };

        manager.OnBeatmapChanged += (beatmap) =>
        {
            Console.WriteLine($"Beatmap Changed: {beatmap.Artist} - {beatmap.Title} [{beatmap.Difficulty}] (ID: {beatmap.BeatmapID})");
        };

        manager.OnPlayModeChanged += (last, current) =>
        {
            Console.WriteLine($"PlayMode Changed: {last} -> {current}");
        };

        manager.OnModsChanged += (mods) =>
        {
            Console.WriteLine($"Mods Changed: {mods.Mod}");
        };

        manager.OnScoreChanged += (score) =>
        {
            // Reduce spam
            // Console.WriteLine($"Score: {score}");
        };

        manager.OnComboChanged += (combo) =>
        {
            // Console.WriteLine($"Combo: {combo}");
        };

        manager.OnAccuracyChanged += (acc) =>
        {
            // Console.WriteLine($"Accuracy: {acc:F2}%");
        };

        manager.OnHealthPointChanged += (hp) =>
        {
            // Console.WriteLine($"HP: {hp:F2}");
        };

        manager.Start();

        Console.WriteLine("Listener started. Press 'q' to quit.");

        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.KeyChar == 'q')
                break;
        }

        manager.Stop();
        Console.WriteLine("Exiting...");
    }
}