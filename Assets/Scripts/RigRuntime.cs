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

    /// <summary>Projects current positional amounts using the same counts as gameplay, including zero/inactive components.</summary>
    public static RigEffectProjection[] ProjectEffects(CardInstance[] snapshot)
    {
        var rig=new RigRuntime { members=(snapshot??Array.Empty<CardInstance>()).ToList() };var result=new List<RigEffectProjection>();
        for(int source=0;source<rig.members.Count;source++)
        {
            var member=rig.members[source];
            for(int index=0;index<member.PrimaryAbility.Effects.Count;index++)
            {
                var effect=member.PrimaryAbility.Effects[index];
                if(effect.Trigger!=RigAbilityTrigger.Continuous&&effect.Trigger!=RigAbilityTrigger.Strike)continue;
                int positions=rig.Count(source,effect);
                result.Add(new RigEffectProjection(member.InstanceId,index,positions,effect,checked(effect.Amount(member.Level)*positions)));
            }
        }
        return result.ToArray();
    }

    /// <summary>Inspects positional dependencies and continuous value recipients in an independent committed or preview snapshot.</summary>
    public static RigAbilityLinks InspectAbility(CardInstance[] snapshot, int instanceId)
    {
        var rig = new RigRuntime { members = (snapshot ?? Array.Empty<CardInstance>()).ToList() };
        int index = rig.members.FindIndex(m => m.InstanceId == instanceId);
        var supports = new HashSet<int>(); var recipients = new HashSet<int>();
        if (index >= 0)
            foreach (var effect in rig.members[index].PrimaryAbility.Effects)
            {
                // Completed acquisition grants cannot be strengthened by a future insertion.
                if (effect.Trigger != RigAbilityTrigger.Continuous && effect.Trigger != RigAbilityTrigger.Strike
                    || rig.Count(index, effect) == 0 || effect.Amount(rig.members[index].Level) == 0) continue;
                supports.UnionWith(rig.SupportingMembers(index, effect));
                if (effect.Trigger == RigAbilityTrigger.Continuous
                    && (effect.Kind == RigAbilityKind.ValueBonus || effect.Kind == RigAbilityKind.ValueMultiplier))
                    for (int target = 0; target < rig.members.Count; target++)
                        if (target != index && rig.Targets(index, target, effect) && HasTag(rig.members[target], effect.TargetTag))
                            recipients.Add(rig.members[target].InstanceId);
            }
        return new RigAbilityLinks(supports, recipients);
    }

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
                if (effect.Trigger != RigAbilityTrigger.Continuous && effect.Trigger != RigAbilityTrigger.Strike) continue;
                int positions = Count(i, effect);
                decimal amount = checked(effect.Amount(member.Level) * positions);
                if (amount == 0) continue;
                string label = member.PrimaryAbility.Name + " (Lv " + member.Level + ")";
                if (effect.Kind == RigAbilityKind.LandingBonus || effect.Kind == RigAbilityKind.LandingMultiplier
                    || effect.Kind == RigAbilityKind.PerfectBase || effect.Kind == RigAbilityKind.GoodWidth
                    || effect.Kind == RigAbilityKind.PerfectWidth || effect.Kind == RigAbilityKind.FishSlowInGood
                    || effect.Kind == RigAbilityKind.ExtraAttempts || effect.Kind == RigAbilityKind.MissProtection)
                    rules.Add(LandingRigRule.FromAbility(label, effect, amount,
                        RigAbilityText.Describe(member.PrimaryAbility, member.Level, false)
                        + (effect.PerPosition && effect.Count != RigPositionCount.Once && effect.Count != RigPositionCount.Center && effect.Count != RigPositionCount.LineTip && effect.Count != RigPositionCount.DifferentNeighbors
                            ? "\nCounted " + positions + (effect.Count == RigPositionCount.DistinctTagsBefore || effect.Count == RigPositionCount.DistinctOtherTags ? " different tags." : " matching creatures.") : ""), SupportingMembers(i, effect)));
                if (effect.Kind == RigAbilityKind.ExtraAttempts && effect.Trigger == RigAbilityTrigger.Continuous) extraAttempts = checked(extraAttempts + (int)amount);
                if (effect.Kind == RigAbilityKind.MissProtection) protectedMisses = checked(protectedMisses + (int)amount);
            }
            result.Add(new LandingRigMember(member.InstanceId, member.Definition.DisplayName, CatchAttachmentRole.Catch, member.Tags, rules.ToArray(), member.Level, member.Definition.Rarity));
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
        if (effect.Count == RigPositionCount.Center) return index > 0 && index * 2 == members.Count - 1 ? 1 : 0;
        if (effect.Count == RigPositionCount.LineTip) return index == members.Count - 1 ? 1 : 0;
        if (effect.Count == RigPositionCount.DifferentNeighbors)
            return index > 0 && index + 1 < members.Count && !string.Equals(members[index - 1].Tags[0], members[index + 1].Tags[0], StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        if (effect.Count == RigPositionCount.DistinctTagsBefore || effect.Count == RigPositionCount.DistinctOtherTags)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int j = 0; j < members.Count; j++)
                if (effect.Count == RigPositionCount.DistinctTagsBefore ? j < index : j != index)
                    foreach (string tag in members[j].Tags)
                        if (effect.Count != RigPositionCount.DistinctOtherTags || !HasTag(members[index], tag)) tags.Add(tag);
            return effect.PerPosition ? tags.Count : tags.Count > 0 ? 1 : 0;
        }
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

    /// <summary>Freezes positional enablers beside their resolved count; views never reconstruct rig conditions.</summary>
    private int[] SupportingMembers(int index, RigAbilityEffect effect)
    {
        var result = new List<int>();
        if (effect.Count == RigPositionCount.Once || effect.Count == RigPositionCount.Center || effect.Count == RigPositionCount.LineTip) return result.ToArray();
        if (effect.Count == RigPositionCount.ContiguousOther)
        {
            if (!HasTag(members[index], effect.Tag)) return result.ToArray();
            for (int j = index - 1; j >= 0 && HasTag(members[j], effect.Tag); j--) result.Add(members[j].InstanceId);
            for (int j = index + 1; j < members.Count && HasTag(members[j], effect.Tag); j++) result.Add(members[j].InstanceId);
            return result.ToArray();
        }
        for (int j = 0; j < members.Count; j++)
        {
            bool matches = effect.Count == RigPositionCount.Adjacent || effect.Count == RigPositionCount.DifferentNeighbors ? Math.Abs(j - index) == 1
                : effect.Count == RigPositionCount.Before || effect.Count == RigPositionCount.DistinctTagsBefore ? j < index
                : effect.Count == RigPositionCount.After ? j > index : j != index;
            if (matches && HasTag(members[j], effect.Tag)
                && (effect.Count != RigPositionCount.DistinctOtherTags || !members[j].Tags.Any(tag => HasTag(members[index], tag)))) result.Add(members[j].InstanceId);
        }
        return result.ToArray();
    }

    /// <summary>Resolves directional value recipients with index zero nearest the Rod.</summary>
    private bool Targets(int source, int target, RigAbilityEffect effect)
    {
        if (effect.ValueTarget == RigValueTarget.UntilNextSameTag)
        {
            if (target <= source) return false;
            for (int j = source + 1; j <= target; j++)
                if (HasTag(members[j], members[source].Tags[0])) return false;
            return true;
        }
        return effect.ValueTarget == RigValueTarget.Self ? source == target
        : effect.ValueTarget == RigValueTarget.Adjacent ? Math.Abs(source - target) == 1
        : effect.ValueTarget == RigValueTarget.Before ? target < source : effect.ValueTarget == RigValueTarget.After ? target > source : true;
    }
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
