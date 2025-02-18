using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Clef_ButtonScript : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource backgroundMusic;
    public AudioSource soundEffects;

    [Header("UI Elements")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    // Start is called before the first frame update
    void Start()
    {
        // load saved audio settings
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        // apply saved settings
        bgmSlider.value = bgmVolume;
        sfxSlider.value = sfxVolume;
        muteToggle.isOn = isMuted;

        ApplyAudioSettings();

        // add listeners
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        muteToggle.onValueChanged.AddListener(ToggleMute);
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        ApplyAudioSettings();
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        ApplyAudioSettings();
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void ToggleMute(bool isMuted)
    {
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ApplyAudioSettings()
    {
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        backgroundMusic.volume = isMuted ? 0 : bgmVolume;
        soundEffects.volume = isMuted ? 0 : sfxVolume;
    }
}
