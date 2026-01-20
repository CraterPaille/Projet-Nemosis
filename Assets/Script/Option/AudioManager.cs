using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Mixer & paramètres exposés")]
    public AudioMixer masterMixer;
    public string masterParam = "MasterVolume";
    public string musicParam = "MusicVolume";
    public string sfxParam = "SFXVolume";
    public string voiceParam = "VoiceVolume";

    [Header("Debug")]
    [SerializeField] private bool enableDebugLogs = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Appliquer les valeurs sauvegardées immédiatement
        ApplySavedVolumes();

        // Ré-applications retardées pour écraser d'éventuels overrides d'initialisation
        StartCoroutine(ReapplyNextFrame());

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // applique puis réapplique la frame suivante (protège contre overrides post-load)
        ApplySavedVolumes();
        StartCoroutine(ReapplyNextFrame());
    }

    private IEnumerator ReapplyNextFrame()
    {
        // attendre la fin du frame courant et du suivant pour être sûr
        yield return null;
        yield return null;
        ApplySavedVolumes();
    }

    private void ApplySavedVolumes()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f), save: false);
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f), save: false);
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f), save: false);
        SetVoiceVolume(PlayerPrefs.GetFloat("VoiceVolume", 1f), save: false);

        if (enableDebugLogs)
        {
            LogMixerParam(masterParam);
            LogMixerParam(musicParam);
            LogMixerParam(sfxParam);
            LogMixerParam(voiceParam);
        }
    }

    private void LogMixerParam(string param)
    {
        if (masterMixer == null)
        {
            if (enableDebugLogs) Debug.LogWarning("AudioManager : masterMixer non assigné.");
            return;
        }

        if (masterMixer.GetFloat(param, out float val))
            Debug.Log($"AudioManager : param '{param}' = {val} dB");
        else
            Debug.LogWarning($"AudioManager : param exposé '{param}' introuvable dans le AudioMixer.");
    }

    public void SetMasterVolume(float linear, bool save = true)
    {
        if (masterMixer != null)
            masterMixer.SetFloat(masterParam, LinearToDB(linear));
        if (save) PlayerPrefs.SetFloat("MasterVolume", linear);
        if (enableDebugLogs) Debug.Log($"AudioManager: SetMasterVolume({linear})");
    }

    public void SetMusicVolume(float linear, bool save = true)
    {
        float db = LinearToDB(linear);
        if (masterMixer != null)
            masterMixer.SetFloat(musicParam, db);
        if (save) PlayerPrefs.SetFloat("MusicVolume", linear);
        if (enableDebugLogs) Debug.Log($"AudioManager: SetMusicVolume({linear}) -> {db} dB");
    }

    public void SetSFXVolume(float linear, bool save = true)
    {
        if (masterMixer != null)
            masterMixer.SetFloat(sfxParam, LinearToDB(linear));
        if (save) PlayerPrefs.SetFloat("SFXVolume", linear);
        if (enableDebugLogs) Debug.Log($"AudioManager: SetSFXVolume({linear})");
    }

    public void SetVoiceVolume(float linear, bool save = true)
    {
        if (masterMixer != null)
            masterMixer.SetFloat(voiceParam, LinearToDB(linear));
        if (save) PlayerPrefs.SetFloat("VoiceVolume", linear);
        if (enableDebugLogs) Debug.Log($"AudioManager: SetVoiceVolume({linear})");
    }

    private float LinearToDB(float linear)
    {
        if (linear <= 0.0001f) return -80f;
        return Mathf.Log10(linear) * 20f;
    }

    public void Save()
    {
        PlayerPrefs.Save();
    }
}