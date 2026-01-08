using UnityEngine;

public class PunchScale : MonoBehaviour
{
    [SerializeField] private float punchScale = 1.2f;
    [SerializeField] private float duration = 0.15f;

    private Vector3 _baseScale;
    private float _timer;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Update()
    {
        if (_timer <= 0f) 
            return;

        _timer -= Time.deltaTime;
        float t = 1f - Mathf.Clamp01(_timer / duration);
        // Ease out
        float s = Mathf.Lerp(punchScale, 1f, t);
        transform.localScale = _baseScale * s;

        if (_timer <= 0f)
            transform.localScale = _baseScale;
    }

    public void Play()
    {
        _timer = duration;
    }
}