using UnityEngine;
using DG.Tweening;

public class CloudsAnimator : MonoBehaviour
{
    [Header("Déplacement du nuage")]
    public float moveSpeed = 2f; // Vitesse de déplacement du nuage
    public float cloudWidth = 32f; // Largeur du sprite du nuage (en unités monde, à ajuster selon votre sprite)

    private Camera mainCamera;
    private float leftBound;
    private float rightBound;

    private void Start()
    {
        mainCamera = Camera.main;
        float width = GetComponent<SpriteRenderer>().bounds.size.x;
        Debug.Log("Largeur du nuage : " + width);

        // Calcul des bords de la caméra
        float camHeight = 2f * mainCamera.orthographicSize;
        float camWidth = camHeight * mainCamera.aspect;
        Vector3 camPos = mainCamera.transform.position;
        leftBound = camPos.x - camWidth / 2 - cloudWidth / 2;
        rightBound = camPos.x + camWidth / 2 + cloudWidth / 2;

        // Animation YOYO avec DOTween
        float distance = rightBound - leftBound;
        float duration = distance / moveSpeed;

        // Place le nuage au départ
        Vector3 startPos = transform.position;
        startPos.x = leftBound;
        transform.position = startPos;

        // Animation aller-retour (YOYO) linéaire
        transform.DOMoveX(rightBound, duration)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);
    }
}