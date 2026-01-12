using UnityEngine;

public class CameraShakeRhytm : MonoBehaviour
{
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private float magnitude = 0.1f;

    private Vector3 _originalPos;
    private float _timer;

    private void Awake()
    {
        _originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (_timer <= 0f) return;

        _timer -= Time.deltaTime;
        Vector2 offset = Random.insideUnitCircle * magnitude * (_timer / duration);
        transform.localPosition = _originalPos + (Vector3)offset;

        if (_timer <= 0f)
            transform.localPosition = _originalPos;
    }

    public void Play(float customDuration = -1f, float customMagnitude = -1f)
    {
        _timer = (customDuration > 0f) ? customDuration : duration;
        if (customMagnitude > 0f) magnitude = customMagnitude;
    }
}