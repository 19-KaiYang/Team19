using UnityEngine;
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private ImpactEffectSpawner bulletEffectSpawner;
    [SerializeField] private GameObject GunMuzzleEffect;
    [SerializeField] private GameObject MuzzlePosition;
    [SerializeField] private Camera playerCamera;
    [SerializeField] public WeaponData weaponData;
    public Inventory inventory;
    protected float nextFireTime = 0f;


    // Abstract method for shooting, to be implemented by subclasses
    public abstract void Shoot();
    // Protected method to handle raycast logic, can be used by subclasses
    protected void PerformRaycast()
    {
        // Find the muzzle for every weapon
        foreach (Transform child in inventory.itemHolderPosition)
        {
            Transform weapon = child.transform;
                
            foreach(Transform child2 in weapon)
            {
                if(child2.CompareTag("muzzleArea"))
                {
                    MuzzlePosition = child2.gameObject;
                }
            }

        }

        // Spawn Muzzle Effect at muzzle for every shot         
        GameObject MuzzleEffect = Instantiate(GunMuzzleEffect, MuzzlePosition.transform.position, Quaternion.identity);
        ParentObject(MuzzleEffect, MuzzlePosition);
        if (MuzzleEffect != null)
        {
            Destroy(MuzzleEffect, 0.2f);
        }


        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit,
        weaponData.range, weaponData.hitLayers))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Instantiate the impact effect
            bulletEffectSpawner.SpawnImpactEffect(hit.point, hit.normal);

            if (hitObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
        }
    }


    void ParentObject(GameObject obj, GameObject parent)
    {
        obj.transform.SetParent(parent.transform, false);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }

}

