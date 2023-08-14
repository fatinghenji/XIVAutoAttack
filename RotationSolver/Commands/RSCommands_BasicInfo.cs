﻿using Dalamud.Game.Command;
using ECommons.DalamudServices;
using RotationSolver.Localization;

namespace RotationSolver.Commands;

public static partial class RSCommands
{
    internal static void Enable()
        => Svc.Commands.AddHandler(Service.Command, new CommandInfo(OnCommand)
        {
            HelpMessage = LocalizationManager.RightLang.Commands_Rotation,
            ShowInHelp = true,
        });

    internal static void Disable() => Svc.Commands.RemoveHandler(Service.Command);

    private static void OnCommand(string command, string arguments)
    {
        DoOneCommand(arguments);
    }

    private static void DoOneCommand(string str)
    {
        if (TryGetOneEnum<StateCommandType>(str, out var stateType))
        {
            DoStateCommandType(stateType);
        }
        else if (TryGetOneEnum<SpecialCommandType>(str, out var specialType))
        {
            DoSpecialCommandType(specialType);
        }
        else if (TryGetOneEnum<OtherCommandType>(str, out var otherType))
        {
            DoOtherCommand(otherType, str[otherType.ToString().Length..].Trim());
        }
        else
        {
            RotationSolverPlugin.OpenConfigWindow();
        }
    }

    private static bool TryGetOneEnum<T>(string str, out T type) where T : struct, Enum
    {
        type = default;
        try
        {
            type = Enum.GetValues<T>().First(c => str.StartsWith(c.ToString(), StringComparison.OrdinalIgnoreCase));
            return true;
        }
        catch
        {
            return false;
        }
    }

    internal static string GetCommandStr<T>(this T command, string extraCommand = "")
        where T : struct, Enum
    {
        var cmdStr = Service.Command + " " + command.ToString();
        if (!string.IsNullOrEmpty(extraCommand))
        {
            cmdStr += " " + extraCommand;
        }
        return cmdStr;
    }
}
