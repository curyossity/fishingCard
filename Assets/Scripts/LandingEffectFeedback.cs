using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Local feedback for real attempt recovery and accuracy/motion assistance, separate from point projectiles.</summary>
public sealed class LandingEffectFeedback : MonoBehaviour
{
    [SerializeField] private LandingScoreRush rush;
    [SerializeField] private UnityEngine.UI.Image goodEdge, perfectEdge, slowPortrait, recoveryPortrait;
    [SerializeField] private TMP_Text slowCaption, recoveryCaption;
    [SerializeField] private TMP_Text behavior, persistentRecovery;
    [SerializeField] private CanvasGroup recovery;
    private LandingChallengeRuntime model;
    private IReadOnlyList<UnityEngine.UI.Image> lamps = Array.Empty<UnityEngine.UI.Image>();
    private LandingStrikeReceipt shown;
    private readonly List<LandingContribution> grants = new List<LandingContribution>();
    private double entered, recovered;
    private readonly Color assist = new Color(.62f, .92f, .85f);
    public bool SlowVisible => slowPortrait != null && slowPortrait.enabled;
    public bool RecoveryVisible => recovery != null && recovery.gameObject.activeSelf;

    /// <summary>Captures assistance identity and the rendered attempt lamps without changing their authoritative count.</summary>
    public void Bind(LandingChallengeRuntime landing, CardInstance[] members, IReadOnlyList<UnityEngine.UI.Image> attemptLamps)
    {
        Clear(); model = landing; lamps = attemptLamps; entered = Time.realtimeSinceStartupAsDouble;
        decimal greatest = -1; int source = -1;
        foreach (var member in landing.Rig)
            foreach (var rule in member.Rules)
                if (rule.Ability?.Kind == RigAbilityKind.FishSlowInGood && rule.Amount > greatest) { greatest = rule.Amount; source = member.Id; }
        slowPortrait.sprite = members.FirstOrDefault(m => m.InstanceId == source)?.Definition.Artwork;
    }

    /// <summary>Shows only actual attempt grants; score multipliers never masquerade as recovered attempts.</summary>
    public void Present(LandingStrikeReceipt receipt)
    {
        if (ReferenceEquals(shown, receipt)) return;
        shown = receipt; recovered = Time.realtimeSinceStartupAsDouble; grants.Clear();
        if (receipt?.Score != null) grants.AddRange(receipt.Score.Contributions.Where(c => c.Attempts > 0));
    }

    /// <summary>Renders assistance at the affected track and relights only the lamps corresponding to genuine grants.</summary>
    private void LateUpdate()
    {
        if (model == null) return;
        bool reduced = rush != null && rush.ReducedMotion;
        float entry = (float)(Time.realtimeSinceStartupAsDouble - entered);
        float pulse = reduced ? 0 : Mathf.Sin(Mathf.Clamp01(entry / .7f) * Mathf.PI) * .3f;
        goodEdge.color = new Color(assist.r, assist.g, assist.b, model.GoodZoneGrowth > 0 ? .18f + pulse : 0);
        perfectEdge.color = new Color(1, .88f, .51f, model.PerfectZoneGrowth > 0 ? .22f + pulse : 0);
        bool slowed = model.State == LandingChallengeState.Active && model.FishSlowFraction > 0 && Math.Abs(model.FishPosition - model.ZonePosition) <= (model.FishWidth + model.GoodWidth) * .5;
        bool showSlowText = slowed && !model.IsTelegraphingBurst;
        slowCaption.gameObject.SetActive(showSlowText); behavior.enabled = !showSlowText;
        slowPortrait.enabled = slowed && slowPortrait.sprite != null;
        if (showSlowText) slowCaption.text = (model.FishSlowFraction * 100).ToString("0.#", CultureInfo.InvariantCulture) + "% SLOWER";
        float age = (float)(Time.realtimeSinceStartupAsDouble - recovered);
        bool recovering = grants.Count > 0 && age >= 0 && age < .8f;
        recovery.gameObject.SetActive(recovering);
        persistentRecovery.enabled = !recovering;
        int total = grants.Sum(c => c.Attempts);
        for (int i = 0; i < lamps.Count; i++)
        {
            bool relight = recovering && i < model.AttemptsRemaining && i >= model.AttemptsRemaining - total;
            lamps[i].color = relight ? Color.Lerp(Color.white, assist, .65f) : Color.white;
            float bounce = relight && !reduced ? Mathf.Sin(Mathf.Clamp01(age / .65f) * Mathf.PI) * .14f : 0;
            lamps[i].rectTransform.localScale = Vector3.one * (1 + bounce);
        }
        if (!recovering) return;
        var grant = grants[Math.Min(grants.Count - 1, (int)(age / .8f * grants.Count))];
        var source = shown.Members.FirstOrDefault(m => m.InstanceId == grant.SourceId);
        recoveryPortrait.sprite = source?.Definition.Artwork; recoveryPortrait.enabled = recoveryPortrait.sprite != null;
        recoveryCaption.text = "+" + grant.Attempts + " ATTEMPT · " + (source?.Definition.DisplayName ?? "RIG").ToUpperInvariant();
        recovery.alpha = reduced ? 1 : Mathf.Clamp01((.8f - age) / .18f);
    }
    /// <summary>Releases all presentation-only lamp transforms and assistance layers at encounter/phase boundaries.</summary>
    private void OnDisable() => Clear();
    /// <summary>Ends a skipped/completed strike's recovery pulse while keeping ongoing track assistance available.</summary>
    public void FinishStrike()
    {
        grants.Clear();
        if (recovery != null) recovery.gameObject.SetActive(false);
        if (persistentRecovery != null) persistentRecovery.enabled = true;
        foreach (var lamp in lamps) if (lamp != null) { lamp.color = Color.white; lamp.rectTransform.localScale = Vector3.one; }
    }
    /// <summary>Clears owned visuals without altering attempts, timing, zone widths or fish velocity.</summary>
    private void Clear()
    {
        FinishStrike();
        if (slowCaption != null) slowCaption.gameObject.SetActive(false);
        if (slowPortrait != null) slowPortrait.enabled = false;
        if (behavior != null) behavior.enabled = true;
        if (goodEdge != null) goodEdge.color = Color.clear;
        if (perfectEdge != null) perfectEdge.color = Color.clear;
        model = null; shown = null; grants.Clear();
    }
}
