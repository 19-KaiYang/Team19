using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
public class Damageable : MonoBehaviour
{
    // For damage effects
    private Color originalColor = Color.white;
    public Color damageColor = Color.red;
    public float damageEffectDuration = 0.5f;
    private Renderer objectRenderer;
    private Coroutine damageCoroutine;

    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private GameObject DroneExplosionEffect;

    private float health;

    public bool DroneDisabled;

    private void Start()
    {
        if (gameObject.tag == "Damageable")
        {
            health = 200f;
        }
        else if (gameObject.tag == "Explosive")
        {
            health = 100f;
        }
        else if (gameObject.tag == "AerialTarget")
        {
            health = 500f;
        }
        DroneDisabled = false;

        objectRenderer = GetComponent<Renderer>();
    }
    public void TakeDamage(float amount)
    {
        health -= amount;
        // Trigger color change effect
        if (objectRenderer != null)
        {
            // Stop any existing color change effect to avoid stacking
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
            damageCoroutine = StartCoroutine(DamageEffect());
        }
        if (health < 0)
        {

            if (gameObject.tag == "Damageable")
            {
                Destroy();
            }
            else if(gameObject.tag == "Explosive")
            {
                if (ExplosionEffect != null)
                {
                    GameObject BarrelExplosion = Instantiate(ExplosionEffect, gameObject.transform.position, Quaternion.identity);
                    Destroy();
                }
            }
            else if(gameObject.tag == "AerialTarget")
            {
                if (DroneExplosionEffect != null)
                {
                    DroneDisabled = true;
                   
                }
            }
        }
        
    }

    private void Update()
    {
        if (DroneDisabled == true)
        {

            // Gradually reduce the drone's Y position
            Vector3 newPosition = transform.position;
            newPosition.y -= 2 * Time.deltaTime;
            transform.position = newPosition;
        }
    }

    public void Destroy()
    {
        Debug.Log(gameObject.name + " has died.");
        Destroy(gameObject);
    }
    private IEnumerator DamageEffect()
    {
        // Set to damage color instantly
        objectRenderer.material.color = damageColor;
        // Gradually transition back to the original color over time
        float elapsedTime = 0f;
        while (elapsedTime < damageEffectDuration)
        {
            objectRenderer.material.color = Color.Lerp(damageColor,
            originalColor, elapsedTime / damageEffectDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        // Ensure the final color is reset to the original
        objectRenderer.material.color = originalColor;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Wall")
        {
            if (gameObject.tag == "AerialTarget")
            {
                Instantiate(DroneExplosionEffect, gameObject.transform.position, Quaternion.identity);
                Destroy();
                Debug.Log("Drone collided with wall");
            }
        }
    }
}
