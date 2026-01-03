using UnityEngine;

public enum SoulType { Good, Neutral, Bad }

[RequireComponent(typeof(Collider2D))]
public class Soul : MonoBehaviour
{
    public enum MovementType { Vertical, Horizontal }
    public MovementType movementType = MovementType.Vertical;

    public SoulType type;
    public float fallSpeed = 2f;
    private Vector3 mouseOffset;

    // Indique si l'objet est actuellement déplacé (souris ou manette)
    [HideInInspector] public bool isBeingDragged = false;

    private void Update()
    {
        if (!TriGameManager.Instance.IsPlaying) return;

        // Ne pas appliquer le mouvement automatique si on est en train de draguer
        if (isBeingDragged) return;

        if (movementType == MovementType.Vertical)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }
        else if (movementType == MovementType.Horizontal)
        {
            transform.Translate(Vector3.right * fallSpeed * Time.deltaTime); // va de gauche à droite
        }

        if (transform.position.y < -6f || transform.position.x < -10f || transform.position.x > 10f)
        {
            TriGameManager.Instance.AddScore(-2);
            Destroy(gameObject);
        }
    }


    private void OnMouseDown()
    {
        if (!TriGameManager.Instance.IsPlaying) return;

        isBeingDragged = true;

        // Calcul du décalage entre la souris et le centre de l'objet
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseOffset = transform.position - new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
    }

    private void OnMouseDrag()
    {
        if (!TriGameManager.Instance.IsPlaying) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorldPos.x + mouseOffset.x, mouseWorldPos.y + mouseOffset.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        // Relâchement de la souris
        isBeingDragged = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SortingZone zone = other.GetComponent<SortingZone>();
        if (zone != null)
        {
            bool correct = zone.AcceptsSoul(type);

            if (correct)
                TriGameManager.Instance.AddScore(1);
            else
                TriGameManager.Instance.AddScore(-1);

            Destroy(gameObject);
        }
    }
}
