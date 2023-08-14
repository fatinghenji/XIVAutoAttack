﻿using ECommons.DalamudServices;
using ECommons.ExcelServices;
using RotationSolver.Basic.Traits;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// The base class of WHM
/// </summary>
public abstract class WHM_Base : CustomRotation
{
    /// <summary>
    /// 
    /// </summary>
    public sealed override Job[] Jobs => new Job[] { Job.WHM, Job.CNJ };

    /// <summary>
    /// 
    /// </summary>
    public override MedicineType MedicineType => MedicineType.Mind;

    #region Job Gauge
    private static WHMGauge JobGauge => Svc.Gauges.Get<WHMGauge>();

    /// <summary>
    /// 
    /// </summary>
    protected static byte Lily => JobGauge.Lily;

    /// <summary>
    /// 
    /// </summary>
    protected static byte BloodLily => JobGauge.BloodLily;

    static float LilyTimerRaw => JobGauge.LilyTimer / 1000f;

    /// <summary>
    /// 
    /// </summary>
    protected static float LilyTimer => LilyTimerRaw - DataCenter.WeaponRemain;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    protected static bool LilyAfter(float time) => LilyTimer <= time;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gcdCount"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected static bool LilyAfterGCD(uint gcdCount = 0, float offset = 0)
        => LilyAfter(GCDTime(gcdCount, offset));
    #endregion

    #region Heal
    private protected sealed override IBaseAction Raise => Raise1;

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Raise1 { get; } = new BaseAction(ActionID.Raise1, ActionOption.Friendly);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Cure { get; } = new BaseAction(ActionID.Cure, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Medica { get; } = new BaseAction(ActionID.Medica, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Cure2 { get; } = new BaseAction(ActionID.Cure2, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Medica2 { get; } = new BaseAction(ActionID.Medica2, ActionOption.Hot)
    {
        StatusProvide = new[] { StatusID.Medica2, StatusID.TrueMedica2 },
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Regen { get; } = new BaseAction(ActionID.Regen, ActionOption.Hot)
    {
        TargetStatus = new[]
        {
            StatusID.Regen1,
            StatusID.Regen2,
            StatusID.Regen3,
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Cure3 { get; } = new BaseAction(ActionID.Cure3, ActionOption.Heal | ActionOption.EndSpecial);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Benediction { get; } = new BaseAction(ActionID.Benediction, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Asylum { get; } = new BaseAction(ActionID.Asylum, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction AfflatusSolace { get; } = new BaseAction(ActionID.AfflatusSolace, ActionOption.Heal)
    {
        ActionCheck = (b, m) => JobGauge.Lily > 0,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Tetragrammaton { get; } = new BaseAction(ActionID.Tetragrammaton, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction DivineBenison { get; } = new BaseAction(ActionID.DivineBenison, ActionOption.Defense)
    {
        StatusProvide = new StatusID[] { StatusID.DivineBenison },
        ChoiceTarget = TargetFilter.FindAttackedTarget,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction AfflatusRapture { get; } = new BaseAction(ActionID.AfflatusRapture, ActionOption.Heal)
    {
        ActionCheck = (b, m) => JobGauge.Lily > 0,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Aquaveil { get; } = new BaseAction(ActionID.Aquaveil, ActionOption.Defense)
    {
        ChoiceTarget = TargetFilter.FindAttackedTarget,
    };
    
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction LiturgyOfTheBell { get; } = new BaseAction(ActionID.LiturgyOfTheBell, ActionOption.Heal);
    #endregion

    #region Attack
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Stone { get; } = new BaseAction(ActionID.Stone);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Aero { get; } = new BaseAction(ActionID.Aero, ActionOption.Dot)
    {
        TargetStatus = new StatusID[]
        {
            StatusID.Aero,
            StatusID.Aero2,
            StatusID.Dia,
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Holy { get; } = new BaseAction(ActionID.Holy);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Assize { get; } = new BaseAction(ActionID.Assize);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction AfflatusMisery { get; } = new BaseAction(ActionID.AfflatusMisery)
    {
        ActionCheck = (b, m) => JobGauge.BloodLily == 3,
    };
    #endregion

    #region buff
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction PresenseOfMind { get; } = new BaseAction(ActionID.PresenseOfMind, ActionOption.Buff)
    {
        ActionCheck = (b, m) => !IsMoving
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ThinAir { get; } = new BaseAction(ActionID.ThinAir, ActionOption.Buff);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction PlenaryIndulgence { get; } = new BaseAction(ActionID.PlenaryIndulgence, ActionOption.Heal);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Temperance { get; } = new BaseAction(ActionID.Temperance, ActionOption.Heal);
    #endregion

    #region Traits
    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MaimAndMend    { get; } = new BaseTrait(23);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait Freecure    { get; } = new BaseTrait(25);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MaimAndMend2 { get; } = new BaseTrait(26);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait StoneMastery { get; } = new BaseTrait(179);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait AeroMastery { get; } = new BaseTrait(180);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait StoneMastery2 { get; } = new BaseTrait(181);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait StoneMastery3 { get; } = new BaseTrait(182);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait SecretOfTheLily { get; } = new BaseTrait(196);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait AeroMastery2    { get; } = new BaseTrait(307);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait StoneMastery4    { get; } = new BaseTrait(308);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait TranscendentAfflatus    { get; } = new BaseTrait(309);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedAsylum    { get; } = new BaseTrait(310);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait GlareMastery    { get; } = new BaseTrait(487);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait HolyMastery    { get; } = new BaseTrait(488);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedHealingMagic    { get; } = new BaseTrait(489);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedDivineBenison    { get; } = new BaseTrait(490);
    #endregion
}