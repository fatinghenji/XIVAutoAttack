﻿using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using ECommons.GameFunctions;
using ECommons.GameHelpers;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.GeneratedSheets;
using RotationSolver.Basic.Configuration;
using System.Text.RegularExpressions;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace RotationSolver.Updaters;

internal static partial class TargetUpdater
{
    internal unsafe static void UpdateTarget()
    {
        DataCenter.AllTargets = Svc.Objects.GetObjectInRadius(30);
        var battles = DataCenter.AllTargets.OfType<BattleChara>(); 
        UpdateHostileTargets(battles);
        UpdateFriends(battles
            .Where(b => b.Character()->CharacterData.OnlineStatus != 15 //Removed the one watching cutscene.
            && b.IsTargetable() //Removed the one can't target.
            ));
        UpdateNamePlate(Svc.Objects.OfType<BattleChara>());
    }

    internal static void ClearTarget()
    {
        var empty = Array.Empty<BattleChara>();
        DataCenter.AllTargets
            = DataCenter.AllHostileTargets 
            = DataCenter.TarOnMeTargets
            = DataCenter.PartyMembers
            = DataCenter.PartyTanks
            = DataCenter.PartyHealers
            = DataCenter.AllianceMembers
            = DataCenter.AllianceTanks
            = DataCenter.DyingPeople
            = empty;

        DataCenter.DeathPeopleAll.Delay(empty);
        DataCenter.DeathPeopleParty.Delay(empty);
        DataCenter.WeakenPeople.Delay(empty);
        DataCenter.HostileTargets.Delay(empty);
        DataCenter.CanInterruptTargets.Delay(empty);
    }

    #region Hostile
    private static float JobRange
    {
        get
        {
            float radius = 25;
            if (!Player.Available) return radius;
            switch (Service.GetSheet<ClassJob>().GetRow(
                Player.Object.ClassJob.Id).GetJobRole())
            {
                case JobRole.Tank:
                case JobRole.Melee:
                    radius = 3;
                    break;
            }
            return radius;
        }
    }

    private unsafe static void UpdateHostileTargets(IEnumerable<BattleChara> allTargets)
    {
        DataCenter.AllHostileTargets = allTargets.Where(b =>
        {
            if (!b.IsNPCEnemy()) return false;

            //Dead.
            if (b.CurrentHp == 0) return false;

            if (!b.IsTargetable()) return false;

            if (b.StatusList.Any(StatusHelper.IsInvincible)) return false;

            if (Service.Config.GetValue(PluginConfigBool.OnlyAttackInView))
            {
                if (!Svc.GameGui.WorldToScreen(b.Position, out _)) return false;
            }

            return true;
        });

        DataCenter.HostileTargets.Delay(GetHostileTargets(DataCenter.AllHostileTargets));
        DataCenter.CanInterruptTargets.Delay(DataCenter.HostileTargets.Where(ObjectHelper.CanInterrupt));

        DataCenter.TarOnMeTargets = DataCenter.HostileTargets.Where(tar => tar.TargetObjectId == Player.Object.ObjectId);

        DataCenter.NumberOfHostilesInRange = DataCenter.HostileTargets.Count(o => o.DistanceToPlayer() <= JobRange);

        DataCenter.NumberOfHostilesInMaxRange = DataCenter.HostileTargets.Count(o => o.DistanceToPlayer() <= 25);

        if (DataCenter.HostileTargets.Count() == 1)
        {
            var tar = DataCenter.HostileTargets.FirstOrDefault();

            DataCenter.IsHostileCastingToTank = IsHostileCastingTank(tar);
            DataCenter.IsHostileCastingAOE = IsHostileCastingArea(tar);
        }
        else
        {
            DataCenter.IsHostileCastingToTank = DataCenter.IsHostileCastingAOE = false;
        }
    }

    private static IEnumerable<BattleChara> GetHostileTargets(IEnumerable<BattleChara> allAttackableTargets)
    {
        var type = DataCenter.RightNowTargetToHostileType;
        if (type == TargetHostileType.AllTargetsCanAttack || Service.CountDownTime > 0)
        {
            return allAttackableTargets;
        }

        uint[] ids = GetEnemies();

        var fateId = DataCenter.FateId;

        allAttackableTargets = allAttackableTargets.Where(b =>
        {
            if (Svc.ClientState == null) return false;

            IEnumerable<string> names = Array.Empty<string>();
            if(OtherConfiguration.NoHostileNames.TryGetValue(Svc.ClientState.TerritoryType, out var ns1))
                names = names.Union(ns1);

            if (OtherConfiguration.NoHostileNames.TryGetValue(0, out var ns2))
                names = names.Union(ns2);

            if (names.Any(n => !string.IsNullOrEmpty(n) && new Regex(n).Match(b.Name.ToString()).Success)) return false;

            var tarFateId = b.FateId();
            return tarFateId == 0 || tarFateId == fateId;
        });


        var hostiles = allAttackableTargets.Where(t =>
        {
            if (ids.Contains(t.ObjectId)) return true;
            if (t.TargetObject == Player.Object
            || t.TargetObject?.OwnerId == Player.Object.ObjectId) return true;

            //Remove other's treasure.
            if (t.IsOthersPlayers()) return false;

            if (t.IsTopPriorityHostile()) return true;

            return t.TargetObject is BattleChara;
        });

        if (type == TargetHostileType.TargetsHaveTargetOrAllTargetsCanAttack)
        {
            if (!hostiles.Any()) hostiles = allAttackableTargets;
        }

        return hostiles;
    }

    private static unsafe uint[] GetEnemies()
    {
        if (!Service.Config.GetValue(PluginConfigBool.AddEnemyListToHostile)) return Array.Empty<uint>();

        var addons = Service.GetAddons<AddonEnemyList>();

        if(!addons.Any()) return Array.Empty<uint>();
        var addon = addons.FirstOrDefault();
        var enemy = (AddonEnemyList*)addon;

        var numArray = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder.NumberArrays[19];
        List<uint> list = new(enemy->EnemyCount);
        for (var i = 0; i < enemy->EnemyCount; i++)
        {
            list.Add((uint)numArray->IntArray[8 + i * 6]);
        }
        return list.ToArray();
    }

    private static bool IsHostileCastingTank(BattleChara h)
    {
        return IsHostileCastingBase(h, (act) =>
        {
            return OtherConfiguration.HostileCastingTank.Contains(act.RowId)
                || h.CastTargetObjectId == h.TargetObjectId;
        });
    }

    private static bool IsHostileCastingArea(BattleChara h)
    {
        return IsHostileCastingBase(h, (act) =>
        {
            return OtherConfiguration.HostileCastingArea.Contains(act.RowId);

            //if ((act.CastType == 1 || act.CastType == 2)
            //  && act.Range == 0
            //  && act.EffectRange >= 40)
            //    return true;

            //if (act.CastType == 2
            // && act.EffectRange == 6
            // && act.Cast100ms == 50
            // && act.CanTargetHostile
            // && !act.CanTargetSelf
            // && act.Range == 100)
            //    return true;

            //return false;
        });
    }

    private static bool IsHostileCastingBase(BattleChara h, Func<Action, bool> check)
    {
        if (h.IsCasting)
        {
            if (h.IsCastInterruptible) return false;
            var last = h.TotalCastTime - h.CurrentCastTime;
            var t = last - DataCenter.WeaponRemain;

            if (!(h.TotalCastTime > 2.5 &&
                t > 0 && t < DataCenter.GCDTime(2))) return false;

            var action = Service.GetSheet<Action>().GetRow(h.CastActionId);
            return check?.Invoke(action) ?? false;
        }
        return false;
    }

    #endregion

    #region Friends
    private static Dictionary<uint, uint> _lastHp = new();
    private static uint _lastMp = 0;
    private unsafe static void UpdateFriends(IEnumerable<BattleChara> allTargets)
    {
        DataCenter.PartyMembers = GetPartyMembers(allTargets);
        DataCenter.AllianceMembers = allTargets.Where(ObjectHelper.IsAlliance);

        var mayPet = allTargets.OfType<BattleNpc>().Where(npc => npc.OwnerId == Player.Object.ObjectId);
        DataCenter.HasPet = mayPet.Any(npc => npc.BattleNpcKind == BattleNpcSubKind.Pet);
        //DataCenter.HasPet = HasPet();

        DataCenter.PartyTanks = DataCenter.PartyMembers.GetJobCategory(JobRole.Tank);
        DataCenter.PartyHealers = DataCenter.PartyMembers.GetJobCategory(JobRole.Healer);
        DataCenter.AllianceTanks = DataCenter.AllianceMembers.GetJobCategory(JobRole.Tank);

        var deathAll = DataCenter.AllianceMembers.GetDeath();
        var deathParty = DataCenter.PartyMembers.GetDeath();
        MaintainDeathPeople(ref deathAll, ref deathParty);
        DataCenter.DeathPeopleAll.Delay(deathAll);
        DataCenter.DeathPeopleParty.Delay(deathParty);

        DataCenter.WeakenPeople.Delay(DataCenter.PartyMembers.Where(p => p.StatusList.Any(StatusHelper.CanDispel)));
        DataCenter.DyingPeople = DataCenter.WeakenPeople.Where(p => p.StatusList.Any(StatusHelper.IsDangerous));

        DataCenter.RefinedHP = DataCenter.PartyMembers
            .ToDictionary(p => p.ObjectId, GetPartyMemberHPRatio);
        DataCenter.PartyMembersHP = DataCenter.RefinedHP.Values.Where(r => r > 0);

        if (DataCenter.PartyMembersHP.Any())
        {
            DataCenter.PartyMembersAverHP = DataCenter.PartyMembersHP.Average();
            DataCenter.PartyMembersDifferHP = (float)Math.Sqrt(DataCenter.PartyMembersHP.Average(d => Math.Pow(d - DataCenter.PartyMembersAverHP, 2)));
        }
        else
        {
            DataCenter.PartyMembersAverHP = DataCenter.PartyMembersDifferHP = 0;
        }

        UpdateCanHeal(Player.Object);

        _lastHp = DataCenter.PartyMembers.ToDictionary(p => p.ObjectId, p => p.CurrentHp);

        if (DataCenter.InEffectTime)
        {
            var rightMp = Player.Object.CurrentMp;
            if(rightMp - _lastMp == DataCenter.MPGain)
            {
                DataCenter.MPGain = 0;
            }
            DataCenter.CurrentMp = Math.Min(10000, Player.Object.CurrentMp + DataCenter.MPGain);
        }
        else
        {
            DataCenter.CurrentMp = Player.Object.CurrentMp;
        }
        _lastMp = Player.Object.CurrentMp;
    }

    private static float GetPartyMemberHPRatio(BattleChara member)
    {
        if (member == null) return 0;

        if (!DataCenter.InEffectTime
            || !DataCenter.HealHP.TryGetValue(member.ObjectId, out var hp))
        {
            return (float)member.CurrentHp / member.MaxHp;
        }

        var rightHp = member.CurrentHp;
        if (rightHp > 0)
        {
            if (!_lastHp.TryGetValue(member.ObjectId, out var lastHp)) lastHp = rightHp;

            if (rightHp - lastHp == hp)
            {
                DataCenter.HealHP.Remove(member.ObjectId);
                return (float)member.CurrentHp / member.MaxHp;
            }
            return Math.Min(1, (hp + rightHp) / (float)member.MaxHp);
        }
        return (float)member.CurrentHp / member.MaxHp;
    }

    private static IEnumerable<BattleChara> GetPartyMembers(IEnumerable<BattleChara> allTargets)
    {
        var party = Svc.Party.Select(p => p.GameObject).OfType<BattleChara>().Where(b => b.DistanceToPlayer() <= 30);

        if (!party.Any()) party = new BattleChara[] { Player.Object };

        return party.Union(allTargets.Where(obj => obj.SubKind == 9));
    }

    static SortedDictionary<uint, Vector3> _locations = new();
    private static void MaintainDeathPeople(ref IEnumerable<BattleChara> deathAll, ref IEnumerable<BattleChara> deathParty)
    {
        SortedDictionary<uint, Vector3> locs = new();
        foreach (var item in deathAll)
        {
            locs[item.ObjectId] = item.Position;
        }
        foreach (var item in deathParty)
        {
            locs[item.ObjectId] = item.Position;
        }

        deathAll = FilterForDeath(deathAll);
        deathParty = FilterForDeath(deathParty);

        _locations = locs;
    }

    private static IEnumerable<BattleChara> FilterForDeath(IEnumerable<BattleChara> battleCharas)
    => battleCharas.Where(b =>
    {
        if (!_locations.TryGetValue(b.ObjectId, out var loc)) return false;

        return loc == b.Position;
    });

    static (float min, float max) GetHealRange() => (Service.Config.GetValue(PluginConfigFloat.HealDelayMin), Service.Config.GetValue(PluginConfigFloat.HealDelayMax));

    static RandomDelay _healDelay1 = new(GetHealRange);
    static RandomDelay _healDelay2 = new(GetHealRange);
    static RandomDelay _healDelay3 = new(GetHealRange);
    static RandomDelay _healDelay4 = new(GetHealRange);
    static void UpdateCanHeal(PlayerCharacter player)
    {
        var job = (Job)player.ClassJob.Id;

        var singleAbility = ShouldHealSingle(StatusHelper.SingleHots, job.GetHealthSingleAbility(), job.GetHealthSingleAbilityHot());
        var singleSpell = ShouldHealSingle(StatusHelper.SingleHots, job.GetHealthSingleSpell(), job.GetHealthSingleSpellHot());
        DataCenter.CanHealSingleAbility = singleAbility > 0;
        DataCenter.CanHealSingleSpell = singleSpell > 0;
        DataCenter.CanHealAreaAbility = singleAbility > 2;
        DataCenter.CanHealAreaSpell = singleSpell > 2;

        if (DataCenter.PartyMembers.Count() > 2)
        {
            //TODO:少了所有罩子类技能
            var ratio = GetHealingOfTimeRatio(player, StatusHelper.AreaHots);

            if (!DataCenter.CanHealAreaAbility)
                DataCenter.CanHealAreaAbility = DataCenter.PartyMembersDifferHP < Service.Config.GetValue(PluginConfigFloat.HealthDifference) && DataCenter.PartyMembersAverHP < Lerp(job.GetHealthAreaAbility(), job.GetHealthAreaAbilityHot(), ratio);

            if (!DataCenter.CanHealAreaSpell)
                DataCenter.CanHealAreaSpell = DataCenter.PartyMembersDifferHP < Service.Config.GetValue(PluginConfigFloat.HealthDifference) && DataCenter.PartyMembersAverHP < Lerp(job.GetHealthAreaSpell(), job.GetHealthAreaSpellHot(), ratio);
        }

        //Delay
        
        DataCenter.CanHealSingleAbility = DataCenter.SetAutoStatus(AutoStatus.HealSingleAbility,
            _healDelay1.Delay(DataCenter.CanHealSingleAbility));
        DataCenter.CanHealSingleSpell = DataCenter.SetAutoStatus(AutoStatus.HealSingleSpell,
            _healDelay2.Delay(DataCenter.CanHealSingleSpell));
        DataCenter.CanHealAreaAbility = DataCenter.SetAutoStatus(AutoStatus.HealAreaAbility,
            _healDelay3.Delay(DataCenter.CanHealAreaAbility));
        DataCenter.CanHealAreaSpell = DataCenter.SetAutoStatus(AutoStatus.HealAreaSpell,
            _healDelay4.Delay(DataCenter.CanHealAreaSpell));

        DataCenter.PartyMembersMinHP = DataCenter.PartyMembersHP.Any() ? DataCenter.PartyMembersHP.Min() : 0;
        DataCenter.HPNotFull = DataCenter.PartyMembersMinHP < 1;
    }

    static float GetHealingOfTimeRatio(BattleChara target, params StatusID[] statusIds)
    {
        const float buffWholeTime = 15;

        var buffTime = target.StatusTime(false, statusIds);

        return Math.Min(1, buffTime / buffWholeTime);
    }

    static int ShouldHealSingle(StatusID[] hotStatus, float healSingle, float healSingleHot) => DataCenter.PartyMembers.Count(p =>
    {
        var ratio = GetHealingOfTimeRatio(p, hotStatus);

        var h = p.GetHealthRatio();
        if (h == 0 || !p.NeedHealing()) return false;

        return h < Lerp(healSingle, healSingle, ratio);
    });

    static float Lerp(float a, float b, float ratio)
    {
        return a + (b - a) * ratio;
    }

    #endregion

    private static void UpdateNamePlate(IEnumerable<BattleChara> allTargets)
    {
        List<uint> charas = new(5);
        //60687 - 60691 For treasure hunt.
        for (int i = 60687; i <= 60691; i++)
        {
            var b = allTargets.FirstOrDefault(obj => obj.GetNamePlateIcon() == i);
            if (b == null || b.CurrentHp == 0) continue;
            charas.Add(b.ObjectId);
        }
        DataCenter.TreasureCharas = charas.ToArray();
    }
}
