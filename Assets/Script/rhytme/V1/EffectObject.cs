using UnityEngine;

public class EffectObject : MonoBehaviour
{
    public float lifetime = 1f;

    void Start()
    {
        Destroy(gameObject, lifetime); // détruit automatiquement après 'lifetime'
    }
}
