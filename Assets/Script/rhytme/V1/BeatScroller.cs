using UnityEngine;
using System.Collections;

public class BeatScroller : MonoBehaviour
{
    [Header("Vitesse globale des notes (unités par seconde)")]
    public float beatTempo = 5f; // même vitesse pour tout le monde

    public bool HasStarted;

    void Update()
    {
        if (!HasStarted)
            return;

        // Déplace TOUT ce qui est enfant de cet objet vers le bas
        transform.position -= new Vector3(0f, beatTempo * Time.deltaTime, 0f);
    }
}
