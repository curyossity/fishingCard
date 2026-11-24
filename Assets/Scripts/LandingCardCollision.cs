using UnityEngine;

/// <summary>Gives the machine's creature and score cards a weighted collision on the existing authoritative score schedule.</summary>
public sealed class LandingCardCollision : MonoBehaviour
{
    [SerializeField] private RectTransform creatureCard, scoreCard, stage;
    [SerializeField] private CanvasGroup creatureFade;
    [SerializeField, Range(8, 60)] private float pullback = 34;
    [SerializeField, Range(2, 30)] private float scoreRecoil = 22;
    [SerializeField, Range(0, 20)] private float shake = 10;
    [SerializeField, Range(0, .18f)] private float windupSquash = .08f;
    [SerializeField, Range(0, .18f)] private float dashStretch = .12f;
    [SerializeField, Range(0, .18f)] private float impactSquash = .12f;
    private Vector2 creatureRest, scoreRest, stageRest;
    private Vector3 creatureScale, scoreScale;
    private Quaternion creatureRotation, scoreRotation;
    private bool captured;
    private float faceAlpha;
    public Vector2 ScreenShakeOffset { get; private set; }
    public float CreatureOffset => captured ? creatureCard.anchoredPosition.x - creatureRest.x : 0;
    public float ScoreOffset => captured ? scoreCard.anchoredPosition.x - scoreRest.x : 0;
    public float ContactDistance => scoreRest.x - scoreCard.rect.width * scoreScale.x * .5f
        - creatureRest.x - creatureCard.rect.width * creatureScale.x * .5f + 10 * creatureScale.x;
    public float CreatureLeadingOffset => CreatureOffset + (creatureCard.localScale.x - creatureScale.x) * creatureCard.rect.width * .5f
        * Mathf.Cos(creatureCard.localEulerAngles.z * Mathf.Deg2Rad);

    /// <summary>Captures authored baselines once for this receipt, after the previous presentation has restored its transforms.</summary>
    public void Begin()
    {
        Clear(); creatureRest = creatureCard.anchoredPosition; scoreRest = scoreCard.anchoredPosition; stageRest = stage.anchoredPosition;
        creatureRotation = creatureCard.localRotation; scoreRotation = scoreCard.localRotation;
        creatureScale = creatureCard.localScale; scoreScale = scoreCard.localScale; faceAlpha = creatureFade.alpha; captured = true;
    }

    /// <summary>Samples a hidden-face handoff, elastic windup, directional stretch and compressed contact without changing score timing or aiming.</summary>
    public void Render(LandingScoreSequence sequence, float elapsed, bool reduced)
    {
        if (!captured) return;
        ResetMotion(); int index = sequence.StepAt(elapsed); var step = sequence.Steps[index];
        float handoffDuration = LandingCardHandoff.DurationFor(sequence, index);
        float handoff = LandingCardHandoff.Progress(sequence, index, elapsed);
        if (handoff < 1)
        {
            creatureFade.alpha = faceAlpha * LandingCardHandoff.FaceOpacity(handoff, reduced);
            if (!reduced) creatureCard.localScale = new Vector3(creatureScale.x * Mathf.Max(.015f, Mathf.Abs(Mathf.Cos(handoff * Mathf.PI))), creatureScale.y, creatureScale.z);
            return;
        }
        if (reduced) return;
        float strength = step.SourceId < 0 ? (sequence.Score.Base > 0 ? .24f : 0) : step.Strength;
        bool strikes = step.SourceId >= 0 || sequence.Score.Base > 0;
        if (strikes && elapsed >= step.Time && elapsed < step.EndAt)
        {
            float weight = .55f + strength * .45f;
            float x, width = 1, rotation = 0, back = pullback * weight;
            if (elapsed < step.LaunchAt)
            {
                float start = step.Time + handoffDuration;
                float u = Mathf.Clamp01((elapsed - start) / Mathf.Max(.001f, step.LaunchAt - start));
                float windup = u * u * (3 - 2 * u); x = -back * windup; rotation = windup * (1 + strength);
                float squashStart = Mathf.Max(start, step.LaunchAt - .09f);
                width = 1 - windupSquash * weight * Mathf.SmoothStep(0, 1, Mathf.InverseLerp(squashStart, step.LaunchAt, elapsed));
            }
            else if (elapsed < step.ImpactAt)
            {
                float u = Mathf.Clamp01((elapsed - step.LaunchAt) / (step.ImpactAt - step.LaunchAt));
                float dash = u * u; x = Mathf.Lerp(-back, ContactDistance, dash); rotation = (1 - dash) * (1 + strength);
                width = Mathf.Lerp(1 - windupSquash * weight, 1 + dashStretch * weight, Mathf.SmoothStep(0, 1, Mathf.Clamp01(u / .65f)));
            }
            else
            {
                float age = elapsed - step.ImpactAt, hold = .075f + strength * .030f;
                float returnTime = Mathf.Clamp01((age - hold) / Mathf.Max(.001f, step.EndAt - step.ImpactAt - hold));
                float settle = 1 - returnTime; settle = settle * settle * (3 - 2 * settle);
                x = (ContactDistance + Recoil(age, strength)) * settle;
                rotation = -Mathf.Sin(returnTime * Mathf.PI) * (1 + strength * .5f);
                if (age < .025f)
                    width = Mathf.Lerp(1 + dashStretch * weight, 1 - impactSquash * weight, Mathf.SmoothStep(0, 1, age / .025f));
                else if (age < hold) width = 1 - impactSquash * weight;
                else width = 1 - impactSquash * weight * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01(returnTime / .5f)))
                    + .025f * weight * Mathf.Sin(Mathf.PI * Mathf.Clamp01((returnTime - .5f) / .5f));
            }
            ApplyCreaturePose(x, rotation, width);
        }
        int arrival = sequence.ImpactStepAt(elapsed); var hit = sequence.Steps[arrival];
        float hitAge = elapsed - hit.ImpactAt;
        float force = arrival == 0 ? (sequence.Score.Base > 0 ? .24f : 0) : hit.Strength;
        if (hitAge < 0 || hitAge > .52f || force <= 0) return;
        float recoil = Recoil(hitAge, force);
        scoreCard.anchoredPosition = scoreRest + Vector2.right * recoil;
        scoreCard.localRotation = scoreRotation * Quaternion.Euler(0, 0, -recoil * .055f);
        // Overlay UI does not move with a world camera: shake its visual stage, leaving normalized aiming coordinates intact.
        float envelope = hitAge > .32f ? 0 : (1 - Mathf.Exp(-hitAge * 170)) * Mathf.Exp(-hitAge * 18) * shake * (.25f + force * .75f);
        ScreenShakeOffset = new Vector2(Mathf.Sin(hitAge * 100), Mathf.Sin(hitAge * 137) * .43f) * envelope;
        stage.anchoredPosition = stageRest + ScreenShakeOffset;
    }

    /// <summary>Preserves apparent area and pins the leading edge to its original trajectory through every shape change.</summary>
    private void ApplyCreaturePose(float offset, float rotation, float width)
    {
        creatureCard.localRotation = creatureRotation * Quaternion.Euler(0, 0, rotation);
        creatureCard.localScale = new Vector3(creatureScale.x * width, creatureScale.y / width, creatureScale.z);
        Vector2 correction = (Vector2)(creatureCard.localRotation * Vector3.right) * (creatureCard.rect.width * creatureScale.x * .5f * (1 - width));
        creatureCard.anchoredPosition = creatureRest + Vector2.right * offset + correction;
    }

    /// <summary>Transfers a quick directional impulse into a heavier, slower settling score panel.</summary>
    private float Recoil(float age, float strength) => age < 0 || age > .52f ? 0
        : (1 - Mathf.Exp(-age * 115)) * Mathf.Exp(-age * 9) * Mathf.Cos(age * 17) * scoreRecoil * (.4f + strength * .6f)
            * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.35f, .52f, age)));

    /// <summary>Restores visual baselines immediately when reduced motion is selected, retaining the current receipt snapshot.</summary>
    public void ResetMotion()
    {
        ScreenShakeOffset = Vector2.zero; if (!captured) return;
        creatureCard.anchoredPosition = creatureRest; creatureCard.localRotation = creatureRotation;
        scoreCard.anchoredPosition = scoreRest; scoreCard.localRotation = scoreRotation; stage.anchoredPosition = stageRest;
        creatureCard.localScale = creatureScale; scoreCard.localScale = scoreScale;
        creatureFade.alpha = faceAlpha;
    }

    /// <summary>Restores both cards and the stage on completion, skip, replacement or phase exit.</summary>
    public void Clear() { ResetMotion(); captured = false; }

    /// <summary>Cannot leave the camera view offset when this component is disabled independently.</summary>
    private void OnDisable() => Clear();
}
