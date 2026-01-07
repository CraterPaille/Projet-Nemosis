using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public PlayerControls keyboardControls;
    public PlayerControls gamepadControls;

    // Permet de créer automatiquement l'InputManager s'il n'existe pas
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("InputManager");
            go.AddComponent<InputManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        keyboardControls = new PlayerControls();
        gamepadControls = new PlayerControls();

        // Active tous les action maps nécessaires
        keyboardControls.Rhytm.Enable();
        keyboardControls.Gameplay.Enable();
        gamepadControls.Rhytm.Enable();
        gamepadControls.Gameplay.Enable();

        // Charger rebinding sauvegardé
        if (PlayerPrefs.HasKey("rebinds_keyboard"))
            keyboardControls.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds_keyboard"));
        if (PlayerPrefs.HasKey("rebinds_gamepad"))
            gamepadControls.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds_gamepad"));

        Debug.Log("[InputManager] Initialisé automatiquement.");
    }

    private void OnEnable()
    {
        keyboardControls?.Rhytm.Enable();
        keyboardControls?.Gameplay.Enable();
        gamepadControls?.Rhytm.Enable();
        gamepadControls?.Gameplay.Enable();
    }

    private void OnDisable()
    {
        keyboardControls?.Rhytm.Disable();
        keyboardControls?.Gameplay.Disable();
        gamepadControls?.Rhytm.Disable();
        gamepadControls?.Gameplay.Disable();
    }

    public void SaveRebinds(bool forGamepad = false)
    {
        if (!forGamepad)
        {
            PlayerPrefs.SetString("rebinds_keyboard", keyboardControls.asset.SaveBindingOverridesAsJson());
        }
        else
        {
            PlayerPrefs.SetString("rebinds_gamepad", gamepadControls.asset.SaveBindingOverridesAsJson());
        }
    }
}
