using UnityEngine;

/// <summary>
/// Gère uniquement le layout visuel des lanes (boutons / colonnes).
/// Permet de permuter 0 & 3, 1 & 2 quand les contrôles sont inversés.
/// </summary>
public class RhythmLaneLayout : MonoBehaviour
{
    [Header("Références visuelles par lane (0 = gauche, 3 = droite)")]
    [SerializeField] private Transform lane0Visual;
    [SerializeField] private Transform lane1Visual;
    [SerializeField] private Transform lane2Visual;
    [SerializeField] private Transform lane3Visual;

    private Vector3 _lane0Pos;
    private Vector3 _lane1Pos;
    private Vector3 _lane2Pos;
    private Vector3 _lane3Pos;

    private void Awake()
    {
        if (lane0Visual != null) _lane0Pos = lane0Visual.position;
        if (lane1Visual != null) _lane1Pos = lane1Visual.position;
        if (lane2Visual != null) _lane2Pos = lane2Visual.position;
        if (lane3Visual != null) _lane3Pos = lane3Visual.position;
    }

    /// <summary>
    /// Applique (ou retire) le layout inversé.
    /// </summary>
    public void ApplyInverted(bool inverted)
    {
        if (!inverted)
        {
            // Position normale
            if (lane0Visual != null) lane0Visual.position = _lane0Pos;
            if (lane1Visual != null) lane1Visual.position = _lane1Pos;
            if (lane2Visual != null) lane2Visual.position = _lane2Pos;
            if (lane3Visual != null) lane3Visual.position = _lane3Pos;
        }
        else
        {
            // Layout inversé : 0<->3, 1<->2
            if (lane0Visual != null) lane0Visual.position = _lane3Pos;
            if (lane1Visual != null) lane1Visual.position = _lane2Pos;
            if (lane2Visual != null) lane2Visual.position = _lane1Pos;
            if (lane3Visual != null) lane3Visual.position = _lane0Pos;
        }
    }
}