using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public List<GameObject> GameItemPrefabs;  // Assign prefabs in the Inspector
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();

    public static ItemManager Instance { get; private set; }
    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            /*DontDestroyOnLoad(gameObject);*/ // Optional: keeps it across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        foreach (var prefab in GameItemPrefabs)
        {
            ObjectData objectData = prefab.GetComponent<ObjectData>();
            RaycastWeapon raycastWeapon = prefab.GetComponent<RaycastWeapon>();
            if (objectData != null)
            {
                // Store the prefab with the weaponName as the key
                prefabDictionary[objectData.item.itemName.ToString()] = prefab; // Ensure weaponName is a string
            }

            if(raycastWeapon != null)
            {
                // Store the prefab with the weaponName as the key
                prefabDictionary[raycastWeapon.weaponData.weaponName.ToString()] = prefab; // Ensure weaponName is a string
            }
        }
    }

    public void SpawnByItemName(string name, Vector3 position, Transform parent = null)
    {
        if (prefabDictionary.TryGetValue(name, out GameObject prefab))
        {
            GameObject spawnedItem = Instantiate(prefab, position, Quaternion.identity);

            // Ensure the spawned object has the correct tag
            if (prefab.CompareTag("Item"))
            {
                spawnedItem.tag = "Item";
            }
            else if (prefab.CompareTag("Weapon"))
            {
                spawnedItem.tag = "Weapon";
            }

            if (parent != null)
            {
                spawnedItem.transform.SetParent(parent, false); // Set parent without changing local scale/position
            }
        }
        else
        {
            Debug.LogError("No prefab found with name: " + name);
        }
    }

}
