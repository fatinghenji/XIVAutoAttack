﻿using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ImGuiScene;
using RotationSolver.Basic.Configuration;
using RotationSolver.Commands;
using RotationSolver.Localization;
using RotationSolver.Updaters;
using System.Drawing;

namespace RotationSolver.UI;

internal class ControlWindow : Window
{
    public const ImGuiWindowFlags BaseFlags = ImGuiWindowFlags.NoScrollbar
                            | ImGuiWindowFlags.NoCollapse
                            | ImGuiWindowFlags.NoTitleBar
                            | ImGuiWindowFlags.NoNav
                            | ImGuiWindowFlags.NoScrollWithMouse;

    public static IAction Wrong { get; set; }
    public static DateTime DidTime { get; set; }

    public ControlWindow()
        : base(nameof(ControlWindow), BaseFlags)
    {
        Size = new Vector2(570f, 300f);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void PreDraw()
    {
        
        Vector4 bgColor = Service.Config.GetValue(PluginConfigBool.IsControlWindowLock)
            ? Service.Config.GetValue(PluginConfigVector4.ControlWindowLockBg)
            : Service.Config.GetValue(PluginConfigVector4.ControlWindowUnlockBg);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, bgColor);

        Flags = BaseFlags;

        if (Service.Config.GetValue(PluginConfigBool.IsControlWindowLock))
        {
            Flags |= ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0);
    }

    public override void PostDraw()
    {
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        base.PostDraw();
    }

    public override void Draw()
    {
        
        ImGui.Columns(3, "Control Bolder", false);
        var gcd = Service.Config.GetValue(PluginConfigFloat.ControlWindowGCDSize) 
            * Service.Config.GetValue(PluginConfigFloat.ControlWindowNextSizeRatio);
        var ability = Service.Config.GetValue(PluginConfigFloat.ControlWindow0GCDSize)
            * Service.Config.GetValue(PluginConfigFloat.ControlWindowNextSizeRatio);
        var width = gcd + ability + ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetColumnWidth(1, 8);

        DrawNextAction(gcd, ability, width);

        ImGui.SameLine();
        var columnWidth = ImGui.GetCursorPosX();
        ImGui.NewLine();

        ImGui.Spacing();

        DrawCommandAction(61751, StateCommandType.Manual, ImGuiColors.DPSRed);

        ImGui.SameLine();
        DrawCommandAction(61764, StateCommandType.Cancel, ImGuiColors.DalamudWhite2);

        ImGui.SameLine();
        columnWidth = Math.Max(columnWidth, ImGui.GetCursorPosX());
        ImGui.NewLine();

        DrawCommandAction(61822, StateCommandType.Auto, ImGuiColors.DPSRed);

        ImGui.SameLine();

        ImGui.BeginGroup();

        ImGui.TextColored(ImGuiColors.DPSRed, DataCenter.TargetingType.ToName());

        var value = Service.Config.GetValue(PluginConfigBool.IsControlWindowLock);
        if(ImGui.Checkbox(LocalizationManager.RightLang.ConfigWindow_Control_IsInfoWindowNoMove, ref value))
        {
            Service.Config.SetValue(PluginConfigBool.IsControlWindowLock, value);
        }
        ImGui.EndGroup();

        ImGui.SameLine();
        columnWidth = Math.Max(columnWidth, ImGui.GetCursorPosX());
        ImGui.NewLine();

        ImGui.SetColumnWidth(0, columnWidth);

        ImGui.NextColumn();
        ImGui.NextColumn();

        DrawSpecials();

        ImGui.Columns(1);
    }

    private static void DrawSpecials()
    {
        var rotation = RotationUpdater.RightNowRotation;

        DrawCommandAction(rotation?.ActionHealAreaGCD, rotation?.ActionHealAreaAbility,
            SpecialCommandType.HealArea, ImGuiColors.HealerGreen);

        ImGui.SameLine();

        DrawCommandAction(rotation?.ActionHealSingleGCD, rotation?.ActionHealSingleAbility,
            SpecialCommandType.HealSingle, ImGuiColors.HealerGreen);

        ImGui.SameLine();

        DrawCommandAction(rotation?.ActionDefenseAreaGCD, rotation?.ActionDefenseAreaAbility,
            SpecialCommandType.DefenseArea, ImGuiColors.TankBlue);

        ImGui.SameLine();

        DrawCommandAction(rotation?.ActionDefenseSingleGCD, rotation?.ActionDefenseSingleAbility,
            SpecialCommandType.DefenseSingle, ImGuiColors.TankBlue);

        ImGui.Spacing();

        DrawCommandAction(rotation?.ActionMoveForwardGCD, rotation?.ActionMoveForwardAbility,
            SpecialCommandType.MoveForward, ImGuiColors.DalamudOrange);

        ImGui.SameLine();

        DrawCommandAction(rotation?.ActionMoveBackAbility,
            SpecialCommandType.MoveBack, ImGuiColors.DalamudOrange);

        ImGui.SameLine();

        DrawCommandAction(61804, SpecialCommandType.Burst, ImGuiColors.DalamudWhite2);

        ImGui.SameLine();

        DrawCommandAction(61753, SpecialCommandType.EndSpecial, ImGuiColors.DalamudWhite2);

        ImGui.Spacing();

        DrawCommandAction(rotation?.EsunaStanceNorthGCD, rotation?.EsunaStanceNorthAbility,
            SpecialCommandType.EsunaStanceNorth, ImGuiColors.ParsedGold);

        ImGui.SameLine();

        DrawCommandAction(rotation?.RaiseShirkGCD, rotation?.RaiseShirkAbility,
            SpecialCommandType.RaiseShirk, ImGuiColors.ParsedBlue);

        ImGui.SameLine();


        DrawCommandAction(rotation?.AntiKnockbackAbility,
            SpecialCommandType.AntiKnockback, ImGuiColors.DalamudWhite2);

        ImGui.SameLine();

        DrawCommandAction(rotation?.ActionSpeedAbility,
            SpecialCommandType.Speed, ImGuiColors.DalamudWhite2);

        ImGui.Spacing();

        ImGui.Text("CMD:");
        ImGui.SameLine();
        
        DrawIAction(DataCenter.CommandNextAction, Service.Config.GetValue(PluginConfigFloat.ControlWindow0GCDSize), 1);

        ImGui.SameLine();

        ImGui.BeginGroup();
        ImGui.Text(DataCenter.RightNowTargetToHostileType switch
        {
             TargetHostileType.AllTargetsCanAttack => LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType1,
             TargetHostileType.TargetsHaveTargetOrAllTargetsCanAttack => LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType2,
             TargetHostileType.TargetsHaveTarget => LocalizationManager.RightLang.ConfigWindow_Param_TargetToHostileType3,
             _ => string.Empty,
        });

        ImGui.Text("Auto: " + DataCenter.AutoStatus.ToString());
        ImGui.EndGroup();

        if (Service.Config.GetValue(PluginConfigFloat.MistakeRatio) > 0)
        {
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DPSRed, "    | Mistake | \n    | Mistake | ");
            ImGui.SameLine();

            DrawIAction(DateTime.Now - DidTime < TimeSpan.FromSeconds(5) ? Wrong : null, 
                Service.Config.GetValue(PluginConfigFloat.ControlWindowGCDSize), 1);
        }
    }

    static void DrawCommandAction(IAction gcd, IAction ability, SpecialCommandType command, Vector4 color)
    {
        var gcdW = Service.Config.GetValue(PluginConfigFloat.ControlWindowGCDSize);
        var abilityW = Service.Config.GetValue(PluginConfigFloat.ControlWindow0GCDSize);
        var width = gcdW + abilityW + ImGui.GetStyle().ItemSpacing.X + ImGui.GetStyle().ItemInnerSpacing.X * 4;
        var str = command.ToString();
        var strWidth = ImGui.CalcTextSize(str).X;

        var pos = ImGui.GetCursorPos();
        ImGui.BeginGroup();
        ImGui.BeginGroup();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, width / 2 - strWidth / 2));
        ImGui.TextColored(color, str);

        var help = GetHelp(command);
        string baseId = "ImgButton" + command.ToString();

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, strWidth / 2 - width / 2));

        if(IconSet.GetTexture(gcd, out var texture))
        {
            DrawIAction(texture.ImGuiHandle, baseId + nameof(gcd), gcdW, command, help);
            if (IconSet.GetTexture(ability, out texture))
            {
                ImGui.SameLine();
                DrawIAction(texture.ImGuiHandle, baseId + nameof(ability), abilityW, command, help);
            }
        }
        ImGui.EndGroup();

        if (DataCenter.SpecialType == command)
        {
            var size = ImGui.GetItemRectSize();
            var winPos = ImGui.GetWindowPos();

            HighLight(winPos + pos, size);

            if(DataCenter.SpecialTimeLeft > 0)
            {
                var time = DataCenter.SpecialTimeLeft.ToString("F2") + "s";
                var strSize = ImGui.CalcTextSize(time);
                CooldownWindow.TextShade(winPos + pos + size - strSize, time);
            }
        }
        ImGui.EndGroup();
    }

    static void HighLight(Vector2 pt, Vector2 size, float thickness = 2f)
    {
        var offset = ImGui.GetStyle().ItemSpacing / 2;
        ImGui.GetWindowDrawList().AddRect(pt - offset, pt + size + offset,
            ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudGrey), 5, ImDrawFlags.RoundCornersAll, thickness);
    }

    static void DrawCommandAction(IAction ability, SpecialCommandType command, Vector4 color)
    {
        if (IconSet.GetTexture(ability, out var texture)) DrawCommandAction(texture, command, color);
    }

    static void DrawCommandAction(uint iconId, SpecialCommandType command, Vector4 color)
    {
        if(IconSet.GetTexture(iconId, out var texture)) DrawCommandAction(texture, command, color);
    }

    static void DrawCommandAction(TextureWrap texture, SpecialCommandType command, Vector4 color)
    {
        var abilityW = Service.Config.GetValue(PluginConfigFloat.ControlWindow0GCDSize);
        var width = abilityW + ImGui.GetStyle().ItemInnerSpacing.X * 2;
        var str = command.ToString();
        var strWidth = ImGui.CalcTextSize(str).X;

        var pos = ImGui.GetCursorPos();
        ImGui.BeginGroup();
        ImGui.BeginGroup();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, width / 2 - strWidth / 2));
        ImGui.TextColored(color, str);

        var help = GetHelp(command);
        string baseId = "ImgButton" + command.ToString();

        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, strWidth / 2 - width / 2));
        if(texture != null) DrawIAction(texture.ImGuiHandle, baseId, abilityW, command, help);
        ImGui.EndGroup();

        if (DataCenter.SpecialType == command)
        {
            var size = ImGui.GetItemRectSize();
            var winPos = ImGui.GetWindowPos();

            HighLight(winPos + pos, size);

            if (DataCenter.SpecialTimeLeft > 0)
            {
                var time = DataCenter.SpecialTimeLeft.ToString("F2") + "s";
                var strSize = ImGui.CalcTextSize(time);
                CooldownWindow.TextShade(winPos + pos + size - strSize, time);
            }
        }
        ImGui.EndGroup();
    }

    static void DrawCommandAction(uint iconId, StateCommandType command, Vector4 color)
    {
        var abilityW = Service.Config.GetValue(PluginConfigFloat.ControlWindow0GCDSize);
        var width = abilityW + ImGui.GetStyle().ItemInnerSpacing.X * 2;
        var str = command.ToString();
        var strWidth = ImGui.CalcTextSize(str).X;

        var pos = ImGui.GetCursorPos();
        ImGui.BeginGroup();
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, width / 2 - strWidth / 2));
        ImGui.TextColored(color, str);

        var help = GetHelp(command);
        string baseId = "ImgButton" + command.ToString();

        if(IconSet.GetTexture(iconId, out var texture))
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, strWidth / 2 - width / 2));
            DrawIAction(texture.ImGuiHandle, baseId, abilityW, command, help);
        }

        ImGui.EndGroup();

        bool isMatch = false;
        switch (command)
        {
            case StateCommandType.Auto when DataCenter.State && !DataCenter.IsManual:
            case StateCommandType.Manual when DataCenter.State && DataCenter.IsManual:
            case StateCommandType.Cancel when !DataCenter.State:
                isMatch = true; 
                break;
        }

        if (isMatch)
        {
            var size = ImGui.GetItemRectSize();
            var winPos = ImGui.GetWindowPos();

            HighLight(winPos + pos, size);
        }
    }

    static string GetHelp(SpecialCommandType command)
    {
        var help = command.ToHelp() + "\n ";

        return help += "\n \n" + LocalizationManager.RightLang.ConfigWindow_Control_ResetButtonOrKeyCommand;
    }

    static string GetHelp(StateCommandType command)
    {
        var help = command.ToHelp() + "\n ";

        return help += "\n \n" + LocalizationManager.RightLang.ConfigWindow_Control_ResetButtonOrKeyCommand;
    }


    static void DrawIAction(nint handle, string id, float width, SpecialCommandType command, string help)
    {
        ImGui.PushID(id);
        if (ImGui.ImageButton(handle, new Vector2(width, width)))
        {
            Svc.Commands.ProcessCommand(command.GetCommandStr());
        }
        ImGui.PopID();
        if (ImGui.IsItemHovered())
        {
            ImGuiHelper.ShowTooltip(help);
        }
    }

    static void DrawIAction(nint handle, string id, float width, StateCommandType command, string help)
    {
        ImGui.PushID(id);
        if (ImGui.ImageButton(handle, new Vector2(width, width)))
        {
            Svc.Commands.ProcessCommand(command.GetCommandStr());
        }
        ImGui.PopID();
        if (ImGui.IsItemHovered())
        {
            ImGuiHelper.ShowTooltip(help);
        }
    }

    internal static (Vector2, Vector2) DrawIAction(IAction action, float width, float percent, bool isAdjust = true)
    {
        if (!IconSet.GetTexture(action, out var texture, isAdjust)) return (default, default);
        var result = DrawIAction(texture.ImGuiHandle, width, action == null ? -1 : percent);
        if (action != null) ImGuiHelper.HoveredString(action.Name, () =>
        {
            if (!DataCenter.State)
            {
                bool canDoIt = false;
                if(action is IBaseAction act)
                {
                    BaseAction.SkipDisable = true;
                    canDoIt = act.CanUse(out _, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo | CanUseOption.IgnoreClippingCheck);
                    BaseAction.SkipDisable = false;
                }
                else if (action is IBaseItem item)
                {
                    canDoIt = item.CanUse(out _, true);
                }
                if(canDoIt) action.Use();
            }
            else
            {
                DataCenter.AddCommandAction(action, 5);
            }
        });
        return result;
    }

    internal static (Vector2, Vector2) DrawIAction(nint handle, float width, float percent)
    {
        var cursor = ImGui.GetCursorPos();
        ImGui.BeginGroup();

        var pt = ImGui.GetCursorPos();
        ImGui.Image(handle, new Vector2(width, width));
        var size = ImGui.GetItemRectSize();
        var pixPerUnit = width / 82;

        if (percent < 0)
        {
            if (IconSet.GetTexture("ui/uld/icona_frame_hr1.tex", out var cover))
            {
                ImGui.SetCursorPos(cursor - new Vector2(pixPerUnit * 3, pixPerUnit * 4));

                var step = new Vector2(88f / cover.Width, 96f / cover.Height);
                var start = new Vector2((96f * 0 + 4f) / cover.Width, (96f * 2) / cover.Height);

                //Out Size is 88, 96
                //Inner Size is 82, 82
                ImGui.Image(cover.ImGuiHandle, new Vector2(pixPerUnit * 88, pixPerUnit * 96),
                    start, start + step);
            }
        }
        else if (percent < 1)
        {
            if (IconSet.GetTexture("ui/uld/icona_recast_hr1.tex", out var cover))
            {
                ImGui.SetCursorPos(cursor - new Vector2(pixPerUnit * 3, pixPerUnit * 0));

                var P = (int)(percent * 81);


                var step = new Vector2(88f / cover.Width, 96f / cover.Height);
                var start = new Vector2(P % 9 * step.X, P / 9 * step.Y);

                //Out Size is 88, 96
                //Inner Size is 82, 82
                ImGui.Image(cover.ImGuiHandle, new Vector2(pixPerUnit * 88, pixPerUnit * 96),
                    start, start + step);
            }
        }
        else
        {
            if (IconSet.GetTexture("ui/uld/icona_frame_hr1.tex", out var cover))
            {

                ImGui.SetCursorPos(cursor - new Vector2(pixPerUnit * 3, pixPerUnit * 4));

                //Out Size is 88, 96
                //Inner Size is 82, 82
                ImGui.Image(cover.ImGuiHandle, new Vector2(pixPerUnit * 88, pixPerUnit * 96),
                    new Vector2(4f / cover.Width, 0f / cover.Height),
                    new Vector2(92f / cover.Width, 96f / cover.Height));
            }
        }

        if (percent > 1)
        {
            if (IconSet.GetTexture("ui/uld/icona_recast2_hr1.tex", out var cover))
            {
                ImGui.SetCursorPos(cursor - new Vector2(pixPerUnit * 3, pixPerUnit * 0));

                var P = (int)(percent % 1 * 81);

                var step = new Vector2(88f / cover.Width, 96f / cover.Height);
                var start = new Vector2((P % 9 + 9) * step.X, P / 9 * step.Y);

                //Out Size is 88, 96
                //Inner Size is 82, 82
                ImGui.Image(cover.ImGuiHandle, new Vector2(pixPerUnit * 88, pixPerUnit * 96),
                    start, start + step);
            }
        }


        ImGui.EndGroup();
        return (pt, size);
    }

    static unsafe void DrawNextAction(float gcd, float ability, float width)
    {
        var str = "Next Action";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + width / 2 - ImGui.CalcTextSize(str).X / 2);
        ImGui.TextColored(ImGuiColors.DalamudYellow, str);

        NextActionWindow.DrawGcdCooldown(width, true);

        DrawIAction(ActionUpdater.NextGCDAction, gcd, 1);

        var next = ActionUpdater.NextGCDAction != ActionUpdater.NextAction ? ActionUpdater.NextAction : null;

        ImGui.SameLine();

        DrawIAction(next, ability, -1);
        if (ImGui.IsItemHovered())
        {
            var help = LocalizationManager.RightLang.ConfigWindow_Control_ResetButtonOrKeyCommand;

            ImGuiHelper.ShowTooltip(help);
        }
    }
}
