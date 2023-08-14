﻿using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using RotationSolver.Basic.Configuration;
using RotationSolver.Commands;

namespace RotationSolver.Updaters;

internal static class ActionUpdater
{
    internal static DateTime _cancelTime = DateTime.MinValue;

    static  RandomDelay _GCDDelay = new(() =>
    (Service.Config.GetValue(PluginConfigFloat.WeaponDelayMin),
    Service.Config.GetValue(PluginConfigFloat.WeaponDelayMax)));

    internal static IAction NextAction { get; set; }
    internal static IBaseAction NextGCDAction { get; set; }
    internal static IAction WrongAction { get; set; }
    static Random _wrongRandom = new();

    internal static void ClearNextAction()
    {
        SetAction(0);
        WrongAction = NextAction = NextGCDAction = null;
    }

    internal static void UpdateNextAction()
    {
        PlayerCharacter localPlayer = Player.Object;
        var customRotation = RotationUpdater.RightNowRotation;

        try
        {
            if (localPlayer != null && customRotation != null
                && customRotation.TryInvoke(out var newAction, out var gcdAction))
            {
                if (Service.Config.GetValue(PluginConfigFloat.MistakeRatio) > 0)
                {
                    var actions = customRotation.AllActions.Where(a =>
                    {
                        if (a.ID == newAction?.ID) return false;
                        if (a is IBaseAction action)
                        {
                            return !action.IsFriendly
                            && action.ChoiceTarget != TargetFilter.FindTargetForMoving
                            && action.CanUse(out _, CanUseOption.MustUseEmpty | CanUseOption.IgnoreClippingCheck);
                        }
                        return false;
                    });
                    WrongAction = actions.ElementAt(_wrongRandom.Next(actions.Count()));
                }

                NextAction = newAction;

                if (gcdAction is IBaseAction GcdAction)
                {
                    if (NextGCDAction != GcdAction)
                    {
                        NextGCDAction = GcdAction;
                    }
                }
                return;
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed to update next action.");
        }

        CustomRotation.MoveTarget = null;
        WrongAction = NextAction = NextGCDAction = null;
    }

    private static void SetAction(uint id) => Svc.PluginInterface.GetOrCreateData("Avarice.ActionOverride", () => new List<uint>() { id })[0] = id;

    internal unsafe static void UpdateActionInfo()
    {
        SetAction(NextGCDAction?.AdjustedID ?? 0);
        UpdateWeaponTime();
        UpdateCombatTime();
        UpdateSlots();
        UpdateMoving();
        UpdateMPTimer();
    }
    private unsafe static void UpdateSlots()
    {
        for (int i = 0; i < DataCenter.BluSlots.Length; i++)
        {
            DataCenter.BluSlots[i] = ActionManager.Instance()->GetActiveBlueMageActionInSlot(i);
        }
        for (ushort i = 0; i < DataCenter.DutyActions.Length; i++)
        {
            DataCenter.DutyActions[i] = ActionManager.GetDutyActionId(i);
        }
    }

    static DateTime _stopMovingTime = DateTime.MinValue;
    private unsafe static void UpdateMoving()
    {
        var last = DataCenter.IsMoving;
        DataCenter.IsMoving = AgentMap.Instance()->IsPlayerMoving > 0;
        if (last && !DataCenter.IsMoving)
        {
            _stopMovingTime = DateTime.Now;
        }
        else if (DataCenter.IsMoving)
        {
            _stopMovingTime = DateTime.MinValue;
        }

        if (_stopMovingTime == DateTime.MinValue)
        {
            DataCenter.StopMovingRaw = 0;
        }
        else
        {
            DataCenter.StopMovingRaw = (float)(DateTime.Now - _stopMovingTime).TotalSeconds;
        }
    }

    static DateTime _startCombatTime = DateTime.MinValue;
    private static void UpdateCombatTime()
    {
        var last = DataCenter.InCombat;
        DataCenter.InCombat = Svc.Condition[ConditionFlag.InCombat];
        if (!last && DataCenter.InCombat)
        {
            _startCombatTime = DateTime.Now;
        }
        else if (last && !DataCenter.InCombat)
        {
            _startCombatTime = DateTime.MinValue;
            if (Service.Config.GetValue(PluginConfigFloat.AutoOffAfterCombat) > 0)
            {
                _cancelTime = DateTime.Now.AddSeconds(Service.Config.GetValue(PluginConfigFloat.AutoOffAfterCombat));
            }
        }

        if (_startCombatTime == DateTime.MinValue)
        {
            DataCenter.CombatTimeRaw = 0;
        }
        else
        {
            DataCenter.CombatTimeRaw = (float)(DateTime.Now - _startCombatTime).TotalSeconds;
        }
    }

    private static unsafe void UpdateWeaponTime()
    {
        var player = Player.Object;
        if (player == null) return;

        var instance = ActionManager.Instance();

        var castTotal = player.TotalCastTime;

        var weaponTotal = instance->GetRecastTime(ActionType.Spell, 11);
        if (castTotal > 0) castTotal += 0.1f;
        if (player.IsCasting) weaponTotal = Math.Max(castTotal, weaponTotal);

        DataCenter.WeaponElapsed = instance->GetRecastTimeElapsed(ActionType.Spell, 11);
        DataCenter.WeaponRemain = DataCenter.WeaponElapsed == 0 ? player.TotalCastTime - player.CurrentCastTime
            : Math.Max(weaponTotal - DataCenter.WeaponElapsed, player.TotalCastTime - player.CurrentCastTime);

        //Casting time.
        if (DataCenter.WeaponElapsed < 0.3) DataCenter.CastingTotal = castTotal;
        if (weaponTotal > 0 && DataCenter.WeaponElapsed > 0.2) DataCenter.WeaponTotal = weaponTotal;
    }

    static uint _lastMP = 0;
    static DateTime _lastMPUpdate = DateTime.Now;

    internal static float MPUpdateElapsed => (float)(DateTime.Now - _lastMPUpdate).TotalSeconds % 3;

    private static void UpdateMPTimer()
    {
        var player = Player.Object;
        if (player == null) return;

        //不是黑魔不考虑啊
        if (player.ClassJob.Id != (uint)ECommons.ExcelServices.Job.BLM) return;

        //有醒梦，就算了啊
        if (player.HasStatus(true, StatusID.LucidDreaming)) return;

        if (_lastMP < player.CurrentMp)
        {
            _lastMPUpdate = DateTime.Now;
        }
        _lastMP = player.CurrentMp;
    }

    internal unsafe static bool CanDoAction()
    {
        if (Svc.Condition[ConditionFlag.OccupiedInQuestEvent]
            || Svc.Condition[ConditionFlag.OccupiedInCutSceneEvent]
            || Svc.Condition[ConditionFlag.Occupied33]
            || Svc.Condition[ConditionFlag.Occupied38]
            || Svc.Condition[ConditionFlag.Jumping61]
            || Svc.Condition[ConditionFlag.BetweenAreas]
            || Svc.Condition[ConditionFlag.BetweenAreas51]
            || Svc.Condition[ConditionFlag.Mounted]
            //|| Svc.Condition[ConditionFlag.SufferingStatusAffliction] //Because of BLU30!
            || Svc.Condition[ConditionFlag.SufferingStatusAffliction2]
            || Svc.Condition[ConditionFlag.RolePlaying]
            || Svc.Condition[ConditionFlag.InFlight]
            ||  ActionManager.Instance()->ActionQueued && NextAction != null
                && ActionManager.Instance()->QueuedActionId != NextAction.AdjustedID
            || Player.Object.CurrentHp == 0) return false;

        var maxAhead = Math.Max(DataCenter.MinAnimationLock - DataCenter.Ping, 0.08f);
        var ahead = Math.Min(maxAhead, Service.Config.GetValue(PluginConfigFloat.ActionAhead));

        //GCD
        var canUseGCD = DataCenter.WeaponRemain <= ahead;
        if (_GCDDelay.Delay(canUseGCD))
        {
            return RSCommands.CanDoAnAction(true);
        }
        if (canUseGCD) return false;

        var nextAction = NextAction;
        if (nextAction == null) return false;

        var timeToNext = DataCenter.ActionRemain;

        //No time to use 0gcd
        if (timeToNext + nextAction.AnimationLockTime
            > DataCenter.WeaponRemain) return false;

        //Skip when casting
        if (DataCenter.WeaponElapsed <= DataCenter.CastingTotal) return false;

        //The last one.
        if (timeToNext + nextAction.AnimationLockTime + DataCenter.Ping + DataCenter.MinAnimationLock > DataCenter.WeaponRemain)
        {
            if (DataCenter.WeaponRemain > nextAction.AnimationLockTime + ahead +
                Math.Max(DataCenter.Ping, Service.Config.GetValue(PluginConfigFloat.MinLastAbilityAdvanced))) return false;

            return RSCommands.CanDoAnAction(false);
        }
        else if (timeToNext < ahead)
        {
            return RSCommands.CanDoAnAction(false);
        }

        return false;
    }
}
