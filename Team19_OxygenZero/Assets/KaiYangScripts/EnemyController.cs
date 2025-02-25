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

    [Header("Animation Settings")]
    public float animationDampTime = 0.1f; // Smoothing for animation transitions
    public string idleAnimationParam = "IsIdle";
    public string walkAnimationParam = "IsWalking";
    public string attackAnimationParam = "IsAttacking";
    public string attackTriggerParam = "Attack";

    [Header("References")]
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Vector3 startPosition;
    private bool isChasing = false;
    private bool isAttacking = false;
    private Animator animator;
    private Rigidbody rb;

    [Header("Movement Settings")]
    public float rotationSpeed = 120f; // Degrees per second, adjust as needed
    private bool isPatrolling = true;
    private float patrolWaitTime = 1f; // Time to wait at each patrol point
    private float patrolTimer = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        agent.updateRotation = false;

        // Get and configure the Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Make enemy immune to physics pushes
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        }

        if (animator == null)
        {
            Debug.LogError("Animator component not found on enemy!");
        }

        SetNewPatrolDestination();
    }

    private void Update()
    {
        // Calculate distance to player at the start, used throughout the method
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Check if player is in attack range/FOV
        bool playerInAttackRange = IsPlayerInFieldOfView(distanceToPlayer, attackRange, attackFieldOfView);

        // Check if player is in detection range/FOV
        bool playerInDetectionRange = IsPlayerInFieldOfView(distanceToPlayer, detectionRange, fieldOfViewAngle);

        if (isAttacking && !playerInAttackRange)
        {
            // Add a short delay before breaking out of attack mode
            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0f)
            {
                isAttacking = false;
                agent.isStopped = false;

                // Reset the attack animation state
                UpdateAnimationState(false, false, false);

                // Resume chasing or patrolling
                if (playerInDetectionRange)
                {
                    isChasing = true;
                    UpdateAnimationState(false, true, false); // Walking
                }
                else
                {
                    isChasing = false;
                    SetNewPatrolDestination();
                }
            }
        }



        // Handle attacking state
        if (isAttacking)
        {
            agent.isStopped = true; // Ensure agent remains stopped
            agent.velocity = Vector3.zero; // Reset velocity
            LookAtTarget(playerTransform.position); // Keep facing the player
            AttackPlayer(); // Execute attack logic
            return; // Skip the rest of the update while attacking
        }

        // Handle detection and chasing
        if (playerInDetectionRange)
        {
            if (playerInAttackRange)
            {
                // Player is in attack range - transition to attack mode
                isChasing = false;
                isAttacking = true;
                agent.isStopped = true;
                agent.velocity = Vector3.zero;

                // Update animation state
                UpdateAnimationState(false, false, true);

                // Look at player and attack
                LookAtTarget(playerTransform.position);
                AttackPlayer();
            }
            else
            {
                // Player detected but not in attack range - chase mode
                if (isAttacking)
                {
                    isAttacking = false;
                    agent.isStopped = false;
                }

                if (!isChasing)
                {
                    isChasing = true;
                    agent.isStopped = false;

                    // Update animation state
                    UpdateAnimationState(false, true, false);
                }

                ChasePlayer();
            }
        }
        else
        {
            // Player not detected - patrol mode
            if (isChasing || isAttacking)
            {
                isChasing = false;
                isAttacking = false;
                agent.isStopped = false;
            }

            Patrol();

            // Update animation based on agent movement
            UpdateAnimationState(agent.velocity.sqrMagnitude < 0.1f, agent.velocity.sqrMagnitude >= 0.1f, false);
        }

        // Decrement attack timer
        attackTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        // If we're stopped (like during attacking), ensure we stay still
        if (agent.isStopped)
        {
            // Reset NavMeshAgent velocity
            agent.velocity = Vector3.zero;

            // Reset physics velocities if Rigidbody exists
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void LookAtTarget(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Keep rotation only on the horizontal plane

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private bool IsPlayerInFieldOfView(float distance, float range, float angle)
    {

        if (distance <= 1.0f)
        {
            return true;
        }
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
        // Check if we've reached the destination
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!isPatrolling)
            {
                patrolTimer += Time.deltaTime;

                if (patrolTimer >= patrolWaitTime)
                {
                    isPatrolling = true;
                    SetNewPatrolDestination();
                }
            }
            else
            {
                // We're waiting at a patrol point
                isPatrolling = false;
                patrolTimer = 0f;
            }
        }
        else
        {
            // Rotate towards the next destination
            Vector3 direction = (agent.steeringTarget - transform.position).normalized;
            direction.y = 0f; // Prevent upward rotation
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(playerTransform.position);

        // Smooth rotation towards the player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f; // Keep rotation on horizontal plane
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void AttackPlayer()
    {
        // Fully stop the NavMeshAgent during the attack
        if (!agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath(); // Clear any paths
        }

        agent.velocity = Vector3.zero; // Stop movement

        if (attackTimer <= 0f)
        {
            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger(attackTriggerParam);
            }

            // Damage dealing logic
            Debug.Log("Enemy attacks player!");

            // Reset cooldown
            attackTimer = attackCooldown;
        }
        else
        {
            attackTimer -= Time.deltaTime; // Countdown until the next attack
        }

        // Keep looking at the player
        LookAtTarget(playerTransform.position);
    }


    private void SetNewPatrolDestination()
    {
        for (int i = 0; i < 5; i++) // Try 5 times to find a valid patrol point
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 randomPoint = startPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }

        // Fallback position if all attempts fail
        Vector3 fallbackPoint = startPosition + Random.insideUnitSphere * (patrolRadius * 0.5f);
        fallbackPoint.y = startPosition.y;
        agent.SetDestination(fallbackPoint);
    }

    private void UpdateAnimationState(bool idle, bool walking, bool attacking)
    {
        if (animator != null)
        {
            animator.SetBool(idleAnimationParam, idle);
            animator.SetBool(walkAnimationParam, walking);
            animator.SetBool(attackAnimationParam, attacking);
        }
    }

    // You might want to add this method and call it from an animation event
    public void OnAttackAnimationHit()
    {
        // This method can be called from an animation event when the attack animation reaches the point of impact
        // Implement damage dealing logic here
        if (Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            Debug.Log("Player takes damage from enemy attack!");
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