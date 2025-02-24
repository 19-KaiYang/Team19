using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolRadius = 10f;
    public float detectionRange = 15f;
    public float fieldOfViewAngle = 120f;  // Detection angle
    public float patrolWaitTime = 2f;

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 startPosition;
    private float waitTimer;
    private bool isWaiting = false;
    private bool isChasing = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        SetNewPatrolDestination();
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (IsPlayerInFieldOfView(distanceToPlayer))
        {
            if (!isChasing)
            {
                isChasing = true;
                agent.speed *= 1.5f; // Move faster when chasing
            }
            ChasePlayer();
        }
        else
        {
            if (isChasing)
            {
                isChasing = false;
                agent.speed /= 1.5f; // Return to normal speed
            }
            Patrol();
        }
    }

    private bool IsPlayerInFieldOfView(float distanceToPlayer)
    {
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            // Check if the player is within the FOV
            if (angleToPlayer <= fieldOfViewAngle / 2f)
            {
                // Optional: Add a raycast to check for obstacles
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
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

    private void Patrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isWaiting = false;
                SetNewPatrolDestination();
            }
        }
        else if (agent.remainingDistance <= agent.stoppingDistance)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(playerTransform.position);
    }

    private void SetNewPatrolDestination()
    {
        Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
        Vector3 randomPoint = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Draw Field of View (FOV)
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
