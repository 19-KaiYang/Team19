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
        // Debug distance more clearly
       
        // First check - is player within the detection radius?
        if (distance <= range)
        {
           
            // Check angle to player - ignore Y axis for better detection
            Vector3 directionToPlayer = playerTransform.transform.position - transform.position;
            directionToPlayer.y = 0; // IMPORTANT: Ignore height difference
            directionToPlayer.Normalize();
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
           
            if (angleToPlayer <= angle / 2f)
            {
                
                // Use more reliable raycast - ignore Y differences for better detection
                Vector3 rayOrigin = transform.position + Vector3.up * 1.0f;
                Vector3 rayDirection = playerTransform.transform.position - rayOrigin;
                float rayDistance = rayDirection.magnitude;
                rayDirection.Normalize();
                // Make ray extremely visible for debugging
                Debug.DrawRay(rayOrigin, rayDirection * range, Color.yellow, 0.5f);
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
                {
                   
                    if (hit.transform.CompareTag("Player"))
                    {
                        
                        return true;
                    }
                    else
                    {
                       
                    }
                }
                else
                {
                    // Try a direct line test as fallback
                   
                    return true; // If in range and FOV but raycast fails, still detect
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