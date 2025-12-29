
using UnityEngine;
using UnityEngine.VFX;

public class MouseToVFX : MonoBehaviour
{
    public VisualEffect vfxRenderer;
    public Camera mainCamera;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        SendMousePositionToVFX();
    }

    void SendMousePositionToVFX()
    {
        if (vfxRenderer == null || mainCamera == null) return;

        Vector3 mouseScreenPos = Input.mousePosition;

        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        vfxRenderer.SetVector3("MousePos", mouseWorldPos);
    }
}
