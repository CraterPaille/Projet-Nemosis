using UnityEngine;
using System.Collections;
public class FollowPlayer : MonoBehaviour
{
    public Transform playerTransform;
    public float zOffset = -10f; // Distance de la caméra par rapport au joueur (négatif pour être en arrière)
    public Vector2 positionOffset = new Vector2(1f, 1f); // Décalage pour le confort visuel
    
    void Update()
    {
        if (playerTransform != null)
        {
            // Suit le joueur en X et Y avec un léger décalage, conserve la distance Z
            transform.position = new Vector3(
                playerTransform.position.x + positionOffset.x, 
                playerTransform.position.y + positionOffset.y, 
                zOffset
            );
        }
    }
}
