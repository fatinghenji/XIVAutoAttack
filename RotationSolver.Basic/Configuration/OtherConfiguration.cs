﻿using Dalamud.Game.ClientState.GamePad;
using Dalamud.Logging;
using ECommons.DalamudServices;

namespace RotationSolver.Basic.Configuration;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class OtherConfiguration
{
    public static HashSet<uint> HostileCastingArea = new();
    public static HashSet<uint> HostileCastingTank = new();

    public static SortedList<uint, float> AnimationLockTime = new();

    public static Dictionary<uint, string[]> NoHostileNames = new();

    public static HashSet<uint> DangerousStatus = new();

    public static HashSet<uint> InvincibleStatus = new();

    public static void Init()
    {
        if (!Directory.Exists(Svc.PluginInterface.ConfigDirectory.FullName))
        {
            Directory.CreateDirectory(Svc.PluginInterface.ConfigDirectory.FullName);
        }

        Task.Run(() => InitOne(ref DangerousStatus, nameof(DangerousStatus)));

        Task.Run(() => InitOne(ref InvincibleStatus, nameof(InvincibleStatus)));

        Task.Run(() => InitOne(ref NoHostileNames, nameof(NoHostileNames)));

        Task.Run(() => InitOne(ref AnimationLockTime, nameof(AnimationLockTime)));

        Task.Run(() => InitOne(ref HostileCastingArea, nameof(HostileCastingArea)));

        Task.Run(() => InitOne(ref HostileCastingTank, nameof(HostileCastingTank)));

    }

    public static void Save()
    {
        SaveDangerousStatus();
        SaveInvincibleStatus();
        SaveNoHostileNames();
        SaveAnimationLockTime();
        SaveHostileCastingArea();
        SaveHostileCastingTank();
    }

    public static void SaveHostileCastingArea()
    {
        Task.Run(() => Save(HostileCastingArea, nameof(HostileCastingArea)));
    }

    public static void SaveHostileCastingTank()
    {
        Task.Run(() => Save(HostileCastingTank, nameof(HostileCastingTank)));
    }

    public static void SaveDangerousStatus()
    {
        Task.Run(() => Save(DangerousStatus, nameof(DangerousStatus)));
    }

    public static void SaveInvincibleStatus()
    {
        Task.Run(() => Save(InvincibleStatus, nameof(InvincibleStatus)));
    }

    public static void SaveNoHostileNames()
    {
        Task.Run(() => Save(NoHostileNames, nameof(NoHostileNames)));
    }

    public static void SaveAnimationLockTime()
    {
        Task.Run(() => Save(AnimationLockTime, nameof(AnimationLockTime)));
    }

    private static string GetFilePath(string name)
    {
        var directory = Svc.PluginInterface.ConfigDirectory.FullName;
#if DEBUG
        var dir = @"E:\OneDrive - stu.zafu.edu.cn\PartTime\FFXIV\RotationSolver\Resources";
        if (Directory.Exists(dir)) directory = dir;
#endif

        return directory + $"\\{name}.json";
    }

    private static void Save<T>(T value, string name)
        => SavePath(value, GetFilePath(name));

    private static void SavePath<T>(T value, string path)
    {
        File.WriteAllTextAsync(path,
        JsonConvert.SerializeObject(value, Formatting.Indented));
    }

    private static void InitOne<T>(ref T value, string name)
    {
        var path = GetFilePath(name);
        if (File.Exists(path))
        {
            try
            {
                value = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                PluginLog.Warning(ex, $"Failed to load {name}.");
            }
        }
        else
        {
            try
            {
                var client = new HttpClient();
                var str = client.GetStringAsync($"https://raw.githubusercontent.com/ArchiDog1998/RotationSolver/main/Resources/{name}.json").Result;

                File.WriteAllText(path, str);
                value = JsonConvert.DeserializeObject<T>(str);
            }
            catch
            {
                SavePath(value, path);
            }
        }
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
