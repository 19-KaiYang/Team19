using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
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

    // Singleton instance
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize audio dictionaries
        LoadAudioDictionaries();

        // Setup initial audio references
        SetupAudioReferences();

        // Register scene load event to reconnect audio sources after scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Unregister event when destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // This gets called when a new scene is loaded
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}. Setting up audio references...");
        StartCoroutine(DelayedSetupAfterSceneLoad());
    }

    // Allow scene to fully load before looking for audio objects
    IEnumerator DelayedSetupAfterSceneLoad()
    {
        yield return new WaitForSeconds(0.2f);
        SetupAudioReferences();
        SetInitialVolumes();
    }

    // Find and setup audio references - used both at start and after scene loads
    private void SetupAudioReferences()
    {
        // Auto-find and assign AudioSource for BGM if null
        if (backgroundMusic == null)
        {
            GameObject bgmObj = GameObject.FindWithTag("BGM");
            if (bgmObj != null)
            {
                backgroundMusic = bgmObj.GetComponent<AudioSource>();
                // Don't destroy the BGM object as its AudioSource is needed
                DontDestroyOnLoad(bgmObj);
                Debug.Log("Found and assigned Background Music AudioSource.");
            }
            else
            {
                Debug.LogWarning("⚠️ No BGM object found in the scene.");
            }
        }

        // Auto-find and assign AudioSource for SFX if null
        if (soundEffects == null)
        {
            GameObject sfxObj = GameObject.FindWithTag("SFX");
            if (sfxObj != null)
            {
                soundEffects = sfxObj.GetComponent<AudioSource>();
                // Don't destroy the SFX object as its AudioSource is needed
                DontDestroyOnLoad(sfxObj);
                Debug.Log("Found and assigned Sound Effects AudioSource.");
            }
            else
            {
                Debug.LogWarning("⚠️ No SFX object found in the scene.");
            }
        }

        // Also look for and set up UI references in the current scene
        SetupUIReferences();
    }

    // Find and setup UI references in the current scene
    private void SetupUIReferences()
    {
        // Find UI elements if they're not assigned
        if (bgmSlider == null)
        {
            // Find all sliders and manually check their names
            Slider[] allSliders = GameObject.FindObjectsOfType<Slider>();
            foreach (Slider slider in allSliders)
            {
                if (slider.gameObject.name.Contains("BGM"))
                {
                    bgmSlider = slider;
                    break;
                }
            }
        }

        if (sfxSlider == null)
        {
            // Find all sliders and manually check their names
            Slider[] allSliders = GameObject.FindObjectsOfType<Slider>();
            foreach (Slider slider in allSliders)
            {
                if (slider.gameObject.name.Contains("SFX"))
                {
                    sfxSlider = slider;
                    break;
                }
            }
        }

        if (muteToggle == null)
        {
            // Find all toggles and manually check their names
            Toggle[] allToggles = GameObject.FindObjectsOfType<Toggle>();
            foreach (Toggle toggle in allToggles)
            {
                if (toggle.gameObject.name.Contains("Mute"))
                {
                    muteToggle = toggle;
                    break;
                }
            }
        }

        // Set up UI listeners
        if (bgmSlider != null)
        {
            bgmSlider.onValueChanged.RemoveAllListeners(); // Clear existing listeners first
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners(); // Clear existing listeners first
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (muteToggle != null)
        {
            muteToggle.onValueChanged.RemoveAllListeners(); // Clear existing listeners first
            muteToggle.onValueChanged.AddListener(ToggleMute);
        }
    }

    void Start()
    {
        StartCoroutine(DelayedInitialization()); // Initialize volume settings
    }

    IEnumerator DelayedInitialization()
    {
        yield return new WaitForSeconds(0.1f); // Ensure scene loads fully
        SetInitialVolumes();
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
            SetBGMVolume(bgmSlider.value); // Trigger listener at start
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
            SetSFXVolume(sfxSlider.value); // Trigger listener at start
        }

        if (muteToggle != null)
        {
            muteToggle.isOn = isMuted;
            ToggleMute(muteToggle.isOn); // Trigger listener at start
        }

        // Initialize and play last saved BGM
        string lastPlayingBGM = PlayerPrefs.GetString("LastPlayingBGM", "BattleBGM");
        PlayBGM(lastPlayingBGM);

        Debug.Log($"Applied Volumes -> BGM: {bgmVolume} (dB: {Mathf.Log10(bgmVolume) * 20f}), SFX: {sfxVolume} (dB: {Mathf.Log10(sfxVolume) * 20f}), Muted: {isMuted}");
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

    public void PlayBGM(string name)
    {
        if (bgmDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (backgroundMusic == null)
            {
                Debug.LogError("Cannot play BGM: backgroundMusic is null");
                return;
            }

            if (backgroundMusic.clip == clip && backgroundMusic.isPlaying)
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
        if (soundEffects == null)
        {
            Debug.LogError("soundEffects AudioSource is NULL! Cannot play SFX.");
            return;
        }

        if (sfxDictionary.TryGetValue(name, out AudioClip clip))
        {
            if (clip == null)
            {
                Debug.LogError($"SFX '{name}' is NULL!");
                return;
            }

            Debug.Log($"Playing SFX: {name}");
            soundEffects.PlayOneShot(clip);
        }
        else
        {
            Debug.LogError($"SFX '{name}' not found in dictionary.");
        }
    }
}