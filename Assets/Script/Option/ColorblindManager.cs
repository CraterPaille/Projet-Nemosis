using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public enum ColorblindMode
{
    Normal = 0,
    Protanopia = 1,
    Deuteranopia = 2,
    Tritanopia = 3
}

public class ColorblindManager : MonoBehaviour
{
    public static ColorblindManager Instance;

    [Header("Volumes par mode")]
    public GameObject normalVolume;
    public GameObject protanopiaVolume;
    public GameObject deuteranopiaVolume;
    public GameObject tritanopiaVolume;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        int savedMode = PlayerPrefs.GetInt("ColorblindMode", 0);
        SetMode((ColorblindMode)savedMode);
    }

    public void SetMode(ColorblindMode mode)
    {
        // Active/désactive les volumes
        if (normalVolume != null)      normalVolume.SetActive(mode == ColorblindMode.Normal);
        if (protanopiaVolume != null)  protanopiaVolume.SetActive(mode == ColorblindMode.Protanopia);
        if (deuteranopiaVolume != null)deuteranopiaVolume.SetActive(mode == ColorblindMode.Deuteranopia);
        if (tritanopiaVolume != null)  tritanopiaVolume.SetActive(mode == ColorblindMode.Tritanopia);

        PlayerPrefs.SetInt("ColorblindMode", (int)mode);
        Debug.Log("Colorblind mode set to: " + mode);
    }
}
