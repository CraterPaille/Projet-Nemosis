using UnityEngine;
using System;


public abstract class Card : ScriptableObject
{
    public string cardName;
    public Sprite icon;
    [TextArea] public string description;
    //public event Action OnCardApplied; // Notifie après effet

    public CardSet cardSet;
    
    public enum CardSet
    {
        MiniJeu,
        Village,
        Relations
    }
}