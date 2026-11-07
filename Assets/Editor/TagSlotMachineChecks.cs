using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic checks for reel ownership, probability tickets and occurrence-safe commands.</summary>
public static class TagSlotMachineChecks
{
    /// <summary>Runs the artwork-independent slot-state checks from the Editor menu.</summary>
    [MenuItem("Fishing Cards/Validation/Tag Slot Machine State")]
    public static void ValidateFromMenu()
    {
        Debug.Log("Tag slot machine state checks passed: " + ValidateRules());
    }

    /// <summary>Checks state invariants without approving an encounter match, rarity or respin tuning policy.</summary>
    public static int ValidateRules()
    {
        int assertions = 0;
        Action<bool, string> check = (condition, message) =>
        {
            if (!condition) throw new InvalidOperationException(message);
            assertions++;
        };
        var fish = new TagSlotMachineRuntime.Symbol("Fish", 1);
        var school = new TagSlotMachineRuntime.Symbol("Schooling", 2);
        var faces = new[] { fish, school };
        IReadOnlyList<TagSlotMachineRuntime.Symbol>[] reels = { faces, faces, faces };
        var machine = new TagSlotMachineRuntime();
        check(!machine.CanSpin && !machine.CanAccept, "Inactive machine accepted input.");
        machine.Begin(reels, 1);
        long occurrence = machine.Occurrence;
        long initial = machine.Revision;
        check(machine.CanSpin && !machine.CanAccept, "Initial state is not awaiting a spin.");
        check(!machine.TryAccept(initial, out _), "Unrolled symbols were accepted.");
        check(!machine.TrySetLocked(initial, 0, true), "Unrolled reel was locked.");
        check(machine.TrySpin(initial, new Tickets(0, 1, 2)), "Initial spin rejected.");
        check(machine.GetTag(0) == "Fish" && machine.GetTag(1) == "Schooling"
            && machine.GetTag(2) == "Schooling", "Weighted ticket boundaries are incorrect.");
        check(machine.RespinsRemaining == 1 && machine.CanAccept, "Initial spin spent a respin.");
        check(!machine.TrySpin(initial, new Tickets()), "Duplicate spin callback accepted.");
        long beforeLock = machine.Revision;
        check(machine.TrySetLocked(beforeLock, 0, true), "Settled reel could not be locked.");
        check(!machine.TryAccept(beforeLock, out _), "Stale acceptance survived a lock change.");
        check(machine.TrySpin(machine.Revision, new Tickets(0, 0)), "Respin rejected.");
        check(machine.GetTag(0) == "Fish" && machine.GetTag(1) == "Fish"
            && machine.GetTag(2) == "Fish", "Locked face changed or unlocked reels did not roll.");
        check(machine.RespinsRemaining == 0 && !machine.CanSpin, "Respin limit was ignored.");
        check(!machine.TrySetLocked(machine.Revision, 0, false), "Locks changed after allowance ran out.");
        check(!machine.TrySpin(machine.Revision, new Tickets()), "Exhausted machine spun again.");
        check(machine.TryAccept(machine.Revision, out var result), "Valid result rejected.");
        check(!machine.Active && !machine.CanSpin && !machine.CanAccept, "Acceptance did not close input.");
        check(result.Occurrence == occurrence && result.Count("fish") == 3, "Result lost symbols or identity.");
        check(!machine.TryAccept(machine.Revision, out _), "Result accepted twice.");
        long oldRevision = machine.Revision;
        machine.Begin(reels, 2);
        check(machine.Occurrence != occurrence && !machine.HasSpun && !machine.IsLocked(0)
            && machine.AcceptedResult == null && machine.GetTag(0) == null, "New session retained old state.");
        check(!machine.TrySpin(oldRevision, new Tickets()), "Previous session callback accepted.");
        check(result.Count("Fish") == 3, "New session mutated an accepted snapshot.");
        machine.TrySpin(machine.Revision, new Tickets(0, 0, 0));
        check(!machine.TrySpin(machine.Revision, new Tickets()), "Respin without a lock accepted.");
        check(machine.TrySetLocked(machine.Revision, 0, true), "Could not lock one reel.");
        check(!machine.TrySetLocked(machine.Revision, 1, true), "Two locked reels accepted.");
        check(machine.TrySetLocked(machine.Revision, 0, false), "Could not unlock a reel.");
        check(machine.TrySetLocked(machine.Revision, 1, true), "Could not change locked reel.");
        check(machine.TrySpin(machine.Revision, new Tickets(2, 2)) && machine.GetTag(0) == "Schooling",
            "Unlock did not restore respin availability.");
        long validRevision = machine.Revision;
        try { machine.Begin(reels, -1); throw new Exception("Negative allowance accepted."); }
        catch (ArgumentOutOfRangeException) { assertions++; }
        check(machine.Revision == validRevision && machine.Active, "Invalid settings destroyed the current session.");
        try
        {
            machine.Begin(new IReadOnlyList<TagSlotMachineRuntime.Symbol>[] { faces, faces, Array.Empty<TagSlotMachineRuntime.Symbol>() }, 1);
            throw new Exception("Empty reel accepted.");
        }
        catch (ArgumentException) { assertions++; }
        check(machine.Revision == validRevision, "Invalid reel changed state.");
        var duplicate = new[] { fish, new TagSlotMachineRuntime.Symbol("fish", 2) };
        try
        {
            machine.Begin(new IReadOnlyList<TagSlotMachineRuntime.Symbol>[] { duplicate, faces, faces }, 1);
            throw new Exception("Duplicate tag accepted.");
        }
        catch (ArgumentException) { assertions++; }
        // Mutating caller-owned authoring data after Begin must not change the reserved probabilities.
        machine.Begin(reels, 0);
        faces[0] = new TagSlotMachineRuntime.Symbol("Predator", 10);
        check(machine.TrySpin(machine.Revision, new Tickets(0, 0, 0)) && machine.GetTag(0) == "Fish",
            "Caller mutated the reserved reel definition.");
        check(!machine.CanSpin && machine.CanAccept, "Zero-respin configuration is invalid.");
        machine.Reset();
        check(!machine.Active && machine.GetTag(0) == null && !machine.IsLocked(0), "Reset retained reel state.");
        return assertions;
    }

    /// <summary>Supplies exact probability tickets and fails on extra random calls, including locked reels.</summary>
    private sealed class Tickets : System.Random
    {
        private readonly Queue<int> tickets;

        /// <summary>Copies the exact sequence to consume.</summary>
        public Tickets(params int[] values) { tickets = new Queue<int>(values); }

        /// <summary>Rejects invalid tickets instead of silently masking a probability bug.</summary>
        public override int Next(int maxValue)
        {
            int value = tickets.Dequeue();
            if (value < 0 || value >= maxValue) throw new InvalidOperationException("Invalid random ticket.");
            return value;
        }
    }
}
