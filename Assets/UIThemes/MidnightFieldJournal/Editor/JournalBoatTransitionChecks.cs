using System;
using System.Collections;
using System.IO;
using UnityEngine;

/// <summary>Checks the real boat-to-encounter animation, completion and interrupted restart in Play Mode.</summary>
public static class JournalBoatTransitionChecks
{
    private const string Output="Docs/UIThemes/MidnightFieldJournal/Sync006/";
    private static JournalModalHost runner;
    private static bool oldBackground;
    private static int assertions;
    public static string Status { get; private set; }="Not started";
    [Serializable] private sealed class Report { public bool passed; public int assertions; public float midTransitionY; public string error; }
    private static float midTransitionY;

    /// <summary>Starts a finite real-controller sequence in the active candidate scene.</summary>
    public static void Start()
    {
        if(!Application.isPlaying||runner!=null)throw new InvalidOperationException("Start once in Play Mode.");
        oldBackground=Application.runInBackground;Application.runInBackground=true;assertions=0;Status="Running";
        runner=new GameObject("Boat Transition Verification").AddComponent<JournalModalHost>();runner.StartCoroutine(Guarded());
    }
    /// <summary>Records failures and restores temporary execution state even if an assertion fails.</summary>
    private static IEnumerator Guarded()
    {
        var sequence=Check();Exception failure=null;
        while(true){bool more;try{more=sequence.MoveNext();}catch(Exception e){failure=e;break;}if(!more)break;yield return sequence.Current;}
        File.WriteAllText(Output+"BoatPlayModeVerification.json",JsonUtility.ToJson(new Report{passed=failure==null,assertions=assertions,midTransitionY=midTransitionY,error=failure==null?null:failure.ToString()},true));
        Application.runInBackground=oldBackground;var owned=runner;runner=null;UnityEngine.Object.Destroy(owned.gameObject);
        Status=failure==null?"Passed":"Failed: "+failure;if(failure!=null)Debug.LogException(failure);
    }
    /// <summary>Samples actual outgoing movement and verifies cleanup without introducing an artificial card definition.</summary>
    private static IEnumerator Check()
    {
        var controller=UnityEngine.Object.FindFirstObjectByType<FishingRunController>();var hud=UnityEngine.Object.FindFirstObjectByType<JournalGameplayHud>();var view=UnityEngine.Object.FindFirstObjectByType<FishingRunView>();
        controller.StartRun();yield return null;yield return null;yield return null;
        var boat=hud.OpeningCard;Transform rest=boat.parent;
        Require(boat.gameObject.activeInHierarchy&&controller.IsOpeningBoatVisible,"Boat not initially visible");
        hud.DescendButton.onClick.Invoke();
        Require(view.IsEncounterTransitionPlaying,"Opening did not use encounter transition");
        Require(boat.gameObject.activeInHierarchy&&!controller.IsOpeningBoatVisible,"Boat disappeared with controller flag");
        Require(boat.parent.name=="Wavy Wipe Root","Boat not using the common wipe/motion hierarchy");
        Require(!hud.DescendButton.interactable&&!hud.SurfaceButton.interactable,"Transition failed to lock action input");
        var motion=(RectTransform)boat.parent.parent;bool sampled=false;float deadline=Time.realtimeSinceStartup+3;var samples=new System.Collections.Generic.List<string>();
        while(view.IsEncounterTransitionPlaying&&Time.realtimeSinceStartup<deadline)
        {
            samples.Add("frame="+Time.frameCount+", y="+motion.anchoredPosition.y+", h="+motion.rect.height+", active="+boat.gameObject.activeInHierarchy+", dt="+Time.unscaledDeltaTime);
            if(!sampled&&motion.anchoredPosition.y>motion.rect.height*.15f&&motion.anchoredPosition.y<motion.rect.height*.7f)
            {
                midTransitionY=motion.anchoredPosition.y;
                Require(boat.gameObject.activeInHierarchy,"Boat disappeared during upward movement");
                ScreenCapture.CaptureScreenshot(Output+"BoatMoving4K.png");sampled=true;
            }
            yield return null;
        }
        File.WriteAllLines(Output+"BoatFrameSamples.txt",samples);
        Require(sampled&&midTransitionY>0,"No upward movement sample");
        Require(!view.IsEncounterTransitionPlaying&&!boat.gameObject.activeSelf,"Boat remained after transition");
        Require(boat.parent==rest,"Boat was not restored to its resting parent");
        Require(controller.CurrentDepth==1&&controller.CatchChain.Length==0&&controller.CurrentEncounter!=null,"First encounter gameplay changed");
        ScreenCapture.CaptureScreenshot(Output+"FirstEncounter4K.png");yield return null;
        controller.StartRun();yield return null;hud.DescendButton.onClick.Invoke();
        Require(view.IsEncounterTransitionPlaying,"Second opening transition missing");
        controller.StartRun();yield return null;
        Require(!view.IsEncounterTransitionPlaying&&boat.gameObject.activeInHierarchy&&boat.parent==rest,"Interrupted restart left a hidden or moving boat");
        Require(controller.CurrentDepth==0&&controller.CurrentEncounter==null&&controller.CatchChain.Length==0,"Restart leaked encounter state");
    }
    /// <summary>Stops the coroutine at the first broken visual or state contract.</summary>
    private static void Require(bool value,string message){assertions++;if(!value)throw new InvalidOperationException(message);}
}
