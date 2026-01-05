using UnityEngine;

public class Lame : MonoBehaviour
{

    public bool isForged = false;
    public GameObject forge;
    public float type = 1f;
    void Start()
    {
        
    }
    void OnMouseDown()
    {
        Debug.Log("Lame clicked");
        if (forge == null)
        {
            return;
        }
        TimerForge bladeForge = forge.GetComponent<TimerForge>();
        if (bladeForge == null)
        {
            Debug.LogWarning("TimerForge component not found on forge GameObject.");
            return;
        }

        bool queued = bladeForge.EnqueueLame(this);
        if (queued)
        {
            Debug.Log($"Blade '{name}' queued for forging.");
        }
        else
        {
            Debug.Log($"Blade '{name}' could not be queued (forge full).");
        }
    }
    void Update()
    {
        
    }
}
