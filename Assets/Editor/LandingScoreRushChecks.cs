using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;

/// <summary>Checks exact score presentation, interruption cleanup and real terminal handoffs with persistence disabled.</summary>
public static class LandingScoreRushChecks
{
    public const string Output = "Docs/Validation/LandingScoreRush/";
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags Fixture = BindingFlags.Static | BindingFlags.NonPublic;
    public static string Result { get; private set; } = "Not run";
    private static int checks;

    /// <summary>Verifies score ordering, precision, bounded pacing and held-clock invariants independently of rendering.</summary>
    public static int ValidateRules()
    {
        checks = 0;
        var score = new LandingScore(LandingAccuracy.Perfect, 20, 30, 12, new[] {
            new LandingContribution(2,"Multiply first in rig",0,3), new LandingContribution(1,"Add",30,1),
            new LandingContribution(3,"Multiply",0,4), new LandingContribution(4,"Recovery",0,1,1) });
        var sequence = new LandingScoreSequence(score);
        Check(sequence.Steps.Select(s => s.Value).SequenceEqual(new decimal[] { 20, 50, 150, 600 }), "Incorrect canonical score progression.");
        Check(sequence.Steps[1].SourceId == 1 && sequence.Steps[2].SourceId == 2, "Multipliers ran before bonuses.");
        Check(sequence.Steps.Count == 4, "Attempt recovery became a scoring operation.");
        Check(sequence.StepAt(.01f) == 0 && sequence.StepAt(99) == 3, "Sequence time lookup failed.");
        Check(sequence.Duration < LandingScoreSequence.MaximumDuration && sequence.PayoutAt < sequence.Duration, "Combo feedback is not bounded.");
        var ordinary = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect,20,6,1.3m,new[] {new LandingContribution(1,"Add",6,1),new LandingContribution(2,"Multiply",0,1.3m)}));
        Check(ordinary.Steps.Last().Value == 33.8m, "Fractional score lost precision.");
        Check(ordinary.Duration < sequence.Duration && ordinary.Intensity < sequence.Intensity, "Ordinary hits do not feel distinct.");
        var miss = new LandingScoreSequence(new LandingScore(LandingAccuracy.Miss,0,0,1,Array.Empty<LandingContribution>()));
        Check(miss.Steps.Count == 1 && miss.Intensity == 0 && miss.Duration < 1, "An ordinary Miss earned a score celebration.");
        var many = Enumerable.Range(0, 150).Select(i => new LandingContribution(i,"Add",1,1)).ToArray();
        var longSequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Good,10,150,1,many));
        Check(longSequence.OperationCount == 150 && longSequence.Steps.Skip(1).Sum(s=>s.SourceIds.Count)==150 && longSequence.Steps.Last().Value == 160 && longSequence.Duration < LandingScoreSequence.MaximumDuration, "Long rigs truncate contributions or extend feedback indefinitely.");
        foreach (var step in sequence.Steps.Skip(1))
        {
            Check(step.Time < step.LaunchAt && step.LaunchAt < step.ImpactAt && step.ImpactAt < step.EndAt, "Missing anticipation, flight or recovery.");
            Check(sequence.ValueAt(step.ImpactAt-.0001f) != step.Value && sequence.ValueAt(step.ImpactAt)==step.Value,"Score did not change exactly on impact.");
        }
        var decimalItems = Enumerable.Range(0,12).Select(i=>new LandingContribution(i,"Fraction",.03m,1.07m)).ToArray();
        decimal product = 1; foreach(var item in decimalItems) product *= item.Multiplier;
        var precise = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect,20,.36m,product,decimalItems));
        Check(precise.Steps.Last().Value == precise.Score.Points,"Animation changed the authoritative decimal arithmetic order.");
        Check(LandingScoreRush.Format(600) == "600" && LandingScoreRush.Format(12000000) == "12M", "Large readout formatting failed.");
        var model = new LandingChallengeRuntime(); double clock = 0;
        model.Begin(1,100000,LandingChallengeChecks.Settings(),Array.Empty<LandingRigMember>(),new System.Random(1),clock);
        LandingChallengeChecks.Strike(model,LandingAccuracy.Perfect,ref clock);
        double fish = model.FishPosition, zone = model.ZonePosition; long revision = model.Revision; decimal points = model.Points; int attempts = model.AttemptsRemaining;
        model.HoldPresentationClock(clock + 2); model.AdvanceTo(clock + 2);
        Check(model.FishPosition == fish && model.ZonePosition == zone,"Held presentation advanced motion.");
        Check(model.Points == points && model.AttemptsRemaining == attempts && model.Revision == revision,"Presentation changed gameplay state.");
        model.AdvanceTo(clock + 2.4);
        Check(model.FishPosition != fish || model.ZonePosition != zone,"Motion did not resume after the hold.");
        ValidateModifierStrength();
        ValidateBoundaryHandoffs();
        ValidateStrikeWater();
        ValidateImpactTiming();
        return checks;
    }

    /// <summary>Checks gain-based visual hierarchy, exact delivery and the readable long-rig time budget.</summary>
    private static void ValidateModifierStrength()
    {
        var small = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 2, 1,
            new[] { new LandingContribution(1, "Small addition", 2, 1) }));
        var large = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 100, 1,
            new[] { new LandingContribution(1, "Large addition", 100, 1) }));
        Check(small.Steps[1].Strength < .1f && large.Steps[1].Strength > .4f, "Small and major bonuses share the same visual intensity.");
        Check(small.Steps[1].EndAt < large.Steps[1].EndAt, "Small bonuses do not resolve faster.");
        var tinyMultiplier = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 0, 1.01m,
            new[] { new LandingContribution(1, "Tiny multiplier", 0, 1.01m) }));
        var earlyMultiplier = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 0, 2,
            new[] { new LandingContribution(1, "Early multiplier", 0, 2) }));
        var lateMultiplier = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 100, 2,
            new[] { new LandingContribution(1, "Foundation", 100, 1), new LandingContribution(2, "Late multiplier", 0, 2) }));
        var late = lateMultiplier.Steps.Last();
        Check(tinyMultiplier.Steps[1].Strength < small.Steps[1].Strength, "A nominal multiplier fakes a major gain.");
        Check(late.Strength > earlyMultiplier.Steps[1].Strength, "The same factor ignores its actual delivered points.");
        Check(late.LaunchAt - late.Time > tinyMultiplier.Steps[1].LaunchAt - tinyMultiplier.Steps[1].Time, "A major multiplier lacks additional preparation.");
        Check(late.PreviousValue == 120 && late.Gain == 120 && late.Value == 240, "Operation metadata changed canonical arithmetic.");
        Check(lateMultiplier.ValueAt(late.ImpactAt - .0001f) == 120 && lateMultiplier.ValueAt(late.ImpactAt) == 240, "Weighted timing exposes multiplier points before arrival.");
        Check(Math.Abs(small.Steps[0].ImpactAt - .3016f) < .000001f && small.EntryAt == .60f && Math.Abs(small.Steps[1].Time - small.EntryAt - LandingScoreSequence.BoundaryTurnDuration) < .000001f, "Card collision lost its accuracy windup or entry transition.");
        var mixed = Enumerable.Range(0, 150).Select(i => new LandingContribution(i, "Grouped", 1, 1.02m)).ToArray();
        decimal product = 1; foreach (var item in mixed) product *= item.Multiplier;
        var longSequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 150, product, mixed));
        Check(longSequence.Duration < LandingScoreSequence.MaximumDuration && longSequence.Steps.Last().Value == longSequence.Score.Points, "Mixed long rigs break the bounded exact sequence.");
        Check(longSequence.Steps.Skip(1).Sum(s => s.SourceIds.Count) == 300, "Grouped delivery drops an operation/source identity.");
        Check(longSequence.Steps.Skip(1).All(s => s.EndAt - s.Time >= .49999f), "Tempo compression removes collision preparation or recovery.");
        Check(longSequence.Steps.Skip(1).All(s => s.Strength >= 0 && s.Strength <= 1 && s.Time < s.LaunchAt && s.LaunchAt < s.ImpactAt && s.ImpactAt < s.EndAt), "Weighted steps have invalid strength or event order.");
        var repeat = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 5, 2,
            new[] { new LandingContribution(4, "Same source", 5, 2) }));
        Check(LandingCardHandoff.DurationFor(repeat, 0) == 0 && LandingCardHandoff.DurationFor(repeat, 1) == 0, "Initial source repeats an ordinary turn after its boundary transition.");
        Check(LandingCardHandoff.DurationFor(repeat, 2) == 0, "Repeated source unnecessarily flips its card.");
        Check(Enumerable.Range(1, longSequence.Steps.Count - 1).All(i => LandingCardHandoff.DurationFor(longSequence, i) <= .14f && longSequence.Steps[i].Time + LandingCardHandoff.DurationFor(longSequence, i) < longSequence.Steps[i].LaunchAt), "Long-rig handoffs consume dash time.");
    }

    /// <summary>Checks three-turn encounter boundaries, single-turn contributor changes and bounded dense-rig schedules.</summary>
    private static void ValidateBoundaryHandoffs()
    {
        var sequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 5, 2,
            new[] { new LandingContribution(4, "Add", 5, 1), new LandingContribution(8, "Multiply", 0, 2) }));
        float duration = LandingScoreSequence.BoundaryTurnDuration;
        foreach (float start in new[] { sequence.EntryAt, sequence.ReturnAt })
        {
            int outgoing = start == sequence.EntryAt ? 0 : sequence.Steps.Count - 1;
            int incoming = start == sequence.EntryAt ? 1 : 0;
            var before = LandingCardHandoff.Sample(sequence, start + duration * .25f);
            var middle = LandingCardHandoff.Sample(sequence, start + duration * .5f);
            var after = LandingCardHandoff.Sample(sequence, start + duration * .75f);
            Check(before.Active && before.Turns == 3 && before.VisibleStep == outgoing, "Boundary loses its outgoing role or three turns.");
            Check(middle.Active && middle.VisibleStep == incoming && LandingCardHandoff.FaceOpacity(middle.FacePhase, false) == 0, "Boundary identity swaps outside the hidden middle turn.");
            Check(after.VisibleStep == incoming && LandingCardHandoff.Sample(sequence, start + duration + .00001f).VisibleStep == incoming, "Boundary reverts its revealed identity.");
            Check(sequence.ValueAt(start) == sequence.ValueAt(start + duration - .00001f), "A boundary scores points without a collision.");
        }
        var between = LandingCardHandoff.Sample(sequence, sequence.Steps[2].Time + .03f);
        Check(between.Active && between.Turns == 1 && between.VisibleStep == 1, "Ordinary attackers use the encounter transition.");
        Check(sequence.ReturnAt == sequence.Steps.Last().EndAt && sequence.PayoutAt >= sequence.ReturnAt + duration, "Return truncates the last rebound or overlaps payout.");
        var ordinary = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 0, 1, Array.Empty<LandingContribution>()));
        Check(!ordinary.HasContributors && !LandingCardHandoff.Sample(ordinary, ordinary.EntryAt).Active && ordinary.Duration < 2, "An empty rig receives encounter/attacker turns.");
        var uneven = new[] { new LandingContribution(20, "Foundation", 1, 1) }
            .Concat(Enumerable.Range(0, 15).Select(i => new LandingContribution(i, "Multiply", 0, 2))).ToArray();
        var worst = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 1, 32768, uneven));
        Check(worst.Steps.Count == 10 && worst.Intensity == 1 && worst.Duration < LandingScoreSequence.MaximumDuration
            && worst.Steps.Skip(1).All(s => s.EndAt - s.Time >= .49999f), "Nine grouped beats plus both three-turn boundaries exceed the timing budget.");
        for (int count = 1; count <= 150; count++)
        {
            var contributions = Enumerable.Range(0, count).Select(i => new LandingContribution(i, "Dense", 1, 1.01m)).ToArray();
            decimal product = 1; foreach (var item in contributions) product *= item.Multiplier;
            var dense = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, count, product, contributions));
            Check(dense.Duration < LandingScoreSequence.MaximumDuration && dense.Steps.Skip(1).All(s => s.EndAt - s.Time >= .49999f), "Boundary turns exceed the feedback bound or remove readable collisions.");
        }
    }

    /// <summary>Checks cumulative per-strike fill, the half-Resistance threshold and one bounded final drain.</summary>
    private static void ValidateStrikeWater()
    {
        Check(LandingStrikeWater.DamageLevel(25, 100) == .5f && LandingStrikeWater.DamageLevel(50, 100) == 1 && LandingStrikeWater.DamageLevel(51, 100) == 1, "Glass does not fill at half Resistance.");
        Check(LandingStrikeWater.DamageLevel(decimal.MaxValue, 1) == 1 && LandingStrikeWater.DamageLevel(decimal.MaxValue, decimal.MaxValue) == 1, "Overkill overflows the visual ratio.");
        Check(LandingStrikeWater.DamageLevel(0, 100) == 0 && LandingStrikeWater.DamageLevel(-1, 100) == 0 && LandingStrikeWater.DamageLevel(20, 0) == 0, "Empty or invalid damage leaves water.");
        var sequence = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 10, 2,
            new[] { new LandingContribution(1, "Add", 10, 1), new LandingContribution(2, "Multiply", 0, 2) }));
        var first = sequence.Steps[0]; var add = sequence.Steps[1]; var last = sequence.Steps[2];
        Check(LandingStrikeWater.LevelAt(sequence, 100, first.ImpactAt - .0001f, false) == 0, "Water rises before contact.");
        Check(Math.Abs(LandingStrikeWater.LevelAt(sequence, 100, add.ImpactAt - .0001f, false) - .4f) < .00001f, "Water anticipates the next contribution.");
        Check(Math.Abs(LandingStrikeWater.LevelAt(sequence, 100, add.ImpactAt + .075f, false) - .6f) < .00001f, "Running subtotals double-count prior damage.");
        Check(LandingStrikeWater.LevelAt(sequence, 100, last.ImpactAt + .075f, false) == 1, "Final multiplier does not finish filling the glass.");
        Check(Math.Abs(LandingStrikeWater.LevelAt(sequence, 100, last.ImpactAt + .08f + LandingStrikeWater.DrainSeconds * .5f, false) - .5f) < .00001f, "Water does not drain over two-thirds of a second.");
        Check(LandingStrikeWater.LevelAt(sequence, 100, last.ImpactAt + .08f + LandingStrikeWater.DrainSeconds + .001f, false) == 0, "Water persists after the drain.");
        Check(LandingStrikeWater.LevelAt(sequence, 100, last.ImpactAt, true) == 1, "Reduced motion loses the delivered result.");
        var ordinary = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 0, 1, Array.Empty<LandingContribution>()));
        Check(LandingStrikeWater.LevelAt(ordinary, 20, ordinary.Duration - .0001f, false) == 0, "Ordinary feedback ends before its drain can finish.");
        var miss = new LandingScoreSequence(new LandingScore(LandingAccuracy.Miss, 0, 0, 1, Array.Empty<LandingContribution>()));
        Check(LandingStrikeWater.LevelAt(miss, 100, .3f, false) == 0, "An ordinary Miss fills the glass.");
    }

    /// <summary>Checks final-only approach/contact/rebound slow motion, reversible clocks and the shared controller timeout bound.</summary>
    private static void ValidateImpactTiming()
    {
        var score = new LandingScore(LandingAccuracy.Perfect, 20, 200, 50,
            new[] { new LandingContribution(1, "Add", 200, 1), new LandingContribution(2, "Multiply", 0, 5), new LandingContribution(3, "Multiply", 0, 10) });
        var sequence = new LandingScoreSequence(score); var timing = new LandingImpactTiming(sequence);
        Check(timing.FocusCount == 1 && timing.Focuses(sequence.Steps.Count - 1) && !timing.Focuses(1) && !timing.Focuses(2) && timing.Duration > sequence.Duration && timing.Duration < LandingScoreSequence.MaximumDuration, "Major hits lack bounded emphasis.");
        foreach (int index in Enumerable.Range(0, sequence.Steps.Count))
        {
            var hit = sequence.Steps[index]; float hold = LandingImpactTiming.ContactHold(hit);
            Check(Math.Abs(timing.ToSequence(timing.ToReal(hit.ImpactAt + hold)) - hit.ImpactAt - hold) < .00001f, "Clock loses the start of return.");
            if (timing.Focuses(index))
            {
                float start = LandingImpactTiming.SlowStart(hit), end = LandingImpactTiming.SlowEnd(hit);
                Check(start > hit.LaunchAt && start < hit.ImpactAt && end > hit.ImpactAt + hold && end < hit.EndAt, "Cinematic window misses approach or initial rebound.");
                Check(Math.Abs(timing.ToReal(start) - start) < .00001f, "Final-hit slow motion changes an earlier action or its launch.");
                Check(timing.ToReal(hit.ImpactAt) - timing.ToReal(start) > (hit.ImpactAt - start) * 3, "Approach does not slow before impact.");
                Check(timing.ToReal(hit.ImpactAt + hold) - timing.ToReal(hit.ImpactAt) > hold * 3, "Major contact hold ends immediately instead of lingering.");
                Check(timing.ToReal(end) - timing.ToReal(hit.ImpactAt + hold) > (end - hit.ImpactAt - hold) * 3, "Slow motion ends before the first rebound.");
                Check(Math.Abs((timing.ToReal(hit.EndAt) - timing.ToReal(end)) - (hit.EndAt - end)) < .00001f, "Normal recovery retains cinematic pacing.");
            }
            else
            {
                Check(Math.Abs((timing.ToReal(hit.ImpactAt) - timing.ToReal(hit.LaunchAt)) - (hit.ImpactAt - hit.LaunchAt)) < .00001f, "An earlier or ordinary dash slows down.");
                Check(Math.Abs((timing.ToReal(hit.EndAt) - timing.ToReal(hit.ImpactAt + hold)) - (hit.EndAt - hit.ImpactAt - hold)) < .00001f, "An earlier or ordinary return slows down.");
            }
        }
        Check(Enumerable.Range(0, 301).All(i => Math.Abs(timing.ToSequence(timing.ToReal(sequence.Duration * i / 300f)) - sequence.Duration * i / 300f) < .00001f), "Presentation clocks cannot seek or skip reliably.");
        var ordinary = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 2, 1, new[] { new LandingContribution(1, "Small", 2, 1) }));
        var plain = new LandingImpactTiming(ordinary);
        Check(plain.FocusCount == 0 && plain.Duration == ordinary.Duration && plain.ToSequence(1) == 1, "Ordinary hits receive cinematic pacing.");
        var earlyMajor = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 10000, 1.0001m,
            new[] { new LandingContribution(1, "Large earlier hit", 10000, 1), new LandingContribution(2, "Small final hit", 0, 1.0001m) }));
        Check(earlyMajor.Steps[1].Strength >= .75f && new LandingImpactTiming(earlyMajor).FocusCount == 0
            && new LandingImpactTiming(earlyMajor).Duration == earlyMajor.Duration, "An earlier major hit substitutes for a weak final impact.");
        var singleMajor = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 2000, 1,
            new[] { new LandingContribution(1, "Only creature", 2000, 1) }));
        Check(new LandingImpactTiming(singleMajor).Focuses(1), "A single final creature cannot trigger the effect.");
        var accuracyOnly = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 2000, 0, 1, Array.Empty<LandingContribution>()));
        Check(new LandingImpactTiming(accuracyOnly).FocusCount == 0, "Accuracy-only damage triggers a creature close-up.");
        var uneven = new[] { new LandingContribution(20, "Foundation", 1, 1) }.Concat(Enumerable.Range(0, 15).Select(i => new LandingContribution(i, "Multiply", 0, 2))).ToArray();
        var dense = new LandingScoreSequence(new LandingScore(LandingAccuracy.Perfect, 20, 1, 32768, uneven));
        Check(new LandingImpactTiming(dense).Duration < LandingScoreSequence.MaximumDuration, "Slow motion exceeds the controller gate for dense rigs.");
    }

    /// <summary>Verifies serialized UI, audio routing and non-intercepting feedback on the actual saved prefab.</summary>
    public static int ValidateAssets()
    {
        checks=0; var root=AssetDatabase.LoadAssetAtPath<GameObject>(LandingChallengeSetup.PrefabPath);
        var host=root.GetComponent<LandingScoreRush>(); Check(host!=null,"Missing score-rush component.");var data=new SerializedObject(host);
        foreach(string field in new[]{"readout","value","operation","caption","scoringPortrait","collision","waterImpact","strikeWater","impactCamera","glow","machine","totalLabel","particles","feedback","soundToggle","reducedMotionToggle","hit","addition","multiplier","payoutSmall","payoutBig","miss"})
            Check(data.FindProperty(field).objectReferenceValue!=null,"Missing score rush binding: "+field);
        var group=(CanvasGroup)data.FindProperty("readout").objectReferenceValue;
        var portraitOwner=new SerializedObject((LandingScoringPortrait)data.FindProperty("scoringPortrait").objectReferenceValue);
        foreach(string field in new[]{"portrait","creatureName","tags"}) Check(portraitOwner.FindProperty(field).objectReferenceValue!=null,"Missing main-slot binding: "+field);
        Check(portraitOwner.FindProperty("portrait").objectReferenceValue==new SerializedObject(root.GetComponent<TackleLandingPresentation>()).FindProperty("portrait").objectReferenceValue,"Contributors do not use the encounter portrait slot.");
        var oldIcon=group.transform.Find("Active source");Check(oldIcon==null||!oldIcon.gameObject.activeSelf,"The old score icon is still visible.");
        Check(!group.blocksRaycasts&&!group.interactable&&!group.gameObject.activeSelf,"Transient readout starts visible or blocks input.");
        Check(!((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).raycastTarget,"Trails block input.");
        Check(((LandingRushGraphic)data.FindProperty("particles").objectReferenceValue).GetComponent<CanvasRenderer>()!=null,"Trails have no native canvas renderer.");
        var collision = new SerializedObject(root.GetComponent<LandingCardCollision>());
        foreach (string field in new[] { "creatureCard", "scoreCard", "stage", "creatureFade" }) Check(collision.FindProperty(field).objectReferenceValue != null, "Missing collision binding: " + field);
        Check(root.GetComponentsInChildren<LandingCollisionGraphic>(true).Length == 3, "Missing source-preserving housing/card graphics.");
        Check(root.GetComponentsInChildren<LandingCollisionGraphic>(true).All(g => !g.raycastTarget), "Rigid card graphics intercept input.");
        ValidatePanelArtwork(root);
        var water = new SerializedObject(root.GetComponent<LandingWaterImpact>());
        foreach (string field in new[] { "spray", "number", "collision" }) Check(water.FindProperty(field).objectReferenceValue != null, "Missing water impact binding: " + field);
        var spray = (UnityEngine.UI.RawImage)water.FindProperty("spray").objectReferenceValue;
        Check(!spray.enabled && !spray.raycastTarget && water.FindProperty("number").objectReferenceValue == ((TMP_Text)data.FindProperty("value").objectReferenceValue).rectTransform, "Water starts visible, intercepts input or pulses another number.");
        var sheets = water.FindProperty("sheets"); Check(sheets.arraySize == 3, "Water lacks its three authored strength tiers.");
        for (int i = 0; i < sheets.arraySize; i++)
        {
            var sheet = (Texture2D)sheets.GetArrayElementAtIndex(i).objectReferenceValue;
            Check(sheet != null && sheet.filterMode == FilterMode.Point && sheet.wrapMode == TextureWrapMode.Clamp && sheet.mipmapCount == 1, "Water atlas loses crisp sampling or readiness.");
            Check(sheet.width == water.FindProperty("columns").intValue * 96 && sheet.height == water.FindProperty("rows").intValue * 144, "Water atlas metadata and texture dimensions disagree.");
        }
        var gauge = (LandingStrikeWater)data.FindProperty("strikeWater").objectReferenceValue;
        var track = (RectTransform)new SerializedObject(root.GetComponent<TackleLandingPresentation>()).FindProperty("track").objectReferenceValue;
        Check(gauge.transform.parent == track && gauge.transform.GetSiblingIndex() == 0 && track.GetComponent<UnityEngine.UI.RectMask2D>() != null, "Strike water is not masked underneath the aiming indicators.");
        Check(!gauge.raycastTarget && gauge.Level == 0, "Strike water intercepts input or starts filled.");
        var waterBounds = gauge.rectTransform.rect;
        Check(gauge.rectTransform.offsetMin == Vector2.one * 4 && gauge.rectTransform.offsetMax == Vector2.one * -4, "Water lacks clearance from the glass frame.");
        Check(!gauge.ContainsGlassPoint(new Vector2(waterBounds.xMin + 1, waterBounds.yMax - 1))
            && !gauge.ContainsGlassPoint(new Vector2(waterBounds.xMax - 1, waterBounds.yMin + 1))
            && gauge.ContainsGlassPoint(waterBounds.center), "Water uses rectangular frame corners.");
        var cameraView = new SerializedObject(root.GetComponent<LandingImpactCamera>());
        foreach (string field in new[] { "stage", "creatureCard", "scoreCard" }) Check(cameraView.FindProperty(field).objectReferenceValue != null, "Missing impact camera binding: " + field);
        var voices=data.FindProperty("voices");Check(voices.arraySize>=4,"No bounded audio voice pool.");
        for(int i=0;i<voices.arraySize;i++)
        {
            var source=(AudioSource)voices.GetArrayElementAtIndex(i).objectReferenceValue;
            Check(source!=null&&!source.playOnAwake&&source.spatialBlend==0&&source.outputAudioMixerGroup!=null,"Voice missing mixer routing or plays unexpectedly.");
        }
        foreach(string cue in new[]{"hit","addition","multiplier","payoutSmall","payoutBig","miss"})
        {
            var clip=(AudioClip)data.FindProperty(cue).objectReferenceValue;var samples=new float[clip.samples];clip.GetData(samples,0);
            Check(samples.Any(s=>Mathf.Abs(s)>.01f)&&samples.All(s=>Mathf.Abs(s)<=.86f),"Silent or clipped cue: "+cue);
        }
        return checks;
    }

    /// <summary>Checks clean movable silhouettes, exact source-pixel registration and unchanged authored collision bounds.</summary>
    private static void ValidatePanelArtwork(GameObject root)
    {
        var stage = root.transform.Find("1920x1080 Stage");
        var master = AssetDatabase.LoadAssetAtPath<Sprite>(TackleLandingArtSetup.HousingPath);
        string[] names = { "Creature collision card", "Score collision card" };
        string[] paths = { LandingPanelArtworkSetup.CreaturePath, LandingPanelArtworkSetup.ScorePath };
        Rect[] regions = { LandingCollisionGraphic.CreatureRegion, LandingCollisionGraphic.ScoreRegion };
        for (int i = 0; i < names.Length; i++)
        {
            var graphic = stage.Find(names[i]).GetComponent<LandingCollisionGraphic>();
            var data = new SerializedObject(graphic);
            var art = (LandingPanelArtwork)data.FindProperty("panel").objectReferenceValue;
            Check(art != null && AssetDatabase.GetAssetPath(art) == paths[i] && art.Face != null, "A moving panel lacks independent artwork.");
            Check(data.FindProperty("source").objectReferenceValue == null && art.Face != master && art.Face.texture == master.texture
                && graphic.mainTexture == master.texture && AssetDatabase.GetAssetPath(art.Face) == paths[i], "Panel artwork duplicates or repaints the source texture.");
            var rect = graphic.rectTransform;
            Check(rect.sizeDelta == regions[i].size && rect.localScale == Vector3.one * .88f
                && rect.anchoredPosition == new Vector2(regions[i].center.x + (i == 0 ? -12 : 12), -regions[i].center.y), "Separating artwork moved or resized a collision panel.");
            Check(art.VertexCount == 8, "A moving panel retains a rectangular crop.");
            var mesh = new UnityEngine.UI.VertexHelper();
            try
            {
                typeof(LandingCollisionGraphic).GetMethod("OnPopulateMesh", Hidden | BindingFlags.DeclaredOnly).Invoke(graphic, new object[] { mesh });
                Check(mesh.currentVertCount == 8 && mesh.currentIndexCount == 18, "Panel renderer does not use its beveled silhouette.");
                Rect shell = TackleMachineGeometry.StageBounds; var vertex = new UIVertex(); bool registered = true, convex = true;
                for (int v = 0; v < art.VertexCount; v++)
                {
                    Vector2 p = art.Point(v), next = art.Point((v + 1) % art.VertexCount), after = art.Point((v + 2) % art.VertexCount);
                    Vector2 a = next - p, b = after - next;
                    convex &= a.x * b.y - a.y * b.x < 0 && p.x >= 0 && p.x <= 1 && p.y >= 0 && p.y <= 1;
                    mesh.PopulateUIVertex(ref vertex, v);
                    Vector2 expected = new Vector2((master.rect.x + (regions[i].x + p.x * regions[i].width - shell.x) / shell.width * master.rect.width) / master.texture.width,
                        (master.rect.y + (1 - (regions[i].yMax - p.y * regions[i].height - shell.y) / shell.height) * master.rect.height) / master.texture.height);
                    registered &= ((Vector2)vertex.uv0 - expected).sqrMagnitude < .0000000001f;
                }
                Check(convex, "Panel silhouette folds over itself or extends outside the preserved bounds.");
                Check(registered, "Independent panel artwork drifts from the original source pixels.");
            }
            finally { mesh.Dispose(); }
        }
    }

    /// <summary>Starts an isolated run using actual gameplay commands and installed creature abilities.</summary>
    public static void Begin()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter a fresh comparison Play session.");
        Result="Running";checks=0;var owner=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>().Controller;
        Directory.CreateDirectory(Output);File.WriteAllText(Output+"LiveVerification.json","{\"passed\":false,\"running\":true}");
        owner.SetDebugSaveResetPending(true);Application.runInBackground=true;owner.StartCoroutine(Run());
    }

    /// <summary>Exercises source motion, sound, skip, new placement, duplicates and immediate full-value failure settlement.</summary>
    private static IEnumerator Run()
    {
        Directory.CreateDirectory(Output);var harbor=UnityEngine.Object.FindAnyObjectByType<JournalHarborFlow>();var owner=harbor.Controller;
        owner.TryPrepareSlotTags(new[]{"Armored","Predator","Schooling","Anchored"});harbor.TryStartNewRun();
        var prep=new SerializedObject(harbor.SlotPreparation);
        ((UnityEngine.UI.Button)prep.FindProperty("confirm").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Glass Minnow","Mackerel","Cuttlefish","Peacock Worm","Squid","Cleaner Shrimp","Sea Sponge","Moonlit Ray","Flounder"})
            Check(owner.Rig.TryAcquire(owner.Rig.Revision,Card(name),owner.Rig.Members.Length,out _,out _),"Could not seed source fixture.");
        Force(owner,Card("Cod"));yield return Accept(owner);
        var landing=UnityEngine.Object.FindAnyObjectByType<TackleLandingPresentation>();var rush=landing.GetComponent<LandingScoreRush>();var data=new SerializedObject(rush);
        var reduced=(UnityEngine.UI.Toggle)data.FindProperty("reducedMotionToggle").objectReferenceValue;
        var sound=(UnityEngine.UI.Toggle)data.FindProperty("soundToggle").objectReferenceValue;
        reduced.SetIsOnWithoutNotify(false);sound.SetIsOnWithoutNotify(true);
        yield return Strike(owner,LandingAccuracy.Perfect);
        var receipt=owner.LastLandingStrike;Check(owner.LandingFeedbackPending&&rush.IsPlaying,"Accepted strike did not start a sequence.");
        Check(rush.Sequence.Steps.Count>=3,"Fixture lacks an addition and multiplier.");
        decimal points=owner.Landing.Points;long revision=owner.Landing.Revision;int attempts=owner.Landing.AttemptsRemaining;
        Check(!owner.TryStrikeLanding(owner.Landing.Occurrence,revision),"Duplicate strike accepted during animation.");
        Check(!owner.TryCommitRigPlacement(owner.BuildOccurrence,owner.Rig.Revision,0),"Placement accepted during count-up.");
        var cards=landing.GetComponentsInChildren<TackleRigCardView>();
        var scoringPortrait=landing.GetComponent<LandingScoringPortrait>();var portraitData=new SerializedObject(scoringPortrait);
        var mainPortrait=(UnityEngine.UI.Image)portraitData.FindProperty("portrait").objectReferenceValue;
        var mainName=(TMP_Text)portraitData.FindProperty("creatureName").objectReferenceValue;
        var mainTags=(TMP_Text)portraitData.FindProperty("tags").objectReferenceValue;
        Sprite targetPortrait=mainPortrait.sprite;string targetName=mainName.text,targetTags=mainTags.text;
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps[1].ImpactAt+.015f});
        var firstSource=receipt.Members.Single(m=>m.InstanceId==rush.Sequence.Steps[1].SourceId);
        Check(scoringPortrait.IsShowingContributors&&mainPortrait.sprite==(firstSource.Definition.EncounterArtwork??firstSource.Definition.Artwork)&&mainName.text==firstSource.Definition.DisplayName.ToUpperInvariant(),"Contributor did not replace the encounter portrait and name.");
        Check(mainTags.text.StartsWith("RIG EFFECT"),"Contributor is not distinguished from the encounter target.");
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.PayoutAt});
        Check(!scoringPortrait.IsShowingContributors&&mainPortrait.sprite==targetPortrait&&mainName.text==targetName&&mainTags.text==targetTags,"Payout did not restore the original encounter identity.");
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps[1].ImpactAt+.015f});
        Check(((CanvasGroup)data.FindProperty("readout").objectReferenceValue).alpha>0,"Growing readout is invisible.");
        Check(landing.GetComponentsInChildren<AudioSource>().Any(s=>s.isPlaying),"Musical step did not start a routed audio voice.");
        foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,800),new Vector2Int(2560,1080)})
            Capture(landing,"Combo-"+size.x+"x"+size.y+".png",size.x,size.y);
        var outside=receipt.Score.Contributions.FirstOrDefault(c=>cards.Any(v=>v.BoundInstance.InstanceId==c.SourceId&&v.transform.position.y<cards[0].transform.position.y-600));
        Check(outside!=null,"Fixture lacks an offscreen source.");
        int outsideIndex=Array.FindIndex(rush.Sequence.Steps.ToArray(),s=>s.SourceId==outside.SourceId);
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps[outsideIndex].Time+.1f});
        Check(scoringPortrait.IsShowingContributors&&mainPortrait.enabled,"Offscreen contributor did not appear in the main slot.");
        Capture(landing,"Offscreen-Source.png",1920,1080);
        reduced.SetIsOnWithoutNotify(true);rush.Tick();
        Check(rush.ReducedMotion,"Reduced-motion choice was ignored.");
        Check(owner.Landing.Points==points&&owner.Landing.Revision==revision&&owner.Landing.AttemptsRemaining==attempts,"Presentation reran scoring or attempts.");
        owner.CompleteLandingFeedback();yield return null;
        Check(!rush.IsPlaying,"Skip left animation running.");
        Check(!scoringPortrait.IsShowingContributors&&mainPortrait.sprite==targetPortrait&&mainName.text==targetName&&mainTags.text==targetTags,"Skip left a contributor in the encounter slot.");
        Check(landing.GetComponentsInChildren<AudioSource>().All(s=>!s.isPlaying),"Skip left audio playing.");
        Check(cards.All(c=>c.transform.localScale==Vector3.one&&c.transform.localRotation==Quaternion.identity),"Card transforms did not return to rest.");
        if(owner.Landing.State==LandingChallengeState.Active)yield return Land(owner,true);
        else yield return WaitFeedback(owner);
        var build=UnityEngine.Object.FindAnyObjectByType<RigBuildingView>();
        Check(build!=null&&owner.AwaitingRigPlacement&&!landing.gameObject.activeInHierarchy,"Winning score did not hand off exclusively to Build.");
        var buildData=new SerializedObject(build);((UnityEngine.UI.Button)buildData.FindProperty("commit").objectReferenceValue).onClick.Invoke();yield return null;
        foreach(string name in new[]{"Sardine","Cuttlefish","Stormjaw Tuna"})
            for(int i=0;i<29;i++) Check(owner.Rig.TryAcquire(owner.Rig.Revision,Card(name),owner.Rig.Members.Length,out _,out _),"Could not upgrade the big-score fixture.");
        Force(owner,Card("Sardine"));yield return Accept(owner);int priorLevel=owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level;
        reduced.SetIsOnWithoutNotify(false);yield return Strike(owner,LandingAccuracy.Perfect);
        Check(owner.Landing.State==LandingChallengeState.Succeeded&&owner.LandingFeedbackPending,"Winning strike did not resolve immediately with feedback pending.");
        Check(owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level==priorLevel,"Duplicate upgraded before its score was presented.");
        Check(rush.Sequence.Intensity>.8f&&rush.Sequence.Score.Points>500,"Big-score fixture did not amplify the result.");
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.Steps.Last().Time+.05f});
        Capture(landing,"Big-Combo.png",1920,1080);
        typeof(LandingScoreRush).GetMethod("Render",Hidden).Invoke(rush,new object[]{rush.Sequence.PayoutAt+.12f});
        Canvas.ForceUpdateCanvases();
        var sparks=(LandingRushGraphic)data.FindProperty("particles").objectReferenceValue;
        Check(sparks.canvasRenderer.GetMesh()==null||sparks.canvasRenderer.GetMesh().vertexCount==0,"Collision-only feedback retained payout particles.");
        Capture(landing,"Big-Payout.png",1920,1080);
        yield return WaitFeedback(owner);yield return null;
        Check(owner.Rig.FindSpecies(Card("Sardine").UniqueId).Level==priorLevel+1&&owner.RigUpgradeFeedbackVisible,"Duplicate did not upgrade automatically after the payout.");
        Check(owner.LastLandingStrike.Members.Single(m=>m.Definition==Card("Sardine")).Level==priorLevel,"Winning receipt was rewritten by upgrade.");
        double deadline=Time.realtimeSinceStartupAsDouble+5;while(owner.RigUpgradeFeedbackVisible&&Time.realtimeSinceStartupAsDouble<deadline)yield return null;
        Force(owner,Card("Cod"));yield return Accept(owner);int gold=owner.TotalGold,haul=owner.Rig.HaulValue;
        while(owner.Landing.State==LandingChallengeState.Active){yield return Strike(owner,LandingAccuracy.Miss);if(owner.RunActive)yield return WaitFeedback(owner);}
        Check(!owner.RunActive&&owner.LandingFeedbackPending&&owner.Rig.Members.Length==0,"Failure did not resolve immediately while its feedback remained visible.");
        Check(owner.TotalGold==gold+haul&&owner.LastHaulValue==haul,"Failure changed the full haul payout.");
        Check(owner.LastLandingStrike.Members.Count>0&&landing.GetComponentsInChildren<TackleRigCardView>().Length>0,"Failure lost frozen source cards before feedback completed.");
        Capture(landing,"Failure-Feedback.png",1920,1080);yield return WaitFeedback(owner);
        Check(owner.TotalGold==gold+haul&&!owner.LandingFeedbackPending,"Feedback repeated settlement or failed to finish.");
        Check(!landing.GetComponentsInChildren<LandingActivationView>().Any()&&!rush.IsPlaying,"Finished feedback retained transient effects.");
        Capture(landing,"Failure-Settled.png",1920,1080);
        Sprite finalTarget=mainPortrait.sprite;string finalTargetName=mainName.text;
        scoringPortrait.CaptureEncounter();scoringPortrait.ShowContributors(receipt.Members.Take(2).ToArray());
        Check(!mainPortrait.enabled&&mainPortrait.GetComponentsInChildren<UnityEngine.UI.Image>().Count(i=>i!=mainPortrait&&i.gameObject.activeSelf)==2,"Grouped beat did not show every contributor in the main slot.");
        Check(mainPortrait.GetComponentsInChildren<UnityEngine.UI.Image>().Where(i=>i!=mainPortrait).All(i=>!i.raycastTarget),"Grouped portraits intercept input.");
        scoringPortrait.enabled=false;
        Check(!scoringPortrait.IsShowingContributors&&mainPortrait.sprite==finalTarget&&mainName.text==finalTargetName,"Disabling portrait presentation did not restore the encounter.");
        scoringPortrait.enabled=true;
        Result="Passed "+checks+" live score-rush checks";File.WriteAllText(Output+"LiveVerification.json","{\"passed\":true,\"assertions\":"+checks+"}");Debug.Log(Result);
    }
    /// <summary>Waits for a robust observed alignment and uses the guarded real strike command.</summary>
    private static IEnumerator Strike(FishingRunController owner,LandingAccuracy accuracy)
    {
        double deadline=Time.realtimeSinceStartupAsDouble+30;long revision=owner.Landing.Revision;
        while(owner.Landing.Revision==revision&&Time.realtimeSinceStartupAsDouble<deadline)
        {
            var model=owner.Landing;
            if(!owner.LandingFeedbackPending)model.AdvanceTo(Time.realtimeSinceStartupAsDouble);
            double distance=Math.Abs(model.FishPosition-model.ZonePosition);
            if(!owner.LandingFeedbackPending&&model.CanStrike&&(accuracy==LandingAccuracy.Perfect?distance<(model.PerfectWidth+model.FishWidth)*.5-.01:distance>(model.GoodWidth+model.FishWidth)*.5+.05))owner.TryStrikeLanding(model.Occurrence,model.Revision);
            if(owner.Landing.Revision==revision)yield return null;
        }
        Check(owner.Landing.Revision>revision,"Strike alignment timed out.");
    }
    /// <summary>Waits on the bounded controller gate rather than guessing animation duration.</summary>
    private static IEnumerator WaitFeedback(FishingRunController owner)
    { double until=Time.realtimeSinceStartupAsDouble+LandingScoreSequence.MaximumDuration+.5;while(owner.LandingFeedbackPending&&Time.realtimeSinceStartupAsDouble<until)yield return null;Check(!owner.LandingFeedbackPending,"Feedback gate timed out."); }
    /// <summary>Uses existing end-to-end fixture logic for the remaining successful attempts.</summary>
    private static IEnumerator Land(FishingRunController owner,bool success)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Land",Fixture).Invoke(null,new object[]{owner,success});
    /// <summary>Accepts the real Discover result after its cosmetic reels settle.</summary>
    private static IEnumerator Accept(FishingRunController owner)=>(IEnumerator)typeof(RigBuildingSceneChecks).GetMethod("Accept",Fixture).Invoke(null,new object[]{owner});
    /// <summary>Constrains only disposable encounter selection, preserving real ability and scoring data.</summary>
    private static void Force(FishingRunController owner,CardDefinition card)=>typeof(RigBuildingSceneChecks).GetMethod("ForceNextCreature",Fixture).Invoke(null,new object[]{owner,card});
    /// <summary>Loads installed species without editing their definitions.</summary>
    private static CardDefinition Card(string name)=>AssetDatabase.FindAssets("t:CardDefinition",new[]{"Assets/Cards"}).Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).Single(c=>c.DisplayName==name);
    /// <summary>Captures the actual native Landing canvas using the project render pipeline.</summary>
    private static void Capture(TackleLandingPresentation landing,string name,int width,int height){Canvas.ForceUpdateCanvases();SlotMachineSceneChecks.Capture(landing.GetComponent<Canvas>(),name,width,height,Output);}
    /// <summary>Retains an actionable status if a live invariant fails.</summary>
    private static void Check(bool valid,string message){if(!valid){Result="FAILED: "+message;throw new InvalidOperationException(message);}checks++;}
}
