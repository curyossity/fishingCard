using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>Projects resolved strike receipts into persistent portrait attribution and bounded source pulses.</summary>
public sealed class LandingStrikeFeedback : MonoBehaviour
{
    [SerializeField] private RectTransform rows, chips, assistance, pulseLayer;
    [SerializeField] private LandingContributionView rowPrefab, chipPrefab;
    [SerializeField] private LandingActivationView activationPrefab;
    [SerializeField] private TMP_Text equation, assistanceTotals, attemptFeedback;
    [SerializeField] private TMP_Text attemptCount;
    [SerializeField] private UnityEngine.UI.ScrollRect rowScroll, chipScroll;
    [SerializeField] private bool compactSummary;
    [SerializeField, Min(.01f)] private float pulseSeconds = .3f, maximumStagger = .12f;
    private readonly List<LandingContributionView> rowViews = new List<LandingContributionView>(), chipViews = new List<LandingContributionView>(), assistViews = new List<LandingContributionView>();
    private readonly List<LandingActivationView> activations = new List<LandingActivationView>();
    private readonly List<HitItem> items = new List<HitItem>();
    private readonly Dictionary<int, string> badges = new Dictionary<int, string>();
    private readonly HashSet<int> attemptSources = new HashSet<int>();
    private IReadOnlyList<TackleRigCardView> cards = Array.Empty<TackleRigCardView>();
    private LandingStrikeReceipt shown;
    private double started;
    private bool animate;
    private bool sequencedScore;
    private Vector2 attemptRest;
    private int inspected = -1;
    private int scoreSource = -1;
    private float scorePulse;
    private readonly HashSet<int> supporting = new HashSet<int>();
    private readonly HashSet<int> scoreSources = new HashSet<int>();
    private bool scoringFocus;
    public LandingStrikeReceipt Shown => shown;
    public int ContributorCount => chipViews.Count(v => v.gameObject.activeSelf);
    private sealed class HitItem
    {
        public CardInstance Member;
        public string Title, Amount, Reason, Context;
        public int[] Support;
    }
    /// <summary>Retains the authored centered attempt position when no recovery message is needed.</summary>
    private void Awake() { if (attemptCount != null) attemptRest = attemptCount.rectTransform.anchoredPosition; }

    /// <summary>Keeps camera-backed captures and resized canvases aligned with the source cards.</summary>
    private void OnEnable() { Canvas.willRenderCanvases += BeforeRender; }
    /// <summary>Updates only active presentation envelopes before the canvas renders.</summary>
    private void BeforeRender() { if (animate || inspected >= 0 || scoreSource >= 0) RenderPulses(); }

    /// <summary>Clears transient state when leaving a phase; the controller retains the immutable receipt.</summary>
    private void OnDisable() { Canvas.willRenderCanvases -= BeforeRender; shown = null; inspected = -1; supporting.Clear(); Clear(); HideUnused(assistViews, 0); }

    /// <summary>Shows ongoing assistance once per encounter, separately from the hit's score sources.</summary>
    public void BindAssistance(LandingChallengeRuntime model, CardInstance[] members)
    {
        if (assistance == null) return;
        int count = 0;
        foreach (var member in model.Rig)
            foreach (var rule in member.Rules)
            {
                var kind = rule.Ability?.Kind;
                if (kind != RigAbilityKind.GoodWidth && kind != RigAbilityKind.PerfectWidth && kind != RigAbilityKind.FishSlowInGood
                    && !(kind == RigAbilityKind.ExtraAttempts && rule.Ability.Trigger == RigAbilityTrigger.Continuous) && kind != RigAbilityKind.MissProtection) continue;
                string label = kind == RigAbilityKind.GoodWidth ? "GOOD" : kind == RigAbilityKind.PerfectWidth ? "PERFECT" : kind == RigAbilityKind.FishSlowInGood ? "SLOW" : kind == RigAbilityKind.ExtraAttempts ? "ATTEMPTS" : "PROTECT";
                var card = members.FirstOrDefault(m => m.InstanceId == member.Id);
                var view = Pool(assistViews, chipPrefab, assistance, count++);
                view.Bind(this, card, "ONGOING ASSISTANCE", label, rule.Explanation,
                    "Ongoing assistance is separate from strike points. Shared assistance limits still apply.", rule.SupportingIds.ToArray());
                view.Anchor.anchoredPosition = new Vector2((count - 1) * 128, 0);
            }
        HideUnused(assistViews, count); assistance.sizeDelta = new Vector2(Mathf.Max(1, count * 128), 28);
        assistanceTotals.text = string.Join("  ", new[] {
            model.GoodZoneGrowth > .00001 ? "GOOD +" + Percent(model.GoodZoneGrowth) + "%" : "",
            model.PerfectZoneGrowth > .00001 ? "PERFECT +" + Percent(model.PerfectZoneGrowth) + "%" : "",
            model.FishSlowFraction > 0 ? "IN GOOD: " + Percent(model.FishSlowFraction) + "% SLOWER" : "" }.Where(s => s.Length > 0));
    }

    /// <summary>Publishes a receipt once; repeated view refreshes cannot replay activation or change attribution.</summary>
    public void Present(LandingStrikeReceipt receipt, IReadOnlyList<TackleRigCardView> rigCards, bool playPulses, bool scoreSequence = false)
    {
        cards = rigCards ?? Array.Empty<TackleRigCardView>();
        while (activations.Count < cards.Count)
        {
            var activation = Instantiate(activationPrefab, pulseLayer); activation.gameObject.SetActive(false); activations.Add(activation);
        }
        for (int i = 0; i < cards.Count; i++) activations[i].Bind(cards[i]);
        for (int i = cards.Count; i < activations.Count; i++) activations[i].gameObject.SetActive(false);
        if (ReferenceEquals(shown, receipt)) return;
        shown = receipt; started = Time.realtimeSinceStartupAsDouble; animate = playPulses; inspected = -1; supporting.Clear();
        sequencedScore = scoreSequence;
        items.Clear(); badges.Clear(); attemptSources.Clear();
        if (receipt?.Score == null) { Clear(); return; }
        var score = receipt.Score;
        foreach (var contribution in score.Contributions.Where(c => c.Bonus != 0)) Add(contribution, "+" + Number(contribution.Bonus) + " points");
        foreach (var contribution in score.Contributions.Where(c => c.Multiplier != 1)) Add(contribution, "×" + Number(contribution.Multiplier));
        foreach (var contribution in score.Contributions.Where(c => c.Attempts > 0))
        {
            attemptSources.Add(contribution.SourceId);
            string recovery = "+" + contribution.Attempts + " attempt";
            badges[contribution.SourceId] = badges.TryGetValue(contribution.SourceId, out var existing) ? existing + " / " + recovery : recovery;
        }
        equation.text = compactSummary ? "LAST HIT +" + LandingScoreRush.Format(score.Points) : score.Accuracy.ToString().ToUpperInvariant() + " +" + LandingScoreRush.Format(score.Points);
        int row = 0;
        if (rows != null)
        {
            BindRow(Pool(rowViews, rowPrefab, rows, row++), null, "Accuracy base", Number(score.Base), "The " + score.Accuracy + " accuracy base for this strike.", "", null);
            foreach (var item in items) BindRow(Pool(rowViews, rowPrefab, rows, row++), item.Member, item.Title, item.Amount, item.Reason, item.Context, item.Support);
            BindRow(Pool(rowViews, rowPrefab, rows, row++), null, "TOTAL", LandingScoreRush.Format(score.Points) + " points", "(" + Number(score.Base) + " + " + Number(score.Bonuses) + ") × " + Number(score.Multiplier) + " = " + Number(score.Points), "Accuracy base plus flat bonuses, then the product of multipliers.", null);
            foreach (var recovery in score.Contributions.Where(c => c.Attempts > 0))
                BindRow(Pool(rowViews, rowPrefab, rows, row++), shown.Members.FirstOrDefault(m => m.InstanceId == recovery.SourceId), recovery.Label,
                    "+" + recovery.Attempts + " attempt", recovery.Explanation, "Recovery is separate from Landing Points.", recovery.SupportingIds.ToArray());
            HideUnused(rowViews, row); rows.sizeDelta = new Vector2(rows.sizeDelta.x, row * 32);
            if (rowScroll != null) rowScroll.verticalNormalizedPosition = 1;
        }
        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i]; var view = Pool(chipViews, chipPrefab, chips, i);
            view.Bind(this, item.Member, item.Title, item.Amount, item.Reason, item.Context, item.Support);
            view.Anchor.anchoredPosition = new Vector2(i * 132, 0);
        }
        HideUnused(chipViews, items.Count); chips.sizeDelta = new Vector2(Mathf.Max(1, items.Count * 132), 34);
        if (chipScroll != null) chipScroll.horizontalNormalizedPosition = 0;
        attemptFeedback.text = string.Join(" / ", score.Contributions.Where(c => c.Attempts > 0).Select(c => "+" + c.Attempts + " attempt · " + Name(c.SourceId)))
            + (receipt.ProtectedMiss ? "  MISS PROTECTED" : "");
        if (attemptCount != null) attemptCount.rectTransform.anchoredPosition = attemptRest + (attemptFeedback.text.Length > 0 ? Vector2.up * 8 : Vector2.zero);
    }

    /// <summary>Stores the frozen source reason and enablers beside each exact score operation.</summary>
    private void Add(LandingContribution contribution, string amount)
    {
        var member = shown.Members.FirstOrDefault(m => m.InstanceId == contribution.SourceId);
        items.Add(new HitItem { Member = member, Title = contribution.Label, Amount = amount, Reason = contribution.Explanation,
            Context = contribution.SupportingIds.Count > 0 ? "Enabled by: " + string.Join(", ", contribution.SupportingIds.Take(3).Select(Name))
                + (contribution.SupportingIds.Count > 3 ? " and " + (contribution.SupportingIds.Count - 3) + " more. Supporting cards are softly outlined." : ".") : "Applied to this strike.", Support = contribution.SupportingIds.ToArray() });
        badges[contribution.SourceId] = badges.TryGetValue(contribution.SourceId, out var old) ? old + "  " + amount : amount;
    }
    /// <summary>Resolves display identity from the receipt even after the live rig has been cleared.</summary>
    private string Name(int id) => shown.Members.FirstOrDefault(m => m.InstanceId == id)?.Definition.DisplayName ?? "Creature";
    /// <summary>Highlights an inspected source strongly and its positional enablers softly.</summary>
    public void Inspect(int sourceId, int[] support)
    { inspected = sourceId; supporting.Clear(); if (support != null) foreach (int id in support) supporting.Add(id); RenderPulses(); }
    /// <summary>Synchronizes only the transient source outline with the score sequence's current operation.</summary>
    public void SetScoreActivation(int sourceId, float strength)
    { scoreSources.Clear(); scoreSource = sourceId; scorePulse = strength; RenderPulses(); }
    /// <summary>Acknowledges all members of a grouped beat without inventing extra scoring triggers.</summary>
    public void SetScoreSources(IReadOnlyList<int> sources, float strength)
    { scoreSource = -1; scoreSources.Clear(); if (sources != null) foreach (int id in sources) scoreSources.Add(id); scorePulse = strength; RenderPulses(); }
    /// <summary>Keeps the final total out of the secondary panel until the primary score has delivered it.</summary>
    public void SetScoringFocus(bool active)
    {
        if (scoringFocus == active) return; scoringFocus = active;
        if (equation != null && shown?.Score != null) equation.text = active ? shown.Score.Accuracy.ToString().ToUpperInvariant() + " · SCORING"
            : compactSummary ? "LAST HIT +" + LandingScoreRush.Format(shown.Score.Points) : shown.Score.Accuracy.ToString().ToUpperInvariant() + " +" + LandingScoreRush.Format(shown.Score.Points);
        foreach (var view in chipViews) foreach (var graphic in view.GetComponentsInChildren<UnityEngine.UI.Graphic>()) graphic.canvasRenderer.SetAlpha(active ? .4f : 1);
    }
    /// <summary>Advances only the short presentation envelope, independent of challenge clocks and input.</summary>
    private void LateUpdate() { if (animate || inspected >= 0) RenderPulses(); }
    /// <summary>Keeps total activation time bounded even for very long rigs.</summary>
    private void RenderPulses()
    {
        double elapsed = Time.realtimeSinceStartupAsDouble - started;
        int total = Math.Max(1, badges.Count), order = 0;
        for (int i = 0; i < cards.Count; i++)
        {
            int id = cards[i].BoundInstance?.InstanceId ?? -1;
            bool contributes = badges.TryGetValue(id, out var badge);
            float time = (float)elapsed - (contributes ? maximumStagger * order++ / Math.Max(1, total - 1) : 0);
            float strength = animate && contributes && time >= 0 && time < pulseSeconds ? Mathf.Sin(Mathf.PI * time / pulseSeconds) : 0;
            if (sequencedScore && !attemptSources.Contains(id)) strength = 0;
            if (id == scoreSource || scoreSources.Contains(id)) strength = Mathf.Max(strength, scorePulse);
            activations[i].Render(strength, badge, attemptSources.Contains(id), inspected == id, supporting.Contains(id), !sequencedScore || attemptSources.Contains(id));
        }
        if (elapsed >= pulseSeconds + maximumStagger) animate = false;
    }
    /// <summary>Clears pooled receipt views at a fresh encounter without retaining an old result.</summary>
    private void Clear()
    {
        animate = false; badges.Clear(); attemptSources.Clear(); inspected = -1; supporting.Clear();
        scoreSource = -1; scorePulse = 0; scoreSources.Clear(); scoringFocus = false;
        if (attemptCount != null) attemptCount.rectTransform.anchoredPosition = attemptRest;
        HideUnused(rowViews, 0); HideUnused(chipViews, 0); if (equation != null) equation.text = "LAST HIT · awaiting first hit"; if (attemptFeedback != null) attemptFeedback.text = "";
        foreach (var view in activations) view.gameObject.SetActive(false);
    }
    /// <summary>Reuses authored inspection targets without rebuilding the interface for each strike.</summary>
    private static LandingContributionView Pool(List<LandingContributionView> pool, LandingContributionView prefab, Transform parent, int index)
    { while (pool.Count <= index) pool.Add(Instantiate(prefab, parent)); return pool[index]; }
    /// <summary>Places a native scroll row with an independent identity and amount.</summary>
    private void BindRow(LandingContributionView view, CardInstance member, string title, string amount, string reason, string context, int[] support)
    { view.Bind(this, member, title, amount, reason, context, support); view.Anchor.anchoredPosition = new Vector2(0, -rowViews.IndexOf(view) * 32); }
    /// <summary>Removes stale pooled rows from navigation and rendering.</summary>
    private static void HideUnused(List<LandingContributionView> pool, int used) { for (int i = used; i < pool.Count; i++) pool[i].gameObject.SetActive(false); }
    /// <summary>Retains exact scoring precision while formatting normalized assistance cleanly.</summary>
    private static string Number(decimal value) => value.ToString("0.############################", CultureInfo.InvariantCulture);
    /// <summary>Rounds only assistance percentages for display, never scoring coefficients or Landing Points.</summary>
    private static string Percent(double value) => (value * 100).ToString("0.#", CultureInfo.InvariantCulture);
}
