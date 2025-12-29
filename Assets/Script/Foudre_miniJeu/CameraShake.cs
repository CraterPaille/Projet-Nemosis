using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Transform cam;
    private Vector3 originalPos;
    private bool shakeEnabled = false;

    void Awake()
    {
        cam = Camera.main.transform;
        originalPos = cam.localPosition;
    }

    public void SetShakeActive(bool on)
    {
        shakeEnabled = on;
    }

    public void Shake(float duration, float magnitude)
    {
        if (!shakeEnabled) return;
        StopAllCoroutines();
        StartCoroutine(DoShake(duration, magnitude));
    }

    IEnumerator DoShake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            cam.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        cam.localPosition = originalPos;
    }
}
