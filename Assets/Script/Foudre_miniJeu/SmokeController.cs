using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem; 

public class SmokeController : MonoBehaviour
{
    public VisualEffect smokeVFX;

    [Header("Dissipation")]
    public float dissipateRadius = 5f; // rayon en unités monde
    public string centerPropertyName = "DissipateCenter";   // nom du property Vector3 dans le VFX
    public string radiusPropertyName = "DissipateRadius";   // nom du property float dans le VFX
    public string eventName = "OnDissipate";                // nom de l’event dans le VFX

    private Camera mainCam;

    private void Awake()
    {
        if (smokeVFX == null)
            smokeVFX = GetComponent<VisualEffect>();

        mainCam = Camera.main;
    }

    private void Update()
    {
        // bouton droit de la souris
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            SendDissipateEventAtMouse();
        }
    }

    private void SendDissipateEventAtMouse()
    {
        if (smokeVFX == null || mainCam == null) return;

        // position de la souris -> monde
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        // on peut projeter sur un plan, par ex. Z = 0
        float distance;
        Plane plane = new Plane(Vector3.forward, Vector3.zero); // plan XY
        if (plane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);

            // envoyer les properties au VFX
            smokeVFX.SetVector3(centerPropertyName, worldPos);
            smokeVFX.SetFloat(radiusPropertyName, dissipateRadius);

            // envoyer l’event
            smokeVFX.SendEvent(eventName);
        }
    }
}