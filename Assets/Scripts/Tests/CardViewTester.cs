using UnityEngine;

public sealed class CardViewTester : MonoBehaviour
{
    [SerializeField] private CardView cardView;
    [SerializeField] private CardDefinition cardDefinition;

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
