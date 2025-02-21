using Unity.VisualScripting;
using UnityEngine;
public class RaycastWeapon : Weapon
{
    public WeaponData[] Datas;
    
    public static string weaponName;
    public int[] ammoCount = new int[3];
    public int[] maxAmmoCount;
    // Rocket Ammo
    public GameObject Missile;
    public GameObject MissilePosition;
    public GameObject LauncherMuzzleEffect;
    public GameObject LauncherMuzzlePosition;

    public bool CanShoot;
    [SerializeField] private PlayerController fpsController;

    [SerializeField] private GameObject Crosshair;

    public void Update()
    {
        for (int i = 0; i < 3; i++)
        {

            if (inventory.itemSlots[i] != null)
            {
                if (inventory.itemSlots[i].activeSelf == true && inventory.itemEquipped[i] == true)
                {
                    if (inventory.itemSlots[i].tag == "Revolver")
                    {
                        int revolverData = 0;
                        weaponData = Datas[revolverData];
                        weaponName = "Revolver";
                        maxAmmoCount[revolverData] = weaponData.maxAmmo;
                        if (Crosshair.activeSelf == false)
                        {
                            Crosshair.SetActive(true);
                        }
                        //Debug.Log("Revolver chosen");
                        break;
                    }
                    else if (inventory.itemSlots[i].tag == "AK47")
                    {
                        int AK47Data = 1;
                        weaponData = Datas[AK47Data];
                        weaponName = "Ak47";
                        maxAmmoCount[AK47Data] = weaponData.maxAmmo;
                        if (Crosshair.activeSelf == false)
                        {
                            Crosshair.SetActive(true);
                        }
                        //Debug.Log("Ak47 chosen");
                        break;
                    }
                }              
            }
            else
            {
                if (Crosshair.activeSelf == true)
                {
                    Crosshair.SetActive(false);
                }
                weaponData = Datas[2];
                weaponName = "Not Equipped";
                break;
            }
           
        }

        if(Time.time >= nextFireTime)
        {
            CanShoot = true;
        }
    }




    public override void Shoot()
    {
        for (int i = 0; i < 3; i++)
        {
            if (Time.time >= nextFireTime)
            {
                
              
                if (weaponData == Datas[i] && ammoCount[i] > 0)
                {
                   
                    //Debug.Log("Can shoot");
                    nextFireTime = Time.time + weaponData.fireRate;
                    ammoCount[i] -= 1;

                   
                    // if weapon is not rocket launcher, perform raycast
                    if (weaponData != Datas[2])
                    {
                        PerformRaycast();
                        CanShoot = false;
                    }
                    else
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

