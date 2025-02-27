using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
using UnityEngine.SceneManagement;

public class OxygenSystem : MonoBehaviour
{
    [Header("Oxygen Settings")]
    public float maxOxygen = 100f;
    public float currentOxygen;
    public float defaultConsumptionRate = 1f;
    private float oxygenConsumptionRate;
    private bool isInSafeZone = false;
    private bool isDead = false;

    [Header("UI References")]
    public RectTransform oxygenBarFill;
    public Image oxygenBarImage;

    private float originalBarHeight;
    private bool isFlashing = false;
    private Color defaultColor;


    [Header("Post-Processing")]
    public Volume postProcessingVolume;  
    private ChromaticAberration chromaticAberration;

    // Oxygen System get player health
    private GameObject PlayerObject;
    private PlayerController playerhealth;



    private void Start()
    {
        currentOxygen = maxOxygen;
        oxygenConsumptionRate = defaultConsumptionRate;
        originalBarHeight = oxygenBarFill.sizeDelta.y;

        defaultColor = oxygenBarImage.color;

        if (postProcessingVolume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration.intensity.value = 0f; 
        }
        PlayerObject = GameObject.FindWithTag("Player");
        playerhealth = PlayerObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!isInSafeZone)
        {
            DecreaseOxygen();
        }
        else
        {
            IncreaseOxygen();
        }
        UpdateUI();
        CheckOxygenWarning();
        UpdateChromaticAberration();

        if (currentOxygen <= 0 && !isDead)
        {
            playerhealth.DepletePlayerHealth(0.03f);
        }

        if (playerhealth.GetPlayerHealth() <= 0)
        {
            Die();
        }
    }

    private void DecreaseOxygen()
    {
        currentOxygen -= oxygenConsumptionRate * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
    }

    private void IncreaseOxygen()
    {
        currentOxygen += 5f * Time.deltaTime;
        currentOxygen = Mathf.Clamp(currentOxygen, 0, maxOxygen);
    }

    private bool lowOxygenWarningPlayed = false;

    private void UpdateChromaticAberration()
    {
        if (chromaticAberration != null)
        {
            if (currentOxygen <= 20)
            {
                float intensity = Mathf.InverseLerp(20, 0, currentOxygen); 
                chromaticAberration.intensity.value = intensity;

                if (currentOxygen >= 19 && currentOxygen <= 20 && !lowOxygenWarningPlayed)
                {
                    // Play low oxygen sound
                    AudioManager audioManager = FindObjectOfType<AudioManager>();
                    if (audioManager != null)
                    {

                        audioManager.PlaySFX("LowOxygen");

                    }
                    else
                    {
                        Debug.LogWarning("LowOxygen");
                    }
                    lowOxygenWarningPlayed = true;
                }
            }

            else
            {
                chromaticAberration.intensity.value = 0f;

                // Reset the flag when oxygen regenerates above 20
                lowOxygenWarningPlayed = false;
            }
           
        }
    }

    private void UpdateUI()
    {
        float normalizedOxygen = currentOxygen / maxOxygen;
        oxygenBarFill.sizeDelta = new Vector2(oxygenBarFill.sizeDelta.x, originalBarHeight * normalizedOxygen);
    }

    private void CheckOxygenWarning()
    {
        if (currentOxygen <= 20 && !isFlashing)
        {
            StartCoroutine(FlashOxygenBar());
        }
    }

    private IEnumerator FlashOxygenBar()
    {
        isFlashing = true;
        while (currentOxygen <= 20)
        {
            oxygenBarImage.color = Color.red;
            yield return new WaitForSeconds(0.5f);
            oxygenBarImage.color = defaultColor; 
            yield return new WaitForSeconds(0.5f);
        }
        isFlashing = false;
    }

    public void SetOxygenConsumptionRate(float newRate)
    {
        oxygenConsumptionRate = newRate;
    }

    public void EnterSafeZone()
    {
        isInSafeZone = true;
    }

    public void ExitSafeZone()
    {
        isInSafeZone = false;
    }

    private void Die()
    {
        isDead = true;
        SceneManager.LoadScene("LoseScene");
    }
}
