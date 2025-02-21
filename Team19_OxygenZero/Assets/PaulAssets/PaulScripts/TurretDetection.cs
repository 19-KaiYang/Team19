
using UnityEngine;

public class TurretDetection : MonoBehaviour
{
    [SerializeField] private TurretAI _turretAI;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _turretAI.SetTarget(other.transform); // Notify the turret AI
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _turretAI.ClearTarget(); // Notify the turret AI that the player left
        }
    }
}
