using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CanvasCameraAssigner : MonoBehaviour
{
    private Canvas canvas;
    
    // Sauvegarde du mode initial pour le restaurer si nécessaire
    private RenderMode initialRenderMode;
    private bool wasScreenSpaceCamera = false;
    
    void Awake()
    {
        Debug.Log($"[CanvasCameraAssigner] Awake sur '{gameObject.name}'");
        canvas = GetComponent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError($"[CanvasCameraAssigner] Pas de Canvas sur '{gameObject.name}' !");
            enabled = false;
            return;
        }
        
        // Sauvegarde le mode initial
        initialRenderMode = canvas.renderMode;
        wasScreenSpaceCamera = (initialRenderMode == RenderMode.ScreenSpaceCamera);
        
        Debug.Log($"[CanvasCameraAssigner] Canvas '{gameObject.name}' - RenderMode initial: {initialRenderMode}");
        
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnEnable()
    {
        if (canvas.enabled && canvas.gameObject.activeInHierarchy)
            StartCoroutine(AssignCameraCoroutine());
    }
    void Start()
    {
        // Ne lance la coroutine que si le Canvas est actif dans la scène
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[CanvasCameraAssigner] Canvas '{gameObject.name}' inactif au Start, AssignCameraCoroutine non démarrée.");
            return;
        }

        StartCoroutine(AssignCameraCoroutine());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[CanvasCameraAssigner] Scène '{scene.name}' chargée - Réassignation pour '{gameObject.name}'");

        if (wasScreenSpaceCamera && canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.Log($"[CanvasCameraAssigner] Restauration du mode Camera pour '{gameObject.name}' (était passé en {canvas.renderMode})");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }

        // Ne pas lancer de coroutine si ce Canvas est inactif
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[CanvasCameraAssigner] Canvas '{gameObject.name}' inactif, AssignCameraCoroutine non démarrée.");
            return;
        }

        StartCoroutine(AssignCameraCoroutine());
    }

    private IEnumerator AssignCameraCoroutine()
    {
        // Attend que tout soit initialisé
        yield return null;
        yield return null;
        
        if (canvas == null)
        {
            Debug.LogError($"[CanvasCameraAssigner] Canvas n'existe plus !");
            yield break;
        }
        
        // Si le Canvas n'était pas en mode Camera au départ, on sort
        if (!wasScreenSpaceCamera)
        {
            Debug.Log($"<color=cyan>[CanvasCameraAssigner] Canvas '{gameObject.name}' n'est pas configuré en Screen Space - Camera (mode: {canvas.renderMode}), script ignoré.</color>");
            yield break;
        }
        
        // Force le mode Camera si Unity l'a changé
        if (canvas.renderMode != RenderMode.ScreenSpaceCamera)
        {
            Debug.Log($"[CanvasCameraAssigner] Restauration du mode Camera pour '{gameObject.name}'");
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
        }
        
        int attempts = 0;
        int maxAttempts = 15;
        
        while (attempts < maxAttempts)
        {
            attempts++;
            
            // Cherche la caméra
            Camera cam = FindCamera();
            
            if (cam != null)
            {
                canvas.worldCamera = cam;
                Debug.Log($"<color=green>✓ [CanvasCameraAssigner] Caméra '{cam.name}' assignée à '{gameObject.name}' (tentative {attempts})</color>");
                yield break;
            }
            
            if (attempts == 1 || attempts % 5 == 0)
            {
                Debug.LogWarning($"[CanvasCameraAssigner] Caméra non trouvée pour '{gameObject.name}' (tentative {attempts}/{maxAttempts})");
            }
            
            yield return new WaitForSeconds(0.15f);
        }
        
        Debug.LogError($"<color=red>✗ [CanvasCameraAssigner] ÉCHEC pour '{gameObject.name}' après {maxAttempts} tentatives - Vérifiez que la caméra a le tag 'MainCamera'</color>");
    }
    
    private Camera FindCamera()
    {
        // 1. Camera.main
        Camera cam = Camera.main;
        if (cam != null && cam.gameObject.activeInHierarchy)
        {
            return cam;
        }
        
        // 2. Par tag MainCamera
        GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (cameraObj != null && cameraObj.activeInHierarchy)
        {
            cam = cameraObj.GetComponent<Camera>();
            if (cam != null)
            {
                return cam;
            }
        }

        // 3. N'importe quelle caméra active
        Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (Camera c in allCameras)
        {
            if (c.gameObject.activeInHierarchy && c.enabled)
            {
                return c;
            }
        }
        
        return null;
    }
    
    // Pour tester manuellement
    [ContextMenu("Forcer l'assignation")]
    public void ForceReassign()
    {
        StopAllCoroutines();
        StartCoroutine(AssignCameraCoroutine());
    }
    
    // Pour vérifier l'état actuel
    [ContextMenu("Afficher l'état")]
    public void ShowStatus()
    {
        if (canvas != null)
        {
            Debug.Log($"Canvas '{gameObject.name}' - Mode: {canvas.renderMode} - Caméra: {(canvas.worldCamera != null ? canvas.worldCamera.name : "NULL")}");
        }
    }
}