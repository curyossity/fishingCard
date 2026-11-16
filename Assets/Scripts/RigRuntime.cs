using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Expedition-owned ordered species, duplicate upgrades, derived abilities and accumulated haul.</summary>
public sealed class RigRuntime
{
    private List<CardInstance> members = new List<CardInstance>();
    private List<CardInstance> removed = new List<CardInstance>();
    private int nextId = 1;
    public long Revision { get; private set; }
    public CardInstance[] Members => members.ToArray();
    public int HaulValue => checked(members.Sum(m => m.HaulValueContribution) + removed.Sum(m => m.HaulValueContribution));

    /// <summary>Finds species by stable identity rather than asset reference, tag, display name or position.</summary>
    public CardInstance FindSpecies(string id) => members.Find(m => string.Equals(m.Definition.UniqueId, id, StringComparison.OrdinalIgnoreCase));

    /// <summary>Reports valid gaps, including continuous insertion restrictions, without changing the rig.</summary>
    public bool CanInsert(CardDefinition creature, int position, out string reason)
    {
        reason = null;
        if (creature == null || position < 0 || position > members.Count) { reason = "Invalid insertion position."; return false; }
        if (FindSpecies(creature.UniqueId) != null) { reason = "This species upgrades its existing position automatically."; return false; }
        for (int i = 0; i < members.Count; i++)
            foreach (var rule in members[i].PrimaryAbility.Effects)
            {
                if (rule.Trigger != RigAbilityTrigger.Continuous || Count(i, rule) == 0) continue;
                if (rule.Kind == RigAbilityKind.BlockInsertionBefore && position <= i
                    || rule.Kind == RigAbilityKind.BlockInsertionAfter && position > i)
                { reason = members[i].Definition.DisplayName + " blocks insertion " + (rule.Kind == RigAbilityKind.BlockInsertionBefore ? "before" : "after") + " it."; return false; }
            }
        return true;
    }

    /// <summary>Simulates complete acquisition on independent snapshots; no live revision, events, levels or value can change.</summary>
    public bool TryPreview(CardDefinition creature, int position, out CardInstance[] preview, out int haul, out string error)
    {
        preview = Array.Empty<CardInstance>(); haul = HaulValue;
        var candidate = Clone();
        if (!candidate.TryAcquireCore(creature, position, out _, out error)) return false;
        preview = candidate.Members; haul = candidate.HaulValue; return true;
    }

    /// <summary>Atomically inserts a new species or upgrades an existing one, rejecting stale commands and numeric overflow.</summary>
    public bool TryAcquire(long revision, CardDefinition creature, int position, out bool upgraded, out string error)
    {
        upgraded = false; error = null;
        if (revision != Revision) { error = "The rig changed. Review placement again."; return false; }
        var candidate = Clone();
        if (!candidate.TryAcquireCore(creature, position, out upgraded, out error)) return false;
        Commit(candidate);
        return true;
    }

    /// <summary>Supports explicitly authorized removal effects; collected base value is retained and live positional bonuses are recalculated.</summary>
    public bool TryRemoveByAbility(long revision, int instanceId, out string error)
    {
        error = null;
        if (revision != Revision) return false;
        var candidate = Clone(); var target = candidate.members.Find(m => m.InstanceId == instanceId);
        if (target == null) return false;
        try
        {
            candidate.members.Remove(target); target.SetRigValue(checked(target.AccumulatedBaseValue + target.TriggeredRigValue));
            candidate.removed.Add(target); candidate.Recalculate();
            Commit(candidate); return true;
        }
        catch (OverflowException) { error = "Rig sale value exceeds supported whole-unit precision."; return false; }
    }

    /// <summary>Captures all earned value before cleanup, including duplicates and preserved base value from removed creatures.</summary>
    public CardInstance[] CreateHaulSnapshot() => members.Concat(removed).Select(m => m.CreateSnapshot()).ToArray();

    /// <summary>Clears all temporary levels, copies and value while invalidating commands from the previous expedition.</summary>
    public void Reset() { members.Clear(); removed.Clear(); nextId = 1; Revision++; }

    /// <summary>Builds immutable current-level scoring and assistance from committed positions only.</summary>
    public LandingRigMember[] SnapshotForLanding(out int extraAttempts, out int protectedMisses)
    {
        extraAttempts = protectedMisses = 0; var result = new List<LandingRigMember>();
        for (int i = 0; i < members.Count; i++)
        {
            var member = members[i]; var rules = new List<LandingRigRule>();
            foreach (var effect in member.PrimaryAbility.Effects)
            {
                if (effect.Trigger != RigAbilityTrigger.Continuous) continue;
                decimal amount = checked(effect.Amount(member.Level) * Count(i, effect));
                string label = member.PrimaryAbility.Name + " (Lv " + member.Level + ")";
                if (effect.Kind == RigAbilityKind.LandingBonus) rules.Add(LandingRigRule.Exact(label, effect.Accuracy, amount, 1));
                if (effect.Kind == RigAbilityKind.LandingMultiplier) rules.Add(LandingRigRule.Exact(label, effect.Accuracy, 0, checked(1 + amount)));
                if (effect.Kind == RigAbilityKind.ExtraAttempts) extraAttempts = checked(extraAttempts + (int)amount);
                if (effect.Kind == RigAbilityKind.MissProtection) protectedMisses = checked(protectedMisses + (int)amount);
            }
            result.Add(new LandingRigMember(member.InstanceId, member.Definition.DisplayName, CatchAttachmentRole.Catch, member.Tags, rules.ToArray(), member.Level));
        }
        return result.ToArray();
    }

    /// <summary>Applies only the matching acquisition trigger; continuous derivation never retriggers a grant.</summary>
    private bool TryAcquireCore(CardDefinition creature, int position, out bool upgraded, out string error)
    {
        upgraded = false; error = null;
        try
        {
            if (creature == null) { error = "No landed creature."; return false; }
            var member = FindSpecies(creature.UniqueId); upgraded = member != null;
            if (!upgraded)
            {
                if (!CanInsert(creature, position, out error)) return false;
                member = CardInstance.CreateRigCreature(nextId++, creature); members.Insert(position, member);
            }
            else member.UpgradeRigCreature();
            int index = members.IndexOf(member);
            foreach (var effect in member.PrimaryAbility.Effects)
                if (effect.Trigger == (upgraded ? RigAbilityTrigger.Upgraded : RigAbilityTrigger.Acquired))
                    member.GrantRigValue(checked((int)(effect.Amount(member.Level) * Count(index, effect))));
            Recalculate();
            SnapshotForLanding(out _, out _); // Validate new power before committing a potentially unusable build.
            _ = HaulValue;
            return true;
        }
        catch (ArgumentException exception) { error = exception.Message; return false; }
        catch (OverflowException) { error = "Rig growth exceeds supported numeric precision. Review ability tuning."; return false; }
    }

    /// <summary>Derives all flat value bonuses before multiplier products from base accumulated value, never from previous derived totals.</summary>
    private void Recalculate()
    {
        var bonuses = new decimal[members.Count]; var multipliers = Enumerable.Repeat(1m, members.Count).ToArray();
        for (int source = 0; source < members.Count; source++)
            foreach (var effect in members[source].PrimaryAbility.Effects)
            {
                if (effect.Trigger != RigAbilityTrigger.Continuous || effect.Kind != RigAbilityKind.ValueBonus && effect.Kind != RigAbilityKind.ValueMultiplier) continue;
                decimal amount = checked(effect.Amount(members[source].Level) * Count(source, effect));
                for (int target = 0; target < members.Count; target++)
                {
                    if (!Targets(source, target, effect) || !HasTag(members[target], effect.TargetTag)) continue;
                    if (effect.Kind == RigAbilityKind.ValueBonus) bonuses[target] = checked(bonuses[target] + amount);
                    else multipliers[target] = checked(multipliers[target] * (1 + amount));
                }
            }
        for (int i = 0; i < members.Count; i++)
            members[i].SetRigValue(checked((int)((members[i].AccumulatedBaseValue + members[i].TriggeredRigValue + bonuses[i]) * multipliers[i])));
    }

    /// <summary>Counts qualifying positions once, regardless of their copy count or level.</summary>
    private int Count(int index, RigAbilityEffect effect)
    {
        if (effect.Count == RigPositionCount.Once) return 1;
        int count = 0;
        if (effect.Count == RigPositionCount.ContiguousOther)
        {
            if (!HasTag(members[index], effect.Tag)) return 0;
            for (int j = index - 1; j >= 0 && HasTag(members[j], effect.Tag); j--) count++;
            for (int j = index + 1; j < members.Count && HasTag(members[j], effect.Tag); j++) count++;
        }
        else for (int j = 0; j < members.Count; j++)
        {
            bool location = effect.Count == RigPositionCount.Adjacent ? Math.Abs(j - index) == 1
                : effect.Count == RigPositionCount.Before ? j < index : effect.Count == RigPositionCount.After ? j > index : j != index;
            if (location && HasTag(members[j], effect.Tag)) count++;
        }
        return effect.PerPosition ? count : count > 0 ? 1 : 0;
    }

    /// <summary>Resolves directional value recipients with index zero nearest the Rod.</summary>
    private static bool Targets(int source, int target, RigAbilityEffect effect) => effect.ValueTarget == RigValueTarget.Self ? source == target
        : effect.ValueTarget == RigValueTarget.Adjacent ? Math.Abs(source - target) == 1
        : effect.ValueTarget == RigValueTarget.Before ? target < source : effect.ValueTarget == RigValueTarget.After ? target > source : true;
    /// <summary>An empty tag filter includes every species without requiring a universal tag.</summary>
    private static bool HasTag(CardInstance member, string tag) => string.IsNullOrWhiteSpace(tag)
        || Array.Exists(member.Tags, t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));
    /// <summary>Creates a fully independent candidate for transactional changes and placement previews.</summary>
    private RigRuntime Clone() => new RigRuntime { members = members.Select(m => m.CreateSnapshot()).ToList(),
        removed = removed.Select(m => m.CreateSnapshot()).ToList(), nextId = nextId, Revision = Revision };

    /// <summary>Commits a prevalidated state while preserving existing object identities for pooled views and scroll state.</summary>
    private void Commit(RigRuntime candidate)
    {
        var existing = members.Concat(removed).ToDictionary(m => m.InstanceId);
        Func<CardInstance, CardInstance> adopt = next =>
        {
            if (!existing.TryGetValue(next.InstanceId, out var original)) return next;
            original.ApplyRigCandidate(next); return original;
        };
        members = candidate.members.Select(adopt).ToList(); removed = candidate.removed.Select(adopt).ToList();
        nextId = candidate.nextId; Revision++;
    }
}
