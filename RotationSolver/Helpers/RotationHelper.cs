﻿using Dalamud.Interface.Colors;
using Dalamud.Logging;
using RotationSolver.Data;
using System.Diagnostics;

namespace RotationSolver.Helpers;

internal record AssemblyInfo(string Name, string Author, string Path, string Support, string Help, string ChangeLog, string Donate);

internal static class RotationHelper
{
    public static List<LoadedAssembly> LoadedCustomRotations = new();

    public static readonly Dictionary<Assembly, AssemblyInfo> _assemblyInfos = new();

    public static string[] AllowedAssembly { get; private set; } = Array.Empty<string>();
    public static AssemblyInfo GetInfo(this Assembly assembly)
    {
        if (_assemblyInfos.TryGetValue(assembly, out var info))
        {
            return info;
        }

        var name = assembly.GetName().Name;
        var location = assembly.Location;
        var version = assembly.GetName().Version?.ToString();
        var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;
        var company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        var product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
        var trademark = assembly.GetCustomAttribute<AssemblyTrademarkAttribute>()?.Trademark;

        var assemblyInfo = new AssemblyInfo(name, version, location, description, company, product, trademark);

        _assemblyInfos[assembly] = assemblyInfo;

        return assemblyInfo;
    }


    public static async Task LoadListAsync()
    {
        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync("https://raw.githubusercontent.com/ArchiDog1998/RotationSolver/main/Resources/whitelist.json");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            AllowedAssembly = JsonConvert.DeserializeObject<string[]>(content);
        }
        catch (Exception ex)
        {
            PluginLog.Log(ex, "Failed to load white List.");
        }
    }

    public static bool IsAllowed(this ICustomRotation rotation, out string name)
    {
        name = "Unknown";
        if (rotation == null) return false;

        Assembly assembly;
        try
        {
            assembly = rotation.GetType().Assembly;
        }
        catch (Exception ex)
        {
            PluginLog.LogError($"Failed to get assembly for rotation {rotation.GetType().Name}: {ex}");
            return false;
        }

        name = assembly.GetName().Name;
        return assembly.IsAllowed();
    }

    public static bool IsAllowed(this Assembly assembly)
    {
        if (_assemblyInfos.TryGetValue(assembly, out var info))
        {
            var assemblyName = info.Name + " - " + info.Author;
            return AllowedAssembly.Contains(assemblyName);
        }
        return false;
    }

    public static Vector4 GetColor(this ICustomRotation rotation)
    {
        if (!rotation.IsValid)
        {
            return ImGuiColors.DPSRed;
        }

        if (!rotation.IsAllowed(out _))
        {
            return ImGuiColors.DalamudViolet;
        }

        if (rotation.IsBeta())
        {
            return ImGuiColors.DalamudOrange;
        }

        return ImGuiColors.DalamudWhite;
    }

    public static bool IsBeta(this ICustomRotation rotation)
    {
        var betaAttribute = rotation.GetType().GetCustomAttribute<BetaRotationAttribute>();
        return betaAttribute != null;
    }

    public static Assembly LoadCustomRotationAssembly(string filePath)
    {
        var loadContext = new RotationLoadContext(new FileInfo(filePath).Directory);
        var assembly = loadContext.LoadFromFile(filePath);
        var assemblyName = assembly.GetName().Name;
        var existingAssembly = GetAssemblyFromPath(filePath);

        if (existingAssembly != null)
        {
            _assemblyInfos.Remove(existingAssembly);
        }

        var author = GetAuthor(filePath, assemblyName);
        var attr = assembly.GetCustomAttribute<AssemblyLinkAttribute>();
        var assemblyInfo = new AssemblyInfo(assemblyName, author, filePath, attr?.SupportLink, attr?.HelpLink, attr?.ChangeLog, attr?.Donate);
        _assemblyInfos[assembly] = assemblyInfo;

        var loadedAssembly = new LoadedAssembly { Path = filePath, LastModified = File.GetLastWriteTimeUtc(filePath).ToString() };
        var existingLoadedAssemblyIndex = LoadedCustomRotations.FindIndex(item => item.Path == loadedAssembly.Path);
        if (existingLoadedAssemblyIndex != -1) LoadedCustomRotations.RemoveAt(existingLoadedAssemblyIndex);
        LoadedCustomRotations.Add(loadedAssembly);

        return assembly;
    }

    private static Assembly GetAssemblyFromPath(string filePath)
    {
        foreach (var asm in _assemblyInfos)
        {
            if (asm.Value.Path == filePath)
            {
                return asm.Key;
            }
        }
        return null;
    }

    private static string GetAuthor(string filePath, string assemblyName)
    {
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
        return string.IsNullOrWhiteSpace(fileVersionInfo.CompanyName) ? assemblyName : fileVersionInfo.CompanyName;
    }
}
