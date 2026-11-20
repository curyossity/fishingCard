using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>Readable before/after comparisons of real positional amounts and sale values, without predicting a strike.</summary>
public sealed class RigBuildChange
{
    public int SourceId { get; }
    public string ShortText { get; }
    public string Description { get; }
    public int Direction { get; }
    private RigBuildChange(int source,List<string> lines,bool gain,bool loss)
    {SourceId=source;ShortText=lines[0];Description=string.Join("\n",lines);Direction=gain==loss?0:gain?1:-1;}

    /// <summary>Compares model projections; completed acquisition grants and concealed sale values are not re-evaluated or exposed.</summary>
    public static RigBuildChange[] Compare(CardInstance[] before,CardInstance[] after)
    {
        var old=RigRuntime.ProjectEffects(before);var next=RigRuntime.ProjectEffects(after);var result=new List<RigBuildChange>();
        foreach(var member in after)
        {
            var previous=before.FirstOrDefault(m=>m.InstanceId==member.InstanceId);if(previous==null)continue;
            var lines=new List<string>();bool gain=false,loss=false;
            foreach(var item in next.Where(e=>e.SourceId==member.InstanceId))
            {
                var prior=old.FirstOrDefault(e=>e.SourceId==item.SourceId&&e.EffectIndex==item.EffectIndex);
                if(item.Amount==prior.Amount&&item.Positions==prior.Positions)continue;
                if(item.Positions!=prior.Positions&&item.Effect.Count!=RigPositionCount.Once)
                {
                    string noun=!item.Effect.PerPosition?"Active":item.Effect.Count==RigPositionCount.DistinctTagsBefore||item.Effect.Count==RigPositionCount.DistinctOtherTags?"Tags"
                        :item.Effect.Count==RigPositionCount.Center||item.Effect.Count==RigPositionCount.LineTip||item.Effect.Count==RigPositionCount.DifferentNeighbors?"Active":"Links";
                    lines.Add(noun+" "+prior.Positions+" → "+item.Positions);
                }
                if(item.Amount!=prior.Amount)lines.Add(Label(item.Effect.Kind)+" "+Amount(item.Effect.Kind,prior.Amount)+" → "+Amount(item.Effect.Kind,item.Amount));
                gain|=item.Amount>prior.Amount;loss|=item.Amount<prior.Amount;
            }
            if(!member.HidesOwnValueDuringRun&&!previous.HidesOwnValueDuringRun&&member.HaulValueContribution!=previous.HaulValueContribution)
            {lines.Add("Sale "+previous.HaulValueContribution+" → "+member.HaulValueContribution);gain|=member.HaulValueContribution>previous.HaulValueContribution;loss|=member.HaulValueContribution<previous.HaulValueContribution;}
            if(lines.Count==0)
            {
                var a=RigRuntime.InspectAbility(before,member.InstanceId);var b=RigRuntime.InspectAbility(after,member.InstanceId);
                if(!a.SupportingIds.OrderBy(i=>i).SequenceEqual(b.SupportingIds.OrderBy(i=>i))||!a.RecipientIds.OrderBy(i=>i).SequenceEqual(b.RecipientIds.OrderBy(i=>i)))lines.Add("Links changed");
            }
            if(lines.Count>0)result.Add(new RigBuildChange(member.InstanceId,lines,gain,loss));
        }
        return result.ToArray();
    }
    /// <summary>Names the operation rather than implying that all preview amounts are unconditional Landing Points.</summary>
    private static string Label(RigAbilityKind kind)
    {
        switch(kind)
        {
            case RigAbilityKind.LandingBonus:return "Bonus";case RigAbilityKind.LandingMultiplier:return "Multiplier";
            case RigAbilityKind.ExtraAttempts:return "Attempts";case RigAbilityKind.MissProtection:return "Miss guards";
            case RigAbilityKind.GoodWidth:return "Good width";case RigAbilityKind.PerfectWidth:return "Perfect width";
            case RigAbilityKind.FishSlowInGood:return "Slow in Good";case RigAbilityKind.PerfectBase:return "Perfect base";
            case RigAbilityKind.ValueBonus:return "Sale bonus";case RigAbilityKind.ValueMultiplier:return "Sale multiplier";
            default:return "Insertion guard";
        }
    }
    /// <summary>Formats display precision without changing the projected decimal values.</summary>
    private static string Amount(RigAbilityKind kind,decimal value)
    {
        bool multiplier=kind==RigAbilityKind.LandingMultiplier||kind==RigAbilityKind.ValueMultiplier;
        bool percent=kind==RigAbilityKind.GoodWidth||kind==RigAbilityKind.PerfectWidth||kind==RigAbilityKind.FishSlowInGood;
        return (multiplier?"×":"")+(multiplier?1+value:percent?value*100:value).ToString("0.##",CultureInfo.InvariantCulture)+(percent?"%":"");
    }
}
