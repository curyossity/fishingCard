using UnityEngine;

public sealed class CardViewTester : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardDefinition cardDefinition;

    /// <summary>
    /// Assigns the configured test card to the configured card view when the scene starts.
    /// </summary>
    private void Start()
    {
        if (cardView == null)
        {
            Debug.LogWarning("CardViewTester needs a CardView reference.", this);
            return;
        }

        if (cardDefinition == null)
        {
            Debug.LogWarning("CardViewTester needs a CardDefinition reference.", this);
            return;
        }

        cardView.SetCard(cardDefinition);
    }
}
