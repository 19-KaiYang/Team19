using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public int damage = 10;

    private GameObject Player;
    private PlayerController Controller;

    void Start()
    {
        Player = GameObject.FindWithTag("Player");
        Controller = Player.GetComponent<PlayerController>();
    }

    private Vector3 direction;

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
            Controller.DepletePlayerHealth(5);
        }
    }
}
