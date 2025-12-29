using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LocaleDropdownManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown languageDropdown;

    [Header("Settings")]
    public string playerPrefsKey = "SelectedLocale";

    private void Start()
    {
        if (languageDropdown == null)
        {
            Debug.LogError("Dropdown not assigned!");
            return;
        }

        // Remplir le Dropdown avec les codes de langue disponibles
        languageDropdown.ClearOptions();
        List<string> options = new List<string>();
        var locales = LocalizationSettings.AvailableLocales.Locales;
        foreach (var locale in locales)
        {
            options.Add(locale.Identifier.Code); // ex: "en", "fr"
        }
        languageDropdown.AddOptions(options);

        // Restaurer la locale sauvegardée si elle existe
        int savedIndex = PlayerPrefs.GetInt(playerPrefsKey, 0);
        savedIndex = Mathf.Clamp(savedIndex, 0, locales.Count - 1);
        languageDropdown.value = savedIndex;

        StartCoroutine(SetLocale(savedIndex, false));

        // Ajouter le listener
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    private void OnDestroy()
    {
        if (languageDropdown != null)
            languageDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
    }

    private void OnDropdownValueChanged(int index)
    {
        StartCoroutine(SetLocale(index, true));
    }

    private IEnumerator SetLocale(int index, bool savePreference)
    {
        // Attendre l'initialisation du système de localisation
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;
        if (index < 0 || index >= locales.Count)
            yield break;

        LocalizationSettings.SelectedLocale = locales[index];

        if (savePreference)
            PlayerPrefs.SetInt(playerPrefsKey, index);
    }
}
