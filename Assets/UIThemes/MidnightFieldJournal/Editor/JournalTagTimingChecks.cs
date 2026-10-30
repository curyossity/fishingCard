using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic tag selection rules and real candidate-scene animation/reveal verification.</summary>
public static class JournalTagTimingChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/Sync009/";
    private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    private static int assertions;
    public static string Status { get; private set; }="Not started";
    private static bool previousBackground;
    private static double smokeDeadline;

    /// <summary>Checks probability ranges directly, fatigue bounds, persistence and authoritative reveal ownership.</summary>
    public static int ValidateRules()
    {
        assertions=0;
        var profile=ScriptableObject.CreateInstance<TagTimingProfile>();var cards=new List<CardDefinition>();
        try
        {
            for(int i=0;i<4;i++)
            {
                var c=ScriptableObject.CreateInstance<CardDefinition>();cards.Add(c);
                Set(c,"uniqueId","tag_fixture_"+i);Set(c,"displayName","Fixture "+i);Set(c,"cardType",CardType.Creature);
                Set(c,"rarity",(CardRarity)i);Set(c,"tags",new[]{"Fish"});
            }
            var model=new EncounterTagTiming();model.Begin(cards.ToArray(),profile,false);
            Assert(model.Zones.Count==8,"Unavailable tags were hidden");
            foreach(var z in model.Zones)Assert(Mathf.Abs(z.End-z.Start-.125f)<.00001f,"Unequal initial zones");
            Stop(model,0,false,profile);
            Assert(!model.PrecisionHit,"Normal hit misclassified");
            for(int i=0;i<4;i++)Assert(model.Choose(new ExactRandom(i,4))==cards[i],"Normal species not uniform across rarities");
            Assert(Mathf.Abs(model.NextShares[0]-(.125f*.85f/(.875f+.125f*.85f)))<.00001f,"First shrink/normalization wrong");
            float first=model.NextShares[0];long old=model.Occurrence;
            Assert(!model.TryStop(old),"Second Stop accepted");Assert(model.NextShares[0]==first,"Second Stop changed fatigue");
            model.Begin(cards.ToArray(),profile,false);Assert(model.Zones[0].End==first,"New phase reset fatigue");
            Assert(!model.TryStop(old),"Stale callback accepted");
            model.ResetRun();model.Begin(cards.ToArray(),profile,false);Stop(model,0,true,profile);
            Assert(model.PrecisionHit&&model.CelebrateStop,"Precision not rewarded");
            int[] weights={1,2,4,8};int ticket=0;
            for(int i=0;i<4;i++)for(int j=0;j<weights[i];j++)Assert(model.Choose(new ExactRandom(ticket++,15))==cards[i],"Precision tickets differ from1/2/4/8");
            model.ResetRun();model.Begin(cards.ToArray(),profile,false);Stop(model,4,false,profile);
            Assert(model.UsesFallbackPool&&model.EligibleSpeciesCount==4,"Unavailable tag did not use full eligible snapshot");
            for(int i=0;i<4;i++)Assert(model.Choose(new ExactRandom(i,4))==cards[i],"Fallback retained rarity bias");
            model.ResetRun();
            for(int hit=0;hit<150;hit++)
            {
                model.Begin(cards.ToArray(),profile,false);Stop(model,0,false,profile);
                Assert(Mathf.Abs(model.NextShares.Sum()-1)<.00001f,"Track does not fill100%");
                foreach(float share in model.NextShares)Assert(share>=.049999f,"Track violated5% floor");
            }
            Assert(Mathf.Abs(model.NextShares[0]-.05f)<.00001f,"Repeated hit never reached floor");
            float floor=model.NextShares[0];model.Begin(cards.ToArray(),profile,false);Stop(model,1,false,profile);
            Assert(model.NextShares[0]>floor,"Floor zone retained hidden fatigue instead of recovering");
            float recovered=model.NextShares[0];model.Reset();model.Begin(cards.ToArray(),profile,false);
            Assert(Mathf.Abs(model.Zones[0].End-recovered)<.00001f,"Phase reset cleared run balance");
            model.ResetRun();model.Begin(cards.ToArray(),profile,false);Assert(model.Zones[0].End==.125f,"Run reset did not restore baseline");
            for(int hit=0;hit<400;hit++)
            {
                model.Begin(cards.ToArray(),profile,false);Stop(model,(hit*7+hit/9)%8,hit%2==0,profile);
                Assert(Mathf.Abs(model.NextShares.Sum()-1)<.00001f&&model.NextShares.All(x=>x>=.049999f),"Mixed hits broke normalization");
            }
            model.ResetRun();model.Begin(new[]{cards[0],cards[0],cards[1]},profile,false);Stop(model,0,false,profile);
            Assert(model.Choose(new ExactRandom(1,2))==cards[1],"Duplicate pool entries bias species");
            Set(cards[1],"tags",new[]{"Small"});model.ResetRun();model.Begin(cards.ToArray(),profile,false);Stop(model,0,false,profile);
            Assert(model.EligibleSpeciesCount==3,"Stopped tag failed filtering");
            Assert(model.Choose(new ExactRandom(1,3))==cards[2],"Other-tag creature leaked into selection");
            Set(cards[1],"tags",new[]{"Fish"});
            var runtime=new EncounterRuntime{TagTimingProfile=profile};var resolver=new EffectResolver();var random=new System.Random(5);int reveals=0;
            runtime.CreatureRevealed+=(c,r)=>reveals++;
            Set(cards[2],"minimumDepth",6);Set(cards[3],"biomeIds",new[]{"other"});
            Assert(runtime.Reveal(cards.ToArray(),"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver),"No deferred creature");
            Assert(runtime.Timing==runtime.TagTiming&&runtime.CurrentEncounter==null&&reveals==0,"Creature exposed before Stop");
            Stop(runtime.TagTiming,0,false,profile);
            Assert(runtime.TagTiming.EligibleSpeciesCount==2,"Biome/depth ignored by tag snapshot");
            Assert(!runtime.CompleteTiming(random),"Stopped hold skipped");runtime.TagTiming.Tick(profile.resultHoldSeconds);
            Assert(runtime.CompleteTiming(new ExactRandom(1,2))&&runtime.CurrentEncounter==cards[1]&&reveals==1,"Delayed reveal failed");
            Assert(!runtime.CompleteTiming(random)&&reveals==1,"Duplicate reveal");
            runtime.Reset(true);Assert(!runtime.Timing.Active,"Biome reset retained active phase");
            Assert(runtime.RevealSelectedCreature(cards[0],"",5,Array.Empty<ActiveCatchEffectRecord>(),resolver),"Promised creature failed");
            Assert(runtime.TagTiming.Zones[0].End<.125f,"Biome transition reset fatigue");
            Stop(runtime.TagTiming,7,true,profile);runtime.TagTiming.Tick(profile.resultHoldSeconds);
            Assert(runtime.CompleteTiming(random)&&runtime.CurrentEncounter==cards[0],"Tag displaced promised identity");
            runtime.Reset();Set(cards[3],"cardType",CardType.ApexEncounter);
            Assert(runtime.RevealApex(cards[3]),"Apex reservation failed");Stop(runtime.TagTiming,0,false,profile);runtime.TagTiming.Tick(profile.resultHoldSeconds);
            Assert(runtime.CompleteTiming(random)&&runtime.CurrentEncounter==cards[3],"Tag displaced Apex");
            runtime.Reset();var hazard=ScriptableObject.CreateInstance<CardDefinition>();cards.Add(hazard);Set(hazard,"cardType",CardType.Hazard);
            Assert(runtime.Reveal(new[]{hazard},"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver)&&!runtime.Timing.Active&&runtime.HasEventReactionWindow,"Event acquired timing");
            runtime.Reset();int cancelled=0;runtime.HazardCancellation=c=>true;runtime.HazardCancelled+=c=>cancelled++;
            runtime.Reveal(new[]{hazard},"",5,random,Array.Empty<ActiveCatchEffectRecord>(),resolver);
            Assert(cancelled==1&&!runtime.Timing.Active&&runtime.HasEventReactionWindow,"Cancelled event acquired timing or lost window");
            Status="Rules passed: "+assertions;return assertions;
        }
        finally{foreach(var c in cards)UnityEngine.Object.DestroyImmediate(c);UnityEngine.Object.DestroyImmediate(profile);}
    }

    /// <summary>Starts a bounded real Play Mode test with temporary background ticking.</summary>
    public static void BeginProductionSmoke()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter candidate Play Mode first.");
        Directory.CreateDirectory(Output);assertions=0;Status="Running";
        previousBackground=Application.runInBackground;Application.runInBackground=true;smokeDeadline=EditorApplication.timeSinceStartup+90;
        EditorApplication.update+=Pump;
        UnityEngine.Object.FindAnyObjectByType<JournalModalHost>().StartCoroutine(Production());
    }

    /// <summary>Ensures the unfocused live editor actually renders frames during automated validation.</summary>
    private static void Pump()
    {
        if(!Application.isPlaying||EditorApplication.timeSinceStartup>smokeDeadline||Status!="Running")
        {EditorApplication.update-=Pump;Application.runInBackground=previousBackground;if(Status=="Running")Status="Failed: smoke timeout";return;}
        EditorApplication.QueuePlayerLoopUpdate();
    }

    /// <summary>Exercises real input locks, smooth widths, feedback, travel, event bypass and restart.</summary>
    private static IEnumerator Production()
    {
        var c=UnityEngine.Object.FindAnyObjectByType<FishingRunController>();Set(c,"useRandomSeed",false);Set(c,"randomSeed",5);
        c.StartRun();Assert(c.TryDescend(),"Boat could not descend");
        var v=UnityEngine.Object.FindAnyObjectByType<JournalTagTimingView>();Assert(v!=null&&c.TagTiming.Active,"Tag view missing");
        Assert(!v.StopButton.interactable&&!c.TryStopEncounterTiming(c.Timing.Occurrence),"Stop accepted before entry");
        Assert(c.CurrentEncounter==null&&!c.CanDescend,"Creature exposed during entry");
        yield return Entry(c,v);float p=c.Timing.Position;int frame=Time.frameCount;yield return new WaitForSecondsRealtime(.1f);
        Assert(Time.frameCount>frame&&c.Timing.Position!=p,"Live marker not advancing");
        Capture("TagBar4K.png",3840,2160);Capture("TagBar1080.png",1920,1080);
        // Use real input, but steer the clock deterministically to a precision centre.
        float target=(c.TagTiming.Zones[0].Start+c.TagTiming.Zones[0].End)*.5f;
        typeof(EncounterTiming).GetField("elapsed",Flags).SetValue(c.Timing,target/.6f);
        long token=c.Timing.Occurrence;v.StopButton.onClick.Invoke();
        Assert(c.Timing.Stopped&&c.TagTiming.PrecisionHit&&c.CurrentEncounter==null,"Stop did not capture hidden precision result");
        Assert(!c.TryStopEncounterTiming(token),"Double Stop accepted");
        yield return null;Assert(v.Celebration!=null&&v.Celebration.IsPlaying,"Precision particles absent");
        yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
        Assert(v.ZoneRects[0].rect.width<180&&v.ZoneRects[0].rect.width>72,"Zone did not resize");
        float exitDeadline=Time.realtimeSinceStartup+3;
        while(v.ExitProgress<=0&&c.Timing.Active&&Time.realtimeSinceStartup<exitDeadline)yield return null;
        Assert(c.CurrentEncounter==null&&c.Timing.Active&&v.ExitProgress>0,"Reveal skipped exit");
        Capture("TagBarPrecision4K.png",3840,2160);
        yield return Reveal(c);
        Assert(c.CurrentEncounter!=null&&!c.Timing.Active&&!c.CanDescend,"Reveal or explicit role requirement broken");
        Assert(c.TryChooseEncounterRole(EncounterEffectChoice.Catch),"Catch selection rejected");yield return new WaitForSecondsRealtime(.2f);
        Assert(c.TryDescend(),"Chosen Catch could not descend");
        yield return new WaitForSecondsRealtime(1.2f);
        Assert(c.Timing.Active||EncounterRuntime.IsEventCard(c.CurrentEncounter),"No next tag/event phase");
        Assert(c.TrySurface(),"Surface could not cancel phase");
        Assert(!c.Timing.Active,"Surface retained timing");yield return new WaitForSecondsRealtime(2);
        c.StartRun();Assert(!c.TryStopEncounterTiming(token),"Old Stop affected restarted run");
        Assert(c.TryDescend(),"Restart boat failed");v=UnityEngine.Object.FindAnyObjectByType<JournalTagTimingView>();
        Assert(c.TagTiming.Zones[0].End==.125f,"Restart retained fatigue");
        yield return Entry(c,v);Assert(c.TryStopEncounterTiming(c.Timing.Occurrence),"Restart Stop failed");yield return Reveal(c);
        c.StartRun();Assert(c.TryDescend(),"Fatigue visual fixture failed");v=UnityEngine.Object.FindAnyObjectByType<JournalTagTimingView>();yield return Entry(c,v);
        var tagProfile=AssetDatabase.LoadAssetAtPath<TagTimingProfile>(JournalTagTimingBuilder.ProfilePath);
        var cod=AssetDatabase.LoadAssetAtPath<CardDefinition>("Assets/Cards/Coastal Cod.asset");Assert(cod!=null,"Cod visual fixture missing");
        for(int hit=0;hit<35;hit++){c.TagTiming.Begin(new[]{cod},tagProfile,false);Stop(c.TagTiming,0,false,tagProfile);}
        c.TagTiming.Begin(new[]{cod},tagProfile,false);typeof(FishingRunController).GetMethod("RefreshViews",Flags).Invoke(c,null);
        yield return Entry(c,v);Canvas.ForceUpdateCanvases();
        Assert(Mathf.Abs(v.ZoneRects[0].rect.width-72)<.1f,"Minimum zone not72 design pixels");
        Assert(v.ZoneRects.All(x=>x.gameObject.activeInHierarchy),"Minimum width hid a tag");
        Assert(v.PrecisionRects.All(x=>Mathf.Abs(x.rect.width/x.parent.GetComponent<RectTransform>().rect.width-.18f)<.001f),"Precision widths not proportional");
        Capture("TagBarFatigue4K.png",3840,2160);
        c.StartRun();
        var runtime=(EncounterRuntime)typeof(FishingRunController).GetField("encounterRuntime",Flags).GetValue(c);
        var hazard=AssetDatabase.FindAssets("t:CardDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<CardDefinition>(AssetDatabase.GUIDToAssetPath(g))).First(x=>x.UniqueId=="reef_snag");
        Set(c,"openingBoatPending",false);runtime.Reset();runtime.TagTimingProfile=AssetDatabase.LoadAssetAtPath<TagTimingProfile>(JournalTagTimingBuilder.ProfilePath);
        Assert(runtime.Reveal(new[]{hazard},c.CurrentBiome.BiomeId,5,new System.Random(2),Array.Empty<ActiveCatchEffectRecord>(),new EffectResolver()),"Event fixture not eligible");runtime.TagTimingProfile=null;
        typeof(FishingRunController).GetMethod("RefreshViews",Flags).Invoke(c,null);
        Assert(c.CurrentEncounter==hazard&&!c.Timing.Active&&UnityEngine.Object.FindAnyObjectByType<JournalTagTimingView>()==null,"Event displayed timing UI");
        Capture("EventBypass4K.png",3840,2160);c.StartRun();
        Status="Passed: "+assertions;File.WriteAllText(Output+"PlayModeVerification.json",JsonUtility.ToJson(new Report{status=Status,assertions=assertions},true));
        EditorApplication.update-=Pump;Application.runInBackground=previousBackground;
    }

    /// <summary>Waits for the exact entry with an explicit timeout.</summary>
    private static IEnumerator Entry(FishingRunController c,JournalTagTimingView v)
    {float end=Time.realtimeSinceStartup+8;while(c.Timing.Active&&!v.HasCompletedEntrance(c.Timing.Occurrence)&&Time.realtimeSinceStartup<end)yield return null;Assert(v.HasCompletedEntrance(c.Timing.Occurrence)&&v.StopButton.interactable,"Entry timeout");}
    /// <summary>Waits until shared hold/exit release the normal reveal.</summary>
    private static IEnumerator Reveal(FishingRunController c)
    {float end=Time.realtimeSinceStartup+8;while(c.Timing.Active&&Time.realtimeSinceStartup<end)yield return null;Assert(!c.Timing.Active,"Reveal timeout");}
    /// <summary>Captures the actual overlay hierarchy in a 16:9 render without changing asset authoring.</summary>
    private static void Capture(string name,int width,int height)
    {
        var canvas=UnityEngine.Object.FindAnyObjectByType<FishingRunView>().GetComponent<Canvas>();
        var cameraObject=new GameObject("Timing verification camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color32(17,30,37,255);camera.orthographic=true;camera.orthographicSize=1080;
        var mode=canvas.renderMode;var oldCamera=canvas.worldCamera;float distance=canvas.planeDistance;
        var target=new RenderTexture(width,height,24);var oldTarget=RenderTexture.active;Texture2D image=null;
        try
        {
            camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=100;
            Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=target;image=new Texture2D(width,height,TextureFormat.RGBA32,false);
            image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(Output+name,image.EncodeToPNG());
        }
        finally
        {
            canvas.renderMode=mode;canvas.worldCamera=oldCamera;canvas.planeDistance=distance;RenderTexture.active=oldTarget;
            camera.targetTexture=null;target.Release();UnityEngine.Object.DestroyImmediate(target);if(image!=null)UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(cameraObject);Canvas.ForceUpdateCanvases();
        }
    }
    /// <summary>Stops a deterministic normal or precision point in a chosen span.</summary>
    private static void Stop(EncounterTagTiming model,int index,bool precision,TagTimingProfile profile)
    {var z=model.Zones[index];float p=Mathf.Lerp(z.Start,z.End,precision?.5f:.15f);model.Tick(p/profile.traversalsPerSecond);Assert(model.TryStop(model.Occurrence),"Fixture Stop failed");Assert(model.Outcome.Tag==z.Tag,"Wrong tag hit");}
    /// <summary>Writes disposable fixture fields without modifying authored card assets.</summary>
    private static void Set(object target,string name,object value){var f=target.GetType().GetField(name,Flags);if(f==null)throw new InvalidOperationException("Missing fixture field "+name);f.SetValue(target,value);}
    /// <summary>Fails a meaningful rule or presentation invariant immediately.</summary>
    private static void Assert(bool condition,string message){assertions++;if(!condition){Status="Failed: "+message;throw new InvalidOperationException(message);}}
    [Serializable] private sealed class Report{public string status;public int assertions;}
    /// <summary>Inspects every lottery ticket instead of relying on a flaky frequency estimate.</summary>
    private sealed class ExactRandom:System.Random
    {
        private readonly int index,count;
        /// <summary>Defines one expected lottery ticket and total range.</summary>
        public ExactRandom(int index,int count){this.index=index;this.count=count;}
        /// <summary>Rejects biased ranges and returns the requested deterministic ticket.</summary>
        public override int Next(int maxValue){Assert(maxValue==count,"Unexpected sampling range "+maxValue+" instead of "+count);return index;}
    }
}
