using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class CatchChainRuntime
{
    [SerializeField] private CardInstance[] catches = Array.Empty<CardInstance>();
    [SerializeField] private ActiveCatchEffectRecord[] activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
    [SerializeField] private CardInstance[] releasedCatches = Array.Empty<CardInstance>();
    [SerializeField] private CardInstance[] lostCatches = Array.Empty<CardInstance>();
    [SerializeField] private string[] consumedNextCatchEffectKeys = Array.Empty<string>();
    [SerializeField] private string[] consumedNextEncounterEffectKeys = Array.Empty<string>();
    [SerializeField] private string[] invalidatedNoReleaseEffectKeys = Array.Empty<string>();
    [SerializeField] private int nextInstanceId = 1;

    /// <summary>Reports each completed deliberate removal without firing for forced loss or exchange.</summary>
    public event Action<CardInstance, CatchRemovalReason> DeliberatelyReleased;

    /// <summary>Reports individual Technique targets before applying changes or removing their attachment.</summary>
    public event Action<CardInstance> TechniqueTargetSelected;

    [SerializeField] private List<PendingCreatureCatchEffect> pendingCreatureCatchEffects = new List<PendingCreatureCatchEffect>();
    public PendingCreatureCatchEffect[] PendingCreatureCatchEffects => pendingCreatureCatchEffects.ToArray();
    public CardInstance[] Catches => catches;
    public ActiveCatchEffectRecord[] ActiveEffectRecords => activeEffectRecords;
    public CardInstance[] ReleasedCatches => releasedCatches;
    public CardInstance[] LostCatches => lostCatches;

    /// <summary>
    /// Calculates the Line Load contributed by all catches still attached to the line.
    /// </summary>
    public int CurrentLineLoad
    {
        get
        {
            int load = 0;

            for (int i = 0; i < catches.Length; i++)
            {
                if (catches[i] != null)
                {
                    load += catches[i].CurrentWeight;
                }
            }

            return load;
        }
    }

    /// <summary>
    /// Adds a committed encounter and tracks its catch-related effects.
    /// </summary>
    public CardInstance Add(
        CardDefinition caughtCard,
        EffectResolver effectResolver,
        System.Random random = null)
    {
        return Add(caughtCard, effectResolver, CatchAttachmentRole.Catch, random);
    }

    /// <summary>Adds a committed encounter in the selected Catch or Bait role.</summary>
    public CardInstance Add(
        CardDefinition caughtCard,
        EffectResolver effectResolver,
        CatchAttachmentRole attachmentRole,
        System.Random random = null)
    {
        if (caughtCard == null)
        {
            return null;
        }

        CardInstance caughtInstance = new CardInstance(nextInstanceId, caughtCard, random, attachmentRole);
        nextInstanceId++;
        catches = AppendCatch(catches, caughtInstance);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        ResolveNewAttachment(caughtInstance, effectResolver);
        return caughtInstance;
    }

    /// <summary>
    /// Reports whether an immediate Technique effect has at least one valid Catch Chain target.
    /// </summary>
    public bool CanApplyTechniqueEffect(CardEffectDefinition effect)
    {
        if (effect == null)
        {
            return false;
        }

        if (effect.Target != CardEffectTarget.CatchChain
            && effect.Target != CardEffectTarget.SpecificCaughtCard)
        {
            return false;
        }

        if (effect.EffectType == CardEffectType.ModifyCaughtCard)
        {
            return CountMatchingCatches(effect.RequiredTags) >= 2;
        }

        if (effect.EffectType != CardEffectType.AddLineLoadModifier
            && effect.EffectType != CardEffectType.RemoveLineLoadModifier
            && effect.EffectType != CardEffectType.ModifyCatchValue
            && effect.EffectType != CardEffectType.ReleaseCatch)
        {
            return false;
        }

        return FindTechniqueTargetIndex(effect) >= 0;
    }

    /// <summary>
    /// Applies an immediate Technique effect to automatically selected Catch Chain targets.
    /// </summary>
    public bool TryApplyTechniqueEffect(
        CardEffectDefinition effect,
        EffectResolver effectResolver,
        out string resultSummary)
    {
        resultSummary = string.Empty;

        if (!CanApplyTechniqueEffect(effect))
        {
            return false;
        }

        if (effect.EffectType == CardEffectType.ReleaseCatch)
        {
            return ReleaseTechniqueTargets(effect, effectResolver, out resultSummary);
        }

        if (effect.EffectType == CardEffectType.ModifyCaughtCard)
        {
            int matchingCount = CountMatchingCatches(effect.RequiredTags);
            int valueChange = effect.Amount * (matchingCount - 1);

            for (int i = 0; i < catches.Length; i++)
            {
                if (EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, catches[i]))
                {
                    catches[i].AddPermanentModifiers(0, valueChange, effect, null);
                }
            }

            effectResolver.ResolveCatchChain(catches, activeEffectRecords);
            resultSummary = $"{matchingCount} interacting catches gained {valueChange} Value each";
            return true;
        }

        int targetIndex = FindTechniqueTargetIndex(effect);
        CardInstance target = catches[targetIndex];
        TechniqueTargetSelected?.Invoke(target);
        int weightChange = 0;
        int valueChangeForTarget = 0;

        if (effect.EffectType == CardEffectType.ModifyCatchValue)
        {
            valueChangeForTarget = effect.Amount;
        }
        else if (effect.EffectType == CardEffectType.RemoveLineLoadModifier)
        {
            weightChange = -Math.Abs(effect.Amount);
        }
        else
        {
            weightChange = effect.Amount;
        }

        target.AddPermanentModifiers(weightChange, valueChangeForTarget, effect, null);
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        resultSummary = $"{target.Definition.DisplayName} changed by Weight {weightChange}, Value {valueChangeForTarget}";
        return true;
    }

    /// <summary>Reverses one lasting negative stat contribution and then reapplies remaining attached effects.</summary>
    public bool TryRemoveAppliedNegativeModifier(CardInstance target, AppliedCatchModifier modifier, EffectResolver resolver)
    {
        if (!IsCatchAttached(target) || !target.TryRemoveAppliedModifier(modifier))
        {
            return false;
        }

        resolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>Removes one active negative effect from its source copy and recalculates the entire chain.</summary>
    public bool TryRemoveNegativeEffect(ActiveCatchEffectRecord record, EffectResolver effectResolver)
    {
        if (record?.Effect == null || record.Effect.EffectTone != CardEffectTone.Negative
            || Array.IndexOf(activeEffectRecords, record) < 0
            || !IsCatchAttached(record.SourceInstance)
            || !record.SourceInstance.TryRemoveEffect(record.Effect))
        {
            return false;
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>Replaces a creature Catch in place without firing Release reactions or transferring its mutations.</summary>
    public CardInstance ReplaceCreatureCatch(
        int instanceId,
        CardDefinition replacement,
        int biomeVisit,
        EffectResolver effectResolver,
        System.Random random)
    {
        int index = Array.FindIndex(catches, c => c != null && c.InstanceId == instanceId);
        if (index < 0 || catches[index].IsBait || catches[index].Definition?.CardType != CardType.Creature
            || replacement == null || replacement.CardType != CardType.Creature)
        {
            return null;
        }

        // A swap removes the old source, but is not a deliberate Release or line-break loss.
        CardInstance next = new CardInstance(nextInstanceId++, replacement, random, CatchAttachmentRole.Catch);
        next.SetAttachmentBiomeVisit(biomeVisit);
        catches[index] = next;
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        ResolveNewAttachment(next, effectResolver);
        return next;
    }

    /// <summary>Changes an attached creature's per-copy base tags and immediately rebuilds all tag-dependent interactions.</summary>
    public bool TrySetCreatureBaseTags(int instanceId, string[] tags, EffectResolver resolver)
    {
        CardInstance target = Array.Find(catches, c => c?.InstanceId == instanceId);
        if (target?.Definition?.CardType != CardType.Creature) return false;
        target.SetRuntimeBaseTags(tags);
        resolver.ResolveCatchChain(catches, activeEffectRecords);
        return true;
    }

    /// <summary>Cancels only a revealed Hazard using the oldest attached Bait with unused protection.</summary>
    public bool TryCancelHazard(CardDefinition encounter)
    {
        foreach (CardInstance card in catches) card?.RefreshHazardProtection();
        if (encounter?.CardType != CardType.Hazard) return false;
        CardInstance oldest = null;
        foreach (CardInstance card in catches)
        {
            if (card != null && card.HasUnusedHazardProtection
                && (oldest == null || card.InstanceId < oldest.InstanceId)) oldest = card;
        }
        return oldest != null && oldest.TryConsumeHazardProtection();
    }

    /// <summary>Applies one lasting bonus per surviving Lobster Catch after a complete revealed Hazard cancellation.</summary>
    public void RecordHazardCancelled(CardDefinition hazard, EffectResolver resolver)
    {
        if (hazard?.CardType != CardType.Hazard) return;
        foreach (CardInstance source in catches)
        {
            if (source == null || source.IsBait) continue;
            CardEffectDefinition effect = Array.Find(source.ActiveEffects, e => e?.EffectType == CardEffectType.GainValueOnHazardCancelled
                && e.Trigger == CardEffectTrigger.AfterHazardCancelled);
            if (effect != null) source.AddPermanentModifiers(0, effect.Amount, effect, source.Definition);
        }
        Recalculate(resolver);
    }

    /// <summary>
    /// Rebuilds selected-role effect records and recalculates catches after role, tag, or lasting-stat changes.
    /// </summary>
    public void Recalculate(EffectResolver effectResolver)
    {
        foreach (CardInstance card in catches) card?.RefreshHazardProtection();
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
    }

    /// <summary>
    /// Removes one catch, records why it left the line, and reports its previous Line Load.
    /// </summary>
    public bool TryRelease(
        int catchIndex,
        EffectResolver effectResolver,
        out CardInstance releasedCatch,
        out int previousLineLoad,
        out string validationMessage,
        CatchRemovalReason removalReason = CatchRemovalReason.PlayerChoice)
    {
        releasedCatch = null;
        previousLineLoad = CurrentLineLoad;
        validationMessage = string.Empty;

        if (catchIndex < 0 || catchIndex >= catches.Length)
        {
            validationMessage = $"Catch Chain index is out of range: {catchIndex}.";
            return false;
        }

        releasedCatch = catches[catchIndex];

        if (releasedCatch == null)
        {
            validationMessage = $"Catch Chain slot {catchIndex} is empty.";
            return false;
        }

        if (removalReason != CatchRemovalReason.LineStrain
            && releasedCatch.PreventsReleaseWhileAttached)
        {
            validationMessage = $"{releasedCatch.Definition.DisplayName} cannot be released once caught.";
            releasedCatch = null;
            return false;
        }

        ApplyAnotherCatchReleasedEffects(releasedCatch, removalReason);
        RecordRemoval(releasedCatch, removalReason);
        catches = RemoveCatchAt(catches, catchIndex);
        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        if (removalReason != CatchRemovalReason.LineStrain)
        {
            DeliberatelyReleased?.Invoke(releasedCatch, removalReason);
        }
        return true;
    }

    /// <summary>Validates every selected release before mutation, removes them together, then resolves survivor Release reactions.</summary>
    public bool TryReleaseTogether(int[] instanceIds, EffectResolver resolver)
    {
        if (instanceIds == null || instanceIds.Length == 0) return false;
        List<CardInstance> selected = new List<CardInstance>();
        foreach (int id in instanceIds)
        {
            CardInstance card = Array.Find(catches, c => c?.InstanceId == id);
            if (card == null || card.PreventsReleaseWhileAttached || selected.Contains(card)) return false;
            selected.Add(card);
        }
        CardInstance[] snapshots = selected.ConvertAll(c => c.CreateSnapshot()).ToArray();
        catches = Array.FindAll(catches, c => !selected.Contains(c));
        Recalculate(resolver);
        foreach (CardInstance released in snapshots)
        {
            ApplyAnotherCatchReleasedEffects(released, CatchRemovalReason.Creature);
            RecordRemoval(released, CatchRemovalReason.Creature);
            Recalculate(resolver);
            DeliberatelyReleased?.Invoke(released, CatchRemovalReason.Creature);
        }
        return true;
    }

    /// <summary>Checks the existing creature/role boundary without treating a Release lock as a mode lock.</summary>
    public bool CanChangeCreatureMode(CardInstance card)
    {
        return card != null && IsCatchAttached(card) && card.Definition?.CardType == CardType.Creature;
    }

    /// <summary>Switches a validated Catch/Bait creature pair atomically, preserving IDs, positions, and resolved modifiers.</summary>
    public bool TrySwapCreatureModes(int catchId, int baitId, EffectResolver resolver)
    {
        CardInstance catchCard = Array.Find(catches, c => c?.InstanceId == catchId);
        CardInstance baitCard = Array.Find(catches, c => c?.InstanceId == baitId);
        if (!CanChangeCreatureMode(catchCard) || !CanChangeCreatureMode(baitCard)
            || catchCard == baitCard || catchCard.IsBait || !baitCard.IsBait) return false;
        MigrateLegacyEffectKeys(catchCard);
        MigrateLegacyEffectKeys(baitCard);
        catchCard.SetAttachmentRole(CatchAttachmentRole.Bait);
        baitCard.SetAttachmentRole(CatchAttachmentRole.Catch);
        Recalculate(resolver);
        return true;
    }

    /// <summary>Runs the new role's own immediate chain effects without consuming another card's next-attachment bonuses.</summary>
    public void ActivateRoleOnAttach(CardInstance instance, EffectResolver resolver)
    {
        if (!IsCatchAttached(instance)) return;
        foreach (CardEffectDefinition effect in instance.ActiveEffects)
        {
            if (effect?.Trigger == CardEffectTrigger.WhenCaught && effect.EffectType == CardEffectType.QueueNextCreatureCatchWeight)
                pendingCreatureCatchEffects.Add(new PendingCreatureCatchEffect(effect, instance.Definition));
        }
        Recalculate(resolver);
    }

    /// <summary>Associates legacy lifecycle keys with their pre-swap role so another role's index cannot inherit them.</summary>
    private void MigrateLegacyEffectKeys(CardInstance source)
    {
        foreach (string[] keys in new[] { consumedNextCatchEffectKeys, consumedNextEncounterEffectKeys, invalidatedNoReleaseEffectKeys })
        {
            for (int i = 0; i < keys.Length; i++)
            {
                string[] parts = keys[i].Split(':');
                if (parts.Length == 2 && parts[0] == source.InstanceId.ToString())
                    keys[i] = parts[0] + ":" + source.AttachmentRole + ":" + parts[1];
            }
        }
    }

    /// <summary>
    /// Randomly loses between one catch and half of the attached chain after an overload line break.
    /// </summary>
    public CardInstance[] LoseRandomCatchesAfterLineBreak(
        EffectResolver effectResolver,
        System.Random random)
    {
        if (catches.Length == 0 || random == null)
        {
            return Array.Empty<CardInstance>();
        }

        int maximumLossCount = Mathf.Max(1, catches.Length / 2);
        int lossCount = random.Next(1, maximumLossCount + 1);
        CardInstance[] losses = new CardInstance[lossCount];

        for (int i = 0; i < lossCount; i++)
        {
            int lossIndex = random.Next(catches.Length);
            CardInstance lostCatch = catches[lossIndex];
            losses[i] = lostCatch;
            RecordRemoval(lostCatch, CatchRemovalReason.LineStrain);
            catches = RemoveCatchAt(catches, lossIndex);
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        return losses;
    }

    /// <summary>
    /// Returns a separate snapshot of all catches currently attached to the line.
    /// </summary>
    public CardInstance[] CreateSnapshot()
    {
        CardInstance[] result = new CardInstance[catches.Length];

        for (int i = 0; i < catches.Length; i++)
        {
            result[i] = catches[i]?.CreateSnapshot();
        }

        return result;
    }

    /// <summary>
    /// Resolves a temporary catch copy against the current chain without mutating the active run.
    /// </summary>
    public CardInstance CreateResolvedCatchPreview(
        CardDefinition card,
        EffectResolver effectResolver,
        CatchAttachmentRole attachmentRole = CatchAttachmentRole.Catch)
    {
        if (card == null || effectResolver == null)
        {
            return null;
        }

        CardInstance[] previewCatches = CreateSnapshot();
        CardInstance previewCatch = new CardInstance(0, card, null, attachmentRole);
        previewCatches = AppendCatch(previewCatches, previewCatch);

        List<ActiveCatchEffectRecord> previewEffects = new List<ActiveCatchEffectRecord>();
        foreach (ActiveCatchEffectRecord record in activeEffectRecords)
        {
            CardInstance sourceCopy = Array.Find(previewCatches, c => c?.InstanceId == record.SourceInstance?.InstanceId);
            previewEffects.Add(new ActiveCatchEffectRecord(sourceCopy, record.Effect, record.ActiveTrigger, record.SourceCatchIndex));
        }
        AppendPreviewEffects(previewEffects, previewCatch, previewCatches.Length - 1);
        effectResolver.ResolveCatchChain(previewCatches, previewEffects.ToArray());
        ApplyPendingNextCatchEffects(previewCatch, false);
        ApplyPendingCreatureCatchEffects(previewCatch, false);
        effectResolver.ResolveCatchChain(previewCatches, previewEffects.ToArray());
        return previewCatch;
    }

    /// <summary>Consumes attached effects that last through exactly one encounter resolution.</summary>
    public void CompleteEncounter(EffectResolver effectResolver)
    {
        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            if (record?.Effect == null
                || record.SourceInstance == null
                || record.ActiveTrigger != CardEffectTrigger.UntilNextEncounterResolved)
            {
                continue;
            }

            int effectIndex = Array.IndexOf(record.SourceInstance.ActiveEffects, record.Effect);
            if (effectIndex >= 0)
            {
                consumedNextEncounterEffectKeys = AppendEffectKey(
                    consumedNextEncounterEffectKeys,
                    BuildEffectKey(record.SourceInstance, effectIndex));
            }
        }

        RebuildActiveEffectRecords();
        effectResolver?.ResolveCatchChain(catches, activeEffectRecords);
    }

    /// <summary>
    /// Clears all Catch Chain cards and active effect records.
    /// </summary>
    public void Reset()
    {
        catches = Array.Empty<CardInstance>();
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();
        releasedCatches = Array.Empty<CardInstance>();
        lostCatches = Array.Empty<CardInstance>();
        consumedNextCatchEffectKeys = Array.Empty<string>();
        consumedNextEncounterEffectKeys = Array.Empty<string>();
        invalidatedNoReleaseEffectKeys = Array.Empty<string>();
        pendingCreatureCatchEffects.Clear();
        nextInstanceId = 1;
    }

    /// <summary>
    /// Tracks effects that become relevant when a card enters the Catch Chain.
    /// </summary>
    private void AddCatchEffects(CardInstance caughtInstance, int catchIndex)
    {
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.AfterAnotherCatchAttached, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.OnLaterBiomeStart, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttached, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenAnotherCatchReleased, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhenNextMatchingCatchCaught, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.UntilNextEncounterResolved, catchIndex);
        AddActiveEffects(caughtInstance, CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased, catchIndex);
    }

    /// <summary>
    /// Adds the temporary catch's own catch-time and attached effects to a preview effect list.
    /// </summary>
    private static void AppendPreviewEffects(
        List<ActiveCatchEffectRecord> previewEffects,
        CardInstance previewCatch,
        int catchIndex)
    {
        CardEffectDefinition[] effects = previewCatch?.ActiveEffects;

        if (effects == null)
        {
            return;
        }

        for (int i = 0; i < effects.Length; i++)
        {
            CardEffectDefinition effect = effects[i];

            if (effect == null
                || (effect.Trigger != CardEffectTrigger.WhenCaught
                    && effect.Trigger != CardEffectTrigger.WhileAttached
                    && effect.Trigger != CardEffectTrigger.WhenAnotherCatchReleased
                    && effect.Trigger != CardEffectTrigger.UntilNextEncounterResolved
                    && effect.Trigger != CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased))
            {
                continue;
            }

            previewEffects.Add(new ActiveCatchEffectRecord(previewCatch, effect, effect.Trigger, catchIndex));
        }
    }

    /// <summary>
    /// Rebuilds active effect records from catches that remain attached.
    /// </summary>
    private void RebuildActiveEffectRecords()
    {
        activeEffectRecords = Array.Empty<ActiveCatchEffectRecord>();

        // Rebuilding also preserves the correct number of records for repeated card definitions.
        for (int i = 0; i < catches.Length; i++)
        {
            AddCatchEffects(catches[i], i);
        }
    }

    /// <summary>
    /// Adds catch effects matching one active trigger.
    /// </summary>
    private void AddActiveEffects(CardInstance sourceInstance, CardEffectTrigger trigger, int catchIndex)
    {
        CardEffectDefinition[] effects = sourceInstance?.ActiveEffects;

        if (effects == null)
        {
            return;
        }

        List<ActiveCatchEffectRecord> records = new List<ActiveCatchEffectRecord>(activeEffectRecords);

        for (int i = 0; i < effects.Length; i++)
        {
            CardEffectDefinition effect = effects[i];

            if (effect == null || effect.Trigger != trigger
                || (trigger == CardEffectTrigger.WhenCaught
                    && (effect.EffectType == CardEffectType.RemoveNegativeEffect
                        || effect.EffectType == CardEffectType.ReleaseCatch
                        || effect.EffectType == CardEffectType.QueueNextCreatureCatchWeight
                        || effect.EffectType == CardEffectType.ChooseNextSchoolingEncounter
                        || effect.EffectType == CardEffectType.CancelNextHazard
                        || effect.EffectType == CardEffectType.DiscardTechniqueOnAttach
                        || effect.EffectType == CardEffectType.ReleaseSelectedCreaturesOnAttach
                        || effect.EffectType == CardEffectType.GrantSelectedTechniqueRecovery
                        || effect.EffectType == CardEffectType.DrawTechniqueOnAttach
                        || effect.EffectType == CardEffectType.SwapCreatureModesOnAttach
                        || effect.EffectType == CardEffectType.QueueCreatureEncounterConcealment
                        || effect.EffectType == CardEffectType.GrantNextCreatureSkip
                        || effect.EffectType == CardEffectType.ReleaseMatchingBaitOnAttach)))
            {
                continue;
            }

            if (trigger == CardEffectTrigger.WhenNextMatchingCatchCaught
                && IsNextCatchEffectConsumed(sourceInstance, i))
            {
                continue;
            }

            if (trigger == CardEffectTrigger.UntilNextEncounterResolved
                && IsEffectKeyTracked(consumedNextEncounterEffectKeys, sourceInstance, i))
            {
                continue;
            }

            if (trigger == CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased
                && IsEffectKeyTracked(invalidatedNoReleaseEffectKeys, sourceInstance, i))
            {
                continue;
            }

            records.Add(new ActiveCatchEffectRecord(sourceInstance, effect, trigger, catchIndex));
        }

        activeEffectRecords = records.ToArray();
    }

    /// <summary>Applies old queued attachment effects after tags resolve, then records this card's independent one-shot effects.</summary>
    private void ResolveNewAttachment(CardInstance instance, EffectResolver resolver)
    {
        ApplyPendingNextCatchEffects(instance, true);
        ApplyPendingCreatureCatchEffects(instance, true);
        foreach (CardEffectDefinition effect in instance.ActiveEffects)
        {
            if (effect?.Trigger == CardEffectTrigger.WhenCaught
                && effect.EffectType == CardEffectType.QueueNextCreatureCatchWeight)
            {
                pendingCreatureCatchEffects.Add(new PendingCreatureCatchEffect(effect, instance.Definition));
            }
        }
        RebuildActiveEffectRecords();
        resolver.ResolveCatchChain(catches, activeEffectRecords);
    }

    /// <summary>Consumes independent pending penalties only on creature Catch attachments; previews leave the queue intact.</summary>
    private void ApplyPendingCreatureCatchEffects(CardInstance instance, bool consume)
    {
        if (instance.IsBait || instance.Definition?.CardType != CardType.Creature) return;
        foreach (PendingCreatureCatchEffect pending in pendingCreatureCatchEffects)
        {
            instance.AddPermanentModifiers(pending.Effect.Amount, 0, pending.Effect, pending.SourceCard);
        }
        if (consume) pendingCreatureCatchEffects.Clear();
    }

    /// <summary>Removes an exact pending negative occurrence even after its original source left the chain.</summary>
    public bool TryRemovePendingCreatureCatchEffect(PendingCreatureCatchEffect pending)
    {
        return pending?.Effect?.EffectTone == CardEffectTone.Negative && pendingCreatureCatchEffects.Remove(pending);
    }

    /// <summary>Applies each attached source's unused one-shot bonus to the next matching catch.</summary>
    private void ApplyPendingNextCatchEffects(CardInstance newCatch, bool consumeEffects)
    {
        if (newCatch?.Definition == null)
        {
            return;
        }

        for (int catchIndex = 0; catchIndex < catches.Length; catchIndex++)
        {
            CardInstance source = catches[catchIndex];
            CardEffectDefinition[] effects = source?.ActiveEffects;
            if (source == newCatch || effects == null)
            {
                continue;
            }

            for (int effectIndex = 0; effectIndex < effects.Length; effectIndex++)
            {
                CardEffectDefinition effect = effects[effectIndex];
                if (effect == null
                    || effect.Trigger != CardEffectTrigger.WhenNextMatchingCatchCaught
                    || effect.Target != CardEffectTarget.SpecificCaughtCard
                    || IsNextCatchEffectConsumed(source, effectIndex)
                    || !EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, newCatch))
                {
                    continue;
                }

                int weightChange = 0;
                int valueChange = 0;
                if (effect.EffectType == CardEffectType.ModifyCatchValue)
                {
                    valueChange = effect.Amount;
                }
                else if (effect.EffectType == CardEffectType.AddLineLoadModifier)
                {
                    weightChange = effect.Amount;
                }
                else if (effect.EffectType == CardEffectType.RemoveLineLoadModifier)
                {
                    weightChange = -Math.Abs(effect.Amount);
                }
                else
                {
                    continue;
                }

                newCatch.AddPermanentModifiers(weightChange, valueChange, effect, source.Definition);
                if (consumeEffects)
                {
                    consumedNextCatchEffectKeys = AppendEffectKey(
                        consumedNextCatchEffectKeys,
                        BuildEffectKey(source, effectIndex));
                }
            }
        }
    }

    /// <summary>Reports whether one source instance already spent its future-catch effect.</summary>
    private bool IsNextCatchEffectConsumed(CardInstance source, int effectIndex)
    {
        return IsEffectKeyTracked(consumedNextCatchEffectKeys, source, effectIndex);
    }

    /// <summary>Reports whether a per-instance effect key exists in one lifecycle-state collection.</summary>
    private static bool IsEffectKeyTracked(string[] keys, CardInstance source, int effectIndex)
    {
        return Array.IndexOf(keys ?? Array.Empty<string>(), BuildEffectKey(source, effectIndex)) >= 0
            || Array.IndexOf(keys ?? Array.Empty<string>(), $"{source?.InstanceId ?? 0}:{effectIndex}") >= 0;
    }

    /// <summary>Builds a stable per-run key for one effect on one caught card instance.</summary>
    private static string BuildEffectKey(CardInstance source, int effectIndex)
    {
        return $"{source?.InstanceId ?? 0}:{source?.AttachmentRole}:{effectIndex}";
    }

    private static string[] AppendEffectKey(string[] source, string key)
    {
        string[] safeSource = source ?? Array.Empty<string>();
        string[] result = new string[safeSource.Length + 1];
        Array.Copy(safeSource, result, safeSource.Length);
        result[result.Length - 1] = key;
        return result;
    }

    /// <summary>
    /// Removes the requested number of automatically selected catches for a Technique effect.
    /// </summary>
    private bool ReleaseTechniqueTargets(
        CardEffectDefinition effect,
        EffectResolver effectResolver,
        out string resultSummary)
    {
        List<string> releasedNames = new List<string>();
        int releaseCount = Math.Max(1, Math.Abs(effect.Amount));

        for (int i = 0; i < releaseCount; i++)
        {
            int targetIndex = FindTechniqueTargetIndex(effect);

            if (targetIndex < 0)
            {
                break;
            }

            CardInstance releasedCatch = catches[targetIndex];
            TechniqueTargetSelected?.Invoke(releasedCatch);
            releasedNames.Add(releasedCatch.Definition.DisplayName);
            ApplyAnotherCatchReleasedEffects(releasedCatch, CatchRemovalReason.Technique);
            RecordRemoval(releasedCatch, CatchRemovalReason.Technique);
            catches = RemoveCatchAt(catches, targetIndex);
            RebuildActiveEffectRecords();
            effectResolver.ResolveCatchChain(catches, activeEffectRecords);
            DeliberatelyReleased?.Invoke(releasedCatch, CatchRemovalReason.Technique);
        }

        RebuildActiveEffectRecords();
        effectResolver.ResolveCatchChain(catches, activeEffectRecords);
        resultSummary = $"Released {string.Join(", ", releasedNames)}";
        return releasedNames.Count > 0;
    }

    /// <summary>Applies lasting self-value reactions when a different catch is deliberately released.</summary>
    private void ApplyAnotherCatchReleasedEffects(
        CardInstance releasedCatch,
        CatchRemovalReason removalReason)
    {
        if (removalReason == CatchRemovalReason.LineStrain)
        {
            return;
        }

        InvalidateNoReleaseEffects(releasedCatch);

        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            CardEffectDefinition effect = record?.Effect;
            CardInstance source = record?.SourceInstance;

            if (record == null
                || source == null
                || ReferenceEquals(source, releasedCatch)
                || !IsCatchAttached(source)
                || effect == null
                || record.ActiveTrigger != CardEffectTrigger.WhenAnotherCatchReleased
                || (effect.EffectType != CardEffectType.ModifyCatchValue
                    && effect.EffectType != CardEffectType.LoseValueOnCreatureRelease)
                || effect.Target != CardEffectTarget.Self)
            {
                continue;
            }

            if (effect.EffectType == CardEffectType.LoseValueOnCreatureRelease)
            {
                if (source.IsBait || releasedCatch.Definition?.CardType != CardType.Creature) continue;
                // Store only the loss actually taken; reaching zero must not create hidden value debt.
                int loss = Math.Min(source.CurrentValue, Math.Abs(effect.Amount));
                source.AddPermanentModifiers(0, -loss, effect, source.Definition);
            }
            else
            {
                source.AddPermanentModifiers(0, effect.Amount, effect, source.Definition);
            }
        }
    }

    /// <summary>Forfeits conditional Value effects when another catch is deliberately released later.</summary>
    private void InvalidateNoReleaseEffects(CardInstance releasedCatch)
    {
        for (int i = 0; i < activeEffectRecords.Length; i++)
        {
            ActiveCatchEffectRecord record = activeEffectRecords[i];
            CardInstance source = record?.SourceInstance;
            if (record?.Effect == null
                || source == null
                || ReferenceEquals(source, releasedCatch)
                || !IsCatchAttached(source)
                || record.ActiveTrigger != CardEffectTrigger.WhileAttachedUntilAnotherCatchReleased)
            {
                continue;
            }

            int effectIndex = Array.IndexOf(source.ActiveEffects, record.Effect);
            if (effectIndex >= 0)
            {
                invalidatedNoReleaseEffectKeys = AppendEffectKey(
                    invalidatedNoReleaseEffectKeys,
                    BuildEffectKey(source, effectIndex));
            }
        }
    }

    /// <summary>Checks whether an effect source still belongs to the current Catch Chain.</summary>
    private bool IsCatchAttached(CardInstance candidate)
    {
        for (int i = 0; i < catches.Length; i++)
        {
            if (ReferenceEquals(catches[i], candidate))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds a removed catch snapshot to either the released or involuntarily lost history.
    /// </summary>
    private void RecordRemoval(CardInstance removedCatch, CatchRemovalReason removalReason)
    {
        if (removedCatch == null)
        {
            return;
        }

        CardInstance snapshot = removedCatch.CreateSnapshot();

        if (removalReason == CatchRemovalReason.LineStrain)
        {
            lostCatches = AppendCatch(lostCatches, snapshot);
            return;
        }

        releasedCatches = AppendCatch(releasedCatches, snapshot);
    }

    /// <summary>
    /// Selects one Catch Chain target using the effect's automatic target direction.
    /// </summary>
    private int FindTechniqueTargetIndex(CardEffectDefinition effect)
    {
        bool searchFromStart = effect.CaughtCardTargetMode == CaughtCardTargetMode.FirstMatching
            || effect.CaughtCardTargetMode == CaughtCardTargetMode.NextMatching;

        if (searchFromStart)
        {
            for (int i = 0; i < catches.Length; i++)
            {
                if (EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, catches[i])
                    && IsValidTechniqueTarget(effect, catches[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        for (int i = catches.Length - 1; i >= 0; i--)
        {
            if (EffectResolver.RequiredTagsMatchInstance(effect.RequiredTags, catches[i])
                && IsValidTechniqueTarget(effect, catches[i]))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>Excludes technique-target immune cards and, for deliberate releases, release-locked cards.</summary>
    private static bool IsValidTechniqueTarget(CardEffectDefinition effect, CardInstance candidate)
    {
        return candidate != null && !candidate.PreventsTechniqueTargeting
            && (effect.EffectType != CardEffectType.ReleaseCatch || !candidate.PreventsReleaseWhileAttached);
    }

    /// <summary>
    /// Counts attached catches matching an effect's required tags.
    /// </summary>
    private int CountMatchingCatches(string[] requiredTags)
    {
        int matchingCount = 0;

        for (int i = 0; i < catches.Length; i++)
        {
            if (EffectResolver.RequiredTagsMatchInstance(requiredTags, catches[i]))
            {
                matchingCount++;
            }
        }

        return matchingCount;
    }

    /// <summary>
    /// Returns a new catch-instance array with one catch appended.
    /// </summary>
    private static CardInstance[] AppendCatch(CardInstance[] source, CardInstance caughtInstance)
    {
        CardInstance[] result = new CardInstance[source.Length + 1];

        for (int i = 0; i < source.Length; i++)
        {
            result[i] = source[i];
        }

        result[result.Length - 1] = caughtInstance;
        return result;
    }

    /// <summary>
    /// Returns a new catch-instance array without the catch at the requested index.
    /// </summary>
    private static CardInstance[] RemoveCatchAt(CardInstance[] source, int removeIndex)
    {
        CardInstance[] result = new CardInstance[source.Length - 1];
        int resultIndex = 0;

        for (int i = 0; i < source.Length; i++)
        {
            if (i == removeIndex)
            {
                continue;
            }

            result[resultIndex] = source[i];
            resultIndex++;
        }

        return result;
    }
}

public enum CatchRemovalReason
{
    PlayerChoice,
    Technique,
    LineStrain,
    Creature
}

/// <summary>An independent queued creature Catch penalty whose lifetime does not depend on its source attachment.</summary>
[Serializable]
public sealed class PendingCreatureCatchEffect
{
    [SerializeField] private CardEffectDefinition effect;
    [SerializeField] private CardDefinition sourceCard;
    public CardEffectDefinition Effect => effect;
    public CardDefinition SourceCard => sourceCard;

    /// <summary>Captures the effect and authored origin without retaining an attached source.</summary>
    public PendingCreatureCatchEffect(CardEffectDefinition effect, CardDefinition sourceCard)
    {
        this.effect = effect;
        this.sourceCard = sourceCard;
    }
}
