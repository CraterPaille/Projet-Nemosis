using UnityEngine;

public class BeatPulse : MonoBehaviour
{
    [SerializeField] private float pulseScale = 1.05f;
    [SerializeField] private float pulseDuration = 0.12f;
    [SerializeField] private BeatScroller beatScroller;

    private float _secondsPerBeat;
    private float _timer;
    private Vector3 _baseScale;

    private void Awake()
    {
        _baseScale = transform.localScale;
    }

    private void Start()
    {
        if (beatScroller != null && beatScroller.beatTempo > 0f)
        {
            // beatTempo = unités / seconde, tu as déjà ton BPM dans le chart;
            // plus simple : expose secondsPerBeat dans BeatScroller ou calcule ici.
            // Pour un truc générique, on te laisse un champ public secondsPerBeat si besoin.
        }
    }

    private void Update()
    {
        if (beatScroller == null || !beatScroller.HasStarted)
            return;

        // On utilise une simple horloge plutôt qu’un vrai BPM précis
        _timer += Time.deltaTime;
        if (_timer >= 0.5f) // approx: un pulse toutes les 0.5s (à ajuster)
        {
            _timer = 0f;
            StartCoroutine(Pulse());
        }
    }

    private System.Collections.IEnumerator Pulse()
    {
        float t = 0f;
        while (t < pulseDuration)
        {
            t += Time.deltaTime;
            float k = 1f + (pulseScale - 1f) * Mathf.Sin((t / pulseDuration) * Mathf.PI);
            transform.localScale = _baseScale * k;
            yield return null;
        }
        transform.localScale = _baseScale;
    }
}