using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Le Joueur

    void Update()
    {
        // Si le joueur existe et qu'il est plus haut que la caméra...
        if (target != null && target.position.y > transform.position.y)
        {
            // La caméra monte
            Vector3 newPos = new Vector3(transform.position.x, target.position.y, transform.position.z);
            transform.position = newPos;
        }
    }
}