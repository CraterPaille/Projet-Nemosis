using UnityEngine;

public class LaneHighlight : MonoBehaviour
{
    [SerializeField] private SpriteRenderer highlightSprite;
    [SerializeField] private Color onColor = Color.white;
    [SerializeField] private float fadeDuration = 0.15f;

    private float _timer;

    private void Awake()
    {
        if (highlightSprite != null)
            highlightSprite.color = new Color(onColor.r, onColor.g, onColor.b, 0f);
    }

    private void Update()
    {
        if (highlightSprite == null || _timer <= 0f)
            return;

        _timer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(_timer / fadeDuration);
        float a = Mathf.Lerp(onColor.a, 0f, t);
        Color c = onColor;
        c.a = a;
        highlightSprite.color = c;
    }

    public void Trigger()
    {
        _timer = fadeDuration;
        if (highlightSprite != null)
            highlightSprite.color = onColor;
    }
}