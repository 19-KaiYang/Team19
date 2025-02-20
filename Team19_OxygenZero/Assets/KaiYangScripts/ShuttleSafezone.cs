using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuttleSafezone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OxygenSystem playerOxygen = other.GetComponent<OxygenSystem>();
            if (playerOxygen != null)
            {
                playerOxygen.EnterSafeZone(); 
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OxygenSystem playerOxygen = other.GetComponent<OxygenSystem>();
            if (playerOxygen != null)
            {
                playerOxygen.ExitSafeZone(); 
            }
        }
    }
}
