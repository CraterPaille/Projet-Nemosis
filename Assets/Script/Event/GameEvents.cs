using System;
public enum StatType
{
    Foi,
    Food,
    Human,
    Or,
    Nemosis
}
public static class GameEvents
{
    public static event Action OnDayEnd;
    public static event Action OnMorningEnd;
    public static event Action<Card, int> OnCardPlayed;
    public static event Action<StatType, float> OnStatChanged;

    public static void TriggerDayEnd() => OnDayEnd?.Invoke();
    public static void TriggerMorningEnd() => OnMorningEnd?.Invoke();
    public static void TriggerCardPlayed(Card card, int numero = 0) => OnCardPlayed?.Invoke(card, numero);
    public static void TriggerStatChanged(StatType type, float amount) => OnStatChanged?.Invoke(type, amount);
}
