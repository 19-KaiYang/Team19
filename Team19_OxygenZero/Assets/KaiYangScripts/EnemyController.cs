using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Patrol & Detection Settings")]
    public float patrolRadius = 10f;
    public float detectionRange = 15f;
    public float fieldOfViewAngle = 120f;

    [Header("Attack Settings")]
    public float attackRange = 5f;
    public float attackFieldOfView = 45f;  // Smaller FOV for attack detection
    public float attackCooldown = 2f;
    private float attackTimer = 0f;

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 startPosition;
    private bool isChasing = false;
    private bool isAttacking = false;

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

        if (IsPlayerInFieldOfView(distanceToPlayer, detectionRange, fieldOfViewAngle))
        {
            if (IsPlayerInFieldOfView(distanceToPlayer, attackRange, attackFieldOfView))
            {
                // Attack Mode
                if (!isAttacking)
                {
                    isAttacking = true;
                    isChasing = false;
                    agent.isStopped = true; // Stop moving
                }

                AttackPlayer();
            }
            else
            {
                // Chase Mode
                if (!isChasing)
                {
                    isChasing = true;
                    isAttacking = false;
                    agent.isStopped = false;
                }

                ChasePlayer();
            }
        }
        else
        {
            // Patrol Mode
            if (isChasing || isAttacking)
            {
                isChasing = false;
                isAttacking = false;
                agent.isStopped = false;
            }

            Patrol();
        }

        attackTimer -= Time.deltaTime;
    }

    private bool IsPlayerInFieldOfView(float distance, float range, float angle)
    {
        if (distance <= range)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
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

    private void Patrol()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewPatrolDestination();
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(playerTransform.position);
    }

    private void AttackPlayer()
    {
        if (attackTimer <= 0f)
        {
            

            



            attackTimer = attackCooldown;
        }
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

        // Draw attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw outer FOV
        Gizmos.color = Color.blue;
        Vector3 forward = transform.forward * detectionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfViewAngle / 2f, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfViewAngle / 2f, 0) * forward;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Draw inner FOV (attack range)
        Gizmos.color = Color.green;
        Vector3 attackForward = transform.forward * attackRange;
        Vector3 attackLeft = Quaternion.Euler(0, -attackFieldOfView / 2f, 0) * attackForward;
        Vector3 attackRight = Quaternion.Euler(0, attackFieldOfView / 2f, 0) * attackForward;

        Gizmos.DrawLine(transform.position, transform.position + attackLeft);
        Gizmos.DrawLine(transform.position, transform.position + attackRight);
    }
}
