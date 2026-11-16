using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Renders the independent Build phase, previews legal insertions and forwards occurrence-bound placement commands.</summary>
public sealed class RigBuildingView : MonoBehaviour
{
    [SerializeField] private TMP_Text heading, haul, details, message, previewLabel, emptyRig;
    [SerializeField] private TackleRigCardView cardPrefab, pendingCard;
    [SerializeField] private RectTransform rigContent, railContent;
    [SerializeField] private UnityEngine.UI.ScrollRect rigScroll, railScroll, previewScroll;
    [SerializeField] private UnityEngine.UI.Button gapTemplate, commit;
    [SerializeField] private Sprite socketIdle, socketSelected, socketBlocked;
    private readonly List<TackleRigCardView> rigCards = new List<TackleRigCardView>(), railCards = new List<TackleRigCardView>();
    private readonly List<UnityEngine.UI.Button> gaps = new List<UnityEngine.UI.Button>();
    private FishingRunController controller;
    private long occurrence = -1, revision = -1;
    private int selected = -1;
    private InputAction move = new InputAction("Build position", InputActionType.Value, expectedControlType: "Axis");
    private InputAction submit = new InputAction("Build commit", InputActionType.Button, "<Keyboard>/enter");

    /// <summary>Uses one keyboard/gamepad position action; native buttons also support focus navigation and touch.</summary>
    private void Awake()
    {
        move.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/leftArrow").With("Positive", "<Keyboard>/rightArrow");
        move.AddCompositeBinding("1DAxis").With("Negative", "<Gamepad>/dpad/left").With("Positive", "<Gamepad>/dpad/right");
        move.performed += Move; submit.performed += Submit;
        commit.onClick.AddListener(Commit);
    }
    /// <summary>Enables Build-only controls.</summary>
    private void OnEnable() { move.Enable(); submit.Enable(); }
    /// <summary>Invalidates stale selection and releases controls when leaving this phase.</summary>
    private void OnDisable() { move.Disable(); submit.Disable(); occurrence = revision = -1; }
    /// <summary>Releases input actions owned by this view.</summary>
    private void OnDestroy() { move.performed -= Move; submit.performed -= Submit; move.Dispose(); submit.Dispose(); }

    /// <summary>Shows only new-species placement or a diagnostic; ordinary duplicate upgrades continue immediately.</summary>
    public void Present(FishingRunController owner)
    {
        controller = owner;
        bool visible = owner != null && owner.RunActive && owner.AwaitingRigPlacement && owner.PendingRigCreature != null;
        gameObject.SetActive(visible);
        if (!visible) return;
        if (occurrence != owner.BuildOccurrence || revision != owner.Rig.Revision)
        {
            occurrence = owner.BuildOccurrence; revision = owner.Rig.Revision; selected = -1;
            var members = owner.Rig.Members;
            for (int i = 0; i <= members.Length; i++) if (owner.Rig.CanInsert(owner.PendingRigCreature, i, out _)) { selected = i; break; }
            Refresh();
            rigScroll.verticalNormalizedPosition = 1; railScroll.horizontalNormalizedPosition = 0;
            if (EventSystem.current != null && selected >= 0) gaps[selected].Select();
        }
        if (!string.IsNullOrEmpty(owner.BuildFailure)) message.text = owner.BuildFailure;
    }

    /// <summary>Updates both the committed rig and a side-effect-free projected rig without mutating gameplay data.</summary>
    private void Refresh()
    {
        var creature = controller.PendingRigCreature; var members = controller.Rig.Members;
        heading.text = "BUILD / " + creature.DisplayName.ToUpperInvariant(); haul.text = "HAUL " + controller.Rig.HaulValue;
        pendingCard.Bind(CardInstance.CreateRigCreature(-1, creature));
        var ability = creature.PrimaryAbility.Snapshot();
        details.text = (ability.Effects.Count > 0 ? ability.Name + "\n" + ability.Description + "\n\n" : "") + string.Join(" / ", creature.Tags)
            + "\nBASE SALE VALUE " + creature.Value + "\nNEW SPECIES / LEVEL 1";
        emptyRig.gameObject.SetActive(members.Length == 0);
        while (rigCards.Count < members.Length) rigCards.Add(Instantiate(cardPrefab, rigContent));
        for (int i = 0; i < rigCards.Count; i++)
        {
            rigCards[i].gameObject.SetActive(i < members.Length);
            if (i >= members.Length) continue;
            rigCards[i].Bind(members[i]); ((RectTransform)rigCards[i].transform).anchoredPosition = new Vector2(0, -20 - i * 275);
        }
        rigContent.sizeDelta = new Vector2(rigContent.sizeDelta.x, Mathf.Max(825, members.Length * 275 + 40));
        while (gaps.Count <= members.Length) gaps.Add(Instantiate(gapTemplate, railContent));
        while (railCards.Count < members.Length) railCards.Add(Instantiate(cardPrefab, railContent));
        float cursor = 12;
        for (int i = 0; i < gaps.Count; i++)
        {
            var gap = gaps[i]; gap.gameObject.SetActive(i <= members.Length); if (i > members.Length) continue;
            int position = i; bool legal = controller.Rig.CanInsert(creature, i, out string reason);
            var rect = (RectTransform)gap.transform; rect.anchoredPosition = new Vector2(cursor, -45);
            gap.interactable = legal; gap.GetComponent<UnityEngine.UI.Image>().sprite = !legal ? socketBlocked : i == selected ? socketSelected : socketIdle;
            gap.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
            gap.onClick.RemoveAllListeners(); gap.onClick.AddListener(() => Select(position));
            cursor += 75;
            if (i < members.Length)
            {
                var card = railCards[i]; card.gameObject.SetActive(true); card.Bind(members[i]);
                var cardRect = (RectTransform)card.transform; cardRect.localScale = Vector3.one * .64f;
                cardRect.anchoredPosition = new Vector2(cursor, -20); cursor += 215;
            }
        }
        for (int i = members.Length; i < railCards.Count; i++) railCards[i].gameObject.SetActive(false);
        railContent.sizeDelta = new Vector2(Mathf.Max(1140, cursor + 20), 235);
        string error = null; int projected = 0; CardInstance[] preview = Array.Empty<CardInstance>();
        bool valid = selected >= 0 && controller.Rig.TryPreview(creature, selected, out preview, out projected, out error);
        commit.interactable = valid;
        if (valid)
        {
            var text = new StringBuilder("PREVIEW / HAUL " + controller.Rig.HaulValue + " -> " + projected + "\n");
            foreach (var member in preview) text.Append(member.Definition.DisplayName).Append("  Lv ").Append(member.Level)
                .Append(" / ").Append(member.HaulValueContribution).Append("   ");
            previewLabel.text = text.ToString();
            message.text = "Position " + (selected + 1) + " / Placement is permanent for this expedition.  ENTER / PLACE";
        }
        else { previewLabel.text = "No legal placement selected."; message.text = error ?? "No legal insertion position. Check active restriction abilities."; }
        previewLabel.ForceMeshUpdate();
        float height = Mathf.Max(previewScroll.viewport.rect.height, previewLabel.preferredHeight);
        previewLabel.rectTransform.sizeDelta = new Vector2(previewScroll.viewport.rect.width, height);
        previewScroll.content.sizeDelta = new Vector2(previewScroll.viewport.rect.width, height); previewScroll.verticalNormalizedPosition = 1;
    }
    /// <summary>Changes only the preview selection.</summary>
    private void Select(int position) { selected = position; Refresh(); }
    /// <summary>Cycles legal positions without moving or rearranging any committed creature.</summary>
    private void Move(InputAction.CallbackContext context)
    {
        if (controller == null || !gameObject.activeInHierarchy || !OwnsInputFocus()) return;
        int step = context.ReadValue<float>() > 0 ? 1 : -1, length = controller.Rig.Members.Length + 1;
        for (int offset = 1; offset <= length; offset++)
        {
            int next = (selected + step * offset + length * 2) % length;
            if (controller.Rig.CanInsert(controller.PendingRigCreature, next, out _)) { Select(next); gaps[next].Select(); break; }
        }
    }
    /// <summary>Shares the same guarded command as pointer/touch input.</summary>
    private void Submit(InputAction.CallbackContext context) { if (gameObject.activeInHierarchy && OwnsInputFocus()) Commit(); }
    /// <summary>Leaves unrelated settings and text-entry surfaces in control of keyboard input.</summary>
    private bool OwnsInputFocus()
    {
        var focused = EventSystem.current?.currentSelectedGameObject;
        return focused == null || focused.transform.IsChildOf(transform);
    }
    /// <summary>Uses the identities that produced the preview so stale callbacks cannot attach a different encounter.</summary>
    private void Commit()
    {
        if (controller == null || !commit.interactable) return;
        controller.TryCommitRigPlacement(occurrence, revision, selected);
    }
}
