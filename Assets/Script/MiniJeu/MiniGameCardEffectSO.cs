using UnityEngine;

public enum MiniGameType
{
    Any,
    Rhythm,
    Chyron,
    Zeus,
    NuitGlaciale,
    Tri
}

[CreateAssetMenu(fileName = "MiniGameCardEffect", menuName = "Card/MiniGameEffect")]
public class MiniGameCardEffectSO : ScriptableObject
{
    [Header("Infos UI")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Cible")]
    public MiniGameType targetMiniGame = MiniGameType.Any;

    [Header("Paramètres génériques")]
    public float speedMultiplier = 1f;     // vitesse notes, scroll, etc.
    public float difficultyMultiplier = 1f;
    public bool invertControls = false;

    // Tu peux ajouter d'autres flags/spécifiques
    public bool moreEnemies = false;
    public bool lessEnemies = false;
}