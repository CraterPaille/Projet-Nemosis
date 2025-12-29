using UnityEngine;

public class FogMover : MonoBehaviour
{
    [Header("Réglage mouvement")]
    public float speedX = 20f;          // vitesse horizontale en pixels / seconde
    public float travelDistanceX = 200; // amplitude horizontale
    public bool pingPong = true;

    [Header("Oscillation verticale")]
    public float verticalAmplitude = 20f;   // pixels
    public float verticalFrequency = 0.2f;  // Hz

    private RectTransform rect;
    private Vector2 startPos;
    private int direction = 1;
    private float time;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (rect != null)
            rect.anchoredPosition = startPos;
        direction = 1;
        time = 0f;
    }

    private void Update()
    {
        if (rect == null) return;

        time += Time.deltaTime;

        Vector2 pos = rect.anchoredPosition;

        // Mouvement horizontal
        pos.x += direction * speedX * Time.deltaTime;

        float offsetX = pos.x - startPos.x;

        if (pingPong)
        {
            if (offsetX > travelDistanceX)
            {
                offsetX = travelDistanceX;
                direction = -1;
            }
            else if (offsetX < -travelDistanceX)
            {
                offsetX = -travelDistanceX;
                direction = 1;
            }
        }
        else
        {
            if (offsetX > travelDistanceX)
                pos.x = startPos.x;
        }

        // Oscillation verticale douce
        pos.y = startPos.y + Mathf.Sin(time * Mathf.PI * 2f * verticalFrequency) * verticalAmplitude;

        rect.anchoredPosition = pos;
    }
}