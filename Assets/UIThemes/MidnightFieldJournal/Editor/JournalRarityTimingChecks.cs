using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Validates eligibility, delayed reveals, event bypass and real candidate-scene interaction.</summary>
public static class JournalRarityTimingChecks
{
    private static string output="Docs/Validation/RarityTiming/";
    private static int assertions;
    public static string Status {get;private set;}="Not started";
    private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;

    /// <summary>Exercises timing and authoritative encounter state without modifying authored cards.</summary>
    public static int ValidateRules()
    {
        assertions=0;var profile=ScriptableObject.CreateInstance<RarityTimingProfile>();
        var cards=new List<CardDefinition>();
        try
        {
            for(int i=0;i<4;i++) {var card=ScriptableObject.CreateInstance<CardDefinition>();Set(card,"cardType",CardType.Creature);Set(card,"rarity",(CardRarity)i);Set(card,"displayName","Fixture "+i);cards.Add(card);}
            var random=new System.Random(5);var model=new EncounterRarityTiming();
            model.Begin(cards.ToArray(),profile,false);
            Assert(model.Zones.Count==3,"Legendary unlocked without configured width");
            Assert(Mathf.Abs(model.Zones[0].End-.87f)<.0001f,"Initial relative widths changed");
            Assert(Mathf.Abs(model.Zones[1].End-model.Zones[1].Start-.05f)<.0001f,"Uncommon must occupy five percent");
            model.Begin(new[]{cards[0],cards[1]},profile,false);
            Assert(Mathf.Abs(model.Zones[1].End-model.Zones[1].Start-.05f)<.0001f,"Missing Rare expanded Uncommon beyond five percent");
            for(int i=0;i<3;i++)
            {
                model.Begin(cards.ToArray(),profile,false);var zone=model.Zones[i];
                model.Tick((zone.Start+zone.End)*.5f/profile.traversalsPerSecond);
                long token=model.Occurrence;Assert(!model.TryStop(token-1),"Stale Stop accepted");
                Assert(model.TryStop(token),"Stop rejected");Assert(!model.TryStop(token),"Repeated Stop accepted");
                Assert(model.Outcome.Rarity==(CardRarity)i,"Wrong interval selected");
                Assert((int)model.Choose(random).Rarity<=i,"Sample escaped selected rarity");
                Assert(!model.ReadyToReveal,"Result hold skipped");model.Tick(profile.resultHoldSeconds);Assert(model.ReadyToReveal,"Result never completed");
            }
            model.Begin(new[]{cards[1]},profile,false);Assert(model.Zones.Count==1&&model.Zones[0].End==1,"Unavailable rarity kept an empty interval");
            model.Tick(1000);Assert(model.Position>=0&&model.Position<=1,"Marker escaped track");
            profile.legendary=3;profile.miss=2;model.Begin(cards.ToArray(),profile,false);
            Assert(model.Zones.Count==5&&model.Zones[4].IsMiss,"Optional zones not independently supported");
            var miss=model.Zones[4];model.Tick((miss.Start+miss.End)*.5f/profile.traversalsPerSecond);model.TryStop(model.Occurrence);Assert(model.Choose(random)==null,"Miss produced a creature");
            for(int ceiling=0;ceiling<4;ceiling++)
            {
                // Duplicate pool entries must not give one species additional lottery tickets.
                model.Begin(new[]{cards[0],cards[0],cards[1],cards[2],cards[3]},profile,false);
                var zone=model.Zones[ceiling];model.Tick((zone.Start+zone.End)*.5f/profile.traversalsPerSecond);model.TryStop(model.Occurrence);
                for(int index=0;index<=ceiling;index++)
                    Assert(model.Choose(new SpeciesIndexRandom(index,ceiling+1))==cards[index],"Ceiling draw biased or excluded an eligible species");
            }
            model.Begin(new[]{cards[2]},profile,true);Assert(model.Zones.Count==1&&!model.Zones[0].IsMiss,"Promise gained a miss or alternate rarity");
            model.Reset();Assert(!model.Active&&!model.TryStop(model.Occurrence-1),"Reset retained timing input");
            profile.miss=profile.legendary=0;
            var encounters=new EncounterRuntime { TimingProfile=profile };var resolver=new EffectResolver();int reveals=0;
            encounters.CreatureRevealed+=(c,r)=>reveals++;
            Assert(encounters.Reveal(new[]{cards[0],cards[1],cards[2]},"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver),"No slot reserved");
            Assert(encounters.RarityTiming.Active&&encounters.CurrentEncounter==null&&encounters.HookedEncounter==null,"Creature exposed before Stop");
            Assert(reveals==0&&encounters.RecentEncounterHistory.Length==0,"Reveal reactions or history ran early");
            Assert(!encounters.CompleteRarityTiming(random),"Reveal accepted before Stop");
            var timing=encounters.RarityTiming;timing.TryStop(timing.Occurrence);
            Assert(!encounters.CompleteRarityTiming(random),"Reveal skipped stopped feedback");timing.Tick(profile.resultHoldSeconds);
            Assert(encounters.CompleteRarityTiming(random)&&reveals==1&&encounters.HookedEncounter==cards[0],"Normal delayed reveal failed");
            Assert(encounters.LastSelectedEncounterWeight==1&&encounters.LastTotalEncounterWeight==1,"Diagnostics retained obsolete rarity weights");
            Assert(!encounters.CompleteRarityTiming(random)&&reveals==1,"Duplicate delayed reveal");
            encounters.TakeHookedEncounter();Set(cards[2],"minimumDepth",6);Set(cards[3],"biomeIds",new[]{"different_biome"});
            Assert(encounters.Reveal(cards.ToArray(),"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver),"Eligible timed slot failed");
            Assert(timing.Zones.Count==2&&timing.Zones[0].Rarity==CardRarity.Common,"History/depth/biome altered the uniform eligible pool");
            var uncommon=timing.Zones[1];timing.Tick((uncommon.Start+uncommon.End)*.5f/profile.traversalsPerSecond);timing.TryStop(timing.Occurrence);timing.Tick(profile.resultHoldSeconds);
            Assert(encounters.CompleteRarityTiming(new SpeciesIndexRandom(0,2))&&encounters.CurrentEncounter==cards[0],"Uncommon ceiling excluded a previous Common or included an ineligible creature");
            Set(cards[2],"minimumDepth",0);Set(cards[3],"biomeIds",Array.Empty<string>());
            encounters.Reset();var hazard=ScriptableObject.CreateInstance<CardDefinition>();cards.Add(hazard);Set(hazard,"cardType",CardType.Hazard);
            Assert(encounters.Reveal(new[]{hazard},"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver),"Event selection failed");
            Assert(!encounters.RarityTiming.Active&&encounters.CurrentEncounter==hazard&&encounters.HasEventReactionWindow,"Event did not bypass timing");
            encounters.Reset();Assert(encounters.RevealSelectedCreature(cards[2],"",5,Array.Empty<ActiveCatchEffectRecord>(),resolver),"Promised selection failed");
            timing.TryStop(timing.Occurrence);timing.Tick(profile.resultHoldSeconds);encounters.CompleteRarityTiming(random);
            Assert(encounters.CurrentEncounter==cards[2],"Promised identity changed");
            encounters.Reset();Set(cards[3],"cardType",CardType.ApexEncounter);Assert(encounters.RevealApex(cards[3]),"Apex reservation failed");
            timing.TryStop(timing.Occurrence);timing.Tick(profile.resultHoldSeconds);encounters.CompleteRarityTiming(random);
            Assert(encounters.CurrentEncounter==cards[3],"Apex schedule changed");
            encounters.Reset();var squid=AssetDatabase.FindAssets("t:CardDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c=>c.DisplayName=="Squid");
            encounters.GrantCreatureConcealment(squid.Effects[0],squid);
            Assert(encounters.RevealSelectedCreature(cards[0],"",5,Array.Empty<ActiveCatchEffectRecord>(),resolver),"Concealed reservation failed");
            Assert(!encounters.IsCreatureConcealed&&encounters.PendingConcealments[0].Remaining==2,"Timing consumed concealment early");
            timing.TryStop(timing.Occurrence);timing.Tick(profile.resultHoldSeconds);encounters.CompleteRarityTiming(random);
            Assert(encounters.IsCreatureConcealed&&encounters.PendingConcealments[0].Remaining==1,"Delayed reveal did not assign exactly one concealment");
            encounters.Reset();int cancelled=0;encounters.HazardCancellation=c=>true;encounters.HazardCancelled+=c=>cancelled++;
            encounters.Reveal(new[]{hazard},"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver);
            Assert(!timing.Active&&cancelled==1&&encounters.CurrentEncounter==null&&encounters.HasEventReactionWindow,"Automatic Hazard cancellation acquired a timing phase or lost its window");
            var alias=ScriptableObject.CreateInstance<CardDefinition>();cards.Add(alias);Set(alias,"cardType",CardType.Creature);
            Set(cards[0],"uniqueId","uniform_common");Set(alias,"uniqueId","uniform_common");
            model.Begin(new[]{cards[0],alias},profile,false);model.TryStop(model.Occurrence);
            Assert(model.Choose(new SpeciesIndexRandom(0,1))==cards[0],"Duplicate species IDs changed uniform probability");
            return assertions;
        }
        finally {foreach(var card in cards)UnityEngine.Object.DestroyImmediate(card);UnityEngine.Object.DestroyImmediate(profile);}
    }

    /// <summary>Checks exact motion endpoints, elastic wind-up and the one-second duration independently of frame rate.</summary>
    public static int ValidateExitMotion()
    {
        assertions=0;
        Assert(JournalRarityExitMotion.Duration==1,"Exit no longer lasts one second");
        Assert(JournalRarityExitMotion.Evaluate(0)==0,"Exit does not start at rest");
        Assert(JournalRarityExitMotion.Evaluate(1)==1,"Exit does not finish off-screen");
        Assert(JournalRarityExitMotion.Evaluate(-1)==0&&JournalRarityExitMotion.Evaluate(2)==1,"Exit endpoints are not clamped");
        Assert(JournalRarityExitMotion.Evaluate(.65f)<0,"Elastic wind-up was clamped away");
        Assert(Mathf.Abs(JournalRarityExitMotion.Evaluate(.9f)-.330337f)<.0001f,"Exit differs from inElastic(1,0.74)");
        return assertions;
    }

    /// <summary>Starts real Game View input, timing and restart checks using the authored comparison scene.</summary>
    public static void BeginProductionSmoke(string outputFolder="Docs/Validation/RarityTiming/")
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter comparison-scene Play Mode first.");
        output=outputFolder.EndsWith("/")?outputFolder:outputFolder+"/";
        Directory.CreateDirectory(output);Status="Running";
        UnityEngine.Object.FindAnyObjectByType<JournalModalHost>().StartCoroutine(Production());
    }

    /// <summary>Waits for optional live entrance motion without bypassing controller input gates.</summary>
    public static IEnumerator WaitForEntrance(FishingRunController controller)
    {
        var view=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();
        float deadline=Time.realtimeSinceStartup+6;
        while(controller.RunActive&&controller.RarityTiming.Active&&view!=null
            &&!view.HasCompletedEntrance(controller.RarityTiming.Occurrence)&&Time.realtimeSinceStartup<deadline)yield return null;
    }

    /// <summary>Runs the production boat/bar/role/next-slot sequence and captures native 4K output.</summary>
    private static IEnumerator Production()
    {
        assertions=0;var controller=UnityEngine.Object.FindAnyObjectByType<FishingRunController>();
        // Fix the smoke fixture's seed so it exercises a creature-first flow reproducibly.
        Set(controller,"useRandomSeed",false);Set(controller,"randomSeed",5);controller.StartRun();
        Assert(controller.IsOpeningBoatVisible,"Boat missing");Assert(controller.TryDescend(),"Boat Descend rejected");
        yield return new WaitForSecondsRealtime(1);
        yield return WaitForEntrance(controller);
        Assert(controller.RarityTiming.Active&&controller.CurrentEncounter==null,"First creature did not wait for Stop");
        Assert(!controller.CanDescend&&!controller.TryDescend(),"Descend bypassed timing");Assert(!controller.CanUseTechniqueCard(0,out _),"Technique played before reveal");
        var view=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();var button=view.StopButton;Canvas.ForceUpdateCanvases();
        var hits=new List<RaycastResult>();var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position)};
        EventSystem.current.RaycastAll(pointer,hits);Assert(hits.Any(h=>h.gameObject==button.gameObject),"Real Stop button cannot receive pointer input");
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"Moving4K.png");yield return null;
        long occurrence=controller.RarityTiming.Occurrence;button.onClick.Invoke();
        Assert(controller.RarityTiming.Stopped&&controller.CurrentEncounter==null,"Stop revealed before feedback");
        Assert(!controller.TryStopRarityTiming(occurrence),"Double Stop accepted");
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"Stopped4K.png");yield return new WaitForSecondsRealtime(.9f);
        Assert(controller.CurrentEncounter==null&&controller.RarityTiming.Active,"Creature revealed before the bar exited");
        Assert(view.ExitProgress>0&&view.ExitProgress<1&&!button.interactable,"Exit did not animate with Stop locked");
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"ExitWindUp4K.png");yield return new WaitForSecondsRealtime(.65f);
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"ExitUpward4K.png");yield return new WaitForSecondsRealtime(.35f);
        float revealDeadline=Time.realtimeSinceStartup+5;
        while(controller.RarityTiming.Active&&Time.realtimeSinceStartup<revealDeadline)yield return null;
        Assert(controller.CurrentEncounter!=null&&!controller.RarityTiming.Active,"Creature not revealed");
        Assert(controller.SelectedEncounterChoice==EncounterEffectChoice.None&&!controller.CanDescend,"Explicit role selection bypassed");
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"Revealed4K.png");yield return null;
        Assert(controller.TryChooseEncounterRole(EncounterEffectChoice.Bait),"Bait choice rejected");
        Assert(controller.TryChooseEncounterRole(EncounterEffectChoice.Catch),"Catch choice rejected");
        Assert(controller.TryDescend(),"Creature Descend rejected");yield return new WaitForSecondsRealtime(1);
        Assert(controller.RarityTiming.Active||EncounterRuntime.IsEventCard(controller.CurrentEncounter),"No next bar/event");
        if(controller.RarityTiming.Active)
        {
            var nextView=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();
            yield return WaitForEntrance(controller);
            Assert(nextView.ExitProgress==0&&nextView.GetComponent<RectTransform>().anchoredPosition==Vector2.zero,"Next attempt retained the previous exit offset");
        }
        // Surface invalidates the pending occurrence; a restarted run must not accept its callbacks.
        Assert(controller.CanSurface&&controller.TrySurface(),"Surface unavailable during timing");yield return new WaitForSecondsRealtime(2);
        Assert(!controller.RarityTiming.Active,"Surface retained pending timing");controller.StartRun();
        Assert(!controller.TryStopRarityTiming(occurrence),"Old Stop affected restarted run");
        Set(controller,"openingBoatPending",false);Set(controller,"currentDepth",5);
        var encounters=(EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime",Flags).GetValue(controller);
        var hazard=AssetDatabase.FindAssets("t:CardDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(c=>c.DisplayName=="Reef Snag");
        encounters.SetCurrentEncounter(hazard);typeof(FishingRunController).GetMethod("RefreshViews",Flags).Invoke(controller,null);yield return new WaitForSecondsRealtime(1);
        Assert(controller.CurrentEncounter==hazard&&!controller.RarityTiming.Active,"Event acquired a bar");
        Assert(UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>()==null,"Timing UI remained over event");
        UnityEngine.ScreenCapture.CaptureScreenshot(output+"EventBypass4K.png");yield return null;
        Assert(controller.TryDescend(),"Event Descend failed");yield return new WaitForSecondsRealtime(1);
        Assert(controller.CurrentDepth==6,"Event added an extra depth step");
        Assert(controller.RarityTiming.Active||EncounterRuntime.IsEventCard(controller.CurrentEncounter),"Event did not continue to normal bar/event selection");
        controller.StartRun();Assert(controller.TryDescend(),"Cancellation fixture could not reach timing");yield return new WaitForSecondsRealtime(1);
        yield return WaitForEntrance(controller);
        var cancelledView=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();
        Assert(controller.TryStopRarityTiming(controller.RarityTiming.Occurrence),"Cancellation fixture could not stop timing");yield return new WaitForSecondsRealtime(.85f);
        Assert(cancelledView.ExitProgress>0&&controller.CurrentEncounter==null,"Cancellation fixture did not reach exit before reveal");
        Assert(controller.TrySurface()&&!controller.RarityTiming.Active&&cancelledView.GetComponent<RectTransform>().anchoredPosition==Vector2.zero,"Surface did not cancel and reset the exiting bar");yield return new WaitForSecondsRealtime(2);
        controller.StartRun();controller.TryDescend();yield return new WaitForSecondsRealtime(1);
        yield return WaitForEntrance(controller);
        var restartedView=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();
        Assert(restartedView!=null&&restartedView.ExitProgress==0&&restartedView.GetComponent<RectTransform>().anchoredPosition==Vector2.zero,"Restart retained an exiting bar pose");
        Status="Passed "+assertions+" production assertions";File.WriteAllText(output+"ProductionValidation.json",JsonUtility.ToJson(new Report{assertions=assertions,status=Status},true));
    }
    [Serializable] private sealed class Report { public int assertions;public string status; }
    /// <summary>Inspects every possible species index rather than relying on a statistical frequency test.</summary>
    private sealed class SpeciesIndexRandom : System.Random
    {
        private readonly int index,expectedCount;
        /// <summary>Specifies an exact uniform draw and its expected number of equal entries.</summary>
        public SpeciesIndexRandom(int index,int expectedCount){this.index=index;this.expectedCount=expectedCount;}
        /// <summary>Requires one equal index per eligible species, with no weighted range.</summary>
        public override int Next(int maxValue){Assert(maxValue==expectedCount,"Species draw still uses weights or excludes eligible lower rarities");return index;}
    }
    /// <summary>Sets only disposable fixture state through its existing serialized field.</summary>
    private static void Set(object target,string name,object value){target.GetType().GetField(name,Flags).SetValue(target,value);}
    /// <summary>Fails immediately on a violated gameplay or input invariant.</summary>
    private static void Assert(bool condition,string message){assertions++;if(!condition){Status="Failed: "+message;throw new InvalidOperationException(message);}}
}
