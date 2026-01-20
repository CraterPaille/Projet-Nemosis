using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer masterMixer;

    [Header("Sliders")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Slider voiceSlider;

    private void Start()
    {
        // Charger les valeurs sauvegardées pour initialiser les sliders
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        voiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 1f);

        // Appliquer immédiatement les valeurs au mixer via AudioManager si dispo, sinon via masterMixer
        ApplyVolumeToSystem(masterSlider.value, musicSlider.value, sfxSlider.value, voiceSlider.value);

        // Ajouter des listeners aux sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
    }

    private void ApplyVolumeToSystem(float master, float music, float sfx, float voice)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(master, save: false);
            AudioManager.Instance.SetMusicVolume(music, save: false);
            AudioManager.Instance.SetSFXVolume(sfx, save: false);
            AudioManager.Instance.SetVoiceVolume(voice, save: false);
        }
        else if (masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", LinearToDB(master));
            masterMixer.SetFloat("MusicVolume", LinearToDB(music));
            masterMixer.SetFloat("SFXVolume", LinearToDB(sfx));
            masterMixer.SetFloat("VoiceVolume", LinearToDB(voice));
        }
        else
        {
            Debug.LogWarning("AudioSettings : pas de AudioManager ni masterMixer assigné.");
        }
    }

    // Méthodes pour contrôler chaque volume
    public void SetMasterVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);
        else if (masterMixer != null)
            masterMixer.SetFloat("MasterVolume", LinearToDB(value));

        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        else if (masterMixer != null)
            masterMixer.SetFloat("MusicVolume", LinearToDB(value));

        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        else if (masterMixer != null)
            masterMixer.SetFloat("SFXVolume", LinearToDB(value));

        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetVoiceVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVoiceVolume(value);
        else if (masterMixer != null)
            masterMixer.SetFloat("VoiceVolume", LinearToDB(value));

        PlayerPrefs.SetFloat("VoiceVolume", value);
    }

    // Conversion du slider 0-1 en dB pour AudioMixer
    private float LinearToDB(float linear)
    {
        if (linear <= 0.0001f) return -80f; // silence complet
        return Mathf.Log10(linear) * 20f;
    }

    // Sauvegarder toutes les valeurs
    public void ApplySettings()
    {
        PlayerPrefs.Save();
        if (AudioManager.Instance != null)
            AudioManager.Instance.Save();
    }
}
