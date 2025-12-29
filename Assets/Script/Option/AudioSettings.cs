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
        // Charger les valeurs sauvegardées
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        voiceSlider.value = PlayerPrefs.GetFloat("VoiceVolume", 1f);

        // Appliquer immédiatement les valeurs au mixer
        SetMasterVolume(masterSlider.value);
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetVoiceVolume(voiceSlider.value);

        // Ajouter des listeners aux sliders
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        voiceSlider.onValueChanged.AddListener(SetVoiceVolume);
    }

    // Méthodes pour contrôler chaque volume
    public void SetMasterVolume(float value)
    {
        masterMixer.SetFloat("MasterVolume", LinearToDB(value));
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        masterMixer.SetFloat("MusicVolume", LinearToDB(value));
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        masterMixer.SetFloat("SFXVolume", LinearToDB(value));
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void SetVoiceVolume(float value)
    {
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
    }
}
