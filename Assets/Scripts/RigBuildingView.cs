using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Projects new-species placement and automatic upgrade receipts into the approved independent Build screen.</summary>
public sealed class RigBuildingView : MonoBehaviour
{
    [SerializeField] private TMP_Text heading, haul, details, message, previewLabel, emptyRig;
    [SerializeField] private TMP_Text creatureName, tags, levelLabel, levelDelta, saleLabel, previewCaption, railHeading;
    [SerializeField] private UnityEngine.UI.Image portrait, ropeTemplate;
    [SerializeField] private TMP_FontAsset displayFont;
    [SerializeField] private TackleRigCardView cardPrefab, railCardPrefab;
    [SerializeField] private RectTransform rigContent, railContent, selectionCaret;
    [SerializeField] private UnityEngine.UI.ScrollRect rigScroll, railScroll, detailsScroll;
    [SerializeField] private UnityEngine.UI.Button gapTemplate, commit;
    [SerializeField] private GameObject upgradeStatus;
    [SerializeField] private Sprite socketIdle, socketSelected, socketBlocked;
    [SerializeField, Min(.1f)] private float upgradeFeedbackSeconds = 1.8f;
    private readonly List<TackleRigCardView> rigCards = new List<TackleRigCardView>(), railCards = new List<TackleRigCardView>();
    private readonly List<UnityEngine.UI.Button> gaps = new List<UnityEngine.UI.Button>();
    private readonly List<UnityEngine.UI.Image> ropes = new List<UnityEngine.UI.Image>();
    private FishingRunController controller;
    private long occurrence = -1, revision = -1;
    private int selected = -1;
    private bool upgrading;
    private Coroutine feedback;
    private readonly InputAction move = new InputAction("Build position", InputActionType.Value, expectedControlType: "Axis");
    private readonly InputAction submit = new InputAction("Build commit", InputActionType.Button, "<Keyboard>/enter");

    /// <summary>Owns only Build input and fonts; the controller retains acquisition and progression ownership.</summary>
    private void Awake()
    {
        if (displayFont != null) MaterialReferenceManager.AddFontAsset(displayFont);
        move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        move.AddCompositeBinding("1DAxis").With("Negative", "<Gamepad>/dpad/left").With("Positive", "<Gamepad>/dpad/right");
        move.performed += Move; submit.performed += Submit; commit.onClick.AddListener(Commit);
    }
    /// <summary>Enables phase-local actions; upgrade feedback still rejects all placement commands.</summary>
    private void OnEnable() { move.Enable(); submit.Enable(); }
    /// <summary>Stops cosmetic work and releases its input gate if a screen owner interrupts the display.</summary>
    private void OnDisable()
    {
        move.Disable(); submit.Disable();
        if (feedback != null) { StopCoroutine(feedback); feedback = null; }
        long shown = occurrence; occurrence = revision = -1;
        if (upgrading && controller != null) controller.CompleteRigUpgradeFeedback(shown);
        upgrading = false;
    }
    /// <summary>Releases the native input actions owned by this view.</summary>
    private void OnDestroy() { move.performed -= Move; submit.performed -= Submit; move.Dispose(); submit.Dispose(); }

    /// <summary>Displays a pending placement or an already committed upgrade without leaking the next Discover screen.</summary>
    public void Present(FishingRunController owner)
    {
        controller = owner;
        bool upgrade = owner != null && owner.RigUpgradeFeedbackVisible && owner.LastRigAcquisition != null;
        bool visible = owner != null && owner.RunActive && (upgrade || owner.AwaitingRigPlacement && owner.PendingRigCreature != null);
        gameObject.SetActive(visible);
        if (!visible) return;
        long id = upgrade ? owner.LastRigAcquisition.Occurrence : owner.BuildOccurrence;
        if (occurrence != id || revision != owner.Rig.Revision || upgrading != upgrade)
        {
            occurrence = id; revision = owner.Rig.Revision; upgrading = upgrade; selected = -1;
            if (!upgrade)
                for (int i = owner.Rig.Members.Length; i >= 0; i--)
                    if (owner.Rig.CanInsert(owner.PendingRigCreature, i, out _)) { selected = i; break; }
            Refresh();
            rigScroll.verticalNormalizedPosition = 1; railScroll.horizontalNormalizedPosition = !upgrade && selected == owner.Rig.Members.Length ? 1 : 0;
            if (upgrade)
            {
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                feedback = StartCoroutine(CompleteFeedback(id));
            }
            else if (EventSystem.current != null && selected >= 0) gaps[selected].Select();
        }
        if (!string.IsNullOrEmpty(owner.BuildFailure)) TackleBuildTypography.SetText(message, owner.BuildFailure);
    }

    /// <summary>Leaves upgrade acquisition automatic and advances its cosmetic feedback on an unscaled clock.</summary>
    private IEnumerator CompleteFeedback(long id)
    {
        yield return new WaitForSecondsRealtime(upgradeFeedbackSeconds);
        feedback = null;
        if (controller != null) controller.CompleteRigUpgradeFeedback(id);
    }

    /// <summary>Uses actual projected sale values and current-level effects, never the example values printed in a mockup.</summary>
    private void Refresh()
    {
        var receipt = controller.LastRigAcquisition;
        var creature = upgrading ? receipt.Member.Definition : controller.PendingRigCreature;
        var members = upgrading ? receipt.Members : controller.Rig.Members;
        int projected = controller.Rig.HaulValue; string error = null; CardInstance[] preview = Array.Empty<CardInstance>();
        bool valid = !upgrading && selected >= 0 && controller.Rig.TryPreview(creature, selected, out preview, out projected, out error);
        var displayed = upgrading ? receipt.Member : valid ? preview.First(m => string.Equals(m.Definition.UniqueId, creature.UniqueId, StringComparison.OrdinalIgnoreCase)) : CardInstance.CreateRigCreature(-1, creature);
        string biome = upgrading ? receipt.BiomeName : controller.CurrentBiome?.GetDepthTier(controller.CurrentDepth)?.DisplayName ?? controller.CurrentBiome?.DisplayName ?? "Waters";
        TackleBuildTypography.SetText(heading, biome.ToUpperInvariant() + " / BUILD");
        TackleBuildTypography.SetText(haul, "HAUL " + (upgrading ? receipt.Haul : controller.Rig.HaulValue));
        TackleBuildTypography.SetText(creatureName, creature.DisplayName.ToUpperInvariant() + " LANDED");
        TackleBuildTypography.SetText(tags, string.Join(" / ", creature.Tags).ToUpperInvariant());
        TackleBuildTypography.SetText(levelLabel, upgrading ? "LV " + receipt.PreviousLevel + " > " + displayed.Level : "LV 1");
        TackleBuildTypography.SetText(levelDelta, upgrading ? "+1" : "");
        TackleBuildTypography.SetText(saleLabel, displayed.HaulValueContribution.ToString());
        string description = RigAbilityText.Describe(displayed.PrimaryAbility, displayed.Level);
        if (displayed.PrimaryAbility.Effects.Count == 1 && displayed.PrimaryAbility.Effects[0].Kind == RigAbilityKind.LandingMultiplier)
        {
            var effect = displayed.PrimaryAbility.Effects[0];
            description = description.Replace(" strikes gain ", " strikes gain\n");
            if (effect.Count == RigPositionCount.ContiguousOther && !string.IsNullOrEmpty(effect.Tag)) description = description.Replace(" " + effect.Tag + " creature.", "\n" + effect.Tag + " creature.");
            description = description.Replace("<color=", "<font=\"" + displayFont.name + "\"><size=23><color=").Replace("</color>", "</color></size></font>");
        }
        TackleBuildTypography.SetText(details, description);
        float rulesWidth = detailsScroll.viewport.rect.width;
        float rulesHeight = Mathf.Max(detailsScroll.viewport.rect.height, details.GetPreferredValues(description, rulesWidth, float.PositiveInfinity).y);
        detailsScroll.content.sizeDelta = new Vector2(rulesWidth, rulesHeight);
        ((RectTransform)details.transform.parent).sizeDelta = new Vector2(rulesWidth, rulesHeight);
        details.rectTransform.sizeDelta = new Vector2(rulesWidth, rulesHeight);
        TackleBuildTypography.SetText(details, description); detailsScroll.verticalNormalizedPosition = 1;
        portrait.sprite = creature.EncounterArtwork ?? creature.Artwork; portrait.enabled = portrait.sprite != null;
        TackleBuildTypography.SetText(previewCaption, upgrading ? "HAUL CHANGE" : "HAUL PREVIEW");
        TackleBuildTypography.SetText(previewLabel, upgrading ? receipt.PreviousHaul + " > " + receipt.Haul : controller.Rig.HaulValue + " > " + projected);
        TackleBuildTypography.SetText(railHeading, upgrading ? "YOUR RIG" : "CHOOSE INSERTION POINT");
        commit.gameObject.SetActive(!upgrading); commit.interactable = valid; upgradeStatus.SetActive(upgrading);
        emptyRig.gameObject.SetActive(members.Length == 0);
        RefreshCabinet(members);
        RefreshRail(members, creature, valid ? preview : null);
        string placement = selected == 0 ? "Before " + (members.Length > 0 ? members[0].Definition.DisplayName : creature.DisplayName)
            : selected > 0 && selected <= members.Length ? "After " + members[selected - 1].Definition.DisplayName : "Choose a position";
        TackleBuildTypography.SetText(message, upgrading ? creature.DisplayName + " +1 level" : valid ? placement : error ?? "No legal insertion position");
    }

    /// <summary>Pools the unchanged ordered rig and extends the cabinet scroll beyond three cards.</summary>
    private void RefreshCabinet(CardInstance[] members)
    {
        while (rigCards.Count < members.Length) rigCards.Add(Instantiate(cardPrefab, rigContent));
        for (int i = 0; i < rigCards.Count; i++)
        {
            rigCards[i].gameObject.SetActive(i < members.Length);
            if (i >= members.Length) continue;
            rigCards[i].Bind(members[i]); ((RectTransform)rigCards[i].transform).anchoredPosition = new Vector2(0, -20 - i * 275);
        }
        rigContent.sizeDelta = new Vector2(315, Mathf.Max(825, 40 + members.Length * 275));
    }

    /// <summary>Fits independent cards, ropes and insertion targets inside the housing's built-in wooden recess, scrolling longer uncapped rigs.</summary>
    private void RefreshRail(CardInstance[] members, CardDefinition creature, CardInstance[] preview)
    {
        int count = members.Length;
        float width = Mathf.Max(railScroll.viewport.rect.width, count * 304 + 106), first = (width - (count * 304 - 83)) * .5f - 3;
        while (railCards.Count < count) railCards.Add(Instantiate(railCardPrefab, railContent));
        while (gaps.Count <= count) gaps.Add(Instantiate(gapTemplate, railContent));
        while (ropes.Count < Mathf.Max(0, count - 1)) { var rope = Instantiate(ropeTemplate, railContent); rope.transform.SetAsFirstSibling(); ropes.Add(rope); }
        for (int i = 0; i < ropes.Count; i++)
        {
            ropes[i].gameObject.SetActive(i < count - 1);
            if (i < count - 1) { ropes[i].rectTransform.anchoredPosition = new Vector2(first + 110 + i * 304, -7); ropes[i].rectTransform.sizeDelta = new Vector2(304, 48); }
        }
        for (int i = 0; i < railCards.Count; i++)
        {
            var card = railCards[i]; card.gameObject.SetActive(i < count); if (i >= count) continue;
            var projectedMember = preview == null ? members[i] : preview.First(m => m.InstanceId == members[i].InstanceId);
            card.Bind(projectedMember, preview != null); ((RectTransform)card.transform).anchoredPosition = new Vector2(first + i * 304, -27);
            card.SetClipVisible(true);
            card.SetAcquiredHighlight(upgrading && members[i].InstanceId == controller.LastRigAcquisition.Member.InstanceId);
        }
        for (int i = 0; i < gaps.Count; i++)
        {
            var gap = gaps[i]; gap.gameObject.SetActive(!upgrading && i <= count); if (upgrading || i > count) continue;
            int position = i; float x = count == 0 ? (width - 70) * .5f : i == 0 ? first - 84 : i == count ? first + (count - 1) * 304 + 238 : first + (i - 1) * 304 + 227;
            bool legal = controller.Rig.CanInsert(creature, i, out _);
            ((RectTransform)gap.transform).anchoredPosition = new Vector2(x, -85);
            gap.interactable = legal; gap.GetComponent<UnityEngine.UI.Image>().sprite = !legal ? socketBlocked : i == selected ? socketSelected : socketIdle;
            gap.onClick.RemoveAllListeners(); gap.onClick.AddListener(() => Select(position));
            gap.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit, selectOnDown = commit,
                selectOnUp = count > 0 ? railCards[Mathf.Min(i,count-1)].InspectionTarget : null };
            if (i == selected) selectionCaret.anchoredPosition = new Vector2(x, -55);
        }
        selectionCaret.gameObject.SetActive(!upgrading && selected >= 0);
        if (!upgrading && selected >= 0) commit.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit, selectOnUp = gaps[selected] };
        selectionCaret.SetAsLastSibling(); railContent.sizeDelta = new Vector2(width, 223);
        for (int i = 0; i < count; i++)
        {
            var target = railCards[i].InspectionTarget;
            if (target != null) target.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnLeft = i > 0 ? railCards[i-1].InspectionTarget : rigCards[i].InspectionTarget,
                selectOnRight = i+1 < count ? railCards[i+1].InspectionTarget : null,
                selectOnDown = !upgrading ? gaps[Mathf.Min(i,count)] : null, selectOnUp = rigCards[i].InspectionTarget };
            target = rigCards[i].InspectionTarget;
            if (target != null) target.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnUp = i > 0 ? rigCards[i-1].InspectionTarget : null, selectOnDown = i+1 < count ? rigCards[i+1].InspectionTarget : commit,
                selectOnRight = railCards[i].InspectionTarget };
        }
    }

    /// <summary>Changes the preview only and scrolls the selected native target into view.</summary>
    private void Select(int position)
    {
        if (upgrading) return;
        selected = position; Refresh(); Canvas.ForceUpdateCanvases();
        var rect = (RectTransform)gaps[position].transform; float left = -railContent.anchoredPosition.x;
        if (rect.anchoredPosition.x < left) left = rect.anchoredPosition.x;
        else if (rect.anchoredPosition.x + rect.rect.width > left + railScroll.viewport.rect.width) left = rect.anchoredPosition.x + rect.rect.width - railScroll.viewport.rect.width;
        railContent.anchoredPosition = new Vector2(-Mathf.Clamp(left, 0, Mathf.Max(0, railContent.rect.width - railScroll.viewport.rect.width)), 0);
    }
    /// <summary>Cycles only legal gaps, preserving the committed members' relative order.</summary>
    private void Move(InputAction.CallbackContext context)
    {
        if (controller == null || upgrading || !gameObject.activeInHierarchy || !OwnsInputFocus()) return;
        int step = context.ReadValue<float>() > 0 ? 1 : -1, length = controller.Rig.Members.Length + 1;
        for (int offset = 1; offset <= length; offset++)
        {
            int next = (selected + step * offset + length * 2) % length;
            if (controller.Rig.CanInsert(controller.PendingRigCreature, next, out _)) { Select(next); gaps[next].Select(); break; }
        }
    }
    /// <summary>Shares the guarded placement command with pointer/touch controls.</summary>
    private void Submit(InputAction.CallbackContext context) { if (!upgrading && gameObject.activeInHierarchy && OwnsInputFocus()) Commit(); }
    /// <summary>Leaves settings and unrelated native surfaces in control of keyboard focus.</summary>
    private bool OwnsInputFocus()
    {
        var focused = EventSystem.current?.currentSelectedGameObject;
        return focused == null || focused.transform.IsChildOf(transform) && focused.GetComponent<TackleCardTooltipSource>() == null;
    }
    /// <summary>Submits only the landed occurrence and rig revision that produced this preview.</summary>
    private void Commit()
    {
        if (controller == null || upgrading || !commit.interactable) return;
        controller.TryCommitRigPlacement(occurrence, revision, selected);
    }
}
