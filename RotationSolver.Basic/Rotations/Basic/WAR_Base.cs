﻿using ECommons.DalamudServices;
using ECommons.ExcelServices;
using RotationSolver.Basic.Traits;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// The base class of war.
/// </summary>
public abstract class WAR_Base : CustomRotation
{
    /// <summary>
    /// 
    /// </summary>
    public override MedicineType MedicineType => MedicineType.Strength;

    /// <summary>
    /// 
    /// </summary>
    public sealed override Job[] Jobs => new [] { Job.WAR, Job.MRD };

    static WARGauge JobGauge => Svc.Gauges.Get<WARGauge>();

    /// <summary>
    /// 
    /// </summary>
    protected static byte BeastGauge => JobGauge.BeastGauge;

    #region Attack Single
    /// <summary>
    /// 1
    /// </summary>
    public static IBaseAction HeavySwing { get; } = new BaseAction(ActionID.HeavySwing);

    /// <summary>
    /// 2
    /// </summary>
    public static IBaseAction Maim { get; } = new BaseAction(ActionID.Maim);

    /// <summary>
    /// 3
    /// </summary>
    public static IBaseAction StormsPath { get; } = new BaseAction(ActionID.StormsPath);

    /// <summary>
    /// 4
    /// </summary>
    public static IBaseAction StormsEye { get; } = new BaseAction(ActionID.StormsEye)
    {
        ActionCheck = (b, m) => Player.WillStatusEndGCD(9, 0, true, StatusID.SurgingTempest),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction InnerBeast { get; } = new BaseAction(ActionID.InnerBeast)
    {
        ActionCheck = (b, m) => JobGauge.BeastGauge >= 50 || Player.HasStatus(true, StatusID.InnerRelease),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Tomahawk { get; } = new BaseAction(ActionID.Tomahawk)
    {
        FilterForHostiles = TargetFilter.TankRangeTarget,
        ActionCheck = (b, m) => !IsLastAction(IActionHelper.MovingActions),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Onslaught { get; } = new BaseAction(ActionID.Onslaught, ActionOption.EndSpecial)
    {
        ChoiceTarget = TargetFilter.FindTargetForMoving,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Upheaval { get; } = new BaseAction(ActionID.Upheaval)
    {
        //TODO: Why is that status?
        StatusNeed = new StatusID[] { StatusID.SurgingTempest },
    };
    #endregion

    #region Attack Area
    /// <summary>
    /// 1
    /// </summary>
    public static IBaseAction Overpower { get; } = new BaseAction(ActionID.Overpower);

    /// <summary>
    /// 2
    /// </summary>
    public static IBaseAction MythrilTempest { get; } = new BaseAction(ActionID.MythrilTempest);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction SteelCyclone { get; } = new BaseAction(ActionID.SteelCyclone)
    {
        ActionCheck = InnerBeast.ActionCheck,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction PrimalRend { get; } = new BaseAction(ActionID.PrimalRend)
    {
        StatusNeed = new[] { StatusID.PrimalRendReady }
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Orogeny { get; } = new BaseAction(ActionID.Orogeny);
    #endregion

    #region Heal
    private sealed protected override IBaseAction TankStance => Defiance;

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Defiance { get; } = new BaseAction(ActionID.Defiance, ActionOption.Defense | ActionOption.EndSpecial);
    #endregion

    #region Support
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Infuriate { get; } = new BaseAction(ActionID.Infuriate)
    {
        StatusProvide = new[] { StatusID.NascentChaos },
        ActionCheck = (b, m) => HasHostilesInRange && JobGauge.BeastGauge <= 50 && InCombat,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction InnerRelease { get; } = new BaseAction(ActionID.InnerRelease);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Berserk { get; } = new BaseAction(ActionID.Berserk)
    {
        ActionCheck = (b, m) => HasHostilesInRange && !InnerRelease.IsCoolingDown,
    };
    #endregion

    #region Defense Ability
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ThrillOfBattle { get; } = new BaseAction(ActionID.ThrillOfBattle, ActionOption.Defense);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Equilibrium { get; } = new BaseAction(ActionID.Equilibrium, ActionOption.Defense);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Vengeance { get; } = new BaseAction(ActionID.Vengeance, ActionOption.Defense)
    {
        StatusProvide = Rampart.StatusProvide,
        ActionCheck = BaseAction.TankDefenseSelf,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction RawIntuition { get; } = new BaseAction(ActionID.RawIntuition, ActionOption.Defense)
    {
        ActionCheck = BaseAction.TankDefenseSelf,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction NascentFlash { get; } = new BaseAction(ActionID.NascentFlash, ActionOption.Defense)
    {
        ChoiceTarget = TargetFilter.FindAttackedTarget,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ShakeItOff { get; } = new BaseAction(ActionID.ShakeItOff, ActionOption.Defense);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Holmgang { get; } = new BaseAction(ActionID.Holmgang, ActionOption.Defense)
    {
        ChoiceTarget = (tars, mustUse) => Player,
    };
    #endregion

    #region Traits
    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedInfuriate { get; } = new BaseTrait(157);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait BerserkMastery { get; } = new BaseTrait(218);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait TheBeastWithin    { get; } = new BaseTrait(249);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait InnerBeastMastery    { get; } = new BaseTrait(265);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait SteelCycloneMastery    { get; } = new BaseTrait(266);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait NascentChaos    { get; } = new BaseTrait(267);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MasteringTheBeast    { get; } = new BaseTrait(268);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedThrillOfBattle    { get; } = new BaseTrait(269);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait TankMastery    { get; } = new BaseTrait(318);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedShakeItOff    { get; } = new BaseTrait(417);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait RawIntuitionMastery    { get; } = new BaseTrait(418);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedNascentFlash    { get; } = new BaseTrait(419);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedEquilibrium    { get; } = new BaseTrait(420);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedOnslaught    { get; } = new BaseTrait(421);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MeleeMastery    { get; } = new BaseTrait(505);
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nextGCD"></param>
    /// <param name="act"></param>
    /// <returns></returns>
    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (Holmgang.CanUse(out act) && BaseAction.TankBreakOtherCheck(Jobs[0])) return true;
        return base.EmergencyAbility(nextGCD, out act);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    [RotationDesc(ActionID.Onslaught)]
    protected sealed override bool MoveForwardAbility(out IAction act)
    {
        if (Onslaught.CanUse(out act)) return true;
        return base.MoveForwardAbility(out act);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    [RotationDesc(ActionID.PrimalRend)]
    protected sealed override bool MoveForwardGCD(out IAction act)
    {
        if (PrimalRend.CanUse(out act, CanUseOption.MustUse)) return true;
        return base.MoveForwardGCD(out act);
    }
}
