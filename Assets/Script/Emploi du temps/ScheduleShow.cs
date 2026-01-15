using System.Text;
using TMPro;
using UnityEngine;

public class ScheduleShow : MonoBehaviour
{
    [Header("Icônes")]
    public Sprite EventIcon;
    public Sprite MiniGameIcon;

    public DayTime currentTime;
    public GameManager gameManager;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI weekText;

    private EventScheduler eventScheduler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }
        eventScheduler = EventScheduler.Instance;

        UpdateWeekText();
    }

    public void UpdateWeekText()
    {
        if (gameManager == null || weekText == null)
        {
            Debug.LogWarning("[ScheduleShow] gameManager ou weekText est null.");
            return;
        }

        // Si pas d'EventScheduler, on affiche seulement les noms des jours
        if (eventScheduler == null)
        {
            BuildSimpleWeekText();
            return;
        }

        // Utilise la liste publique scheduledEvents au lieu d'une méthode inexistante
        var scheduled = eventScheduler.scheduledEvents; // IReadOnlyList<EventDay> possible

        StringBuilder sb = new StringBuilder();

        // On part du jour courant dans la campagne (currentDay) pour calculer les 7 prochains jours
        // et les mapper sur les noms de semaine (Lundi..Dimanche)
        for (int i = 0; i < gameManager.weekDays.Length; i++)
        {
            int absoluteDay = gameManager.currentDay - ((int)((gameManager.currentDay - 1) % 7)) + i;

            string weekDayName = gameManager.weekDays[i];

            // Chercher s'il y a un EventDay dont le "day" == absoluteDay
            string eventName = null;
            Sprite eventSprite = null;
            if (scheduled != null)
            {
                foreach (var eventDay in scheduled)
                {
                    if (eventDay.day == absoluteDay && eventDay.gameEvent != null)
                    {
                        var info = eventDay.gameEvent.GetEventInfo();

                        eventName = !string.IsNullOrEmpty(info.eventName)
                            ? info.eventName
                            : eventDay.gameEvent.name;

                        // sprite de l'événement : soit celui défini dans EventInfo, sinon l'icône par défaut
                        eventSprite = info.eventImage != null ? info.eventImage : EventIcon;
                        break;
                    }
                }
            }

            // Mini-jeu : tous les dimanches matin
            bool isSunday = weekDayName == "Dimanche";
            bool hasMiniGame = isSunday; // si ta logique change plus tard, adapte ici
            string miniGameName = hasMiniGame ? "\nMini-jeu (dimanche matin)" : null;
            Sprite miniGameSprite = hasMiniGame ? MiniGameIcon : null;

            // Marquer le jour de la semaine actuel visuellement
            bool isToday = weekDayName == gameManager.currentWeekDay;

            if (isToday)
                sb.Append("<b>"); // début gras

            sb.Append(weekDayName);
            sb.Append(" : ");

            // Affichage événement
            if (!string.IsNullOrEmpty(eventName))
            {
                if (eventSprite != null)
                    sb.Append("<sprite name=\"EventIcon\"> "); // ex : tag TMP pour un sprite (mapping par nom dans l'Atlas)

                sb.Append(eventName);
            }
            else
            {
                sb.Append("Aucun événement");
            }

            // Saut de ligne entre événement et mini-jeu si les deux existent
            if (!string.IsNullOrEmpty(eventName) && hasMiniGame)
            {
                sb.Append(" / ");
            }

            // Affichage mini-jeu (dimanche matin)
            if (hasMiniGame)
            {
                if (miniGameSprite != null)
                    sb.Append("<sprite name=\"MiniGameIcon\"> ");

                sb.Append(miniGameName);
            }

            if (isToday)
                sb.Append("</b>"); // fin gras

            if (i < gameManager.weekDays.Length - 1)
                sb.AppendLine();
        }

        weekText.text = sb.ToString();
    }

    private void BuildSimpleWeekText()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < gameManager.weekDays.Length; i++)
        {
            string weekDayName = gameManager.weekDays[i];
            bool isToday = weekDayName == gameManager.currentWeekDay;

            if (isToday) sb.Append("<b>");
            sb.Append(weekDayName);
            if (isToday) sb.Append("</b>");

            if (i < gameManager.weekDays.Length - 1)
                sb.Append(" | ");
        }

        weekText.text = sb.ToString();
    }
}