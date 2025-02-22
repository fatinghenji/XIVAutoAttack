using System.Collections.Generic;
using System.Linq;
using RotationSolver.Actions.BaseAction;
using RotationSolver.Updaters;
using RotationSolver.Actions;
using RotationSolver.Helpers;
using RotationSolver.Data;
using RotationSolver.Configuration.RotationConfig;
using RotationSolver.Rotations.CustomRotation;
using RotationSolver.Rotations.Basic;

namespace RotationSolver.Rotations.Healer.SGE;

internal sealed class SGE_Default : SGE_Base
{
    public override string GameVersion => "6.18";

    public override string RotationName => "Default";


    /// <summary>
    /// 自用均衡诊断
    /// </summary>
    private static BaseAction MEukrasianDiagnosis { get; } = new(ActionID.EukrasianDiagnosis, true)
    {
        ChoiceTarget = (Targets, mustUse) =>
        {
            var targets = Targets.GetJobCategory(JobRole.Tank);
            if (!targets.Any()) return null;
            return targets.First();
        },
        ActionCheck = b =>
        {
            if (InCombat) return false;
            if (b == Player) return false;
            if (b.HasStatus(false, StatusID.EukrasianDiagnosis, StatusID.EukrasianPrognosis, StatusID.Galvanize)) return false;
            return true;
        }
    };

    protected override bool CanHealSingleSpell => base.CanHealSingleSpell && (Configs.GetBool("GCDHeal") || TargetUpdater.PartyHealers.Count() < 2);
    protected override bool CanHealAreaSpell => base.CanHealAreaSpell && (Configs.GetBool("GCDHeal") || TargetUpdater.PartyHealers.Count() < 2);

    private protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("GCDHeal", false, "自动用GCD奶");
    }

    public override SortedList<DescType, string> DescriptionDict => new()
    {
        {DescType.HealArea, $"GCD: {Prognosis}\n                     能力: {Holos}, {Ixochole}, {Physis}"},
        {DescType.HealSingle, $"GCD: {Diagnosis}\n                     能力: {Druochole}"},
        {DescType.DefenseArea, $"{Panhaima}, {Kerachole}, {Prognosis}"},
        {DescType.DefenseSingle, $"GCD: {Diagnosis}\n                     能力: {Haima}, {Taurochole}"},
        {DescType.MoveAction, $"{Icarus}，目标为面向夹角小于30°内最远目标。"},
    };
    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        act = null!;
        return false;
    }

    private protected override bool EmergencyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        if (base.EmergencyAbility(abilityRemain, nextGCD, out act)) return true;

        //下个技能是
        if (nextGCD.IsAnySameAction(false, Pneuma, EukrasianDiagnosis,
            EukrasianPrognosis, Diagnosis, Prognosis))
        {
            //活化
            if (Zoe.ShouldUse(out act)) return true;
        }

        if (nextGCD == Diagnosis)
        {
            //混合
            if (Krasis.ShouldUse(out act)) return true;
        }

        act = null;
        return false;
    }

    private protected override bool DefenceSingleAbility(byte abilityRemain, out IAction act)
    {

        if (Addersgall == 0 || Dyskrasia.ShouldUse(out _))
        {
            //输血
            if (Haima.ShouldUse(out act)) return true;
        }

        //白牛清汁
        if (Taurochole.ShouldUse(out act) && Taurochole.Target.GetHealthRatio() < 0.8) return true;

        act = null!;
        return false;
    }

    private protected override bool DefenseSingleGCD(out IAction act)
    {
        //诊断
        if (EukrasianDiagnosis.ShouldUse(out act))
        {
            if (EukrasianDiagnosis.Target.HasStatus(true,
                StatusID.EukrasianDiagnosis,
                StatusID.EukrasianPrognosis,
                StatusID.Galvanize
            )) return false;

            //均衡
            if (Eukrasia.ShouldUse(out act)) return true;

            act = EukrasianDiagnosis;
            return true;
        }

        act = null!;
        return false;
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //泛输血
        if (Addersgall == 0 && TargetUpdater.PartyMembersAverHP < 0.7)
        {
            if (Panhaima.ShouldUse(out act)) return true;
        }

        //坚角清汁
        if (Kerachole.ShouldUse(out act)) return true;

        //整体论
        if (Holos.ShouldUse(out act)) return true;

        act = null!;
        return false;
    }

    private protected override bool DefenseAreaGCD(out IAction act)
    {
        //预后
        if (EukrasianPrognosis.ShouldUse(out act))
        {
            if (EukrasianDiagnosis.Target.HasStatus(true,
                StatusID.EukrasianDiagnosis,
                StatusID.EukrasianPrognosis,
                StatusID.Galvanize
            )) return false;

            //均衡
            if (Eukrasia.ShouldUse(out act)) return true;

            act = EukrasianPrognosis;
            return true;
        }

        act = null!;
        return false;
    }

    private protected override bool MoveForwardAbility(byte abilityRemain, out IAction act)
    {
        //神翼
        if (Icarus.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        return false;
    }

    private protected override bool GeneralAbility(byte abilityRemain, out IAction act)
    {
        //心关
        if (Kardia.ShouldUse(out act)) return true;

        //根素
        if (Addersgall == 0 && Rhizomata.ShouldUse(out act)) return true;

        //拯救
        if (Soteria.ShouldUse(out act) && TargetUpdater.PartyMembers.Any(b => b.HasStatus(true, StatusID.Kardion) && b.GetHealthRatio() < Service.Configuration.HealthSingleAbility)) return true;

        //消化
        if (Pepsis.ShouldUse(out act)) return true;

        act = null!;
        return false;
    }

    private protected override bool GeneralGCD(out IAction act)
    {
        //if (HasEukrasia && InCombat && !EukrasianDosis.ShouldUse(out _))
        //{
        //    if (DefenseAreaGCD(out act)) return true;
        //    if (DefenseSingleGCD(out act)) return true;
        //}

        //发炎 留一层走位
        if (Phlegma3.ShouldUse(out act, mustUse: true, emptyOrSkipCombo: IsMoving || Dyskrasia.ShouldUse(out _))) return true;
        if (!Phlegma3.EnoughLevel && Phlegma2.ShouldUse(out act, mustUse: true, emptyOrSkipCombo: IsMoving || Dyskrasia.ShouldUse(out _))) return true;
        if (!Phlegma2.EnoughLevel && Phlegma.ShouldUse(out act, mustUse: true, emptyOrSkipCombo: IsMoving || Dyskrasia.ShouldUse(out _))) return true;

        //失衡
        if (Dyskrasia.ShouldUse(out act)) return true;

        if (EukrasianDosis.ShouldUse(out var enAct))
        {
            //补上Dot
            if (Eukrasia.ShouldUse(out act)) return true;
            act = enAct;
            return true;
        }

        //注药
        if (Dosis.ShouldUse(out act)) return true;

        //箭毒
        if (Toxikon.ShouldUse(out act, mustUse: true)) return true;

        //脱战给T刷单盾嫖豆子
        if (MEukrasianDiagnosis.ShouldUse(out _))
        {
            //均衡
            if (Eukrasia.ShouldUse(out act)) return true;

            act = MEukrasianDiagnosis;
            return true;
        }
        if (Eukrasia.ShouldUse(out act)) return true;


        return false;
    }

    private protected override bool HealSingleAbility(byte abilityRemain, out IAction act)
    {
        //白牛青汁
        if (Taurochole.ShouldUse(out act)) return true;

        //灵橡清汁
        if (Druochole.ShouldUse(out act)) return true;

        //当资源不足时加入范围治疗缓解压力
        var tank = TargetUpdater.PartyTanks;
        var isBoss = Dosis.Target.IsBoss();
        if (Addersgall == 0 && tank.Count() == 1 && tank.Any(t => t.GetHealthRatio() < 0.6f) && !isBoss)
        {
            //整体论
            if (Holos.ShouldUse(out act)) return true;

            //自生
            if (Physis.ShouldUse(out act)) return true;

            //泛输血
            if (Panhaima.ShouldUse(out act)) return true;
        }

        act = null!;
        return false;
    }

    private protected override bool HealSingleGCD(out IAction act)
    {
        if (Diagnosis.ShouldUse(out act)) return true;
        act = null;
        return false;
    }

    private protected override bool HealAreaGCD(out IAction act)
    {
        if (TargetUpdater.PartyMembersAverHP < 0.65f || Dyskrasia.ShouldUse(out _) && TargetUpdater.PartyTanks.Any(t => t.GetHealthRatio() < 0.6f))
        {
            //魂灵风息
            if (Pneuma.ShouldUse(out act, mustUse: true)) return true;
        }

        //预后
        if (EukrasianPrognosis.Target.HasStatus(false, StatusID.EukrasianDiagnosis, StatusID.EukrasianPrognosis, StatusID.Galvanize))
        {
            if (Prognosis.ShouldUse(out act)) return true;
        }

        if (EukrasianPrognosis.ShouldUse(out _))
        {
            //均衡
            if (Eukrasia.ShouldUse(out act)) return true;

            act = EukrasianPrognosis;
            return true;
        }

        act = null;
        return false;
    }
    private protected override bool HealAreaAbility(byte abilityRemain, out IAction act)
    {
        //坚角清汁
        if (Kerachole.ShouldUse(out act) && Level >= 78) return true;

        //自生
        if (Physis.ShouldUse(out act)) return true;

        //整体论
        if (Holos.ShouldUse(out act) && TargetUpdater.PartyMembersAverHP < 0.65f) return true;

        //寄生清汁
        if (Ixochole.ShouldUse(out act)) return true;

        return false;
    }
}
