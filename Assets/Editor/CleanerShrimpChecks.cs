using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Exercises Cleaner Shrimp effect lifetimes, biome swaps, controller suspension, and modal layout.</summary>
public static class CleanerShrimpChecks
{
    private static readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();
    private static int assertions;

    /// <summary>Runs the focused suite in an isolated editor project or from the editor menu.</summary>
    [MenuItem("Fishing Cards/Validation/Cleaner Shrimp")]
    public static void Run()
    {
        string output = Environment.GetEnvironmentVariable("CLEANER_SHRIMP_VALIDATION_OUTPUT")
            ?? "Docs/Validation/CleanerShrimp";
        Directory.CreateDirectory(output);
        try
        {
            assertions = 0;
            ValidateAsset();
            ValidateCleanseLifetime();
            ValidateOffersAndSwaps();
            ValidateController();
            ValidateModal(output);
            File.WriteAllText(Path.Combine(output, "result.json"),
                "{\"status\":\"passed\",\"assertions\":" + assertions + ",\"unity\":\"" + Application.unityVersion + "\"}");
            Debug.Log("Cleaner Shrimp validation passed: " + assertions + " assertions.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            File.WriteAllText(Path.Combine(output, "failure.txt"), error.ToString());
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
            else throw;
        }
        finally
        {
            foreach (UnityEngine.Object obj in created)
            {
                if (obj != null) UnityEngine.Object.DestroyImmediate(obj);
            }
            created.Clear();
        }
    }

    /// <summary>Checks authored stats, art retention, role separation, and exact effect type mapping.</summary>
    private static void ValidateAsset()
    {
        CardDefinition shrimp = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Cleaner Shrimp.asset");
        Require(shrimp != null, "Cleaner Shrimp asset missing.");
        Require(shrimp.Weight == 1 && shrimp.Value == 8 && shrimp.Rarity == CardRarity.Uncommon, "Stats differ from supplied design.");
        Require(shrimp.MinimumDepth == 1 && shrimp.MaximumDepth == 14, "Depth bounds differ.");
        Require(shrimp.Tags.Length == 1 && shrimp.Tags[0] == "Small", "Tags differ.");
        Require(shrimp.Artwork != null && shrimp.EncounterArtwork != null, "Existing art was lost.");
        Require(shrimp.Effects.Length == 1 && shrimp.Effects[0].EffectType == CardEffectType.RemoveNegativeEffect, "Catch effect differs.");
        Require(shrimp.BaitEffects.Length == 1 && shrimp.BaitEffects[0].EffectType == CardEffectType.SwapCatchAtBiomeStart, "Bait effect differs.");
    }

    /// <summary>Checks cleansing across duplicate sources, recalculation, snapshots, and future triggers.</summary>
    private static void ValidateCleanseLifetime()
    {
        EffectResolver resolver = new EffectResolver();
        CatchChainRuntime chain = new CatchChainRuntime();
        EncounterRuntime encounters = new EncounterRuntime();
        TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        CardEffectDefinition penalty = Effect(CardEffectType.AddLineLoadModifier, CardEffectTrigger.WhileAttached,
            CardEffectTarget.Self, 2, CardEffectTone.Negative);
        CardDefinition crab = Card("Crab", penalty);
        CardInstance first = chain.Add(crab, resolver);
        CardInstance second = chain.Add(crab, resolver);
        Require(chain.CurrentLineLoad == 6, "Persistent penalties did not apply.");
        NegativeEffectOption[] options = NegativeEffectRuntime.GetOptions(chain, encounters, techniques, resolver);
        Require(options.Length == 2 && options[0].TryRemove(), "Negative selection failed.");
        Require(first.CurrentWeight == 1 && second.CurrentWeight == 3, "Removal leaked between copies.");
        chain.Add(Card("Other"), resolver);
        chain.Recalculate(resolver);
        Require(first.CurrentWeight == 1 && first.CreateSnapshot().ActiveEffects[0] == null, "Removed effect returned on rebuild or snapshot.");
        Require(crab.Effects[0] == penalty, "Shared definition mutated.");
        Require(NegativeEffectRuntime.GetOptions(chain, encounters, techniques, resolver).Length == 1, "Removed effect still eligible.");

        CardEffectDefinition lockEffect = Effect(CardEffectType.PreventRelease, CardEffectTrigger.WhileAttached,
            CardEffectTarget.Self, 0, CardEffectTone.Negative);
        CardInstance mussel = chain.Add(Card("Mussel", lockEffect), resolver);
        Require(mussel.PreventsReleaseWhileAttached, "Release lock missing.");
        options = NegativeEffectRuntime.GetOptions(chain, encounters, techniques, resolver);
        Require(options[options.Length - 1].TryRemove() && !mussel.PreventsReleaseWhileAttached, "Cleansed lock persisted.");

        CardEffectDefinition nextPenalty = Effect(CardEffectType.AddLineLoadModifier, CardEffectTrigger.WhenNextMatchingCatchCaught,
            CardEffectTarget.SpecificCaughtCard, 2, CardEffectTone.Negative);
        CatchChainRuntime future = new CatchChainRuntime();
        future.Add(Card("Jelly", nextPenalty), resolver);
        options = NegativeEffectRuntime.GetOptions(future, encounters, techniques, resolver);
        Require(options.Length == 1 && options[0].TryRemove(), "Future trigger could not be cleansed.");
        Require(future.Add(Card("Fish"), resolver).CurrentWeight == 1, "Removed future penalty still triggered.");
        future.Reset();
        future.Add(Card("Jelly", nextPenalty), resolver);
        CardInstance affected = future.Add(Card("Fish"), resolver);
        Require(affected.CurrentWeight == 3, "Expected one-time penalty did not resolve.");
        options = NegativeEffectRuntime.GetOptions(future, encounters, techniques, resolver);
        Require(options.Length == 1 && options[0].TryRemove() && affected.CurrentWeight == 1,
            "Lasting resolved penalty could not be removed.");
        Require(NegativeEffectRuntime.GetOptions(future, encounters, techniques, resolver).Length == 0,
            "Removed lasting penalty remained eligible.");

        Set(techniques, "pendingDescendEffects", new[] { penalty, penalty });
        options = NegativeEffectRuntime.GetOptions(new CatchChainRuntime(), encounters, techniques, resolver);
        Require(options.Length == 2 && options[0].TryRemove() && techniques.PendingDescendEffects.Length == 1, "Queued duplicate removal was not exact.");
        techniques.Reset();
        encounters.SetCurrentEncounter(Card("Hooked", Effect(CardEffectType.HideEncounterInformation,
            CardEffectTrigger.WhileAttached, CardEffectTarget.CurrentEncounter, 0, CardEffectTone.Negative)));
        options = NegativeEffectRuntime.GetOptions(new CatchChainRuntime(), encounters, techniques, resolver);
        Require(options.Length == 1 && options[0].TryRemove() && encounters.HookedEffectRecords.Length == 0, "Hooked negative removal failed.");
    }

    /// <summary>Checks offer filtering, role restrictions, sequential Shrimp, and replacement attachment behavior.</summary>
    private static void ValidateOffersAndSwaps()
    {
        EffectResolver resolver = new EffectResolver();
        EncounterRuntime encounters = new EncounterRuntime();
        CatchChainRuntime chain = new CatchChainRuntime();
        TechniqueEffectRuntime techniques = new TechniqueEffectRuntime();
        CreatureEffectRuntime runtime = new CreatureEffectRuntime();
        CardDefinition shrimp = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Cleaner Shrimp.asset");
        CardDefinition one = Card("One");
        CardDefinition two = Card("Two");
        Set(two, "weight", 4);
        Set(two, "value", 17);
        CardDefinition apex = Card("Apex"); Set(apex, "cardType", CardType.ApexEncounter);
        CardDefinition wrongDepth = Card("Too deep"); Set(wrongDepth, "minimumDepth", 9);
        CardDefinition wrongBiome = Card("Wrong biome"); Set(wrongBiome, "biomeIds", new[] { "Elsewhere" });
        BiomeDefinition biome = Biome("New", 3, new[] { one, one, two, apex, wrongDepth, wrongBiome });
        encounters.SetCurrentEncounter(one);
        CardDefinition current = encounters.CurrentEncounter;
        CardDefinition[] offers = encounters.OfferCreatures(biome.GetEncounterPool(3), "New", 3, 2,
            new System.Random(11), chain.ActiveEffectRecords, resolver);
        Require(offers.Length == 2 && offers[0] != offers[1], "Offers were not distinct.");
        Require(Array.Exists(offers, c => c == one) && Array.Exists(offers, c => c == two), "Offer eligibility failed.");
        Require(encounters.CurrentEncounter == current && encounters.RecentEncounterHistory.Length == 0, "Offers consumed a normal encounter.");
        CardInstance baitOne = chain.Add(shrimp, resolver, CatchAttachmentRole.Bait);
        CardInstance baitTwo = chain.Add(shrimp, resolver, CatchAttachmentRole.Bait);
        runtime.RecordAttachment(baitOne); runtime.RecordAttachment(baitTwo);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && baitOne.HaulValueContribution == 0, "Bait triggered Catch cleanse or gained haul Value.");
        CardInstance catchOne = chain.Add(one, resolver); runtime.RecordAttachment(catchOne);
        CardInstance catchTwo = chain.Add(two, resolver); runtime.RecordAttachment(catchTwo);
        catchOne.AddPermanentModifiers(2, 10);
        runtime.BeginBiome(chain.Catches);
        runtime.PrepareChoice(chain, encounters, biome, techniques, resolver, new System.Random(22), () => 0);
        Require(runtime.GetSwapTargets(chain.Catches).Length == 2, "Pre-entry Catch eligibility failed.");
        Require(!runtime.TrySwap(baitOne.InstanceId, 0, chain, resolver, new System.Random(23)), "Bait was selectable for swap.");
        Require(runtime.TrySwap(catchOne.InstanceId, 0, chain, resolver, new System.Random(23)), "First swap failed.");
        CardInstance replacement = chain.Catches[2];
        Require(replacement.InstanceId != catchOne.InstanceId && !replacement.IsBait
            && replacement.PermanentWeightModifier == 0 && replacement.PermanentValueModifier == 0, "Replacement retained old mutations or role.");
        runtime.PrepareChoice(chain, encounters, biome, techniques, resolver, new System.Random(24), () => 0);
        Require(runtime.GetSwapTargets(chain.Catches).Length == 1 && runtime.GetSwapTargets(chain.Catches)[0] == catchTwo, "Replacement was eligible again in the same biome.");
        Require(runtime.TrySwap(catchTwo.InstanceId, 0, chain, resolver, new System.Random(25)), "Second Shrimp did not resolve.");
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None && chain.ReleasedCatches.Length == 0, "Swap fired Release accounting or queue remained.");
        runtime.BeginBiome(chain.Catches);
        runtime.PrepareChoice(chain, encounters, biome, techniques, resolver, new System.Random(26), () => 0);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.SwapCatch && runtime.GetSwapTargets(chain.Catches).Length == 2, "Surviving Shrimp did not trigger at the next biome.");

        runtime.Reset(); chain.Reset();
        CardInstance bait = chain.Add(shrimp, resolver, CatchAttachmentRole.Bait); runtime.RecordAttachment(bait);
        runtime.BeginBiome(chain.Catches);
        runtime.PrepareChoice(chain, encounters, biome, techniques, resolver, new System.Random(27), () => 0);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No-Catch swap did not skip.");
        CardInstance old = chain.Add(one, resolver); runtime.RecordAttachment(old);
        BiomeDefinition single = Biome("Single", 1, new[] { shrimp });
        runtime.BeginBiome(chain.Catches);
        runtime.PrepareChoice(chain, encounters, single, techniques, resolver, new System.Random(28), () => 0);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "Single eligible offer did not skip.");
        single = Biome("Coastal", 1, new[] { shrimp, one });
        runtime.BeginBiome(chain.Catches);
        runtime.PrepareChoice(chain, encounters, single, techniques, resolver, new System.Random(28), () => 0);
        int shrimpOffer = Array.IndexOf(runtime.OfferedCreatures, shrimp);
        Require(runtime.TrySwap(old.InstanceId, shrimpOffer, chain, resolver, new System.Random(29))
            && runtime.ChoiceKind == CreatureEffectChoiceKind.RemoveNegativeEffect, "Replacement's Catch on-attach effect did not activate.");
        runtime.PrepareChoice(chain, encounters, single, techniques, resolver, new System.Random(30), () => 0);
        Require(runtime.ChoiceKind == CreatureEffectChoiceKind.None, "No-negative on-attach effect did not skip.");
    }

    /// <summary>Checks suspended Descend actions and preservation across an authored biome transition.</summary>
    private static void ValidateController()
    {
        CardDefinition shrimp = AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Cleaner Shrimp.asset");
        BiomeDefinition coastal = Biome("Coastal", 1, new[] { shrimp });
        GameObject obj = new GameObject("Controller Check"); created.Add(obj);
        FishingRunController controller = obj.AddComponent<FishingRunController>();
        Set(controller, "startRunOnAwake", false); Set(controller, "startingBiome", coastal);
        Set(controller, "startingDepth", 1); Set(controller, "startingLineCapacity", 100);
        controller.StartRun();
        CatchChainRuntime chain = Get<CatchChainRuntime>(controller, "catchChainRuntime");
        EffectResolver resolver = Get<EffectResolver>(controller, "effectResolver");
        CardDefinition penaltyCard = Card("Penalty", Effect(CardEffectType.AddLineLoadModifier,
            CardEffectTrigger.WhileAttached, CardEffectTarget.Self, 2, CardEffectTone.Negative));
        chain.Add(penaltyCard, resolver);
        Require(controller.TryDescend() && controller.CurrentDepth == 1
            && controller.PendingCreatureChoice == CreatureEffectChoiceKind.RemoveNegativeEffect, "Descend did not pause immediately after attachment.");
        Require(!controller.TryDescend() && !controller.TrySurface() && !controller.TryReleaseCatch(0), "Core action bypassed a pending choice.");
        Require(controller.TryChooseNegativeEffect(0) && controller.CurrentDepth == 2
            && !controller.HasPendingCreatureChoice, "Descend did not resume after cleansing.");
        Require(chain.Catches[0].CurrentWeight == 1, "Controller cleansing failed.");
        Require(TechniqueRuleChecks.ReleaseFixture(controller, 1) && chain.Catches[0].CurrentWeight == 1, "Releasing Shrimp undid its completed cleanse.");
        CreatureEffectRuntime runtime = Get<CreatureEffectRuntime>(controller, "creatureEffectRuntime");
        runtime.RecordAttachment(chain.Add(shrimp, resolver, CatchAttachmentRole.Bait));
        BiomeDefinition next = Biome("Next", 5, new[] { Card("New fish"), Card("Second new fish") });
        Set(coastal, "nextBiome", next);
        Require(!controller.TryEnterNextBiome(), "Biome entry bypassed the unresolved Apex boundary.");
        BiomeApexRuntime apexRuntime = Get<BiomeApexRuntime>(controller, "biomeApexRuntime");
        Set(apexRuntime, "state", BiomeApexState.Caught);
        apexRuntime.RecordNextWatersPresented();
        int beforeCount = chain.Catches.Length;
        Require(controller.TryEnterNextBiome() && controller.CurrentBiome == next
            && controller.CurrentDepth == 5 && chain.Catches.Length == beforeCount, "Biome transition lost build state.");
        Require(controller.PendingCreatureChoice == CreatureEffectChoiceKind.SwapCatch
            && controller.CurrentEncounter == null, "First encounter appeared before entry swaps.");
        Require(controller.TryChooseCreatureSwap(chain.Catches[0].InstanceId, 0)
            && controller.CurrentEncounter != null && !controller.HasPendingCreatureChoice, "Biome reveal did not resume after swapping.");
    }

    /// <summary>Checks scrolling options, callback forwarding, input blocking, and renders the actual modal.</summary>
    private static void ValidateModal(string output)
    {
        GameObject cameraObject = new GameObject("Choice Camera"); created.Add(cameraObject);
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.08f, 0.1f);
        camera.orthographic = true;
        GameObject canvasObject = new GameObject("Choice Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        created.Add(canvasObject);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1f;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        GameObject modalObject = new GameObject("Creature Effect Choice", typeof(RectTransform));
        modalObject.transform.SetParent(canvasObject.transform, false);
        CreatureEffectChoiceView modal = modalObject.AddComponent<CreatureEffectChoiceView>();
        modal.Initialize(TMP_Settings.defaultFontAsset);
        int selected = -1;
        modal.Show("Quick Clean", "Choose one negative effect to remove.",
            new[] { "Clinging Crab", "Mussel", "Glass Minnow", "Stormjaw Tuna", "Squid", "Lionfish" },
            new[] { "The previous catch gains 2 Weight while Clinging Crab is attached.", "Mussel cannot be released while attached.",
                "Predators become more likely.", "Predator catches gain 1 Weight.", "The next encounter is hidden.", "On another Release, lose 5 Value." },
            null, index => selected = index);
        Canvas.ForceUpdateCanvases();
        Require(modalObject.GetComponent<Image>().raycastTarget, "Modal does not block gameplay input.");
        Button[] buttons = modalObject.GetComponentsInChildren<Button>();
        Require(buttons.Length == 6, "Options were lost.");
        buttons[1].onClick.Invoke(); Require(selected == 1, "Choice callback differs.");
        RenderTexture render = new RenderTexture(1280, 720, 24); created.Add(render);
        camera.targetTexture = render; camera.Render();
        RenderTexture previous = RenderTexture.active; RenderTexture.active = render;
        Texture2D image = new Texture2D(1280, 720, TextureFormat.RGB24, false); created.Add(image);
        image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
        File.WriteAllBytes(Path.Combine(output, "quick-clean-modal.png"), image.EncodeToPNG());
        RenderTexture.active = previous; camera.targetTexture = null;
        modal.Hide(); Require(!modalObject.activeSelf, "Modal remained visible after completion.");
    }

    /// <summary>Creates an ordinary creature fixture with controlled role effects.</summary>
    private static CardDefinition Card(string name, params CardEffectDefinition[] effects)
    {
        CardDefinition card = ScriptableObject.CreateInstance<CardDefinition>(); created.Add(card);
        Set(card, "displayName", name); Set(card, "uniqueId", name); Set(card, "cardType", CardType.Creature);
        Set(card, "weight", 1); Set(card, "value", 8); Set(card, "effects", effects);
        return card;
    }

    /// <summary>Creates a biome fixture with one valid starting tier.</summary>
    private static BiomeDefinition Biome(string id, int depth, CardDefinition[] pool)
    {
        BiomeDefinition biome = ScriptableObject.CreateInstance<BiomeDefinition>(); created.Add(biome);
        Set(biome, "biomeId", id); Set(biome, "displayName", id);
        BiomeDepthTierDefinition tier = new BiomeDepthTierDefinition();
        Set(tier, "minimumDepth", depth); Set(tier, "maximumDepth", depth + 10); Set(tier, "encounterPool", pool);
        Set(biome, "depthTiers", new[] { tier });
        return biome;
    }

    /// <summary>Creates an authored effect fixture without changing project definitions.</summary>
    private static CardEffectDefinition Effect(CardEffectType type, CardEffectTrigger trigger, CardEffectTarget target,
        int amount, CardEffectTone tone)
    {
        CardEffectDefinition effect = new CardEffectDefinition();
        Set(effect, "effectType", type); Set(effect, "trigger", trigger); Set(effect, "target", target);
        Set(effect, "amount", amount); Set(effect, "effectTone", tone);
        return effect;
    }

    /// <summary>Sets private authored fixture fields through their existing serialization contract.</summary>
    private static void Set(object owner, string name, object value)
    {
        owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(owner, value);
    }

    /// <summary>Reads a private runtime owner for action-level checks.</summary>
    private static T Get<T>(object owner, string name)
    {
        return (T)owner.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
    }

    /// <summary>Fails validation with a focused explanation.</summary>
    private static void Require(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new InvalidOperationException(message);
    }
}
