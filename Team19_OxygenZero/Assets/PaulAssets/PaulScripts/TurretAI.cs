
using UnityEngine;

public class TurretAI : MonoBehaviour
{
    // Turret health
    private float turretHealth;

    public enum TurretState { Idle, Attack }
    public TurretState currentState = TurretState.Idle;

    // The turret head that should rotate
    public Transform turretHead; 
    // The target you want to hit aka the player
    public Transform target;
    
    // rotation speed of the turret head
    public float rotationSpeed = 5f;
    // fire rate of the turret in attack state
    public float fireRate = 1f;
    // the bullet prefab that the turret will shoot out 
    public GameObject bulletPrefab;
    // the point where the bullet would instantiate from
    public Transform firePoint;

    private float fireCooldown;

    private float idleRotationTimer;
    private Quaternion idleTargetRotation;

    public Collider detectionCollider;

    private void Start()
    {
        PickNewIdleRotation();
    }

    void Update()
    {
        switch (currentState)
        {
            case TurretState.Idle:
                IdleState();
                break;
            case TurretState.Attack:
                AttackState();
                break;
        }

        if (currentState == TurretState.Attack && target != null)
        {
            RotateTowardsTarget();

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = fireRate;
            }
            fireCooldown -= Time.deltaTime;
        }
    }

    private void IdleState()
    {
        // Smoothly rotate to the new random rotation
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, idleTargetRotation, Time.deltaTime * rotationSpeed);

        // Change direction after some time
        idleRotationTimer -= Time.deltaTime;
        if (idleRotationTimer <= 0)
        {
            PickNewIdleRotation();
        }
    }

    private void AttackState()
    {
        if (currentState == TurretState.Attack && target != null)
        {
            RotateTowardsTarget();

            if (fireCooldown <= 0f)
            {
                Fire();
                fireCooldown = fireRate;
            }
            fireCooldown -= Time.deltaTime;
        }
    }

    private void PickNewIdleRotation()
    {
        idleRotationTimer = Random.Range(2f, 5f); // Change direction every 2-5 seconds
        float randomYRotation = Random.Range(0f, 360f); // Random Y-axis rotation
        idleTargetRotation = Quaternion.Euler(0f, randomYRotation, 0f);
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void Fire()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        TurretBullet bulletScript = bulletPrefab.GetComponent<TurretBullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.forward); // Fire in the turret head's direction
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        currentState = TurretState.Attack;
    }

    public void ClearTarget()
    {
        target = null;
        currentState = TurretState.Idle;
        PickNewIdleRotation();
    }
}
