using UnityEngine;

/// <summary>Fits any physical hand count to the approved shallow fan without changing deck rules.</summary>
public static class JournalTechniqueFan
{
    /// <summary>Returns a bounded, symmetric resting pose; four cards match the approved Penpot centers and angles.</summary>
    public static void Evaluate(int index, int count, float spacing, float tilt, out Vector3 position, out Quaternion rotation)
    {
        if (count <= 0) { position = Vector3.zero; rotation = Quaternion.identity; return; }
        float center = Mathf.Clamp(index, 0, count - 1) - (count - 1) * .5f;
        float compression = Mathf.Min(1, 3f / Mathf.Max(1, count - 1));
        float curveIndex = center * compression;
        float nearest = count % 2 == 0 ? .5f * compression : 0;
        float step = Mathf.Min(Mathf.Max(0, spacing), 1500f / Mathf.Max(1, count - 1));
        float angle = Mathf.Abs(curveIndex) <= .5f ? Mathf.Abs(curveIndex) * 10
            : Mathf.Lerp(5, 8, Mathf.InverseLerp(.5f, 1.5f, Mathf.Abs(curveIndex)));
        // Screen-down Y450 is seventeen below the projection center. More cards compress horizontally, never deepen the arc.
        position = new Vector3(center * step, -17 - 25 * (curveIndex * curveIndex - nearest * nearest), 0);
        rotation = Quaternion.Euler(tilt, 0, -Mathf.Sign(curveIndex) * angle);
    }
}
