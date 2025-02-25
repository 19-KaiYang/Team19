
using UnityEngine;

public class TurretDetection : MonoBehaviour
{
    [SerializeField] private TurretAI _turretAI;

    [SerializeField] private float detectionRange;
    [SerializeField] private float fieldOfViewAngle;
    [SerializeField] private GameObject playerTransform;


    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.transform.position);

        // Turret
        if (IsPlayerInFieldOfView(distanceToPlayer, detectionRange, fieldOfViewAngle))
        {
            _turretAI.SetTarget(playerTransform); // Notify the turret AI
        }
        else
        {
            _turretAI.ClearTarget(); // Notify the turret AI that the player left
        }
    }

    private bool IsPlayerInFieldOfView(float distance, float range, float angle)
    {
        if (distance <= range)
        {
            Vector3 directionToPlayer = (playerTransform.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= angle / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, range))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw outer FOV
        Gizmos.color = Color.blue;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

    }
}
