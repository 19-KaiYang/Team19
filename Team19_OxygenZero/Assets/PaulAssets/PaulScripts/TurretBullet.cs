using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    private Vector3 direction;

    // Oxygen System get player health
    private GameObject PlayerObject;
    private PlayerController playerhealth;

    private void Start()
    {
        PlayerObject = GameObject.FindWithTag("Player");
        playerhealth = PlayerObject.GetComponent<PlayerController>();
    }

    public void Initialize(Vector3 shootDirection)
    {
        direction = shootDirection.normalized; // Normalize direction to ensure consistent speed
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerhealth.DepletePlayerHealth(5);
        }     
    }


}
