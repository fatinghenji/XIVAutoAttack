﻿using ECommons.ExcelServices;
using ImGuiScene;
using RotationSolver.Basic.Configuration;
using RotationSolver.Localization;
using RotationSolver.UI.SearchableConfigs;

namespace RotationSolver.UI.SearchableSettings;

internal class CheckBoxSearchPlugin : CheckBoxSearch
{
    private PluginConfigBool _config;
    public override string ID => _config.ToString();

    public override string Name => _config.ToName();

    public override string Description => Action == ActionID.None ? _config.ToDescription() : Action.ToString();

    public override LinkDescription[] Tooltips => _config.ToAction();

    public override string Command => _config.ToCommand();

    public CheckBoxSearchPlugin(PluginConfigBool config, params ISearchable[] children)
        :base(children)
    {
        _config = config;
    }

    protected override bool GetValue(Job job)
    {
        return Service.Config.GetValue(_config);
    }

    protected override void SetValue(Job job, bool value)
    {
        Service.Config.SetValue(_config, value);
    }

    public override void ResetToDefault(Job job)
    {
        Service.Config.SetValue(_config, Service.Config.GetDefault(_config));
    }
}

internal abstract class CheckBoxSearch : Searchable
{
    public ISearchable[] Children { get; protected set; }

    public ActionID Action { get; init; } = ActionID.None;

    public CheckBoxSearch(params ISearchable[] children)
    {
        Children = children;
        foreach (var child in Children)
        {
            child.Parent = this;
        }
    }

    protected abstract bool GetValue(Job job);
    protected abstract void SetValue(Job job, bool value);

    protected virtual void DrawChildren(Job job)
    {
        var lastIs = false;
        foreach (var child in Children)
        {
            var thisIs = child is CheckBoxSearch c && c.Action != ActionID.None && IconSet.GetTexture(c.Action, out var texture);
            if (lastIs && thisIs)
            {
                ImGui.SameLine();
            }
            lastIs = thisIs;

            child.Draw(job);
        }
    }

    protected override void DrawMain(Job job)
    {
        var hasChild = Children != null && Children.Length > 0;
        TextureWrap texture = null;
        var hasIcon = Action != ActionID.None && IconSet.GetTexture(Action, out texture);

        var enable = GetValue(job);
        if (ImGui.Checkbox($"##{ID}", ref enable))
        {
            SetValue(job, enable);
        }
        if (ImGui.IsItemHovered()) ShowTooltip(job);

        ImGui.SameLine();

        var name = $"{Name}##Config_{ID}";
        if(hasIcon)
        {
            ImGui.BeginGroup();
            var cursor = ImGui.GetCursorPos();
            var size = ImGuiHelpers.GlobalScale * 32;
            if (RotationConfigWindow.NoPaddingNoColorImageButton(texture.ImGuiHandle, Vector2.One * size, ID))
            {
                SetValue(job, !enable);
            }
            RotationConfigWindow.DrawActionOverlay(cursor, size, enable ? 1 : 0);
            ImGui.EndGroup();

            if (ImGui.IsItemHovered()) ShowTooltip(job);
        }
        else if (hasChild)
        {
            if (enable)
            {
                var x = ImGui.GetCursorPosX();
                var drawBody = ImGui.TreeNode(name);
                if (ImGui.IsItemHovered()) ShowTooltip(job);

                if (drawBody)
                {
                    ImGui.SetCursorPosX(x);
                    ImGui.BeginGroup();
                    DrawChildren(job);
                    ImGui.EndGroup();
                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, 0x0);
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, 0x0);
                ImGui.TreeNodeEx(name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen);
                if (ImGui.IsItemHovered()) ShowTooltip(job, false);

                ImGui.PopStyleColor(2);
            }
        }
        else
        {
            ImGui.TextWrapped(Name);
            if (ImGui.IsItemHovered()) ShowTooltip(job, false);
        }
    }
}
