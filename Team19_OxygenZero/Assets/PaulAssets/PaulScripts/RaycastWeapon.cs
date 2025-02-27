using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class RaycastWeapon : Weapon
{
    
    public string weaponState;
    public int ammoCount;
    public int maxAmmoCount;
    // Rocket Ammo
    public GameObject Missile;
    public GameObject MissilePosition;
    public GameObject LauncherMuzzleEffect;
    public GameObject LauncherMuzzlePosition;



    public bool CanShoot;
    [SerializeField] private PlayerController fpsController;

    public GameObject Crosshair;

    public GameObject ammoText;
    public TMP_Text ammoTextComponent;
    public int magazineSize;

    public void InitializeWeapon()
    {
        Crosshair = GameObject.FindWithTag("Crosshair");        
        GameObject player = GameObject.FindWithTag("Player");
        fpsController = player.GetComponent<PlayerController>();
        GameObject inventoryObject = GameObject.FindWithTag("Inventory");
        inventory = inventoryObject.GetComponent<Inventory>();
        fpsController.currentWeapon = this.GetComponent<RaycastWeapon>();
        Image CrosshairImage = Crosshair.GetComponent<Image>();

        CrosshairImage.enabled = true;

        
        ammoText = GameObject.FindWithTag("ammoText");
        ammoTextComponent = ammoText.GetComponent<TMP_Text>();

        for (int i = 0; i < 3; i++)
        {

            if (inventory.itemSlots[i] != null)
            {
                if (inventory.itemSlots[i].activeSelf == true && inventory.itemEquipped[i] == true)
                {
                    if (inventory.itemSlots[i].tag == "Weapon")
                    {
                        weaponState = weaponData.weaponName;
                        //ammoCount = weaponData.Ammo;
                        //maxAmmoCount = weaponData.maxAmmo;
                        if (Crosshair.activeSelf == false)
                        {
                            Crosshair.SetActive(true);
                        }
                        break;
                    }
                }
                else
                {
                    if (Crosshair.activeSelf == true)
                    {
                        Crosshair.SetActive(false);
                    }
                    weaponState = "Not Equipped";
                }
            }

        }
    }

    public void Update()
    {
       
        if (Time.time >= nextFireTime)
        {
            CanShoot = true;
        }

        if (ammoTextComponent != null)
        {
            ammoTextComponent.text = "Ammo: " + ammoCount + " / " + maxAmmoCount;
        }
    }

    public override void Shoot()
    {
        
        
            if (Time.time >= nextFireTime)
            {
                
                // If weapon exists
                if (CanShoot == true)
                {

                    // If there is ammo in the weapon
                    if (ammoCount > 0)
                    {
                        //Debug.Log("Can shoot");
                        nextFireTime = Time.time + weaponData.fireRate;
                        ammoCount -= 1;

                        // if weapon is not rocket launcher, perform raycast
                        if (weaponData.name != null || weaponData.weaponName != "Not Equipped")
                        {
                            PerformRaycast();
                            CanShoot = false;
                        }
                        else if(weaponData.name == "Missile Launcher")
                        {
                            Vector3 MissileSpawnPoint = MissilePosition.transform.position;
                            // Instantiate the missile at the spawn point with the weapon's current rotation (facing direction)
                            GameObject missile = Instantiate(Missile, MissileSpawnPoint, MissilePosition.transform.rotation);
                            // Get the missile's Rigidbody to apply velocity
                            Rigidbody missileRb = missile.GetComponent<Rigidbody>();
                            if (missileRb != null)
                            {
                                // Get the direction the weapon is facing (forward direction of the weapon's transform)
                                Vector3 missileDirection = MissilePosition.transform.forward;

                                // Apply the direction to the missile's velocity
                                missileRb.velocity = missileDirection * 20;

                            }
                            GameObject MuzzleEffect = Instantiate(LauncherMuzzleEffect, LauncherMuzzlePosition.transform.position, Quaternion.identity);
                            ParentObject(MuzzleEffect, LauncherMuzzlePosition);

                            Destroy(MuzzleEffect, 2);
                            CanShoot = false;
                        }
                    }
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

