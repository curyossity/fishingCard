using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>Owns three tag reels independently of artwork, encounter matching and landing rules.</summary>
public sealed class TagSlotMachineRuntime
{
    public const int ReelCount = 3;

    /// <summary>One authored symbol and its relative integer probability on a reel.</summary>
    public readonly struct Symbol
    {
        public string Tag { get; }
        public int Weight { get; }

        /// <summary>Creates a selectable tag; invalid authored data is rejected before a session starts.</summary>
        public Symbol(string tag, int weight)
        {
            if (string.IsNullOrWhiteSpace(tag)) throw new ArgumentException("A reel symbol needs a tag.", nameof(tag));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
            Tag = tag.Trim();
            Weight = weight;
        }
    }

    /// <summary>Immutable accepted symbols; duplicate counts do not imply a particular encounter reward.</summary>
    public sealed class Result
    {
        public long Occurrence { get; }
        public IReadOnlyList<string> Tags { get; }

        /// <summary>Copies settled symbols so future sessions cannot change a committed result.</summary>
        internal Result(long occurrence, string[] tags)
        {
            Occurrence = occurrence;
            Tags = new ReadOnlyCollection<string>((string[])tags.Clone());
        }

        /// <summary>Counts matching reel faces without applying encounter-selection or rarity policy.</summary>
        public int Count(string tag)
        {
            int count = 0;
            foreach (string symbol in Tags)
                if (string.Equals(symbol, tag, StringComparison.OrdinalIgnoreCase)) count++;
            return count;
        }
    }

    private Symbol[][] strips = Array.Empty<Symbol[]>();
    private readonly string[] tags = new string[ReelCount];
    private readonly bool[] locked = new bool[ReelCount];
    private int[] totalWeights = Array.Empty<int>();
    public long Occurrence { get; private set; }
    public long Revision { get; private set; }
    public long SpinRevision { get; private set; }
    public bool Active { get; private set; }
    public bool HasSpun { get; private set; }
    public int RespinsRemaining { get; private set; }
    public Result AcceptedResult { get; private set; }
    public bool CanAccept => Active && HasSpun;
    public bool CanSpin => Active && (!HasSpun || RespinsRemaining > 0 && LockedReelCount() == 1);

    /// <summary>Copies and validates all reel distributions before replacing the current session.</summary>
    public void Begin(IReadOnlyList<Symbol>[] reelSymbols, int allowedRespins)
    {
        if (reelSymbols == null || reelSymbols.Length != ReelCount)
            throw new ArgumentException("The tag machine requires exactly three reels.", nameof(reelSymbols));
        if (allowedRespins < 0) throw new ArgumentOutOfRangeException(nameof(allowedRespins));
        var nextStrips = new Symbol[ReelCount][];
        var nextTotals = new int[ReelCount];
        for (int reel = 0; reel < ReelCount; reel++)
        {
            IReadOnlyList<Symbol> source = reelSymbols[reel];
            if (source == null || source.Count == 0)
                throw new ArgumentException("Every reel requires at least one symbol.", nameof(reelSymbols));
            nextStrips[reel] = new Symbol[source.Count];
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < source.Count; i++)
            {
                Symbol symbol = source[i];
                if (string.IsNullOrWhiteSpace(symbol.Tag) || symbol.Weight <= 0 || !seen.Add(symbol.Tag))
                    throw new ArgumentException("Reel tags must be unique and have positive weights.", nameof(reelSymbols));
                nextTotals[reel] = checked(nextTotals[reel] + symbol.Weight);
                nextStrips[reel][i] = symbol;
            }
        }

        Reset();
        strips = nextStrips;
        totalWeights = nextTotals;
        RespinsRemaining = allowedRespins;
        Active = true;
    }

    /// <summary>Returns one settled face, or null before the first spin; callers cannot mutate reel state.</summary>
    public string GetTag(int reel)
    {
        ValidateReel(reel);
        return tags[reel];
    }

    /// <summary>Reports the authoritative lock state of a reel.</summary>
    public bool IsLocked(int reel)
    {
        ValidateReel(reel);
        return locked[reel];
    }

    /// <summary>Changes a settled reel's lock only for the exact current state and while respins remain.</summary>
    public bool TrySetLocked(long expectedRevision, int reel, bool value)
    {
        if (expectedRevision != Revision || !Active || !HasSpun || RespinsRemaining == 0
            || reel < 0 || reel >= ReelCount || locked[reel] == value
            || value && LockedReelCount() > 0) return false;
        locked[reel] = value;
        Revision++;
        return true;
    }

    /// <summary>Rolls unlocked reels atomically; initial rolls do not consume the configured respin allowance.</summary>
    public bool TrySpin(long expectedRevision, Random random)
    {
        if (expectedRevision != Revision || !CanSpin) return false;
        if (random == null) throw new ArgumentNullException(nameof(random));
        var rolled = (string[])tags.Clone();
        for (int reel = 0; reel < ReelCount; reel++)
        {
            if (locked[reel]) continue;
            int ticket = random.Next(totalWeights[reel]);
            foreach (Symbol symbol in strips[reel])
            {
                ticket -= symbol.Weight;
                if (ticket < 0) { rolled[reel] = symbol.Tag; break; }
            }
        }
        Array.Copy(rolled, tags, ReelCount);
        if (HasSpun) RespinsRemaining--;
        HasSpun = true;
        Revision++;
        SpinRevision = Revision;
        return true;
    }

    /// <summary>Commits the current faces once; the encounter owner interprets this immutable result.</summary>
    public bool TryAccept(long expectedRevision, out Result result)
    {
        result = null;
        if (expectedRevision != Revision || !CanAccept) return false;
        AcceptedResult = result = new Result(Occurrence, tags);
        Active = false;
        Revision++;
        return true;
    }

    /// <summary>Clears expedition-local state and invalidates callbacks without reusing session identities.</summary>
    public void Reset()
    {
        Occurrence++;
        Revision++;
        Active = HasSpun = false;
        SpinRevision = 0;
        RespinsRemaining = 0;
        AcceptedResult = null;
        strips = Array.Empty<Symbol[]>();
        totalWeights = Array.Empty<int>();
        Array.Clear(tags, 0, tags.Length);
        Array.Clear(locked, 0, locked.Length);
    }

    /// <summary>Enforces exactly one preserved face for the optional respin.</summary>
    private int LockedReelCount()
    {
        int count = 0;
        for (int i = 0; i < ReelCount; i++) if (locked[i]) count++;
        return count;
    }

    /// <summary>Rejects invalid read indices consistently.</summary>
    private static void ValidateReel(int reel)
    {
        if (reel < 0 || reel >= ReelCount) throw new ArgumentOutOfRangeException(nameof(reel));
    }
}
