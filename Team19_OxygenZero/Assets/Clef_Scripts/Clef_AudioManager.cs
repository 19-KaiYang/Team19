using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class Clef_AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource backgroundMusic;
    public AudioSource soundEffects;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    public string bgmMixerParam = "BGMVolume"; // The parameter name in your audio mixer
    public string sfxMixerParam = "SFXVolume";

    [Header("UI Elements")]
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle muteToggle;

    [Header("Audio Clips")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    private float bgmVolume = 1f;
    private float sfxVolume = 1f;

    [Header("Dictionaries")]
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        // Initialize dictionary
        LoadAudioDictionaries();
    }

    private void SetInitialVolumes()
    {
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        // Load saved volume settings
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Set AudioSource volumes
        if (backgroundMusic != null)
            backgroundMusic.volume = isMuted ? 0f : bgmVolume;
        if (soundEffects != null)
            soundEffects.volume = isMuted ? 0f : sfxVolume;

        // Set AudioMixer volumes
        if (audioMixer != null)
        {
            float bgmMixerVolume = isMuted ? -80f : Mathf.Log10(bgmVolume) * 20f;
            float sfxMixerVolume = isMuted ? -80f : Mathf.Log10(sfxVolume) * 20f;

            audioMixer.SetFloat(bgmMixerParam, bgmMixerVolume);
            audioMixer.SetFloat(sfxMixerParam, sfxMixerVolume);
        }

        // Set UI elements and trigger listeners immediately
        if (bgmSlider != null)
        {
            bgmSlider.value = bgmVolume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            SetBGMVolume(bgmSlider.value); // 🔥 Trigger listener at start
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            SetSFXVolume(sfxSlider.value); // 🔥 Trigger listener at start
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
            muteToggle.onValueChanged.AddListener(ToggleMute);
            ToggleMute(muteToggle.isOn); // 🔥 Trigger listener at start
        }

        // Initialize and play last saved BGM
        string lastPlayingBGM = PlayerPrefs.GetString("LastPlayingBGM", "BattleBGM");
        PlayBGM(lastPlayingBGM);

        Debug.Log($"Applied Volumes -> BGM: {bgmVolume} (dB: {Mathf.Log10(bgmVolume) * 20f}), SFX: {sfxVolume} (dB: {Mathf.Log10(sfxVolume) * 20f}), Muted: {isMuted}");
    }





    void Start()
    {
        // Initialize both AudioSource and AudioMixer volumes
        SetInitialVolumes();
    }

    private void LoadAudioDictionaries()
    {
        // Load BGM into dictionary
        foreach (AudioClip clip in bgmClips)
        {
            if (clip != null && !bgmDictionary.ContainsKey(clip.name))
            {
                bgmDictionary.Add(clip.name, clip);
            }
        }

        // Load SFX into dictionary
        foreach (AudioClip clip in sfxClips)
        {
            if (clip != null && !sfxDictionary.ContainsKey(clip.name))
            {
                sfxDictionary.Add(clip.name, clip);
            }
        }
    }


    public void SetBGMVolume(float volume)
    {
        bgmVolume = volume;
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        if (!isMuted && backgroundMusic != null)
        {
            backgroundMusic.volume = volume;
            if (audioMixer != null)
            {
                float mixerVolume = Mathf.Log10(volume) * 20f;
                audioMixer.SetFloat(bgmMixerParam, mixerVolume);
            }
        }

        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        if (!isMuted && soundEffects != null)
        {
            soundEffects.volume = volume;
            if (audioMixer != null)
            {
                float mixerVolume = Mathf.Log10(volume) * 20f;
                audioMixer.SetFloat(sfxMixerParam, mixerVolume);
            }
        }

        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void ToggleMute(bool isMuted)
    {
        if (audioMixer != null)
        {
            float bgmMixerVolume = isMuted ? -80f : Mathf.Log10(bgmVolume) * 20f;
            float sfxMixerVolume = isMuted ? -80f : Mathf.Log10(sfxVolume) * 20f;

            audioMixer.SetFloat(bgmMixerParam, bgmMixerVolume);
            audioMixer.SetFloat(sfxMixerParam, sfxMixerVolume);
        }

        if (backgroundMusic != null)
            backgroundMusic.volume = isMuted ? 0f : bgmVolume;
        if (soundEffects != null)
            soundEffects.volume = isMuted ? 0f : sfxVolume;

        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void ApplyAudioSettings()
    {
        bool isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        float bgmVolumeDB = isMuted ? -80f : Mathf.Log10(bgmVolume) * 20;
        float sfxVolumeDB = isMuted ? -80f : Mathf.Log10(sfxVolume) * 20;

        backgroundMusic.volume = isMuted ? 0 : bgmVolume;
        soundEffects.volume = isMuted ? 0 : sfxVolume;
    }

    public void PlayBGM(string name)
    {
        if (bgmDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (backgroundMusic.clip == clip)
            {
                Debug.Log($"BGM already playing: {name}");
                return;
            }

            Debug.Log($"Playing BGM: {name}");
            backgroundMusic.clip = clip;
            backgroundMusic.loop = true;
            backgroundMusic.Play();

            // Save the currently playing BGM
            PlayerPrefs.SetString("LastPlayingBGM", name);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.LogError($"BGM not found: {name}");
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            Debug.Log($"Playing SFX: {name}");
            soundEffects.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"SFX not found: {name}");
        }
    }
}