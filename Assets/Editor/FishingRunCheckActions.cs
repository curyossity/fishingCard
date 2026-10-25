/// <summary>Performs explicit player choices in older controller fixtures without weakening production input rules.</summary>
public static class FishingRunCheckActions
{
    /// <summary>Chooses Catch for an unselected creature before the fixture's intended successful Descend.</summary>
    public static bool TryDescendAsCatch(FishingRunController controller)
    {
        if(controller.SelectedEncounterChoice==EncounterEffectChoice.None)
            controller.TryChooseEncounterRole(EncounterEffectChoice.Catch);
        return controller.TryDescend();
    }
}
