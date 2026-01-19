using UnityEngine;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("Chart & Notes")]
    public ChartLoader chartLoader;
    public GameObject notePrefab;
    public Transform spawnParent;

    [Header("Lanes & Keys")]
    public float[] lanePositions = { -1.5f, -0.5f, 0.5f, 1.5f };

    [Header("Timing (dérivé du BPM)")]
    [Tooltip("Nombre de temps (beats) d'avance entre le spawn et le hit")]
    public float beatsAhead = 4f; // 4 temps = 1 mesure en 4/4

    private float spawnLeadTime; // calculé à partir du BPM

    private AudioSource musicSource;
    private List<NoteData> notes;
    private int nextNoteIndex = 0;
    private bool isPlaying = false;

    void Start()
    {
        musicSource = FindFirstObjectByType<GameManagerRhytme>().theMusic;
        notes = chartLoader.loadedChart.notes;

        // calcule le temps d'avance en secondes à partir du BPM du chart
        float bpm = chartLoader.loadedChart.bpm;
        float secondsPerBeat = 60f / bpm;
        spawnLeadTime = beatsAhead * secondsPerBeat;
    }

    void Update()
    {
        if (!isPlaying && GameManagerRhytme.instance.StartPlaying)
            isPlaying = true;

        if (!isPlaying || nextNoteIndex >= notes.Count) return;

        float songPosition = musicSource.time - chartLoader.loadedChart.offset;

        while (nextNoteIndex < notes.Count && notes[nextNoteIndex].time <= songPosition + spawnLeadTime)
        {
            SpawnNote(notes[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    void SpawnNote(NoteData data)
    {
        if (lanePositions == null || lanePositions.Length == 0)
        {
            Debug.LogWarning("[NoteSpawner] lanePositions non configuré.");
            return;
        }

        // Lane venant du chart (0..3)
        int chartLane = Mathf.Clamp(data.lane, 0, lanePositions.Length - 1);

        // Lane logique (après inversion éventuelle par la carte)
        int logicLane = chartLane;
        if (GameManagerRhytme.instance != null)
        {
            logicLane = Mathf.Clamp(GameManagerRhytme.instance.GetLogicalLane(chartLane),
                                    0, lanePositions.Length - 1);
        }

        // Position X en fonction de la lane logique
        Vector3 spawnPos = new Vector3(lanePositions[logicLane], transform.position.y, 0f);
        GameObject note = Instantiate(notePrefab, spawnPos, Quaternion.identity, spawnParent);

        if (note.TryGetComponent(out NoteObject noteObj))
        {
            noteObj.lane = logicLane;       // IMPORTANT : la lane logique sert à l'input
            noteObj.duration = data.duration;
            noteObj.InitFromChart();        // orientation + hold
        }
    }
}
