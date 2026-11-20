using System.Collections.Generic;
using System.Linq;

/// <summary>Read-only positional ability attribution; strike conditions are deliberately not predicted by Build.</summary>
public sealed class RigAbilityLinks
{
    public IReadOnlyList<int> SupportingIds { get; }
    public IReadOnlyList<int> RecipientIds { get; }

    /// <summary>Copies unique identities without retaining a mutable rig or granting any effects.</summary>
    public RigAbilityLinks(IEnumerable<int> supports, IEnumerable<int> recipients)
    {
        SupportingIds = System.Array.AsReadOnly(supports.Distinct().ToArray());
        RecipientIds = System.Array.AsReadOnly(recipients.Distinct().ToArray());
    }
}
