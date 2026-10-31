using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>Records shared creature/event/Technique regressions for the unlimited-hand rule.</summary>
public static class UnlimitedTechniqueHandChecks
{
    public const string Output = "Docs/Validation/UnlimitedTechniqueHand/";

    /// <summary>Verifies real effect ordering, recovery identity and equipment tradeoffs without changing player saves.</summary>
    public static void ValidateRules()
    {
        Directory.CreateDirectory(Output);
        var results = new List<string>();
        Check(results, "Technique rules", TechniqueRuleChecks.ValidateRules);
        Check(results, "Creature Release", CreatureReleaseChecks.ValidateRules);
        Check(results, "Bass Urchin Octopus", BassUrchinOctopusChecks.ValidateRules);
        Check(results, "Shell Feeding Exchange", ShellFeedingExchangeChecks.ValidateRules);
        Check(results, "Lobster Squid Eel", LobsterSquidEelChecks.ValidateRules);
        Check(results, "Confirmed event actions", EventActionChecks.ValidateRules);
        Check(results, "Event reactions", EventResolutionChecks.ValidateRules);
        Check(results, "Coastal events", CoastalEventChecks.ValidateRules);
        Check(results, "Event Techniques", EventTechniqueChecks.ValidateRules);
        Check(results, "Approved Techniques", ApprovedTechniqueChecks.ValidateRules);
        Check(results, "Authored Techniques", ApprovedTechniqueChecks.ValidateAuthoredAssets);
        TackleShopChecks.ValidateRules(Output);
        File.WriteAllText(Output + "RegressionVerification.json", "{\"passed\":true,\"suites\":[" + string.Join(",", results) + "]}");
    }

    /// <summary>Stops immediately on a violated real runtime invariant and records each passing suite's assertion count.</summary>
    private static void Check(List<string> results, string name, Func<int> test)
    {
        int count = test();
        results.Add("{\"name\":\"" + name + "\",\"assertions\":" + count + "}");
    }
}
