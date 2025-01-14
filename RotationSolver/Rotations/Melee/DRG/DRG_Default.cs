using RotationSolver.Actions;
using RotationSolver.Commands;
using RotationSolver.Configuration.RotationConfig;
using RotationSolver.Data;
using RotationSolver.Helpers;
using RotationSolver.Rotations.Basic;
using RotationSolver.Rotations.CustomRotation;
using System.Collections.Generic;

namespace RotationSolver.Rotations.Melee.DRG;

internal sealed class DRG_Default : DRG_Base
{
    public override string GameVersion => "6.18";

    public override string RotationName => "Default";

    private static bool safeMove = false;


    private protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("DRG_ShouldDelay", true, "�Ӻ����Ѫ")
            .SetBool("DRG_Opener", false, "88������")
            .SetBool("DRG_SafeMove", true, "��ȫλ��");
    }

    public override SortedList<DescType, string> DescriptionDict => new SortedList<DescType, string>()
    {
        {DescType.MoveAction, $"{SpineshatterDive}, {DragonfireDive}"},
    };

    private protected override bool MoveForwardAbility(byte abilityRemain, out IAction act)
    {
        if (abilityRemain > 1)
        {
            if (SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
            if (DragonfireDive.ShouldUse(out act, mustUse: true, emptyOrSkipCombo: true)) return true;
        }

        act = null;
        return false;
    }
    private protected override bool EmergencyAbility(byte abilityRemain, IAction nextGCD, out IAction act)
    {
        if (nextGCD.IsAnySameAction(true, FullThrust, CoerthanTorment)
            || Player.HasStatus(true, StatusID.LanceCharge) && nextGCD.IsAnySameAction(false, FangandClaw))
        {
            //����
            if (abilityRemain == 1 && LifeSurge.ShouldUse(out act, emptyOrSkipCombo: true)) return true;
        }

        return base.EmergencyAbility(abilityRemain, nextGCD, out act);
    }

    private protected override bool AttackAbility(byte abilityRemain, out IAction act)
    {
        if (SettingBreak)
        {
            //��ǹ
            if (LanceCharge.ShouldUse(out act, mustUse: true))
            {
                if (abilityRemain == 1 && !Player.HasStatus(true, StatusID.PowerSurge)) return true;
                if (Player.HasStatus(true, StatusID.PowerSurge)) return true;
            }

            //��������
            if (DragonSight.ShouldUse(out act, mustUse: true)) return true;

            //ս������
            if (BattleLitany.ShouldUse(out act, mustUse: true)) return true;
        }

        //����֮��
        if (Nastrond.ShouldUse(out act, mustUse: true)) return true;

        //׹�ǳ�
        if (Stardiver.ShouldUse(out act, mustUse: true)) return true;

        //����
        if (HighJump.EnoughLevel)
        {
            if (HighJump.ShouldUse(out act)) return true;
        }
        else
        {
            if (Jump.ShouldUse(out act)) return true;
        }

        //���Խ������Ѫ
        if (Geirskogul.ShouldUse(out act, mustUse: true)) return true;

        //�����
        if (SpineshatterDive.ShouldUse(out act, emptyOrSkipCombo: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedAfterGCD(3)) return true;
        }
        if (Player.HasStatus(true, StatusID.PowerSurge) && SpineshatterDive.CurrentCharges != 1 && SpineshatterDive.ShouldUse(out act)) return true;

        //�����
        if (MirageDive.ShouldUse(out act)) return true;

        //���׳�
        if (DragonfireDive.ShouldUse(out act, mustUse: true))
        {
            if (Player.HasStatus(true, StatusID.LanceCharge) && LanceCharge.ElapsedAfterGCD(3)) return true;
        }

        //�����㾦
        if (WyrmwindThrust.ShouldUse(out act, mustUse: true)) return true;

        return false;
    }

    private protected override bool GeneralGCD(out IAction act)
    {
        safeMove = Configs.GetBool("DRG_SafeMove");

        #region Ⱥ��
        if (CoerthanTorment.ShouldUse(out act)) return true;
        if (SonicThrust.ShouldUse(out act)) return true;
        if (DoomSpike.ShouldUse(out act)) return true;

        #endregion

        #region ����
        if (Configs.GetBool("ShouldDelay"))
        {
            if (WheelingThrust.ShouldUse(out act)) return true;
            if (FangandClaw.ShouldUse(out act)) return true;
        }
        else
        {
            if (FangandClaw.ShouldUse(out act)) return true;
            if (WheelingThrust.ShouldUse(out act)) return true;
        }

        //�����Ƿ���Ҫ��Buff
        if (!Player.WillStatusEndGCD(5, 0, true, StatusID.PowerSurge))
        {
            if (FullThrust.ShouldUse(out act)) return true;
            if (VorpalThrust.ShouldUse(out act)) return true;
            if (ChaosThrust.ShouldUse(out act)) return true;
        }
        else
        {
            if (Disembowel.ShouldUse(out act)) return true;
        }
        if (TrueThrust.ShouldUse(out act)) return true;

        if (RSCommands.SpecialType == SpecialCommandType.MoveForward && MoveForwardAbility(1, out act)) return true;
        if (PiercingTalon.ShouldUse(out act)) return true;

        return false;

        #endregion
    }

    private protected override bool DefenceAreaAbility(byte abilityRemain, out IAction act)
    {
        //ǣ��
        if (Feint.ShouldUse(out act)) return true;
        return false;
    }
}
