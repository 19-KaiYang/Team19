using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Clef_AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource backgroundMusic;
    public AudioSource soundEffects;

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

    // Start is called before the first frame update
    void Start()
    {
        // initialize dictionary
        LoadAudioDictionaries();

        // Play background music if not already playing
        if (!backgroundMusic.isPlaying)
        {
            PlayBGM("BattleBGM"); // Change this to load the correct saved BGM
        }


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