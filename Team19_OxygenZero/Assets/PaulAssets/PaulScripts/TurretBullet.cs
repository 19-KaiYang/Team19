using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized; // Normalize direction to ensure consistent speed
        Destroy(gameObject, lifetime); // Destroy bullet after a set time
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Apply damage logic here (e.g., call a health script on the player)
            Debug.Log("Player hit!");

            Destroy(gameObject); // Destroy bullet on impact
        }
        else if (!other.CompareTag("Turret")) // Prevent bullets from colliding with the turret itself
        {
            Destroy(gameObject); // Destroy on hitting any other object
        }
    }
}
