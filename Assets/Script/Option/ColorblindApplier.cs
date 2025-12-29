using UnityEngine;

public enum ColorblindModeApply
{
    Normal = 0,
    Protanopia = 1,
    Deuteranopia = 2,
    Tritanopia = 3
}

public class ColorblindApplier : MonoBehaviour
{
    [Header("Volumes par mode dans cette scène")]
    public GameObject normalVolume;
    public GameObject protanopiaVolume;
    public GameObject deuteranopiaVolume;
    public GameObject tritanopiaVolume;

    private void Start()
    {
        int saved = PlayerPrefs.GetInt("ColorblindMode", 0);
        ApplyMode((ColorblindMode)saved);
    }

    public void ApplyMode(ColorblindMode mode)
    {
        if (normalVolume      != null) normalVolume.SetActive(mode == ColorblindMode.Normal);
        if (protanopiaVolume  != null) protanopiaVolume.SetActive(mode == ColorblindMode.Protanopia);
        if (deuteranopiaVolume!= null) deuteranopiaVolume.SetActive(mode == ColorblindMode.Deuteranopia);
        if (tritanopiaVolume  != null) tritanopiaVolume.SetActive(mode == ColorblindMode.Tritanopia);
    }
}