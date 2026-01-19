using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public PlayerControls keyboardControls;
    public PlayerControls gamepadControls;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("InputManager");
            go.AddComponent<InputManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        keyboardControls = new PlayerControls();
        gamepadControls = new PlayerControls();

        keyboardControls.Rhytm.Enable();
        gamepadControls.Rhytm.Enable();

        // Charger rebinding sauvegardé
        if (PlayerPrefs.HasKey("rebinds_keyboard"))
            keyboardControls.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds_keyboard"));
        if (PlayerPrefs.HasKey("rebinds_gamepad"))
            gamepadControls.asset.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds_gamepad"));
    }

    private void OnEnable()
    {
        keyboardControls?.Rhytm.Enable();
        gamepadControls?.Rhytm.Enable();
    }

    private void OnDisable()
    {
        keyboardControls?.Rhytm.Disable();
        gamepadControls?.Rhytm.Disable();
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
