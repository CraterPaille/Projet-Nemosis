using UnityEngine;

public class SortingZone : MonoBehaviour
{
    public SoulType acceptedType;

    public bool AcceptsSoul(SoulType soulType)
    {
        return soulType == acceptedType;
    }

    private void OnDrawGizmos()
    {
        // Pour bien visualiser dans la scène
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
    }
}
