﻿using ECommons.DalamudServices;
using ECommons.ExcelServices;
using RotationSolver.Basic.Traits;

namespace RotationSolver.Basic.Rotations.Basic;

/// <summary>
/// NIN action.
/// </summary>
public interface INinAction : IBaseAction
{
    /// <summary>
    /// The aciton it needs.
    /// </summary>
    IBaseAction[] Ninjutsu { get; }
}

/// <summary>
/// The base class of Nin.
/// </summary>
public abstract class NIN_Base : CustomRotation
{
    /// <summary>
    /// 
    /// </summary>
    public override MedicineType MedicineType => MedicineType.Dexterity;

    /// <summary>
    /// 
    /// </summary>
    public sealed override Job[] Jobs => new [] { Job.NIN, Job.ROG };

    #region Job Gauge
    static NINGauge JobGauge => Svc.Gauges.Get<NINGauge>();

    /// <summary>
    /// 
    /// </summary>
    [Obsolete("Better not use this, use HutonEndAfter instead")]
    protected static bool InHuton => HutonTimeRaw > 0;

    /// <summary>
    /// 
    /// </summary>
    protected static byte Ninki => JobGauge.Ninki;

    static float HutonTimeRaw => JobGauge.HutonTimer / 1000f;

    /// <summary>
    /// 
    /// </summary>
    protected static float HutonTime => HutonTimeRaw - DataCenter.WeaponRemain;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    protected static bool HutonEndAfter(float time) => HutonTime <= time;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gctCount"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    protected static bool HutonEndAfterGCD(uint gctCount = 0, float offset = 0)
        => HutonEndAfter(GCDTime(gctCount, offset));
    #endregion

    #region Attack Single
    /// <summary>
    /// 1
    /// </summary>
    public static IBaseAction SpinningEdge { get; } = new BaseAction(ActionID.SpinningEdge);

    /// <summary>
    /// 2
    /// </summary>
    public static IBaseAction GustSlash { get; } = new BaseAction(ActionID.GustSlash);

    /// <summary>
    /// 3
    /// </summary>
    public static IBaseAction AeolianEdge { get; } = new BaseAction(ActionID.AeolianEdge);

    /// <summary>
    /// 4
    /// </summary>
    public static IBaseAction ArmorCrush { get; } = new BaseAction(ActionID.ArmorCrush)
    {
        ActionCheck = (b, m) => HutonEndAfter(25) && !HutonEndAfterGCD(),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Huraijin { get; } = new BaseAction(ActionID.Huraijin)
    {
        ActionCheck = (b, m) => HutonEndAfterGCD(),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction PhantomKamaitachi { get; } = new BaseAction(ActionID.PhantomKamaitachi)
    {
        StatusNeed = new[] { StatusID.PhantomKamaitachiReady },
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ThrowingDagger { get; } = new BaseAction(ActionID.ThrowingDagger)
    {
        FilterForHostiles = TargetFilter.MeleeRangeTargetFilter,
        ActionCheck = (b, m) => !IsLastAction(IActionHelper.MovingActions),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Assassinate { get; } = new BaseAction(ActionID.Assassinate);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction DreamWithinADream { get; } = new BaseAction(ActionID.DreamWithInADream);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Bhavacakra { get; } = new BaseAction(ActionID.Bhavacakra)
    {
        ActionCheck = (b, m) => Ninki >= 50,
    };
    #endregion

    #region Attack Area
    /// <summary>
    /// 1
    /// </summary>
    public static IBaseAction DeathBlossom { get; } = new BaseAction(ActionID.DeathBlossom);

    /// <summary>
    /// 2
    /// </summary>
    public static IBaseAction HakkeMujinsatsu { get; } = new BaseAction(ActionID.HakkeMujinsatsu);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction HellfrogMedium { get; } = new BaseAction(ActionID.HellFrogMedium)
    {
        ActionCheck = Bhavacakra.ActionCheck,
    };
    #endregion

    #region Support
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Meisui { get; } = new BaseAction(ActionID.Meisui)
    {
        StatusNeed = new[] { StatusID.Suiton },
        ActionCheck = (b, m) => JobGauge.Ninki <= 50,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Hide { get; } = new BaseAction(ActionID.Hide, ActionOption.Buff);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ShadeShift { get; } = new BaseAction(ActionID.ShadeShift, ActionOption.Defense);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Mug { get; } = new BaseAction(ActionID.Mug)
    {
        ActionCheck = (b, m) => JobGauge.Ninki <= 60,
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction TrickAttack { get; } = new BaseAction(ActionID.TrickAttack)
    {
        StatusNeed = new StatusID[] { StatusID.Suiton, StatusID.Hidden },
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Bunshin { get; } = new BaseAction(ActionID.Bunshin)
    {
        ActionCheck = Bhavacakra.ActionCheck,
    };
    #endregion

    #region Ninjutsu
    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Ten { get; } = new BaseAction(ActionID.Ten, ActionOption.Buff);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Chi { get; } = new BaseAction(ActionID.Chi, ActionOption.Buff);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Jin { get; } = new BaseAction(ActionID.Jin, ActionOption.Buff);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction TenChiJin { get; } = new BaseAction(ActionID.TenChiJin)
    {
        StatusProvide = new[] { StatusID.Kassatsu, StatusID.TenChiJin },
        ActionCheck = (b, m) => !HutonEndAfterGCD(),
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction FumaShurikenTen { get; } = new BaseAction(ActionID.FumaShurikenTen);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction FumaShurikenJin { get; } = new BaseAction(ActionID.FumaShurikenJin);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction KatonTen { get; } = new BaseAction(ActionID.KatonTen);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction RaitonChi { get; } = new BaseAction(ActionID.RaitonChi);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction DotonChi { get; } = new BaseAction(ActionID.DotonChi);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction SuitonJin { get; } = new BaseAction(ActionID.SuitonJin);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Kassatsu { get; } = new BaseAction(ActionID.Kassatsu)
    {
        StatusProvide = TenChiJin.StatusProvide,
    };

    /// <summary>
    /// 
    /// </summary>
    public class NinAction : BaseAction, INinAction
    {
        /// <summary>
        /// 
        /// </summary>
        public IBaseAction[] Ninjutsu { get; }

        internal NinAction(ActionID actionID, params IBaseAction[] ninjutsu)
            : base(actionID)
        {
            Ninjutsu = ninjutsu;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static INinAction RabbitMedium { get; } = new NinAction(ActionID.RabbitMedium);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction FumaShuriken { get; } = new NinAction(ActionID.FumaShuriken, Ten);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Katon { get; } = new NinAction(ActionID.Katon, Chi, Ten);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Raiton { get; } = new NinAction(ActionID.Raiton, Ten, Chi);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Hyoton { get; } = new NinAction(ActionID.Hyoton, Ten, Jin);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Huton { get; } = new NinAction(ActionID.Huton, Jin, Chi, Ten)
    {
        ActionCheck = (b, m) => HutonEndAfterGCD(),
    };

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Doton { get; } = new NinAction(ActionID.Doton, Jin, Ten, Chi)
    {
        StatusProvide = new[] { StatusID.Doton },
    };

    /// <summary>
    /// 
    /// </summary>
    public static INinAction Suiton { get; } = new NinAction(ActionID.Suiton, Ten, Chi, Jin)
    {
        StatusProvide = new[] { StatusID.Suiton },
    };

    /// <summary>
    /// 
    /// </summary>
    public static INinAction GokaMekkyaku { get; } = new NinAction(ActionID.GokaMekkyaku, Chi, Ten);

    /// <summary>
    /// 
    /// </summary>
    public static INinAction HyoshoRanryu { get; } = new NinAction(ActionID.HyoshoRanryu, Ten, Jin);

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction FleetingRaiju { get; } = new BaseAction(ActionID.FleetingRaiju)
    {
        StatusNeed = new[] { StatusID.RaijuReady },
    };

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction ForkedRaiju { get; } = new BaseAction(ActionID.ForkedRaiju)
    {
        StatusNeed = FleetingRaiju.StatusNeed,
    };
    #endregion

    #region Traits
    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait AllFours { get; } = new BaseTrait(90);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait FleetOfFoot    { get; } = new BaseTrait(93);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait Shukiho    { get; } = new BaseTrait(165);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedShukuchi    { get; } = new BaseTrait(166);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedMug    { get; } = new BaseTrait(167);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedKassatsu    { get; } = new BaseTrait(250);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedShukuchi2    { get; } = new BaseTrait(279);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait Shukiho2 { get; } = new BaseTrait(280);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait Shukiho3 { get; } = new BaseTrait(439);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedMeisui { get; } = new BaseTrait(440);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait EnhancedRaiton { get; } = new BaseTrait(441);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait AdeptAssassination { get; } = new BaseTrait(515);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MeleeMastery { get; } = new BaseTrait(516);

    /// <summary>
    /// 
    /// </summary>
    protected static IBaseTrait MeleeMastery2 { get; } = new BaseTrait(522);
    #endregion

    /// <summary>
    /// 
    /// </summary>
    public static IBaseAction Shukuchi { get; } = new BaseAction(ActionID.Shukuchi, ActionOption.EndSpecial);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    [RotationDesc(ActionID.Shukuchi)]
    protected sealed override bool MoveForwardAbility(out IAction act)
    {
        if (Shukuchi.CanUse(out act)) return true;
        return base.MoveForwardAbility(out act);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    [RotationDesc(ActionID.Feint)]
    protected sealed override bool DefenseAreaAbility(out IAction act)
    {
        if (Feint.CanUse(out act)) return true;
        return base.DefenseAreaAbility(out act);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    [RotationDesc(ActionID.ShadeShift)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        if (ShadeShift.CanUse(out act)) return true;
        return base.DefenseSingleAbility(out act);
    }
}
