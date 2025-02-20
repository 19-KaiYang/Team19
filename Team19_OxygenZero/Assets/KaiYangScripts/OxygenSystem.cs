using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    private void Start()
    {
        currentOxygen = maxOxygen;
        oxygenConsumptionRate = defaultConsumptionRate;
        originalBarHeight = oxygenBarFill.sizeDelta.y;

        defaultColor = oxygenBarImage.color; 
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

        if (currentOxygen <= 0 && !isDead)
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
        //wynn or someone add the relevant codes here
    }
}
