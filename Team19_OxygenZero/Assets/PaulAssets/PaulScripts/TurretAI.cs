using UnityEngine;

public class TurretAI : MonoBehaviour
{
    private float turretHealth;

    public enum TurretState { Idle, Attack }
    public TurretState currentState = TurretState.Idle;

    public Transform turretHead;
    public GameObject target;

    public float rotationSpeed = 5f;
    public float fireRate = 1f;
    public GameObject bulletPrefab;
    public Transform firePoint;

    public float fireCooldown = 1f;
    private float idleRotationTimer;
    private Quaternion idleTargetRotation;

    public Collider detectionCollider;

    private void Start()
    {
        PickNewIdleRotation();
        target = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        if (currentState == TurretState.Idle)
        {
            IdleState();
        }
        else if (currentState == TurretState.Attack && target != null)
        {
            RotateTowardsTarget();

            if (fireCooldown <= 0f)
            {
                Fire();
                // Play turretshot explosion sound
                AudioManager audioManager = FindObjectOfType<AudioManager>();
                if (audioManager != null)
                {
                    audioManager.PlaySFX("TurretShot");

                }
                else
                {
                    Debug.LogWarning("TurretShot");
                }
                fireCooldown = fireRate;
                Debug.Log("bullet fired");
            }
            fireCooldown -= 0.7f * Time.deltaTime;
        }
    }

    private void IdleState()
    {
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, idleTargetRotation, Time.deltaTime * rotationSpeed);

        idleRotationTimer -= Time.deltaTime;
        if (idleRotationTimer <= 0)
        {
            PickNewIdleRotation();
        }
    }

    private void PickNewIdleRotation()
    {
        idleRotationTimer = Random.Range(2f, 5f);
        float randomYRotation = Random.Range(0f, 360f);
        idleTargetRotation = Quaternion.Euler(0f, randomYRotation, 0f);
    }

    void RotateTowardsTarget()
    {
        Vector3 direction = (target.transform.position - turretHead.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void Fire()
    {
        firePoint.rotation = turretHead.rotation;  // Ensure bullets fire in the correct direction
        GameObject newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        TurretBullet bulletScript = newBullet.GetComponent<TurretBullet>();

        if (bulletScript != null)
        {
            bulletScript.Initialize(firePoint.forward);
        }
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
        currentState = TurretState.Attack;

         // Play turret sensor sound
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        if (audioManager != null)
        {
            audioManager.PlaySFX("TurretSensor");

        }
        else
        {
            Debug.LogWarning("TurretSensor not found");
        }
    }


    public GameObject GetTarget()
    {
        return target;
    }

    public void ClearTarget()
    {
        target = null;
        currentState = TurretState.Idle;
    }
}
