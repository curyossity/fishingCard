using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;

/// <summary>Checks bottom-entry geometry, occurrence-bound input locks and cancellation/reuse in the real HUD.</summary>
public static class JournalRarityEntranceChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/RarityBarEntrance/";
    private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    private static int assertions;
    public static string Status {get;private set;}="Not started";

    /// <summary>Verifies mirrored elastic endpoints, settling, clamping and viewport-independent scale.</summary>
    public static int ValidateRules()
    {
        assertions=0;
        Assert(JournalRarityExitMotion.EntranceOffset(0,2160)==Vector2.down*2160,"Entrance did not start below the viewport");
        Assert(JournalRarityExitMotion.EntranceOffset(1,2160)==Vector2.zero,"Entrance did not end at rest");
        Assert(JournalRarityExitMotion.EntranceOffset(.35f,2160).y>0,"Mirrored elastic settling was clamped away");
        Assert(JournalRarityExitMotion.EntranceOffset(.15f,2160).x==0,"Entrance moved horizontally");
        Assert(Vector2.Distance(JournalRarityExitMotion.EntranceOffset(.15f,2160),JournalRarityExitMotion.EntranceOffset(.15f,1080)*2)<.001f,"Entrance distance did not scale with viewport");
        Assert(JournalRarityExitMotion.EntranceOffset(-1,2160)==Vector2.down*2160,"Negative entrance progress escaped start");
        Assert(JournalRarityExitMotion.EntranceOffset(2,2160)==Vector2.zero,"Excess entrance progress escaped rest");
        Assert(JournalRarityExitMotion.EntranceOffset(.5f,-100)==Vector2.zero,"Invalid viewport produced inverted travel");
        return assertions;
    }

    /// <summary>Starts real bar entrance, input-lock, refresh and cancellation verification.</summary>
    public static void BeginProductionSmoke()
    {
        if(!Application.isPlaying)throw new InvalidOperationException("Enter comparison-scene Play Mode first.");
        Directory.CreateDirectory(Output);Status="Running";
        UnityEngine.Object.FindAnyObjectByType<JournalModalHost>().StartCoroutine(Production());
    }

    /// <summary>Captures a live entrance, verifies it cannot be stopped early, then cancels and reuses it.</summary>
    private static IEnumerator Production()
    {
        assertions=0;var controller=UnityEngine.Object.FindAnyObjectByType<FishingRunController>();
        Set(controller,"useRandomSeed",false);Set(controller,"randomSeed",5);controller.StartRun();Assert(controller.TryDescend(),"Boat could not start timing");
        var view=UnityEngine.Object.FindAnyObjectByType<JournalRarityTimingView>();Assert(view!=null,"Timing view missing");
        Assert(view.GetComponent<RectTransform>().anchoredPosition.y<0,"Bar flashed at rest before entering");
        yield return WaitForEntryStart(view);
        Assert(view.EntranceProgress>0&&view.EntranceProgress<1,"Entrance did not animate");
        Assert(!view.StopButton.interactable&&!controller.TryStopRarityTiming(controller.RarityTiming.Occurrence),"Stop accepted during entrance");
        Assert(controller.RarityTiming.Position==0&&controller.CurrentEncounter==null,"Marker or creature advanced during entrance");
        float progress=view.EntranceProgress;typeof(FishingRunController).GetMethod("RefreshViews",Flags).Invoke(controller,null);
        Assert(view.EntranceProgress>=progress,"Refresh restarted the entrance");
        Assert(view.Celebration==null||!view.Celebration.IsPlaying,"Entrance triggered result particles");
        UnityEngine.ScreenCapture.CaptureScreenshot(Output+"EnteringFromBelow4K.png");yield return null;
        yield return JournalRarityTimingChecks.WaitForEntrance(controller);
        Assert(view.HasCompletedEntrance(controller.RarityTiming.Occurrence)&&view.StopButton.interactable,"Entrance did not unlock Stop");
        Assert(view.GetComponent<RectTransform>().anchoredPosition==Vector2.zero,"Entrance did not settle at its authored position");
        UnityEngine.ScreenCapture.CaptureScreenshot(Output+"Settled4K.png");yield return null;
        Assert(controller.TrySurface(),"Surface failed after entrance");yield return new WaitForSecondsRealtime(2);
        controller.StartRun();Assert(controller.TryDescend(),"Second attempt failed");
        yield return WaitForEntryStart(view);
        Assert(!view.HasCompletedEntrance(controller.RarityTiming.Occurrence),"Second entrance was skipped");
        Assert(controller.CanSurface&&controller.TrySurface(),"Surface failed during entrance");
        Assert(!controller.RarityTiming.Active&&view.GetComponent<RectTransform>().anchoredPosition==Vector2.zero&&view.EntranceProgress==0,"Cancellation retained a travelling bar");
        yield return new WaitForSecondsRealtime(2);controller.StartRun();controller.TryDescend();
        yield return JournalRarityTimingChecks.WaitForEntrance(controller);
        Assert(view.StopButton.interactable&&view.GetComponent<RectTransform>().anchoredPosition==Vector2.zero,"Restart did not complete a fresh entrance");
        Status="Passed "+assertions+" production assertions";File.WriteAllText(Output+"ProductionValidation.json",JsonUtility.ToJson(new Report{status=Status,assertions=assertions},true));
    }
    [Serializable] private sealed class Report {public string status;public int assertions;}
    /// <summary>Waits for a visible portion of entrance motion after the outgoing card transition.</summary>
    private static IEnumerator WaitForEntryStart(JournalRarityTimingView view)
    {float deadline=Time.realtimeSinceStartup+6;while(view.EntranceProgress<.12f&&Time.realtimeSinceStartup<deadline)yield return null;}
    /// <summary>Changes only transient Play Mode fixture configuration.</summary>
    private static void Set(object target,string field,object value){target.GetType().GetField(field,Flags).SetValue(target,value);}
    /// <summary>Fails on an entrance, input or lifecycle invariant.</summary>
    private static void Assert(bool condition,string message){assertions++;if(!condition){Status="Failed: "+message;throw new InvalidOperationException(message);}}
}
