using UnityEditor;
using UnityEngine;

/// <summary>Separates supplied card regions into native movable groups and preserves those bindings during Landing regeneration.</summary>
public static class LandingCardCollisionSetup
{
    /// <summary>Reuses supplied housing and native fields, then binds the separate authored water-impact layer.</summary>
    public static void Configure(GameObject root)
    {
        Transform stage = root.transform.Find("1920x1080 Stage");
        var owner = root.GetComponent<LandingCardCollision>(); if (owner == null) owner = root.AddComponent<LandingCardCollision>();
        var rush = new SerializedObject(root.GetComponent<LandingScoreRush>());
        var view = new SerializedObject(root.GetComponent<TackleLandingPresentation>());
        var machine = ((RectTransform)rush.FindProperty("machine").objectReferenceValue);
        var original = machine.GetComponent<UnityEngine.UI.Image>(); original.enabled = false;
        var shell = machine.Find("Collision housing") as RectTransform;
        if (shell == null) shell = (RectTransform)new GameObject("Collision housing", typeof(RectTransform)).transform;
        shell.SetParent(machine, false); shell.anchorMin = Vector2.zero; shell.anchorMax = Vector2.one; shell.offsetMin = shell.offsetMax = Vector2.zero;
        Artwork(shell, original.sprite, TackleMachineGeometry.StageBounds, true);
        var creature = Group(stage, "Creature collision card", LandingCollisionGraphic.CreatureRegion, original.sprite, -12);
        var score = Group(stage, "Score collision card", LandingCollisionGraphic.ScoreRegion, original.sprite, 12);
        var fade = creature.GetComponent<CanvasGroup>(); if (fade == null) fade = creature.gameObject.AddComponent<CanvasGroup>();
        fade.alpha = 1; fade.interactable = false; fade.blocksRaycasts = false;
        foreach (string field in new[] { "portrait", "tags" }) Move(view.FindProperty(field).objectReferenceValue, creature, LandingCollisionGraphic.CreatureRegion);
        foreach (string field in new[] { "points", "resistance" }) Move(view.FindProperty(field).objectReferenceValue, score, LandingCollisionGraphic.ScoreRegion);
        foreach (string name in new[] { "PointsCaption", "ResistanceCaption", "Separator", "Progress Clip" })
            Move((Object)(stage.Find(name) ?? score.Find(name)), score, LandingCollisionGraphic.ScoreRegion);
        Move(rush.FindProperty("readout").objectReferenceValue, score, LandingCollisionGraphic.ScoreRegion);
        var data = new SerializedObject(owner);
        data.FindProperty("creatureCard").objectReferenceValue = creature; data.FindProperty("scoreCard").objectReferenceValue = score;
        data.FindProperty("creatureFlash").objectReferenceValue = Flash(creature);
        data.FindProperty("scoreFlash").objectReferenceValue = Flash(score);
        data.FindProperty("stage").objectReferenceValue = stage;
        data.FindProperty("creatureFade").objectReferenceValue = fade; data.ApplyModifiedPropertiesWithoutUndo();
        rush.FindProperty("collision").objectReferenceValue = owner; rush.ApplyModifiedPropertiesWithoutUndo();
        var camera = root.GetComponent<LandingImpactCamera>(); if (camera == null) camera = root.AddComponent<LandingImpactCamera>();
        var cameraData = new SerializedObject(camera);
        cameraData.FindProperty("stage").objectReferenceValue = stage;
        cameraData.FindProperty("creatureCard").objectReferenceValue = creature; cameraData.FindProperty("scoreCard").objectReferenceValue = score;
        cameraData.ApplyModifiedPropertiesWithoutUndo();
        rush.FindProperty("impactCamera").objectReferenceValue = camera; rush.ApplyModifiedPropertiesWithoutUndo();
        LandingWaterImpactSetup.Configure(root);
    }

    /// <summary>Places a non-intercepting white silhouette above the panel's artwork and native fields.</summary>
    private static LandingPanelFlash Flash(RectTransform panel)
    {
        var rect = panel.Find("Impact white flash") as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject("Impact white flash", typeof(RectTransform), typeof(LandingPanelFlash)).transform;
        rect.SetParent(panel, false); rect.SetAsLastSibling();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
        var graphic = rect.GetComponent<LandingPanelFlash>(); graphic.raycastTarget = false; graphic.Show(0);
        var source = new SerializedObject(panel.GetComponent<LandingCollisionGraphic>());
        var data = new SerializedObject(graphic); data.FindProperty("panel").objectReferenceValue = source.FindProperty("panel").objectReferenceValue;
        data.ApplyModifiedPropertiesWithoutUndo(); return graphic;
    }

    /// <summary>Leaves clear travel space around smaller rigid faces while preserving their internal native-field layout.</summary>
    private static RectTransform Group(Transform stage, string name, Rect region, Sprite source, float separation)
    {
        var rect = stage.Find(name) as RectTransform;
        if (rect == null) rect = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rect.SetParent(stage, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(.5f, .5f);
        rect.anchoredPosition = new Vector2(region.center.x + separation, -region.center.y); rect.sizeDelta = region.size;
        rect.localScale = Vector3.one * .88f;
        Artwork(rect, source, region, false); return rect;
    }

    /// <summary>Binds a fixed shell or independent panel silhouette without changing the existing motion bounds.</summary>
    private static void Artwork(RectTransform rect, Sprite source, Rect area, bool housing)
    {
        var graphic = rect.GetComponent<LandingCollisionGraphic>(); if (graphic == null) graphic = rect.gameObject.AddComponent<LandingCollisionGraphic>();
        graphic.raycastTarget = false;
        var data = new SerializedObject(graphic); data.FindProperty("source").objectReferenceValue = housing ? source : null;
        data.FindProperty("panel").objectReferenceValue = housing ? null : LandingPanelArtworkSetup.Get(source, area == LandingCollisionGraphic.ScoreRegion);
        data.FindProperty("region").rectValue = area; data.FindProperty("housing").boolValue = housing; data.ApplyModifiedPropertiesWithoutUndo();
    }

    /// <summary>Moves an existing native field into its panel without changing its reference-pixel layout or serialized identity.</summary>
    private static void Move(Object value, RectTransform parent, Rect bounds)
    {
        var component = value as Component; if (component == null) return;
        var rect = component.transform as RectTransform; if (rect == null || rect.parent == parent) return;
        Vector2 position = rect.anchoredPosition;
        rect.SetParent(parent, false); rect.anchoredPosition = position - new Vector2(bounds.x, -bounds.y);
    }
}
