using UnityEngine;

public class LandmineScript : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // spawn explosion at the landmine position
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            Destroy(explosion, 3f); // destroy effect after 3 sec

            // Play landmine explosion sound
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.PlaySFX("LandmineExplosion");
               
            }
            else
            {
                Debug.LogWarning("AudioManager not found!");
            }

            PlayerController playerhealth = other.GetComponent<PlayerController>();
            playerhealth.DepletePlayerHealth(30);

            Destroy(gameObject); // destroy landmine because it exploded

            
        }
    }
}
