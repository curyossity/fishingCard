using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>Verifies highest-visible-rarity feedback independently of gameplay randomness and selection.</summary>
public static class JournalRarityCelebrationChecks
{
    private static string output="Docs/Validation/RarityCelebration/";
    private static int assertions;
    private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Status {get;private set;}="Not started";

    /// <summary>Checks changing available rarities, optional Miss, duplicate prevention and emitter reset.</summary>
    public static int ValidateRules()
    {
        assertions=0;var profile=ScriptableObject.CreateInstance<RarityTimingProfile>();var cards=new List<CardDefinition>();GameObject fixture=null;
        try
        {
            for(int i=0;i<4;i++){var card=ScriptableObject.CreateInstance<CardDefinition>();Set(card,"rarity",(CardRarity)i);cards.Add(card);}
            var timing=new EncounterRarityTiming();timing.Begin(cards.ToArray(),profile,false);
            Assert(!JournalRarityCelebration.ShouldCelebrate(timing),"Moving marker celebrated");
            StopAt(timing,profile,CardRarity.Uncommon);Assert(!JournalRarityCelebration.ShouldCelebrate(timing),"Lower rarity celebrated");
            timing.Begin(cards.ToArray(),profile,false);StopAt(timing,profile,CardRarity.Rare);Assert(JournalRarityCelebration.ShouldCelebrate(timing),"Highest initial Rare did not celebrate");
            timing.Begin(new[]{cards[0],cards[1]},profile,false);StopAt(timing,profile,CardRarity.Uncommon);Assert(JournalRarityCelebration.ShouldCelebrate(timing),"Highest available Uncommon did not celebrate");
            profile.legendary=3;profile.miss=2;timing.Begin(cards.ToArray(),profile,false);StopAt(timing,profile,CardRarity.Rare);Assert(!JournalRarityCelebration.ShouldCelebrate(timing),"Rare celebrated while Legendary available");
            timing.Begin(cards.ToArray(),profile,false);StopAt(timing,profile,CardRarity.Legendary);Assert(JournalRarityCelebration.ShouldCelebrate(timing),"Legendary did not celebrate beside Miss");
            timing.Begin(cards.ToArray(),profile,false);var miss=timing.Zones[timing.Zones.Count-1];timing.Tick((miss.Start+miss.End)*.5f/profile.traversalsPerSecond);timing.TryStop(timing.Occurrence);
            Assert(!JournalRarityCelebration.ShouldCelebrate(timing),"Miss celebrated as a rarity");
            timing.Begin(new[]{cards[0]},profile,true);StopAt(timing,profile,CardRarity.Common);Assert(JournalRarityCelebration.ShouldCelebrate(timing),"Single highest available rarity failed");
            fixture=new GameObject("Particle fixture",typeof(RectTransform),typeof(CanvasRenderer));var emitter=fixture.AddComponent<JournalRarityCelebration>();
            Assert(emitter.Play(4)&&emitter.IsPlaying,"Emitter did not start");Assert(!emitter.Play(4)&&emitter.EmissionCount==1,"Same stop emitted twice");
            Assert(!emitter.raycastTarget,"Particles block pointer input");emitter.Clear();Assert(!emitter.IsPlaying,"Cancellation retained particles");
            Assert(emitter.Play(5)&&emitter.EmissionCount==2,"Emitter was not reusable");
            Set(emitter,"startedAt",Time.unscaledTime-2);typeof(JournalRarityCelebration).GetMethod("Update",Flags).Invoke(emitter,null);
            Assert(!emitter.IsPlaying,"Finished particles did not expire");
            return assertions;
        }
        finally{if(fixture!=null)UnityEngine.Object.DestroyImmediate(fixture);foreach(var card in cards)UnityEngine.Object.DestroyImmediate(card);UnityEngine.Object.DestroyImmediate(profile);}
    }

    /// <summary>Starts the authored game's highest-zone, lower-zone, cancellation and reuse smoke.</summary>
    public static void BeginProductionSmoke(string outputFolder="Docs/Validation/RarityCelebration/")
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter comparison-scene Play Mode.");
        output=outputFolder.EndsWith("/")?outputFolder:outputFolder+"/";
        Directory.CreateDirectory(output);Status="Running";UnityEngine.Object.FindAnyObjectByType<JournalModalHost>().StartCoroutine(Production());
    }

    /// <summary>Stops the live marker in a narrow highest zone, verifies a single burst, then checks a lower stop.</summary>
    private static IEnumerator Production()
    {
        assertions=0;var controller=UnityEngine.Object.FindAnyObjectByType<FishingRunController>();
        Set(controller,"useRandomSeed",false);Set(controller,"randomSeed",5);controller.StartRun();Assert(controller.TryDescend(),"Boat did not descend");yield return new WaitForSecondsRealtime(1);
        yield return JournalRarityTimingChecks.WaitForEntrance(controller);
        Assert(controller.RarityTiming.Active,"Creature timing missing");var timing=controller.RarityTiming;
        var highest=timing.Zones[0];foreach(var zone in timing.Zones)if(!zone.IsMiss&&(int)zone.Rarity>(int)highest.Rarity)highest=zone;
        float deadline=Time.realtimeSinceStartup+8;
        while((timing.Position<highest.Start+(highest.End-highest.Start)*.2f||timing.Position>highest.End-(highest.End-highest.Start)*.2f)&&Time.realtimeSinceStartup<deadline)yield return null;
        var view=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();view.StopButton.onClick.Invoke();
        Assert(JournalRarityCelebration.ShouldCelebrate(timing),"Live stop missed highest rarity");
        Assert(view.Celebration!=null&&view.Celebration.IsPlaying&&view.Celebration.EmissionCount==1,"Highest stop has no single particle burst");
        typeof(FishingRunController).GetMethod("RefreshViews",Flags).Invoke(controller,null);Assert(view.Celebration.EmissionCount==1,"View refresh replayed burst");
        yield return new WaitForSecondsRealtime(.25f);UnityEngine.ScreenCapture.CaptureScreenshot(output+"HighestRarityParticles4K.png");yield return null;
        Assert(controller.CurrentEncounter==null,"Particles exposed creature early");Assert(controller.TrySurface(),"Surface could not cancel celebration");
        Assert(!view.Celebration.IsPlaying,"Surface retained celebration");yield return new WaitForSecondsRealtime(2);
        controller.StartRun();controller.TryDescend();yield return new WaitForSecondsRealtime(1);
        yield return JournalRarityTimingChecks.WaitForEntrance(controller);
        view=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();Assert(view.Celebration==null||!view.Celebration.IsPlaying,"New attempt retained particles");
        Assert(controller.TryStopRarityTiming(controller.RarityTiming.Occurrence),"Lower stop failed");
        Assert(!JournalRarityCelebration.ShouldCelebrate(controller.RarityTiming),"Fixture did not stop a lower zone");
        Assert(view.Celebration==null||!view.Celebration.IsPlaying,"Lower rarity emitted celebration");
        Status="Passed "+assertions+" production assertions";File.WriteAllText(output+"ProductionValidation.json",JsonUtility.ToJson(new Report{status=Status,assertions=assertions},true));
    }
    [Serializable] private sealed class Report {public string status;public int assertions;}
    /// <summary>Advances a fixture directly to a specific visible rarity interval.</summary>
    private static void StopAt(EncounterRarityTiming timing,RarityTimingProfile profile,CardRarity rarity)
    {foreach(var zone in timing.Zones)if(!zone.IsMiss&&zone.Rarity==rarity){timing.Tick((zone.Start+zone.End)*.5f/profile.traversalsPerSecond);timing.TryStop(timing.Occurrence);return;}throw new InvalidOperationException("Missing fixture zone");}
    /// <summary>Changes only disposable or Play Mode fixture fields.</summary>
    private static void Set(object target,string field,object value){target.GetType().GetField(field,Flags).SetValue(target,value);}
    /// <summary>Fails with a reproducible feedback condition.</summary>
    private static void Assert(bool condition,string message){assertions++;if(!condition){Status="Failed: "+message;throw new InvalidOperationException(message);}}
}
