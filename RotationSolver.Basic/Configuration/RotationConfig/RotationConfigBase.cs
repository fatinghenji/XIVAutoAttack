﻿using ECommons.ExcelServices;

namespace RotationSolver.Basic.Configuration.RotationConfig;

internal abstract class RotationConfigBase : IRotationConfig
{
    public string Name { get; }
    public string DefaultValue { get; }
    public string DisplayName { get; }

    public RotationConfigBase(string name, string value, string displayName)
    {
        Name = name;
        DefaultValue = value;
        DisplayName = displayName;
    }

    public string GetValue(Job job, string rotationName)
    {
        var jobDict = Service.Config.GetJobConfig(job).RotationsConfigurations;
        if (!jobDict.TryGetValue(rotationName, out var configDict)) return DefaultValue;
        if (!configDict.TryGetValue(Name, out var config)) return DefaultValue;
        return config;
    }

    public virtual string GetDisplayValue(Job job, string rotationName) => GetValue(job, rotationName);

    public void SetValue(Job job, string rotationName, string value)
    {
        var jobDict = Service.Config.GetJobConfig(job).RotationsConfigurations;

        if (!jobDict.TryGetValue(rotationName, out var configDict))
        {
            configDict = jobDict[rotationName] = new Dictionary<string, string>();
        }

        configDict[Name] = value;
    }

    public virtual bool DoCommand(IRotationConfigSet set, string str) => str.StartsWith(Name);
}
