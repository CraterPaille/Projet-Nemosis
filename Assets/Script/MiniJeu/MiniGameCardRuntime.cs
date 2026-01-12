using UnityEngine;

public class MiniGameCardRuntime : MonoBehaviour
{
    public static MiniGameCardRuntime Instance { get; private set; }

    public MiniGameCardEffectSO SelectedCard { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectCard(MiniGameCardEffectSO card)
    {
        SelectedCard = card;
        Debug.Log($"[MiniGameCardRuntime] Carte sélectionnée : {card?.cardName}");
    }

    public void Clear()
    {
        SelectedCard = null;
    }
}